using Domain.Exceptions;
using Domain.Repositories;
using Mediator;
using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Sifaris.Queries
{
    public record SifarisDocumentResult(Guid SifarisId, string CompiledHtml, string ApproverName, string ApproverRole, DateTime IssuedAt);

    public record GetSifarisDocumentQuery(Guid SifarisId) : IRequest<SifarisDocumentResult>;

    public class GetSifarisDocumentQueryHandler : IRequestHandler<GetSifarisDocumentQuery, SifarisDocumentResult>
    {
        private readonly ISifarisRepository _sifarisRepository;
        private readonly IDocumentTemplateRepository _templateRepository;

        public GetSifarisDocumentQueryHandler(ISifarisRepository sifarisRepository, IDocumentTemplateRepository templateRepository)
        {
            _sifarisRepository = sifarisRepository;
            _templateRepository = templateRepository;
        }

        public async Task<SifarisDocumentResult> Handle(GetSifarisDocumentQuery request, CancellationToken cancellationToken)
        {
            var sifaris = await _sifarisRepository.GetByIdAsync(request.SifarisId, cancellationToken);
            if (sifaris == null) throw new DomainException("Sifaris document not found.");

            var template = await _templateRepository.GetByIdAsync(sifaris.SifarisTemplateId, cancellationToken);
            string compiledHtml = template?.HtmlContent ?? "";

            try
            {
                var json = JsonDocument.Parse(sifaris.SnapshotDataJson);
                foreach (var prop in json.RootElement.EnumerateObject())
                {
                    compiledHtml = compiledHtml.Replace("{{" + prop.Name + "}}", prop.Value.GetString() ?? "");
                }
            }
            catch { }

            return new SifarisDocumentResult(
                sifaris.Id, 
                compiledHtml, 
                sifaris.ApproverName ?? "Authorized Officer", 
                sifaris.ApproverRole ?? "Official", 
                sifaris.CreatedAt);
        }
    }

    public record GetSifarisByApplicationIdQuery(Guid ApplicationId) : IRequest<Domain.Entities.Sifaris?>;

    public class GetSifarisByApplicationIdQueryHandler : IRequestHandler<GetSifarisByApplicationIdQuery, Domain.Entities.Sifaris?>
    {
        private readonly ISifarisRepository _repository;
        public GetSifarisByApplicationIdQueryHandler(ISifarisRepository repository) => _repository = repository;
        
        public async Task<Domain.Entities.Sifaris?> Handle(GetSifarisByApplicationIdQuery request, CancellationToken ct) =>
            await _repository.GetByApplicationIdAsync(request.ApplicationId, ct);
    }
}