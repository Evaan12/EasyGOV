using Domain.Common;
using Domain.Enums;
using System;

namespace Domain.Entities
{
    public class CampaignApproval : Entity
    {
        public Guid AlertCampaignId { get; private set; }
        public Guid ReviewedBy { get; private set; }
        public ApprovalDecision Decision { get; private set; }
        public string? Remarks { get; private set; }

        private CampaignApproval() { }

        public CampaignApproval(Guid id, Guid alertCampaignId, Guid reviewedBy, ApprovalDecision decision, string? remarks, Guid createdBy)
            : base(id, createdBy) 
        {
            AlertCampaignId = alertCampaignId;
            ReviewedBy = reviewedBy;
            Decision = decision;
            Remarks = remarks;
        }
    }
}