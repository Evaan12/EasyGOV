using Application.Interfaces;
using Domain.Entities;
using Domain.Events;
using Domain.Repositories;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.AlertCampaigns.EventHandlers
{
    public class CampaignApprovedEventHandler : IDomainEventHandler<CampaignApprovedEvent>
    {
        private readonly IAlertCampaignRepository _campaignRepository;
        private readonly ICitizenProfileRepository _profileRepository;
        private readonly ITenantService _tenantService;
        private readonly ICampaignDispatchRepository _dispatchRepository;
        private readonly ITelephonyProvider _telephonyProvider;
        private readonly IUnitOfWork _unitOfWork;

        public CampaignApprovedEventHandler(
            IAlertCampaignRepository campaignRepository,
            ICitizenProfileRepository profileRepository,
            ITenantService tenantService,
            ICampaignDispatchRepository dispatchRepository,
            ITelephonyProvider telephonyProvider,
            IUnitOfWork unitOfWork)
        {
            _campaignRepository = campaignRepository;
            _profileRepository = profileRepository;
            _tenantService = tenantService;
            _dispatchRepository = dispatchRepository;
            _telephonyProvider = telephonyProvider;
            _unitOfWork = unitOfWork;
        }

        public async Task HandleAsync(CampaignApprovedEvent domainEvent, CancellationToken cancellationToken = default)
        {
            var campaign = await _campaignRepository.GetByIdAsync(domainEvent.CampaignId, cancellationToken);
            if (campaign == null || campaign.Status != Domain.Enums.CampaignStatus.Approved) return;

            var allowedTenantIds = await _tenantService.GetAllowedTenantIdsAsync(domainEvent.TargetTenantId, cancellationToken);
            
            // Optimization: Filter is now executed on the DB to prevent thousands of entity RAM allocations
            var citizens = (await _profileRepository.GetActiveCitizensByWardIdsAsync(allowedTenantIds, requireMobile: true, cancellationToken)).ToList();

            if (!citizens.Any())
            {
                campaign.MarkAsRunning(_telephonyProvider.ProviderName, "NO_TARGETS", Guid.Empty);
                campaign.MarkAsCompleted(Guid.Empty);
                await _campaignRepository.UpdateAsync(campaign, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                return;
            }

            var dispatches = citizens.Select(c =>
                new CampaignDispatch(Guid.NewGuid(), campaign.Id, c.Id, Guid.Empty)
            ).ToList();

            var apiResult = await _telephonyProvider.LaunchCampaignAsync(campaign.Title, campaign.MessageScript, citizens, cancellationToken);

            foreach (var dispatch in dispatches)
            {
                var profile = citizens.First(c => c.Id == dispatch.CitizenId);
                var phone = profile.MobileNumber!.Value;
                
                if (apiResult.CitizenToDispatchExternalIds.TryGetValue(phone, out var extId) ||
                    apiResult.CitizenToDispatchExternalIds.TryGetValue(phone.Replace("+", ""), out extId) ||
                    apiResult.CitizenToDispatchExternalIds.TryGetValue("+" + phone, out extId) ||
                    apiResult.CitizenToDispatchExternalIds.TryGetValue("+" + phone.Replace("+", ""), out extId))
                {
                    dispatch.AssignExternalId(extId, Guid.Empty);
                }
            }

            campaign.MarkAsRunning(_telephonyProvider.ProviderName, apiResult.ExternalCampaignId, Guid.Empty);

            await _dispatchRepository.AddRangeAsync(dispatches, cancellationToken);
            await _campaignRepository.UpdateAsync(campaign, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}