using Mediator;
using System;

namespace Application.Features.Roles.Commands
{
    public record DeleteRoleCommand(Guid RoleId) : IRequest<Unit>;
}