using Domain.Enums;
using Mediator;
using System;
using System.Collections.Generic;

namespace Application.Features.RolePermissions.Queries
{
    public record RolePermissionDto(
        Guid Id, 
        ResourceType ResourceType, 
        ActionType ActionType, 
        TimeSpan? StartTime, 
        TimeSpan? EndTime, 
        string? AllowedIpAddress);

    public record GetRolePermissionsQuery(Guid RoleId) : IRequest<List<RolePermissionDto>>;
}