using Application.Common.Pagination;
using Application.Features.Roles.DTOs;
using Mediator;

namespace Application.Features.Roles.Queries
{
    public record GetPaginatedRolesQuery(PaginationParameters Pagination) : IRequest<PagedResult<RoleDto>>;
}