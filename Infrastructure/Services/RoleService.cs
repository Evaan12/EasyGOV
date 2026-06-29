using Application.Common.Caching;
using Application.Features.Roles.DTOs;
using Application.Interfaces;
using Domain.Enums;
using Domain.Exceptions;
using Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public class RoleService : IRoleService
    {
        private readonly RoleManager<AppRole> _roleManager;
        private readonly ICacheService _cacheService;

        public RoleService(RoleManager<AppRole> roleManager, ICacheService cacheService)
        {
            _roleManager = roleManager;
            _cacheService = cacheService;
        }

        public async Task<Guid> CreateRoleAsync(string name, TenantType tenantType, Guid tenantId, CancellationToken cancellationToken = default)
        {
            var role = new AppRole
            {
                Id = Guid.NewGuid(),
                Name = name,
                TenantType = tenantType,
                TenantId = tenantId
            };

            var result = await _roleManager.CreateAsync(role);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new DomainException($"Failed to create role: {errors}");
            }

            await _cacheService.RemoveAsync(CacheKeys.RoleByName(name), cancellationToken);

            return role.Id;
        }

        public async Task UpdateRoleAsync(Guid roleId, string name, CancellationToken cancellationToken = default)
        {
            var role = await _roleManager.FindByIdAsync(roleId.ToString());
            if (role == null) throw new DomainException("Role not found.");

            if (role.IsDefault == true) throw new DomainException("Cannot modify a default system role.");

            var oldName = role.Name;
            role.Name = name;

            var result = await _roleManager.UpdateAsync(role);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new DomainException($"Failed to update role: {errors}");
            }

            if (oldName != null) await _cacheService.RemoveAsync(CacheKeys.RoleByName(oldName), cancellationToken);
            await _cacheService.RemoveAsync(CacheKeys.RoleByName(name), cancellationToken);
            await _cacheService.RemoveAsync(CacheKeys.RoleById(roleId), cancellationToken);
        }

        public async Task DeleteRoleAsync(Guid roleId, CancellationToken cancellationToken = default)
        {
            var role = await _roleManager.FindByIdAsync(roleId.ToString());
            if (role == null) throw new DomainException("Role not found.");

            if (role.IsDefault == true) throw new DomainException("Cannot delete a default system role.");

            var result = await _roleManager.DeleteAsync(role);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new DomainException($"Failed to delete role: {errors}");
            }

            if (role.Name != null) await _cacheService.RemoveAsync(CacheKeys.RoleByName(role.Name), cancellationToken);
            await _cacheService.RemoveAsync(CacheKeys.RoleById(roleId), cancellationToken);
            await _cacheService.RemoveMultipleAsync(new[] { CacheKeys.RolePermissions(roleId) }, new[] { CacheKeys.Tags.RolePermissions }, cancellationToken);
        }

        public async Task<RoleDto?> GetRoleByIdAsync(Guid roleId, CancellationToken cancellationToken = default)
        {
            return await _cacheService.GetOrSetAsync(CacheKeys.RoleById(roleId), async (ct) =>
            {
                var role = await _roleManager.Roles.AsNoTracking().FirstOrDefaultAsync(r => r.Id == roleId, ct);
                if (role == null) return null;

                return new RoleDto(role.Id, role.Name ?? string.Empty, role.TenantType, role.TenantId, role.IsDefault ?? false);
            }, TimeSpan.FromHours(1), null, cancellationToken);
        }

        public async Task<(IEnumerable<RoleDto> Items, int TotalCount)> GetPaginatedRolesAsync(
            int skip, 
            int take, 
            string? searchTerm, 
            TenantType currentUserTenantType, 
            Guid currentUserTenantId, 
            CancellationToken cancellationToken = default)
        {
            var query = _roleManager.Roles.AsNoTracking();

            if (currentUserTenantType != TenantType.Central)
            {
                // Grants visibility into local roles OR Global seeded Default Roles matching the current tier.
                query = query.Where(r => r.TenantId == currentUserTenantId || (r.IsDefault == true && r.TenantType == currentUserTenantType));
            }

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(r => r.Name != null && r.Name.Contains(searchTerm));
            }

            var totalCount = await query.CountAsync(cancellationToken);

            var roles = await query
                .OrderBy(r => r.Name)
                .Skip(skip)
                .Take(take)
                .Select(r => new RoleDto(r.Id, r.Name ?? string.Empty, r.TenantType, r.TenantId, r.IsDefault ?? false))
                .ToListAsync(cancellationToken);

            return (roles, totalCount);
        }
    }
}