using Application.Features.SifarisVerification.Queries;
using Domain.Exceptions;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Web.Controllers
{
    [AllowAnonymous]
    public class PublicVerificationController : Controller
    {
        private readonly IMediator _mediator;

        public PublicVerificationController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("verify/{id}")]
        public async Task<IActionResult> VerifySifaris(Guid id)
        {
            try
            {
                var result = await _mediator.Send(new VerifySifarisQuery(id, null));
                return View("VerifySifaris", result);
            }
            catch (DomainException ex)
            {
                ViewBag.ErrorMessage = ex.Message;
                return View("VerificationFailed");
            }
        }

        [HttpPost("verify/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifySifarisPost(Guid id, string challenge)
        {
            try
            {
                var result = await _mediator.Send(new VerifySifarisQuery(id, challenge));
                return View("VerifySifaris", result);
            }
            catch (DomainException ex)
            {
                ViewBag.ErrorMessage = ex.Message;
                return View("VerificationFailed");
            }
        }
    }
}