using Application.Common.Security;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Domain.Exceptions;
using Domain.Repositories;
using Domain.ValueObjects;
using Mediator;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.RolePermissions.Commands
{
    public record CreateRolePermissionCommand(
        Guid RoleId, 
        ResourceType ResourceType, 
        ActionType ActionType, 
        TimeSpan? StartTime, 
        TimeSpan? EndTime, 
        string? AllowedIpAddress, 
        Guid UserId) : IRequest<Guid>;

    public class CreateRolePermissionCommandHandler : IRequestHandler<CreateRolePermissionCommand, Guid>
    {
        private readonly IRolePermissionRepository _repository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public CreateRolePermissionCommandHandler(IRolePermissionRepository repository, IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<Guid> Handle(CreateRolePermissionCommand request, CancellationToken cancellationToken)
        {
            PermissionMetaDataConfigHelper.EnforceDelegationRules(request.ResourceType, request.ActionType, _currentUserService.HasUnrestrictedAdmin);

            if (!PermissionMetaDataConfigHelper.ValidatePermissions(request.ResourceType, request.ActionType))
                throw new DomainException($"The requested actions are invalid for resource {request.ResourceType}");

            AccessTimeWindow? timeWindow = null;
            if (request.StartTime.HasValue && request.EndTime.HasValue)
            {
                timeWindow = new AccessTimeWindow(request.StartTime.Value, request.EndTime.Value);
            }

            var entity = new RolePermission(
                Guid.NewGuid(), 
                request.RoleId, 
                request.ResourceType, 
                request.ActionType, 
                timeWindow, 
                request.AllowedIpAddress, 
                request.UserId);
            
            await _repository.AddAsync(entity, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            
            return entity.Id;
        }
    }
}