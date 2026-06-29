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
    public class CampaignDispatchRepository : ICampaignDispatchRepository
    {
        private readonly AppDbContext _context;
        public CampaignDispatchRepository(AppDbContext context) => _context = context;

        public async Task<IEnumerable<CampaignDispatch>> GetByCampaignIdAsync(Guid campaignId, CancellationToken cancellationToken = default) =>
            await _context.CampaignDispatches.Where(d => d.AlertCampaignId == campaignId && !d.IsDeleted).ToListAsync(cancellationToken);

        public async Task AddRangeAsync(IEnumerable<CampaignDispatch> dispatches, CancellationToken cancellationToken = default) => 
            await _context.CampaignDispatches.AddRangeAsync(dispatches, cancellationToken);

        public Task UpdateRangeAsync(IEnumerable<CampaignDispatch> dispatches, CancellationToken cancellationToken = default)
        {
            _context.CampaignDispatches.UpdateRange(dispatches);
            return Task.CompletedTask;
        }
    }
}