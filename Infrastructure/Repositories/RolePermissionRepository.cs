using Application.Common.Caching;
using Domain.Entities;
using Domain.Repositories;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class RolePermissionRepository : IRolePermissionRepository
    {
        private readonly AppDbContext _context;
        private readonly ICacheService _cacheService;

        public RolePermissionRepository(AppDbContext context, ICacheService cacheService)
        {
            _context = context;
            _cacheService = cacheService;
        }

        public async Task<RolePermission?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
            await _context.Set<RolePermission>().AsNoTracking().FirstOrDefaultAsync(r => r.Id == id, ct);

        public async Task<IEnumerable<RolePermission>> GetByRoleIdAsync(Guid roleId, CancellationToken ct = default)
        {
            return await _cacheService.GetOrSetAsync(CacheKeys.RolePermissions(roleId), async (token) =>
            {
                return await _context.Set<RolePermission>().Where(r => r.RoleId == roleId).AsNoTracking().ToListAsync(token);
            }, TimeSpan.FromHours(1), new[] { CacheKeys.Tags.RolePermissions }, ct);
        }

        public async Task AddAsync(RolePermission entity, CancellationToken ct = default)
        {
            await _context.Set<RolePermission>().AddAsync(entity, ct);
            _ = _cacheService.RemoveMultipleAsync(new[] { CacheKeys.RolePermissions(entity.RoleId) }, new[] { CacheKeys.Tags.RolePermissions }, ct);
        }

        public Task UpdateAsync(RolePermission entity, CancellationToken ct = default)
        {
            _context.Set<RolePermission>().Update(entity);
            _ = _cacheService.RemoveMultipleAsync(new[] { CacheKeys.RolePermissions(entity.RoleId) }, new[] { CacheKeys.Tags.RolePermissions }, ct);
            return Task.CompletedTask;
        }

        public Task DeleteAsync(RolePermission entity, CancellationToken ct = default)
        {
            _context.Set<RolePermission>().Remove(entity);
            _ = _cacheService.RemoveMultipleAsync(new[] { CacheKeys.RolePermissions(entity.RoleId) }, new[] { CacheKeys.Tags.RolePermissions }, ct);
            return Task.CompletedTask;
        }
    }
}