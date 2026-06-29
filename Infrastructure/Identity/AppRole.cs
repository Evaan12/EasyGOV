using Domain.Entities;
using Domain.Enums;
using Microsoft.AspNetCore.Identity;
using System;

namespace Infrastructure.Identity
{
    public class AppRole : IdentityRole<Guid>
    {
        public TenantType TenantType { get; set; }
        public Guid TenantId { get; set; }
        public Tenant? Tenant { get; set; }
        
        public bool? IsDefault { get; set; }
    }
}