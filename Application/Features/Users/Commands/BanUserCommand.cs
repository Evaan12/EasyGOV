using Mediator;
using System;

namespace Application.Features.Users.Commands
{
    public record BanUserCommand(Guid UserId, string Reason) : IRequest<Unit>;
}