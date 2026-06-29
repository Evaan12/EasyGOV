using Application.Interfaces;
using Domain.Exceptions;
using Domain.Repositories;
using Domain.ValueObjects;
using FaceRecognitionDotNet;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public class FaceRecognitionService : IFaceRecognitionService
    {
        private readonly Lazy<FaceRecognition> _faceRecognitionLazy;
        private readonly Lazy<InferenceSession> _arcFaceSessionLazy;
        private readonly IServiceScopeFactory _scopeFactory;

        public FaceRecognitionService(Lazy<FaceRecognition> faceRecognitionLazy, Lazy<InferenceSession> arcFaceSessionLazy, IServiceScopeFactory scopeFactory)
        {
            _faceRecognitionLazy = faceRecognitionLazy;
            _arcFaceSessionLazy = arcFaceSessionLazy;
            _scopeFactory = scopeFactory;
        }

        private FaceRecognition _faceRecognition => _faceRecognitionLazy.Value;
        private InferenceSession _arcFaceSession => _arcFaceSessionLazy.Value;

        public async Task<BiometricEmbedding> GenerateEmbeddingAsync(Stream imageStream, CancellationToken cancellationToken = default)
        {
            imageStream.Position = 0;
            using var ms = new MemoryStream();
            await imageStream.CopyToAsync(ms, cancellationToken);
            byte[] imageBytes = ms.ToArray();

            var (skBitmap, _) = DecodeAndScaleSafely(imageBytes);
            if (skBitmap == null) throw new DomainException("Could not decode image for face recognition.");

            using (skBitmap)
            {
                using var faceImage = LoadFaceImageFromSKBitmap(skBitmap);
                var locations = _faceRecognition.FaceLocations(faceImage, 1).ToList();

                if (!locations.Any()) throw new DomainException("No face detected in the live selfie.");

                var primaryFace = locations.OrderByDescending(f => (f.Right - f.Left) * (f.Bottom - f.Top)).First();
                var landmarks = _faceRecognition.FaceLandmark(faceImage, new[] { primaryFace }).FirstOrDefault();

                if (landmarks != null) EnsureLiveness(landmarks);
                else throw new DomainException("Could not extract critical facial landmarks.");

                using var croppedFace = CropFace(skBitmap, primaryFace);
                var tensor = PrepareArcFaceTensor(croppedFace);

                var inputName = _arcFaceSession.InputMetadata.Keys.First();
                var inputs = new List<NamedOnnxValue> { NamedOnnxValue.CreateFromTensor(inputName, tensor) };

                using var results = _arcFaceSession.Run(inputs);
                var embedding = results.First().AsEnumerable<float>().ToArray();

                return new BiometricEmbedding(embedding);
            }
        }

        public async Task<BiometricEmbedding> ExtractFaceFromDocumentAsync(Stream documentImageStream, CancellationToken cancellationToken = default)
        {
            documentImageStream.Position = 0;
            using var ms = new MemoryStream();
            await documentImageStream.CopyToAsync(ms, cancellationToken);
            byte[] imageBytes = ms.ToArray();

            var (skBitmap, _) = DecodeAndScaleSafely(imageBytes);
            if (skBitmap == null) throw new DomainException("Could not decode document image.");

            using (skBitmap)
            {
                using var faceImage = LoadFaceImageFromSKBitmap(skBitmap);
                var locations = _faceRecognition.FaceLocations(faceImage, 1).ToList();

                if (!locations.Any()) throw new DomainException("Could not detect a clear face on the document. Ensure you uploaded the front side containing the photograph.");

                var primaryFace = locations.OrderByDescending(f => (f.Right - f.Left) * (f.Bottom - f.Top)).First();
                using var croppedFace = CropFace(skBitmap, primaryFace);
                var tensor = PrepareArcFaceTensor(croppedFace);

                var inputName = _arcFaceSession.InputMetadata.Keys.First();
                var inputs = new List<NamedOnnxValue> { NamedOnnxValue.CreateFromTensor(inputName, tensor) };

                using var results = _arcFaceSession.Run(inputs);
                var embedding = results.First().AsEnumerable<float>().ToArray();

                return new BiometricEmbedding(embedding);
            }
        }

        public Task<double> CompareFacesAsync(BiometricEmbedding source, BiometricEmbedding target, CancellationToken cancellationToken = default)
        {
            return Task.Run(() => source.ComputeCosineSimilarity(target), cancellationToken);
        }

        public async Task<List<FaceMatchResultDto>> DetectAndMatchFacesAsync(Stream videoFrameStream, double matchThreshold = 0.20, CancellationToken cancellationToken = default)
        {
            videoFrameStream.Position = 0;
            using var ms = new MemoryStream();
            await videoFrameStream.CopyToAsync(ms, cancellationToken);
            byte[] frameBytes = ms.ToArray();

            var (skBitmap, scale) = DecodeAndScaleSafely(frameBytes);
            if (skBitmap == null) return new List<FaceMatchResultDto>();

            using (skBitmap)
            {
                using var faceImage = LoadFaceImageFromSKBitmap(skBitmap);
                
                var locations = _faceRecognition.FaceLocations(faceImage, 1).ToList();
                var results = new List<FaceMatchResultDto>();

                if (!locations.Any()) return results;

                using var scope = _scopeFactory.CreateScope();
                var missingPersonRepo = scope.ServiceProvider.GetRequiredService<IMissingPersonRepository>();

                var inputName = _arcFaceSession.InputMetadata.Keys.First();

                foreach (var loc in locations)
                {
                    using var croppedFace = CropFace(skBitmap, loc);
                    var tensor = PrepareArcFaceTensor(croppedFace);

                    var inputs = new List<NamedOnnxValue> { NamedOnnxValue.CreateFromTensor(inputName, tensor) };
                    using var inferenceResults = _arcFaceSession.Run(inputs);

                    var embeddingArray = inferenceResults.First().AsEnumerable<float>().ToArray();
                    var frameEmbedding = new BiometricEmbedding(embeddingArray);

                    var match = await missingPersonRepo.FindNearestMatchAsync(frameEmbedding, matchThreshold, cancellationToken);

                    // Re-calculate the box boundaries accounting for the downscale multiplier 
                    // so it perfectly aligns with the original dimensions the frontend sent.
                    var box = new BoundingBoxDto(
                        (int)(loc.Left / scale), 
                        (int)(loc.Top / scale), 
                        (int)((loc.Right - loc.Left) / scale), 
                        (int)((loc.Bottom - loc.Top) / scale)
                    );

                    if (match != null)
                    {
                        results.Add(new FaceMatchResultDto(box, true, match.Value.MissingPerson.Id, match.Value.MissingPerson.FullName, match.Value.SimilarityScore));
                    }
                    else
                    {
                        results.Add(new FaceMatchResultDto(box, false, null, null, 0.0));
                    }
                }

                return results;
            }
        }

        public async Task<bool> VerifyLivenessActionAsync(Stream imageStream, string expectedAction, CancellationToken cancellationToken = default)
        {
            try
            {
                imageStream.Position = 0;
                using var ms = new MemoryStream();
                await imageStream.CopyToAsync(ms, cancellationToken);
                byte[] imageBytes = ms.ToArray();

                var (skBitmap, _) = DecodeAndScaleSafely(imageBytes);
                if (skBitmap == null) return false;

                using (skBitmap)
                {
                    using var faceImage = LoadFaceImageFromSKBitmap(skBitmap);
                    var locations = _faceRecognition.FaceLocations(faceImage, 1).ToList();

                    if (!locations.Any()) return false;

                    var primaryFace = locations.OrderByDescending(f => (f.Right - f.Left) * (f.Bottom - f.Top)).First();
                    var landmarks = _faceRecognition.FaceLandmark(faceImage, new[] { primaryFace }).FirstOrDefault();

                    if (landmarks == null) return false;

                    var leftEye = landmarks.ContainsKey(FacePart.LeftEye) ? landmarks[FacePart.LeftEye].ToArray() : null;
                    var rightEye = landmarks.ContainsKey(FacePart.RightEye) ? landmarks[FacePart.RightEye].ToArray() : null;
                    var nose = landmarks.ContainsKey(FacePart.NoseTip) ? landmarks[FacePart.NoseTip].ToArray() : null;
                    var topLip = landmarks.ContainsKey(FacePart.TopLip) ? landmarks[FacePart.TopLip].ToArray() : null;
                    var bottomLip = landmarks.ContainsKey(FacePart.BottomLip) ? landmarks[FacePart.BottomLip].ToArray() : null;
                    var leftEyebrow = landmarks.ContainsKey(FacePart.LeftEyebrow) ? landmarks[FacePart.LeftEyebrow].ToArray() : null;
                    var rightEyebrow = landmarks.ContainsKey(FacePart.RightEyebrow) ? landmarks[FacePart.RightEyebrow].ToArray() : null;

                    if (leftEye == null || leftEye.Length != 6 || rightEye == null || rightEye.Length != 6 ||
                        nose == null || !nose.Any() || topLip == null || bottomLip == null ||
                        leftEyebrow == null || rightEyebrow == null)
                        return false;

                    double leftEyeCenterY = leftEye.Average(p => p.Point.Y);
                    double rightEyeCenterY = rightEye.Average(p => p.Point.Y);
                    double leftEyeCenterX = leftEye.Average(p => p.Point.X);
                    double rightEyeCenterX = rightEye.Average(p => p.Point.X);

                    double averageEyeY = (leftEyeCenterY + rightEyeCenterY) / 2.0;
                    double eyeDist = Math.Abs(rightEyeCenterX - leftEyeCenterX) + 0.0001;

                    double noseX = nose.Average(p => p.Point.X);
                    double noseY = nose.Average(p => p.Point.Y);

                    double leftEAR = CalculateEAR(leftEye);
                    double rightEAR = CalculateEAR(rightEye);

                    double leftDist = Math.Abs(noseX - leftEyeCenterX);
                    double rightDist = Math.Abs(rightEyeCenterX - noseX);
                    double symmetricRatio = leftDist / (rightDist + 0.0001);

                    double topLipY = topLip.Average(p => p.Point.Y);
                    double bottomLipY = bottomLip.Average(p => p.Point.Y);
                    double mouthOpenDist = Math.Abs(bottomLipY - topLipY);
                    double mouthRatio = mouthOpenDist / eyeDist;

                    double leftBrowY = leftEyebrow.Average(p => p.Point.Y);
                    double rightBrowY = rightEyebrow.Average(p => p.Point.Y);
                    double leftBrowDist = leftEyeCenterY - leftBrowY;
                    double rightBrowDist = rightEyeCenterY - rightBrowY;
                    double browRatio = (leftBrowDist + rightBrowDist) / (2.0 * eyeDist);

                    double noseDistToEyes = Math.Abs(noseY - averageEyeY);
                    double noseDistToMouth = Math.Abs(topLipY - noseY);
                    double verticalRatio = noseDistToEyes / (noseDistToMouth + 0.0001);

                    return expectedAction.ToLowerInvariant() switch
                    {
                        "blink" => leftEAR < 0.26 && rightEAR < 0.26,
                        "turn_head" => symmetricRatio < 0.65 || symmetricRatio > 1.55,
                        "open_mouth" => mouthRatio > 0.35,
                        "raise_eyebrows" => browRatio > 0.38,
                        "look_up" => verticalRatio < 1.28,
                        "look_down" => verticalRatio > 2.50,
                        "look_straight" => leftEAR > 0.20 && rightEAR > 0.20 &&
                                           symmetricRatio >= 0.88 && symmetricRatio <= 1.12 &&
                                           verticalRatio >= 1.45 && verticalRatio <= 2.25,
                        _ => false
                    };
                }
            }
            catch
            {
                return false;
            }
        }

        private (SKBitmap? Bitmap, float Scale) DecodeAndScaleSafely(byte[] imageBytes, int maxWidth = 1024)
        {
            var bitmap = SKBitmap.Decode(imageBytes);
            if (bitmap == null) return (null, 1.0f);

            if (bitmap.Width > maxWidth)
            {
                float scale = (float)maxWidth / bitmap.Width;
                int newHeight = (int)(bitmap.Height * scale);
                var scaled = bitmap.Resize(new SKImageInfo(maxWidth, newHeight), SKFilterQuality.Medium);
                bitmap.Dispose();
                return (scaled, scale);
            }
            return (bitmap, 1.0f);
        }

        private FaceRecognitionDotNet.Image LoadFaceImageFromSKBitmap(SKBitmap skBitmap)
        {
            int width = skBitmap.Width;
            int height = skBitmap.Height;
            byte[] rgbData = new byte[width * height * 3];
            int index = 0;

            var pixels = skBitmap.Pixels;
            foreach (var pixel in pixels)
            {
                rgbData[index++] = pixel.Red;
                rgbData[index++] = pixel.Green;
                rgbData[index++] = pixel.Blue;
            }

            return FaceRecognitionDotNet.FaceRecognition.LoadImage(rgbData, height, width, width * 3, Mode.Rgb);
        }

        private void EnsureLiveness(IDictionary<FacePart, IEnumerable<FacePoint>> landmarks)
        {
            var leftEye = landmarks[FacePart.LeftEye].ToArray();
            var rightEye = landmarks[FacePart.RightEye].ToArray();
            var nose = landmarks[FacePart.NoseTip].ToArray();

            if (leftEye.Length != 6 || rightEye.Length != 6 || !nose.Any())
                throw new DomainException("Unable to map critical features needed for liveness verification.");

            double leftEAR = CalculateEAR(leftEye);
            double rightEAR = CalculateEAR(rightEye);
            if (leftEAR < 0.15 || rightEAR < 0.15)
                throw new DomainException("Liveness verification failed: Eyes appear to be closed.");

            double leftEyeCenter = leftEye.Average(p => p.Point.X);
            double rightEyeCenter = rightEye.Average(p => p.Point.X);
            double noseX = nose.Average(p => p.Point.X);

            double leftDist = Math.Abs(noseX - leftEyeCenter);
            double rightDist = Math.Abs(rightEyeCenter - noseX);

            double symmetricRatio = leftDist / (rightDist + 0.0001);

            if (symmetricRatio < 0.4 || symmetricRatio > 2.5)
                throw new DomainException("Liveness verification failed: Face is turned too far to the side.");
        }

        private double CalculateEAR(FacePoint[] eye)
        {
            double v1 = Distance(eye[1], eye[5]);
            double v2 = Distance(eye[2], eye[4]);
            double h = Distance(eye[0], eye[3]);
            return (v1 + v2) / (2.0 * h + 0.0001);
        }

        private double Distance(FacePoint p1, FacePoint p2) => Math.Sqrt(Math.Pow(p1.Point.X - p2.Point.X, 2) + Math.Pow(p1.Point.Y - p2.Point.Y, 2));

        private SKBitmap CropFace(SKBitmap source, Location location)
        {
            int width = location.Right - location.Left;
            int height = location.Bottom - location.Top;

            int marginX = (int)(width * 0.15);
            int marginY = (int)(height * 0.15);

            int left = Math.Max(0, location.Left - marginX);
            int top = Math.Max(0, location.Top - marginY);
            int right = Math.Min(source.Width, location.Right + marginX);
            int bottom = Math.Min(source.Height, location.Bottom + marginY);

            var rect = new SKRectI(left, top, right, bottom);
            var cropped = new SKBitmap(rect.Width, rect.Height);
            source.ExtractSubset(cropped, rect);

            return cropped;
        }

        private DenseTensor<float> PrepareArcFaceTensor(SKBitmap faceBitmap)
        {
            using var resized = faceBitmap.Resize(new SKImageInfo(112, 112), SKFilterQuality.High);
            var tensor = new DenseTensor<float>(new[] { 1, 3, 112, 112 });

            for (int y = 0; y < 112; y++)
            {
                for (int x = 0; x < 112; x++)
                {
                    var pixel = resized.GetPixel(x, y);
                    tensor[0, 0, y, x] = (pixel.Red - 127.5f) / 127.5f;
                    tensor[0, 1, y, x] = (pixel.Green - 127.5f) / 127.5f;
                    tensor[0, 2, y, x] = (pixel.Blue - 127.5f) / 127.5f;
                }
            }

            return tensor;
        }
    }
}

