using Domain.Entities;
using Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Domain.Repositories
{
    public interface IMissingPersonRepository
    {
        Task<MissingPerson?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<IEnumerable<MissingPerson>> GetActiveMissingPersonsAsync(CancellationToken cancellationToken = default);
        
        Task<(MissingPerson MissingPerson, double SimilarityScore)?> FindNearestMatchAsync(BiometricEmbedding targetEmbedding, double threshold, CancellationToken cancellationToken = default);
        
        Task AddAsync(MissingPerson entity, CancellationToken cancellationToken = default);
        Task UpdateAsync(MissingPerson entity, CancellationToken cancellationToken = default);
    }
}