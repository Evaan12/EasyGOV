using Application.Common.Caching;
using Domain.Entities;
using Domain.Enums;
using Domain.Repositories;
using Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Infrastructure.Authorization
{
    public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
    {
        private readonly ICacheService _cacheService;
        private readonly IRolePermissionRepository _rolePermissionRepository;
        private readonly ITenantSecurityPolicyRepository _tenantSecurityPolicyRepository;
        private readonly RoleManager<AppRole> _roleManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly TimeProvider _timeProvider;

        public PermissionAuthorizationHandler(
            ICacheService cacheService, 
            IRolePermissionRepository rolePermissionRepository, 
            ITenantSecurityPolicyRepository tenantSecurityPolicyRepository, 
            RoleManager<AppRole> roleManager, 
            IHttpContextAccessor httpContextAccessor,
            TimeProvider timeProvider)
        {
            _cacheService = cacheService;
            _rolePermissionRepository = rolePermissionRepository;
            _tenantSecurityPolicyRepository = tenantSecurityPolicyRepository;
            _roleManager = roleManager;
            _httpContextAccessor = httpContextAccessor;
            _timeProvider = timeProvider;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
        {
            if (context.User?.Identity?.IsAuthenticated != true) return;

            var userRoles = context.User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
            if (!userRoles.Any()) return;

            var securityPolicy = await _tenantSecurityPolicyRepository.GetGlobalPolicyAsync();

            var currentTime = _timeProvider.GetLocalNow().TimeOfDay;
            var userIp = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();

            if (securityPolicy != null)
            {
                if (securityPolicy.TimeWindow != null && !securityPolicy.TimeWindow.IsWithinWindow(currentTime))
                    return;

                if (!string.IsNullOrEmpty(securityPolicy.AllowedIpAddress) && securityPolicy.AllowedIpAddress != userIp)
                    return;
            }

            var userTenantIdClaim = context.User.FindFirst("TenantId")?.Value;
            Guid? userTenantId = Guid.TryParse(userTenantIdClaim, out var parsedUserTenantId) ? parsedUserTenantId : null;

            var allUserPermissions = new List<RolePermission>();
            foreach (var roleName in userRoles)
            {
                var role = await _cacheService.GetOrSetAsync(CacheKeys.RoleByName(roleName), async (ct) => {
                    return await _roleManager.FindByNameAsync(roleName);
                }, TimeSpan.FromHours(1), null);

                if (role != null)
                {
                    var perms = await _rolePermissionRepository.GetByRoleIdAsync(role.Id);

                    foreach (var p in perms)
                    {
                        // Constraints apply intrinsically to the permission record regardless of structural origin.
                        // Inherited/Global roles applied from Central Tenant must still adhere to localized constraints.
                        if (p.TimeWindow != null && !p.TimeWindow.IsWithinWindow(currentTime))
                            continue; 

                        if (!string.IsNullOrEmpty(p.AllowedIpAddress) && p.AllowedIpAddress != userIp)
                            continue; 

                        allUserPermissions.Add(p);
                    }
                }
            }

            if (allUserPermissions.Any(p => p.ResourceType == ResourceType.Admin && p.ActionType.HasFlag(ActionType.Admin)))
            {
                context.Succeed(requirement);
                return;
            }

            bool isSatisfied = true;
            foreach (var orConditionList in requirement.RequiredPermissions)
            {
                bool orConditionMet = false;

                foreach (var req in orConditionList)
                {
                    var matchingPermissions = allUserPermissions.Where(p => req.Resource == null || p.ResourceType == req.Resource.Value);

                    if (req.Action == null)
                    {
                        if (matchingPermissions.Any(p => p.ActionType != ActionType.None))
                        {
                            orConditionMet = true;
                            break; 
                        }
                    }
                    else
                    {
                        if (matchingPermissions.Any(p => p.ActionType.HasFlag(req.Action.Value)))
                        {
                            orConditionMet = true;
                            break; 
                        }
                    }
                }

                if (!orConditionMet)
                {
                    isSatisfied = false; 
                    break; 
                }
            }

            if (isSatisfied)
            {
                context.Succeed(requirement);
            }
        }
    }
}