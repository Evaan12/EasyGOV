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
    public class GunasoRepository : IGunasoRepository
    {
        private readonly AppDbContext _context;

        public GunasoRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Gunaso?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Set<Gunaso>().FirstOrDefaultAsync(g => g.Id == id, cancellationToken);
        }

        public async Task<(IEnumerable<Gunaso> Items, int TotalCount)> GetPaginatedAsync(string? targetLtreePath, bool isCitizen, Guid? citizenId, string? citizenWardLtreePath, int skip, int take, CancellationToken ct = default)
        {
            IQueryable<Gunaso> query;

            // Citizens natively observe their own records AND any overarching public grievance posted to their hierarchy lineage
            if (isCitizen && citizenId.HasValue && citizenId != Guid.Empty && !string.IsNullOrEmpty(citizenWardLtreePath))
            {
                query = _context.Set<Gunaso>()
                    .FromSqlInterpolated($"SELECT * FROM \"Gunasos\" WHERE \"IsDeleted\" = false AND ({citizenWardLtreePath}::ltree <@ \"TargetLtreePath\"::ltree OR \"CitizenId\" = {citizenId.Value})")
                    .AsNoTracking();
            }
            else if (isCitizen && citizenId.HasValue && citizenId != Guid.Empty)
            {
                query = _context.Set<Gunaso>().Where(g => g.CitizenId == citizenId && !g.IsDeleted).AsNoTracking();
            }
            else if (!string.IsNullOrEmpty(targetLtreePath))
            {
                query = _context.Set<Gunaso>()
                    .FromSqlInterpolated($"SELECT * FROM \"Gunasos\" WHERE \"TargetLtreePath\" <@ {targetLtreePath}::ltree AND \"IsDeleted\" = false")
                    .AsNoTracking();
            }
            else 
            {
                query = _context.Set<Gunaso>().Where(g => !g.IsDeleted).AsNoTracking();
            }

            var count = await query.CountAsync(ct);
            var items = await query.OrderByDescending(g => g.CreatedAt).Skip(skip).Take(take).ToListAsync(ct);
            
            return (items, count);
        }

        public async Task AddAsync(Gunaso entity, CancellationToken cancellationToken = default)
        {
            await _context.Set<Gunaso>().AddAsync(entity, cancellationToken);
        }

        public Task UpdateAsync(Gunaso entity, CancellationToken cancellationToken = default)
        {
            _context.Set<Gunaso>().Update(entity);
            return Task.CompletedTask;
        }
    }
}