using Application.Common.Pagination;
using Application.Features.Users.DTOs;
using Application.Interfaces;
using Mediator;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Users.Queries
{
    public class SearchUsersQueryHandler : IRequestHandler<SearchUsersQuery, PagedResult<UserSearchDto>>
    {
        private readonly IUserService _userService;

        public SearchUsersQueryHandler(IUserService userService)
        {
            _userService = userService;
        }

        public async Task<PagedResult<UserSearchDto>> Handle(SearchUsersQuery request, CancellationToken cancellationToken)
        {
            return await _userService.SearchUsersAsync(request.SearchTerm, request.PageNumber, request.PageSize, cancellationToken);
        }
    }
}