using System;
using Domain.Common;

namespace Domain.Events
{
    public class GunasoResolvedEvent : IDomainEvent
    {
        public Guid GunasoId { get; }
        public Guid CitizenId { get; }
        public string EventType => nameof(GunasoResolvedEvent);
        public DateTime OccurredOn { get; }

        public GunasoResolvedEvent(Guid gunasoId, Guid citizenId)
        {
            GunasoId = gunasoId;
            CitizenId = citizenId;
            OccurredOn = DateTime.UtcNow;
        }
    }
}