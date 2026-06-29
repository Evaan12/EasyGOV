using Application.Common.Caching;
using Domain.Entities;
using Domain.Repositories;
using Domain.ValueObjects;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Pgvector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class MissingPersonRepository : IMissingPersonRepository
    {
        private readonly AppDbContext _context;
        private readonly ICacheService _cacheService;

        public MissingPersonRepository(AppDbContext context, ICacheService cacheService)
        {
            _context = context;
            _cacheService = cacheService;
        }

        public async Task<MissingPerson?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Set<MissingPerson>().FirstOrDefaultAsync(m => m.Id == id, cancellationToken);
        }

        public async Task<IEnumerable<MissingPerson>> GetActiveMissingPersonsAsync(CancellationToken cancellationToken = default)
        {
            string cacheKey = "ActiveMissingPersons";

            return await _cacheService.GetOrSetAsync(cacheKey, async (ct) =>
            {
                return await _context.Set<MissingPerson>()
                    .Where(m => !m.IsFound)
                    .AsNoTracking()
                    .ToListAsync(ct);
            }, TimeSpan.FromMinutes(10), new[] { "MissingPersons_Tag" }, cancellationToken);
        }

        public async Task<(MissingPerson MissingPerson, double SimilarityScore)?> FindNearestMatchAsync(BiometricEmbedding targetEmbedding, double threshold, CancellationToken cancellationToken = default)
        {
            var vectorParam = new Vector(targetEmbedding.VectorData);

            // Fetch Nearest Neighbor optimally utilizing HNSW pgvector indexing using native parameter.
            // This safely avoids strict PostgreSQL float casting runtime exceptions.
            var match = await _context.Set<MissingPerson>()
                .FromSqlRaw(@"
                    SELECT * FROM ""MissingPersons"" 
                    WHERE ""IsFound"" = false 
                    ORDER BY ""FaceVectorData"" <=> {0} 
                    LIMIT 1
                ", vectorParam)
                .AsNoTracking()
                .FirstOrDefaultAsync(cancellationToken);

            if (match != null)
            {
                double exactSimilarity = match.FaceEmbedding.ComputeCosineSimilarity(targetEmbedding);
                
                // Compare to our tolerant minimum threshold natively
                if (exactSimilarity >= threshold)
                {
                    return (match, exactSimilarity);
                }
            }

            return null;
        }

        public async Task AddAsync(MissingPerson entity, CancellationToken cancellationToken = default)
        {
            await _context.Set<MissingPerson>().AddAsync(entity, cancellationToken);
            _ = _cacheService.RemoveMultipleAsync(Array.Empty<string>(), new[] { "MissingPersons_Tag" }, cancellationToken);
        }

        public Task UpdateAsync(MissingPerson entity, CancellationToken cancellationToken = default)
        {
            _context.Set<MissingPerson>().Update(entity);
            _ = _cacheService.RemoveMultipleAsync(Array.Empty<string>(), new[] { "MissingPersons_Tag" }, cancellationToken);
            return Task.CompletedTask;
        }
    }
}
