using Application.Interfaces;
using Mediator;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.MissingPersons.Commands
{
    public record ProcessVideoFrameCommand(string ConnectionId, byte[] FrameData) : IRequest<Unit>;

    public class ProcessVideoFrameCommandHandler : IRequestHandler<ProcessVideoFrameCommand, Unit>
    {
        private readonly IFaceRecognitionService _faceRecognitionService;
        private readonly ILiveMissingPersonNotifier _notifier;

        public ProcessVideoFrameCommandHandler(IFaceRecognitionService faceRecognitionService, ILiveMissingPersonNotifier notifier)
        {
            _faceRecognitionService = faceRecognitionService;
            _notifier = notifier;
        }

        public async Task<Unit> Handle(ProcessVideoFrameCommand request, CancellationToken cancellationToken)
        {
            using var stream = new MemoryStream(request.FrameData);
            
            var results = await _faceRecognitionService.DetectAndMatchFacesAsync(stream, 0.20, cancellationToken);
            
            await _notifier.BroadcastFrameResultsAsync(request.ConnectionId, results);

            return Unit.Value;
        }
    }
}

