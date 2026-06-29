using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Domain.Repositories
{
    public interface ICitizenProfileRepository
    {
        Task<CitizenProfile?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<CitizenProfile?> GetByCitizenshipNumberAsync(string citizenshipNumber, CancellationToken cancellationToken = default);
        
        Task<IEnumerable<CitizenProfile>> GetActiveCitizensByWardIdsAsync(IEnumerable<Guid> wardIds, bool requireMobile = false, CancellationToken cancellationToken = default);

        Task<(IEnumerable<CitizenProfile> Items, int TotalCount)> GetPaginatedAsync(IEnumerable<Guid>? allowedWardIds, int skip, int take, string? searchTerm, CancellationToken cancellationToken = default);

        Task AddAsync(CitizenProfile entity, CancellationToken cancellationToken = default);
        Task UpdateAsync(CitizenProfile entity, CancellationToken cancellationToken = default);
    }
}