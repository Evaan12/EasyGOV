using Application.Interfaces;
using Domain.Exceptions;
using Domain.Repositories;
using Mediator;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.MissingPersons.Commands
{
    public record MarkMissingPersonFoundCommand(Guid MissingPersonId) : IRequest<Unit>;

    public class MarkMissingPersonFoundCommandHandler : IRequestHandler<MarkMissingPersonFoundCommand, Unit>
    {
        private readonly IMissingPersonRepository _missingPersonRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUser;

        public MarkMissingPersonFoundCommandHandler(
            IMissingPersonRepository missingPersonRepository, 
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUser)
        {
            _missingPersonRepository = missingPersonRepository;
            _unitOfWork = unitOfWork;
            _currentUser = currentUser;
        }

        public async Task<Unit> Handle(MarkMissingPersonFoundCommand request, CancellationToken cancellationToken)
        {
            var person = await _missingPersonRepository.GetByIdAsync(request.MissingPersonId, cancellationToken);
            if (person == null) throw new DomainException("Missing person record not found.");

            person.MarkAsFound(_currentUser.UserId);

            await _missingPersonRepository.UpdateAsync(person, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}