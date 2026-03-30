using System;

namespace WebApplication_shravani.Models.ViewModels
{
    public class MyReviewViewModel
    {
        public int ReviewId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string ProductImage { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public int Rating { get; set; }
        public string Comment { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public string FormattedDate => CreatedDate.ToString("MMM dd, yyyy");
        public bool CanEdit { get; set; }
        public bool CanDelete { get; set; }
    }
}