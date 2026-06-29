using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Domain.Repositories
{
    public interface ISifarisRepository
    {
        Task<Sifaris?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<IEnumerable<Sifaris>> GetByCitizenIdAsync(Guid citizenId, CancellationToken cancellationToken = default);
        Task<Sifaris?> GetByApplicationIdAsync(Guid applicationId, CancellationToken cancellationToken = default);
        Task<(IEnumerable<Sifaris> Items, int TotalCount)> GetPaginatedByWardIdAsync(Guid wardId, int skip, int take, string? searchTerm, CancellationToken cancellationToken = default);
        Task AddAsync(Sifaris entity, CancellationToken cancellationToken = default);
        Task UpdateAsync(Sifaris entity, CancellationToken cancellationToken = default);
    }
}