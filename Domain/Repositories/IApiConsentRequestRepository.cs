using Domain.Entities;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Domain.Repositories
{
    public interface IApiConsentRequestRepository
    {
        Task<ApiConsentRequest?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<ApiConsentRequest?> GetActivePendingRequestAsync(Guid citizenId, string thirdPartyClientId, CancellationToken cancellationToken = default);
        Task AddAsync(ApiConsentRequest entity, CancellationToken cancellationToken = default);
        Task UpdateAsync(ApiConsentRequest entity, CancellationToken cancellationToken = default);
    }
}