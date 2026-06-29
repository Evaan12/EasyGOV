using Application.Common.Pagination;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Domain.Exceptions;
using Domain.Repositories;
using Mediator;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.DevelopmentPlans.Queries
{
    public record GetPaginatedDevelopmentPlansQuery(PaginationParameters Pagination) : IRequest<PagedResult<DevelopmentPlan>>;

    public class GetPaginatedDevelopmentPlansQueryHandler : IRequestHandler<GetPaginatedDevelopmentPlansQuery, PagedResult<DevelopmentPlan>>
    {
        private readonly IDevelopmentPlanRepository _repository;
        private readonly ITenantRepository _tenantRepository;
        private readonly ICurrentUserService _currentUser;

        public GetPaginatedDevelopmentPlansQueryHandler(IDevelopmentPlanRepository repository, ITenantRepository tenantRepository, ICurrentUserService currentUser)
        {
            _repository = repository;
            _tenantRepository = tenantRepository;
            _currentUser = currentUser;
        }

        public async Task<PagedResult<DevelopmentPlan>> Handle(GetPaginatedDevelopmentPlansQuery request, CancellationToken cancellationToken)
        {
            string? ltreePath = null;

            if (_currentUser.TenantType != TenantType.Central)
            {
                if (_currentUser.TenantId == Guid.Empty) throw new DomainException("Valid tenant anchor strictly required.");
                var tenant = await _tenantRepository.GetByIdAsync(_currentUser.TenantId, cancellationToken);
                ltreePath = tenant?.LtreePath;
            }

            var result = await _repository.GetVisiblePlansAsync(
                ltreePath,
                request.Pagination.Skip,
                request.Pagination.PageSize,
                cancellationToken
            );

            return new PagedResult<DevelopmentPlan>(result.Items, result.TotalCount, request.Pagination.PageNumber, request.Pagination.PageSize);
        }
    }
}