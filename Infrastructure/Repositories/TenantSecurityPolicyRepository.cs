using Application.Common.Caching;
using Domain.Entities;
using Domain.Repositories;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class TenantSecurityPolicyRepository : ITenantSecurityPolicyRepository
    {
        private readonly AppDbContext _context;
        private readonly ICacheService _cacheService;

        public TenantSecurityPolicyRepository(AppDbContext context, ICacheService cacheService)
        {
            _context = context;
            _cacheService = cacheService;
        }

        public async Task<TenantSecurityPolicy?> GetGlobalPolicyAsync(CancellationToken ct = default)
        {
            return await _cacheService.GetOrSetAsync(CacheKeys.GlobalSecurityPolicy, async (token) =>
            {
                return await _context.Set<TenantSecurityPolicy>().AsNoTracking().FirstOrDefaultAsync(token);
            }, System.TimeSpan.FromHours(1), null, ct);
        }

        public async Task AddAsync(TenantSecurityPolicy entity, CancellationToken ct = default)
        {
            await _context.Set<TenantSecurityPolicy>().AddAsync(entity, ct);
            _ = _cacheService.RemoveAsync(CacheKeys.GlobalSecurityPolicy, ct);
        }

        public Task UpdateAsync(TenantSecurityPolicy entity, CancellationToken ct = default)
        {
            _context.Set<TenantSecurityPolicy>().Update(entity);
            _ = _cacheService.RemoveAsync(CacheKeys.GlobalSecurityPolicy, ct);
            return Task.CompletedTask;
        }
    }
}