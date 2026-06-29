using Application.Common.Pagination;
using Application.Features.Users.DTOs;
using Mediator;

namespace Application.Features.Users.Queries
{
    public record SearchUsersQuery(string SearchTerm, int PageNumber, int PageSize) : IRequest<PagedResult<UserSearchDto>>;
}