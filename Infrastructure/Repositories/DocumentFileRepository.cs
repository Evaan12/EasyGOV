using Domain.Entities;
using Domain.Repositories;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class DocumentFileRepository : IDocumentFileRepository
    {
        private readonly AppDbContext _context;

        public DocumentFileRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<DocumentFile?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Set<DocumentFile>().FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
        }

        public async Task<IEnumerable<DocumentFile>> GetByOwnerIdAsync(Guid ownerId, CancellationToken cancellationToken = default)
        {
            return await _context.Set<DocumentFile>()
                .Where(d => d.OwnerId == ownerId)
                .ToListAsync(cancellationToken);
        }

        public async Task AddAsync(DocumentFile entity, CancellationToken cancellationToken = default)
        {
            await _context.Set<DocumentFile>().AddAsync(entity, cancellationToken);
        }

        public Task DeleteAsync(DocumentFile entity, CancellationToken cancellationToken = default)
        {
            _context.Set<DocumentFile>().Remove(entity);
            return Task.CompletedTask;
        }
    }
}