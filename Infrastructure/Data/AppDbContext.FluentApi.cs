using Domain.Entities;
using Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Pgvector;
using System;

namespace Infrastructure.Data
{
    public partial class AppDbContext
    {
        private void ConfigureFluentApi(ModelBuilder builder)
        {
            // Forces mapping resolution for Unspecified Kind forms globally across PostgreSQL instances
            var dateTimeConverter = new ValueConverter<DateTime, DateTime>(
                v => v.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(v, DateTimeKind.Utc) : v.ToUniversalTime(),
                v => DateTime.SpecifyKind(v, DateTimeKind.Utc)
            );

            var nullableDateTimeConverter = new ValueConverter<DateTime?, DateTime?>(
                v => v.HasValue ? (v.Value.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : v.Value.ToUniversalTime()) : v,
                v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : v
            );

            foreach (var entityType in builder.Model.GetEntityTypes())
            {
                if (entityType.IsKeyless) continue;
                foreach (var property in entityType.GetProperties())
                {
                    if (property.ClrType == typeof(DateTime))
                        property.SetValueConverter(dateTimeConverter);
                    else if (property.ClrType == typeof(DateTime?))
                        property.SetValueConverter(nullableDateTimeConverter);
                }
            }

            var vectorConverter = new ValueConverter<float[], Vector>(
                v => new Vector(v),
                v => v.ToArray()
            );

            builder.Entity<OutboxMessage>(entity =>
            {
                entity.ToTable("OutboxMessages");
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.ProcessedOn, e.RetryCount, e.LockedOn });
                entity.HasIndex(e => e.OccurredOn);
            });

