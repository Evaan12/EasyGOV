using Application.Common.Security;
using Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Infrastructure.Authorization
{
    public class PermissionPolicyProvider : DefaultAuthorizationPolicyProvider
    {
        public const string POLICY_PREFIX = "Require:";

        public PermissionPolicyProvider(IOptions<AuthorizationOptions> options) : base(options) { }

        public override async Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
        {
            if (!policyName.StartsWith(POLICY_PREFIX, StringComparison.OrdinalIgnoreCase))
                return await base.GetPolicyAsync(policyName);

            var requirementString = policyName.Substring(POLICY_PREFIX.Length);
            var andGroups = requirementString.Split(',', StringSplitOptions.RemoveEmptyEntries);

            var requirementMatrix = new List<List<(ResourceType?, ActionType?)>>();

            foreach (var andGroup in andGroups)
            {
                var orGroup = andGroup.Split('|', StringSplitOptions.RemoveEmptyEntries);
                var currentOrList = new List<(ResourceType?, ActionType?)>();

                foreach (var orItem in orGroup)
                {
                    var parts = orItem.Split('.');
                    if (parts.Length == 2)
                    {
                        ResourceType? resType = null;
                        if (!string.Equals(parts[0], "Any", StringComparison.OrdinalIgnoreCase))
                        {
                            if (Enum.TryParse<ResourceType>(parts[0], true, out var r)) resType = r;
                            else continue; // Invalid explicit resource type
                        }

                        ActionType? actType = null;
                        if (!string.Equals(parts[1], "Any", StringComparison.OrdinalIgnoreCase))
                        {
                            actType = PermissionMetaDataConfigHelper.ResolveAction(parts[1]);
                            if (actType == ActionType.None) continue; // Invalid explicit action type
                        }

                        currentOrList.Add((resType, actType));
                    }
                }
                
                if (currentOrList.Any()) 
                    requirementMatrix.Add(currentOrList);
            }

            if (!requirementMatrix.Any()) return null;

            var policy = new AuthorizationPolicyBuilder();
            policy.AddRequirements(new PermissionRequirement(requirementMatrix));
            return policy.Build();
        }
    }
}