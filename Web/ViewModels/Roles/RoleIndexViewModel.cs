using System.Collections.Generic;
using Web.ViewModels.Pagination;

namespace Web.ViewModels.Roles
{
    public class RoleIndexViewModel
    {
        public List<RoleViewModel> Roles { get; set; } = new();
        public PaginationInfoViewModel Pagination { get; set; } = new();
        public string? SearchTerm { get; set; }
    }
}