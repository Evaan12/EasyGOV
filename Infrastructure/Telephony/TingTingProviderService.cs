using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Telephony.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.Telephony
{
    public class TingTingProviderService : ITelephonyProvider
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<TingTingProviderService> _logger;
        private readonly TingTingOptions _options;

        public string ProviderName => "TingTing_v1";

        public TingTingProviderService(HttpClient httpClient, IOptions<TingTingOptions> options, ILogger<TingTingProviderService> logger)
        {
            _options = options.Value;
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri(_options.BaseUrl);
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiToken);
            _logger = logger;
        }

        public async Task<LaunchCampaignResult> LaunchCampaignAsync(string campaignTitle, string messageScript, IEnumerable<CitizenProfile> citizens, CancellationToken cancellationToken = default)
        {
            try
            {
                int callerId = 3; 

                try
                {
                    var brokerResponse = await _httpClient.GetFromJsonAsync<JsonElement>("active-broker-phone/", cancellationToken);
                    if (brokerResponse.ValueKind == JsonValueKind.Array && brokerResponse.GetArrayLength() > 0)
                    {
                        callerId = brokerResponse.EnumerateArray().FirstOrDefault().GetProperty("id").GetInt32();
                    }
                    else
                    {
                        var numbersResponse = await _httpClient.GetFromJsonAsync<JsonElement>("phone-number/active/", cancellationToken);
                        if (numbersResponse.ValueKind == JsonValueKind.Array && numbersResponse.GetArrayLength() > 0)
                        {
                            callerId = numbersResponse.EnumerateArray().FirstOrDefault().GetProperty("id").GetInt32();
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Could not fetch active phone numbers dynamically. Proceeding with fallback caller ID: {CallerId}", callerId);
                }

                var createPayload = new
                {
                    name = campaignTitle,
                    services = "PHONE",
                    user_phone = new[] { callerId },
                    message = messageScript,
                    sms_message = messageScript,
                    description = "Official N-DAS System Alert"
                };

                var createResponse = await _httpClient.PostAsJsonAsync("campaign/create/", createPayload, cancellationToken);
                createResponse.EnsureSuccessStatusCode();
                var campaignJson = await createResponse.Content.ReadFromJsonAsync<JsonElement>(cancellationToken);
                var externalCampaignId = campaignJson.GetProperty("id").GetInt32().ToString();

                var voicePayload = new
                {
                    voice = 12,
                    category = "Text",
                    message = messageScript,
                    sms_message = messageScript
                };

                var patchResponse = await _httpClient.PatchAsJsonAsync($"campaign/create/{externalCampaignId}/message/", voicePayload, cancellationToken);
                patchResponse.EnsureSuccessStatusCode();

                var contactPayload = new
                {
                    contacts = citizens.Select(b => new
                    {
                        number = b.MobileNumber!.Value.Replace("+", ""),
                        other_variables = new { name = b.FullName }
                    }).ToArray()
                };

                var addContactResponse = await _httpClient.PostAsJsonAsync($"campaign/{externalCampaignId}/add-contact/", contactPayload, cancellationToken);
                addContactResponse.EnsureSuccessStatusCode();
                var addedContactsJson = await addContactResponse.Content.ReadFromJsonAsync<JsonElement>(cancellationToken);

                var dispatchMap = new Dictionary<string, string>();

                JsonElement contactsList = default;
                if (addedContactsJson.ValueKind == JsonValueKind.Array) contactsList = addedContactsJson;
                else if (addedContactsJson.ValueKind == JsonValueKind.Object)
                {
                    if (addedContactsJson.TryGetProperty("contacts", out var cArray) && cArray.ValueKind == JsonValueKind.Array) contactsList = cArray;
                    else if (addedContactsJson.TryGetProperty("data", out var dArray) && dArray.ValueKind == JsonValueKind.Array) contactsList = dArray;
                    else if (addedContactsJson.TryGetProperty("results", out var rArray) && rArray.ValueKind == JsonValueKind.Array) contactsList = rArray;
                }

                if (contactsList.ValueKind == JsonValueKind.Array)
                {
                    foreach (var contact in contactsList.EnumerateArray())
                    {
                        if (contact.TryGetProperty("id", out var idProp) && contact.TryGetProperty("number", out var numProp))
                        {
                            var id = idProp.ToString();
                            var numString = numProp.ToString();
                            var num = numString.StartsWith("+") ? numString : "+" + numString;
                            dispatchMap[num] = id;
                        }
                    }
                }

                var runResponse = await _httpClient.PostAsync($"run-campaign/{externalCampaignId}/", new StringContent("{}", System.Text.Encoding.UTF8, "application/json"), cancellationToken);
                runResponse.EnsureSuccessStatusCode();

                return new LaunchCampaignResult(externalCampaignId, dispatchMap);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to launch TingTing campaign: {CampaignTitle}", campaignTitle);
                throw;
            }
        }

        public async Task<IEnumerable<DispatchStatusResult>> GetDispatchStatusesAsync(string externalCampaignId, CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<JsonElement>($"campaign-detail/{externalCampaignId}/", cancellationToken);

                var results = new List<DispatchStatusResult>();

                JsonElement resultsList = default;
                if (response.ValueKind == JsonValueKind.Array) resultsList = response;
                else if (response.ValueKind == JsonValueKind.Object)
                {
                    if (response.TryGetProperty("results", out var rArray) && rArray.ValueKind == JsonValueKind.Array) resultsList = rArray;
                    else if (response.TryGetProperty("data", out var dArray) && dArray.ValueKind == JsonValueKind.Array) resultsList = dArray;
                    else if (response.TryGetProperty("contacts", out var cArray) && cArray.ValueKind == JsonValueKind.Array) resultsList = cArray;
                }

                if (resultsList.ValueKind == JsonValueKind.Array)
                {
                    foreach (var contact in resultsList.EnumerateArray())
                    {
                        if (!contact.TryGetProperty("id", out var idProp)) continue;
                        var extId = idProp.ToString();

                        var rawStatus = "";
                        if (contact.TryGetProperty("status", out var statusProp))
                            rawStatus = statusProp.ToString()?.ToLower() ?? "";

                        int duration = 0;
                        if (contact.TryGetProperty("call_duration", out var durProp))
                        {
                            var durationStr = durProp.ToString()?.Replace("s", "");
                            int.TryParse(durationStr, out duration);
                        }

                        var statusStr = rawStatus.Replace("_", " ").Replace("-", " ");
                        var status = statusStr switch
                        {
                            "answered" => DispatchStatus.Answered,
                            "completed" => DispatchStatus.Answered,
                            "no answer" => DispatchStatus.NoAnswer,
                            "failed" => DispatchStatus.Failed,
                            "in progress" => DispatchStatus.InProgress,
                            "pending" => DispatchStatus.Pending,
                            _ => DispatchStatus.Pending
                        };

                        results.Add(new DispatchStatusResult(extId, status, duration));
                    }
                }

                return results;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch dispatch statuses for external campaign {ExternalCampaignId}", externalCampaignId);
                return Enumerable.Empty<DispatchStatusResult>();
            }
        }
    }
}