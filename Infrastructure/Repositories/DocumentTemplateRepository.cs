using Application.Common.Caching;
using Domain.Entities;
using Domain.Enums;
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
    public class DocumentTemplateRepository : IDocumentTemplateRepository
    {
        private readonly AppDbContext _context;
        private readonly ICacheService _cacheService;

        public DocumentTemplateRepository(AppDbContext context, ICacheService cacheService)
        {
            _context = context;
            _cacheService = cacheService;
        }

        public async Task<DocumentTemplate?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Set<DocumentTemplate>().FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
        }

        public async Task<IEnumerable<DocumentTemplate>> GetActiveTemplatesForTenantAsync(Guid tenantId, TemplateType type, CancellationToken cancellationToken = default)
        {
            string cacheKey = $"DocTemplates_{tenantId}_{type}";
            
            return await _cacheService.GetOrSetAsync(cacheKey, async (ct) =>
            {
                return await _context.Set<DocumentTemplate>()
                    .Where(t => t.Type == type && (t.TenantType == TenantType.Central || t.TenantId == tenantId))
                    .AsNoTracking()
                    .ToListAsync(ct);
            }, TimeSpan.FromHours(4), new[] { "DocTemplates_Tag" }, cancellationToken);
        }

        public async Task<(IEnumerable<DocumentTemplate> Items, int TotalCount)> GetPaginatedAsync(Guid? tenantId, TenantType tenantType, int skip, int take, string? searchTerm, CancellationToken cancellationToken = default)
        {
            var query = _context.Set<DocumentTemplate>().AsNoTracking();

            if (tenantType != TenantType.Central)
            {
                query = query.Where(t => t.TenantId == tenantId || t.TenantType == TenantType.Central);
            }

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.ToLower();
                query = query.Where(t => t.Name.ToLower().Contains(term));
            }

            var totalCount = await query.CountAsync(cancellationToken);
            var items = await query.OrderByDescending(t => t.CreatedAt)
                                   .Skip(skip)
                                   .Take(take)
                                   .ToListAsync(cancellationToken);

            return (items, totalCount);
        }

        public async Task AddAsync(DocumentTemplate entity, CancellationToken cancellationToken = default)
        {
            await _context.Set<DocumentTemplate>().AddAsync(entity, cancellationToken);
            _ = _cacheService.RemoveMultipleAsync(Array.Empty<string>(), new[] { "DocTemplates_Tag" }, cancellationToken);
        }

        public Task UpdateAsync(DocumentTemplate entity, CancellationToken cancellationToken = default)
        {
            _context.Set<DocumentTemplate>().Update(entity);
            _ = _cacheService.RemoveMultipleAsync(Array.Empty<string>(), new[] { "DocTemplates_Tag" }, cancellationToken);
            return Task.CompletedTask;
        }
    }
}