using Domain.Entities;
using System;
using System.Collections.Generic;

namespace Application.Common.Caching
{
    public class RolePermissionsCacheItem
    {
        public List<RolePermission> Permissions { get; set; } = new();
        public Guid RoleTenantId { get; set; }

        public RolePermissionsCacheItem() { }

        public RolePermissionsCacheItem(List<RolePermission> permissions, Guid roleTenantId)
        {
            Permissions = permissions;
            RoleTenantId = roleTenantId;
        }
    }
}