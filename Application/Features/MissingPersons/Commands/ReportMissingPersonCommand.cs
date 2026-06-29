using Application.Interfaces;
using Domain.Entities;
using Domain.Exceptions;
using Domain.Repositories;
using Mediator;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.MissingPersons.Commands
{
    public record ReportMissingPersonCommand(Guid CitizenId, string? Notes) : IRequest<Guid>;

    public class ReportMissingPersonCommandHandler : IRequestHandler<ReportMissingPersonCommand, Guid>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUser;
        private readonly ICitizenProfileRepository _citizenProfileRepository;
        private readonly IMissingPersonRepository _missingPersonRepository;

        public ReportMissingPersonCommandHandler(
            IUnitOfWork unitOfWork, 
            ICurrentUserService currentUser, 
            ICitizenProfileRepository citizenProfileRepository,
            IMissingPersonRepository missingPersonRepository)
        {
            _unitOfWork = unitOfWork;
            _currentUser = currentUser;
            _citizenProfileRepository = citizenProfileRepository;
            _missingPersonRepository = missingPersonRepository;
        }

        public async Task<Guid> Handle(ReportMissingPersonCommand request, CancellationToken cancellationToken)
        {
            var citizen = await _citizenProfileRepository.GetByIdAsync(request.CitizenId, cancellationToken);
            if (citizen == null) 
                throw new DomainException("Target citizen profile not found in the registry.");

            if (citizen.FaceEmbedding == null) 
                throw new DomainException("Citizen does not possess a verified biometric embedding from e-KYC. High-accuracy missing person tracking requires an existing biometric anchor.");

            var person = new MissingPerson(
                Guid.NewGuid(), 
                citizen.FullName, 
                citizen.FaceEmbedding, 
                _currentUser.TenantId, 
                request.Notes, 
                _currentUser.UserId);
            
            await _missingPersonRepository.AddAsync(person, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            
            return person.Id;
        }
    }
}