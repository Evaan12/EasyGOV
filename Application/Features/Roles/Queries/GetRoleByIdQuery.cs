using Application.Features.Roles.DTOs;
using Mediator;
using System;

namespace Application.Features.Roles.Queries
{
    public record GetRoleByIdQuery(Guid RoleId) : IRequest<RoleDto?>;
}