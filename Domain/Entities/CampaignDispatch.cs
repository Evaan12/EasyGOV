using Domain.Common;
using Domain.Enums;
using System;

namespace Domain.Entities
{
    public class CampaignDispatch : Entity, IAggregateRoot
    {
        public Guid AlertCampaignId { get; private set; }
        public Guid CitizenId { get; private set; }

        public string? ExternalDispatchId { get; private set; }
        public DispatchStatus DispatchStatus { get; private set; }
        public int? DurationInSeconds { get; private set; }

        private CampaignDispatch() { }

        public CampaignDispatch(Guid id, Guid alertCampaignId, Guid citizenId, Guid createdBy)
            : base(id, createdBy) 
        {
            AlertCampaignId = alertCampaignId;
            CitizenId = citizenId;
            DispatchStatus = DispatchStatus.Pending;
        }

        public void AssignExternalId(string externalDispatchId, Guid updatedBy)
        {
            ExternalDispatchId = externalDispatchId;
            UpdatedBy = updatedBy;
            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdateStatusFromPoll(DispatchStatus callStatus, int? durationInSeconds, Guid updatedBy)
        {
            DispatchStatus = callStatus;
            DurationInSeconds = durationInSeconds;
            UpdatedBy = updatedBy;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}