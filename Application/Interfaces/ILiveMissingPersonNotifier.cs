using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface ILiveMissingPersonNotifier
    {
        Task BroadcastFrameResultsAsync(string connectionId, List<FaceMatchResultDto> results);
    }
}