            builder.Entity<Sifaris>(entity =>
            {
                entity.ToTable("Sifaris");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.SnapshotDataJson).HasColumnType("jsonb");
                entity.HasIndex(e => e.CitizenId);
                entity.HasIndex(e => e.WardId);
                entity.HasIndex(e => e.Status);
            });

            builder.Entity<SifarisApplication>(entity =>
            {
                entity.ToTable("SifarisApplications");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.SubmittedDataJson).HasColumnType("jsonb");
                entity.HasIndex(e => e.CitizenId);
                entity.HasIndex(e => e.TargetWardId);
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.TargetSifarisTemplateId);
            });

            builder.Entity<MissingPerson>(entity =>
            {
                entity.ToTable("MissingPersons");
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.IsFound);
                entity.HasIndex(e => e.ReportedByWardId);

                entity.OwnsOne(e => e.FaceEmbedding, be =>
                {
                    be.Property(p => p.VectorData)
                      .HasColumnType("vector(512)")
                      .HasColumnName("FaceVectorData")
                      .HasConversion(vectorConverter);

                    be.HasIndex(p => p.VectorData)
                      .HasMethod("hnsw")
                      .HasOperators("vector_cosine_ops");
                });
            });

            builder.Entity<CitizenProfile>(entity =>
            {
                entity.ToTable("CitizenProfiles");
                entity.HasKey(e => e.Id);
                entity.HasOne<AppUser>().WithOne(u => u.CitizenProfile).HasForeignKey<CitizenProfile>(c => c.Id).OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => e.RegisteredWardId);
                entity.HasIndex(e => e.Status);

                entity.OwnsOne(e => e.FaceEmbedding, be =>
                {
                    be.Property(p => p.VectorData)
                      .HasColumnType("vector(512)")
                      .HasColumnName("FaceVectorData")
                      .HasConversion(vectorConverter);

                    be.HasIndex(p => p.VectorData)
                      .HasMethod("hnsw")
                      .HasOperators("vector_cosine_ops");
                });

                entity.OwnsOne(e => e.Citizenship, c =>
                {
                    c.Property(p => p.CitizenshipNumber).HasColumnName("CitizenshipNumber");
                    c.HasIndex(p => p.CitizenshipNumber);
                    c.Property(p => p.IssueDate).HasColumnName("CitizenshipIssueDate");
                    c.Property(p => p.IssueDistrictId).HasColumnName("CitizenshipIssueDistrictId");
                });

                entity.OwnsOne(e => e.NationalId, n =>
                {
                    n.Property(p => p.NinNumber).HasColumnName("NationalIdNumber");
                    n.HasIndex(p => p.NinNumber);
                    n.Property(p => p.IssueDate).HasColumnName("NationalIdIssueDate");
                });

                entity.OwnsOne(e => e.BirthCertificate, b =>
                {
                    b.Property(p => p.RegistrationNumber).HasColumnName("BirthRegistrationNumber");
                    b.HasIndex(p => p.RegistrationNumber);
                    b.Property(p => p.IssueDate).HasColumnName("BirthCertificateIssueDate");
                    b.Property(p => p.IssueDistrictId).HasColumnName("BirthCertificateIssueDistrictId");
                });

                entity.OwnsOne(e => e.MobileNumber, m =>
                {
                    m.Property(p => p.Value).HasColumnName("MobileNumber");
                    m.HasIndex(p => p.Value);
                });
            });

            builder.Entity<TenantSecurityPolicy>(entity =>
            {
                entity.ToTable("TenantSecurityPolicies");
                entity.HasKey(e => e.Id);

                entity.OwnsOne(e => e.TimeWindow, tw => {
                    tw.Property(p => p.StartTime).HasColumnName("AllowedStartTime");
                    tw.Property(p => p.EndTime).HasColumnName("AllowedEndTime");
                });
            });

            builder.Entity<Tenant>(entity =>
            {
                entity.ToTable("Tenants");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(256);

                entity.Property(e => e.LtreePath).HasColumnType("ltree").IsRequired();

                entity.HasOne(e => e.Parent).WithMany().HasForeignKey(e => e.ParentId).OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<RolePermission>(entity =>
            {
                entity.ToTable("RolePermissions");
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.RoleId);
                
                entity.OwnsOne(e => e.TimeWindow, tw => { 
                    tw.Property(p => p.StartTime).HasColumnName("AllowedStartTime"); 
                    tw.Property(p => p.EndTime).HasColumnName("AllowedEndTime"); 
                });
            });

            builder.Entity<AppUser>(entity =>
            {
                entity.HasIndex(e => e.FullName);
                entity.HasIndex(e => e.Email);
                entity.HasOne(u => u.Tenant).WithMany().HasForeignKey(u => u.TenantId).OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<AppRole>(entity =>
            {
                entity.HasOne(r => r.Tenant).WithMany().HasForeignKey(r => r.TenantId).OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<DocumentTemplate>(entity =>
            {
                entity.ToTable("DocumentTemplates");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.FormSchemaJson).HasColumnType("jsonb");
                entity.HasOne<Tenant>().WithMany().HasForeignKey(e => e.TenantId).OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<DocumentFile>(entity =>
            {
                entity.ToTable("DocumentFiles");
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.OwnerId);

                entity.OwnsOne(e => e.AnalysisResult, ar =>
                {
                    ar.Property(p => p.ElaForgeryScore).HasColumnName("ElaForgeryScore");
                    ar.Property(p => p.IsMetadataTampered).HasColumnName("IsMetadataTampered");
                    ar.Property(p => p.SoftwareSignatures).HasColumnName("SoftwareSignatures");
                });
            });

            builder.Entity<ApiConsentRequest>(entity =>
            {
                entity.ToTable("ApiConsentRequests");
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.CitizenId);
                entity.HasIndex(e => e.ThirdPartyClientId);
                entity.HasIndex(e => e.Status);
            });

            builder.Entity<AlertCampaign>(entity =>
            {
                entity.ToTable("AlertCampaigns");
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.TargetTenantId);
                entity.HasMany(e => e.Approvals).WithOne().HasForeignKey(a => a.AlertCampaignId).OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<CampaignDispatch>(entity =>
            {
                entity.ToTable("CampaignDispatches");
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.AlertCampaignId);
                entity.HasIndex(e => e.ExternalDispatchId);
            });

            builder.Entity<Gunaso>(entity =>
            {
                entity.ToTable("Gunasos");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.TargetLtreePath).HasColumnType("ltree").IsRequired();
                entity.HasIndex(e => e.CitizenId);
                entity.HasIndex(e => e.TargetTenantId);
                entity.HasIndex(e => e.Status);
            });
            
            builder.Entity<DevelopmentPlan>(entity =>
            {
                entity.ToTable("DevelopmentPlans");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.TenantLtreePath).HasColumnType("ltree").IsRequired();
                entity.HasIndex(e => e.TenantId);
                entity.HasIndex(e => e.Status);
            });
        }
    }
}