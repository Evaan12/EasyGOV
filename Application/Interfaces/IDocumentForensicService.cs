using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Domain.ValueObjects;

namespace Application.Interfaces
{
    public interface IDocumentForensicService
    {
        Task<DocumentAnalysisResult> AnalyzeDocumentAsync(Stream imageStream, CancellationToken cancellationToken = default);
    }
}