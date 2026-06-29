using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using Web.Helpers;

namespace Web.Controllers.Base
{
    public abstract class BaseController : Controller
    {
        protected Guid CurrentUserId
        {
            get
            {
                if (User.Identity?.IsAuthenticated != true)
                    return Guid.Empty;

                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                                  ?? User.FindFirst("sub")?.Value;

                if (Guid.TryParse(userIdClaim, out var userId))
                    return userId;

                throw new UnauthorizedAccessException("User is authenticated but the identifier claim is missing or invalid.");
            }
        }

        protected void ShowSuccess(string message) => this.AddSuccess(message);

        protected void ShowError(string message) => this.AddError(message);

        protected void ShowWarning(string message) => this.AddWarning(message);
    }
}