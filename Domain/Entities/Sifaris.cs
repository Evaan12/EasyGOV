using Domain.Common;
using Domain.Enums;
using Domain.Events;
using Domain.Exceptions;
using System;

namespace Domain.Entities
{
    public class Sifaris : Entity, IAggregateRoot
    {
        public Guid CitizenId { get; private set; }
        public Guid WardId { get; private set; }
        public Guid SifarisTemplateId { get; private set; }
        public Guid ApplicationId { get; private set; }
        
        public string ProfileHashAtIssuance { get; private set; }
        public string SnapshotDataJson { get; private set; } 
        
        public SifarisStatus Status { get; private set; }
        
        public string? RevocationReason { get; private set; }
        public Guid? RevokedBy { get; private set; }
        public DateTime? RevokedAt { get; private set; }

        public string? ApproverName { get; private set; }
        public string? ApproverRole { get; private set; }

        private Sifaris() { }

        public Sifaris(Guid id, Guid citizenId, Guid wardId, Guid sifarisTemplateId, Guid applicationId, string snapshotDataJson, string profileHashAtIssuance, string approverName, string approverRole, Guid createdBy)
            : base(id, createdBy)
        {
            CitizenId = citizenId;
            WardId = wardId;
            SifarisTemplateId = sifarisTemplateId;
            ApplicationId = applicationId;
            SnapshotDataJson = snapshotDataJson;
            ProfileHashAtIssuance = profileHashAtIssuance;
            ApproverName = approverName;
            ApproverRole = approverRole;
            Status = SifarisStatus.Approved; 
            
            AddDomainEvent(new SifarisIssuedEvent(Id, CitizenId));
        }

        public void Revoke(string reason, Guid revokedBy)
        {
            if (Status != SifarisStatus.Approved) throw new DomainException("Only valid issued Sifaris can be revoked.");
            if (string.IsNullOrWhiteSpace(reason)) throw new DomainException("A legitimate revocation reason is required.");

            Status = SifarisStatus.Revoked;
            RevocationReason = reason;
            RevokedBy = revokedBy;
            RevokedAt = DateTime.UtcNow;
            UpdatedBy = revokedBy;
            UpdatedAt = DateTime.UtcNow;
            
            AddDomainEvent(new SifarisRevokedEvent(Id, CitizenId, reason));
        }
    }
}