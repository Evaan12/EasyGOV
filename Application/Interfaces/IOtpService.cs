using System.Threading;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IOtpService
    {
        Task SendOtpAsync(string destination, string otpCode, CancellationToken cancellationToken = default);
        string GenerateCryptoSecureOtp();
        string ComputeHash(string otpCode);
    }
}