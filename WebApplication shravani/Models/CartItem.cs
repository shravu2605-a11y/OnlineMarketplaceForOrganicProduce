using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication_shravani.Models
{
    public class CartItem
    {
        [Key]
        public int CartItemId { get; set; }

        public string UserId { get; set; } = string.Empty;

        public int ProductId { get; set; }

        public int Quantity { get; set; }

        public decimal Price { get; set; }

        [NotMapped]
        public string ProductName { get; set; } = string.Empty;

        [NotMapped]
        public string ImageUrl { get; set; } = string.Empty;

        [NotMapped]
        public decimal Total => Quantity * Price;

        [ForeignKey("UserId")]
        public virtual User? User { get; set; }

        [ForeignKey("ProductId")]
        public virtual Product? Product { get; set; }
    }
}