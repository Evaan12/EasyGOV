using Application.Common.Caching;
using Domain.Enums;
using Domain.Repositories;
using Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Infrastructure.Identity
{
    public class CustomClaimsPrincipalFactory : UserClaimsPrincipalFactory<AppUser, AppRole>
    {
        private readonly IRolePermissionRepository _rolePermissionRepository;
        private readonly ICacheService _cacheService;

        public CustomClaimsPrincipalFactory(
            UserManager<AppUser> userManager,
            RoleManager<AppRole> roleManager,
            IOptions<IdentityOptions> optionsAccessor,
            IRolePermissionRepository rolePermissionRepository,
            ICacheService cacheService)
            : base(userManager, roleManager, optionsAccessor)
        {
            _rolePermissionRepository = rolePermissionRepository;
            _cacheService = cacheService;
        }

        protected override async Task<ClaimsIdentity> GenerateClaimsAsync(AppUser user)
        {
            var identity = await base.GenerateClaimsAsync(user);

            // Strict Non-Nullable Multi-Tenancy Anchor
            identity.AddClaim(new Claim("TenantId", user.TenantId.ToString()));
            identity.AddClaim(new Claim("TenantType", user.TenantType.ToString()));

            var userRoles = await UserManager.GetRolesAsync(user);
            bool isUnrestricted = false;

            foreach (var roleName in userRoles)
            {
                var role = await _cacheService.GetOrSetAsync(CacheKeys.RoleByName(roleName), async (ct) => {
                    return await RoleManager.FindByNameAsync(roleName);
                }, TimeSpan.FromHours(1), null);

                if (role != null)
                {
                    var rolePerms = await _rolePermissionRepository.GetByRoleIdAsync(role.Id);
                    
                    if (rolePerms.Any(rp => rp.ResourceType == ResourceType.Admin && rp.ActionType.HasFlag(ActionType.Admin)))
                    {
                        isUnrestricted = true;
                        break;
                    }
                }
            }

            if (isUnrestricted)
            {
                identity.AddClaim(new Claim("UnrestrictedAdmin", "true"));
            }

            return identity;
        }
    }
}