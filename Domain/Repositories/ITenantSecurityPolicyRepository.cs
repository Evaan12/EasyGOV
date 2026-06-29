using Domain.Entities;
using System.Threading;
using System.Threading.Tasks;

namespace Domain.Repositories
{
    public interface ITenantSecurityPolicyRepository
    {
        Task<TenantSecurityPolicy?> GetGlobalPolicyAsync(CancellationToken cancellationToken = default);
        Task AddAsync(TenantSecurityPolicy entity, CancellationToken cancellationToken = default);
        Task UpdateAsync(TenantSecurityPolicy entity, CancellationToken cancellationToken = default);
    }
}