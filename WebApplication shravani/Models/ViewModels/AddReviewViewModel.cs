using System.ComponentModel.DataAnnotations;

namespace WebApplication_shravani.Models.ViewModels
{
    public class AddReviewViewModel
    {
        [Required]
        public int ProductId { get; set; }

        [Required]
        [Range(1, 5)]
        public int Rating { get; set; }

        [Required]
        [StringLength(1000, MinimumLength = 5)]
        public string Comment { get; set; } = string.Empty;
    }
}