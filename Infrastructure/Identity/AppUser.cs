using Domain.Common;
using Domain.Entities;
using Domain.Enums;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;

namespace Infrastructure.Identity
{
    public class AppUser : IdentityUser<Guid>, IHasDomainEvents
    {
        public string FullName { get; set; } = string.Empty;
        
        public TenantType TenantType { get; set; }
        public Guid TenantId { get; set; }
        public Tenant? Tenant { get; set; }

        public virtual CitizenProfile? CitizenProfile { get; set; }

        public bool? IsDefault { get; set; }
        public DateTimeOffset? SuspensionEndDate { get; set; }
        public bool IsBanned { get; set; }
        public string? BanReason { get; set; }

        private readonly List<IDomainEvent> _domainEvents = new();
        public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

        public void AddDomainEvent(IDomainEvent domainEvent) { _domainEvents.Add(domainEvent); }
        public void RemoveDomainEvent(IDomainEvent domainEvent) { _domainEvents.Remove(domainEvent); }
        public void ClearDomainEvents() { _domainEvents.Clear(); }
    }
}