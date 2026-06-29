using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Domain.Repositories
{
    public interface IRolePermissionRepository
    {
        Task<RolePermission?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<IEnumerable<RolePermission>> GetByRoleIdAsync(Guid roleId, CancellationToken cancellationToken = default);
        Task AddAsync(RolePermission entity, CancellationToken cancellationToken = default);
        Task UpdateAsync(RolePermission entity, CancellationToken cancellationToken = default);
        Task DeleteAsync(RolePermission entity, CancellationToken cancellationToken = default);
    }
}