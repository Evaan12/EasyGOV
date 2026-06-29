using Domain.Enums;
using System;

namespace Web.ViewModels.Roles
{
    public class ResourcePermissionsViewModel
    {
        public ResourceType Resource { get; set; }
        public string ResourceName { get; set; } = string.Empty;
        
        public bool CanCrud { get; set; }
        public bool CanRead { get; set; }
        public bool CanCreate { get; set; }
        public bool CanUpdate { get; set; }
        public bool CanDelete { get; set; }
        public bool CanExport { get; set; }
        public bool CanApprove { get; set; }
        public bool CanAssign { get; set; }
        public bool CanAdmin { get; set; }
        public bool CanActivate { get; set; }

        public ActionType AvailableActions { get; set; }

        public TimeSpan? StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }
        public string? AllowedIpAddress { get; set; }
    }
}