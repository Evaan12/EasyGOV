using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Web.ViewModels.Validation
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class MinDateAttribute : ValidationAttribute, IClientModelValidator
    {
        private readonly DateOnly _minDate;
        private readonly string _minDateString;

        public MinDateAttribute(string minDateString)
        {
            _minDateString = minDateString;
            _minDate = DateOnly.Parse(minDateString);
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is DateOnly dateOnlyValue)
            {
                if (dateOnlyValue < _minDate)
                {
                    return new ValidationResult(ErrorMessage ?? $"Date cannot be before {_minDate:yyyy-MM-dd}.");
                }
            }
            else if (value is DateTime dateTimeValue)
            {
                if (DateOnly.FromDateTime(dateTimeValue) < _minDate)
                {
                    return new ValidationResult(ErrorMessage ?? $"Date cannot be before {_minDate:yyyy-MM-dd}.");
                }
            }
            return ValidationResult.Success;
        }

        public void AddValidation(ClientModelValidationContext context)
        {
            MergeAttribute(context.Attributes, "data-val", "true");
            MergeAttribute(context.Attributes, "data-val-mindate", ErrorMessage ?? $"Date cannot be before {_minDate:yyyy-MM-dd}.");
            MergeAttribute(context.Attributes, "data-val-mindate-min", _minDateString);
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