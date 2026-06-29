using Application.Features.Tenants.DTOs;
using Domain.Repositories;
using Mediator;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Tenants.Queries
{
    public record GetTenantByIdQuery(Guid TenantId) : IRequest<TenantDto?>;

    public class GetTenantByIdQueryHandler : IRequestHandler<GetTenantByIdQuery, TenantDto?>
    {
        private readonly ITenantRepository _tenantRepository;

        public GetTenantByIdQueryHandler(ITenantRepository tenantRepository)
        {
            _tenantRepository = tenantRepository;
        }

        public async Task<TenantDto?> Handle(GetTenantByIdQuery request, CancellationToken cancellationToken)
        {
            var tenant = await _tenantRepository.GetByIdAsync(request.TenantId, cancellationToken);
            if (tenant == null || tenant.IsDeleted) return null;

            var parent = tenant.ParentId.HasValue ? await _tenantRepository.GetByIdAsync(tenant.ParentId.Value, cancellationToken) : null;

            return new TenantDto(
                tenant.Id,
                tenant.Name,
                tenant.TenantType,
                tenant.ParentId,
                parent?.Name,
                tenant.IsActivated,
                tenant.IsDefault ?? false,
                tenant.RegistrationId
            );
        }
    }
}