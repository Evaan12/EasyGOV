using System;
using System.ComponentModel.DataAnnotations;
using Domain.Enums;
using Microsoft.AspNetCore.Http;

namespace Web.ViewModels
{
    public class EkycViewModel
    {
        [Required(ErrorMessage = "Document Front image is required.")]
        public IFormFile DocumentFront { get; set; } = null!;

        public IFormFile? DocumentBack { get; set; }

        [Required(ErrorMessage = "Live Selfie image is required.")]
        public IFormFile LiveSelfie { get; set; } = null!;

        [Required(ErrorMessage = "Document Type is required.")]
        public IdentityDocumentType DocumentType { get; set; }

        [Required(ErrorMessage = "Document Number is required.")]
        public string DocumentNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Issue Date is required.")]
        public DateTime IssueDate { get; set; } = DateTime.Today;

        public Guid? IssueDistrictId { get; set; }
    }
}