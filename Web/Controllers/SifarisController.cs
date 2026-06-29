using Application.Features.Sifaris.Queries;
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
    public class SifarisController : BaseController
    {
        private readonly IMediator _mediator;

        public SifarisController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("Sifaris/Document/{id}")]
        public async Task<IActionResult> Document(Guid id)
        {
            try
            {
                var result = await _mediator.Send(new GetSifarisDocumentQuery(id));
                return View(result);
            }
            catch (DomainException ex)
            {
                ShowError(ex.Message);
                return RedirectToAction("Index", "Home");
            }
        }
    }
}