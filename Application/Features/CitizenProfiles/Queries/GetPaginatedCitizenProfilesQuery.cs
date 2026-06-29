using Application.Common.Pagination;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Domain.Exceptions;
using Domain.Repositories;
using Mediator;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.CitizenProfiles.Queries
{
    public record GetPaginatedCitizenProfilesQuery(PaginationParameters Pagination) : IRequest<PagedResult<CitizenProfile>>;

    public class GetPaginatedCitizenProfilesQueryHandler : IRequestHandler<GetPaginatedCitizenProfilesQuery, PagedResult<CitizenProfile>>
    {
        private readonly ICitizenProfileRepository _repository;
        private readonly ICurrentUserService _currentUser;
        private readonly ITenantService _tenantService;

        public GetPaginatedCitizenProfilesQueryHandler(ICitizenProfileRepository repository, ICurrentUserService currentUser, ITenantService tenantService)
        {
            _repository = repository;
            _currentUser = currentUser;
            _tenantService = tenantService;
        }

        public async Task<PagedResult<CitizenProfile>> Handle(GetPaginatedCitizenProfilesQuery request, CancellationToken cancellationToken)
        {
            System.Collections.Generic.IEnumerable<Guid>? allowedWardIds = null;

            if (_currentUser.TenantType != TenantType.Central)
            {
                if (_currentUser.TenantId == Guid.Empty)
                    throw new DomainException("Valid tenant required for profile access.");
                
                allowedWardIds = await _tenantService.GetAllowedTenantIdsAsync(_currentUser.TenantId, cancellationToken);
            }

            var result = await _repository.GetPaginatedAsync(allowedWardIds, request.Pagination.Skip, request.Pagination.PageSize, request.Pagination.SearchTerm, cancellationToken);

            return new PagedResult<CitizenProfile>(result.Items, result.TotalCount, request.Pagination.PageNumber, request.Pagination.PageSize);
        }
    }
}