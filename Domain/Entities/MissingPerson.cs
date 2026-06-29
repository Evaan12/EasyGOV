using Domain.Common;
using Domain.Exceptions;
using Domain.ValueObjects;
using System;

namespace Domain.Entities
{
    public class MissingPerson : Entity, IAggregateRoot
    {
        public string FullName { get; private set; }
        public BiometricEmbedding FaceEmbedding { get; private set; }
        public Guid ReportedByWardId { get; private set; }
        public bool IsFound { get; private set; }
        public string? Notes { get; private set; }

        private MissingPerson() { }

        public MissingPerson(Guid id, string fullName, BiometricEmbedding faceEmbedding, Guid reportedByWardId, string? notes, Guid createdBy)
            : base(id, createdBy)
        {
            if (string.IsNullOrWhiteSpace(fullName)) throw new DomainException("Full name is required.");
            if (notes != null && notes.Length > 2000) throw new DomainException("Notes cannot exceed 2000 characters.");

            FullName = fullName;
            FaceEmbedding = faceEmbedding;
            ReportedByWardId = reportedByWardId;
            Notes = notes;
            IsFound = false;
        }

        public void UpdateDetails(string fullName, string? notes, Guid updatedBy)
        {
            if (IsFound) throw new DomainException("Cannot update details of a person who has already been marked as found.");
            if (string.IsNullOrWhiteSpace(fullName)) throw new DomainException("Full name is required.");
            if (notes != null && notes.Length > 2000) throw new DomainException("Notes cannot exceed 2000 characters.");

            FullName = fullName;
            Notes = notes;
            UpdatedBy = updatedBy;
            UpdatedAt = DateTime.UtcNow;
        }

        public void MarkAsFound(Guid updatedBy)
        {
            if (IsFound) throw new DomainException("Person is already marked as found.");
            IsFound = true;
            UpdatedBy = updatedBy;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}