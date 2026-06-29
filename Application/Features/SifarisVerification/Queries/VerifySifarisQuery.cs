using Domain.Entities;
using Domain.Exceptions;
using Domain.Repositories;
using Mediator;
using System;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.SifarisVerification.Queries
{
    public record SifarisVerificationResult(
        Guid SifarisId, 
        string CompiledHtml, 
        bool IsRevoked, 
        string? RevocationReason, 
        bool ProfileChanged,
        bool IsChallengeSuccessful,
        string? CurrentFullName,
        string? ApproverName,
        string? ApproverRole);

    public record VerifySifarisQuery(Guid SifarisId, string? ChallengeFullName) : IRequest<SifarisVerificationResult>;

    public class VerifySifarisQueryHandler : IRequestHandler<VerifySifarisQuery, SifarisVerificationResult>
    {
        private readonly ISifarisRepository _sifarisRepository;
        private readonly ICitizenProfileRepository _profileRepository;
        private readonly IDocumentTemplateRepository _templateRepository;

        public VerifySifarisQueryHandler(
            ISifarisRepository sifarisRepository, 
            ICitizenProfileRepository profileRepository, 
            IDocumentTemplateRepository templateRepository)
        {
            _sifarisRepository = sifarisRepository;
            _profileRepository = profileRepository;
            _templateRepository = templateRepository;
        }

        public async Task<SifarisVerificationResult> Handle(VerifySifarisQuery request, CancellationToken cancellationToken)
        {
            var sifaris = await _sifarisRepository.GetByIdAsync(request.SifarisId, cancellationToken);
            if (sifaris == null) throw new DomainException("Document not found or invalid QR code.");

            var profile = await _profileRepository.GetByIdAsync(sifaris.CitizenId, cancellationToken);
            if (profile == null) throw new DomainException("Associated citizen profile no longer exists.");

            string currentHash;
            using (var sha256 = SHA256.Create())
            {
                var hashBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(profile.FullName.ToLowerInvariant()));
                currentHash = Convert.ToBase64String(hashBytes);
            }

            bool isRevoked = sifaris.Status == Domain.Enums.SifarisStatus.Revoked;
            bool profileChanged = currentHash != sifaris.ProfileHashAtIssuance;
            bool isChallengeSuccessful = false;
            string? currentFullName = null;

            if (profileChanged && !string.IsNullOrEmpty(request.ChallengeFullName))
            {
                if (profile.FullName.Equals(request.ChallengeFullName.Trim(), StringComparison.OrdinalIgnoreCase))
                {
                    isChallengeSuccessful = true;
                    currentFullName = profile.FullName;
                }
                else
                {
                    throw new DomainException("Challenge failed: The provided name does not match the updated master record.");
                }
            }

            var template = await _templateRepository.GetByIdAsync(sifaris.SifarisTemplateId, cancellationToken);
            string compiledHtml = template?.HtmlContent ?? "";

            try
            {
                var json = JsonDocument.Parse(sifaris.SnapshotDataJson);
                foreach (var prop in json.RootElement.EnumerateObject())
                {
                    compiledHtml = compiledHtml.Replace("{{" + prop.Name + "}}", prop.Value.GetString() ?? "");
                }
            }
            catch { }

            return new SifarisVerificationResult(
                sifaris.Id,
                compiledHtml,
                isRevoked,
                sifaris.RevocationReason,
                profileChanged,
                isChallengeSuccessful,
                currentFullName,
                sifaris.ApproverName,
                sifaris.ApproverRole
            );
        }
    }
}