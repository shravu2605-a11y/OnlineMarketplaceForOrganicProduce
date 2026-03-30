using Microsoft.AspNetCore.Mvc;
using WebApplication_shravani.Data;
using WebApplication_shravani.Models;
using System.Linq;

namespace WebApplication_shravani.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;

        public HomeController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Fruits()
        {
            var fruits = _context.Products
                .Where(p => p.Category == "Fruits")
                .ToList();
            return View(fruits);
        }

        public IActionResult Vegetables()
        {
            var vegetables = _context.Products
                .Where(p => p.Category == "Vegetables")
                .ToList();
            return View(vegetables);
        }
        [HttpGet]
        public IActionResult GetAllProducts()
        {
            var products = _context.Products
                .Select(p => new
                {
                    p.ProductId,
                    p.Name,
                    p.Category,
                    p.Price,
                    p.Description,
                    p.ImageUrl,
                    p.AverageRating,
                    p.TotalReviews,
                    p.StockQuantity
                })
                .ToList();
            return Json(products);
        }
    }
}