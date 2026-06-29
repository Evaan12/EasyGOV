using System;
using Domain.Common;

namespace Domain.Events
{
    public class DevelopmentPlanPublishedEvent : IDomainEvent
    {
        public Guid PlanId { get; }
        public Guid TenantId { get; }
        public string EventType => nameof(DevelopmentPlanPublishedEvent);
        public DateTime OccurredOn { get; }

        public DevelopmentPlanPublishedEvent(Guid planId, Guid tenantId)
        {
            PlanId = planId;
            TenantId = tenantId;
            OccurredOn = DateTime.UtcNow;
        }
    }
}