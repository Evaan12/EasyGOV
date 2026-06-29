using System;
using Domain.Common;

namespace Domain.Events
{
    public class ApiConsentGrantedEvent : IDomainEvent
    {
        public Guid RequestId { get; }
        public Guid CitizenId { get; }
        public string ThirdPartyClientId { get; }
        public string EventType => nameof(ApiConsentGrantedEvent);
        public DateTime OccurredOn { get; }

        public ApiConsentGrantedEvent(Guid requestId, Guid citizenId, string thirdPartyClientId)
        {
            RequestId = requestId;
            CitizenId = citizenId;
            ThirdPartyClientId = thirdPartyClientId;
            OccurredOn = DateTime.UtcNow;
        }
    }
}