using Domain.Enums;
using Mediator;
using System;
using System.Collections.Generic;

namespace Application.Features.RolePermissions.Commands
{
    public record PermissionUpdateDto(
        ResourceType ResourceType, 
        ActionType ActionType, 
        TimeSpan? StartTime, 
        TimeSpan? EndTime, 
        string? AllowedIpAddress);

    public record UpdateRolePermissionsCommand(Guid RoleId, List<PermissionUpdateDto> Permissions) : IRequest<Unit>;
}