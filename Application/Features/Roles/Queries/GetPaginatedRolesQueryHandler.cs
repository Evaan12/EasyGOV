using Application.Common.Pagination;
using Application.Features.Roles.DTOs;
using Application.Interfaces;
using Mediator;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Roles.Queries
{
    public class GetPaginatedRolesQueryHandler : IRequestHandler<GetPaginatedRolesQuery, PagedResult<RoleDto>>
    {
        private readonly IRoleService _roleService;
        private readonly ICurrentUserService _currentUserService;

        public GetPaginatedRolesQueryHandler(IRoleService roleService, ICurrentUserService currentUserService)
        {
            _roleService = roleService;
            _currentUserService = currentUserService;
        }

        public async Task<PagedResult<RoleDto>> Handle(GetPaginatedRolesQuery request, CancellationToken cancellationToken)
        {
            var result = await _roleService.GetPaginatedRolesAsync(
                request.Pagination.Skip,
                request.Pagination.PageSize,
                request.Pagination.SearchTerm,
                _currentUserService.TenantType,
                _currentUserService.TenantId,
                cancellationToken);

            return new PagedResult<RoleDto>(result.Items, result.TotalCount, request.Pagination.PageNumber, request.Pagination.PageSize);
        }
    }
}