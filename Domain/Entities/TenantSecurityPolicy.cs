using System;
using Domain.Common;
using Domain.ValueObjects;

namespace Domain.Entities
{
    public class TenantSecurityPolicy : Entity, IAggregateRoot
    {
        public AccessTimeWindow? TimeWindow { get; private set; }
        public string? AllowedIpAddress { get; private set; }

        private TenantSecurityPolicy() { }

        public TenantSecurityPolicy(Guid id, AccessTimeWindow? timeWindow, string? allowedIpAddress, Guid createdBy) 
            : base(id, createdBy)
        {
            TimeWindow = timeWindow;
            AllowedIpAddress = allowedIpAddress;
        }

        public void UpdateSecurityConstraints(AccessTimeWindow? timeWindow, string? allowedIpAddress, Guid updatedBy)
        {
            TimeWindow = timeWindow;
            AllowedIpAddress = allowedIpAddress;
            UpdatedBy = updatedBy;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}