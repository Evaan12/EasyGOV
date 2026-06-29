using Domain.Entities;
using Domain.Repositories;
using Mediator;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.MissingPersons.Queries
{
    public record GetPublicMissingPersonsQuery() : IRequest<IEnumerable<MissingPerson>>;

    public class GetPublicMissingPersonsQueryHandler : IRequestHandler<GetPublicMissingPersonsQuery, IEnumerable<MissingPerson>>
    {
        private readonly IMissingPersonRepository _repository;

        public GetPublicMissingPersonsQueryHandler(IMissingPersonRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<MissingPerson>> Handle(GetPublicMissingPersonsQuery request, CancellationToken cancellationToken)
        {
            return await _repository.GetActiveMissingPersonsAsync(cancellationToken);
        }
    }
}