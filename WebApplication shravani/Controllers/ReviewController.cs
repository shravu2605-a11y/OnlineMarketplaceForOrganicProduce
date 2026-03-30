using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using WebApplication_shravani.Data;
using WebApplication_shravani.Models;
using WebApplication_shravani.Models.ViewModels;

namespace WebApplication_shravani.Controllers
{
    [Authorize]
    public class ReviewController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<User> _userManager;

        public ReviewController(AppDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> MyReviews()
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var reviews = await _context.Reviews
                .Include(r => r.Product)
                .Where(r => r.CustomerId == userId)
                .OrderByDescending(r => r.CreatedDate)
                .Select(r => new MyReviewViewModel
                {
                    ReviewId = r.ReviewId,
                    ProductId = r.ProductId,
                    ProductName = r.Product != null ? r.Product.Name : string.Empty,
                    ProductImage = r.Product != null ? r.Product.ImageUrl : string.Empty,
                    Rating = r.Rating,
                    Comment = r.Comment ?? string.Empty,
                    CreatedDate = r.CreatedDate,
                    CanEdit = (DateTime.Now - r.CreatedDate).TotalDays <= 30,
                    CanDelete = true
                })
                .ToListAsync();

            return View(reviews);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var userId = _userManager.GetUserId(User);
            var review = await _context.Reviews
                .FirstOrDefaultAsync(r => r.ReviewId == id && r.CustomerId == userId);

            if (review == null)
            {
                return NotFound();
            }

            var model = new AddReviewViewModel
            {
                ProductId = review.ProductId,
                Rating = review.Rating,
                Comment = review.Comment ?? string.Empty
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AddReviewViewModel model)
        {
            if (ModelState.IsValid)
            {
                var userId = _userManager.GetUserId(User);
                var review = await _context.Reviews
                    .FirstOrDefaultAsync(r => r.ReviewId == id && r.CustomerId == userId);

                if (review == null)
                {
                    return NotFound();
                }

                review.Rating = model.Rating;
                review.Comment = model.Comment;
                review.UpdatedDate = DateTime.Now;

                await _context.SaveChangesAsync();
                await UpdateProductAverageRating(review.ProductId);

                TempData["Success"] = "Review updated successfully!";
                return RedirectToAction(nameof(MyReviews));
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = _userManager.GetUserId(User);
            var review = await _context.Reviews
                .FirstOrDefaultAsync(r => r.ReviewId == id && r.CustomerId == userId);

            if (review != null)
            {
                int productId = review.ProductId;
                _context.Reviews.Remove(review);
                await _context.SaveChangesAsync();
                await UpdateProductAverageRating(productId);

                TempData["Success"] = "Review deleted successfully!";
            }

            return RedirectToAction(nameof(MyReviews));
        }

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