using Application.Interfaces;
using Domain.Exceptions;
using Domain.Enums;
using Domain.Repositories;
using Mediator;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Tenants.Commands
{
    public record ToggleTenantStatusCommand(Guid TenantId, bool Activate) : IRequest<Unit>;

    public class ToggleTenantStatusCommandHandler : IRequestHandler<ToggleTenantStatusCommand, Unit>
    {
        private readonly ITenantRepository _tenantRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly ITenantService _tenantService;
        private readonly IUnitOfWork _unitOfWork;

        public ToggleTenantStatusCommandHandler(
            ITenantRepository tenantRepository, 
            ICurrentUserService currentUserService, 
            ITenantService tenantService,
            IUnitOfWork unitOfWork)
        {
            _tenantRepository = tenantRepository;
            _currentUserService = currentUserService;
            _tenantService = tenantService;
            _unitOfWork = unitOfWork;
        }

        public async Task<Unit> Handle(ToggleTenantStatusCommand request, CancellationToken cancellationToken)
        {
            var tenant = await _tenantRepository.GetByIdAsync(request.TenantId, cancellationToken);
            if (tenant == null) throw new DomainException("Tenant not found.");

            if (_currentUserService.TenantType != TenantType.Central)
            {
                var allowedTenants = await _tenantService.GetAllowedTenantIdsAsync(_currentUserService.TenantId, cancellationToken);
                if (!allowedTenants.Contains(request.TenantId))
                    throw new DomainException("You do not have permission to modify this tenant.");
            }

            if (request.Activate) tenant.Activate(_currentUserService.UserId);
            else tenant.Deactivate(_currentUserService.UserId);

            await _tenantRepository.UpdateAsync(tenant, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}