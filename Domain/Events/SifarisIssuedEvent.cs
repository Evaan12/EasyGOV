using System;
using Domain.Common;

namespace Domain.Events
{
    public class SifarisIssuedEvent : IDomainEvent
    {
        public Guid SifarisId { get; }
        public Guid CitizenId { get; }
        public string EventType => nameof(SifarisIssuedEvent);
        public DateTime OccurredOn { get; }

        public SifarisIssuedEvent(Guid sifarisId, Guid citizenId)
        {
            SifarisId = sifarisId;
            CitizenId = citizenId;
            OccurredOn = DateTime.UtcNow;
        }
    }
}