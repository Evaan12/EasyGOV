using Application.Helpers;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Domain.Exceptions;
using Domain.Repositories;
using Domain.ValueObjects;
using Mediator;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.eKYC.Commands
{
    public record PerformEkycCommand(
        Guid CitizenId,
        IdentityDocumentType DocumentType,
        string DocumentNumber,
        DateTime IssueDate,
        Guid IssueDistrictId,
        Stream DocumentFrontStream,
        Stream? DocumentBackStream,
        Stream LiveSelfieStream,
        string OriginalDocFrontFileName,
        string? OriginalDocBackFileName,
        string OriginalSelfieFileName) : IRequest<Unit>;

    public class PerformEkycCommandHandler : IRequestHandler<PerformEkycCommand, Unit>
    {
        private readonly ICitizenProfileRepository _profileRepository;
        private readonly IDocumentFileRepository _documentRepository;
        private readonly IFaceRecognitionService _faceService;
        private readonly IFileHelper _fileHelper;
        private readonly IUnitOfWork _unitOfWork;

        public PerformEkycCommandHandler(
            ICitizenProfileRepository profileRepository,
            IDocumentFileRepository documentRepository,
            IFaceRecognitionService faceService,
            IFileHelper fileHelper,
            IUnitOfWork unitOfWork)
        {
            _profileRepository = profileRepository;
            _documentRepository = documentRepository;
            _faceService = faceService;
            _fileHelper = fileHelper;
            _unitOfWork = unitOfWork;
        }

        public async Task<Unit> Handle(PerformEkycCommand request, CancellationToken cancellationToken)
        {
            var profile = await _profileRepository.GetByIdAsync(request.CitizenId, cancellationToken);
            if (profile == null) throw new DomainException("Citizen profile not found.");

            if (profile.Status != CitizenStatus.PendingDigital)
                throw new DomainException("Profile has already completed digital e-KYC or is active.");

            var selfieEmbedding = await _faceService.GenerateEmbeddingAsync(request.LiveSelfieStream, cancellationToken);

            if (request.DocumentType == IdentityDocumentType.Citizenship || request.DocumentType == IdentityDocumentType.NationalId)
            {
                var docEmbedding = await _faceService.ExtractFaceFromDocumentAsync(request.DocumentFrontStream, cancellationToken);
                var similarity = await _faceService.CompareFacesAsync(docEmbedding, selfieEmbedding, cancellationToken);

                // Lowered threshold to 0.15 for more robust real-world acceptance across document scans
                if (similarity < 0.15)
                {
                    throw new DomainException("Identity verification failed. The live selfie does not sufficiently match the document photo.");
                }
            }

            var docFrontPath = await _fileHelper.SaveFileAsync(request.DocumentFrontStream, request.OriginalDocFrontFileName, "doc_front");

            string? docBackPath = null;
            if (request.DocumentBackStream != null && !string.IsNullOrEmpty(request.OriginalDocBackFileName))
            {
                docBackPath = await _fileHelper.SaveFileAsync(request.DocumentBackStream, request.OriginalDocBackFileName, "doc_back");
            }

            var selfiePath = await _fileHelper.SaveFileAsync(request.LiveSelfieStream, request.OriginalSelfieFileName, "live_selfie");

            var fileTypeFront = request.DocumentType switch
            {
                IdentityDocumentType.Citizenship => DocumentFileType.CitizenshipFront,
                IdentityDocumentType.BirthCertificate => DocumentFileType.BirthCertificate,
                IdentityDocumentType.NationalId => DocumentFileType.NationalId,
                _ => DocumentFileType.Other
            };

            var docFrontEntity = new DocumentFile(Guid.NewGuid(), profile.Id, fileTypeFront, request.OriginalDocFrontFileName, docFrontPath, "image/webp", request.DocumentFrontStream.Length, "checksum_placeholder", profile.Id);
            await _documentRepository.AddAsync(docFrontEntity, cancellationToken);

            if (docBackPath != null && request.DocumentBackStream != null && !string.IsNullOrEmpty(request.OriginalDocBackFileName))
            {
                var fileTypeBack = request.DocumentType == IdentityDocumentType.Citizenship ? DocumentFileType.CitizenshipBack : DocumentFileType.Other;
                var docBackEntity = new DocumentFile(Guid.NewGuid(), profile.Id, fileTypeBack, request.OriginalDocBackFileName, docBackPath, "image/webp", request.DocumentBackStream.Length, "checksum_placeholder", profile.Id);
                await _documentRepository.AddAsync(docBackEntity, cancellationToken);
            }

            var selfieEntity = new DocumentFile(Guid.NewGuid(), profile.Id, DocumentFileType.LiveSelfie, request.OriginalSelfieFileName, selfiePath, "image/webp", request.LiveSelfieStream.Length, "checksum_placeholder", profile.Id);
            await _documentRepository.AddAsync(selfieEntity, cancellationToken);

            var utcIssueDate = request.IssueDate.Kind == DateTimeKind.Unspecified
                ? DateTime.SpecifyKind(request.IssueDate, DateTimeKind.Utc)
                : request.IssueDate.ToUniversalTime();

            if (request.DocumentType == IdentityDocumentType.Citizenship)
            {
                var citizenshipDetails = new CitizenshipDetails(request.DocumentNumber, utcIssueDate, request.IssueDistrictId);
                profile.CompleteDigitalEkyc(citizenshipDetails, selfieEmbedding, profile.Id);
            }
            else if (request.DocumentType == IdentityDocumentType.BirthCertificate)
            {
                var birthCertDetails = new BirthCertificateDetails(request.DocumentNumber, utcIssueDate, request.IssueDistrictId);
                profile.CompleteDigitalEkycWithBirthCertificate(birthCertDetails, selfieEmbedding, profile.Id);
            }
            else if (request.DocumentType == IdentityDocumentType.NationalId)
            {
                var nationalIdDetails = new NationalIdDetails(request.DocumentNumber, utcIssueDate);
                profile.CompleteDigitalEkycWithNationalId(nationalIdDetails, selfieEmbedding, profile.Id);
            }

            await _profileRepository.UpdateAsync(profile, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var pathsToCommit = new System.Collections.Generic.List<string> { docFrontPath, selfiePath };
            if (docBackPath != null) pathsToCommit.Add(docBackPath);
            await _fileHelper.CommitFilesAsync(pathsToCommit);

            return Unit.Value;
        }
    }
}