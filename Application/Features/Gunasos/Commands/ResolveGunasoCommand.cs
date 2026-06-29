using Application.Interfaces;
using Domain.Exceptions;
using Domain.Repositories;
using Mediator;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Gunasos.Commands
{
    public record ResolveGunasoCommand(Guid GunasoId, string ResolutionNotes) : IRequest<Unit>;

    public class ResolveGunasoCommandHandler : IRequestHandler<ResolveGunasoCommand, Unit>
    {
        private readonly IGunasoRepository _gunasoRepository;
        private readonly ICurrentUserService _currentUser;
        private readonly IUnitOfWork _unitOfWork;

        public ResolveGunasoCommandHandler(IGunasoRepository gunasoRepository, ICurrentUserService currentUser, IUnitOfWork unitOfWork)
        {
            _gunasoRepository = gunasoRepository;
            _currentUser = currentUser;
            _unitOfWork = unitOfWork;
        }

        public async Task<Unit> Handle(ResolveGunasoCommand request, CancellationToken cancellationToken)
        {
            var gunaso = await _gunasoRepository.GetByIdAsync(request.GunasoId, cancellationToken);
            if (gunaso == null) throw new DomainException("Gunaso not found.");

            gunaso.Resolve(request.ResolutionNotes, _currentUser.UserId);

            await _gunasoRepository.UpdateAsync(gunaso, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}