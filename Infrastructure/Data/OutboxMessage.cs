using System;

namespace Infrastructure.Data
{
    public class OutboxMessage
    {
        public Guid Id { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime OccurredOn { get; set; }
        public DateTime? ProcessedOn { get; set; }
        public string? Error { get; set; }
        public int RetryCount { get; set; } = 0;
        public DateTime? LockedOn { get; set; } 
        public string? LockedBy { get; set; }
    }
}