using Application.Features.eKYC.Commands;
using Application.Features.Tenants.Queries;
using Application.Interfaces;
using Domain.Exceptions;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Web.Controllers.Base;
using Web.ViewModels;

namespace Web.Controllers
{
    [Authorize]
    public class EkycController : BaseController
    {
        private readonly IMediator _mediator;
        private readonly IOcrService _ocrService;
        private readonly IFaceRecognitionService _faceRecognitionService;

        public EkycController(IMediator mediator, IOcrService ocrService, IFaceRecognitionService faceRecognitionService)
        {
            _mediator = mediator;
            _ocrService = ocrService;
            _faceRecognitionService = faceRecognitionService;
        }

        public async Task<IActionResult> Index()
        {
            var districts = await _mediator.Send(new GetAllDistrictsQuery());
            ViewBag.Districts = new SelectList(districts, "Id", "Name");
            return View(new EkycViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> ExtractOcr(IFormFile document, [FromForm] string side, CancellationToken requestCancellation)
        {
            if (document == null) return BadRequest("Document missing.");
            
            try
            {
                // Create a combined cancellation token with a 60-second timeout
                using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(requestCancellation);
                timeoutCts.CancelAfter(TimeSpan.FromSeconds(60));

                using var stream = document.OpenReadStream();
                // Ensure english model is also heavily prioritized. eng+nep works best for mixed text documents.
                string language = side == "front" ? "eng+nep" : "eng";
                var extracted = await _ocrService.ExtractTextAndFieldsAsync(stream, language, timeoutCts.Token);
                return Json(extracted);
            }
            catch (OperationCanceledException)
            {
                return Json(new Dictionary<string, string> { ["Error"] = "OCR processing timed out. Please try with a clearer image or fill in the details manually." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> VerifyAction(IFormFile frame, [FromForm] string action)
        {
            if (frame == null || string.IsNullOrEmpty(action)) return BadRequest("Invalid request.");
            try
            {
                using var stream = frame.OpenReadStream();
                bool isVerified = await _faceRecognitionService.VerifyLivenessActionAsync(stream, action);
                return Json(new { verified = isVerified });
            }
            catch (Exception ex)
            {
                return Json(new { verified = false, error = ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(EkycViewModel model)
        {
            if (model.DocumentType == Domain.Enums.IdentityDocumentType.Citizenship && model.DocumentBack == null)
            {
                ModelState.AddModelError("DocumentBack", "Document Back image is strictly required for Citizenship.");
            }

            if (model.DocumentType != Domain.Enums.IdentityDocumentType.NationalId && !model.IssueDistrictId.HasValue)
            {
                ModelState.AddModelError("IssueDistrictId", "Issue District is required.");
            }

            if (!ModelState.IsValid)
            {
                ShowError("Please complete all required fields and capture the necessary document images.");
                var districts = await _mediator.Send(new GetAllDistrictsQuery());
                ViewBag.Districts = new SelectList(districts, "Id", "Name");
                return View("Index", model);
            }

            try
            {
                using var docFrontStream = model.DocumentFront.OpenReadStream();
                Stream? docBackStream = model.DocumentBack?.OpenReadStream();
                using var selfieStream = model.LiveSelfie.OpenReadStream();

                var issueDistrict = model.IssueDistrictId ?? Guid.Empty;

                await _mediator.Send(new PerformEkycCommand(
                    CurrentUserId,
                    model.DocumentType,
                    model.DocumentNumber,
                    model.IssueDate,
                    issueDistrict,
                    docFrontStream,
                    docBackStream,
                    selfieStream,
                    model.DocumentFront.FileName,
                    model.DocumentBack?.FileName,
                    model.LiveSelfie.FileName
                ));

                if (docBackStream != null) docBackStream.Dispose();

                ShowSuccess("Digital e-KYC Verification Successful! Your identity is securely anchored.");
                return RedirectToAction("Index", "Home");
            }
            catch (DomainException ex)
            {
                ShowError(ex.Message);
                var districts = await _mediator.Send(new GetAllDistrictsQuery());
                ViewBag.Districts = new SelectList(districts, "Id", "Name");
                return View("Index", model);
            }
        }
    }
}