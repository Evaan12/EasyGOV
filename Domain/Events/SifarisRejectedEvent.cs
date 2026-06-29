using System;
using Domain.Common;

namespace Domain.Events
{
    public class SifarisRejectedEvent : IDomainEvent
    {
        public Guid SifarisId { get; }
        public Guid CitizenId { get; }
        public string Reason { get; }
        public string EventType => nameof(SifarisRejectedEvent);
        public DateTime OccurredOn { get; }

        public SifarisRejectedEvent(Guid sifarisId, Guid citizenId, string reason)
        {
            SifarisId = sifarisId;
            CitizenId = citizenId;
            Reason = reason;
            OccurredOn = DateTime.UtcNow;
        }
    }
}