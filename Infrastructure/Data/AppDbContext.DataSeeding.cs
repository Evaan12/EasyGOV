using Domain.Entities;
using Domain.Enums;
using Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;

namespace Infrastructure.Data
{
    public partial class AppDbContext
    {
        private void SeedData(ModelBuilder builder)
        {
            var adminUserId = Guid.Parse("22222222-2222-2222-2222-222222222222");
            var seedDate = new DateTime(2026, 6, 25, 0, 0, 0, DateTimeKind.Utc);

            var centralTenantId = Guid.Parse("00000000-0000-0000-0000-000000000001");

            builder.Entity<Tenant>().HasData(
                new
                {
                    Id = centralTenantId,
                    Name = "Central Government",
                    TenantType = TenantType.Central,
                    IsActivated = true,
                    LtreePath = centralTenantId.ToString("N"),
                    HasAdminAssigned = true,
                    CreatedBy = adminUserId,
                    CreatedAt = seedDate,
                    IsDeleted = false,
                    RowVersion = Array.Empty<byte>(),
                    IsDefault = (bool?)false
                }
            );

            var adminRoleId = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var chiefMinisterRoleId = Guid.Parse("22222222-1111-1111-1111-111111111111");
            var districtCoordRoleId = Guid.Parse("33333333-1111-1111-1111-111111111111");
            var mayorRoleId = Guid.Parse("44444444-4444-4444-4444-444444444444");
            var wardChairRoleId = Guid.Parse("55555555-5555-5555-5555-555555555555");
            var wardMemberRoleId = Guid.Parse("77777777-7777-7777-7777-777777777777");

            builder.Entity<AppRole>().HasData(
                new AppRole
                {
                    Id = adminRoleId,
                    Name = "Super Admin",
                    NormalizedName = "SUPER ADMIN",
                    ConcurrencyStamp = adminRoleId.ToString(),
                    TenantType = TenantType.Central,
                    TenantId = centralTenantId,
                    IsDefault = true
                },
                new AppRole
                {
                    Id = chiefMinisterRoleId,
                    Name = "Chief Minister",
                    NormalizedName = "CHIEF MINISTER",
                    ConcurrencyStamp = chiefMinisterRoleId.ToString(),
                    TenantType = TenantType.Province,
                    TenantId = centralTenantId,
                    IsDefault = true
                },
                new AppRole
                {
                    Id = districtCoordRoleId,
                    Name = "District Coordinator",
                    NormalizedName = "DISTRICT COORDINATOR",
                    ConcurrencyStamp = districtCoordRoleId.ToString(),
                    TenantType = TenantType.District,
                    TenantId = centralTenantId,
                    IsDefault = true
                },
                new AppRole
                {
                    Id = mayorRoleId,
                    Name = "Mayor",
                    NormalizedName = "MAYOR",
                    ConcurrencyStamp = mayorRoleId.ToString(),
                    TenantType = TenantType.Municipality,
                    TenantId = centralTenantId,
                    IsDefault = true
                },
                new AppRole
                {
                    Id = wardChairRoleId,
                    Name = "Ward Chairperson",
                    NormalizedName = "WARD CHAIRPERSON",
                    ConcurrencyStamp = wardChairRoleId.ToString(),
                    TenantType = TenantType.Ward,
                    TenantId = centralTenantId,
                    IsDefault = true
                },
                new AppRole
                {
                    Id = wardMemberRoleId,
                    Name = "Ward Member",
                    NormalizedName = "WARD MEMBER",
                    ConcurrencyStamp = wardMemberRoleId.ToString(),
                    TenantType = TenantType.Ward,
                    TenantId = centralTenantId,
                    IsDefault = true
                }
            );

            var adminUser = new AppUser
            {
                Id = adminUserId,
                UserName = "admin@gov.com",
                NormalizedUserName = "ADMIN@GOV.COM",
                Email = "admin@gov.com",
                NormalizedEmail = "ADMIN@GOV.COM",
                EmailConfirmed = true,
                FullName = "System Administrator",
                SecurityStamp = "55555555-5555-5555-5555-555555555555",
                ConcurrencyStamp = "66666666-6666-6666-6666-666666666666",
                PasswordHash = "AQAAAAIAAYagAAAAEODdXfoJg9QaIh0KZuwdn5m+jzFPc7tq7ok2hEqqTSkV1L7p1DuI3ppSLI4uG/md0g==", // admin
                TenantType = TenantType.Central,
                TenantId = centralTenantId,
                IsDefault = true
            };

            builder.Entity<AppUser>().HasData(adminUser);

            builder.Entity<IdentityUserRole<Guid>>().HasData(new IdentityUserRole<Guid>
            {
                RoleId = adminRoleId,
                UserId = adminUserId
            });

            var allPermissions = new List<object>();

            // Super Admin unrestricted access mapping
            allPermissions.Add(new
            {
                Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                RoleId = adminRoleId,
                ResourceType = ResourceType.Admin,
                ActionType = ActionType.Admin,
                CreatedBy = adminUserId,
                CreatedAt = seedDate,
                IsDeleted = false,
                RowVersion = Array.Empty<byte>(),
                IsDefault = (bool?)true
            });

            // Head Roles Full Granular Mapping
            var headRoles = new[] { chiefMinisterRoleId, districtCoordRoleId, mayorRoleId, wardChairRoleId };
            var resourcePermissions = new List<(ResourceType, ActionType)>
            {
                (ResourceType.User, ActionType.Read | ActionType.Create | ActionType.Update | ActionType.Delete),
                (ResourceType.Role, ActionType.Read | ActionType.Create | ActionType.Update | ActionType.Delete | ActionType.Assign),
                (ResourceType.SubTenant, ActionType.Read | ActionType.Create | ActionType.Update | ActionType.Delete | ActionType.Activate),
                (ResourceType.Sifaris, ActionType.Read | ActionType.Create | ActionType.Approve | ActionType.Revoke | ActionType.Export),
                (ResourceType.CitizenProfile, ActionType.Read | ActionType.Update | ActionType.Delete),
                (ResourceType.BiometricEnrollment, ActionType.Create | ActionType.Read | ActionType.Update | ActionType.Delete),
                (ResourceType.MissingPerson, ActionType.Create | ActionType.Read | ActionType.Update | ActionType.Delete),
                (ResourceType.AlertCampaign, ActionType.Read | ActionType.Create | ActionType.Update | ActionType.Delete | ActionType.Approve),
                (ResourceType.CampaignDispatch, ActionType.Read),
                (ResourceType.Gunaso, ActionType.Read | ActionType.Update | ActionType.Delete),
                (ResourceType.DevelopmentPlan, ActionType.Read | ActionType.Create | ActionType.Update | ActionType.Delete)
            };

            foreach (var roleId in headRoles)
            {
                foreach (var perm in resourcePermissions)
                {
                    // Deterministic Guid formulation
                    byte[] bytes = roleId.ToByteArray();
                    bytes[0] = (byte)((int)perm.Item1 % 256);
                    bytes[1] = (byte)((int)perm.Item1 >> 8);

                    allPermissions.Add(new
                    {
                        Id = new Guid(bytes),
                        RoleId = roleId,
                        ResourceType = perm.Item1,
                        ActionType = perm.Item2,
                        CreatedBy = adminUserId,
                        CreatedAt = seedDate,
                        IsDeleted = false,
                        RowVersion = Array.Empty<byte>(),
                        IsDefault = (bool?)true
                    });
                }
            }

            // Standard Ward Member Minimal Mapping
            byte[] wmBytes = wardMemberRoleId.ToByteArray();
            wmBytes[0] = (byte)((int)ResourceType.CitizenProfile % 256);
            wmBytes[1] = (byte)((int)ResourceType.CitizenProfile >> 8);

            allPermissions.Add(new
            {
                Id = new Guid(wmBytes),
                RoleId = wardMemberRoleId,
                ResourceType = ResourceType.CitizenProfile,
                ActionType = ActionType.Read,
                CreatedBy = adminUserId,
                CreatedAt = seedDate,
                IsDeleted = false,
                RowVersion = Array.Empty<byte>(),
                IsDefault = (bool?)true
            });

            builder.Entity<RolePermission>().HasData(allPermissions.ToArray());

            // Seed Document Templates (Nepali)
            var nibedanTemplateId = Guid.Parse("99999999-9999-9999-9999-999999999990");
            var schemaNibedan = "{\"fields\": [{\"name\": \"date\", \"label\": \"मिति (Date)\", \"type\": \"date\", \"autoFillKey\": \"system.date\"},{\"name\": \"targetWard\", \"label\": \"सम्बोधन (Target Ward)\", \"type\": \"text\"},{\"name\": \"purpose\", \"label\": \"सिफारिसको प्रयोजन (Purpose)\", \"type\": \"text\"},{\"name\": \"citizen.fullname\", \"label\": \"निवेदकको नाम\", \"type\": \"text\", \"autoFillKey\": \"citizen.fullname\"},{\"name\": \"citizen.citizenshipnumber\", \"label\": \"नागरिकता नं.\", \"type\": \"text\", \"autoFillKey\": \"citizen.citizenshipnumber\"},{\"name\": \"citizen.mobilenumber\", \"label\": \"सम्पर्क नं.\", \"type\": \"text\", \"autoFillKey\": \"citizen.mobilenumber\"}]}";
            var htmlNibedan = @"<div style=""padding:40px; background:#fff; font-family: 'Kalimati', 'Nepali', sans-serif;"">
                <div style=""text-align: right; margin-bottom: 20px;"">
                    मिति: <strong>{{date}}</strong>
                </div>
                <h3 style=""text-align:center; text-decoration: underline; margin-bottom: 30px;"">विषय: सिफारिस पाऊँ भन्ने बारे।</h3>
                <p>श्री वडा अध्यक्ष ज्यू,</p>
                <p>वडा कार्यालय,</p>
                <p><strong>{{targetWard}}</strong></p>
                <br/>
                <p>महोदय,</p>
                <p style=""text-indent: 40px; line-height: 1.8;"">
                    उपरोक्त विषयमा म <strong>{{citizen.fullname}}</strong> (नागरिकता नं. <strong>{{citizen.citizenshipnumber}}</strong>) यस वडाको स्थायी बासिन्दा हुँ। मलाई हाल <strong>{{purpose}}</strong> को प्रयोजनको लागि सम्बन्धित निकायमा पेश गर्न आधिकारिक सिफारिसको आवश्यकता परेको छ।
                </p>
                <p style=""text-indent: 40px; line-height: 1.8;"">
                    अतः मेरो उल्लेखित विवरण र आवश्यकतालाई मध्यनजर गर्दै, उक्त कार्यको लागि आवश्यक सिफारिस पत्र उपलब्ध गराइदिनुहुन विनम्र अनुरोध गर्दछु। यस निवेदनमा उल्लेखित सम्पूर्ण विवरण सत्य तथ्य हुन्, झुठा ठहरे कानुन बमोजिम सहनेछु।
                </p>
                <br/><br/>
                <div style=""text-align: right;"">
                    <p>निवेदक,</p>
                    <p>हस्ताक्षर: ...........................</p>
                    <p>नाम: <strong>{{citizen.fullname}}</strong></p>
                    <p>सम्पर्क नं: <strong>{{citizen.mobilenumber}}</strong></p>
                </div>
            </div>";

            var template1Id = Guid.Parse("99999999-9999-9999-9999-999999999999");
            var schema1Json = "{\"fields\": [{\"name\": \"fiscalYear\", \"label\": \"आर्थिक वर्ष (Fiscal Year)\", \"type\": \"text\"},{\"name\": \"dispatchNumber\", \"label\": \"चलानी नं. (Dispatch No.)\", \"type\": \"text\"},{\"name\": \"issueDate\", \"label\": \"मिति (Issue Date)\", \"type\": \"date\", \"autoFillKey\": \"system.date\"},{\"name\": \"issueDistrict\", \"label\": \"जिल्ला (District)\", \"type\": \"text\"},{\"name\": \"fatherName\", \"label\": \"बुबाको नाम (Father Name)\", \"type\": \"text\"},{\"name\": \"citizen.fullname\", \"label\": \"निवेदकको नाम\", \"type\": \"text\", \"autoFillKey\": \"citizen.fullname\"},{\"name\": \"citizen.dob\", \"label\": \"जन्म मिति\", \"type\": \"date\", \"autoFillKey\": \"citizen.dob\"}]}";
            var htmlContent1 = @"<div style=""padding: 40px; position: relative; background: #fff; font-family: 'Kalimati', 'Nepali', sans-serif;"">
                <div style=""text-align: center; margin-bottom: 20px;"">
                    <h2 style=""color: #b30000; margin-bottom: 5px; font-weight: bold;"">नेपाल सरकार</h2>
                    <h3 style=""margin-top: 0; font-weight: bold;"">स्थानीय सरकार</h3>
                    <h4 style=""margin-top: 0;"">वडा कार्यालय</h4>
                </div>
                <div style=""display: flex; justify-content: space-between; margin-bottom: 10px;"">
                    <div>पत्र संख्या: <strong>{{fiscalYear}}</strong></div>
                    <div>मिति: <strong>{{issueDate}}</strong></div>
                </div>
                <div style=""margin-bottom: 20px;"">चलानी नं: <strong>{{dispatchNumber}}</strong></div>
                <hr style=""border-top: 2px solid #000; margin-bottom: 30px;""/>
                <h3 style=""text-align: center; text-decoration: underline; margin-top: 20px; margin-bottom: 40px; font-weight: bold;"">विषय: नागरिकता प्रमाणपत्र सिफारिस।</h3>
                <p style=""font-size: 1.2rem; line-height: 1.8;"">श्री प्रमुख जिल्ला अधिकारी ज्यु, <br/>जिल्ला प्रशासन कार्यालय,<br/><strong>{{issueDistrict}}</strong>।</p>
                <br/>
                <p style=""font-size: 1.2rem; line-height: 1.8; text-indent: 50px; text-align: justify;"">
                    उपरोक्त विषयमा, यस वडामा स्थायी बसोबास गर्ने श्री <strong>{{fatherName}}</strong> को छोरा/छोरी श्री <strong>{{citizen.fullname}}</strong> (जन्म मिति: <strong>{{citizen.dob}}</strong>) ले नेपाली नागरिकताको प्रमाणपत्र पाऊँ भनि यस कार्यालयमा निवेदन दिनुभएको हुनाले, निजलाई कानुन बमोजिम नेपाली नागरिकताको प्रमाणपत्र उपलब्ध गराइदिनुहुन सिफारिस साथ अनुरोध गर्दछु।
                </p>
            </div>";

            var template2Id = Guid.Parse("99999999-9999-9999-9999-999999999998");
            var schema2Json = "{\"fields\": [{\"name\": \"fiscalYear\", \"label\": \"आर्थिक वर्ष (Fiscal Year)\", \"type\": \"text\"},{\"name\": \"dispatchNumber\", \"label\": \"चलानी नं. (Dispatch No.)\", \"type\": \"text\"},{\"name\": \"issueDate\", \"label\": \"मिति (Issue Date)\", \"type\": \"date\", \"autoFillKey\": \"system.date\"},{\"name\": \"fatherName\", \"label\": \"बुबाको नाम (Father Name)\", \"type\": \"text\"},{\"name\": \"motherName\", \"label\": \"आमाको नाम (Mother Name)\", \"type\": \"text\"},{\"name\": \"citizen.fullname\", \"label\": \"बच्चाको नाम (Child Name)\", \"type\": \"text\", \"autoFillKey\": \"citizen.fullname\"},{\"name\": \"citizen.dob\", \"label\": \"जन्म मिति (DOB)\", \"type\": \"date\", \"autoFillKey\": \"citizen.dob\"}]}";
            var htmlContent2 = @"<div style=""padding: 40px; position: relative; background: #fff; font-family: 'Kalimati', 'Nepali', sans-serif;"">
                <div style=""text-align: center; margin-bottom: 20px;"">
                    <h2 style=""color: #b30000; margin-bottom: 5px; font-weight: bold;"">नेपाल सरकार</h2>
                    <h3 style=""margin-top: 0; font-weight: bold;"">स्थानीय सरकार</h3>
                    <h4 style=""margin-top: 0;"">वडा कार्यालय</h4>
                </div>
                <div style=""display: flex; justify-content: space-between; margin-bottom: 10px;"">
                    <div>पत्र संख्या: <strong>{{fiscalYear}}</strong></div>
                    <div>मिति: <strong>{{issueDate}}</strong></div>
                </div>
                <div style=""margin-bottom: 20px;"">चलानी नं: <strong>{{dispatchNumber}}</strong></div>
                <hr style=""border-top: 2px solid #000; margin-bottom: 30px;""/>
                <h3 style=""text-align: center; text-decoration: underline; margin-top: 20px; margin-bottom: 40px; font-weight: bold;"">विषय: जन्म दर्ता सिफारिस।</h3>
                <br/>
                <p style=""font-size: 1.2rem; line-height: 1.8; text-indent: 50px; text-align: justify;"">
                    प्रमाणित गरिन्छ कि श्री <strong>{{fatherName}}</strong> र श्रीमती <strong>{{motherName}}</strong> को सन्तानको रुपमा श्री/सुश्री <strong>{{citizen.fullname}}</strong> को मिति <strong>{{citizen.dob}}</strong> मा जन्म भएको व्यहोरा साँचो हो। निजको जन्म दर्ता प्रमाणपत्र उपलब्ध गराउन सिफारिस गरिन्छ।
                </p>
            </div>";

            var template3Id = Guid.Parse("99999999-9999-9999-9999-999999999997");
            var schema3Json = "{\"fields\": [{\"name\": \"fiscalYear\", \"label\": \"आर्थिक वर्ष (Fiscal Year)\", \"type\": \"text\"},{\"name\": \"dispatchNumber\", \"label\": \"चलानी नं. (Dispatch No.)\", \"type\": \"text\"},{\"name\": \"issueDate\", \"label\": \"मिति (Issue Date)\", \"type\": \"date\", \"autoFillKey\": \"system.date\"},{\"name\": \"permanentAddress\", \"label\": \"स्थायी ठेगाना (Permanent Address)\", \"type\": \"text\"},{\"name\": \"citizen.fullname\", \"label\": \"निवेदकको नाम\", \"type\": \"text\", \"autoFillKey\": \"citizen.fullname\"},{\"name\": \"citizen.citizenshipnumber\", \"label\": \"नागरिकता नं.\", \"type\": \"text\", \"autoFillKey\": \"citizen.citizenshipnumber\"}]}";
            var htmlContent3 = @"<div style=""padding: 40px; position: relative; background: #fff; font-family: 'Kalimati', 'Nepali', sans-serif;"">
                <div style=""text-align: center; margin-bottom: 20px;"">
                    <h2 style=""color: #b30000; margin-bottom: 5px; font-weight: bold;"">नेपाल सरकार</h2>
                    <h3 style=""margin-top: 0; font-weight: bold;"">स्थानीय सरकार</h3>
                    <h4 style=""margin-top: 0;"">वडा कार्यालय</h4>
                </div>
                <div style=""display: flex; justify-content: space-between; margin-bottom: 10px;"">
                    <div>पत्र संख्या: <strong>{{fiscalYear}}</strong></div>
                    <div>मिति: <strong>{{issueDate}}</strong></div>
                </div>
                <div style=""margin-bottom: 20px;"">चलानी नं: <strong>{{dispatchNumber}}</strong></div>
                <hr style=""border-top: 2px solid #000; margin-bottom: 30px;""/>
                <h3 style=""text-align: center; text-decoration: underline; margin-top: 20px; margin-bottom: 40px; font-weight: bold;"">विषय: स्थायी बसोबास प्रमाणित।</h3>
                <br/>
                <p style=""font-size: 1.2rem; line-height: 1.8; text-indent: 50px; text-align: justify;"">
                    प्रमाणित गरिन्छ कि श्री <strong>{{citizen.fullname}}</strong> (नागरिकता नं. <strong>{{citizen.citizenshipnumber}}</strong>) यस वडाको स्थायी बासिन्दा हुनुहुन्छ। निजको स्थायी ठेगाना <strong>{{permanentAddress}}</strong> रहेको व्यहोरा अनुरोध छ।
                </p>
            </div>";

            builder.Entity<DocumentTemplate>().HasData(
                new
                {
                    Id = nibedanTemplateId,
                    Type = TemplateType.SifarisApplication,
                    Name = "General Application (Nibedan)",
                    Description = "Standard application form format for requesting any official Sifaris.",
                    FormSchemaJson = schemaNibedan,
                    HtmlContent = htmlNibedan,
                    TenantType = TenantType.Central,
                    TenantId = centralTenantId,
                    OverridesTemplateId = (Guid?)null,
                    LinkedTemplateId = (Guid?)null,
                    CreatedBy = adminUserId,
                    CreatedAt = seedDate,
                    IsDeleted = false,
                    RowVersion = Array.Empty<byte>()
                },
                new
                {
                    Id = template1Id,
                    Type = TemplateType.SifarisCertificate,
                    Name = "नागरिकता प्रमाणपत्र सिफारिस (Citizenship Sifaris)",
                    Description = "नागरिकता प्रमाणपत्र प्राप्त गर्न वडाबाट दिइने सिफारिस",
                    FormSchemaJson = schema1Json,
                    HtmlContent = htmlContent1,
                    TenantType = TenantType.Central,
                    TenantId = centralTenantId,
                    OverridesTemplateId = (Guid?)null,
                    LinkedTemplateId = (Guid?)nibedanTemplateId,
                    CreatedBy = adminUserId,
                    CreatedAt = seedDate,
                    IsDeleted = false,
                    RowVersion = Array.Empty<byte>()
                },
                new
                {
                    Id = template2Id,
                    Type = TemplateType.SifarisCertificate,
                    Name = "जन्म दर्ता सिफारिस (Birth Registration)",
                    Description = "जन्म दर्ता प्रमाणपत्र प्राप्त गर्नको लागि सिफारिस",
                    FormSchemaJson = schema2Json,
                    HtmlContent = htmlContent2,
                    TenantType = TenantType.Central,
                    TenantId = centralTenantId,
                    OverridesTemplateId = (Guid?)null,
                    LinkedTemplateId = (Guid?)nibedanTemplateId,
                    CreatedBy = adminUserId,
                    CreatedAt = seedDate,
                    IsDeleted = false,
                    RowVersion = Array.Empty<byte>()
                },
                new
                {
                    Id = template3Id,
                    Type = TemplateType.SifarisCertificate,
                    Name = "स्थायी बसोबास प्रमाणित (Address Verification)",
                    Description = "स्थायी बसोबास प्रमाणित गर्ने आधिकारिक पत्र",
                    FormSchemaJson = schema3Json,
                    HtmlContent = htmlContent3,
                    TenantType = TenantType.Central,
                    TenantId = centralTenantId,
                    OverridesTemplateId = (Guid?)null,
                    LinkedTemplateId = (Guid?)nibedanTemplateId,
                    CreatedBy = adminUserId,
                    CreatedAt = seedDate,
                    IsDeleted = false,
                    RowVersion = Array.Empty<byte>()
                }
            );

            // Adding a Base Profile and Seeded Application/Sifaris Document 
            builder.Entity<CitizenProfile>().HasData(new
            {
                Id = adminUserId,
                FullName = "System Administrator",
                DateOfBirth = new DateTime(1990, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                Gender = Gender.Male,
                RegisteredWardId = centralTenantId,
                Status = CitizenStatus.Active,
                CreatedBy = adminUserId,
                CreatedAt = seedDate,
                IsDeleted = false,
                RowVersion = Array.Empty<byte>()
            });

            var appId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
            builder.Entity<SifarisApplication>().HasData(new
            {
                Id = appId,
                CitizenId = adminUserId,
                TargetWardId = centralTenantId,
                ApplicationTemplateId = nibedanTemplateId,
                TargetSifarisTemplateId = template1Id,
                SubmittedDataJson = "{\"date\": \"2026-06-25\", \"targetWard\": \"वडा नं १\", \"purpose\": \"नागरिकता प्रमाणपत्र बनाउन\", \"citizen.fullname\": \"System Administrator\", \"citizen.citizenshipnumber\": \"123-456-789\", \"citizen.mobilenumber\": \"9800000000\"}",
                Status = ApplicationStatus.Approved,
                ReviewNotes = "Pre-Seeded Documentation for Ward requirements.",
                ReviewedBy = adminUserId,
                ReviewedAt = seedDate,
                ApproverName = "System Administrator",
                ApproverRole = "Super Admin",
                CreatedBy = adminUserId,
                CreatedAt = seedDate,
                IsDeleted = false,
                RowVersion = Array.Empty<byte>()
            });

            var sifarisId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
            builder.Entity<Sifaris>().HasData(new
            {
                Id = sifarisId,
                CitizenId = adminUserId,
                WardId = centralTenantId,
                SifarisTemplateId = template1Id,
                ApplicationId = appId,
                ProfileHashAtIssuance = "dummyhash",
                SnapshotDataJson = "{\"fiscalYear\": \"2082/83\", \"dispatchNumber\": \"105\", \"issueDate\": \"2026-06-25\", \"issueDistrict\": \"Kathmandu\", \"fatherName\": \"Shyam Admin\", \"citizen.fullname\": \"System Administrator\", \"citizen.dob\": \"1990-01-01\"}",
                Status = SifarisStatus.Approved,
                ApproverName = "System Administrator",
                ApproverRole = "Super Admin",
                CreatedBy = adminUserId,
                CreatedAt = seedDate,
                IsDeleted = false,
                RowVersion = Array.Empty<byte>()
            });
        }
    }
}