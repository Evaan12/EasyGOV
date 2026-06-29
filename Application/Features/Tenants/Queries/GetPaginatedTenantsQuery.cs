using Application.Common.Pagination;
using Application.Features.Tenants.DTOs;
using Application.Interfaces;
using Domain.Enums;
using Domain.Repositories;
using Mediator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Tenants.Queries
{
    public record GetPaginatedTenantsQuery(PaginationParameters Pagination, Guid? ProvinceId, Guid? DistrictId, Guid? MunicipalityId, TenantType? TenantType, bool? IsActivated) : IRequest<PagedResult<TenantDto>>;

    public class GetPaginatedTenantsQueryHandler : IRequestHandler<GetPaginatedTenantsQuery, PagedResult<TenantDto>>
    {
        private readonly ITenantRepository _tenantRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly ITenantService _tenantService;

        public GetPaginatedTenantsQueryHandler(ITenantRepository tenantRepository, ICurrentUserService currentUserService, ITenantService tenantService)
        {
            _tenantRepository = tenantRepository;
            _currentUserService = currentUserService;
            _tenantService = tenantService;
        }

        public async Task<PagedResult<TenantDto>> Handle(GetPaginatedTenantsQuery request, CancellationToken cancellationToken)
        {
            IEnumerable<Guid>? allowedIds = null;

            if (_currentUserService.TenantType != TenantType.Central)
            {
                allowedIds = await _tenantService.GetAllowedTenantIdsAsync(_currentUserService.TenantId, cancellationToken);
            }

            var dbResult = await _tenantRepository.GetPaginatedAsync(request.Pagination.Skip, request.Pagination.PageSize, request.Pagination.SearchTerm, request.ProvinceId, request.DistrictId, request.MunicipalityId, request.TenantType, request.IsActivated, allowedIds, cancellationToken);

            var allTenants = await _tenantRepository.GetAllAsync(cancellationToken); 

            var items = dbResult.Items.Select(t => new TenantDto(
                    t.Id, 
                    t.Name, 
                    t.TenantType, 
                    t.ParentId, 
                    allTenants.FirstOrDefault(p => p.Id == t.ParentId)?.Name,
                    t.IsActivated,
                    t.IsDefault ?? false,
                    t.RegistrationId
                )).ToList();

            return new PagedResult<TenantDto>(items, dbResult.TotalCount, request.Pagination.PageNumber, request.Pagination.PageSize);
        }
    }
}