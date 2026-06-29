using Domain.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace Web.ViewModels.Roles
{
    public class RoleFormViewModel
    {
        public Guid? Id { get; set; }

        [Required(ErrorMessage = "Role Name is required.")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Target Tier")]
        [Required(ErrorMessage = "Target Tier must be mapped accordingly.")]
        public TenantType TenantType { get; set; }
    }
}