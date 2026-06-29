using Application.Interfaces;
using Domain.Enums;
using Domain.Exceptions;
using Domain.Repositories;
using Mediator;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.AlertCampaigns.Commands
{
    public record ReviewAlertCampaignCommand(Guid CampaignId, ApprovalDecision Decision, string? Remarks) : IRequest<Unit>;

    public class ReviewAlertCampaignCommandHandler : IRequestHandler<ReviewAlertCampaignCommand, Unit>
    {
        private readonly IAlertCampaignRepository _repository;
        private readonly ICurrentUserService _currentUser;
        private readonly IUnitOfWork _unitOfWork;

        public ReviewAlertCampaignCommandHandler(IAlertCampaignRepository repository, ICurrentUserService currentUser, IUnitOfWork unitOfWork)
        {
            _repository = repository;
            _currentUser = currentUser;
            _unitOfWork = unitOfWork;
        }

        public async Task<Unit> Handle(ReviewAlertCampaignCommand request, CancellationToken cancellationToken)
        {
            var campaign = await _repository.GetByIdAsync(request.CampaignId, cancellationToken);
            if (campaign == null) throw new DomainException("Campaign not found.");

            campaign.ProcessApproval(_currentUser.UserId, request.Decision, request.Remarks);
            await _repository.UpdateAsync(campaign, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}