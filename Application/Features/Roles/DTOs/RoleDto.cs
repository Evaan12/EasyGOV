using Domain.Enums;
using System;

namespace Application.Features.Roles.DTOs
{
    public record RoleDto(
        Guid Id,
        string Name,
        TenantType TenantType,
        Guid TenantId,
        bool IsDefault
    );
}