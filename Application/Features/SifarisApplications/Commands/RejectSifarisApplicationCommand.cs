using Application.Interfaces;
using Domain.Exceptions;
using Domain.Repositories;
using Mediator;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.SifarisApplications.Commands
{
    public record RejectSifarisApplicationCommand(Guid ApplicationId, string RejectionReason) : IRequest<Unit>;

    public class RejectSifarisApplicationCommandHandler : IRequestHandler<RejectSifarisApplicationCommand, Unit>
    {
        private readonly ISifarisApplicationRepository _applicationRepository;
        private readonly ICurrentUserService _currentUser;
        private readonly IUnitOfWork _unitOfWork;

        public RejectSifarisApplicationCommandHandler(
            ISifarisApplicationRepository applicationRepository, 
            ICurrentUserService currentUser, 
            IUnitOfWork unitOfWork)
        {
            _applicationRepository = applicationRepository;
            _currentUser = currentUser;
            _unitOfWork = unitOfWork;
        }

        public async Task<Unit> Handle(RejectSifarisApplicationCommand request, CancellationToken cancellationToken)
        {
            var application = await _applicationRepository.GetByIdAsync(request.ApplicationId, cancellationToken);
            if (application == null) throw new DomainException("Sifaris application not found.");

            application.Reject(request.RejectionReason, _currentUser.UserId, DateTime.UtcNow);

            await _applicationRepository.UpdateAsync(application, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}

