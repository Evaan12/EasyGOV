using Application.Interfaces;
using Infrastructure.Telephony.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public class OtpService : IOtpService
    {
        private readonly HttpClient _httpClient;
        private readonly TingTingOptions _options;
        private readonly ILogger<OtpService> _logger;
        private const string SaltKey = "Civic_Sifaris_Gov_Secret_Salt_2026";

        public OtpService(HttpClient httpClient, IOptions<TingTingOptions> options, ILogger<OtpService> logger)
        {
            _options = options.Value;
            _httpClient = httpClient;
            if (!string.IsNullOrWhiteSpace(_options.BaseUrl))
            {
                _httpClient.BaseAddress = new Uri(_options.BaseUrl);
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiToken);
            }
            _logger = logger;
        }

        public async Task SendOtpAsync(string destination, string otpCode, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(_options.BaseUrl) || string.IsNullOrWhiteSpace(_options.ApiToken))
            {
                _logger.LogWarning("TingTing Telephony not configured. Simulating SMS OTP to [{Destination}]: {OtpCode}", destination, otpCode);
                return;
            }

            try
            {
                var payload = new
                {
                    number = destination.Replace("+", ""),
                    message = "हजुरको प्रमाणिकरण कोड {otp} हो।",
                    sms_send_options = "text",
                    otp_options = "personnel",
                    otp = otpCode,
                    company_name = "Nepal Government"
                };

                var response = await _httpClient.PostAsJsonAsync("auths/send/otp/", payload, cancellationToken);
                response.EnsureSuccessStatusCode();

                _logger.LogInformation("Successfully sent OTP [{OtpCode}] to destination [{Destination}] via TingTing API.", otpCode, destination);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send OTP to [{Destination}] via TingTing API.", destination);
                throw new Domain.Exceptions.DomainException("Communication service is currently unreachable. Please try again later.");
            }
        }

        public string GenerateCryptoSecureOtp()
        {
            const string chars = "0123456789";
            var result = new char[6];
            using var rng = RandomNumberGenerator.Create();
            var uintBuffer = new byte[4];

            for (int i = 0; i < result.Length; i++)
            {
                rng.GetBytes(uintBuffer);
                uint num = BitConverter.ToUInt32(uintBuffer, 0);
                result[i] = chars[(int)(num % chars.Length)];
            }

            return new string(result);
        }

        public string ComputeHash(string otpCode)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(otpCode + SaltKey); 
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }
    }
}