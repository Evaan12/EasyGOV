using System;
using System.Collections.Generic;

namespace Domain.Common
{
    public abstract class Entity : IHasDomainEvents
    {
        public Guid Id { get; protected set; }
        public Guid CreatedBy { get; protected set; }
        public DateTime CreatedAt { get; protected set; }
        public Guid? UpdatedBy { get; protected set; }
        public DateTime? UpdatedAt { get; protected set; }
        public bool IsDeleted { get; protected set; } = false;
        public Guid? DeletedBy { get; protected set; }
        public DateTime? DeletedAt { get; protected set; }
        public byte[] RowVersion { get; protected set; } = Array.Empty<byte>();
        
        public bool? IsDefault { get; protected set; }

        private readonly List<IDomainEvent> _domainEvents = new();
        public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

        protected Entity() { }
        protected Entity(Guid id, Guid createdBy) { Id = id; CreatedBy = createdBy; }

        public void AddDomainEvent(IDomainEvent domainEvent) { _domainEvents.Add(domainEvent); }
        public void RemoveDomainEvent(IDomainEvent domainEvent) { _domainEvents.Remove(domainEvent); }
        public void ClearDomainEvents() { _domainEvents.Clear(); }

        public override bool Equals(object? obj)
        {
            if (obj is not Entity other) return false;
            if (ReferenceEquals(this, other)) return true;
            if (Id.Equals(default) || other.Id.Equals(default)) return false;
            return Id.Equals(other.Id);
        }

        public override int GetHashCode() => Id.GetHashCode();
    }
}