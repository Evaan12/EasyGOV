using Domain.Enums;
using Mediator;
using System;

namespace Application.Features.Roles.Commands
{
    public record UpdateRoleCommand(Guid RoleId, string Name) : IRequest<Unit>;
}