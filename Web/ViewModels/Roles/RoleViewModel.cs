using Domain.Enums;
using System;

namespace Web.ViewModels.Roles
{
    public class RoleViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public TenantType? TenantType { get; set; }
        public bool IsGlobal { get; set; }
        public bool IsDefault { get; set; }
    }
}