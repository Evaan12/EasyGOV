using System;
using Domain.Common;

namespace Domain.Events
{
    public class UserBannedEvent : IDomainEvent
    {
        public Guid UserId { get; }
        public string Reason { get; }
        public string EventType => nameof(UserBannedEvent);
        public DateTime OccurredOn { get; }

        public UserBannedEvent(Guid userId, string reason)
        {
            UserId = userId;
            Reason = reason;
            OccurredOn = DateTime.UtcNow;
        }
    }
}