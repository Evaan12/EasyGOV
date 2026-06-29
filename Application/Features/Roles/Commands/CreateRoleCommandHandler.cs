using Application.Interfaces;
using Domain.Enums;
using Mediator;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Roles.Commands
{
    public class CreateRoleCommandHandler : IRequestHandler<CreateRoleCommand, Guid>
    {
        private readonly IRoleService _roleService;
        private readonly ICurrentUserService _currentUserService;

        public CreateRoleCommandHandler(IRoleService roleService, ICurrentUserService currentUserService)
        {
            _roleService = roleService;
            _currentUserService = currentUserService;
        }

        public async Task<Guid> Handle(CreateRoleCommand request, CancellationToken cancellationToken)
        {
            TenantType effectiveType = request.TenantType;
            Guid effectiveId = request.TenantId;

            if (_currentUserService.TenantType != TenantType.Central)
            {
                effectiveType = _currentUserService.TenantType;
                effectiveId = _currentUserService.TenantId;
            }

            return await _roleService.CreateRoleAsync(request.Name, effectiveType, effectiveId, cancellationToken);
        }
    }
}