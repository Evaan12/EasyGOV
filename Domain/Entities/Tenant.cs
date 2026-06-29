using Domain.Common;
using Domain.Enums;
using Domain.Events;
using Domain.Exceptions;
using System;

namespace Domain.Entities
{
    public class Tenant : Entity, IAggregateRoot
    {
        public string Name { get; private set; } = string.Empty;
        public TenantType TenantType { get; private set; }
        public bool IsActivated { get; private set; }
        
        public string LtreePath { get; private set; } = string.Empty;
        
        public Guid? ParentId { get; private set; }
        public Tenant? Parent { get; private set; }

        public Guid? ProvinceId { get; private set; }
        public Guid? DistrictId { get; private set; }
        public Guid? MunicipalityId { get; private set; }

        public string? RegistrationId { get; private set; }
        public bool HasAdminAssigned { get; private set; }

        private Tenant() { }

        public Tenant(Guid id, string name, TenantType tenantType, Guid? parentId, string ltreePath, Guid? provinceId, Guid? districtId, Guid? municipalityId, Guid createdBy) 
            : base(id, createdBy)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new DomainException("Tenant name cannot be empty.");
            if (name.Length > 256) throw new DomainException("Tenant name cannot exceed 256 characters.");

            Name = name;
            TenantType = tenantType;
            ParentId = parentId;
            LtreePath = ltreePath;
            ProvinceId = provinceId;
            DistrictId = districtId;
            MunicipalityId = municipalityId;
            IsActivated = false;
            HasAdminAssigned = false;
        }

        public void SetLtreePath(string path)
        {
            LtreePath = path;
        }

        public void Update(string name, Guid updatedBy)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new DomainException("Tenant name cannot be empty.");
            if (name.Length > 256) throw new DomainException("Tenant name cannot exceed 256 characters.");
            
            Name = name;
            UpdatedBy = updatedBy;
            UpdatedAt = DateTime.UtcNow;
        }

        public void MarkAdminAssigned(Guid updatedBy)
        {
            HasAdminAssigned = true;
            UpdatedBy = updatedBy;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Activate(Guid activatedBy)
        {
            if (!HasAdminAssigned) 
                throw new DomainException("Cannot activate a tenant without an assigned administrator to manage it.");
            if (IsActivated) 
                throw new DomainException("Tenant is already activated and operational.");
            
            IsActivated = true;
            
            if (TenantType == TenantType.Ward && string.IsNullOrEmpty(RegistrationId))
            {
                RegistrationId = Guid.NewGuid().ToString("N").Substring(0, 10).ToUpperInvariant();
            }

            UpdatedBy = activatedBy;
            UpdatedAt = DateTime.UtcNow;
            AddDomainEvent(new TenantActivatedEvent(Id));
        }

        public void Deactivate(Guid deactivatedBy)
        {
            if (!IsActivated) throw new DomainException("Tenant is already deactivated.");
            IsActivated = false;
            UpdatedBy = deactivatedBy;
            UpdatedAt = DateTime.UtcNow;
            AddDomainEvent(new TenantDeactivatedEvent(Id));
        }

        public void MarkAsDeleted(Guid deletedBy)
        {
            IsDeleted = true;
            DeletedBy = deletedBy;
            DeletedAt = DateTime.UtcNow;
            IsActivated = false;
        }
    }
}
