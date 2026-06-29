// ===== Project(5)/Application/Features/Tenants/Queries/GetAllDistrictsQuery.cs =====
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
    public record DistrictLookupDto(Guid Id, string Name);

    public record GetAllDistrictsQuery() : IRequest<List<DistrictLookupDto>>;

    public class GetAllDistrictsQueryHandler : IRequestHandler<GetAllDistrictsQuery, List<DistrictLookupDto>>
    {
        private readonly ITenantRepository _tenantRepository;

        public GetAllDistrictsQueryHandler(ITenantRepository tenantRepository)
        {
            _tenantRepository = tenantRepository;
        }

        public async Task<List<DistrictLookupDto>> Handle(GetAllDistrictsQuery request, CancellationToken cancellationToken)
        {
            var tenants = await _tenantRepository.GetAllAsync(cancellationToken);

            return tenants
                .Where(t => t.TenantType == TenantType.District && !t.IsDeleted)
                .Select(t => new DistrictLookupDto(t.Id, t.Name))
                .OrderBy(t => t.Name)
                .ToList();
        }
    }
}