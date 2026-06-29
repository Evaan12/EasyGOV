using System.Collections.Generic;
using Domain.Enums;

namespace Application.DTOs
{
    public record LaunchCampaignResult(string ExternalCampaignId, Dictionary<string, string> CitizenToDispatchExternalIds);

    public record DispatchStatusResult(string ExternalDispatchId, DispatchStatus Status, int? Duration);
}