using Application.Features.Roles.DTOs;
using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IRoleService
    {
        Task<Guid> CreateRoleAsync(string name, TenantType tenantType, Guid tenantId, CancellationToken cancellationToken = default);
        Task UpdateRoleAsync(Guid roleId, string name, CancellationToken cancellationToken = default);
        Task DeleteRoleAsync(Guid roleId, CancellationToken cancellationToken = default);
        
        Task<RoleDto?> GetRoleByIdAsync(Guid roleId, CancellationToken cancellationToken = default);
        
        Task<(IEnumerable<RoleDto> Items, int TotalCount)> GetPaginatedRolesAsync(
            int skip, 
            int take, 
            string? searchTerm, 
            TenantType currentUserTenantType, 
            Guid currentUserTenantId, 
            CancellationToken cancellationToken = default);
    }
}