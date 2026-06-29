using Application.Features.MissingPersons.Commands;
using Application.Features.MissingPersons.Queries;
using Domain.Exceptions;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Web.Controllers.Base;

namespace Web.Controllers
{
    [Authorize]
    public class MissingPersonsController : BaseController
    {
        private readonly IMediator _mediator;

        public MissingPersonsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [Authorize(Policy = "Require:MissingPerson.Read")]
        public async Task<IActionResult> Index()
        {
            var results = await _mediator.Send(new GetPublicMissingPersonsQuery());
            return View(results);
        }

        [AllowAnonymous]
        public async Task<IActionResult> PublicRegistry()
        {
            var results = await _mediator.Send(new GetPublicMissingPersonsQuery());
            return View(results);
        }

        [Authorize(Policy = "Require:MissingPerson.Create")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Report(Guid citizenId, string notes)
        {
            try
            {
                if (citizenId == Guid.Empty)
                    throw new DomainException("A valid registered citizen must be selected to fetch their biometric anchors.");

                await _mediator.Send(new ReportMissingPersonCommand(citizenId, notes));
                
                ShowSuccess("Missing person reported successfully. Real-time biometric tracking has been initialized.");
            }
            catch (DomainException ex)
            {
                ShowError(ex.Message);
            }

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Policy = "Require:MissingPerson.Update")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkFound(Guid id)
        {
            try
            {
                await _mediator.Send(new MarkMissingPersonFoundCommand(id));
                ShowSuccess("Person securely marked as found. Tracking routines deactivated.");
            }
            catch (DomainException ex)
            {
                ShowError(ex.Message);
            }

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Policy = "Require:MissingPerson.Read")]
        public IActionResult LiveScanner()
        {
            return View();
        }
    }
}
