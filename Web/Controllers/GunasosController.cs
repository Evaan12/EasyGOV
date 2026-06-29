using Application.Common.Pagination;
using Application.Features.Gunasos.Commands;
using Application.Features.Gunasos.Queries;
using Application.Features.Tenants.Queries;
using Domain.Exceptions;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Threading.Tasks;
using Web.Controllers.Base;

namespace Web.Controllers
{
    [Authorize]
    public class GunasosController : BaseController
    {
        private readonly IMediator _mediator;

        public GunasosController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [AllowAnonymous]
        public async Task<IActionResult> Index(PaginationParameters pagination, bool isCitizenView = false)
        {
            var result = await _mediator.Send(new GetPaginatedGunasosQuery(pagination, isCitizenView));
            ViewBag.IsCitizenView = isCitizenView;
            
            if (isCitizenView)
            {
                var tenants = await _mediator.Send(new GetTenantsLookupQuery());
                ViewBag.Tenants = new SelectList(tenants, "Id", "Name");
            }
            
            return View(result);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FileGunaso(string rawText, Guid targetTenantId)
        {
            try
            {
                await _mediator.Send(new FileGunasoCommand(rawText, targetTenantId));
                ShowSuccess("Your grievance was filed successfully and analyzed via AI.");
            }
            catch (DomainException ex)
            {
                ShowError(ex.Message);
            }
            return RedirectToAction(nameof(Index), new { isCitizenView = true });
        }

        [Authorize(Policy = "Require:Gunaso.Update")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Resolve(Guid id, string resolutionNotes)
        {
            try
            {
                await _mediator.Send(new ResolveGunasoCommand(id, resolutionNotes));
                ShowSuccess("Gunaso has been resolved and officially recorded.");
            }
            catch (DomainException ex)
            {
                ShowError(ex.Message);
            }
            return RedirectToAction(nameof(Index));
        }
    }
}