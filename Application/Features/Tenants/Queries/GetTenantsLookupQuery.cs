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
    public record TenantLookupDto(Guid Id, string Name, TenantType TenantType, Guid? ProvinceId, Guid? DistrictId, Guid? MunicipalityId);

    public record GetTenantsLookupQuery(TenantType? TypeFilter = null) : IRequest<List<TenantLookupDto>>;

    public class GetTenantsLookupQueryHandler : IRequestHandler<GetTenantsLookupQuery, List<TenantLookupDto>>
    {
        private readonly ITenantRepository _tenantRepository;
        private readonly ICurrentUserService _currentUser;
        private readonly ITenantService _tenantService;

        public GetTenantsLookupQueryHandler(ITenantRepository tenantRepository, ICurrentUserService currentUser, ITenantService tenantService)
        {
            _tenantRepository = tenantRepository;
            _currentUser = currentUser;
            _tenantService = tenantService;
        }

        public async Task<List<TenantLookupDto>> Handle(GetTenantsLookupQuery request, CancellationToken cancellationToken)
        {
            IEnumerable<Guid>? allowedIds = null;

            if (_currentUser.UserId != Guid.Empty && _currentUser.TenantType != TenantType.Central)
            {
                allowedIds = await _tenantService.GetAllowedTenantIdsAsync(_currentUser.TenantId, cancellationToken);
            }

            var (items, _) = await _tenantRepository.GetPaginatedAsync(0, 5000, null, null, null, null, request.TypeFilter, null, allowedIds, cancellationToken);
            
            var allTenants = await _tenantRepository.GetAllAsync(cancellationToken);

            return items.Select(t => {
                string fullName = t.Name;
                
                // Construct formatted hierarchical name contextualization for Wards and Municipalities
                if (t.TenantType == TenantType.Ward)
                {
                    var muni = allTenants.FirstOrDefault(p => p.Id == t.MunicipalityId);
                    var dist = allTenants.FirstOrDefault(p => p.Id == t.DistrictId);
                    if (muni != null && dist != null)
                        fullName = $"{t.Name}, {muni.Name}, {dist.Name}";
                    else if (muni != null)
                        fullName = $"{t.Name}, {muni.Name}";
                }
                
                return new TenantLookupDto(t.Id, fullName, t.TenantType, t.ProvinceId, t.DistrictId, t.MunicipalityId);
            }).OrderBy(t => t.Name).ToList();
        }
    }
}