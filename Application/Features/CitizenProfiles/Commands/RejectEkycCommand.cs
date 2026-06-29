using Application.Interfaces;
using Domain.Exceptions;
using Domain.Repositories;
using Mediator;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.CitizenProfiles.Commands
{
    public record RejectEkycCommand(Guid CitizenId, string Reason) : IRequest<Unit>;

    public class RejectEkycCommandHandler : IRequestHandler<RejectEkycCommand, Unit>
    {
        private readonly ICitizenProfileRepository _repository;
        private readonly ICurrentUserService _currentUser;
        private readonly IUnitOfWork _unitOfWork;

        public RejectEkycCommandHandler(ICitizenProfileRepository repository, ICurrentUserService currentUser, IUnitOfWork unitOfWork)
        {
            _repository = repository;
            _currentUser = currentUser;
            _unitOfWork = unitOfWork;
        }

        public async Task<Unit> Handle(RejectEkycCommand request, CancellationToken cancellationToken)
        {
            var profile = await _repository.GetByIdAsync(request.CitizenId, cancellationToken);
            if (profile == null) throw new DomainException("Citizen profile not found.");

            if (_currentUser.TenantType != Domain.Enums.TenantType.Central && _currentUser.TenantId != profile.RegisteredWardId)
            {
                throw new DomainException("You do not have administrative authority over this citizen's ward.");
            }

            profile.RejectDigitalEkyc(_currentUser.UserId);
            
            await _repository.UpdateAsync(profile, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}