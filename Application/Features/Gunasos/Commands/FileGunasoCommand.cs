using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Domain.Exceptions;
using Domain.Repositories;
using Mediator;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Gunasos.Commands
{
    public record FileGunasoCommand(string RawText, Guid TargetTenantId) : IRequest<Guid>;

    public class FileGunasoCommandHandler : IRequestHandler<FileGunasoCommand, Guid>
    {
        private readonly IGunasoRepository _gunasoRepository;
        private readonly ITenantRepository _tenantRepository;
        private readonly ICurrentUserService _currentUser;
        private readonly IAIService _aiService;
        private readonly IUnitOfWork _unitOfWork;

        public FileGunasoCommandHandler(
            IGunasoRepository gunasoRepository, 
            ITenantRepository tenantRepository, 
            ICurrentUserService currentUser,
            IAIService aiService,
            IUnitOfWork unitOfWork)
        {
            _gunasoRepository = gunasoRepository;
            _tenantRepository = tenantRepository;
            _currentUser = currentUser;
            _aiService = aiService;
            _unitOfWork = unitOfWork;
        }

        public async Task<Guid> Handle(FileGunasoCommand request, CancellationToken cancellationToken)
        {

            var citizenWardId = _currentUser.TenantId;

            var isAncestor = await _tenantRepository.IsAncestorAsync(request.TargetTenantId, citizenWardId, cancellationToken);
            
            if (!isAncestor && request.TargetTenantId != citizenWardId)
                throw new DomainException("Security Violation: You can only file a Gunaso to authorities directly within your vertical hierarchy.");

            var targetTenant = await _tenantRepository.GetByIdAsync(request.TargetTenantId, cancellationToken);
            
            var aiResult = await _aiService.AnalyzeGunasoAsync(request.RawText, cancellationToken);

            var gunaso = new Gunaso(
                Guid.NewGuid(),
                aiResult.Title,
                aiResult.CoherentDescription,
                aiResult.Severity,
                _currentUser.UserId,
                request.TargetTenantId,
                targetTenant!.LtreePath,
                _currentUser.UserId
            );

            await _gunasoRepository.AddAsync(gunaso, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return gunaso.Id;
        }
    }
}