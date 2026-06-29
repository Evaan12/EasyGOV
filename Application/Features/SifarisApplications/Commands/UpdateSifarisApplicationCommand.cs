using Application.Interfaces;
using Domain.Exceptions;
using Domain.Repositories;
using Mediator;
using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.SifarisApplications.Commands
{
    public record UpdateSifarisApplicationCommand(Guid ApplicationId, JsonElement UpdatedData) : IRequest<Unit>;

    public class UpdateSifarisApplicationCommandHandler : IRequestHandler<UpdateSifarisApplicationCommand, Unit>
    {
        private readonly ISifarisApplicationRepository _applicationRepository;
        private readonly ICurrentUserService _currentUser;
        private readonly IUnitOfWork _unitOfWork;

        public UpdateSifarisApplicationCommandHandler(
            ISifarisApplicationRepository applicationRepository, 
            ICurrentUserService currentUser, 
            IUnitOfWork unitOfWork)
        {
            _applicationRepository = applicationRepository;
            _currentUser = currentUser;
            _unitOfWork = unitOfWork;
        }

        public async Task<Unit> Handle(UpdateSifarisApplicationCommand request, CancellationToken cancellationToken)
        {
            var application = await _applicationRepository.GetByIdAsync(request.ApplicationId, cancellationToken);
            if (application == null) throw new DomainException("Sifaris application not found.");

            if (application.CitizenId != _currentUser.UserId)
                throw new DomainException("You can only modify applications that were structurally issued to you.");

            string updatedSnapshotJson = JsonSerializer.Serialize(request.UpdatedData);

            application.UpdateSubmittedData(updatedSnapshotJson, _currentUser.UserId);

            await _applicationRepository.UpdateAsync(application, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}