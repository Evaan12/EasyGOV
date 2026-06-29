using Mediator;
using System;

namespace Application.Features.Users.Commands
{
    public record SuspendUserCommand(Guid UserId, TimeSpan Duration) : IRequest<Unit>;
}