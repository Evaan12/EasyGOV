using Application.Interfaces;
using Domain.Entities;
using Domain.Exceptions;
using Domain.Repositories;
using Mediator;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.DevelopmentPlans.Commands
{
    public record CreateDevelopmentPlanCommand(string Title, string Description, decimal Budget, DateTime StartDate, DateTime EndDate) : IRequest<Guid>;

    public class CreateDevelopmentPlanCommandHandler : IRequestHandler<CreateDevelopmentPlanCommand, Guid>
    {
        private readonly IDevelopmentPlanRepository _repository;
        private readonly ITenantRepository _tenantRepository;
        private readonly ICurrentUserService _currentUser;
        private readonly IUnitOfWork _unitOfWork;

        public CreateDevelopmentPlanCommandHandler(
            IDevelopmentPlanRepository repository, 
            ITenantRepository tenantRepository, 
            ICurrentUserService currentUser, 
            IUnitOfWork unitOfWork)
        {
            _repository = repository;
            _tenantRepository = tenantRepository;
            _currentUser = currentUser;
            _unitOfWork = unitOfWork;
        }

        public async Task<Guid> Handle(CreateDevelopmentPlanCommand request, CancellationToken cancellationToken)
        {
            if (_currentUser.TenantId == Guid.Empty) throw new DomainException("Tenant anchor missing.");

            var tenant = await _tenantRepository.GetByIdAsync(_currentUser.TenantId, cancellationToken);

            var plan = new DevelopmentPlan(
                Guid.NewGuid(),
                request.Title,
                request.Description,
                request.Budget,
                request.StartDate,
                request.EndDate,
                tenant!.Id,
                tenant.LtreePath,
                _currentUser.UserId
            );

            await _repository.AddAsync(plan, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return plan.Id;
        }
    }
}