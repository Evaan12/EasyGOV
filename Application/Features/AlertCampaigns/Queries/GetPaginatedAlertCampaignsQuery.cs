using Application.Common.Pagination;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Domain.Repositories;
using Mediator;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.AlertCampaigns.Queries
{
    public record GetPaginatedAlertCampaignsQuery(PaginationParameters Pagination) : IRequest<PagedResult<AlertCampaign>>;

    public class GetPaginatedAlertCampaignsQueryHandler : IRequestHandler<GetPaginatedAlertCampaignsQuery, PagedResult<AlertCampaign>>
    {
        private readonly IAlertCampaignRepository _repository;
        private readonly ICurrentUserService _currentUser;
        private readonly ITenantService _tenantService;

        public GetPaginatedAlertCampaignsQueryHandler(IAlertCampaignRepository repository, ICurrentUserService currentUser, ITenantService tenantService)
        {
            _repository = repository;
            _currentUser = currentUser;
            _tenantService = tenantService;
        }

        public async Task<PagedResult<AlertCampaign>> Handle(GetPaginatedAlertCampaignsQuery request, CancellationToken cancellationToken)
        {
            System.Collections.Generic.IEnumerable<Guid>? allowedTenantIds = null;

            if (_currentUser.TenantType != TenantType.Central)
            {
                if (_currentUser.TenantId == Guid.Empty)
                    return new PagedResult<AlertCampaign>(System.Linq.Enumerable.Empty<AlertCampaign>(), 0, request.Pagination.PageNumber, request.Pagination.PageSize);
                
                allowedTenantIds = await _tenantService.GetAllowedTenantIdsAsync(_currentUser.TenantId, cancellationToken);
            }

            var result = await _repository.GetPaginatedAsync(allowedTenantIds, request.Pagination.Skip, request.Pagination.PageSize, cancellationToken);

            return new PagedResult<AlertCampaign>(result.Items, result.TotalCount, request.Pagination.PageNumber, request.Pagination.PageSize);
        }
    }
}