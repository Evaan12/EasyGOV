using Domain.Common;
using System;

namespace Domain.Events
{
    public class CampaignApprovedEvent : IDomainEvent
    {
        public Guid CampaignId { get; }
        public Guid TargetTenantId { get; }
        public string EventType => nameof(CampaignApprovedEvent);
        public DateTime OccurredOn { get; }

        public CampaignApprovedEvent(Guid campaignId, Guid targetTenantId)
        {
            CampaignId = campaignId;
            TargetTenantId = targetTenantId;
            OccurredOn = DateTime.UtcNow;
        }
    }
}