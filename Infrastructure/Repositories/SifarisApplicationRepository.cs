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
    public class SifarisApplicationRepository : ISifarisApplicationRepository
    {
        private readonly AppDbContext _context;

        public SifarisApplicationRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<SifarisApplication?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Set<SifarisApplication>().FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
        }

        public async Task<IEnumerable<SifarisApplication>> GetPendingByWardIdAsync(Guid wardId, CancellationToken cancellationToken = default)
        {
            return await _context.Set<SifarisApplication>()
                .Where(s => s.TargetWardId == wardId && s.Status == ApplicationStatus.PendingReview)
                .OrderBy(s => s.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<SifarisApplication>> GetByCitizenIdAsync(Guid citizenId, CancellationToken cancellationToken = default)
        {
            return await _context.Set<SifarisApplication>()
                .Where(s => s.CitizenId == citizenId)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<(IEnumerable<SifarisApplication> Items, int TotalCount)> GetPaginatedByWardIdAsync(Guid wardId, int skip, int take, string? searchTerm, CancellationToken cancellationToken = default)
        {
            var query = _context.Set<SifarisApplication>()
                .Where(s => s.TargetWardId == wardId)
                .AsNoTracking();

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

        public async Task AddAsync(SifarisApplication entity, CancellationToken cancellationToken = default)
        {
            await _context.Set<SifarisApplication>().AddAsync(entity, cancellationToken);
        }

        public Task UpdateAsync(SifarisApplication entity, CancellationToken cancellationToken = default)
        {
            _context.Set<SifarisApplication>().Update(entity);
            return Task.CompletedTask;
        }
    }
}