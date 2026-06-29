using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Web.ViewModels.Validation
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class MaxDateAttribute : ValidationAttribute, IClientModelValidator
    {
        private readonly DateOnly? _maxDate;
        private readonly string? _maxDateString;
        private readonly bool _useCurrentDate;

        public MaxDateAttribute(string maxDateString)
        {
            _maxDateString = maxDateString;
            _maxDate = DateOnly.Parse(maxDateString);
            _useCurrentDate = false;
        }

        public MaxDateAttribute(bool useCurrentDate = true)
        {
            _useCurrentDate = useCurrentDate;
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            var maxAllowedDate = _useCurrentDate 
                ? DateOnly.FromDateTime(DateTime.UtcNow) 
                : _maxDate!.Value;

            if (value is DateOnly dateOnlyValue)
            {
                if (dateOnlyValue > maxAllowedDate)
                {
                    return new ValidationResult(ErrorMessage ?? $"Date cannot be later than {maxAllowedDate:yyyy-MM-dd}.");
                }
            }
            else if (value is DateTime dateTimeValue)
            {
                if (DateOnly.FromDateTime(dateTimeValue) > maxAllowedDate)
                {
                    return new ValidationResult(ErrorMessage ?? $"Date cannot be later than {maxAllowedDate:yyyy-MM-dd}.");
                }
            }
            return ValidationResult.Success;
        }

        public void AddValidation(ClientModelValidationContext context)
        {
            var limitString = _useCurrentDate ? DateTime.UtcNow.ToString("yyyy-MM-dd") : _maxDateString;

            MergeAttribute(context.Attributes, "data-val", "true");
            MergeAttribute(context.Attributes, "data-val-maxdate", ErrorMessage ?? $"Date cannot be later than {limitString}.");
            MergeAttribute(context.Attributes, "data-val-maxdate-max", limitString!);
        }

        private static void MergeAttribute(IDictionary<string, string> attributes, string key, string value)
        {
            if (!attributes.ContainsKey(key))
            {
                attributes.Add(key, value);
            }
        }
    }
}