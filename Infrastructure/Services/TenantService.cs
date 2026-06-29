using Application.Common.Caching;
using Application.Interfaces;
using Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public class TenantService : ITenantService
    {
        private readonly ITenantRepository _tenantRepository;
        private readonly ICacheService _cacheService;

        public TenantService(ITenantRepository tenantRepository, ICacheService cacheService)
        {
            _tenantRepository = tenantRepository;
            _cacheService = cacheService;
        }

        public async Task<IEnumerable<Guid>> GetAllowedTenantIdsAsync(Guid currentTenantId, CancellationToken cancellationToken = default)
        {
            var cacheKey = CacheKeys.TenantDescendants(currentTenantId);

            return await _cacheService.GetOrSetAsync(cacheKey, async ct =>
            {
                var tenant = await _tenantRepository.GetByIdAsync(currentTenantId, ct);
                if (tenant == null) return new List<Guid>();

                // Fast Native Hierarchical extraction utilizing the ltree operator
                var descendants = await _tenantRepository.GetDescendantsAsync(tenant.LtreePath, ct);
                
                var allowedIds = new HashSet<Guid>();
                foreach (var descendant in descendants)
                {
                    allowedIds.Add(descendant.Id);
                }

                return allowedIds.AsEnumerable();
            }, TimeSpan.FromHours(12), new[] { CacheKeys.Tags.Tenants }, cancellationToken);
        }
    }
}