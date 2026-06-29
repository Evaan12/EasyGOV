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
    public record ActivateAndProvisionTenantCommand(
        Guid TenantId, 
        string HeadFullName, 
        string HeadEmail, 
        string HeadPassword, 
        Guid HeadRoleId) : IRequest<Unit>;

    public class ActivateAndProvisionTenantCommandHandler : IRequestHandler<ActivateAndProvisionTenantCommand, Unit>
    {
        private readonly ITenantRepository _tenantRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly IUserService _userService;
        private readonly ITenantService _tenantService;
        private readonly IUnitOfWork _unitOfWork;

        public ActivateAndProvisionTenantCommandHandler(
            ITenantRepository tenantRepository, 
            ICurrentUserService currentUserService, 
            IUserService userService,
            ITenantService tenantService,
            IUnitOfWork unitOfWork)
        {
            _tenantRepository = tenantRepository;
            _currentUserService = currentUserService;
            _userService = userService;
            _tenantService = tenantService;
            _unitOfWork = unitOfWork;
        }

        public async Task<Unit> Handle(ActivateAndProvisionTenantCommand request, CancellationToken cancellationToken)
        {
            var tenant = await _tenantRepository.GetByIdAsync(request.TenantId, cancellationToken);
            if (tenant == null) throw new DomainException("Tenant not found.");

            if (_currentUserService.TenantType != TenantType.Central)
            {
                var allowedTenants = await _tenantService.GetAllowedTenantIdsAsync(_currentUserService.TenantId, cancellationToken);
                if (!allowedTenants.Contains(request.TenantId))
                    throw new DomainException("You do not possess the clearance to activate this tenant.");
            }

            var userId = await _userService.CreateTenantAdminAsync(
                request.HeadEmail, 
                request.HeadFullName, 
                request.HeadPassword, 
                request.TenantId, 
                request.HeadRoleId, 
                cancellationToken);

            var trackedTenant = await _tenantRepository.GetByIdAsync(request.TenantId, cancellationToken);
            if (trackedTenant == null) throw new DomainException("Tenant synchronization failed during activation.");

            trackedTenant.Activate(_currentUserService.UserId);

            await _tenantRepository.UpdateAsync(trackedTenant, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}