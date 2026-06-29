using Application.Common.Pagination;
using Domain.Entities;
using Domain.Enums;
using Domain.Repositories;
using Application.Interfaces;
using Mediator;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Gunasos.Queries
{
    public record GetPaginatedGunasosQuery(PaginationParameters Pagination, bool IsCitizenView) : IRequest<PagedResult<Gunaso>>;

    public class GetPaginatedGunasosQueryHandler : IRequestHandler<GetPaginatedGunasosQuery, PagedResult<Gunaso>>
    {
        private readonly IGunasoRepository _gunasoRepository;
        private readonly ITenantRepository _tenantRepository;
        private readonly ICurrentUserService _currentUser;
        private readonly ICitizenProfileRepository _profileRepository;

        public GetPaginatedGunasosQueryHandler(IGunasoRepository gunasoRepository, ITenantRepository tenantRepository, ICurrentUserService currentUser, ICitizenProfileRepository profileRepository)
        {
            _gunasoRepository = gunasoRepository;
            _tenantRepository = tenantRepository;
            _currentUser = currentUser;
            _profileRepository = profileRepository;
        }

        public async Task<PagedResult<Gunaso>> Handle(GetPaginatedGunasosQuery request, CancellationToken cancellationToken)
        {
            string? ltreePath = null;
            string? citizenWardLtreePath = null;
            Guid? citizenId = request.IsCitizenView ? _currentUser.UserId : null;

            if (request.IsCitizenView)
            {
                var profile = await _profileRepository.GetByIdAsync(_currentUser.UserId, cancellationToken);
                if (profile != null)
                {
                    var ward = await _tenantRepository.GetByIdAsync(profile.RegisteredWardId, cancellationToken);
                    citizenWardLtreePath = ward?.LtreePath;
                }
            }
            else if (_currentUser.UserId != Guid.Empty && _currentUser.TenantType != TenantType.Central)
            {
                if (_currentUser.TenantId != Guid.Empty)
                {
                    var currentTenant = await _tenantRepository.GetByIdAsync(_currentUser.TenantId, cancellationToken);
                    ltreePath = currentTenant?.LtreePath;
                }
            }

            var dbResult = await _gunasoRepository.GetPaginatedAsync(
                ltreePath,
                request.IsCitizenView,
                citizenId,
                citizenWardLtreePath,
                request.Pagination.Skip,
                request.Pagination.PageSize,
                cancellationToken
            );

            return new PagedResult<Gunaso>(dbResult.Items, dbResult.TotalCount, request.Pagination.PageNumber, request.Pagination.PageSize);
        }
    }
}