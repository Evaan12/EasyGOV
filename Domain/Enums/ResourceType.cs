namespace Domain.Enums
{
    public enum ResourceType : int
    {
        None = 0,
        User = 2,
        Role = 3,
        Settings = 5,
        Admin = 6,
        Tenant = 7,
        SubTenant = 8,
        Sifaris = 9,
        CitizenProfile = 10,
        BiometricEnrollment = 11,
        MissingPerson = 12,
        AlertCampaign = 13,
        CampaignDispatch = 14,
        Gunaso = 15,
        DevelopmentPlan = 16
    }
}