using Domain.Common;
using Domain.Exceptions;
using System;
using System.Collections.Generic;

namespace Domain.ValueObjects
{
    public class NationalIdDetails : ValueObject
    {
        public string NinNumber { get; }
        public DateTime IssueDate { get; }

        private NationalIdDetails() { NinNumber = string.Empty; }

        public NationalIdDetails(string ninNumber, DateTime issueDate)
        {
            if (string.IsNullOrWhiteSpace(ninNumber))
                throw new DomainException("National ID Number (NIN) is strictly required.");
            
            if (issueDate > DateTime.UtcNow)
                throw new DomainException("Issue date cannot be in the future.");

            NinNumber = ninNumber;
            IssueDate = issueDate;
        }

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return NinNumber;
            yield return IssueDate;
        }
    }
}