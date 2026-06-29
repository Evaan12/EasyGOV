using Mediator;
using System;

namespace Application.Features.Users.Commands
{
    public record AssignRoleToUserCommand(Guid UserId, Guid RoleId) : IRequest<Unit>;
}