using Domain.Common;
using Domain.Constants;
using Domain.Enums;
using Domain.Exceptions;
using System;

namespace Domain.Entities
{
    public class SifarisApplication : Entity, IAggregateRoot
    {
        public Guid CitizenId { get; private set; }
        public Guid TargetWardId { get; private set; }
        public Guid ApplicationTemplateId { get; private set; }
        public Guid TargetSifarisTemplateId { get; private set; }
        
        public string SubmittedDataJson { get; private set; } 
        
        public ApplicationStatus Status { get; private set; }
        public string? ReviewNotes { get; private set; }
        
        public Guid? ReviewedBy { get; private set; }
        public DateTime? ReviewedAt { get; private set; }

        public string? ApproverName { get; private set; }
        public string? ApproverRole { get; private set; }

        private SifarisApplication() { }

        public SifarisApplication(Guid id, Guid citizenId, Guid targetWardId, Guid applicationTemplateId, Guid targetSifarisTemplateId, string submittedDataJson, Guid createdBy)
            : base(id, createdBy)
        {
            if (submittedDataJson.Length > AppConstants.MaxJsonPayloadSizeBytes) 
                throw new DomainException($"Snapshot JSON structure exceeds the maximum allowed payload limit of {AppConstants.MaxJsonPayloadSizeBytes / 1024}KB to prevent database bloat.");

            CitizenId = citizenId;
            TargetWardId = targetWardId;
            ApplicationTemplateId = applicationTemplateId;
            TargetSifarisTemplateId = targetSifarisTemplateId;
            SubmittedDataJson = submittedDataJson;
            Status = ApplicationStatus.PendingReview;
        }

        public void UpdateSubmittedData(string updatedDataJson, Guid updatedBy)
        {
            if (Status == ApplicationStatus.Approved || Status == ApplicationStatus.Rejected)
                throw new DomainException("Cannot update an application that has already been permanently finalized.");

            if (updatedDataJson.Length > AppConstants.MaxJsonPayloadSizeBytes) 
                throw new DomainException($"Snapshot data size exceeds the maximum allowed payload limit of {AppConstants.MaxJsonPayloadSizeBytes / 1024}KB.");

            SubmittedDataJson = updatedDataJson;
            Status = ApplicationStatus.PendingReview; 
            UpdatedBy = updatedBy;
        }

        public void Approve(Guid approvedBy, string approverName, string approverRole, string reviewNotes, DateTime utcNow)
        {
            if (Status != ApplicationStatus.PendingReview) throw new DomainException("Only pending applications can be approved.");
            
            Status = ApplicationStatus.Approved;
            ReviewNotes = reviewNotes;
            ApproverName = approverName;
            ApproverRole = approverRole;
            ReviewedBy = approvedBy;
            ReviewedAt = utcNow;
            UpdatedBy = approvedBy;
        }

        public void Reject(string rejectionReason, Guid rejectedBy, DateTime utcNow)
        {
            if (Status != ApplicationStatus.PendingReview) throw new DomainException("Only pending applications can be rejected.");
            if (string.IsNullOrWhiteSpace(rejectionReason)) throw new DomainException("Rejection reason is required.");
            
            Status = ApplicationStatus.Rejected;
            ReviewNotes = rejectionReason;
            ReviewedBy = rejectedBy;
            ReviewedAt = utcNow;
            UpdatedBy = rejectedBy;
        }

        public void RequestModification(string reviewNotes, Guid reviewedBy, DateTime utcNow)
        {
            if (Status != ApplicationStatus.PendingReview) throw new DomainException("Only pending applications can be marked for modification.");
            if (string.IsNullOrWhiteSpace(reviewNotes)) throw new DomainException("Modification notes are required.");
            
            Status = ApplicationStatus.RequiresModification;
            ReviewNotes = reviewNotes;
            ReviewedBy = reviewedBy;
            ReviewedAt = utcNow;
            UpdatedBy = reviewedBy;
        }
    }
}