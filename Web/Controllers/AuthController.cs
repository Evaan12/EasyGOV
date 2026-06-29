using Application.Features.Tenants.Queries;
using Application.Features.Users.Commands;
using Domain.Exceptions;
using Infrastructure.Identity;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Web.Controllers.Base;
using Web.ViewModels.Auth;

namespace Web.Controllers
{
    public class AuthController : BaseController
    {
        private readonly SignInManager<AppUser> _signInManager;
        private readonly UserManager<AppUser> _userManager;
        private readonly IMediator _mediator;

        public AuthController(SignInManager<AppUser> signInManager, UserManager<AppUser> userManager, IMediator mediator)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _mediator = mediator;
        }

        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true) return RedirectToAction("Index", "Home");
            ViewData["ReturnUrl"] = returnUrl;
            return View(new LoginViewModel());
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (!ModelState.IsValid) return View(model);

            var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                ShowSuccess("Successfully logged in.");
                return !string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl)
                    ? Redirect(returnUrl)
                    : RedirectToAction("Index", "Home");
            }

            ShowError("Invalid login attempt.");
            return View(model);
        }

        [AllowAnonymous]
        [HttpGet("Auth/Register/{registrationId}")]
        public async Task<IActionResult> PublicRegister(string registrationId)
        {
            var validWardId = await _mediator.Send(new ValidateWardRegistrationIdQuery(registrationId));
            if (!validWardId.HasValue)
            {
                ShowError("The registration link is invalid or the Ward is inactive.");
                return RedirectToAction(nameof(Login));
            }

            return View(new RegisterViewModel { RegistrationId = registrationId });
        }

        [HttpPost("Auth/Register/{registrationId}")]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PublicRegister(string registrationId, RegisterViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            try
            {
                await _mediator.Send(new RegisterPublicCitizenCommand(model.Email, model.FullName, model.Password, registrationId, model.PhoneNumber));
                ShowSuccess("Citizen profile registered successfully! You may now log in.");
                return RedirectToAction(nameof(Login));
            }
            catch (DomainException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                ShowError(ex.Message);
                return View(model);
            }
        }

        [Authorize(Policy = "Require:User.Create")]
        public IActionResult Register()
        {
            return View(new RegisterViewModel());
        }

        [HttpPost]
        [Authorize(Policy = "Require:User.Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = new AppUser { UserName = model.Email, Email = model.Email, FullName = model.FullName, PhoneNumber = model.PhoneNumber };
            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                ShowSuccess($"User '{user.FullName}' registered successfully.");
                return RedirectToAction("Index", "Home");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            
            ShowError("Failed to register user. Please review errors.");
            return View(model);
        }

        [Authorize]
        public IActionResult ChangePassword()
        {
            return View(new ChangePasswordViewModel());
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");

            var changePasswordResult = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
            if (!changePasswordResult.Succeeded)
            {
                foreach (var error in changePasswordResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                ShowError("Failed to change password. Please check constraints.");
                return View(model);
            }

            await _signInManager.RefreshSignInAsync(user);
            ShowSuccess("Your password has been securely updated.");
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            ShowSuccess("You have successfully logged out.");
            return RedirectToAction(nameof(Login));
        }

        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}