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
    public class AlertCampaignRepository : IAlertCampaignRepository
    {
        private readonly AppDbContext _context;
        public AlertCampaignRepository(AppDbContext context) => _context = context;

        public async Task<AlertCampaign?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
            await _context.AlertCampaigns.Include(c => c.Approvals).FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted, cancellationToken);

        public async Task<IEnumerable<AlertCampaign>> GetAllRunningAsync(CancellationToken cancellationToken = default) =>
            await _context.AlertCampaigns.Where(c => c.Status == Domain.Enums.CampaignStatus.Running && !c.IsDeleted).ToListAsync(cancellationToken);

        public async Task<(IEnumerable<AlertCampaign> Items, int TotalCount)> GetPaginatedAsync(IEnumerable<Guid>? allowedTenantIds, int skip, int take, CancellationToken cancellationToken = default)
        {
            var query = _context.AlertCampaigns.Include(c => c.Approvals).Where(c => !c.IsDeleted).AsNoTracking();

            if (allowedTenantIds != null)
            {
                query = query.Where(c => allowedTenantIds.Contains(c.TargetTenantId));
            }

            var totalCount = await query.CountAsync(cancellationToken);
            var items = await query.OrderByDescending(c => c.CreatedAt)
                                   .Skip(skip)
                                   .Take(take)
                                   .ToListAsync(cancellationToken);

            return (items, totalCount);
        }

        public async Task AddAsync(AlertCampaign entity, CancellationToken cancellationToken = default) => 
            await _context.AlertCampaigns.AddAsync(entity, cancellationToken);

        public Task UpdateAsync(AlertCampaign entity, CancellationToken cancellationToken = default)
        {
            _context.AlertCampaigns.Update(entity);
            return Task.CompletedTask;
        }
    }
}