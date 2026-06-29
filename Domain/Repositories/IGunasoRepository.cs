using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Domain.Repositories
{
    public interface IGunasoRepository
    {
        Task<Gunaso?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<(IEnumerable<Gunaso> Items, int TotalCount)> GetPaginatedAsync(string? targetLtreePath, bool isCitizen, Guid? citizenId, string? citizenWardLtreePath, int skip, int take, CancellationToken ct = default);
        Task AddAsync(Gunaso entity, CancellationToken cancellationToken = default);
        Task UpdateAsync(Gunaso entity, CancellationToken cancellationToken = default);
    }
}