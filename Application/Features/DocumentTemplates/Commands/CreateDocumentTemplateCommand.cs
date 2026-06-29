using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Domain.Repositories;
using Mediator;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.DocumentTemplates.Commands
{
    public record CreateDocumentTemplateCommand(TemplateType Type, string Name, string Description, string FormSchemaJson, string HtmlContent, Guid? LinkedTemplateId) : IRequest<Guid>;

    public class CreateDocumentTemplateCommandHandler : IRequestHandler<CreateDocumentTemplateCommand, Guid>
    {
        private readonly IDocumentTemplateRepository _repository;
        private readonly ICurrentUserService _currentUser;
        private readonly IUnitOfWork _unitOfWork;

        public CreateDocumentTemplateCommandHandler(IDocumentTemplateRepository repository, ICurrentUserService currentUser, IUnitOfWork unitOfWork)
        {
            _repository = repository;
            _currentUser = currentUser;
            _unitOfWork = unitOfWork;
        }

        public async Task<Guid> Handle(CreateDocumentTemplateCommand request, CancellationToken cancellationToken)
        {
            var template = new DocumentTemplate(
                Guid.NewGuid(),
                request.Type,
                request.Name,
                request.Description,
                request.FormSchemaJson,
                request.HtmlContent,
                _currentUser.TenantType,
                _currentUser.TenantId,
                null,
                request.LinkedTemplateId,
                _currentUser.UserId
            );

            await _repository.AddAsync(template, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return template.Id;
        }
    }
}