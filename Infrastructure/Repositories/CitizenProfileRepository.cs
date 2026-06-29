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
    public class CitizenProfileRepository : ICitizenProfileRepository
    {
        private readonly AppDbContext _context;
        private readonly ICacheService _cacheService;

        public CitizenProfileRepository(AppDbContext context, ICacheService cacheService)
        {
            _context = context;
            _cacheService = cacheService;
        }

        public async Task<CitizenProfile?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _cacheService.GetOrSetAsync(CacheKeys.CitizenProfile(id), async (ct) =>
            {
                return await _context.Set<CitizenProfile>().AsNoTracking().FirstOrDefaultAsync(c => c.Id == id, ct);
            }, TimeSpan.FromHours(2), new[] { CacheKeys.Tags.CitizenProfiles }, cancellationToken);
        }

        public async Task<CitizenProfile?> GetByCitizenshipNumberAsync(string citizenshipNumber, CancellationToken cancellationToken = default)
        {
            return await _cacheService.GetOrSetAsync(CacheKeys.CitizenByCitizenship(citizenshipNumber), async (ct) =>
            {
                return await _context.Set<CitizenProfile>()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c => c.Citizenship != null && c.Citizenship.CitizenshipNumber == citizenshipNumber, ct);
            }, TimeSpan.FromHours(2), new[] { CacheKeys.Tags.CitizenProfiles }, cancellationToken);
        }

        public async Task<IEnumerable<CitizenProfile>> GetActiveCitizensByWardIdsAsync(IEnumerable<Guid> wardIds, bool requireMobile = false, CancellationToken cancellationToken = default)
        {
            var query = _context.Set<CitizenProfile>()
                .Where(c => c.Status == Domain.Enums.CitizenStatus.Active && wardIds.Contains(c.RegisteredWardId));

            if (requireMobile)
            {
                query = query.Where(c => c.MobileNumber != null);
            }

            return await query.AsNoTracking().ToListAsync(cancellationToken);
        }

        public async Task<(IEnumerable<CitizenProfile> Items, int TotalCount)> GetPaginatedAsync(IEnumerable<Guid>? allowedWardIds, int skip, int take, string? searchTerm, CancellationToken cancellationToken = default)
        {
            var query = _context.Set<CitizenProfile>().AsNoTracking();

            if (allowedWardIds != null)
            {
                query = query.Where(c => allowedWardIds.Contains(c.RegisteredWardId));
            }

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.ToLowerInvariant();
                query = query.Where(c => c.FullName.ToLower().Contains(term) || (c.Citizenship != null && c.Citizenship.CitizenshipNumber.Contains(term)));
            }

            var totalCount = await query.CountAsync(cancellationToken);
            var items = await query.OrderBy(c => c.FullName)
                                   .Skip(skip)
                                   .Take(take)
                                   .ToListAsync(cancellationToken);

            return (items, totalCount);
        }

        public async Task AddAsync(CitizenProfile entity, CancellationToken cancellationToken = default)
        {
            await _context.Set<CitizenProfile>().AddAsync(entity, cancellationToken);
            _ = _cacheService.RemoveMultipleAsync(Array.Empty<string>(), new[] { CacheKeys.Tags.CitizenProfiles }, cancellationToken);
        }

        public Task UpdateAsync(CitizenProfile entity, CancellationToken cancellationToken = default)
        {
            _context.Set<CitizenProfile>().Update(entity);
            
            var keysToRemove = new List<string> { CacheKeys.CitizenProfile(entity.Id) };
            if (entity.Citizenship != null)
                keysToRemove.Add(CacheKeys.CitizenByCitizenship(entity.Citizenship.CitizenshipNumber));

            _ = _cacheService.RemoveMultipleAsync(keysToRemove, new[] { CacheKeys.Tags.CitizenProfiles }, cancellationToken);
            return Task.CompletedTask;
        }
    }
}