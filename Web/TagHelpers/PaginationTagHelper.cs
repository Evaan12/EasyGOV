using Application.Common.Pagination;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Linq;

namespace Web.TagHelpers
{
    [HtmlTargetElement("pager")]
    public class PaginationTagHelper : TagHelper
    {
        private readonly IUrlHelperFactory _urlHelperFactory;

        public PaginationTagHelper(IUrlHelperFactory urlHelperFactory)
        {
            _urlHelperFactory = urlHelperFactory;
        }

        [ViewContext]
        [HtmlAttributeNotBound]
        public ViewContext ViewContext { get; set; } = null!;

        [HtmlAttributeName("paged-result")]
        public dynamic PagedResult { get; set; } = null!; 

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "nav";
            output.Attributes.SetAttribute("aria-label", "Page navigation");

            int currentPage = (int)PagedResult.PageNumber;
            int totalPages = (int)PagedResult.TotalPages;

            if (totalPages <= 1)
            {
                output.SuppressOutput();
                return;
            }

            var ul = new TagBuilder("ul");
            ul.AddCssClass("pagination justify-content-center mt-4");

            var requestQuery = ViewContext.HttpContext.Request.Query;
            var queryParams = requestQuery.ToDictionary(q => q.Key, q => q.Value.ToString());

            var prevLi = new TagBuilder("li");
            prevLi.AddCssClass("page-item " + (currentPage == 1 ? "disabled" : ""));
            var prevA = new TagBuilder("a");
            prevA.AddCssClass("page-link");
            queryParams["PageNumber"] = (currentPage - 1).ToString();
            prevA.Attributes["href"] = BuildUrl(queryParams);
            prevA.InnerHtml.AppendHtml("&laquo; Previous");
            prevLi.InnerHtml.AppendHtml(prevA);
            ul.InnerHtml.AppendHtml(prevLi);

            for (int i = 1; i <= totalPages; i++)
            {
                if (i == 1 || i == totalPages || (i >= currentPage - 2 && i <= currentPage + 2))
                {
                    var li = new TagBuilder("li");
                    li.AddCssClass("page-item " + (i == currentPage ? "active" : ""));
                    var a = new TagBuilder("a");
                    a.AddCssClass("page-link");
                    queryParams["PageNumber"] = i.ToString();
                    a.Attributes["href"] = BuildUrl(queryParams);
                    a.InnerHtml.Append(i.ToString());
                    li.InnerHtml.AppendHtml(a);
                    ul.InnerHtml.AppendHtml(li);
                }
                else if (i == currentPage - 3 || i == currentPage + 3)
                {
                    var li = new TagBuilder("li");
                    li.AddCssClass("page-item disabled");
                    var span = new TagBuilder("span");
                    span.AddCssClass("page-link");
                    span.InnerHtml.Append("...");
                    li.InnerHtml.AppendHtml(span);
                    ul.InnerHtml.AppendHtml(li);
                }
            }

            var nextLi = new TagBuilder("li");
            nextLi.AddCssClass("page-item " + (currentPage == totalPages ? "disabled" : ""));
            var nextA = new TagBuilder("a");
            nextA.AddCssClass("page-link");
            queryParams["PageNumber"] = (currentPage + 1).ToString();
            nextA.Attributes["href"] = BuildUrl(queryParams);
            nextA.InnerHtml.AppendHtml("Next &raquo;");
            nextLi.InnerHtml.AppendHtml(nextA);
            ul.InnerHtml.AppendHtml(nextLi);

            output.Content.AppendHtml(ul);
        }

        private string BuildUrl(System.Collections.Generic.Dictionary<string, string> queryParams)
        {
            var qs = string.Join("&", queryParams.Select(kvp => $"{kvp.Key}={System.Uri.EscapeDataString(kvp.Value)}"));
            return $"?{qs}";
        }
    }
}