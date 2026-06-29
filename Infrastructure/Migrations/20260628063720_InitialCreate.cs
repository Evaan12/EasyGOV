using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using Pgvector;
using System;
using System.Text.Json;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:ltree", ",,")
                .Annotation("Npgsql:PostgresExtension:vector", ",,");

            migrationBuilder.CreateTable(
                name: "AlertCampaigns",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Category = table.Column<int>(type: "integer", nullable: false),
                    MessageScript = table.Column<string>(type: "text", nullable: false),
                    TargetTenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    TelephonyProvider = table.Column<string>(type: "text", nullable: true),
                    ExternalProviderCampaignId = table.Column<string>(type: "text", nullable: true),
                    ProgressPercent = table.Column<decimal>(type: "numeric", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "bytea", nullable: false),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlertCampaigns", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ApiConsentRequests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CitizenId = table.Column<Guid>(type: "uuid", nullable: false),
                    ThirdPartyClientId = table.Column<string>(type: "text", nullable: false),
                    RequestedDataScopes = table.Column<string>(type: "text", nullable: false),
                    OtpHash = table.Column<string>(type: "text", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "bytea", nullable: false),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApiConsentRequests", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CampaignDispatches",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AlertCampaignId = table.Column<Guid>(type: "uuid", nullable: false),
                    CitizenId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExternalDispatchId = table.Column<string>(type: "text", nullable: true),
                    DispatchStatus = table.Column<int>(type: "integer", nullable: false),
                    DurationInSeconds = table.Column<int>(type: "integer", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "bytea", nullable: false),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CampaignDispatches", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DevelopmentPlans",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Budget = table.Column<decimal>(type: "numeric", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantLtreePath = table.Column<string>(type: "ltree", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "bytea", nullable: false),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DevelopmentPlans", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DocumentFiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OwnerId = table.Column<Guid>(type: "uuid", nullable: false),
                    FileType = table.Column<int>(type: "integer", nullable: false),
                    OriginalFileName = table.Column<string>(type: "text", nullable: false),
                    FilePath = table.Column<string>(type: "text", nullable: false),
                    MimeType = table.Column<string>(type: "text", nullable: false),
                    FileSizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    ChecksumSha256 = table.Column<string>(type: "text", nullable: false),
                    ElaForgeryScore = table.Column<double>(type: "double precision", nullable: true),
                    IsMetadataTampered = table.Column<bool>(type: "boolean", nullable: true),
                    SoftwareSignatures = table.Column<string>(type: "text", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "bytea", nullable: false),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentFiles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Gunasos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Severity = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CitizenId = table.Column<Guid>(type: "uuid", nullable: false),
                    TargetTenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    TargetLtreePath = table.Column<string>(type: "ltree", nullable: false),
                    ResolutionNotes = table.Column<string>(type: "text", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "bytea", nullable: false),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Gunasos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MissingPersons",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FullName = table.Column<string>(type: "text", nullable: false),
                    FaceVectorData = table.Column<Vector>(type: "vector(512)", nullable: false),
                    ReportedByWardId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsFound = table.Column<bool>(type: "boolean", nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "bytea", nullable: false),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MissingPersons", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OutboxMessages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    Content = table.Column<string>(type: "text", nullable: false),
                    OccurredOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ProcessedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Error = table.Column<string>(type: "text", nullable: true),
                    RetryCount = table.Column<int>(type: "integer", nullable: false),
                    LockedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LockedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OutboxMessages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RolePermissions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RoleId = table.Column<Guid>(type: "uuid", nullable: false),
                    ResourceType = table.Column<int>(type: "integer", nullable: false),
                    ActionType = table.Column<int>(type: "integer", nullable: false),
                    AllowedStartTime = table.Column<TimeSpan>(type: "interval", nullable: true),
                    AllowedEndTime = table.Column<TimeSpan>(type: "interval", nullable: true),
                    AllowedIpAddress = table.Column<string>(type: "text", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "bytea", nullable: false),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RolePermissions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Sifaris",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CitizenId = table.Column<Guid>(type: "uuid", nullable: false),
                    WardId = table.Column<Guid>(type: "uuid", nullable: false),
                    SifarisTemplateId = table.Column<Guid>(type: "uuid", nullable: false),
                    ApplicationId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProfileHashAtIssuance = table.Column<string>(type: "text", nullable: false),
                    SnapshotDataJson = table.Column<string>(type: "jsonb", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    RevocationReason = table.Column<string>(type: "text", nullable: true),
                    RevokedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    RevokedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ApproverName = table.Column<string>(type: "text", nullable: true),
                    ApproverRole = table.Column<string>(type: "text", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "bytea", nullable: false),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sifaris", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SifarisApplications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CitizenId = table.Column<Guid>(type: "uuid", nullable: false),
                    TargetWardId = table.Column<Guid>(type: "uuid", nullable: false),
                    ApplicationTemplateId = table.Column<Guid>(type: "uuid", nullable: false),
                    TargetSifarisTemplateId = table.Column<Guid>(type: "uuid", nullable: false),
                    SubmittedDataJson = table.Column<string>(type: "jsonb", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ReviewNotes = table.Column<string>(type: "text", nullable: true),
                    ReviewedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    ReviewedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ApproverName = table.Column<string>(type: "text", nullable: true),
                    ApproverRole = table.Column<string>(type: "text", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "bytea", nullable: false),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SifarisApplications", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Tenants",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    TenantType = table.Column<int>(type: "integer", nullable: false),
                    IsActivated = table.Column<bool>(type: "boolean", nullable: false),
                    LtreePath = table.Column<string>(type: "ltree", nullable: false),
                    ParentId = table.Column<Guid>(type: "uuid", nullable: true),
                    ProvinceId = table.Column<Guid>(type: "uuid", nullable: true),
                    DistrictId = table.Column<Guid>(type: "uuid", nullable: true),
                    MunicipalityId = table.Column<Guid>(type: "uuid", nullable: true),
                    RegistrationId = table.Column<string>(type: "text", nullable: true),
                    HasAdminAssigned = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "bytea", nullable: false),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tenants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tenants_Tenants_ParentId",
                        column: x => x.ParentId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TenantSecurityPolicies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AllowedStartTime = table.Column<TimeSpan>(type: "interval", nullable: true),
                    AllowedEndTime = table.Column<TimeSpan>(type: "interval", nullable: true),
                    AllowedIpAddress = table.Column<string>(type: "text", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "bytea", nullable: false),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TenantSecurityPolicies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CampaignApprovals",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AlertCampaignId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReviewedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    Decision = table.Column<int>(type: "integer", nullable: false),
                    Remarks = table.Column<string>(type: "text", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "bytea", nullable: false),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CampaignApprovals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CampaignApprovals_AlertCampaigns_AlertCampaignId",
                        column: x => x.AlertCampaignId,
                        principalTable: "AlertCampaigns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantType = table.Column<int>(type: "integer", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: true),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoles_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FullName = table.Column<string>(type: "text", nullable: false),
                    TenantType = table.Column<int>(type: "integer", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: true),
                    SuspensionEndDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    IsBanned = table.Column<bool>(type: "boolean", nullable: false),
                    BanReason = table.Column<string>(type: "text", nullable: true),
                    UserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: true),
                    SecurityStamp = table.Column<string>(type: "text", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true),
                    PhoneNumber = table.Column<string>(type: "text", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUsers_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DocumentTemplates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    FormSchemaJson = table.Column<string>(type: "jsonb", nullable: false),
                    HtmlContent = table.Column<string>(type: "text", nullable: false),
                    TenantType = table.Column<int>(type: "integer", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    OverridesTemplateId = table.Column<Guid>(type: "uuid", nullable: true),
                    LinkedTemplateId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "bytea", nullable: false),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentTemplates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DocumentTemplates_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RoleId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    ProviderKey = table.Column<string>(type: "text", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "text", nullable: true),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    RoleId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CitizenProfiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FullName = table.Column<string>(type: "text", nullable: false),
                    DateOfBirth = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Gender = table.Column<int>(type: "integer", nullable: false),
                    MobileNumber = table.Column<string>(type: "text", nullable: true),
                    CitizenshipNumber = table.Column<string>(type: "text", nullable: true),
                    CitizenshipIssueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CitizenshipIssueDistrictId = table.Column<Guid>(type: "uuid", nullable: true),
                    NationalIdNumber = table.Column<string>(type: "text", nullable: true),
                    NationalIdIssueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    BirthRegistrationNumber = table.Column<string>(type: "text", nullable: true),
                    BirthCertificateIssueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    BirthCertificateIssueDistrictId = table.Column<Guid>(type: "uuid", nullable: true),
                    RegisteredWardId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    FaceVectorData = table.Column<Vector>(type: "vector(512)", nullable: true),
                    FingerprintTemplate = table.Column<byte[]>(type: "bytea", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "bytea", nullable: false),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CitizenProfiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CitizenProfiles_AspNetUsers_Id",
                        column: x => x.Id,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "RolePermissions",
                columns: new[] { "Id", "ActionType", "AllowedIpAddress", "CreatedAt", "CreatedBy", "DeletedAt", "DeletedBy", "IsDefault", "IsDeleted", "ResourceType", "RoleId", "RowVersion", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { new Guid("22220002-1111-1111-1111-111111111111"), 15, null, new DateTime(2026, 6, 25, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("22222222-2222-2222-2222-222222222222"), null, null, true, false, 2, new Guid("22222222-1111-1111-1111-111111111111"), new byte[0], null, null },
                    { new Guid("22220003-1111-1111-1111-111111111111"), 79, null, new DateTime(2026, 6, 25, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("22222222-2222-2222-2222-222222222222"), null, null, true, false, 3, new Guid("22222222-1111-1111-1111-111111111111"), new byte[0], null, null },
                    { new Guid("22220008-1111-1111-1111-111111111111"), 527, null, new DateTime(2026, 6, 25, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("22222222-2222-2222-2222-222222222222"), null, null, true, false, 8, new Guid("22222222-1111-1111-1111-111111111111"), new byte[0], null, null },
                    { new Guid("22220009-1111-1111-1111-111111111111"), 307, null, new DateTime(2026, 6, 25, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("22222222-2222-2222-2222-222222222222"), null, null, true, false, 9, new Guid("22222222-1111-1111-1111-111111111111"), new byte[0], null, null },
                    { new Guid("2222000a-1111-1111-1111-111111111111"), 13, null, new DateTime(2026, 6, 25, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("22222222-2222-2222-2222-222222222222"), null, null, true, false, 10, new Guid("22222222-1111-1111-1111-111111111111"), new byte[0], null, null },
                    { new Guid("2222000b-1111-1111-1111-111111111111"), 15, null, new DateTime(2026, 6, 25, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("22222222-2222-2222-2222-222222222222"), null, null, true, false, 11, new Guid("22222222-1111-1111-1111-111111111111"), new byte[0], null, null },
                    { new Guid("2222000c-1111-1111-1111-111111111111"), 15, null, new DateTime(2026, 6, 25, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("22222222-2222-2222-2222-222222222222"), null, null, true, false, 12, new Guid("22222222-1111-1111-1111-111111111111"), new byte[0], null, null },
                    { new Guid("2222000d-1111-1111-1111-111111111111"), 47, null, new DateTime(2026, 6, 25, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("22222222-2222-2222-2222-222222222222"), null, null, true, false, 13, new Guid("22222222-1111-1111-1111-111111111111"), new byte[0], null, null },
                    { new Guid("2222000e-1111-1111-1111-111111111111"), 1, null, new DateTime(2026, 6, 25, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("22222222-2222-2222-2222-222222222222"), null, null, true, false, 14, new Guid("22222222-1111-1111-1111-111111111111"), new byte[0], null, null },
                    { new Guid("2222000f-1111-1111-1111-111111111111"), 13, null, new DateTime(2026, 6, 25, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("22222222-2222-2222-2222-222222222222"), null, null, true, false, 15, new Guid("22222222-1111-1111-1111-111111111111"), new byte[0], null, null },
                    { new Guid("22220010-1111-1111-1111-111111111111"), 15, null, new DateTime(2026, 6, 25, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("22222222-2222-2222-2222-222222222222"), null, null, true, false, 16, new Guid("22222222-1111-1111-1111-111111111111"), new byte[0], null, null },
                    { new Guid("33330002-1111-1111-1111-111111111111"), 15, null, new DateTime(2026, 6, 25, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("22222222-2222-2222-2222-222222222222"), null, null, true, false, 2, new Guid("33333333-1111-1111-1111-111111111111"), new byte[0], null, null },
                    { new Guid("33330003-1111-1111-1111-111111111111"), 79, null, new DateTime(2026, 6, 25, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("22222222-2222-2222-2222-222222222222"), null, null, true, false, 3, new Guid("33333333-1111-1111-1111-111111111111"), new byte[0], null, null },
                    { new Guid("33330008-1111-1111-1111-111111111111"), 527, null, new DateTime(2026, 6, 25, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("22222222-2222-2222-2222-222222222222"), null, null, true, false, 8, new Guid("33333333-1111-1111-1111-111111111111"), new byte[0], null, null },
                    { new Guid("33330009-1111-1111-1111-111111111111"), 307, null, new DateTime(2026, 6, 25, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("22222222-2222-2222-2222-222222222222"), null, null, true, false, 9, new Guid("33333333-1111-1111-1111-111111111111"), new byte[0], null, null },
                    { new Guid("3333000a-1111-1111-1111-111111111111"), 13, null, new DateTime(2026, 6, 25, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("22222222-2222-2222-2222-222222222222"), null, null, true, false, 10, new Guid("33333333-1111-1111-1111-111111111111"), new byte[0], null, null },
                    { new Guid("3333000b-1111-1111-1111-111111111111"), 15, null, new DateTime(2026, 6, 25, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("22222222-2222-2222-2222-222222222222"), null, null, true, false, 11, new Guid("33333333-1111-1111-1111-111111111111"), new byte[0], null, null },
                    { new Guid("3333000c-1111-1111-1111-111111111111"), 15, null, new DateTime(2026, 6, 25, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("22222222-2222-2222-2222-222222222222"), null, null, true, false, 12, new Guid("33333333-1111-1111-1111-111111111111"), new byte[0], null, null },
                    { new Guid("3333000d-1111-1111-1111-111111111111"), 47, null, new DateTime(2026, 6, 25, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("22222222-2222-2222-2222-222222222222"), null, null, true, false, 13, new Guid("33333333-1111-1111-1111-111111111111"), new byte[0], null, null },
                    { new Guid("3333000e-1111-1111-1111-111111111111"), 1, null, new DateTime(2026, 6, 25, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("22222222-2222-2222-2222-222222222222"), null, null, true, false, 14, new Guid("33333333-1111-1111-1111-111111111111"), new byte[0], null, null },
                    { new Guid("3333000f-1111-1111-1111-111111111111"), 13, null, new DateTime(2026, 6, 25, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("22222222-2222-2222-2222-222222222222"), null, null, true, false, 15, new Guid("33333333-1111-1111-1111-111111111111"), new byte[0], null, null },
                    { new Guid("33330010-1111-1111-1111-111111111111"), 15, null, new DateTime(2026, 6, 25, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("22222222-2222-2222-2222-222222222222"), null, null, true, false, 16, new Guid("33333333-1111-1111-1111-111111111111"), new byte[0], null, null },
                    { new Guid("33333333-3333-3333-3333-333333333333"), 128, null, new DateTime(2026, 6, 25, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("22222222-2222-2222-2222-222222222222"), null, null, true, false, 6, new Guid("11111111-1111-1111-1111-111111111111"), new byte[0], null, null },
                    { new Guid("44440002-4444-4444-4444-444444444444"), 15, null, new DateTime(2026, 6, 25, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("22222222-2222-2222-2222-222222222222"), null, null, true, false, 2, new Guid("44444444-4444-4444-4444-444444444444"), new byte[0], null, null },
                    { new Guid("44440003-4444-4444-4444-444444444444"), 79, null, new DateTime(2026, 6, 25, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("22222222-2222-2222-2222-222222222222"), null, null, true, false, 3, new Guid("44444444-4444-4444-4444-444444444444"), new byte[0], null, null },
                    { new Guid("44440008-4444-4444-4444-444444444444"), 527, null, new DateTime(2026, 6, 25, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("22222222-2222-2222-2222-222222222222"), null, null, true, false, 8, new Guid("44444444-4444-4444-4444-444444444444"), new byte[0], null, null },
                    { new Guid("44440009-4444-4444-4444-444444444444"), 307, null, new DateTime(2026, 6, 25, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("22222222-2222-2222-2222-222222222222"), null, null, true, false, 9, new Guid("44444444-4444-4444-4444-444444444444"), new byte[0], null, null },
                    { new Guid("4444000a-4444-4444-4444-444444444444"), 13, null, new DateTime(2026, 6, 25, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("22222222-2222-2222-2222-222222222222"), null, null, true, false, 10, new Guid("44444444-4444-4444-4444-444444444444"), new byte[0], null, null },
                    { new Guid("4444000b-4444-4444-4444-444444444444"), 15, null, new DateTime(2026, 6, 25, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("22222222-2222-2222-2222-222222222222"), null, null, true, false, 11, new Guid("44444444-4444-4444-4444-444444444444"), new byte[0], null, null },
                    { new Guid("4444000c-4444-4444-4444-444444444444"), 15, null, new DateTime(2026, 6, 25, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("22222222-2222-2222-2222-222222222222"), null, null, true, false, 12, new Guid("44444444-4444-4444-4444-444444444444"), new byte[0], null, null },
                    { new Guid("4444000d-4444-4444-4444-444444444444"), 47, null, new DateTime(2026, 6, 25, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("22222222-2222-2222-2222-222222222222"), null, null, true, false, 13, new Guid("44444444-4444-4444-4444-444444444444"), new byte[0], null, null },
                    { new Guid("4444000e-4444-4444-4444-444444444444"), 1, null, new DateTime(2026, 6, 25, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("22222222-2222-2222-2222-222222222222"), null, null, true, false, 14, new Guid("44444444-4444-4444-4444-444444444444"), new byte[0], null, null },
                    { new Guid("4444000f-4444-4444-4444-444444444444"), 13, null, new DateTime(2026, 6, 25, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("22222222-2222-2222-2222-222222222222"), null, null, true, false, 15, new Guid("44444444-4444-4444-4444-444444444444"), new byte[0], null, null },
                    { new Guid("44440010-4444-4444-4444-444444444444"), 15, null, new DateTime(2026, 6, 25, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("22222222-2222-2222-2222-222222222222"), null, null, true, false, 16, new Guid("44444444-4444-4444-4444-444444444444"), new byte[0], null, null },
                    { new Guid("55550002-5555-5555-5555-555555555555"), 15, null, new DateTime(2026, 6, 25, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("22222222-2222-2222-2222-222222222222"), null, null, true, false, 2, new Guid("55555555-5555-5555-5555-555555555555"), new byte[0], null, null },
                    { new Guid("55550003-5555-5555-5555-555555555555"), 79, null, new DateTime(2026, 6, 25, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("22222222-2222-2222-2222-222222222222"), null, null, true, false, 3, new Guid("55555555-5555-5555-5555-555555555555"), new byte[0], null, null },
                    { new Guid("55550008-5555-5555-5555-555555555555"), 527, null, new DateTime(2026, 6, 25, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("22222222-2222-2222-2222-222222222222"), null, null, true, false, 8, new Guid("55555555-5555-5555-5555-555555555555"), new byte[0], null, null },
                    { new Guid("55550009-5555-5555-5555-555555555555"), 307, null, new DateTime(2026, 6, 25, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("22222222-2222-2222-2222-222222222222"), null, null, true, false, 9, new Guid("55555555-5555-5555-5555-555555555555"), new byte[0], null, null },
                    { new Guid("5555000a-5555-5555-5555-555555555555"), 13, null, new DateTime(2026, 6, 25, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("22222222-2222-2222-2222-222222222222"), null, null, true, false, 10, new Guid("55555555-5555-5555-5555-555555555555"), new byte[0], null, null },
                    { new Guid("5555000b-5555-5555-5555-555555555555"), 15, null, new DateTime(2026, 6, 25, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("22222222-2222-2222-2222-222222222222"), null, null, true, false, 11, new Guid("55555555-5555-5555-5555-555555555555"), new byte[0], null, null },
                    { new Guid("5555000c-5555-5555-5555-555555555555"), 15, null, new DateTime(2026, 6, 25, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("22222222-2222-2222-2222-222222222222"), null, null, true, false, 12, new Guid("55555555-5555-5555-5555-555555555555"), new byte[0], null, null },
                    { new Guid("5555000d-5555-5555-5555-555555555555"), 47, null, new DateTime(2026, 6, 25, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("22222222-2222-2222-2222-222222222222"), null, null, true, false, 13, new Guid("55555555-5555-5555-5555-555555555555"), new byte[0], null, null },
                    { new Guid("5555000e-5555-5555-5555-555555555555"), 1, null, new DateTime(2026, 6, 25, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("22222222-2222-2222-2222-222222222222"), null, null, true, false, 14, new Guid("55555555-5555-5555-5555-555555555555"), new byte[0], null, null },
                    { new Guid("5555000f-5555-5555-5555-555555555555"), 13, null, new DateTime(2026, 6, 25, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("22222222-2222-2222-2222-222222222222"), null, null, true, false, 15, new Guid("55555555-5555-5555-5555-555555555555"), new byte[0], null, null },
                    { new Guid("55550010-5555-5555-5555-555555555555"), 15, null, new DateTime(2026, 6, 25, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("22222222-2222-2222-2222-222222222222"), null, null, true, false, 16, new Guid("55555555-5555-5555-5555-555555555555"), new byte[0], null, null },
                    { new Guid("7777000a-7777-7777-7777-777777777777"), 1, null, new DateTime(2026, 6, 25, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("22222222-2222-2222-2222-222222222222"), null, null, true, false, 10, new Guid("77777777-7777-7777-7777-777777777777"), new byte[0], null, null }
                });

            migrationBuilder.InsertData(
                table: "Sifaris",
                columns: new[] { "Id", "ApplicationId", "ApproverName", "ApproverRole", "CitizenId", "CreatedAt", "CreatedBy", "DeletedAt", "DeletedBy", "IsDefault", "IsDeleted", "ProfileHashAtIssuance", "RevocationReason", "RevokedAt", "RevokedBy", "RowVersion", "SifarisTemplateId", "SnapshotDataJson", "Status", "UpdatedAt", "UpdatedBy", "WardId" },
                values: new object[] { new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), "System Administrator", "Super Admin", new Guid("22222222-2222-2222-2222-222222222222"), new DateTime(2026, 6, 25, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("22222222-2222-2222-2222-222222222222"), null, null, null, false, "dummyhash", null, null, null, new byte[0], new Guid("99999999-9999-9999-9999-999999999999"), "{\"fiscalYear\": \"2082/83\", \"dispatchNumber\": \"105\", \"issueDate\": \"2026-06-25\", \"issueDistrict\": \"Kathmandu\", \"fatherName\": \"Shyam Admin\", \"citizen.fullname\": \"System Administrator\", \"citizen.dob\": \"1990-01-01\"}", 2, null, null, new Guid("00000000-0000-0000-0000-000000000001") });

            migrationBuilder.InsertData(
                table: "SifarisApplications",
                columns: new[] { "Id", "ApplicationTemplateId", "ApproverName", "ApproverRole", "CitizenId", "CreatedAt", "CreatedBy", "DeletedAt", "DeletedBy", "IsDefault", "IsDeleted", "ReviewNotes", "ReviewedAt", "ReviewedBy", "RowVersion", "Status", "SubmittedDataJson", "TargetSifarisTemplateId", "TargetWardId", "UpdatedAt", "UpdatedBy" },
                values: new object[] { new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), new Guid("99999999-9999-9999-9999-999999999990"), "System Administrator", "Super Admin", new Guid("22222222-2222-2222-2222-222222222222"), new DateTime(2026, 6, 25, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("22222222-2222-2222-2222-222222222222"), null, null, null, false, "Pre-Seeded Documentation for Ward requirements.", new DateTime(2026, 6, 25, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("22222222-2222-2222-2222-222222222222"), new byte[0], 3, "{\"date\": \"2026-06-25\", \"targetWard\": \"वडा नं १\", \"purpose\": \"नागरिकता प्रमाणपत्र बनाउन\", \"citizen.fullname\": \"System Administrator\", \"citizen.citizenshipnumber\": \"123-456-789\", \"citizen.mobilenumber\": \"9800000000\"}", new Guid("99999999-9999-9999-9999-999999999999"), new Guid("00000000-0000-0000-0000-000000000001"), null, null });

            migrationBuilder.InsertData(
                table: "Tenants",
                columns: new[] { "Id", "CreatedAt", "CreatedBy", "DeletedAt", "DeletedBy", "DistrictId", "HasAdminAssigned", "IsActivated", "IsDefault", "IsDeleted", "LtreePath", "MunicipalityId", "Name", "ParentId", "ProvinceId", "RegistrationId", "RowVersion", "TenantType", "UpdatedAt", "UpdatedBy" },
                values: new object[] { new Guid("00000000-0000-0000-0000-000000000001"), new DateTime(2026, 6, 25, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("22222222-2222-2222-2222-222222222222"), null, null, null, true, true, false, false, "00000000000000000000000000000001", null, "Central Government", null, null, null, new byte[0], 1, null, null });

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "IsDefault", "Name", "NormalizedName", "TenantId", "TenantType" },
                values: new object[,]
                {
                    { new Guid("11111111-1111-1111-1111-111111111111"), "11111111-1111-1111-1111-111111111111", true, "Super Admin", "SUPER ADMIN", new Guid("00000000-0000-0000-0000-000000000001"), 1 },
                    { new Guid("22222222-1111-1111-1111-111111111111"), "22222222-1111-1111-1111-111111111111", true, "Chief Minister", "CHIEF MINISTER", new Guid("00000000-0000-0000-0000-000000000001"), 2 },
                    { new Guid("33333333-1111-1111-1111-111111111111"), "33333333-1111-1111-1111-111111111111", true, "District Coordinator", "DISTRICT COORDINATOR", new Guid("00000000-0000-0000-0000-000000000001"), 3 },
                    { new Guid("44444444-4444-4444-4444-444444444444"), "44444444-4444-4444-4444-444444444444", true, "Mayor", "MAYOR", new Guid("00000000-0000-0000-0000-000000000001"), 4 },
                    { new Guid("55555555-5555-5555-5555-555555555555"), "55555555-5555-5555-5555-555555555555", true, "Ward Chairperson", "WARD CHAIRPERSON", new Guid("00000000-0000-0000-0000-000000000001"), 5 },
                    { new Guid("77777777-7777-7777-7777-777777777777"), "77777777-7777-7777-7777-777777777777", true, "Ward Member", "WARD MEMBER", new Guid("00000000-0000-0000-0000-000000000001"), 5 }
                });

            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "BanReason", "ConcurrencyStamp", "Email", "EmailConfirmed", "FullName", "IsBanned", "IsDefault", "LockoutEnabled", "LockoutEnd", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "SecurityStamp", "SuspensionEndDate", "TenantId", "TenantType", "TwoFactorEnabled", "UserName" },
                values: new object[] { new Guid("22222222-2222-2222-2222-222222222222"), 0, null, "66666666-6666-6666-6666-666666666666", "admin@gov.com", true, "System Administrator", false, true, false, null, "ADMIN@GOV.COM", "ADMIN@GOV.COM", "AQAAAAIAAYagAAAAEODdXfoJg9QaIh0KZuwdn5m+jzFPc7tq7ok2hEqqTSkV1L7p1DuI3ppSLI4uG/md0g==", null, false, "55555555-5555-5555-5555-555555555555", null, new Guid("00000000-0000-0000-0000-000000000001"), 1, false, "admin@gov.com" });

            migrationBuilder.InsertData(
                table: "DocumentTemplates",
                columns: new[] { "Id", "CreatedAt", "CreatedBy", "DeletedAt", "DeletedBy", "Description", "FormSchemaJson", "HtmlContent", "IsDefault", "IsDeleted", "LinkedTemplateId", "Name", "OverridesTemplateId", "RowVersion", "TenantId", "TenantType", "Type", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { new Guid("99999999-9999-9999-9999-999999999990"), new DateTime(2026, 6, 25, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("22222222-2222-2222-2222-222222222222"), null, null, "Standard application form format for requesting any official Sifaris.", "{\"fields\": [{\"name\": \"date\", \"label\": \"मिति (Date)\", \"type\": \"date\", \"autoFillKey\": \"system.date\"},{\"name\": \"targetWard\", \"label\": \"सम्बोधन (Target Ward)\", \"type\": \"text\"},{\"name\": \"purpose\", \"label\": \"सिफारिसको प्रयोजन (Purpose)\", \"type\": \"text\"},{\"name\": \"citizen.fullname\", \"label\": \"निवेदकको नाम\", \"type\": \"text\", \"autoFillKey\": \"citizen.fullname\"},{\"name\": \"citizen.citizenshipnumber\", \"label\": \"नागरिकता नं.\", \"type\": \"text\", \"autoFillKey\": \"citizen.citizenshipnumber\"},{\"name\": \"citizen.mobilenumber\", \"label\": \"सम्पर्क नं.\", \"type\": \"text\", \"autoFillKey\": \"citizen.mobilenumber\"}]}", "<div style=\"padding:40px; background:#fff; font-family: 'Kalimati', 'Nepali', sans-serif;\">\n                <div style=\"text-align: right; margin-bottom: 20px;\">\n                    मिति: <strong>{{date}}</strong>\n                </div>\n                <h3 style=\"text-align:center; text-decoration: underline; margin-bottom: 30px;\">विषय: सिफारिस पाऊँ भन्ने बारे।</h3>\n                <p>श्री वडा अध्यक्ष ज्यू,</p>\n                <p>वडा कार्यालय,</p>\n                <p><strong>{{targetWard}}</strong></p>\n                <br/>\n                <p>महोदय,</p>\n                <p style=\"text-indent: 40px; line-height: 1.8;\">\n                    उपरोक्त विषयमा म <strong>{{citizen.fullname}}</strong> (नागरिकता नं. <strong>{{citizen.citizenshipnumber}}</strong>) यस वडाको स्थायी बासिन्दा हुँ। मलाई हाल <strong>{{purpose}}</strong> को प्रयोजनको लागि सम्बन्धित निकायमा पेश गर्न आधिकारिक सिफारिसको आवश्यकता परेको छ।\n                </p>\n                <p style=\"text-indent: 40px; line-height: 1.8;\">\n                    अतः मेरो उल्लेखित विवरण र आवश्यकतालाई मध्यनजर गर्दै, उक्त कार्यको लागि आवश्यक सिफारिस पत्र उपलब्ध गराइदिनुहुन विनम्र अनुरोध गर्दछु। यस निवेदनमा उल्लेखित सम्पूर्ण विवरण सत्य तथ्य हुन्, झुठा ठहरे कानुन बमोजिम सहनेछु।\n                </p>\n                <br/><br/>\n                <div style=\"text-align: right;\">\n                    <p>निवेदक,</p>\n                    <p>हस्ताक्षर: ...........................</p>\n                    <p>नाम: <strong>{{citizen.fullname}}</strong></p>\n                    <p>सम्पर्क नं: <strong>{{citizen.mobilenumber}}</strong></p>\n                </div>\n            </div>", null, false, null, "General Application (Nibedan)", null, new byte[0], new Guid("00000000-0000-0000-0000-000000000001"), 1, 1, null, null },
                    { new Guid("99999999-9999-9999-9999-999999999997"), new DateTime(2026, 6, 25, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("22222222-2222-2222-2222-222222222222"), null, null, "स्थायी बसोबास प्रमाणित गर्ने आधिकारिक पत्र", "{\"fields\": [{\"name\": \"fiscalYear\", \"label\": \"आर्थिक वर्ष (Fiscal Year)\", \"type\": \"text\"},{\"name\": \"dispatchNumber\", \"label\": \"चलानी नं. (Dispatch No.)\", \"type\": \"text\"},{\"name\": \"issueDate\", \"label\": \"मिति (Issue Date)\", \"type\": \"date\", \"autoFillKey\": \"system.date\"},{\"name\": \"permanentAddress\", \"label\": \"स्थायी ठेगाना (Permanent Address)\", \"type\": \"text\"},{\"name\": \"citizen.fullname\", \"label\": \"निवेदकको नाम\", \"type\": \"text\", \"autoFillKey\": \"citizen.fullname\"},{\"name\": \"citizen.citizenshipnumber\", \"label\": \"नागरिकता नं.\", \"type\": \"text\", \"autoFillKey\": \"citizen.citizenshipnumber\"}]}", "<div style=\"padding: 40px; position: relative; background: #fff; font-family: 'Kalimati', 'Nepali', sans-serif;\">\n                <div style=\"text-align: center; margin-bottom: 20px;\">\n                    <h2 style=\"color: #b30000; margin-bottom: 5px; font-weight: bold;\">नेपाल सरकार</h2>\n                    <h3 style=\"margin-top: 0; font-weight: bold;\">स्थानीय सरकार</h3>\n                    <h4 style=\"margin-top: 0;\">वडा कार्यालय</h4>\n                </div>\n                <div style=\"display: flex; justify-content: space-between; margin-bottom: 10px;\">\n                    <div>पत्र संख्या: <strong>{{fiscalYear}}</strong></div>\n                    <div>मिति: <strong>{{issueDate}}</strong></div>\n                </div>\n                <div style=\"margin-bottom: 20px;\">चलानी नं: <strong>{{dispatchNumber}}</strong></div>\n                <hr style=\"border-top: 2px solid #000; margin-bottom: 30px;\"/>\n                <h3 style=\"text-align: center; text-decoration: underline; margin-top: 20px; margin-bottom: 40px; font-weight: bold;\">विषय: स्थायी बसोबास प्रमाणित।</h3>\n                <br/>\n                <p style=\"font-size: 1.2rem; line-height: 1.8; text-indent: 50px; text-align: justify;\">\n                    प्रमाणित गरिन्छ कि श्री <strong>{{citizen.fullname}}</strong> (नागरिकता नं. <strong>{{citizen.citizenshipnumber}}</strong>) यस वडाको स्थायी बासिन्दा हुनुहुन्छ। निजको स्थायी ठेगाना <strong>{{permanentAddress}}</strong> रहेको व्यहोरा अनुरोध छ।\n                </p>\n            </div>", null, false, new Guid("99999999-9999-9999-9999-999999999990"), "स्थायी बसोबास प्रमाणित (Address Verification)", null, new byte[0], new Guid("00000000-0000-0000-0000-000000000001"), 1, 2, null, null },
                    { new Guid("99999999-9999-9999-9999-999999999998"), new DateTime(2026, 6, 25, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("22222222-2222-2222-2222-222222222222"), null, null, "जन्म दर्ता प्रमाणपत्र प्राप्त गर्नको लागि सिफारिस", "{\"fields\": [{\"name\": \"fiscalYear\", \"label\": \"आर्थिक वर्ष (Fiscal Year)\", \"type\": \"text\"},{\"name\": \"dispatchNumber\", \"label\": \"चलानी नं. (Dispatch No.)\", \"type\": \"text\"},{\"name\": \"issueDate\", \"label\": \"मिति (Issue Date)\", \"type\": \"date\", \"autoFillKey\": \"system.date\"},{\"name\": \"fatherName\", \"label\": \"बुबाको नाम (Father Name)\", \"type\": \"text\"},{\"name\": \"motherName\", \"label\": \"आमाको नाम (Mother Name)\", \"type\": \"text\"},{\"name\": \"citizen.fullname\", \"label\": \"बच्चाको नाम (Child Name)\", \"type\": \"text\", \"autoFillKey\": \"citizen.fullname\"},{\"name\": \"citizen.dob\", \"label\": \"जन्म मिति (DOB)\", \"type\": \"date\", \"autoFillKey\": \"citizen.dob\"}]}", "<div style=\"padding: 40px; position: relative; background: #fff; font-family: 'Kalimati', 'Nepali', sans-serif;\">\n                <div style=\"text-align: center; margin-bottom: 20px;\">\n                    <h2 style=\"color: #b30000; margin-bottom: 5px; font-weight: bold;\">नेपाल सरकार</h2>\n                    <h3 style=\"margin-top: 0; font-weight: bold;\">स्थानीय सरकार</h3>\n                    <h4 style=\"margin-top: 0;\">वडा कार्यालय</h4>\n                </div>\n                <div style=\"display: flex; justify-content: space-between; margin-bottom: 10px;\">\n                    <div>पत्र संख्या: <strong>{{fiscalYear}}</strong></div>\n                    <div>मिति: <strong>{{issueDate}}</strong></div>\n                </div>\n                <div style=\"margin-bottom: 20px;\">चलानी नं: <strong>{{dispatchNumber}}</strong></div>\n                <hr style=\"border-top: 2px solid #000; margin-bottom: 30px;\"/>\n                <h3 style=\"text-align: center; text-decoration: underline; margin-top: 20px; margin-bottom: 40px; font-weight: bold;\">विषय: जन्म दर्ता सिफारिस।</h3>\n                <br/>\n                <p style=\"font-size: 1.2rem; line-height: 1.8; text-indent: 50px; text-align: justify;\">\n                    प्रमाणित गरिन्छ कि श्री <strong>{{fatherName}}</strong> र श्रीमती <strong>{{motherName}}</strong> को सन्तानको रुपमा श्री/सुश्री <strong>{{citizen.fullname}}</strong> को मिति <strong>{{citizen.dob}}</strong> मा जन्म भएको व्यहोरा साँचो हो। निजको जन्म दर्ता प्रमाणपत्र उपलब्ध गराउन सिफारिस गरिन्छ।\n                </p>\n            </div>", null, false, new Guid("99999999-9999-9999-9999-999999999990"), "जन्म दर्ता सिफारिस (Birth Registration)", null, new byte[0], new Guid("00000000-0000-0000-0000-000000000001"), 1, 2, null, null },
                    { new Guid("99999999-9999-9999-9999-999999999999"), new DateTime(2026, 6, 25, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("22222222-2222-2222-2222-222222222222"), null, null, "नागरिकता प्रमाणपत्र प्राप्त गर्न वडाबाट दिइने सिफारिस", "{\"fields\": [{\"name\": \"fiscalYear\", \"label\": \"आर्थिक वर्ष (Fiscal Year)\", \"type\": \"text\"},{\"name\": \"dispatchNumber\", \"label\": \"चलानी नं. (Dispatch No.)\", \"type\": \"text\"},{\"name\": \"issueDate\", \"label\": \"मिति (Issue Date)\", \"type\": \"date\", \"autoFillKey\": \"system.date\"},{\"name\": \"issueDistrict\", \"label\": \"जिल्ला (District)\", \"type\": \"text\"},{\"name\": \"fatherName\", \"label\": \"बुबाको नाम (Father Name)\", \"type\": \"text\"},{\"name\": \"citizen.fullname\", \"label\": \"निवेदकको नाम\", \"type\": \"text\", \"autoFillKey\": \"citizen.fullname\"},{\"name\": \"citizen.dob\", \"label\": \"जन्म मिति\", \"type\": \"date\", \"autoFillKey\": \"citizen.dob\"}]}", "<div style=\"padding: 40px; position: relative; background: #fff; font-family: 'Kalimati', 'Nepali', sans-serif;\">\n                <div style=\"text-align: center; margin-bottom: 20px;\">\n                    <h2 style=\"color: #b30000; margin-bottom: 5px; font-weight: bold;\">नेपाल सरकार</h2>\n                    <h3 style=\"margin-top: 0; font-weight: bold;\">स्थानीय सरकार</h3>\n                    <h4 style=\"margin-top: 0;\">वडा कार्यालय</h4>\n                </div>\n                <div style=\"display: flex; justify-content: space-between; margin-bottom: 10px;\">\n                    <div>पत्र संख्या: <strong>{{fiscalYear}}</strong></div>\n                    <div>मिति: <strong>{{issueDate}}</strong></div>\n                </div>\n                <div style=\"margin-bottom: 20px;\">चलानी नं: <strong>{{dispatchNumber}}</strong></div>\n                <hr style=\"border-top: 2px solid #000; margin-bottom: 30px;\"/>\n                <h3 style=\"text-align: center; text-decoration: underline; margin-top: 20px; margin-bottom: 40px; font-weight: bold;\">विषय: नागरिकता प्रमाणपत्र सिफारिस।</h3>\n                <p style=\"font-size: 1.2rem; line-height: 1.8;\">श्री प्रमुख जिल्ला अधिकारी ज्यु, <br/>जिल्ला प्रशासन कार्यालय,<br/><strong>{{issueDistrict}}</strong>।</p>\n                <br/>\n                <p style=\"font-size: 1.2rem; line-height: 1.8; text-indent: 50px; text-align: justify;\">\n                    उपरोक्त विषयमा, यस वडामा स्थायी बसोबास गर्ने श्री <strong>{{fatherName}}</strong> को छोरा/छोरी श्री <strong>{{citizen.fullname}}</strong> (जन्म मिति: <strong>{{citizen.dob}}</strong>) ले नेपाली नागरिकताको प्रमाणपत्र पाऊँ भनि यस कार्यालयमा निवेदन दिनुभएको हुनाले, निजलाई कानुन बमोजिम नेपाली नागरिकताको प्रमाणपत्र उपलब्ध गराइदिनुहुन सिफारिस साथ अनुरोध गर्दछु।\n                </p>\n            </div>", null, false, new Guid("99999999-9999-9999-9999-999999999990"), "नागरिकता प्रमाणपत्र सिफारिस (Citizenship Sifaris)", null, new byte[0], new Guid("00000000-0000-0000-0000-000000000001"), 1, 2, null, null }
                });

            migrationBuilder.InsertData(
                table: "AspNetUserRoles",
                columns: new[] { "RoleId", "UserId" },
                values: new object[] { new Guid("11111111-1111-1111-1111-111111111111"), new Guid("22222222-2222-2222-2222-222222222222") });

            migrationBuilder.InsertData(
                table: "CitizenProfiles",
                columns: new[] { "Id", "CreatedAt", "CreatedBy", "DateOfBirth", "DeletedAt", "DeletedBy", "FingerprintTemplate", "FullName", "Gender", "IsDefault", "IsDeleted", "RegisteredWardId", "RowVersion", "Status", "UpdatedAt", "UpdatedBy" },
                values: new object[] { new Guid("22222222-2222-2222-2222-222222222222"), new DateTime(2026, 6, 25, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("22222222-2222-2222-2222-222222222222"), new DateTime(1990, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, "System Administrator", 1, null, false, new Guid("00000000-0000-0000-0000-000000000001"), new byte[0], 3, null, null });

            migrationBuilder.CreateIndex(
                name: "IX_AlertCampaigns_Status",
                table: "AlertCampaigns",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_AlertCampaigns_TargetTenantId",
                table: "AlertCampaigns",
                column: "TargetTenantId");

            migrationBuilder.CreateIndex(
                name: "IX_ApiConsentRequests_CitizenId",
                table: "ApiConsentRequests",
                column: "CitizenId");

            migrationBuilder.CreateIndex(
                name: "IX_ApiConsentRequests_Status",
                table: "ApiConsentRequests",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_ApiConsentRequests_ThirdPartyClientId",
                table: "ApiConsentRequests",
                column: "ThirdPartyClientId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoles_TenantId",
                table: "AspNetRoles",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_Email",
                table: "AspNetUsers",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_FullName",
                table: "AspNetUsers",
                column: "FullName");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_TenantId",
                table: "AspNetUsers",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CampaignApprovals_AlertCampaignId",
                table: "CampaignApprovals",
                column: "AlertCampaignId");

            migrationBuilder.CreateIndex(
                name: "IX_CampaignDispatches_AlertCampaignId",
                table: "CampaignDispatches",
                column: "AlertCampaignId");

            migrationBuilder.CreateIndex(
                name: "IX_CampaignDispatches_ExternalDispatchId",
                table: "CampaignDispatches",
                column: "ExternalDispatchId");

            migrationBuilder.CreateIndex(
                name: "IX_CitizenProfiles_BirthRegistrationNumber",
                table: "CitizenProfiles",
                column: "BirthRegistrationNumber");

            migrationBuilder.CreateIndex(
                name: "IX_CitizenProfiles_CitizenshipNumber",
                table: "CitizenProfiles",
                column: "CitizenshipNumber");

            migrationBuilder.CreateIndex(
                name: "IX_CitizenProfiles_FaceVectorData",
                table: "CitizenProfiles",
                column: "FaceVectorData")
                .Annotation("Npgsql:IndexMethod", "hnsw")
                .Annotation("Npgsql:IndexOperators", new[] { "vector_cosine_ops" });

            migrationBuilder.CreateIndex(
                name: "IX_CitizenProfiles_MobileNumber",
                table: "CitizenProfiles",
                column: "MobileNumber");

            migrationBuilder.CreateIndex(
                name: "IX_CitizenProfiles_NationalIdNumber",
                table: "CitizenProfiles",
                column: "NationalIdNumber");

            migrationBuilder.CreateIndex(
                name: "IX_CitizenProfiles_RegisteredWardId",
                table: "CitizenProfiles",
                column: "RegisteredWardId");

            migrationBuilder.CreateIndex(
                name: "IX_CitizenProfiles_Status",
                table: "CitizenProfiles",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_DevelopmentPlans_Status",
                table: "DevelopmentPlans",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_DevelopmentPlans_TenantId",
                table: "DevelopmentPlans",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentFiles_OwnerId",
                table: "DocumentFiles",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentTemplates_TenantId",
                table: "DocumentTemplates",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Gunasos_CitizenId",
                table: "Gunasos",
                column: "CitizenId");

            migrationBuilder.CreateIndex(
                name: "IX_Gunasos_Status",
                table: "Gunasos",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Gunasos_TargetTenantId",
                table: "Gunasos",
                column: "TargetTenantId");

            migrationBuilder.CreateIndex(
                name: "IX_MissingPersons_FaceVectorData",
                table: "MissingPersons",
                column: "FaceVectorData")
                .Annotation("Npgsql:IndexMethod", "hnsw")
                .Annotation("Npgsql:IndexOperators", new[] { "vector_cosine_ops" });

            migrationBuilder.CreateIndex(
                name: "IX_MissingPersons_IsFound",
                table: "MissingPersons",
                column: "IsFound");

            migrationBuilder.CreateIndex(
                name: "IX_MissingPersons_ReportedByWardId",
                table: "MissingPersons",
                column: "ReportedByWardId");

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessages_OccurredOn",
                table: "OutboxMessages",
                column: "OccurredOn");

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessages_ProcessedOn_RetryCount_LockedOn",
                table: "OutboxMessages",
                columns: new[] { "ProcessedOn", "RetryCount", "LockedOn" });

            migrationBuilder.CreateIndex(
                name: "IX_RolePermissions_RoleId",
                table: "RolePermissions",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_Sifaris_CitizenId",
                table: "Sifaris",
                column: "CitizenId");

            migrationBuilder.CreateIndex(
                name: "IX_Sifaris_Status",
                table: "Sifaris",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Sifaris_WardId",
                table: "Sifaris",
                column: "WardId");

            migrationBuilder.CreateIndex(
                name: "IX_SifarisApplications_CitizenId",
                table: "SifarisApplications",
                column: "CitizenId");

            migrationBuilder.CreateIndex(
                name: "IX_SifarisApplications_Status",
                table: "SifarisApplications",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_SifarisApplications_TargetSifarisTemplateId",
                table: "SifarisApplications",
                column: "TargetSifarisTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_SifarisApplications_TargetWardId",
                table: "SifarisApplications",
                column: "TargetWardId");

            migrationBuilder.CreateIndex(
                name: "IX_Tenants_ParentId",
                table: "Tenants",
                column: "ParentId");
            migrationBuilder.Sql(@"
    CREATE OR REPLACE FUNCTION notify_outbox_messages() RETURNS trigger AS $$
    BEGIN
      PERFORM pg_notify('outbox_messages', NEW.""Id""::text);
      RETURN NEW;
    END;
    $$ LANGUAGE plpgsql;

    CREATE TRIGGER outbox_messages_trigger
    AFTER INSERT ON ""OutboxMessages""
    FOR EACH ROW EXECUTE FUNCTION notify_outbox_messages();
");

            migrationBuilder.Sql("CREATE INDEX IX_Tenants_LtreePath ON \"Tenants\" USING GIST (\"LtreePath\");");

            var filePath = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "wwwroot", "data", "tenants.json");
            if (!System.IO.File.Exists(filePath)) filePath = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "..", "Web", "wwwroot", "data", "tenants.json");
            if (!System.IO.File.Exists(filePath)) filePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "wwwroot", "data", "tenants.json");

            if (System.IO.File.Exists(filePath))
            {
                var json = System.IO.File.ReadAllText(filePath);
                var tenants = JsonSerializer.Deserialize<System.Collections.Generic.List<Infrastructure.Data.TenantSeedDto>>(json);

                if (tenants != null && tenants.Count > 0)
                {
                    var columns = new[]
                    {
            "Id", "Name", "TenantType", "LtreePath", "ParentId", "ProvinceId",
            "DistrictId", "MunicipalityId", "CreatedBy", "CreatedAt",
            "IsDeleted", "IsDefault", "RowVersion", "IsActivated", "HasAdminAssigned"
        };

                    var values = new object[tenants.Count, columns.Length];
                    var tenantDict = new System.Collections.Generic.Dictionary<string, string>();

                    for (int i = 0; i < tenants.Count; i++)
                    {
                        var t = tenants[i];
                        string currentLtreeId = Guid.Parse(t.Id).ToString("N");
                        string path = currentLtreeId;

                        if (!string.IsNullOrEmpty(t.ParentId))
                        {
                            if (tenantDict.TryGetValue(t.ParentId, out var parentPath))
                            {
                                path = $"{parentPath}.{currentLtreeId}";
                            }
                            else
                            {
                                path = $"{Guid.Parse(t.ParentId):N}.{currentLtreeId}";
                            }
                        }

                        tenantDict[t.Id] = path;

                        values[i, 0] = Guid.Parse(t.Id);
                        values[i, 1] = t.Name;
                        values[i, 2] = t.TenantType;
                        values[i, 3] = path;
                        values[i, 4] = string.IsNullOrEmpty(t.ParentId) ? (object)null! : Guid.Parse(t.ParentId);
                        values[i, 5] = string.IsNullOrEmpty(t.ProvinceId) ? (object)null! : Guid.Parse(t.ProvinceId);
                        values[i, 6] = string.IsNullOrEmpty(t.DistrictId) ? (object)null! : Guid.Parse(t.DistrictId);
                        values[i, 7] = string.IsNullOrEmpty(t.MunicipalityId) ? (object)null! : Guid.Parse(t.MunicipalityId);
                        values[i, 8] = Guid.Parse(t.CreatedBy);
                        values[i, 9] = DateTimeOffset.Parse(t.CreatedAt).UtcDateTime;
                        values[i, 10] = t.IsDeleted;
                        values[i, 11] = t.IsDefault;
                        values[i, 12] = Array.Empty<byte>();
                        values[i, 13] = false;
                        values[i, 14] = false;
                    }

                    migrationBuilder.InsertData(table: "Tenants", columns: columns, values: values);
                }
            }
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ApiConsentRequests");

            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "CampaignApprovals");

            migrationBuilder.DropTable(
                name: "CampaignDispatches");

            migrationBuilder.DropTable(
                name: "CitizenProfiles");

            migrationBuilder.DropTable(
                name: "DevelopmentPlans");

            migrationBuilder.DropTable(
                name: "DocumentFiles");

            migrationBuilder.DropTable(
                name: "DocumentTemplates");

            migrationBuilder.DropTable(
                name: "Gunasos");

            migrationBuilder.DropTable(
                name: "MissingPersons");

            migrationBuilder.DropTable(
                name: "OutboxMessages");

            migrationBuilder.DropTable(
                name: "RolePermissions");

            migrationBuilder.DropTable(
                name: "Sifaris");

            migrationBuilder.DropTable(
                name: "SifarisApplications");

            migrationBuilder.DropTable(
                name: "TenantSecurityPolicies");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "AlertCampaigns");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "Tenants");

            migrationBuilder.Sql("DROP FUNCTION IF EXISTS notify_outbox_messages();");
        }
    }
}
