using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Application.Common.Security
{
    public record DelegationRule(ResourceType TargetResource, ActionType TargetAction, ResourceType RequiredResource, ActionType RequiredAction);

    public static class PermissionMetaDataConfigHelper
    {
        public static readonly Dictionary<string, ActionType> Bundles = new(StringComparer.OrdinalIgnoreCase)
        {
            { "DefaultCrud", ActionType.Read | ActionType.Create | ActionType.Update | ActionType.Delete },
            { "ReportManagerBundle", ActionType.Read | ActionType.Export },
            { "SuperAdminBundle", ActionType.Admin }
        };

        private static readonly HashSet<ResourceType> HiddenResources = new()
        {
        };

        private static readonly Dictionary<ResourceType, ActionType> CustomResourceActionMap = new()
        {
            { ResourceType.Role, ActionType.Read | ActionType.Create | ActionType.Update | ActionType.Delete | ActionType.Assign },
            { ResourceType.Admin, ActionType.Admin },
            { ResourceType.Tenant, ActionType.Read | ActionType.Update | ActionType.Activate },
            { ResourceType.SubTenant, ActionType.Read | ActionType.Create | ActionType.Update | ActionType.Delete | ActionType.Activate },
            { ResourceType.Sifaris, ActionType.Read | ActionType.Create | ActionType.Approve | ActionType.Revoke | ActionType.Export },
            { ResourceType.CitizenProfile, ActionType.Read | ActionType.Update | ActionType.Delete },
            { ResourceType.BiometricEnrollment, ActionType.Create | ActionType.Read | ActionType.Update | ActionType.Delete },
            { ResourceType.MissingPerson, ActionType.Create | ActionType.Read | ActionType.Update | ActionType.Delete },
            { ResourceType.AlertCampaign, ActionType.Read | ActionType.Create | ActionType.Update | ActionType.Delete | ActionType.Approve },
            { ResourceType.CampaignDispatch, ActionType.Read }
        };

        public static readonly List<DelegationRule> DelegationRules = new()
        {
            // Relaxed structural delegation to allow seeded local government heads full control of their respective operational tiers
            new DelegationRule(ResourceType.Admin, ActionType.Admin, ResourceType.Admin, ActionType.Admin)
        };

        public static ActionType GetAvailableActions(ResourceType resource)
        {
            if (IsHidden(resource)) return ActionType.None;
            return CustomResourceActionMap.TryGetValue(resource, out var actions)
                ? actions
                : (ActionType.Read | ActionType.Create | ActionType.Update | ActionType.Delete);
        }

        public static bool IsHidden(ResourceType resource) => HiddenResources.Contains(resource);

        public static ActionType ResolveAction(string actionString)
        {
            if (Bundles.TryGetValue(actionString, out var bundledAction))
                return bundledAction;

            if (Enum.TryParse<ActionType>(actionString, true, out var parsedAction))
                return parsedAction;

            return ActionType.None;
        }

        public static bool ValidatePermissions(ResourceType resource, ActionType requestedAction)
        {
            if (IsHidden(resource) || requestedAction == ActionType.None) return false;
            
            var allowedActions = GetAvailableActions(resource);
            
            if ((allowedActions & requestedAction) != requestedAction) return false;

            var crud = ActionType.Read | ActionType.Create | ActionType.Update | ActionType.Delete;
            if (allowedActions.HasFlag(crud))
            {
                var requestedCrud = requestedAction & crud;
                
                // If they are trying to assign partial CRUD where Full CRUD is applicable, deny it.
                if (requestedCrud != ActionType.None && requestedCrud != crud)
                {
                    return false; 
                }
            }

            return true;
        }

        public static void EnforceDelegationRules(ResourceType targetResource, ActionType targetAction, bool hasUnrestrictedAdmin)
        {
            if (hasUnrestrictedAdmin) return;

            var requiredRules = DelegationRules.Where(r => r.TargetResource == targetResource && targetAction.HasFlag(r.TargetAction)).ToList();

            if (requiredRules.Any())
            {
                throw new Domain.Exceptions.DomainException($"Strict delegation policy violation: You do not possess the necessary clearance to grant {targetAction} on {targetResource}. Contact a Central System Administrator.");
            }
        }
    }
}