using Application.Common.Pagination;
using Application.Features.DocumentTemplates.Commands;
using Application.Features.DocumentTemplates.Queries;
using Application.Interfaces;
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
    [Authorize(Policy = "Require:Sifaris.Read")]
    public class DocumentTemplatesController : BaseController
    {
        private readonly IMediator _mediator;
        private readonly ICurrentUserService _currentUserService;

        public DocumentTemplatesController(IMediator mediator, ICurrentUserService currentUserService)
        {
            _mediator = mediator;
            _currentUserService = currentUserService;
        }

        public async Task<IActionResult> Index(PaginationParameters pagination)
        {
            var result = await _mediator.Send(new GetPaginatedDocumentTemplatesQuery(pagination));
            return View(result);
        }

        [Authorize(Policy = "Require:Sifaris.Create")]
        public async Task<IActionResult> Create()
        {
            var nibedans = await _mediator.Send(new GetActiveTemplatesQuery(TemplateType.SifarisApplication, _currentUserService.TenantId));
            ViewBag.NibedanTemplates = new SelectList(nibedans, "Id", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "Require:Sifaris.Create")]
        public async Task<IActionResult> Create(TemplateType type, string name, string description, string formSchemaJson, string htmlContent, Guid? linkedTemplateId)
        {
            try
            {
                await _mediator.Send(new CreateDocumentTemplateCommand(type, name, description, formSchemaJson, htmlContent, linkedTemplateId));
                ShowSuccess("Sifaris template successfully added.");
                return RedirectToAction(nameof(Index));
            }
            catch (DomainException ex)
            {
                ShowError(ex.Message);
                var nibedans = await _mediator.Send(new GetActiveTemplatesQuery(TemplateType.SifarisApplication, _currentUserService.TenantId));
                ViewBag.NibedanTemplates = new SelectList(nibedans, "Id", "Name");
                return View();
            }
        }
    }
}