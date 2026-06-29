using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IOcrService
    {
        Task<Dictionary<string, string>> ExtractTextAndFieldsAsync(Stream imageStream, string language = "eng+nep", CancellationToken cancellationToken = default);
    }
}