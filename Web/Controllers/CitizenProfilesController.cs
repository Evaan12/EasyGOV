using Application.Common.Pagination;
using Application.Features.CitizenProfiles.Commands;
using Application.Features.CitizenProfiles.Queries;
using Domain.Exceptions;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Web.Controllers.Base;

namespace Web.Controllers
{
    [Authorize(Policy = "Require:CitizenProfile.Read")]
    public class CitizenProfilesController : BaseController
    {
        private readonly IMediator _mediator;

        public CitizenProfilesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task<IActionResult> Index(PaginationParameters pagination)
        {
            var result = await _mediator.Send(new GetPaginatedCitizenProfilesQuery(pagination));
            return View(result);
        }

        [Authorize(Policy = "Require:BiometricEnrollment.Create")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EnrollBiometrics(Guid citizenId, string base64Template)
        {
            try
            {
                if (string.IsNullOrEmpty(base64Template))
                    throw new DomainException("Fingerprint template data is missing.");

                var templateBytes = Convert.FromBase64String(base64Template);
                await _mediator.Send(new EnrollPhysicalBiometricsCommand(citizenId, templateBytes));
                
                ShowSuccess("Physical biometric enrollment completed. Profile is now Active.");
            }
            catch (DomainException ex)
            {
                ShowError(ex.Message);
            }
            catch (FormatException)
            {
                ShowError("Invalid biometric template format.");
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet("api/citizens/{id}/documents")]
        public async Task<IActionResult> GetDocuments(Guid id)
        {
            var docs = await _mediator.Send(new GetCitizenDocumentsQuery(id));
            return Json(docs.Select(d => new {
                type = d.FileType.ToString(),
                path = "/uploads/" + Path.GetFileName(d.FilePath),
                name = d.OriginalFileName
            }));
        }

        [Authorize(Policy = "Require:BiometricEnrollment.Delete")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectEkyc(Guid citizenId, string reason)
        {
            try
            {
                await _mediator.Send(new RejectEkycCommand(citizenId, reason));
                ShowSuccess("e-KYC Request discarded successfully.");
            }
            catch(Exception ex)
            {
                ShowError(ex.Message);
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpGet("api/citizens/search-anchored")]
        public async Task<IActionResult> SearchAnchored(string term)
        {
            if (string.IsNullOrWhiteSpace(term) || term.Length < 2) return Json(Array.Empty<object>());
            var res = await _mediator.Send(new SearchCitizensQuery(term));
            return Json(res);
        }
    }
}