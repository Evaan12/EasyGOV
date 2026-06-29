using Application.Interfaces;
using Mediator;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Users.Commands
{
    public record RegisterPublicCitizenCommand(string Email, string FullName, string Password, string RegistrationId, string PhoneNumber) : IRequest<Guid>;

    public class RegisterPublicCitizenCommandHandler : IRequestHandler<RegisterPublicCitizenCommand, Guid>
    {
        private readonly IUserService _userService;

        public RegisterPublicCitizenCommandHandler(IUserService userService)
        {
            _userService = userService;
        }

        public async Task<Guid> Handle(RegisterPublicCitizenCommand request, CancellationToken cancellationToken)
        {
            return await _userService.RegisterPublicCitizenAsync(request.Email, request.FullName, request.Password, request.RegistrationId, request.PhoneNumber, cancellationToken);
        }
    }
}