using Domain.Enums;
using Mediator;
using System;

namespace Application.Features.Roles.Commands
{
    public record CreateRoleCommand(string Name, TenantType TenantType, Guid TenantId) : IRequest<Guid>;
}