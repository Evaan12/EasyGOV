using Application.Common.Pagination;
using Application.Features.AlertCampaigns.Commands;
using Application.Features.AlertCampaigns.Queries;
using Application.Features.Tenants.Queries;
using Domain.Enums;
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
    [Authorize(Policy = "Require:AlertCampaign.Read")]
    public class AlertCampaignsController : BaseController
    {
        private readonly IMediator _mediator;

        public AlertCampaignsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task<IActionResult> Index(PaginationParameters pagination)
        {
            var result = await _mediator.Send(new GetPaginatedAlertCampaignsQuery(pagination));
            return View(result);
        }

        [Authorize(Policy = "Require:AlertCampaign.Create")]
        public async Task<IActionResult> Create()
        {
            ViewBag.Tenants = new SelectList(await _mediator.Send(new GetTenantsLookupQuery(TenantType.Ward)), "Id", "Name");
            return View();
        }

        [Authorize(Policy = "Require:AlertCampaign.Create")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string title, CampaignCategory category, string messageScript, Guid targetTenantId)
        {
            try
            {
                await _mediator.Send(new CreateAlertCampaignCommand(title, category, messageScript, targetTenantId));
                ShowSuccess("Campaign drafted successfully. Pending official approval.");
                return RedirectToAction(nameof(Index));
            }
            catch (DomainException ex)
            {
                ShowError(ex.Message);
                ViewBag.Tenants = new SelectList(await _mediator.Send(new GetTenantsLookupQuery(TenantType.Ward)), "Id", "Name");
                return View();
            }
        }

        [Authorize(Policy = "Require:AlertCampaign.Approve")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Review(Guid campaignId, ApprovalDecision decision, string? remarks)
        {
            try
            {
                await _mediator.Send(new ReviewAlertCampaignCommand(campaignId, decision, remarks));
                ShowSuccess($"Campaign was {decision.ToString().ToLower()} successfully.");
            }
            catch (DomainException ex)
            {
                ShowError(ex.Message);
            }
            return RedirectToAction(nameof(Index));
        }
    }
}