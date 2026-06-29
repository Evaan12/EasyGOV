namespace Domain.Enums
{
    public enum CampaignStatus : int
    {
        Draft = 1,
        PendingApproval = 2,
        Approved = 3,
        Running = 4,
        Completed = 5,
        Rejected = 6,
        Cancelled = 7
    }
}