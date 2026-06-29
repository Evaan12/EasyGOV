using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Domain.ValueObjects;

namespace Application.Interfaces
{
    public record BoundingBoxDto(int X, int Y, int Width, int Height);
    
    public record FaceMatchResultDto(BoundingBoxDto Box, bool IsMatch, Guid? MissingPersonId, string? MissingPersonName, double Confidence);

    public interface IFaceRecognitionService
    {
        Task<BiometricEmbedding> GenerateEmbeddingAsync(Stream imageStream, CancellationToken cancellationToken = default);
        
        Task<BiometricEmbedding> ExtractFaceFromDocumentAsync(Stream documentImageStream, CancellationToken cancellationToken = default);
        
        Task<double> CompareFacesAsync(BiometricEmbedding source, BiometricEmbedding target, CancellationToken cancellationToken = default);
        
        Task<List<FaceMatchResultDto>> DetectAndMatchFacesAsync(Stream videoFrameStream, double matchThreshold = 0.20, CancellationToken cancellationToken = default);

        Task<bool> VerifyLivenessActionAsync(Stream imageStream, string expectedAction, CancellationToken cancellationToken = default);
    }
}