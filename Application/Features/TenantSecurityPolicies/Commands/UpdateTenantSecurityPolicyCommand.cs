using Domain.Entities;
using Domain.Repositories;
using Domain.ValueObjects;
using Mediator;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.TenantSecurityPolicies.Commands
{
    public record UpdateTenantSecurityPolicyCommand(TimeSpan? StartTime, TimeSpan? EndTime, string? AllowedIpAddress, Guid UserId) : IRequest<Guid>;

    public class UpdateTenantSecurityPolicyCommandHandler : IRequestHandler<UpdateTenantSecurityPolicyCommand, Guid>
    {
        private readonly ITenantSecurityPolicyRepository _repository;
        private readonly IUnitOfWork _unitOfWork;

        public UpdateTenantSecurityPolicyCommandHandler(ITenantSecurityPolicyRepository repository, IUnitOfWork unitOfWork)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Guid> Handle(UpdateTenantSecurityPolicyCommand request, CancellationToken cancellationToken)
        {
            AccessTimeWindow? window = null;
            if (request.StartTime.HasValue && request.EndTime.HasValue)
            {
                window = new AccessTimeWindow(request.StartTime.Value, request.EndTime.Value);
            }

            var policy = await _repository.GetGlobalPolicyAsync(cancellationToken);

            if (policy == null)
            {
                policy = new TenantSecurityPolicy(Guid.NewGuid(), window, request.AllowedIpAddress, request.UserId);
                await _repository.AddAsync(policy, cancellationToken);
            }
            else
            {
                policy.UpdateSecurityConstraints(window, request.AllowedIpAddress, request.UserId);
                await _repository.UpdateAsync(policy, cancellationToken);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return policy.Id;
        }
    }
}