using Application.Interfaces;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Infrastructure.RealTime
{
    public class LiveMissingPersonNotifier : ILiveMissingPersonNotifier
    {
        private readonly IHubContext<MissingPersonHub> _hubContext;

        public LiveMissingPersonNotifier(IHubContext<MissingPersonHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task BroadcastFrameResultsAsync(string connectionId, List<FaceMatchResultDto> results)
        {
            // Fires bounding box telemetry directly back to the requesting client connection in real-time
            await _hubContext.Clients.Client(connectionId).SendAsync("ReceiveFrameResults", results);
        }
    }
}