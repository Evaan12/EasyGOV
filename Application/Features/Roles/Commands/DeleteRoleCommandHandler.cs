using Application.Interfaces;
using Domain.Exceptions;
using Domain.Enums;
using Mediator;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Roles.Commands
{
    public class DeleteRoleCommandHandler : IRequestHandler<DeleteRoleCommand, Unit>
    {
        private readonly IRoleService _roleService;
        private readonly ICurrentUserService _currentUserService;
        private readonly ITenantService _tenantService;

        public DeleteRoleCommandHandler(IRoleService roleService, ICurrentUserService currentUserService, ITenantService tenantService)
        {
            _roleService = roleService;
            _currentUserService = currentUserService;
            _tenantService = tenantService;
        }

        public async Task<Unit> Handle(DeleteRoleCommand request, CancellationToken cancellationToken)
        {
            var role = await _roleService.GetRoleByIdAsync(request.RoleId, cancellationToken);
            if (role == null) throw new DomainException("Role not found.");

            if (role.IsDefault)
                throw new DomainException("Cannot delete a default system role.");

            if (_currentUserService.TenantType != TenantType.Central)
            {
                var allowedTenantIds = await _tenantService.GetAllowedTenantIdsAsync(_currentUserService.TenantId, cancellationToken);
                if (!allowedTenantIds.Contains(role.TenantId))
                    throw new DomainException("You do not have permission to delete this role.");
            }

            await _roleService.DeleteRoleAsync(request.RoleId, cancellationToken);
            return Unit.Value;
        }
    }
}