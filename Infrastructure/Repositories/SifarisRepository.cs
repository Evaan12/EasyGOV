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
    public class SifarisRepository : ISifarisRepository
    {
        private readonly AppDbContext _context;
        private readonly ICacheService _cacheService;

        public SifarisRepository(AppDbContext context, ICacheService cacheService)
        {
            _context = context;
            _cacheService = cacheService;
        }

        public async Task<Sifaris?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _cacheService.GetOrSetAsync(CacheKeys.SifarisById(id), async (ct) =>
            {
                return await _context.Set<Sifaris>().AsNoTracking().FirstOrDefaultAsync(s => s.Id == id, ct);
            }, TimeSpan.FromHours(1), new[] { CacheKeys.Tags.Sifaris }, cancellationToken);
        }

        public async Task<IEnumerable<Sifaris>> GetByCitizenIdAsync(Guid citizenId, CancellationToken cancellationToken = default)
        {
            return await _cacheService.GetOrSetAsync(CacheKeys.SifarisByCitizen(citizenId), async (ct) =>
            {
                return await _context.Set<Sifaris>()
                    .Where(s => s.CitizenId == citizenId)
                    .OrderByDescending(s => s.CreatedAt)
                    .AsNoTracking()
                    .ToListAsync(ct);
            }, TimeSpan.FromHours(1), new[] { CacheKeys.Tags.Sifaris }, cancellationToken);
        }

        public async Task<Sifaris?> GetByApplicationIdAsync(Guid applicationId, CancellationToken cancellationToken = default)
        {
            return await _context.Set<Sifaris>().AsNoTracking().FirstOrDefaultAsync(s => s.ApplicationId == applicationId, cancellationToken);
        }

        public async Task<(IEnumerable<Sifaris> Items, int TotalCount)> GetPaginatedByWardIdAsync(Guid wardId, int skip, int take, string? searchTerm, CancellationToken cancellationToken = default)
        {
            var query = _context.Set<Sifaris>().Where(s => s.WardId == wardId).AsNoTracking();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.ToLower();
                query = query.Where(s => s.Id.ToString().ToLower().Contains(term));
            }

            var count = await query.CountAsync(cancellationToken);
            var items = await query.OrderByDescending(s => s.CreatedAt)
                                   .Skip(skip).Take(take)
                                   .ToListAsync(cancellationToken);

            return (items, count);
        }

        public async Task AddAsync(Sifaris entity, CancellationToken cancellationToken = default)
        {
            await _context.Set<Sifaris>().AddAsync(entity, cancellationToken);
            _ = _cacheService.RemoveMultipleAsync(new[] { CacheKeys.SifarisByCitizen(entity.CitizenId) }, new[] { CacheKeys.Tags.Sifaris }, cancellationToken);
        }

        public Task UpdateAsync(Sifaris entity, CancellationToken cancellationToken = default)
        {
            _context.Set<Sifaris>().Update(entity);
            _ = _cacheService.RemoveMultipleAsync(new[] { CacheKeys.SifarisById(entity.Id), CacheKeys.SifarisByCitizen(entity.CitizenId) }, new[] { CacheKeys.Tags.Sifaris }, cancellationToken);
            return Task.CompletedTask;
        }
    }
}