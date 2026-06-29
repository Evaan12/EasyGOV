using System.Collections.Generic;

namespace Domain.Constants
{
    public static class AppConstants
    {
        public const string CacheKeyPrefix = "ndas_sys_";
        public const int DefaultPageSize = 10;
        
        public const int MaxFileUploadSizeBytes = 5 * 1024 * 1024; // 5MB limit globally enforced
        public const int MaxJsonPayloadSizeBytes = 20480; // 20KB limit for JSON snapshots
        
        public const string PolicyPrefix = "Require:";
        public const string SuperAdminRoleName = "Super Admin";
        
        public static readonly string[] AllowedFileExtensions = { ".jpeg", ".png", ".jpg", ".webp", ".pdf", ".docx", ".doc", ".xls", ".xlsx" };
        public static readonly string[] ImageExtensions = { ".jpeg", ".png", ".jpg", ".webp" };
    }
}