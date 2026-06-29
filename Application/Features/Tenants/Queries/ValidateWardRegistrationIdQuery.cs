using Domain.Enums;
using Domain.Repositories;
using Mediator;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Tenants.Queries
{
    public record ValidateWardRegistrationIdQuery(string RegistrationId) : IRequest<Guid?>;

    public class ValidateWardRegistrationIdQueryHandler : IRequestHandler<ValidateWardRegistrationIdQuery, Guid?>
    {
        private readonly ITenantRepository _tenantRepository;

        public ValidateWardRegistrationIdQueryHandler(ITenantRepository tenantRepository)
        {
            _tenantRepository = tenantRepository;
        }

        public async Task<Guid?> Handle(ValidateWardRegistrationIdQuery request, CancellationToken cancellationToken)
        {
            var tenant = await _tenantRepository.GetByRegistrationIdAsync(request.RegistrationId, cancellationToken);
            if (tenant == null || !tenant.IsActivated || tenant.TenantType != TenantType.Ward) return null;

            return tenant.Id;
        }
    }
}