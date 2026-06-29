using Domain.Entities;
using Domain.Exceptions;
using Domain.Repositories;
using Mediator;
using System;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.DocumentTemplates.Queries
{
    public record SifarisTemplateDetailsDto(Guid TemplateId, string Name, string HtmlContent, string PrefilledSchemaJson);

    public record GetSifarisTemplateForUserQuery(Guid TemplateId, Guid CitizenId) : IRequest<SifarisTemplateDetailsDto>;

    public class GetSifarisTemplateForUserQueryHandler : IRequestHandler<GetSifarisTemplateForUserQuery, SifarisTemplateDetailsDto>
    {
        private readonly IDocumentTemplateRepository _templateRepository;
        private readonly ICitizenProfileRepository _profileRepository;

        public GetSifarisTemplateForUserQueryHandler(IDocumentTemplateRepository templateRepository, ICitizenProfileRepository profileRepository)
        {
            _templateRepository = templateRepository;
            _profileRepository = profileRepository;
        }

        public async Task<SifarisTemplateDetailsDto> Handle(GetSifarisTemplateForUserQuery request, CancellationToken cancellationToken)
        {
            var template = await _templateRepository.GetByIdAsync(request.TemplateId, cancellationToken);
            if (template == null) throw new DomainException("Requested template not found.");

            var profile = await _profileRepository.GetByIdAsync(request.CitizenId, cancellationToken);

            string prefilledSchema = template.FormSchemaJson;

            try
            {
                var schemaNode = JsonNode.Parse(template.FormSchemaJson);
                if (schemaNode != null && schemaNode["fields"] is JsonArray fields)
                {
                    foreach (var field in fields)
                    {
                        var autoFillKey = field["autoFillKey"]?.ToString();
                        if (!string.IsNullOrEmpty(autoFillKey))
                        {
                            field["defaultValue"] = GetAutoFillValue(profile, autoFillKey);
                        }
                    }
                    prefilledSchema = schemaNode.ToJsonString();
                }
            }
            catch { }

            return new SifarisTemplateDetailsDto(template.Id, template.Name, template.HtmlContent, prefilledSchema);
        }

        private string GetAutoFillValue(CitizenProfile? profile, string key)
        {
            if (key.Equals("system.date", StringComparison.OrdinalIgnoreCase))
                return DateTime.UtcNow.ToString("yyyy-MM-dd");

            if (profile == null) return "";

            return key.ToLowerInvariant() switch
            {
                "citizen.fullname" => profile.FullName,
                "citizen.dob" => profile.DateOfBirth.ToString("yyyy-MM-dd"),
                "citizen.gender" => profile.Gender.ToString(),
                "citizen.citizenshipnumber" => profile.Citizenship?.CitizenshipNumber ?? "",
                "citizen.mobilenumber" => profile.MobileNumber?.Value ?? "",
                _ => ""
            };
        }
    }

    public record GetNibedanTemplateForSifarisQuery(Guid SifarisTemplateId, Guid CitizenId) : IRequest<SifarisTemplateDetailsDto>;

    public class GetNibedanTemplateForSifarisQueryHandler : IRequestHandler<GetNibedanTemplateForSifarisQuery, SifarisTemplateDetailsDto>
    {
        private readonly IDocumentTemplateRepository _templateRepository;
        private readonly ICitizenProfileRepository _profileRepository;

        public GetNibedanTemplateForSifarisQueryHandler(IDocumentTemplateRepository templateRepository, ICitizenProfileRepository profileRepository)
        {
            _templateRepository = templateRepository;
            _profileRepository = profileRepository;
        }

        public async Task<SifarisTemplateDetailsDto> Handle(GetNibedanTemplateForSifarisQuery request, CancellationToken cancellationToken)
        {
            var sifarisTemplate = await _templateRepository.GetByIdAsync(request.SifarisTemplateId, cancellationToken);
            if (sifarisTemplate == null || sifarisTemplate.LinkedTemplateId == null)
                throw new DomainException("The selected Sifaris configuration is missing a linked Application (Nibedan) template.");

            var nibedanTemplate = await _templateRepository.GetByIdAsync(sifarisTemplate.LinkedTemplateId.Value, cancellationToken);
            if (nibedanTemplate == null) throw new DomainException("Linked Nibedan template not found.");

            var profile = await _profileRepository.GetByIdAsync(request.CitizenId, cancellationToken);

            string prefilledSchema = nibedanTemplate.FormSchemaJson;

            try
            {
                var schemaNode = JsonNode.Parse(nibedanTemplate.FormSchemaJson);
                if (schemaNode != null && schemaNode["fields"] is JsonArray fields)
                {
                    foreach (var field in fields)
                    {
                        var autoFillKey = field["autoFillKey"]?.ToString();
                        if (!string.IsNullOrEmpty(autoFillKey))
                        {
                            field["defaultValue"] = GetAutoFillValue(profile, autoFillKey);
                        }
                    }
                    prefilledSchema = schemaNode.ToJsonString();
                }
            }
            catch { }

            return new SifarisTemplateDetailsDto(nibedanTemplate.Id, nibedanTemplate.Name, nibedanTemplate.HtmlContent, prefilledSchema);
        }

        private string GetAutoFillValue(CitizenProfile? profile, string key)
        {
            if (key.Equals("system.date", StringComparison.OrdinalIgnoreCase))
                return DateTime.UtcNow.ToString("yyyy-MM-dd");

            if (profile == null) return "";

            return key.ToLowerInvariant() switch
            {
                "citizen.fullname" => profile.FullName,
                "citizen.dob" => profile.DateOfBirth.ToString("yyyy-MM-dd"),
                "citizen.gender" => profile.Gender.ToString(),
                "citizen.citizenshipnumber" => profile.Citizenship?.CitizenshipNumber ?? "",
                "citizen.mobilenumber" => profile.MobileNumber?.Value ?? "",
                _ => ""
            };
        }
    }

    public record GetSifarisTemplateForOfficerQuery(Guid ApplicationId, Guid OfficerId) : IRequest<SifarisTemplateDetailsDto>;

    public class GetSifarisTemplateForOfficerQueryHandler : IRequestHandler<GetSifarisTemplateForOfficerQuery, SifarisTemplateDetailsDto>
    {
        private readonly ISifarisApplicationRepository _appRepository;
        private readonly IDocumentTemplateRepository _templateRepository;
        private readonly ICitizenProfileRepository _profileRepository;

        public GetSifarisTemplateForOfficerQueryHandler(ISifarisApplicationRepository appRepository, IDocumentTemplateRepository templateRepository, ICitizenProfileRepository profileRepository)
        {
            _appRepository = appRepository;
            _templateRepository = templateRepository;
            _profileRepository = profileRepository;
        }

        public async Task<SifarisTemplateDetailsDto> Handle(GetSifarisTemplateForOfficerQuery request, CancellationToken cancellationToken)
        {
            var application = await _appRepository.GetByIdAsync(request.ApplicationId, cancellationToken);
            if (application == null) throw new DomainException("Application not found.");

            var sifarisTemplate = await _templateRepository.GetByIdAsync(application.TargetSifarisTemplateId, cancellationToken);
            if (sifarisTemplate == null) throw new DomainException("Sifaris template not found.");

            var profile = await _profileRepository.GetByIdAsync(application.CitizenId, cancellationToken);

            string prefilledSchema = sifarisTemplate.FormSchemaJson;

            try
            {
                var schemaNode = JsonNode.Parse(sifarisTemplate.FormSchemaJson);
                var submittedData = JsonNode.Parse(application.SubmittedDataJson);

                if (schemaNode != null && schemaNode["fields"] is JsonArray fields)
                {
                    foreach (var field in fields)
                    {
                        var autoFillKey = field["autoFillKey"]?.ToString();
                        if (!string.IsNullOrEmpty(autoFillKey))
                        {
                            if (autoFillKey.Equals("system.date", StringComparison.OrdinalIgnoreCase))
                            {
                                field["defaultValue"] = DateTime.UtcNow.ToString("yyyy-MM-dd");
                            }
                            else if (autoFillKey.StartsWith("citizen.") && profile != null)
                            {
                                field["defaultValue"] = GetAutoFillValue(profile, autoFillKey);
                            }
                            else if (submittedData != null)
                            {
                                var submittedVal = submittedData[autoFillKey]?.ToString();
                                if (submittedVal != null)
                                {
                                    field["defaultValue"] = submittedVal;
                                }
                            }
                        }

                        if (string.IsNullOrEmpty(field["defaultValue"]?.ToString()) && submittedData != null)
                        {
                            var fieldName = field["name"]?.ToString();
                            if (!string.IsNullOrEmpty(fieldName))
                            {
                                var val = submittedData[fieldName]?.ToString();
                                if (val != null)
                                {
                                    field["defaultValue"] = val;
                                }
                            }
                        }
                    }
                    prefilledSchema = schemaNode.ToJsonString();
                }
            }
            catch { }

            return new SifarisTemplateDetailsDto(sifarisTemplate.Id, sifarisTemplate.Name, sifarisTemplate.HtmlContent, prefilledSchema);
        }

        private string GetAutoFillValue(CitizenProfile? profile, string key)
        {
            if (key.Equals("system.date", StringComparison.OrdinalIgnoreCase))
                return DateTime.UtcNow.ToString("yyyy-MM-dd");

            if (profile == null) return "";

            return key.ToLowerInvariant() switch
            {
                "citizen.fullname" => profile.FullName,
                "citizen.dob" => profile.DateOfBirth.ToString("yyyy-MM-dd"),
                "citizen.gender" => profile.Gender.ToString(),
                "citizen.citizenshipnumber" => profile.Citizenship?.CitizenshipNumber ?? "",
                "citizen.mobilenumber" => profile.MobileNumber?.Value ?? "",
                _ => ""
            };
        }
    }
}