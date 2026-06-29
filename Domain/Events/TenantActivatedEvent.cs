using System;
using Domain.Common;

namespace Domain.Events
{
    public class TenantActivatedEvent : IDomainEvent
    {
        public Guid TenantId { get; }
        public string EventType => nameof(TenantActivatedEvent);
        public DateTime OccurredOn { get; }

        public TenantActivatedEvent(Guid tenantId)
        {
            TenantId = tenantId;
            OccurredOn = DateTime.UtcNow;
        }
    }
}