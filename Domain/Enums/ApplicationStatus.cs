namespace Domain.Enums
{
    public enum ApplicationStatus : int
    {
        Draft = 1,
        PendingReview = 2,
        Approved = 3,
        Rejected = 4,
        RequiresModification = 5
    }
}