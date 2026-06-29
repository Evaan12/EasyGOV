using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface ITenantService
    {
        Task<IEnumerable<Guid>> GetAllowedTenantIdsAsync(Guid currentTenantId, CancellationToken cancellationToken = default);
    }
}