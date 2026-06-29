using Domain.Entities;
using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Domain.Repositories
{
    public interface IDocumentTemplateRepository
    {
        Task<DocumentTemplate?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<IEnumerable<DocumentTemplate>> GetActiveTemplatesForTenantAsync(Guid tenantId, TemplateType type, CancellationToken cancellationToken = default);
        Task<(IEnumerable<DocumentTemplate> Items, int TotalCount)> GetPaginatedAsync(Guid? tenantId, TenantType tenantType, int skip, int take, string? searchTerm, CancellationToken cancellationToken = default);
        Task AddAsync(DocumentTemplate entity, CancellationToken cancellationToken = default);
        Task UpdateAsync(DocumentTemplate entity, CancellationToken cancellationToken = default);
    }
}