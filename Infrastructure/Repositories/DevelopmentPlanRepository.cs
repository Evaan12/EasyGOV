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
    public class DevelopmentPlanRepository : IDevelopmentPlanRepository
    {
        private readonly AppDbContext _context;

        public DevelopmentPlanRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<DevelopmentPlan?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Set<DevelopmentPlan>().FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        }

        public async Task<(IEnumerable<DevelopmentPlan> Items, int TotalCount)> GetVisiblePlansAsync(string? citizenOrAdminLtreePath, int skip, int take, CancellationToken ct = default)
        {
            IQueryable<DevelopmentPlan> query;

            if (string.IsNullOrEmpty(citizenOrAdminLtreePath))
            {
                // Central Admin access unbridled
                query = _context.Set<DevelopmentPlan>().Where(p => !p.IsDeleted).AsNoTracking();
            }
            else
            {
                // Leveraging Postgres ltree operator '@>' (is ancestor of)
                query = _context.Set<DevelopmentPlan>()
                    .FromSqlInterpolated($"SELECT * FROM \"DevelopmentPlans\" WHERE \"TenantLtreePath\"::ltree @> {citizenOrAdminLtreePath}::ltree AND \"IsDeleted\" = false")
                    .AsNoTracking();
            }
                
            var count = await query.CountAsync(ct);
            var items = await query.OrderByDescending(p => p.StartDate).Skip(skip).Take(take).ToListAsync(ct);
            
            return (items, count);
        }

        public async Task AddAsync(DevelopmentPlan entity, CancellationToken cancellationToken = default)
        {
            await _context.Set<DevelopmentPlan>().AddAsync(entity, cancellationToken);
        }

        public Task UpdateAsync(DevelopmentPlan entity, CancellationToken cancellationToken = default)
        {
            _context.Set<DevelopmentPlan>().Update(entity);
            return Task.CompletedTask;
        }
    }
}