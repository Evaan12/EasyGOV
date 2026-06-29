using Application.Interfaces;
using Domain.Enums;
using Infrastructure.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public class LLMStudioAIService : IAIService
    {
        private readonly HttpClient _httpClient;
        private readonly LLMStudioSettings _settings;
        private readonly ILogger<LLMStudioAIService> _logger;

        public LLMStudioAIService(HttpClient httpClient, IOptions<LLMStudioSettings> options, ILogger<LLMStudioAIService> logger)
        {
            _httpClient = httpClient;
            _settings = options.Value;
            _logger = logger;
        }

        public async Task<GunasoAiResult> AnalyzeGunasoAsync(string rawText, CancellationToken cancellationToken = default)
        {
            var requestUrl = $"{_settings.BaseUrl.TrimEnd('/')}/chat/completions";

            var prompt = $@"
Analyze the following raw grievance/complaint text from a citizen.
Translate or rephrase it into a highly coherent, formal, and clear description in Nepali.
Generate a short, concise Title.
Assign a Severity level from 1 to 4 (1=Low, 2=Medium, 3=High, 4=Critical) based on the context's urgency.

Raw Text: ""{rawText}""

Respond ONLY in valid JSON format matching this exact structure, with no markdown formatting or backticks:
{{
    ""title"": ""..."",
    ""coherentDescription"": ""..."",
    ""severity"": 2
}}";

            var payload = new
            {
                model = _settings.ModelName,
                messages = new[]
                {
                    new { role = "system", content = "You are an AI assistant that structures citizen complaints into formal JSON." },
                    new { role = "user", content = prompt }
                },
                temperature = 0.1,
                stream = false
            };

            var jsonPayload = JsonSerializer.Serialize(payload);
            var request = new HttpRequestMessage(HttpMethod.Post, requestUrl)
            {
                Content = new StringContent(jsonPayload, Encoding.UTF8, "application/json")
            };

            if (!string.IsNullOrEmpty(_settings.ApiKey))
            {
                request.Headers.Add("Authorization", $"Bearer {_settings.ApiKey}");
            }

            try
            {
                var response = await _httpClient.SendAsync(request, cancellationToken);
                response.EnsureSuccessStatusCode();

                var responseString = await response.Content.ReadAsStringAsync(cancellationToken);
                using var jsonDoc = JsonDocument.Parse(responseString);
                var content = jsonDoc.RootElement
                    .GetProperty("choices")[0]
                    .GetProperty("message")
                    .GetProperty("content")
                    .GetString();

                if (string.IsNullOrWhiteSpace(content))
                    throw new Exception("Empty response from AI.");

                content = content.Trim();
                if (content.StartsWith("```json")) content = content.Substring(7);
                if (content.StartsWith("```")) content = content.Substring(3);
                if (content.EndsWith("```")) content = content.Substring(0, content.Length - 3);

                using var resultDoc = JsonDocument.Parse(content.Trim());
                var title = resultDoc.RootElement.GetProperty("title").GetString() ?? "Untitled Gunaso";
                var description = resultDoc.RootElement.GetProperty("coherentDescription").GetString() ?? rawText;
                var severityInt = resultDoc.RootElement.GetProperty("severity").GetInt32();
                
                var severity = severityInt switch {
                    1 => GunasoSeverity.Low,
                    2 => GunasoSeverity.Medium,
                    3 => GunasoSeverity.High,
                    4 => GunasoSeverity.Critical,
                    _ => GunasoSeverity.Medium
                };

                return new GunasoAiResult(title, description, severity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to analyze Gunaso via AI. Falling back to default structural extraction.");
                return new GunasoAiResult("Citizen Grievance", rawText, GunasoSeverity.Medium);
            }
        }
    }
}