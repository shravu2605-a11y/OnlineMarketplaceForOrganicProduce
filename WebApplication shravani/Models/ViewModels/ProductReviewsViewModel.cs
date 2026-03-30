using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WebApplication_shravani.Models.ViewModels
{
    public class ProductReviewsViewModel
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string ProductImage { get; set; } = string.Empty;
        public double AverageRating { get; set; }
        public int TotalReviews { get; set; }
        public Dictionary<int, int> RatingBreakdown { get; set; } = new Dictionary<int, int>();
        public List<ReviewDisplayViewModel> Reviews { get; set; } = new List<ReviewDisplayViewModel>();
        public AddReviewViewModel NewReview { get; set; } = new AddReviewViewModel();
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
    }

    public class ReviewDisplayViewModel
    {
        public int ReviewId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public int Rating { get; set; }
        public string Comment { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public bool IsVerifiedPurchase { get; set; }
        public string FormattedDate => CreatedDate.ToString("MMM dd, yyyy");
    }

    
}