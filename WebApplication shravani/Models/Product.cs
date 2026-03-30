using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication_shravani.Models
{
    public class Product
    {
        [Key]
        public int ProductId { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        public string Category { get; set; } = string.Empty;

        public decimal Price { get; set; }

        public int? FarmerId { get; set; }

        public string Description { get; set; } = string.Empty;

        public string ImageUrl { get; set; } = string.Empty;

        public int StockQuantity { get; set; }

        public DateTime? CreatedDate { get; set; }

        public bool IsAvailable { get; set; }

        public double AverageRating { get; set; }

        public int TotalReviews { get; set; }

        [ForeignKey("FarmerId")]
        public virtual Farmer? Farmer { get; set; }

        public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
    }
}