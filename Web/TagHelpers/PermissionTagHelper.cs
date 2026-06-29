using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Threading.Tasks;

namespace Web.TagHelpers
{
    [HtmlTargetElement(Attributes = "asp-permission")]
    public class PermissionTagHelper : TagHelper
    {
        private readonly IAuthorizationService _authorizationService;

        public PermissionTagHelper(IAuthorizationService authorizationService)
        {
            _authorizationService = authorizationService;
        }

        [HtmlAttributeName("asp-permission")]
        public string PolicyString { get; set; } = string.Empty;

        [HtmlAttributeNotBound]
        [ViewContext]
        public ViewContext ViewContext { get; set; } = null!;

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            if (string.IsNullOrWhiteSpace(PolicyString))
            {
                output.SuppressOutput();
                return;
            }

            var user = ViewContext.HttpContext.User;
            var authorized = await _authorizationService.AuthorizeAsync(user, PolicyString);

            if (!authorized.Succeeded)
            {
                output.SuppressOutput();
            }
        }
    }
}