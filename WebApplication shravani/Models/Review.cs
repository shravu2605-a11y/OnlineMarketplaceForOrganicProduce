using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication_shravani.Models
{
    public class Review
    {
        [Key]
        public int ReviewId { get; set; }

        [Required]
        public int ProductId { get; set; }

        [Required]
        public string CustomerId { get; set; } = string.Empty;

        [Required]
        [Range(1, 5)]
        public int Rating { get; set; }

        [StringLength(1000)]
        public string Comment { get; set; } = string.Empty;

        [Required]
        public DateTime CreatedDate { get; set; }

        // ADD THIS - UpdatedDate property
        public DateTime? UpdatedDate { get; set; }

        public bool IsVerifiedPurchase { get; set; }

        // ADD THIS - ImageUrl property
        [StringLength(500)]
        public string ImageUrl { get; set; } = string.Empty;

        [ForeignKey("ProductId")]
        public virtual Product? Product { get; set; }

        [ForeignKey("CustomerId")]
        public virtual User? Customer { get; set; }
    }
}