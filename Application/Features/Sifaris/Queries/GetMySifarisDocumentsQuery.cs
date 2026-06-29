using Domain.Entities;
using Domain.Repositories;
using Mediator;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Sifaris.Queries
{
    public record GetMySifarisDocumentsQuery(Guid CitizenId) : IRequest<IEnumerable<Domain.Entities.Sifaris>>;

    public class GetMySifarisDocumentsQueryHandler : IRequestHandler<GetMySifarisDocumentsQuery, IEnumerable<Domain.Entities.Sifaris>>
    {
        private readonly ISifarisRepository _repository;
        public GetMySifarisDocumentsQueryHandler(ISifarisRepository repository) => _repository = repository;
        
        public async Task<IEnumerable<Domain.Entities.Sifaris>> Handle(GetMySifarisDocumentsQuery request, CancellationToken ct) =>
            await _repository.GetByCitizenIdAsync(request.CitizenId, ct);
    }
}