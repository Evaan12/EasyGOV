using System;

namespace Domain.Common
{
    public interface IDomainEvent
    {
        string EventType { get; }
        DateTime OccurredOn { get; }
    }
}