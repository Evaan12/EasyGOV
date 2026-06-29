using System;
using Domain.Common;

namespace Domain.Events
{
    public class SifarisRevokedEvent : IDomainEvent
    {
        public Guid SifarisId { get; }
        public Guid CitizenId { get; }
        public string Reason { get; }
        public string EventType => nameof(SifarisRevokedEvent);
        public DateTime OccurredOn { get; }

        public SifarisRevokedEvent(Guid sifarisId, Guid citizenId, string reason)
        {
            SifarisId = sifarisId;
            CitizenId = citizenId;
            Reason = reason;
            OccurredOn = DateTime.UtcNow;
        }
    }
}