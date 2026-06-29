using Application.Interfaces;
using Mediator;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Users.Commands
{
    public class BanUserCommandHandler : IRequestHandler<BanUserCommand, Unit>
    {
        private readonly IUserService _userService;

        public BanUserCommandHandler(IUserService userService)
        {
            _userService = userService;
        }

        public async Task<Unit> Handle(BanUserCommand request, CancellationToken cancellationToken)
        {
            await _userService.BanUserAsync(request.UserId, request.Reason, cancellationToken);
            return Unit.Value;
        }
    }
}