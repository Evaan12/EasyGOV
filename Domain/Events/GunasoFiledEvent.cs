using System;
using Domain.Common;

namespace Domain.Events
{
    public class GunasoFiledEvent : IDomainEvent
    {
        public Guid GunasoId { get; }
        public Guid TargetTenantId { get; }
        public Guid CitizenId { get; }
        public string EventType => nameof(GunasoFiledEvent);
        public DateTime OccurredOn { get; }

        public GunasoFiledEvent(Guid gunasoId, Guid targetTenantId, Guid citizenId)
        {
            GunasoId = gunasoId;
            TargetTenantId = targetTenantId;
            CitizenId = citizenId;
            OccurredOn = DateTime.UtcNow;
        }
    }
}