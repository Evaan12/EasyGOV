namespace Infrastructure.Data
{
    public class OutboxState
    {
        public bool HasPendingMessages { get; set; }
    }
}