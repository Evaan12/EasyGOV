using System;
using Domain.Common;

namespace Domain.Events
{
    public class TenantDeactivatedEvent : IDomainEvent
    {
        public Guid TenantId { get; }
        public string EventType => nameof(TenantDeactivatedEvent);
        public DateTime OccurredOn { get; }

        public TenantDeactivatedEvent(Guid tenantId)
        {
            TenantId = tenantId;
            OccurredOn = DateTime.UtcNow;
        }
    }
}