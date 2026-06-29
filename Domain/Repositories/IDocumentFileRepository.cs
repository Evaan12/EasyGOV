using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Domain.Repositories
{
    public interface IDocumentFileRepository
    {
        Task<DocumentFile?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<IEnumerable<DocumentFile>> GetByOwnerIdAsync(Guid ownerId, CancellationToken cancellationToken = default);
        Task AddAsync(DocumentFile entity, CancellationToken cancellationToken = default);
        Task DeleteAsync(DocumentFile entity, CancellationToken cancellationToken = default);
    }
}