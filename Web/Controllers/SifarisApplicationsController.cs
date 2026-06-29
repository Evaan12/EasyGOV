using Application.Common.Pagination;
using Application.Features.SifarisApplications.Commands;
using Application.Features.SifarisApplications.Queries;
using Application.Features.Sifaris.Queries;
using Application.Features.CitizenProfiles.Queries;
using Application.Features.DocumentTemplates.Queries;
using Application.Interfaces;
using Domain.Exceptions;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Text.Json;
using System.Threading.Tasks;
using Web.Controllers.Base;

namespace Web.Controllers
{
    [Authorize]
    public class SifarisApplicationsController : BaseController
    {
        private readonly IMediator _mediator;
        private readonly ICurrentUserService _currentUserService;
        private readonly IAuthorizationService _authorizationService;

        public SifarisApplicationsController(IMediator mediator, ICurrentUserService currentUserService, IAuthorizationService authorizationService)
        {
            _mediator = mediator;
            _currentUserService = currentUserService;
            _authorizationService = authorizationService;
        }

        public async Task<IActionResult> Index()
        {
            var canApproveResult = await _authorizationService.AuthorizeAsync(User, "Require:Sifaris.Approve");
            bool canApprove = canApproveResult.Succeeded;
            
            ViewBag.CanApprove = canApprove;

            if (canApprove)
            {
                ViewBag.PendingApplications = await _mediator.Send(new GetPendingSifarisApplicationsQuery(_currentUserService.TenantId));
            }
            else
            {
                ViewBag.Templates = await _mediator.Send(new GetActiveTemplatesQuery(Domain.Enums.TemplateType.SifarisCertificate, _currentUserService.TenantId));
                ViewBag.MyApplications = await _mediator.Send(new GetMySifarisApplicationsQuery(_currentUserService.UserId));
                ViewBag.MyDocuments = await _mediator.Send(new GetMySifarisDocumentsQuery(_currentUserService.UserId));
            }

            return View(); 
        }

        [Authorize(Policy = "Require:Sifaris.Read")]
        public async Task<IActionResult> History(PaginationParameters pagination)
        {
            var apps = await _mediator.Send(new GetPaginatedWardSifarisApplicationsQuery(_currentUserService.TenantId, pagination));
            return View(apps);
        }

        [Authorize(Policy = "Require:Sifaris.Approve")]
        [HttpGet]
        public async Task<IActionResult> Review(Guid id)
        {
            var application = await _mediator.Send(new GetSifarisApplicationByIdQuery(id));
            if (application == null) return NotFound();

            if (application.TargetWardId != _currentUserService.TenantId)
                return Forbid();

            var template = await _mediator.Send(new GetSifarisTemplateForUserQuery(application.ApplicationTemplateId, application.CitizenId));
            
            string compiledHtml = template.HtmlContent;
            try
            {
                var json = JsonDocument.Parse(application.SubmittedDataJson);
                foreach (var prop in json.RootElement.EnumerateObject())
                {
                    compiledHtml = compiledHtml.Replace("{{" + prop.Name + "}}", prop.Value.GetString() ?? "");
                }
            }
            catch { }

            var documents = await _mediator.Send(new GetCitizenDocumentsQuery(application.CitizenId));
            var profile = await _mediator.Send(new GetCitizenProfileByIdQuery(application.CitizenId));
            
            ViewBag.CompiledHtml = compiledHtml;
            ViewBag.Documents = documents;
            ViewBag.CitizenProfile = profile;

            return View(application);
        }

        [HttpGet("SifarisApplications/ViewDocument/{applicationId}")]
        public async Task<IActionResult> ViewDocument(Guid applicationId)
        {
            var sifaris = await _mediator.Send(new GetSifarisByApplicationIdQuery(applicationId));
            if (sifaris != null)
            {
                return RedirectToAction("Document", "Sifaris", new { id = sifaris.Id });
            }
            ShowError("Sifaris document not found.");
            return RedirectToAction(nameof(History));
        }

        [HttpGet("api/sifaris/nibedan-template/{sifarisTemplateId}")]
        public async Task<IActionResult> GetNibedanTemplate(Guid sifarisTemplateId)
        {
            try 
            {
                var dto = await _mediator.Send(new GetNibedanTemplateForSifarisQuery(sifarisTemplateId, _currentUserService.UserId));
                return Json(dto);
            }
            catch (DomainException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("api/sifaris/officer-template/{applicationId}")]
        public async Task<IActionResult> GetOfficerTemplate(Guid applicationId)
        {
            try 
            {
                var dto = await _mediator.Send(new GetSifarisTemplateForOfficerQuery(applicationId, _currentUserService.UserId));
                return Json(dto);
            }
            catch (DomainException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("api/sifaris/preview")]
        public async Task<IActionResult> Preview(Guid templateId, [FromBody] JsonElement jsonData, [FromQuery] Guid? applicationId = null)
        {
            try
            {
                string compiledHtml = "";
                if (applicationId.HasValue && applicationId.Value != Guid.Empty)
                {
                    var template = await _mediator.Send(new GetSifarisTemplateForOfficerQuery(applicationId.Value, _currentUserService.UserId));
                    compiledHtml = template.HtmlContent;
                }
                else
                {
                    var template = await _mediator.Send(new GetSifarisTemplateForUserQuery(templateId, _currentUserService.UserId));
                    compiledHtml = template.HtmlContent;
                }
                
                foreach (var prop in jsonData.EnumerateObject())
                {
                    compiledHtml = compiledHtml.Replace("{{" + prop.Name + "}}", prop.Value.GetString() ?? "");
                }
                
                return Content(compiledHtml, "text/html");
            }
            catch (DomainException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(Guid targetSifarisTemplateId, Guid nibedanTemplateId, string jsonData)
        {
            try
            {
                var jsonElement = JsonSerializer.Deserialize<JsonElement>(jsonData);
                await _mediator.Send(new SubmitSifarisApplicationCommand(nibedanTemplateId, targetSifarisTemplateId, jsonElement));
                ShowSuccess("Application (Nibedan) submitted successfully. It is now pending official verification.");
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
            }

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Policy = "Require:Sifaris.Approve")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(Guid applicationId, string reviewNotes, string sifarisDataJson)
        {
            try
            {
                var sifarisId = await _mediator.Send(new ApproveSifarisApplicationCommand(applicationId, reviewNotes, sifarisDataJson));
                ShowSuccess("Application approved and Sifaris issued securely.");
                return RedirectToAction("Document", "Sifaris", new { id = sifarisId });
            }
            catch (DomainException ex)
            {
                ShowError(ex.Message);
                return RedirectToAction(nameof(Index));
            }
        }

        [Authorize(Policy = "Require:Sifaris.Approve")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(Guid applicationId, string reviewNotes)
        {
            try
            {
                await _mediator.Send(new RejectSifarisApplicationCommand(applicationId, reviewNotes));
                ShowSuccess("Application successfully rejected.");
                return RedirectToAction(nameof(Index));
            }
            catch (DomainException ex)
            {
                ShowError(ex.Message);
                return RedirectToAction(nameof(Index));
            }
        }
    }
}