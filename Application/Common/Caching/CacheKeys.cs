using Domain.Constants;
using System;

namespace Application.Common.Caching
{
    public static class CacheKeys
    {
        public const string Prefix = AppConstants.CacheKeyPrefix;

        public const string OutboxPending = $"{Prefix}outbox_pending";
        public const string GlobalSecurityPolicy = $"{Prefix}global_sec_policy";
        public const string AllTenants = $"{Prefix}all_tenants";

        public static string CitizenProfile(Guid id) => $"{Prefix}citizen_{id}";
        public static string CitizenByCitizenship(string num) => $"{Prefix}citizen_cit_{num}";
        
        public static string RoleById(Guid id) => $"{Prefix}role_{id}";
        public static string RoleByName(string name) => $"{Prefix}role_name_{name.ToLowerInvariant()}";
        public static string RolePermissions(Guid roleId) => $"{Prefix}role_perms_{roleId}";
        
        public static string TenantDescendants(Guid tenantId) => $"{Prefix}tenant_desc_{tenantId}";
        
        public static string SifarisById(Guid id) => $"{Prefix}sifaris_{id}";
        public static string SifarisByCitizen(Guid citizenId) => $"{Prefix}sifaris_cit_{citizenId}";

        public static class Tags
        {
            public const string RolePermissions = "RolePermissions_Tag";
            public const string CitizenProfiles = "CitizenProfiles_Tag";
            public const string Tenants = "Tenants_Tag";
            public const string Sifaris = "Sifaris_Tag";
            public const string DocTemplates = "DocTemplates_Tag";
            public const string MissingPersons = "MissingPersons_Tag";
        }
    }
}