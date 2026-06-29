using Domain.Entities;
using Domain.Enums;
using Domain.Repositories;
using Mediator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.DocumentTemplates.Queries
{
    public record TemplateDto(Guid Id, string Name, string Description, bool IsGlobal);

    public record GetActiveTemplatesQuery(TemplateType Type, Guid TenantId) : IRequest<List<TemplateDto>>;

    public class GetActiveTemplatesQueryHandler : IRequestHandler<GetActiveTemplatesQuery, List<TemplateDto>>
    {
        private readonly IDocumentTemplateRepository _repository;

        public GetActiveTemplatesQueryHandler(IDocumentTemplateRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<TemplateDto>> Handle(GetActiveTemplatesQuery request, CancellationToken cancellationToken)
        {
            var rawTemplates = await _repository.GetActiveTemplatesForTenantAsync(request.TenantId, request.Type, cancellationToken);

            var activeTemplates = new List<TemplateDto>();
            
            var localOverrides = rawTemplates
                .Where(t => t.TenantType != TenantType.Central && t.OverridesTemplateId.HasValue)
                .Select(t => t.OverridesTemplateId!.Value)
                .ToHashSet();

            foreach (var template in rawTemplates)
            {
                bool isGlobal = template.TenantType == TenantType.Central;

                // If it's a global template but the tenant has an active override for it, ignore the global one.
                if (isGlobal && localOverrides.Contains(template.Id))
                    continue;

                activeTemplates.Add(new TemplateDto(template.Id, template.Name, template.Description, isGlobal));
            }

            return activeTemplates.OrderBy(t => t.Name).ToList();
        }
    }
}