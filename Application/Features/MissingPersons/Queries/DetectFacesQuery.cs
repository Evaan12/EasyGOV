using Application.Interfaces;
using Mediator;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.MissingPersons.Queries
{
    public record DetectFacesQuery(Stream FrameStream) : IRequest<List<FaceMatchResultDto>>;

    public class DetectFacesQueryHandler : IRequestHandler<DetectFacesQuery, List<FaceMatchResultDto>>
    {
        private readonly IFaceRecognitionService _faceRecognitionService;

        public DetectFacesQueryHandler(IFaceRecognitionService faceRecognitionService)
        {
            _faceRecognitionService = faceRecognitionService;
        }

        public async Task<List<FaceMatchResultDto>> Handle(DetectFacesQuery request, CancellationToken cancellationToken)
        {
            // Execute deep-learning matching against the active MissingPerson registry
            // Adjusted threshold strictly to 0.20 as requested for robust real-world acceptance
            return await _faceRecognitionService.DetectAndMatchFacesAsync(request.FrameStream, 0.20, cancellationToken);
        }
    }
}