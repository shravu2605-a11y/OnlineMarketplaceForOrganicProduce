using System.Collections.Generic;
using System.Linq;

namespace WebApplication_shravani.Models
{
    public class Cart
    {
        public List<CartItem> Items { get; set; } = new();

        public void AddItem(CartItem item)
        {
            var existing = Items.FirstOrDefault(i => i.ProductId == item.ProductId);

            if (existing != null)
                existing.Quantity += item.Quantity;
            else
                Items.Add(item);
        }

        // ✅ ADD THIS
        public decimal GrandTotal => Items.Sum(i => i.Total);
    }
}
