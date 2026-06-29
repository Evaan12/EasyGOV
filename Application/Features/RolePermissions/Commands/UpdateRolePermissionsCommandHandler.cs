using Application.Common.Security;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Domain.Exceptions;
using Domain.Repositories;
using Domain.ValueObjects;
using Mediator;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.RolePermissions.Commands
{
    public class UpdateRolePermissionsCommandHandler : IRequestHandler<UpdateRolePermissionsCommand, Unit>
    {
        private readonly IRolePermissionRepository _repository;
        private readonly IRoleService _roleService;
        private readonly ICurrentUserService _currentUserService;
        private readonly IUnitOfWork _unitOfWork;

        public UpdateRolePermissionsCommandHandler(
            IRolePermissionRepository repository, 
            IRoleService roleService, 
            ICurrentUserService currentUserService,
            IUnitOfWork unitOfWork)
        {
            _repository = repository;
            _roleService = roleService;
            _currentUserService = currentUserService;
            _unitOfWork = unitOfWork;
        }

        public async Task<Unit> Handle(UpdateRolePermissionsCommand request, CancellationToken cancellationToken)
        {
            var role = await _roleService.GetRoleByIdAsync(request.RoleId, cancellationToken);
            if (role == null) throw new DomainException("Role not found.");

            if (role.IsDefault)
            {
                throw new DomainException("Cannot modify permissions for a default system role.");
            }

            if (_currentUserService.TenantType != TenantType.Central && role.TenantId != _currentUserService.TenantId)
            {
                throw new DomainException("You do not have permission to modify this role.");
            }

            var currentPermissions = await _repository.GetByRoleIdAsync(request.RoleId, cancellationToken);
            
            bool isAssigningAdmin = request.Permissions.Any(p => p.ResourceType == ResourceType.Admin && p.ActionType.HasFlag(ActionType.Admin));
            bool isRemovingAdmin = currentPermissions.Any(p => p.ResourceType == ResourceType.Admin && p.ActionType.HasFlag(ActionType.Admin)) 
                                   && !isAssigningAdmin;

            if ((isAssigningAdmin || isRemovingAdmin) && !_currentUserService.HasUnrestrictedAdmin)
            {
                throw new DomainException("Only users with explicit Admin.Admin access can modify Admin privileges.");
            }

            foreach (var perm in currentPermissions)
            {
                await _repository.DeleteAsync(perm, cancellationToken);
            }

            foreach (var newPerm in request.Permissions)
            {
                if (newPerm.ActionType == ActionType.None) continue;

                PermissionMetaDataConfigHelper.EnforceDelegationRules(newPerm.ResourceType, newPerm.ActionType, _currentUserService.HasUnrestrictedAdmin);

                if (!PermissionMetaDataConfigHelper.ValidatePermissions(newPerm.ResourceType, newPerm.ActionType))
                {
                    throw new DomainException($"Invalid action set requested for resource {newPerm.ResourceType}.");
                }

                AccessTimeWindow? timeWindow = null;
                if (newPerm.StartTime.HasValue && newPerm.EndTime.HasValue)
                {
                    timeWindow = new AccessTimeWindow(newPerm.StartTime.Value, newPerm.EndTime.Value);
                }

                var entity = new RolePermission(
                    Guid.NewGuid(), 
                    request.RoleId, 
                    newPerm.ResourceType, 
                    newPerm.ActionType, 
                    timeWindow,
                    newPerm.AllowedIpAddress,
                    _currentUserService.UserId);

                await _repository.AddAsync(entity, cancellationToken);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}