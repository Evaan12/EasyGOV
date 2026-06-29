using Application.DTOs;
using Domain.Entities;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface ITelephonyProvider
    {
        string ProviderName { get; }
        
        Task<LaunchCampaignResult> LaunchCampaignAsync(string campaignTitle, string messageScript, IEnumerable<CitizenProfile> citizens, CancellationToken cancellationToken = default);
        
        Task<IEnumerable<DispatchStatusResult>> GetDispatchStatusesAsync(string externalCampaignId, CancellationToken cancellationToken = default);
    }
}