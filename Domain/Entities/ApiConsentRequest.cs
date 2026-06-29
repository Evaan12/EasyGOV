using Domain.Common;
using Domain.Enums;
using Domain.Events;
using Domain.Exceptions;
using System;

namespace Domain.Entities
{
    public class ApiConsentRequest : Entity, IAggregateRoot
    {
        public Guid CitizenId { get; private set; }
        public string ThirdPartyClientId { get; private set; }
        public string RequestedDataScopes { get; private set; }
        
        public string OtpHash { get; private set; } 
        public DateTime ExpiresAt { get; private set; }
        public ConsentStatus Status { get; private set; }

        private ApiConsentRequest() { }

        public ApiConsentRequest(Guid id, Guid citizenId, string thirdPartyClientId, string requestedDataScopes, string otpHash, TimeSpan validityWindow, Guid createdBy)
            : base(id, createdBy)
        {
            CitizenId = citizenId;
            ThirdPartyClientId = thirdPartyClientId;
            RequestedDataScopes = requestedDataScopes;
            OtpHash = otpHash;
            ExpiresAt = DateTime.UtcNow.Add(validityWindow);
            Status = ConsentStatus.PendingOTP;
        }

        public void GrantConsent(string computedIncomingHash)
        {
            if (Status != ConsentStatus.PendingOTP)
                throw new DomainException("This consent request is no longer valid or has already been processed.");
            
            if (DateTime.UtcNow > ExpiresAt)
            {
                Status = ConsentStatus.Expired;
                throw new DomainException("The OTP timeframe has expired.");
            }

            if (OtpHash != computedIncomingHash)
            {
                throw new DomainException("Invalid OTP provided. Consent denied.");
            }

            Status = ConsentStatus.Consented;
            UpdatedAt = DateTime.UtcNow;

            AddDomainEvent(new ApiConsentGrantedEvent(Id, CitizenId, ThirdPartyClientId));
        }

        public void DenyConsent()
        {
            Status = ConsentStatus.Denied;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}