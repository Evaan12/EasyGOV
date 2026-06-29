using Application.Common.Pagination;
using Application.Features.DevelopmentPlans.Commands;
using Application.Features.DevelopmentPlans.Queries;
using Domain.Exceptions;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Web.Controllers.Base;

namespace Web.Controllers
{
    [Authorize(Policy = "Require:DevelopmentPlan.Read")]
    public class DevelopmentPlansController : BaseController
    {
        private readonly IMediator _mediator;

        public DevelopmentPlansController(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task<IActionResult> Index(PaginationParameters pagination)
        {
            var result = await _mediator.Send(new GetPaginatedDevelopmentPlansQuery(pagination));
            return View(result);
        }

        [Authorize(Policy = "Require:DevelopmentPlan.Create")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string title, string description, decimal budget, DateTime startDate, DateTime endDate)
        {
            try
            {
                await _mediator.Send(new CreateDevelopmentPlanCommand(title, description, budget, startDate, endDate));
                ShowSuccess("Development Plan securely drafted.");
            }
            catch (DomainException ex)
            {
                ShowError(ex.Message);
            }
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Policy = "Require:DevelopmentPlan.Update")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Publish(Guid id)
        {
            try
            {
                await _mediator.Send(new PublishDevelopmentPlanCommand(id));
                ShowSuccess("Plan published successfully.");
            }
            catch (DomainException ex) { ShowError(ex.Message); }
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Policy = "Require:DevelopmentPlan.Update")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Complete(Guid id)
        {
            try
            {
                await _mediator.Send(new CompleteDevelopmentPlanCommand(id));
                ShowSuccess("Plan marked as completed.");
            }
            catch (DomainException ex) { ShowError(ex.Message); }
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Policy = "Require:DevelopmentPlan.Update")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(Guid id)
        {
            try
            {
                await _mediator.Send(new CancelDevelopmentPlanCommand(id));
                ShowSuccess("Plan has been cancelled.");
            }
            catch (DomainException ex) { ShowError(ex.Message); }
            return RedirectToAction(nameof(Index));
        }
    }
}