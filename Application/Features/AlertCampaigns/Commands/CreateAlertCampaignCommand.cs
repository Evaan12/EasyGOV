using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Domain.Repositories;
using Mediator;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.AlertCampaigns.Commands
{
    public record CreateAlertCampaignCommand(string Title, CampaignCategory Category, string MessageScript, Guid TargetTenantId) : IRequest<Guid>;

    public class CreateAlertCampaignCommandHandler : IRequestHandler<CreateAlertCampaignCommand, Guid>
    {
        private readonly IAlertCampaignRepository _repository;
        private readonly ICurrentUserService _currentUser;
        private readonly IUnitOfWork _unitOfWork;

        public CreateAlertCampaignCommandHandler(IAlertCampaignRepository repository, ICurrentUserService currentUser, IUnitOfWork unitOfWork)
        {
            _repository = repository;
            _currentUser = currentUser;
            _unitOfWork = unitOfWork;
        }

        public async Task<Guid> Handle(CreateAlertCampaignCommand request, CancellationToken cancellationToken)
        {
            var campaign = new AlertCampaign(
                Guid.NewGuid(),
                request.Title,
                request.Category,
                request.MessageScript,
                request.TargetTenantId,
                _currentUser.UserId
            );

            await _repository.AddAsync(campaign, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return campaign.Id;
        }
    }
}