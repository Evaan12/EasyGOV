using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Domain.Repositories
{
    public interface ICampaignDispatchRepository
    {
        Task<IEnumerable<CampaignDispatch>> GetByCampaignIdAsync(Guid campaignId, CancellationToken cancellationToken = default);
        Task AddRangeAsync(IEnumerable<CampaignDispatch> dispatches, CancellationToken cancellationToken = default);
        Task UpdateRangeAsync(IEnumerable<CampaignDispatch> dispatches, CancellationToken cancellationToken = default);
    }
}