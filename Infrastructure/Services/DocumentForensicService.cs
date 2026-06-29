using Application.Interfaces;
using Domain.ValueObjects;
using SkiaSharp;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public class DocumentForensicService : IDocumentForensicService
    {
        public async Task<DocumentAnalysisResult> AnalyzeDocumentAsync(Stream imageStream, CancellationToken cancellationToken = default)
        {
            imageStream.Position = 0;
            using var ms = new MemoryStream();
            await imageStream.CopyToAsync(ms, cancellationToken);
            var originalBytes = ms.ToArray();
            
            // Scan header for well-known editing software signatures (Tampering check based on signatures)
            string header = Encoding.ASCII.GetString(originalBytes.Take(1024).ToArray());
            string? softwareSignature = null;
            bool isMetadataTampered = false;

            if (header.Contains("Photoshop") || header.Contains("GIMP") || header.Contains("Corel"))
            {
                softwareSignature = "Image Editing Software Signature Detected";
                isMetadataTampered = true;
            }

            using var originalBitmap = SKBitmap.Decode(originalBytes);
            if (originalBitmap == null)
            {
                return new DocumentAnalysisResult(1.0, true, "Format mismatch or Corrupted data structure.");
            }

            // Error Level Analysis (ELA) for detecting sudden changes in heatmap / compression levels
            using var originalImage = SKImage.FromBitmap(originalBitmap);
            using var compressedData = originalImage.Encode(SKEncodedImageFormat.Jpeg, 90);
            using var compressedBitmap = SKBitmap.Decode(compressedData.ToArray());

            long diffSum = 0;
            int width = originalBitmap.Width;
            int height = originalBitmap.Height;

            int step = Math.Max(1, width / 150); 

            for (int y = 0; y < height; y += step)
            {
                for (int x = 0; x < width; x += step)
                {
                    var p1 = originalBitmap.GetPixel(x, y);
                    var p2 = compressedBitmap.GetPixel(x, y);

                    diffSum += Math.Abs(p1.Red - p2.Red) +
                               Math.Abs(p1.Green - p2.Green) +
                               Math.Abs(p1.Blue - p2.Blue);
                }
            }

            int totalPixelsSampled = (width / step) * (height / step);
            double averageDiff = (double)diffSum / totalPixelsSampled;
            
            // Normalize ELA score for probabilistic heatmap detection
            double elaScore = Math.Min(1.0, averageDiff / 50.0);

            // If ELA shows significant artifacts or software signatures were found
            bool isTampered = elaScore > 0.20 || isMetadataTampered; 

            return new DocumentAnalysisResult(
                elaScore, 
                isMetadataTampered, 
                isTampered ? (softwareSignature ?? "Suspicious ELA footprint detected") : "Clean Profile");
        }
    }
}