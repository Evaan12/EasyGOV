using Domain.Common;
using Domain.Enums;
using Domain.Events;
using Domain.Exceptions;
using Domain.ValueObjects;
using System;

namespace Domain.Entities
{
    public class CitizenProfile : Entity, IAggregateRoot
    {
        public string FullName { get; private set; }
        public DateTime DateOfBirth { get; private set; }
        public Gender Gender { get; private set; }
        public PhoneNumber? MobileNumber { get; private set; } 
        
        public CitizenshipDetails? Citizenship { get; private set; }
        public NationalIdDetails? NationalId { get; private set; }
        public BirthCertificateDetails? BirthCertificate { get; private set; }
        
        public Guid RegisteredWardId { get; private set; }
        public CitizenStatus Status { get; private set; }
        
        public BiometricEmbedding? FaceEmbedding { get; private set; }
        public byte[]? FingerprintTemplate { get; private set; } 

        private CitizenProfile() { FullName = string.Empty; }

        public CitizenProfile(Guid id, string fullName, DateTime dateOfBirth, Gender gender, PhoneNumber? mobileNumber, Guid registeredWardId, Guid createdBy)
            : base(id, createdBy)
        {
            FullName = fullName;
            DateOfBirth = dateOfBirth;
            Gender = gender;
            MobileNumber = mobileNumber;
            RegisteredWardId = registeredWardId;
            Status = CitizenStatus.PendingDigital;
        }

        public void CompleteDigitalEkyc(CitizenshipDetails citizenship, BiometricEmbedding faceEmbedding, Guid updatedBy)
        {
            if (Status != CitizenStatus.PendingDigital)
                throw new DomainException("e-KYC can only be performed on profiles pending digital verification.");

            Citizenship = citizenship;
            FaceEmbedding = faceEmbedding;
            Status = CitizenStatus.PendingActivation;
            UpdatedBy = updatedBy;
            UpdatedAt = DateTime.UtcNow;
        }

        public void CompleteDigitalEkycWithBirthCertificate(BirthCertificateDetails birthCertificate, BiometricEmbedding faceEmbedding, Guid updatedBy)
        {
            if (Status != CitizenStatus.PendingDigital)
                throw new DomainException("e-KYC can only be performed on profiles pending digital verification.");

            BirthCertificate = birthCertificate;
            FaceEmbedding = faceEmbedding;
            Status = CitizenStatus.PendingActivation;
            UpdatedBy = updatedBy;
            UpdatedAt = DateTime.UtcNow;
        }

        public void CompleteDigitalEkycWithNationalId(NationalIdDetails nationalId, BiometricEmbedding faceEmbedding, Guid updatedBy)
        {
            if (Status != CitizenStatus.PendingDigital)
                throw new DomainException("e-KYC can only be performed on profiles pending digital verification.");

            NationalId = nationalId;
            FaceEmbedding = faceEmbedding;
            Status = CitizenStatus.PendingActivation;
            UpdatedBy = updatedBy;
            UpdatedAt = DateTime.UtcNow;
        }

        public void EnrollPhysicalBiometrics(byte[] fingerprintTemplate, Guid enrolledBy)
        {
            if (Status == CitizenStatus.Active) 
                throw new DomainException("Citizen is already active and biometrically anchored.");
            
            if (Status == CitizenStatus.PendingDigital)
                throw new DomainException("Citizen must complete digital e-KYC before physical activation.");

            if (fingerprintTemplate == null || fingerprintTemplate.Length == 0) 
                throw new DomainException("A valid ISO biometric template is strictly required.");

            FingerprintTemplate = fingerprintTemplate;
            Status = CitizenStatus.Active;
            UpdatedBy = enrolledBy;
            UpdatedAt = DateTime.UtcNow;
            
            AddDomainEvent(new CitizenBiometricEnrolledEvent(Id, RegisteredWardId));
        }

        public void RejectDigitalEkyc(Guid rejectedBy)
        {
            if (Status != CitizenStatus.PendingActivation)
                throw new DomainException("Can only reject profiles that are pending physical activation.");
            
            Citizenship = null;
            BirthCertificate = null;
            NationalId = null;
            FaceEmbedding = null;
            Status = CitizenStatus.PendingDigital;
            UpdatedBy = rejectedBy;
            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdateCivicDetails(string newFullName, NationalIdDetails? nationalId, Guid updatedBy)
        {
            FullName = newFullName;
            if (nationalId != null) NationalId = nationalId;
            
            UpdatedBy = updatedBy;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}