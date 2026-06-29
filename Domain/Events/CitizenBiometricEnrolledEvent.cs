using System;
using Domain.Common;

namespace Domain.Events
{
    public class CitizenBiometricEnrolledEvent : IDomainEvent
    {
        public Guid CitizenId { get; }
        public Guid WardId { get; }
        public string EventType => nameof(CitizenBiometricEnrolledEvent);
        public DateTime OccurredOn { get; }

        public CitizenBiometricEnrolledEvent(Guid citizenId, Guid wardId)
        {
            CitizenId = citizenId;
            WardId = wardId;
            OccurredOn = DateTime.UtcNow;
        }
    }
}