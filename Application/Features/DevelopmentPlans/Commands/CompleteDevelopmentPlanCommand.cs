using Application.Interfaces;
using Domain.Exceptions;
using Domain.Repositories;
using Mediator;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.DevelopmentPlans.Commands
{
    public record CompleteDevelopmentPlanCommand(Guid PlanId) : IRequest<Unit>;

    public class CompleteDevelopmentPlanCommandHandler : IRequestHandler<CompleteDevelopmentPlanCommand, Unit>
    {
        private readonly IDevelopmentPlanRepository _repository;
        private readonly ICurrentUserService _currentUser;
        private readonly IUnitOfWork _unitOfWork;

        public CompleteDevelopmentPlanCommandHandler(IDevelopmentPlanRepository repository, ICurrentUserService currentUser, IUnitOfWork unitOfWork)
        {
            _repository = repository;
            _currentUser = currentUser;
            _unitOfWork = unitOfWork;
        }

        public async Task<Unit> Handle(CompleteDevelopmentPlanCommand request, CancellationToken cancellationToken)
        {
            var plan = await _repository.GetByIdAsync(request.PlanId, cancellationToken);
            if (plan == null) throw new DomainException("Plan not found.");

            plan.Complete(_currentUser.UserId);
            
            await _repository.UpdateAsync(plan, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}