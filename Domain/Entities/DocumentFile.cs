using Domain.Common;
using Domain.Constants;
using Domain.Enums;
using Domain.Events;
using Domain.Exceptions;
using Domain.ValueObjects;
using System;

namespace Domain.Entities
{
    public class DocumentFile : Entity, IAggregateRoot
    {
        public Guid OwnerId { get; private set; }
        public DocumentFileType FileType { get; private set; }
        public string OriginalFileName { get; private set; }
        public string FilePath { get; private set; } 
        public string MimeType { get; private set; }
        public long FileSizeBytes { get; private set; }
        public string ChecksumSha256 { get; private set; }
        
        public DocumentAnalysisResult? AnalysisResult { get; private set; }

        private DocumentFile() { }

        public DocumentFile(Guid id, Guid ownerId, DocumentFileType fileType, string originalFileName, string filePath, string mimeType, long fileSizeBytes, string checksumSha256, Guid createdBy)
            : base(id, createdBy)
        {
            if (fileSizeBytes > AppConstants.MaxFileUploadSizeBytes) throw new DomainException($"File size exceeds the {AppConstants.MaxFileUploadSizeBytes / (1024 * 1024)}MB maximum limit constraint.");
            if (fileSizeBytes == 0) throw new DomainException("Cannot upload an empty file.");
            if (string.IsNullOrWhiteSpace(originalFileName)) throw new DomainException("Original filename is required.");
            if (string.IsNullOrWhiteSpace(filePath)) throw new DomainException("File path is required.");
            if (string.IsNullOrWhiteSpace(checksumSha256)) throw new DomainException("File checksum is strictly required for integrity checks.");

            OwnerId = ownerId;
            FileType = fileType;
            OriginalFileName = originalFileName;
            FilePath = filePath;
            MimeType = mimeType;
            FileSizeBytes = fileSizeBytes;
            ChecksumSha256 = checksumSha256;
        }

        public void RegisterForensicAnalysis(DocumentAnalysisResult analysis, Guid analyzedBy)
        {
            AnalysisResult = analysis;
            UpdatedBy = analyzedBy;

            if (!analysis.IsVerifiedAuthentic)
            {
                AddDomainEvent(new DocumentForgeryDetectedEvent(Id, OwnerId, analysis.ElaForgeryScore));
            }
        }

        public void UpdateFilePath(string newPath, string newChecksum, Guid updatedBy)
        {
            if (string.IsNullOrWhiteSpace(newPath)) throw new DomainException("File path is required.");
            if (string.IsNullOrWhiteSpace(newChecksum)) throw new DomainException("File checksum is strictly required for integrity checks.");

            FilePath = newPath;
            ChecksumSha256 = newChecksum;
            UpdatedBy = updatedBy;
        }
    }
}