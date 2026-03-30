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
    /// <summary>
    /// using Microsoft.AspNetCore.Authorization;
    /// </summary>

   // [Authorize(Roles = "Farmer")]
    public class FarmerPanelController : Controller
    {
        private readonly AppDbContext _context;

        public FarmerPanelController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Dashboard()
        {
            var role = HttpContext.Session.GetString("UserRole");
            var farmerId = HttpContext.Session.GetInt32("FarmerId");

            if (role != "Farmer" || !farmerId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            var farmer = _context.Farmers.Find(farmerId.Value);
            if (farmer == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var products = _context.Products.Where(p => p.FarmerId == farmerId).ToList();
            var productIds = products.Select(p => p.ProductId).ToList();

            var orderDetails = _context.OrderDetails
                .Where(od => productIds.Contains(od.ProductId))
                .Include(od => od.Product)
                .ToList();

            var totalProducts = products.Count;
            var totalOrders = orderDetails.Select(od => od.OrderId).Distinct().Count();
            var totalEarnings = orderDetails.Sum(od => od.Price * od.Quantity);

            var productSales = orderDetails
                .GroupBy(od => od.Product != null ? od.Product.Name : "Unknown")
                .Select(g => new ProductSalesViewModel
                {
                    ProductName = g.Key,
                    QuantitySold = g.Sum(x => x.Quantity),
                    TotalRevenue = g.Sum(x => x.Price * x.Quantity)
                })
                .ToList();

            var viewModel = new FarmerDashboardViewModel
            {
                FarmerId = farmer.Id,
                Name = farmer.Name ?? string.Empty,
                Phone = farmer.Phone ?? string.Empty,
                Email = farmer.Email ?? string.Empty,
                Address = farmer.Address ?? string.Empty,
                City = farmer.City ?? string.Empty,
                Area = farmer.Area ?? string.Empty,
                Location = farmer.Location ?? farmer.City ?? string.Empty,
                FarmType = farmer.FarmType ?? string.Empty,
                Certification = farmer.Certification ?? string.Empty,
                CertificateImage = farmer.CertificateImage ?? "/images/default-certificate.jpg",
                TotalProducts = totalProducts,
                TotalOrders = totalOrders,
                TotalEarnings = totalEarnings,
                ProductSales = productSales
            };

            return View(viewModel);
        }

        public IActionResult AddProduct()
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Farmer")
            {
                return RedirectToAction("Login", "Account");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddProduct(Product product, IFormFile? imageFile)
        {
            var farmerId = HttpContext.Session.GetInt32("FarmerId");
            if (!farmerId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

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

            product.FarmerId = farmerId;
            product.CreatedDate = DateTime.Now;
            product.IsAvailable = true;
            product.AverageRating = 0;
            product.TotalReviews = 0;
            product.Reviews = new List<Review>();

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Product added successfully!";
            return RedirectToAction("Dashboard");
        }

        public IActionResult MyProducts()
        {
            var role = HttpContext.Session.GetString("UserRole");
            var farmerId = HttpContext.Session.GetInt32("FarmerId");

            if (role != "Farmer" || !farmerId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            var products = _context.Products
                .Where(p => p.FarmerId == farmerId)
                .OrderByDescending(p => p.CreatedDate)
                .ToList();

            return View(products);
        }

        public IActionResult EditProduct(int id)
        {
            var role = HttpContext.Session.GetString("UserRole");
            var farmerId = HttpContext.Session.GetInt32("FarmerId");

            if (role != "Farmer" || !farmerId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            var product = _context.Products
                .FirstOrDefault(p => p.ProductId == id && p.FarmerId == farmerId);

            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        [HttpPost]
        public async Task<IActionResult> EditProduct(Product product, IFormFile? imageFile)
        {
            var farmerId = HttpContext.Session.GetInt32("FarmerId");
            if (!farmerId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            var existingProduct = _context.Products
                .FirstOrDefault(p => p.ProductId == product.ProductId && p.FarmerId == farmerId);

            if (existingProduct == null)
            {
                return NotFound();
            }

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

                existingProduct.ImageUrl = "/images/products/" + fileName;
            }

            existingProduct.Name = product.Name;
            existingProduct.Description = product.Description;
            existingProduct.Price = product.Price;
            existingProduct.Category = product.Category;
            existingProduct.StockQuantity = product.StockQuantity;
            existingProduct.IsAvailable = product.IsAvailable;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Product updated successfully!";
            return RedirectToAction("MyProducts");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var farmerId = HttpContext.Session.GetInt32("FarmerId");
            if (!farmerId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            var product = _context.Products
                .FirstOrDefault(p => p.ProductId == id && p.FarmerId == farmerId);

            if (product != null)
            {
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Product deleted successfully!";
            }

            return RedirectToAction("MyProducts");
        }

        // View Orders
        public IActionResult ViewOrders()
        {
            var role = HttpContext.Session.GetString("UserRole");
            var farmerId = HttpContext.Session.GetInt32("FarmerId");

            if (role != "Farmer" || !farmerId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            var products = _context.Products.Where(p => p.FarmerId == farmerId).Select(p => p.ProductId).ToList();

            var orders = _context.OrderDetails
                .Where(od => products.Contains(od.ProductId))
                .Include(od => od.Order)
                .Include(od => od.Product)
                .ToList()
                .GroupBy(od => od.OrderId)
                .Select(g => new
                {
                    OrderId = g.Key,
                    Order = g.First().Order,
                    Items = g.ToList(),
                    TotalAmount = g.Sum(x => x.Price * x.Quantity)
                })
                .OrderByDescending(o => o.Order?.OrderDate)
                .ToList();

            return View(orders);
        }

        // Profile
        public IActionResult Profile()
        {
            var role = HttpContext.Session.GetString("UserRole");
            var farmerId = HttpContext.Session.GetInt32("FarmerId");

            if (role != "Farmer" || !farmerId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            var farmer = _context.Farmers.Find(farmerId.Value);
            if (farmer == null)
            {
                return RedirectToAction("Login", "Account");
            }

            return View(farmer);
        }

        // Update Profile
        [HttpPost]
        public async Task<IActionResult> UpdateProfile(Farmer updatedFarmer, IFormFile? certificateImage)
        {
            var farmerId = HttpContext.Session.GetInt32("FarmerId");
            if (!farmerId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            var farmer = _context.Farmers.Find(farmerId.Value);
            if (farmer != null)
            {
                farmer.Name = updatedFarmer.Name;
                farmer.Phone = updatedFarmer.Phone;
                farmer.Email = updatedFarmer.Email;
                farmer.Address = updatedFarmer.Address;
                farmer.City = updatedFarmer.City;
                farmer.Area = updatedFarmer.Area;
                farmer.FarmType = updatedFarmer.FarmType;
                farmer.Certification = updatedFarmer.Certification;

                if (certificateImage != null && certificateImage.Length > 0)
                {
                    string folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/certificates");
                    if (!Directory.Exists(folder))
                    {
                        Directory.CreateDirectory(folder);
                    }

                    string fileName = Guid.NewGuid().ToString() + "_" + certificateImage.FileName;
                    string filePath = Path.Combine(folder, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await certificateImage.CopyToAsync(stream);
                    }

                    farmer.CertificateImage = "/images/certificates/" + fileName;
                }

                await _context.SaveChangesAsync();
                TempData["Success"] = "Profile updated successfully!";
            }

            return RedirectToAction("Profile");
        }

        // Update Order Status
        [HttpPost]
        public async Task<IActionResult> UpdateOrderStatus(int orderId, string status)
        {
            var farmerId = HttpContext.Session.GetInt32("FarmerId");
            if (!farmerId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            var order = _context.Orders.Find(orderId);
            if (order != null)
            {
                order.Status = status;
                await _context.SaveChangesAsync();
                TempData["Success"] = "Order status updated!";
            }

            return RedirectToAction("ViewOrders");
        }
    }
}