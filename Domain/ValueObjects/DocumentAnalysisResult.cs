using System.Collections.Generic;
using Domain.Common;
using Domain.Exceptions;

namespace Domain.ValueObjects
{
    public class DocumentAnalysisResult : ValueObject
    {
        public double ElaForgeryScore { get; }
        public bool IsMetadataTampered { get; }
        public string? SoftwareSignatures { get; } 
        
        public bool IsVerifiedAuthentic => ElaForgeryScore < 0.20 && !IsMetadataTampered;

        private DocumentAnalysisResult() { }

        public DocumentAnalysisResult(double elaForgeryScore, bool isMetadataTampered, string? softwareSignatures)
        {
            if (elaForgeryScore < 0 || elaForgeryScore > 1)
                throw new DomainException("ELA Forgery Score must be bounded probabilistically between 0.0 and 1.0.");

            ElaForgeryScore = elaForgeryScore;
            IsMetadataTampered = isMetadataTampered;
            SoftwareSignatures = softwareSignatures;
        }

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return ElaForgeryScore;
            yield return IsMetadataTampered;
            yield return SoftwareSignatures;
        }
    }
}