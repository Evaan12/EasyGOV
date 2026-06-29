namespace Domain.Enums
{
    public enum ConsentStatus : int
    {
        PendingOTP = 1,
        Consented = 2,
        Denied = 3,
        Expired = 4
    }
}