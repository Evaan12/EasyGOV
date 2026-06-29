using Application.Common.Pagination;
using Application.Features.Users.DTOs;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IUserService
    {
        Task<PagedResult<UserSearchDto>> SearchUsersAsync(string searchTerm, int pageNumber, int pageSize, CancellationToken cancellationToken = default);
        Task SuspendUserAsync(Guid userId, TimeSpan duration, CancellationToken cancellationToken = default);
        Task BanUserAsync(Guid userId, string reason, CancellationToken cancellationToken = default);
        Task AssignRoleToUserAsync(Guid userId, Guid roleId, CancellationToken cancellationToken = default);

        Task<Guid> CreateTenantAdminAsync(string email, string fullName, string password, Guid tenantId, Guid roleId, CancellationToken cancellationToken = default);
        Task<Guid> RegisterPublicCitizenAsync(string email, string fullName, string password, string registrationId, string phoneNumber, CancellationToken cancellationToken = default);

        Task<(string FullName, string PrimaryRole)> GetUserDetailsAsync(Guid userId, CancellationToken cancellationToken = default);
    }
}