using Domain.Repositories;
using Mediator;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.RolePermissions.Queries
{
    public class GetRolePermissionsQueryHandler : IRequestHandler<GetRolePermissionsQuery, List<RolePermissionDto>>
    {
        private readonly IRolePermissionRepository _repository;

        public GetRolePermissionsQueryHandler(IRolePermissionRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<RolePermissionDto>> Handle(GetRolePermissionsQuery request, CancellationToken cancellationToken)
        {
            var permissions = await _repository.GetByRoleIdAsync(request.RoleId, cancellationToken);
            
            return permissions
                .Select(p => new RolePermissionDto(
                    p.Id, 
                    p.ResourceType, 
                    p.ActionType, 
                    p.TimeWindow?.StartTime, 
                    p.TimeWindow?.EndTime, 
                    p.AllowedIpAddress))
                .ToList();
        }
    }
}