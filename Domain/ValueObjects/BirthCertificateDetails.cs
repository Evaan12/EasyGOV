using Domain.Common;
using Domain.Exceptions;
using System;
using System.Collections.Generic;

namespace Domain.ValueObjects
{
    public class BirthCertificateDetails : ValueObject
    {
        public string RegistrationNumber { get; }
        public DateTime IssueDate { get; }
        public Guid IssueDistrictId { get; }

        private BirthCertificateDetails() { }

        public BirthCertificateDetails(string registrationNumber, DateTime issueDate, Guid issueDistrictId)
        {
            if (string.IsNullOrWhiteSpace(registrationNumber))
                throw new DomainException("Registration number is strictly required.");
            
            if (issueDate > DateTime.UtcNow)
                throw new DomainException("Issue date cannot be in the future.");

            RegistrationNumber = registrationNumber;
            IssueDate = issueDate;
            IssueDistrictId = issueDistrictId;
        }

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return RegistrationNumber;
            yield return IssueDate;
            yield return IssueDistrictId;
        }
    }
}