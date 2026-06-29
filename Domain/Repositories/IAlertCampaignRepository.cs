using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Domain.Repositories
{
    public interface IAlertCampaignRepository
    {
        Task<AlertCampaign?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<IEnumerable<AlertCampaign>> GetAllRunningAsync(CancellationToken cancellationToken = default);
        Task<(IEnumerable<AlertCampaign> Items, int TotalCount)> GetPaginatedAsync(IEnumerable<Guid>? allowedTenantIds, int skip, int take, CancellationToken cancellationToken = default);
        Task AddAsync(AlertCampaign entity, CancellationToken cancellationToken = default);
        Task UpdateAsync(AlertCampaign entity, CancellationToken cancellationToken = default);
    }
}