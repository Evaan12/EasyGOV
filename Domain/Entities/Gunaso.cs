using Domain.Common;
using Domain.Enums;
using Domain.Events;
using Domain.Exceptions;
using System;

namespace Domain.Entities
{
    public class Gunaso : Entity, IAggregateRoot
    {
        public string Title { get; private set; }
        public string Description { get; private set; }
        public GunasoSeverity Severity { get; private set; }
        public GunasoStatus Status { get; private set; }
        public Guid CitizenId { get; private set; }
        
        public Guid TargetTenantId { get; private set; }
        public string TargetLtreePath { get; private set; }
        
        public string? ResolutionNotes { get; private set; }

        private Gunaso() { }

        public Gunaso(Guid id, string title, string description, GunasoSeverity severity, Guid citizenId, Guid targetTenantId, string targetLtreePath, Guid createdBy)
            : base(id, createdBy)
        {
            if (string.IsNullOrWhiteSpace(title)) throw new DomainException("Title is required.");
            if (string.IsNullOrWhiteSpace(description)) throw new DomainException("Description is required.");

            Title = title;
            Description = description;
            Severity = severity;
            CitizenId = citizenId;
            TargetTenantId = targetTenantId;
            TargetLtreePath = targetLtreePath;
            Status = GunasoStatus.Submitted;

            AddDomainEvent(new GunasoFiledEvent(Id, TargetTenantId, CitizenId));
        }

        public void Escalate(Guid newTargetTenantId, string newTargetLtreePath, Guid updatedBy)
        {
            if (Status == GunasoStatus.Resolved || Status == GunasoStatus.Closed)
                throw new DomainException("Cannot escalate a resolved or closed Gunaso.");

            TargetTenantId = newTargetTenantId;
            TargetLtreePath = newTargetLtreePath;
            Status = GunasoStatus.Escalated;
            UpdatedBy = updatedBy;
            UpdatedAt = DateTime.UtcNow;

            AddDomainEvent(new GunasoEscalatedEvent(Id, newTargetTenantId));
        }

        public void Resolve(string resolutionNotes, Guid resolvedBy)
        {
            if (string.IsNullOrWhiteSpace(resolutionNotes)) throw new DomainException("Resolution notes are strictly required.");
            
            ResolutionNotes = resolutionNotes;
            Status = GunasoStatus.Resolved;
            UpdatedBy = resolvedBy;
            UpdatedAt = DateTime.UtcNow;

            AddDomainEvent(new GunasoResolvedEvent(Id, CitizenId));
        }
    }
}