using Domain.Enums;
using System;

namespace Application.Features.Tenants.DTOs
{
    public record TenantDto(
        Guid Id,
        string Name,
        TenantType TenantType,
        Guid? ParentId,
        string? ParentName,
        bool IsActivated,
        bool IsDefault,
        string? RegistrationId
    );
}