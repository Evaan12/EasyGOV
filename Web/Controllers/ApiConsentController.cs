using Application.Features.ApiConsent.Commands;
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
    public class ApiConsentController : BaseController
    {
        private readonly IMediator _mediator;

        public ApiConsentController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [AllowAnonymous] 
        [HttpPost("api/consent/request")]
        public async Task<IActionResult> RequestConsent(Guid citizenId, string clientId, string scopes)
        {
            try
            {
                var id = await _mediator.Send(new RequestApiConsentCommand(citizenId, clientId, scopes));
                return Ok(new { requestId = id, message = "OTP sent to citizen." });
            }
            catch (DomainException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyConsent(Guid requestId, string otp)
        {
            try
            {
                await _mediator.Send(new VerifyApiConsentCommand(requestId, otp));
                ShowSuccess("Consent granted successfully.");
                return RedirectToAction("Index", "Home");
            }
            catch (DomainException ex)
            {
                ShowError(ex.Message);
                return RedirectToAction("Index", "Home");
            }
        }
    }
}