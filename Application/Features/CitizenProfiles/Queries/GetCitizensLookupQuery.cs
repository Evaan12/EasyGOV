using Application.Interfaces;
using Domain.Enums;
using Domain.Repositories;
using Mediator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.CitizenProfiles.Queries
{
    public record CitizenLookupDto(Guid Id, string FullName);

    public record GetCitizensLookupQuery() : IRequest<List<CitizenLookupDto>>;

    public class GetCitizensLookupQueryHandler : IRequestHandler<GetCitizensLookupQuery, List<CitizenLookupDto>>
    {
        private readonly ICitizenProfileRepository _repository;
        private readonly ICurrentUserService _currentUser;
        private readonly ITenantService _tenantService;

        public GetCitizensLookupQueryHandler(ICitizenProfileRepository repository, ICurrentUserService currentUser, ITenantService tenantService)
        {
            _repository = repository;
            _currentUser = currentUser;
            _tenantService = tenantService;
        }

        public async Task<List<CitizenLookupDto>> Handle(GetCitizensLookupQuery request, CancellationToken cancellationToken)
        {
            IEnumerable<Guid>? allowedWardIds = null;

            if (_currentUser.TenantType != TenantType.Central)
            {
                if (_currentUser.TenantId == Guid.Empty) return new List<CitizenLookupDto>();
                allowedWardIds = await _tenantService.GetAllowedTenantIdsAsync(_currentUser.TenantId, cancellationToken);
            }

            var (items, _) = await _repository.GetPaginatedAsync(allowedWardIds, 0, 5000, null, cancellationToken);

            return items
                .Where(c => c.Status == Domain.Enums.CitizenStatus.Active)
                .Select(c => new CitizenLookupDto(c.Id, c.FullName))
                .OrderBy(c => c.FullName)
                .ToList();
        }
    }
}