using Application.Features.Users.Commands;
using Application.Features.Users.Queries;
using Application.Interfaces;
using Domain.Enums;
using Domain.Exceptions;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.RateLimiting;
using System;
using System.Linq;
using System.Threading.Tasks;
using Web.Controllers.Base;
using Web.ViewModels.Users;

namespace Web.Controllers
{
    [Authorize(Policy = "Require:User.Update")]
    public class UsersController : BaseController
    {
        private readonly IMediator _mediator;
        private readonly IRoleService _roleService;
        private readonly ICurrentUserService _currentUserService;

        public UsersController(IMediator mediator, IRoleService roleService, ICurrentUserService currentUserService)
        {
            _mediator = mediator;
            _roleService = roleService;
            _currentUserService = currentUserService;
        }

        public async Task<IActionResult> Manage()
        {
            var (roles, _) = await _roleService.GetPaginatedRolesAsync(0, 100, null, _currentUserService.TenantType, _currentUserService.TenantId);

            var availableRoles = roles.AsEnumerable();
            
            if (!_currentUserService.HasUnrestrictedAdmin)
            {
                availableRoles = availableRoles.Where(r => r.Name != "Super Admin");
            }

            var model = new UserManagementViewModel
            {
                AvailableRoles = availableRoles.Select(r => new SelectListItem { Value = r.Id.ToString(), Text = r.Name }).ToList()
            };

            return View(model);
        }

        [HttpGet("api/users/search")]
        [Produces("application/json")]
        public async Task<IActionResult> SearchApi([FromQuery] string term)
        {
            if (string.IsNullOrWhiteSpace(term) || term.Length < 3) return Json(new { items = Array.Empty<object>() });

            var result = await _mediator.Send(new SearchUsersQuery(term, 1, 10));
            return Json(result.Items);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [EnableRateLimiting("StrictUserActionLimiter")]
        public async Task<IActionResult> Suspend(UserManagementViewModel model)
        {
            try
            {
                var duration = TimeSpan.FromHours(model.SuspendDurationHours);
                await _mediator.Send(new SuspendUserCommand(model.SelectedUserId, duration));
                ShowSuccess($"User was successfully suspended for {model.SuspendDurationHours} hours.");
            }
            catch (DomainException ex)
            {
                ShowError(ex.Message);
            }
            
            return RedirectToAction(nameof(Manage));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [EnableRateLimiting("StrictUserActionLimiter")]
        public async Task<IActionResult> Ban(UserManagementViewModel model)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(model.BanReason)) throw new DomainException("Ban reason is strictly required.");
                await _mediator.Send(new BanUserCommand(model.SelectedUserId, model.BanReason));
                ShowSuccess("User has been permanently banned from the system.");
            }
            catch (DomainException ex)
            {
                ShowError(ex.Message);
            }

            return RedirectToAction(nameof(Manage));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "Require:Role.Assign")]
        public async Task<IActionResult> AssignRole(UserManagementViewModel model)
        {
            try
            {
                await _mediator.Send(new AssignRoleToUserCommand(model.SelectedUserId, model.SelectedRoleId));
                ShowSuccess("Security Role was successfully mapped to the user.");
            }
            catch (DomainException ex)
            {
                ShowError(ex.Message);
            }

            return RedirectToAction(nameof(Manage));
        }
    }
}