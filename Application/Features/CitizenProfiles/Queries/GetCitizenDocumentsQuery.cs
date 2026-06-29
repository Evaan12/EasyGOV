using Domain.Entities;
using Domain.Repositories;
using Mediator;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.CitizenProfiles.Queries
{
    public record GetCitizenDocumentsQuery(Guid CitizenId) : IRequest<IEnumerable<DocumentFile>>;

    public class GetCitizenDocumentsQueryHandler : IRequestHandler<GetCitizenDocumentsQuery, IEnumerable<DocumentFile>>
    {
        private readonly IDocumentFileRepository _repository;

        public GetCitizenDocumentsQueryHandler(IDocumentFileRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<DocumentFile>> Handle(GetCitizenDocumentsQuery request, CancellationToken cancellationToken)
        {
            return await _repository.GetByOwnerIdAsync(request.CitizenId, cancellationToken);
        }
    }
}