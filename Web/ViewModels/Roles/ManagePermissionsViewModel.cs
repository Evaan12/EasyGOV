using System;
using System.Collections.Generic;

namespace Web.ViewModels.Roles
{
    public class ManagePermissionsViewModel
    {
        public Guid RoleId { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public List<ResourcePermissionsViewModel> Resources { get; set; } = new();
    }
}