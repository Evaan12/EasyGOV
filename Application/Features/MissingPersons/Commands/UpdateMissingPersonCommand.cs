using Application.Interfaces;
using Domain.Exceptions;
using Domain.Repositories;
using Mediator;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.MissingPersons.Commands
{
    public record UpdateMissingPersonCommand(Guid MissingPersonId, string FullName, string? Notes) : IRequest<Unit>;

    public class UpdateMissingPersonCommandHandler : IRequestHandler<UpdateMissingPersonCommand, Unit>
    {
        private readonly IMissingPersonRepository _missingPersonRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUser;

        public UpdateMissingPersonCommandHandler(
            IMissingPersonRepository missingPersonRepository, 
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUser)
        {
            _missingPersonRepository = missingPersonRepository;
            _unitOfWork = unitOfWork;
            _currentUser = currentUser;
        }

        public async Task<Unit> Handle(UpdateMissingPersonCommand request, CancellationToken cancellationToken)
        {
            var person = await _missingPersonRepository.GetByIdAsync(request.MissingPersonId, cancellationToken);
            if (person == null) throw new DomainException("Missing person record not found.");

            person.UpdateDetails(request.FullName, request.Notes, _currentUser.UserId);

            await _missingPersonRepository.UpdateAsync(person, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}