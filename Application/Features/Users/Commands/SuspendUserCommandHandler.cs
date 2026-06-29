using Application.Interfaces;
using Mediator;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Users.Commands
{
    public class SuspendUserCommandHandler : IRequestHandler<SuspendUserCommand, Unit>
    {
        private readonly IUserService _userService;

        public SuspendUserCommandHandler(IUserService userService)
        {
            _userService = userService;
        }

        public async Task<Unit> Handle(SuspendUserCommand request, CancellationToken cancellationToken)
        {
            await _userService.SuspendUserAsync(request.UserId, request.Duration, cancellationToken);
            return Unit.Value;
        }
    }
}