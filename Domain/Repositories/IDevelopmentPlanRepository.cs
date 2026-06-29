using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Domain.Repositories
{
    public interface IDevelopmentPlanRepository
    {
        Task<DevelopmentPlan?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<(IEnumerable<DevelopmentPlan> Items, int TotalCount)> GetVisiblePlansAsync(string? citizenOrAdminLtreePath, int skip, int take, CancellationToken ct = default);
        Task AddAsync(DevelopmentPlan entity, CancellationToken cancellationToken = default);
        Task UpdateAsync(DevelopmentPlan entity, CancellationToken cancellationToken = default);
    }
}