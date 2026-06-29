using Domain.Enums;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public record GunasoAiResult(string Title, string CoherentDescription, GunasoSeverity Severity);

    public interface IAIService
    {
        Task<GunasoAiResult> AnalyzeGunasoAsync(string rawText, CancellationToken cancellationToken = default);
    }
}