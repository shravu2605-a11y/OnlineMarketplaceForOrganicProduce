using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication_shravani.Data;
using WebApplication_shravani.Models;
using WebApplication_shravani.Models.ViewModels;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication_shravani.Controllers
{
    public class ProductController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<User> _userManager;

        public ProductController(AppDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            var products = _context.Products.ToList();
            return View(products);
        }

        // Product Details
        public IActionResult Details(int id)
        {
            var product = _context.Products
                .Include(p => p.Reviews)
                .ThenInclude(r => r.Customer)
                .FirstOrDefault(p => p.ProductId == id);

            if (product == null)
            {
                return NotFound();
            }

            // Get related products (same category)
            var relatedProducts = _context.Products
                .Where(p => p.ProductId != id && p.Category == product.Category)
                .Take(6)
                .ToList();

            var viewModel = new ProductDetailsViewModel
            {
                Product = product,
                RelatedProducts = relatedProducts ?? new List<Product>()
            };

            return View(viewModel);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Product product, IFormFile? imageFile)
        {
            if (imageFile != null && imageFile.Length > 0)
            {
                string folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/products");

                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }

                string fileName = Guid.NewGuid().ToString() + "_" + imageFile.FileName;
                string filePath = Path.Combine(folder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(stream);
                }

                product.ImageUrl = "/images/products/" + fileName;
            }
            else
            {
                product.ImageUrl = "/images/products/default.jpg";
            }

            product.CreatedDate = DateTime.Now;
            product.IsAvailable = true;
            product.AverageRating = 0;
            product.TotalReviews = 0;

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        public IActionResult Edit(int id)
        {
            var product = _context.Products.Find(id);
            if (product == null) return NotFound();
            return View(product);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Product product)
        {
            _context.Products.Update(product);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        public IActionResult Delete(int id)
        {
            var product = _context.Products.Find(id);
            if (product == null) return NotFound();
            return View(product);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddReview([FromBody] AddReviewViewModel model)
        {
            try
            {
                var userId = _userManager.GetUserId(User);

                if (string.IsNullOrEmpty(userId))
                {
                    return Json(new { success = false, message = "Please login to submit a review" });
                }

                var existingReview = await _context.Reviews
                    .FirstOrDefaultAsync(r => r.ProductId == model.ProductId && r.CustomerId == userId);

                if (existingReview != null)
                {
                    return Json(new { success = false, message = "You have already reviewed this product!" });
                }

                var review = new Review
                {
                    ProductId = model.ProductId,
                    CustomerId = userId,
                    Rating = model.Rating,
                    Comment = model.Comment,
                    CreatedDate = DateTime.Now,
                    IsVerifiedPurchase = true
                };

                _context.Reviews.Add(review);
                await _context.SaveChangesAsync();

                await UpdateProductAverageRating(model.ProductId);

                return Json(new { success = true, message = "Thank you for your review!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
        // ============ UPDATE PRODUCT AVERAGE RATING ============
        private async Task UpdateProductAverageRating(int productId)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product != null)
            {
                var reviews = await _context.Reviews
                    .Where(r => r.ProductId == productId)
                    .ToListAsync();

                product.TotalReviews = reviews.Count;
                product.AverageRating = reviews.Any() ? reviews.Average(r => r.Rating) : 0;
                await _context.SaveChangesAsync();
            }
        }
    }
}