using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;

namespace Web.ViewModels.Users
{
    public class UserManagementViewModel
    {
        public Guid SelectedUserId { get; set; }
        public int SuspendDurationHours { get; set; } = 24;
        public string BanReason { get; set; } = string.Empty;
        public Guid SelectedRoleId { get; set; }
        public List<SelectListItem> AvailableRoles { get; set; } = new();
    }
}