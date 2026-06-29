using Domain.Entities;
using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Domain.Repositories
{
    public interface ITenantRepository
    {
        Task<Tenant?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<Tenant?> GetByRegistrationIdAsync(string registrationId, CancellationToken cancellationToken = default);
        Task<IEnumerable<Tenant>> GetAllAsync(CancellationToken cancellationToken = default);
        
        Task<(IEnumerable<Tenant> Items, int TotalCount)> GetPaginatedAsync(int skip, int take, string? searchTerm, Guid? provinceId, Guid? districtId, Guid? municipalityId, TenantType? tenantType, bool? isActivated, IEnumerable<Guid>? allowedIds, CancellationToken cancellationToken = default);

        Task<IEnumerable<Tenant>> GetDescendantsAsync(string parentLtreePath, CancellationToken cancellationToken = default);
        Task<bool> IsAncestorAsync(Guid ancestorId, Guid descendantId, CancellationToken ct = default);

        Task AddAsync(Tenant entity, CancellationToken cancellationToken = default);
        Task UpdateAsync(Tenant entity, CancellationToken cancellationToken = default);
    }
}