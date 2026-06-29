using Application.Interfaces;
using Domain.Entities;
using Domain.Exceptions;
using Domain.Repositories;
using Mediator;
using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.SifarisApplications.Commands
{
    public record SubmitSifarisApplicationCommand(Guid NibedanTemplateId, Guid TargetSifarisTemplateId, JsonElement SubmittedData) : IRequest<Guid>;

    public class SubmitSifarisApplicationCommandHandler : IRequestHandler<SubmitSifarisApplicationCommand, Guid>
    {
        private readonly ISifarisApplicationRepository _applicationRepository;
        private readonly ICitizenProfileRepository _profileRepository;
        private readonly ICurrentUserService _currentUser;
        private readonly IUnitOfWork _unitOfWork;

        public SubmitSifarisApplicationCommandHandler(
            ISifarisApplicationRepository applicationRepository, 
            ICitizenProfileRepository profileRepository, 
            ICurrentUserService currentUser, 
            IUnitOfWork unitOfWork)
        {
            _applicationRepository = applicationRepository;
            _profileRepository = profileRepository;
            _currentUser = currentUser;
            _unitOfWork = unitOfWork;
        }

        public async Task<Guid> Handle(SubmitSifarisApplicationCommand request, CancellationToken cancellationToken)
        {
            var profile = await _profileRepository.GetByIdAsync(_currentUser.UserId, cancellationToken);
            if (profile == null) throw new DomainException("Citizen profile not found.");

            // Serialize application payload into JSONB string for immutable snapshotting
            string snapshotJson = JsonSerializer.Serialize(request.SubmittedData);
            
            // Limit checks are structurally enforced within the constructor of the Domain Entity layer to abide by SOLID

            var application = new SifarisApplication(
                Guid.NewGuid(),
                profile.Id,
                profile.RegisteredWardId, // Automatically routes to the citizen's registered ward via routing bindings
                request.NibedanTemplateId,
                request.TargetSifarisTemplateId,
                snapshotJson,
                _currentUser.UserId
            );

            await _applicationRepository.AddAsync(application, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return application.Id;
        }
    }
}