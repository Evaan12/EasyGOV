using Domain.Common;
using Domain.Exceptions;
using System;
using System.Collections.Generic;

namespace Domain.ValueObjects
{
    public class CitizenshipDetails : ValueObject
    {
        public string CitizenshipNumber { get; }
        public DateTime IssueDate { get; }
        public Guid IssueDistrictId { get; }

        private CitizenshipDetails() { }

        public CitizenshipDetails(string citizenshipNumber, DateTime issueDate, Guid issueDistrictId)
        {
            if (string.IsNullOrWhiteSpace(citizenshipNumber))
                throw new DomainException("Citizenship number is strictly required.");
            
            if (issueDate > DateTime.UtcNow)
                throw new DomainException("Issue date cannot be in the future.");

            CitizenshipNumber = citizenshipNumber;
            IssueDate = issueDate;
            IssueDistrictId = issueDistrictId;
        }

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return CitizenshipNumber;
            yield return IssueDate;
            yield return IssueDistrictId;
        }
    }
}