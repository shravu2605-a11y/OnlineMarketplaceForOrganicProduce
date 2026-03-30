using System.Collections.Generic;
using WebApplication_shravani.Models;

namespace WebApplication_shravani.Models.ViewModels
{
    public class HomeIndexViewModel
    {
        public List<Product> Fruits { get; set; } = new List<Product>();
        public List<Product> Vegetables { get; set; } = new List<Product>();
        public List<Product> FeaturedProducts { get; set; } = new List<Product>();
    }
}