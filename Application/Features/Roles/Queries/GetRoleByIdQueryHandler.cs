using Application.Features.Roles.DTOs;
using Application.Interfaces;
using Domain.Enums;
using Mediator;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Roles.Queries
{
    public class GetRoleByIdQueryHandler : IRequestHandler<GetRoleByIdQuery, RoleDto?>
    {
        private readonly IRoleService _roleService;
        private readonly ICurrentUserService _currentUserService;
        private readonly ITenantService _tenantService;

        public GetRoleByIdQueryHandler(IRoleService roleService, ICurrentUserService currentUserService, ITenantService tenantService)
        {
            _roleService = roleService;
            _currentUserService = currentUserService;
            _tenantService = tenantService;
        }

        public async Task<RoleDto?> Handle(GetRoleByIdQuery request, CancellationToken cancellationToken)
        {
            var role = await _roleService.GetRoleByIdAsync(request.RoleId, cancellationToken);
            if (role == null) return null;

            if (_currentUserService.TenantType != TenantType.Central)
            {
                var allowedTenantIds = await _tenantService.GetAllowedTenantIdsAsync(_currentUserService.TenantId, cancellationToken);
                if (!allowedTenantIds.Contains(role.TenantId))
                {
                    return null; 
                }
            }

            return role;
        }
    }
}