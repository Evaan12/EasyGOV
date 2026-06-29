using System;
using Domain.Common;

namespace Domain.Events
{
    public class UserSuspendedEvent : IDomainEvent
    {
        public Guid UserId { get; }
        public TimeSpan Duration { get; }
        public string EventType => nameof(UserSuspendedEvent);
        public DateTime OccurredOn { get; }

        public UserSuspendedEvent(Guid userId, TimeSpan duration)
        {
            UserId = userId;
            Duration = duration;
            OccurredOn = DateTime.UtcNow;
        }
    }
}