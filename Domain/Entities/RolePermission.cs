using System;
using Domain.Common;
using Domain.Enums;
using Domain.Exceptions;
using Domain.ValueObjects;

namespace Domain.Entities
{
    public class RolePermission : Entity
    {
        public Guid RoleId { get; private set; }
        public ResourceType ResourceType { get; private set; }
        public ActionType ActionType { get; private set; }
        
        public AccessTimeWindow? TimeWindow { get; private set; }
        public string? AllowedIpAddress { get; private set; }

        private RolePermission() { }

        public RolePermission(
            Guid id, 
            Guid roleId, 
            ResourceType resourceType, 
            ActionType actionType, 
            AccessTimeWindow? timeWindow, 
            string? allowedIpAddress, 
            Guid createdBy)
            : base(id, createdBy)
        {
            if (actionType == ActionType.None)
                throw new DomainException("A permission record must grant at least one action.");

            RoleId = roleId;
            ResourceType = resourceType;
            ActionType = actionType;
            TimeWindow = timeWindow;
            AllowedIpAddress = allowedIpAddress;
        }

        public void Update(ActionType newActionType, AccessTimeWindow? timeWindow, string? allowedIpAddress, Guid updatedBy)
        {
            if (newActionType == ActionType.None)
                throw new DomainException("A permission record must grant at least one action.");

            ActionType = newActionType;
            TimeWindow = timeWindow;
            AllowedIpAddress = allowedIpAddress;
            UpdatedBy = updatedBy;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}