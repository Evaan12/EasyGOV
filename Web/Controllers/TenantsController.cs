using Application.Common.Pagination;
using Application.Features.Tenants.Commands;
using Application.Features.Tenants.Queries;
using Application.Interfaces;
using Domain.Enums;
using Domain.Exceptions;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Web.Controllers.Base;

namespace Web.Controllers
{
    [Authorize(Policy = "Require:Admin.Admin")]
    public class TenantsController : BaseController
    {
        private readonly IMediator _mediator;
        private readonly IRoleService _roleService;
        private readonly ICurrentUserService _currentUserService;

        public TenantsController(IMediator mediator, IRoleService roleService, ICurrentUserService currentUserService)
        {
            _mediator = mediator;
            _roleService = roleService;
            _currentUserService = currentUserService;
        }

        public async Task<IActionResult> Index(PaginationParameters pagination, Guid? provinceId, Guid? districtId, Guid? municipalityId, TenantType? tenantType, bool? isActivated)
        {
            var result = await _mediator.Send(new GetPaginatedTenantsQuery(pagination, provinceId, districtId, municipalityId, tenantType, isActivated));

            var rolesResult = await _roleService.GetPaginatedRolesAsync(0, 100, null, _currentUserService.TenantType, _currentUserService.TenantId);
            ViewBag.AvailableRoles = rolesResult.Items.Where(r => r.IsDefault).ToList();

            ViewBag.SearchTerm = pagination.SearchTerm;
            ViewBag.ProvinceId = provinceId;
            ViewBag.DistrictId = districtId;
            ViewBag.MunicipalityId = municipalityId;
            ViewBag.TenantType = tenantType;
            ViewBag.IsActivated = isActivated;

            var allTenants = await _mediator.Send(new GetTenantsLookupQuery());
            
            var provinces = allTenants.Where(t => t.TenantType == TenantType.Province).ToList();
            
            var districts = provinceId.HasValue 
                ? allTenants.Where(t => t.TenantType == TenantType.District && t.ProvinceId == provinceId).ToList() 
                : new List<TenantLookupDto>();

            var municipalities = districtId.HasValue 
                ? allTenants.Where(t => t.TenantType == TenantType.Municipality && t.DistrictId == districtId).ToList() 
                : new List<TenantLookupDto>();

            ViewBag.ProvinceList = new SelectList(provinces, "Id", "Name", provinceId);
            ViewBag.DistrictList = new SelectList(districts, "Id", "Name", districtId);
            ViewBag.MunicipalityList = new SelectList(municipalities, "Id", "Name", municipalityId);

            return View(result);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ActivateTenant(Guid tenantId, string headFullName, string headEmail, string headPassword, Guid headRoleId)
        {
            try
            {
                await _mediator.Send(new ActivateAndProvisionTenantCommand(tenantId, headFullName, headEmail, headPassword, headRoleId));
                ShowSuccess("Tenant officially activated and Organizational Head securely provisioned.");
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest") return Json(new { success = true });
            }
            catch (DomainException ex)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest") return Json(new { success = false, message = ex.Message });
                ShowError(ex.Message);
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(Guid tenantId, string name)
        {
            try
            {
                await _mediator.Send(new UpdateTenantCommand(tenantId, name));
                ShowSuccess("Tenant profile updated successfully.");
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest") return Json(new { success = true });
            }
            catch (DomainException ex)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest") return Json(new { success = false, message = ex.Message });
                ShowError(ex.Message);
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(Guid id, bool activate)
        {
            try
            {
                await _mediator.Send(new ToggleTenantStatusCommand(id, activate));
                ShowSuccess($"Tenant status successfully {(activate ? "activated" : "deactivated")}.");
            }
            catch (DomainException ex)
            {
                ShowError(ex.Message);
            }
            return RedirectToAction(nameof(Index));
        }
    }
}