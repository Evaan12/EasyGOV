using Application.Interfaces;
using Mediator;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Users.Commands
{
    public record CreateTenantAdminCommand(string Email, string FullName, string Password, Guid TenantId, Guid RoleId) : IRequest<Guid>;

    public class CreateTenantAdminCommandHandler : IRequestHandler<CreateTenantAdminCommand, Guid>
    {
        private readonly IUserService _userService;

        public CreateTenantAdminCommandHandler(IUserService userService)
        {
            _userService = userService;
        }

        public async Task<Guid> Handle(CreateTenantAdminCommand request, CancellationToken cancellationToken)
        {
            return await _userService.CreateTenantAdminAsync(request.Email, request.FullName, request.Password, request.TenantId, request.RoleId, cancellationToken);
        }
    }
}