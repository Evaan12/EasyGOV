using Domain.Common;
using Domain.Enums;
using Domain.Events;
using Domain.Exceptions;
using System;
using System.Collections.Generic;

namespace Domain.Entities
{
    public class AlertCampaign : Entity, IAggregateRoot
    {
        public string Title { get; private set; } = string.Empty;
        public CampaignCategory Category { get; private set; }
        public string MessageScript { get; private set; } = string.Empty;
        
        public Guid TargetTenantId { get; private set; }
        
        public CampaignStatus Status { get; private set; }
        public string? TelephonyProvider { get; private set; }
        public string? ExternalProviderCampaignId { get; private set; }
        public decimal ProgressPercent { get; private set; }

        private readonly List<CampaignApproval> _approvals = new();
        public IReadOnlyCollection<CampaignApproval> Approvals => _approvals.AsReadOnly();

        private AlertCampaign() { }

        public AlertCampaign(Guid id, string title, CampaignCategory category, string messageScript, Guid targetTenantId, Guid createdBy)
            : base(id, createdBy)
        {
            if (string.IsNullOrWhiteSpace(title)) throw new DomainException("Campaign title cannot be empty.");
            if (string.IsNullOrWhiteSpace(messageScript)) throw new DomainException("Campaign script cannot be empty.");

            Title = title;
            Category = category;
            MessageScript = messageScript;
            TargetTenantId = targetTenantId;
            Status = CampaignStatus.Draft;
        }

        public void RequestApproval(Guid updatedBy)
        {
            if (Status != CampaignStatus.Draft && Status != CampaignStatus.Rejected)
                throw new DomainException("Only Draft or Rejected campaigns can be submitted for approval.");

            Status = CampaignStatus.PendingApproval;
            UpdatedBy = updatedBy;
            UpdatedAt = DateTime.UtcNow;
        }

        public void ProcessApproval(Guid reviewerId, ApprovalDecision decision, string? remarks)
        {
            if (Status != CampaignStatus.PendingApproval)
                throw new DomainException("Campaign is not pending approval.");

            if (reviewerId == CreatedBy)
                throw new DomainException("Creator cannot approve their own campaign. Dual-authorization required.");

            if (decision == ApprovalDecision.Rejected && string.IsNullOrWhiteSpace(remarks))
                throw new DomainException("Remarks are required when rejecting a campaign.");

            _approvals.Add(new CampaignApproval(Guid.NewGuid(), Id, reviewerId, decision, remarks, reviewerId));

            if (decision == ApprovalDecision.Approved)
            {
                Status = CampaignStatus.Approved;
                AddDomainEvent(new CampaignApprovedEvent(Id, TargetTenantId));
            }
            else
            {
                Status = CampaignStatus.Rejected;
            }

            UpdatedBy = reviewerId;
            UpdatedAt = DateTime.UtcNow;
        }

        public void MarkAsRunning(string providerName, string externalCampaignId, Guid updatedBy)
        {
            if (Status != CampaignStatus.Approved)
                throw new DomainException("Only approved campaigns can be transitioned to running.");

            TelephonyProvider = providerName;
            ExternalProviderCampaignId = externalCampaignId;
            Status = CampaignStatus.Running;
            ProgressPercent = 0;

            UpdatedBy = updatedBy;
            UpdatedAt = DateTime.UtcNow;
        }

        public void MarkAsCompleted(Guid updatedBy)
        {
            Status = CampaignStatus.Completed;
            ProgressPercent = 100;
            UpdatedBy = updatedBy;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}