using Application.Interfaces;
using Mediator;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Users.Commands
{
    public class AssignRoleToUserCommandHandler : IRequestHandler<AssignRoleToUserCommand, Unit>
    {
        private readonly IUserService _userService;

        public AssignRoleToUserCommandHandler(IUserService userService)
        {
            _userService = userService;
        }

        public async Task<Unit> Handle(AssignRoleToUserCommand request, CancellationToken cancellationToken)
        {
            await _userService.AssignRoleToUserAsync(request.UserId, request.RoleId, cancellationToken);
            return Unit.Value;
        }
    }
}