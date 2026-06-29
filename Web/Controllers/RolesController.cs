using Application.Common.Pagination;
using Application.Features.Roles.Commands;
using Application.Features.Roles.Queries;
using Application.Features.RolePermissions.Commands;
using Application.Features.RolePermissions.Queries;
using Application.Interfaces;
using Domain.Enums;
using Domain.Exceptions;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Web.Controllers.Base;
using Web.ViewModels.Roles;

namespace Web.Controllers
{
    [Authorize(Policy = "Require:Role.Read")]
    public class RolesController : BaseController
    {
        private readonly IMediator _mediator;
        private readonly ICurrentUserService _currentUserService;

        public RolesController(IMediator mediator, ICurrentUserService currentUserService)
        {
            _mediator = mediator;
            _currentUserService = currentUserService;
        }

        public async Task<IActionResult> Index(PaginationParameters parameters)
        {
            var result = await _mediator.Send(new GetPaginatedRolesQuery(parameters));
            
            var viewModel = new RoleIndexViewModel
            {
                SearchTerm = parameters.SearchTerm,
                Roles = result.Items.Select(r => new RoleViewModel
                {
                    Id = r.Id,
                    Name = r.Name,
                    TenantType = r.TenantType,
                    IsGlobal = r.TenantType == TenantType.Central,
                    IsDefault = r.IsDefault
                }).ToList(),
                Pagination = new ViewModels.Pagination.PaginationInfoViewModel
                {
                    PageNumber = result.PageNumber,
                    PageSize = result.PageSize,
                    TotalCount = result.TotalCount
                }
            };

            return View(viewModel);
        }

        [Authorize(Policy = "Require:Role.Create")]
        public IActionResult Create() => View(new RoleFormViewModel());

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "Require:Role.Create")]
        public async Task<IActionResult> Create(RoleFormViewModel model)
        {
            if (!ModelState.IsValid) return View(model);
            
            try
            {
                await _mediator.Send(new CreateRoleCommand(model.Name, model.TenantType, _currentUserService.TenantId));
                ShowSuccess("Role created successfully.");
                return RedirectToAction(nameof(Index));
            }
            catch(DomainException ex)
            {
                ShowError(ex.Message);
                return View(model);
            }
        }

        [Authorize(Policy = "Require:Role.Assign")]
        public async Task<IActionResult> ManagePermissions(Guid id)
        {
            var role = await _mediator.Send(new GetRoleByIdQuery(id));
            if (role == null) return NotFound();

            if (role.IsDefault)
            {
                ShowError("Cannot modify permissions of default system roles.");
                return RedirectToAction(nameof(Index));
            }

            var permissions = await _mediator.Send(new GetRolePermissionsQuery(id));

            var viewModel = new ManagePermissionsViewModel
            {
                RoleId = role.Id,
                RoleName = role.Name,
                Resources = GenerateResourceMatrix(permissions)
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "Require:Role.Assign")]
        public async Task<IActionResult> ManagePermissions(Guid id, ManagePermissionsViewModel model)
        {
            if (id != model.RoleId) return BadRequest();

            var role = await _mediator.Send(new GetRoleByIdQuery(model.RoleId));
            if (role == null || role.IsDefault)
            {
                ShowError("Cannot modify permissions of default system roles.");
                return RedirectToAction(nameof(Index));
            }

            var mappedPermissions = new List<PermissionUpdateDto>();

            foreach (var res in model.Resources)
            {
                ActionType finalAction = ActionType.None;
                var allowedActions = Application.Common.Security.PermissionMetaDataConfigHelper.GetAvailableActions(res.Resource);
                var crud = ActionType.Read | ActionType.Create | ActionType.Update | ActionType.Delete;

                if (allowedActions.HasFlag(crud))
                {
                    if (res.CanCrud) finalAction |= crud;
                }
                else
                {
                    if (res.CanRead && allowedActions.HasFlag(ActionType.Read)) finalAction |= ActionType.Read;
                    if (res.CanCreate && allowedActions.HasFlag(ActionType.Create)) finalAction |= ActionType.Create;
                    if (res.CanUpdate && allowedActions.HasFlag(ActionType.Update)) finalAction |= ActionType.Update;
                    if (res.CanDelete && allowedActions.HasFlag(ActionType.Delete)) finalAction |= ActionType.Delete;
                }

                if (res.CanExport && allowedActions.HasFlag(ActionType.Export)) finalAction |= ActionType.Export;
                if (res.CanApprove && allowedActions.HasFlag(ActionType.Approve)) finalAction |= ActionType.Approve;
                if (res.CanAssign && allowedActions.HasFlag(ActionType.Assign)) finalAction |= ActionType.Assign;
                if (res.CanAdmin && allowedActions.HasFlag(ActionType.Admin)) finalAction |= ActionType.Admin;
                if (res.CanActivate && allowedActions.HasFlag(ActionType.Activate)) finalAction |= ActionType.Activate;

                if (finalAction != ActionType.None)
                {
                    mappedPermissions.Add(new PermissionUpdateDto(res.Resource, finalAction, res.StartTime, res.EndTime, res.AllowedIpAddress));
                }
            }

            try
            {
                await _mediator.Send(new UpdateRolePermissionsCommand(model.RoleId, mappedPermissions));
                ShowSuccess("Role permissions successfully updated.");
                return RedirectToAction(nameof(Index));
            }
            catch (DomainException ex)
            {
                ShowError(ex.Message);
                return View(model);
            }
        }

        private List<ResourcePermissionsViewModel> GenerateResourceMatrix(List<RolePermissionDto> existingPerms)
        {
            var matrix = new List<ResourcePermissionsViewModel>();
            var allResources = Enum.GetValues<ResourceType>().Where(r => r != ResourceType.None);
            var crud = ActionType.Read | ActionType.Create | ActionType.Update | ActionType.Delete;

            foreach (var res in allResources)
            {
                var allowedActions = Application.Common.Security.PermissionMetaDataConfigHelper.GetAvailableActions(res);
                if (allowedActions == ActionType.None) continue;

                var current = existingPerms.FirstOrDefault(p => p.ResourceType == res);
                var row = new ResourcePermissionsViewModel
                {
                    Resource = res,
                    ResourceName = res.ToString(),

                    AvailableActions = allowedActions,

                    CanCrud = current != null && (current.ActionType & crud) == crud,
                    CanRead = current?.ActionType.HasFlag(ActionType.Read) ?? false,
                    CanCreate = current?.ActionType.HasFlag(ActionType.Create) ?? false,
                    CanUpdate = current?.ActionType.HasFlag(ActionType.Update) ?? false,
                    CanDelete = current?.ActionType.HasFlag(ActionType.Delete) ?? false,
                    CanExport = current?.ActionType.HasFlag(ActionType.Export) ?? false,
                    CanApprove = current?.ActionType.HasFlag(ActionType.Approve) ?? false,
                    CanAssign = current?.ActionType.HasFlag(ActionType.Assign) ?? false,
                    CanAdmin = current?.ActionType.HasFlag(ActionType.Admin) ?? false,
                    CanActivate = current?.ActionType.HasFlag(ActionType.Activate) ?? false,

                    StartTime = current?.StartTime,
                    EndTime = current?.EndTime,
                    AllowedIpAddress = current?.AllowedIpAddress
                };
                matrix.Add(row);
            }

            return matrix;
        }
    }
}