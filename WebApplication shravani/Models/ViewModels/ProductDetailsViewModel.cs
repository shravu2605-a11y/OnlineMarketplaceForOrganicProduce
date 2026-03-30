using System.Collections.Generic;
using WebApplication_shravani.Models;

namespace WebApplication_shravani.Models.ViewModels
{
    public class ProductDetailsViewModel
    {
        public Product? Product { get; set; }
        public List<Product> RelatedProducts { get; set; } = new List<Product>();
    }
}