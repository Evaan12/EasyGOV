using Domain.Entities;
using Domain.Repositories;
using Mediator;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.CitizenProfiles.Queries
{
    public record GetCitizenProfileByIdQuery(Guid Id) : IRequest<CitizenProfile?>;

    public class GetCitizenProfileByIdQueryHandler : IRequestHandler<GetCitizenProfileByIdQuery, CitizenProfile?>
    {
        private readonly ICitizenProfileRepository _repository;

        public GetCitizenProfileByIdQueryHandler(ICitizenProfileRepository repository)
        {
            _repository = repository;
        }

        public async Task<CitizenProfile?> Handle(GetCitizenProfileByIdQuery request, CancellationToken cancellationToken)
        {
            return await _repository.GetByIdAsync(request.Id, cancellationToken);
        }
    }
}