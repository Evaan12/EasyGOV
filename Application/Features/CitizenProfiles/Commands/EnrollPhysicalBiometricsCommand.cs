using Application.Interfaces;
using Domain.Enums;
using Domain.Exceptions;
using Domain.Repositories;
using Mediator;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.CitizenProfiles.Commands
{
    public record EnrollPhysicalBiometricsCommand(Guid CitizenId, byte[] FingerprintTemplate) : IRequest<Unit>;

    public class EnrollPhysicalBiometricsCommandHandler : IRequestHandler<EnrollPhysicalBiometricsCommand, Unit>
    {
        private readonly ICitizenProfileRepository _repository;
        private readonly ICurrentUserService _currentUser;
        private readonly IUnitOfWork _unitOfWork;

        public EnrollPhysicalBiometricsCommandHandler(
            ICitizenProfileRepository repository, 
            ICurrentUserService currentUser, 
            IUnitOfWork unitOfWork)
        {
            _repository = repository;
            _currentUser = currentUser;
            _unitOfWork = unitOfWork;
        }

        public async Task<Unit> Handle(EnrollPhysicalBiometricsCommand request, CancellationToken cancellationToken)
        {
            var profile = await _repository.GetByIdAsync(request.CitizenId, cancellationToken);
            if (profile == null) throw new DomainException("Citizen profile not found.");

            if (_currentUser.TenantType != TenantType.Central && _currentUser.TenantId != profile.RegisteredWardId)
            {
                throw new DomainException("You do not have administrative authority over this citizen's ward.");
            }

            profile.EnrollPhysicalBiometrics(request.FingerprintTemplate, _currentUser.UserId);
            
            await _repository.UpdateAsync(profile, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}