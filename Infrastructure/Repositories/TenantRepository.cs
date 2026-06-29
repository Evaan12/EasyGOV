using Application.Common.Caching;
using Domain.Entities;
using Domain.Enums;
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
    public class TenantRepository : ITenantRepository
    {
        private readonly AppDbContext _context;
        private readonly ICacheService _cacheService;

        public TenantRepository(AppDbContext context, ICacheService cacheService)
        {
            _context = context;
            _cacheService = cacheService;
        }

        public async Task<Tenant?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
            await _context.Tenants.AsNoTracking().FirstOrDefaultAsync(t => t.Id == id, ct);

        public async Task<Tenant?> GetByRegistrationIdAsync(string registrationId, CancellationToken cancellationToken = default) =>
            await _context.Tenants.AsNoTracking().FirstOrDefaultAsync(t => t.RegistrationId == registrationId, cancellationToken);

        public async Task<IEnumerable<Tenant>> GetAllAsync(CancellationToken ct = default)
        {
            return await _cacheService.GetOrSetAsync(CacheKeys.AllTenants, async (token) =>
            {
                return await _context.Tenants.AsNoTracking().ToListAsync(token);
            }, TimeSpan.FromHours(12), new[] { CacheKeys.Tags.Tenants }, ct);
        }

        public async Task<(IEnumerable<Tenant> Items, int TotalCount)> GetPaginatedAsync(int skip, int take, string? searchTerm, Guid? provinceId, Guid? districtId, Guid? municipalityId, TenantType? tenantType, bool? isActivated, IEnumerable<Guid>? allowedIds, CancellationToken ct = default)
        {
            var query = _context.Tenants.AsNoTracking();

            if (allowedIds != null)
            {
                query = query.Where(t => allowedIds.Contains(t.Id));
            }

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.ToLowerInvariant()
                    .Replace("ward number", "")
                    .Replace("ward no.", "")
                    .Replace("ward no", "")
                    .Replace("ward", "")
                    .Replace("municipality", "")
                    .Replace("district", "")
                    .Replace("province", "")
                    .Trim();

                var terms = term.Split(new[] { ' ', ',', '-' }, StringSplitOptions.RemoveEmptyEntries);

                foreach (var t in terms)
                {
                    query = query.Where(tenant => 
                        tenant.Name.ToLower().Contains(t) ||
                        _context.Tenants.Any(m => m.Id == tenant.MunicipalityId && m.Name.ToLower().Contains(t)) ||
                        _context.Tenants.Any(d => d.Id == tenant.DistrictId && d.Name.ToLower().Contains(t)) ||
                        _context.Tenants.Any(p => p.Id == tenant.ProvinceId && p.Name.ToLower().Contains(t))
                    );
                }
            }

            if (provinceId.HasValue)
                query = query.Where(t => t.ProvinceId == provinceId.Value || t.Id == provinceId.Value);

            if (districtId.HasValue)
                query = query.Where(t => t.DistrictId == districtId.Value || t.Id == districtId.Value);

            if (municipalityId.HasValue)
                query = query.Where(t => t.MunicipalityId == municipalityId.Value || t.Id == municipalityId.Value);
                
            if (tenantType.HasValue)
                query = query.Where(t => t.TenantType == tenantType.Value);

            if (isActivated.HasValue)
                query = query.Where(t => t.IsActivated == isActivated.Value);

            var totalCount = await query.CountAsync(ct);
            var items = await query.OrderBy(t => t.TenantType).ThenBy(t => t.Name)
                                   .Skip(skip)
                                   .Take(take)
                                   .ToListAsync(ct);

            return (items, totalCount);
        }

        public async Task<IEnumerable<Tenant>> GetDescendantsAsync(string parentLtreePath, CancellationToken cancellationToken = default)
        {
            return await _context.Tenants
                .FromSqlInterpolated($"SELECT * FROM \"Tenants\" WHERE \"LtreePath\" <@ {parentLtreePath}::ltree AND \"IsDeleted\" = false")
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        public async Task<bool> IsAncestorAsync(Guid ancestorId, Guid descendantId, CancellationToken ct = default)
        {
            return await _context.Tenants
                .FromSqlInterpolated($"SELECT * FROM \"Tenants\" WHERE \"Id\" = {descendantId} AND \"LtreePath\" <@ (SELECT \"LtreePath\" FROM \"Tenants\" WHERE \"Id\" = {ancestorId})")
                .AnyAsync(ct);
        }

        public async Task AddAsync(Tenant entity, CancellationToken ct = default)
        {
            await _context.Tenants.AddAsync(entity, ct);
            _ = _cacheService.RemoveMultipleAsync(Array.Empty<string>(), new[] { CacheKeys.Tags.Tenants }, ct);
        }

        public Task UpdateAsync(Tenant entity, CancellationToken ct = default)
        {
            _context.Tenants.Update(entity);
            _ = _cacheService.RemoveMultipleAsync(Array.Empty<string>(), new[] { CacheKeys.Tags.Tenants }, ct);
            return Task.CompletedTask;
        }
    }
}