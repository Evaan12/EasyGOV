using Domain.Common;
using Domain.Exceptions;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Domain.ValueObjects
{
    public class PhoneNumber : ValueObject
    {
        public string Value { get; }

        private PhoneNumber() { Value = string.Empty; }

        public PhoneNumber(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new DomainException("Phone number cannot be empty.");

            if (!Regex.IsMatch(value, @"^\+977(98|97)\d{8}$"))
                throw new DomainException("Invalid phone number format. Must be a valid Nepalese mobile number starting with '+977' followed by a 10-digit number starting with 98 or 97.");

            Value = value;
        }

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return Value;
        }
    }
}