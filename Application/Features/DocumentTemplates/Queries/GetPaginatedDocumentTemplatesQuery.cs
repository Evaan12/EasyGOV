using Application.Common.Pagination;
using Application.Interfaces;
using Domain.Entities;
using Domain.Repositories;
using Mediator;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.DocumentTemplates.Queries
{
    public record GetPaginatedDocumentTemplatesQuery(PaginationParameters Pagination) : IRequest<PagedResult<DocumentTemplate>>;

    public class GetPaginatedDocumentTemplatesQueryHandler : IRequestHandler<GetPaginatedDocumentTemplatesQuery, PagedResult<DocumentTemplate>>
    {
        private readonly IDocumentTemplateRepository _repository;
        private readonly ICurrentUserService _currentUser;

        public GetPaginatedDocumentTemplatesQueryHandler(IDocumentTemplateRepository repository, ICurrentUserService currentUser)
        {
            _repository = repository;
            _currentUser = currentUser;
        }

        public async Task<PagedResult<DocumentTemplate>> Handle(GetPaginatedDocumentTemplatesQuery request, CancellationToken cancellationToken)
        {
            var result = await _repository.GetPaginatedAsync(
                _currentUser.TenantId,
                _currentUser.TenantType,
                request.Pagination.Skip,
                request.Pagination.PageSize,
                request.Pagination.SearchTerm,
                cancellationToken);

            return new PagedResult<DocumentTemplate>(result.Items, result.TotalCount, request.Pagination.PageNumber, request.Pagination.PageSize);
        }
    }
}