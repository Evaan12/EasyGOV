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
    public record CitizenSearchDto(Guid Id, string FullName, string CitizenshipNumber);

    public record SearchCitizensQuery(string Term) : IRequest<List<CitizenSearchDto>>;

    public class SearchCitizensQueryHandler : IRequestHandler<SearchCitizensQuery, List<CitizenSearchDto>>
    {
        private readonly ICitizenProfileRepository _repository;
        private readonly ICurrentUserService _currentUser;
        private readonly ITenantService _tenantService;

        public SearchCitizensQueryHandler(ICitizenProfileRepository repository, ICurrentUserService currentUser, ITenantService tenantService)
        {
            _repository = repository;
            _currentUser = currentUser;
            _tenantService = tenantService;
        }

        public async Task<List<CitizenSearchDto>> Handle(SearchCitizensQuery request, CancellationToken cancellationToken)
        {
            IEnumerable<Guid>? allowedWardIds = null;

            if (_currentUser.TenantType != TenantType.Central)
            {
                if (_currentUser.TenantId == Guid.Empty) return new List<CitizenSearchDto>();
                allowedWardIds = await _tenantService.GetAllowedTenantIdsAsync(_currentUser.TenantId, cancellationToken);
            }

            var (items, _) = await _repository.GetPaginatedAsync(allowedWardIds, 0, 50, request.Term, cancellationToken);

            return items
                .Where(c => c.Status == CitizenStatus.Active && c.FaceEmbedding != null)
                .Select(c => new CitizenSearchDto(c.Id, c.FullName, c.Citizenship?.CitizenshipNumber ?? c.NationalId?.NinNumber ?? ""))
                .ToList();
        }
    }
}