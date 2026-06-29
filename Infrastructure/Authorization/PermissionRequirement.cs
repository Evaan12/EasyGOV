using Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;

namespace Infrastructure.Authorization
{
    public class PermissionRequirement : IAuthorizationRequirement
    {
        // Outer List evaluates as AND, Inner List evaluates as OR
        // Null values indicate "Any" wildcard (Any Resource or Any Action)
        public List<List<(ResourceType? Resource, ActionType? Action)>> RequiredPermissions { get; }

        public PermissionRequirement(List<List<(ResourceType?, ActionType?)>> requiredPermissions)
        {
            RequiredPermissions = requiredPermissions;
        }
    }
}