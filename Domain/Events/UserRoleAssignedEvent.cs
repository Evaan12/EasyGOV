using System;
using Domain.Common;

namespace Domain.Events
{
    public class UserRoleAssignedEvent : IDomainEvent
    {
        public Guid UserId { get; }
        public Guid RoleId { get; }
        public string EventType => nameof(UserRoleAssignedEvent);
        public DateTime OccurredOn { get; }

        public UserRoleAssignedEvent(Guid userId, Guid roleId)
        {
            UserId = userId;
            RoleId = roleId;
            OccurredOn = DateTime.UtcNow;
        }
    }
}