using Domain.Enums;
using System;

namespace Application.Interfaces
{
    public interface ICurrentUserService
    {
        Guid UserId { get; }
        Guid TenantId { get; }
        TenantType TenantType { get; }
        bool HasUnrestrictedAdmin { get; }
    }
}