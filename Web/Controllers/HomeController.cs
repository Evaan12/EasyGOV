using Application.Features.Tenants.Queries;
using Application.Interfaces;
using Domain.Enums;
using Domain.Repositories;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Web.ViewModels;

namespace Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ICurrentUserService _currentUser;
        private readonly IMediator _mediator;
        private readonly ICitizenProfileRepository _profileRepository;

        public HomeController(ILogger<HomeController> logger, ICurrentUserService currentUser, IMediator mediator, ICitizenProfileRepository profileRepository)
        {
            _logger = logger;
            _currentUser = currentUser;
            _mediator = mediator;
            _profileRepository = profileRepository;
        }

        [Authorize] 
        public async Task<IActionResult> Index()
        {
            var tenant = await _mediator.Send(new GetTenantByIdQuery(_currentUser.TenantId));
            ViewBag.Tenant = tenant;

            if (_currentUser.TenantType == TenantType.Ward && !User.Claims.Any(c => c.Type == ClaimTypes.Role))
            {
                var profile = await _profileRepository.GetByIdAsync(_currentUser.UserId);
                if (profile != null)
                {
                    if (profile.Status == CitizenStatus.PendingDigital)
                        return RedirectToAction("Index", "Ekyc");
                    
                    ViewBag.Profile = profile;
                }
            }

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [Route("Error/{statusCode}")]
        public IActionResult Error(int statusCode)
        {
            var exceptionFeature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();
            if (exceptionFeature?.Error != null)
            {
                _logger.LogError(exceptionFeature.Error, "Unhandled exception caught at path: {Path}", exceptionFeature.Path);
                statusCode = 500; 
            }

            var statusCodeResult = HttpContext.Features.Get<IStatusCodeReExecuteFeature>();
            var originalPath = exceptionFeature?.Path ?? statusCodeResult?.OriginalPath;

            var model = new ErrorViewModel
            {
                StatusCode = statusCode,
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
                OriginalPath = originalPath
            };

            switch (statusCode)
            {
                case 404:
                    model.Title = "Page Not Found";
                    model.Message = "Sorry, the page or resource you are looking for does not exist or has been moved.";
                    break;
                case 401:
                    model.Title = "Unauthorized";
                    model.Message = "You must be logged in to access this resource.";
                    break;
                case 403:
                    model.Title = "Access Denied";
                    model.Message = "You do not have the required permissions to view this resource.";
                    break;
                case 400:
                    model.Title = "Bad Request";
                    model.Message = "The request could not be processed by the server due to invalid syntax.";
                    break;
                case 429:
                    model.Title = "Too Many Requests";
                    model.Message = "You have made too many requests in a short period. Please wait and try again.";
                    break;
                case 500:
                    model.Title = "Internal Server Error";
                    model.Message = "An unexpected error occurred on our end. Please try again later.";
                    break;
                default:
                    model.Title = "Unexpected Error";
                    model.Message = "An unexpected error occurred while processing your request.";
                    break;
            }

            return View("Error", model);
        }
    }
}