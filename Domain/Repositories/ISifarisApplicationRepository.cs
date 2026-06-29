using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Domain.Repositories
{
    public interface ISifarisApplicationRepository
    {
        Task<SifarisApplication?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<IEnumerable<SifarisApplication>> GetPendingByWardIdAsync(Guid wardId, CancellationToken cancellationToken = default);
        Task<IEnumerable<SifarisApplication>> GetByCitizenIdAsync(Guid citizenId, CancellationToken cancellationToken = default);
        Task<(IEnumerable<SifarisApplication> Items, int TotalCount)> GetPaginatedByWardIdAsync(Guid wardId, int skip, int take, string? searchTerm, CancellationToken cancellationToken = default);
        Task AddAsync(SifarisApplication entity, CancellationToken cancellationToken = default);
        Task UpdateAsync(SifarisApplication entity, CancellationToken cancellationToken = default);
    }
}