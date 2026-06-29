using System;
using Domain.Common;

namespace Domain.Events
{
    public class GunasoEscalatedEvent : IDomainEvent
    {
        public Guid GunasoId { get; }
        public Guid NewTargetTenantId { get; }
        public string EventType => nameof(GunasoEscalatedEvent);
        public DateTime OccurredOn { get; }

        public GunasoEscalatedEvent(Guid gunasoId, Guid newTargetTenantId)
        {
            GunasoId = gunasoId;
            NewTargetTenantId = newTargetTenantId;
            OccurredOn = DateTime.UtcNow;
        }
    }
}