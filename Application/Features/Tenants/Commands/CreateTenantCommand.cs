using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Domain.Exceptions;
using Domain.Repositories;
using Mediator;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Tenants.Commands
{
    public record CreateTenantCommand(string Name, TenantType TenantType, Guid? ParentId) : IRequest<Guid>;

    public class CreateTenantCommandHandler : IRequestHandler<CreateTenantCommand, Guid>
    {
        private readonly ITenantRepository _tenantRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly IUnitOfWork _unitOfWork;

        public CreateTenantCommandHandler(ITenantRepository tenantRepository, ICurrentUserService currentUserService, IUnitOfWork unitOfWork)
        {
            _tenantRepository = tenantRepository;
            _currentUserService = currentUserService;
            _unitOfWork = unitOfWork;
        }

        public async Task<Guid> Handle(CreateTenantCommand request, CancellationToken cancellationToken)
        {
            Guid? provId = null, distId = null, munId = null;
            string ltreePath = string.Empty;
            var newId = Guid.NewGuid();

            if (request.ParentId.HasValue)
            {
                var parent = await _tenantRepository.GetByIdAsync(request.ParentId.Value, cancellationToken);
                if (parent == null) throw new DomainException("Specified parent tenant does not exist.");

                if (request.TenantType <= parent.TenantType && parent.TenantType != TenantType.Central)
                    throw new DomainException("Sub-tenant must be of a lower hierarchical tier than its parent.");

                provId = parent.ProvinceId ?? (parent.TenantType == TenantType.Province ? parent.Id : null);
                distId = parent.DistrictId ?? (parent.TenantType == TenantType.District ? parent.Id : null);
                munId = parent.MunicipalityId ?? (parent.TenantType == TenantType.Municipality ? parent.Id : null);

                ltreePath = $"{parent.LtreePath}.{newId:N}";
            }
            else if (request.TenantType != TenantType.Central)
            {
                throw new DomainException("A parent tenant is required for this tenant type.");
            }
            else
            {
                ltreePath = $"{newId:N}";
            }

            var tenant = new Tenant(
                newId,
                request.Name,
                request.TenantType,
                request.ParentId,
                ltreePath,
                provId,
                distId,
                munId,
                _currentUserService.UserId
            );

            await _tenantRepository.AddAsync(tenant, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return tenant.Id;
        }
    }
}