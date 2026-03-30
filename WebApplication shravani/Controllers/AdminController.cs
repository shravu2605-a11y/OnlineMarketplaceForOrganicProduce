using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication_shravani.Data;
using WebApplication_shravani.Models;
using WebApplication_shravani.Models.ViewModels;
using System.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WebApplication_shravani.Controllers
{
    using Microsoft.AspNetCore.Authorization;

    [Authorize(Roles = "Admin")]
    
        public class AdminController : Controller
    {
        private readonly AppDbContext _context;

        public AdminController(AppDbContext context)
        {
            _context = context;
        }

        // Admin Dashboard
        public IActionResult Dashboard()
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Admin")
            {
                return RedirectToAction("Login", "Account");
            }

            ViewBag.TotalFarmers = _context.Farmers.Count();
            ViewBag.TotalProducts = _context.Products.Count();
            ViewBag.TotalOrders = _context.Orders.Count();
            ViewBag.TotalRevenue = _context.Orders.Sum(o => o.TotalAmount ?? 0);

            return View();
        }

        // ==================== MANAGE FARMERS ====================

        public IActionResult ManageFarmers()
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Admin")
            {
                return RedirectToAction("Login", "Account");
            }

            var farmers = _context.Farmers
                .OrderByDescending(f => f.CreatedDate)
                .ToList();

            // Convert to ViewModel with product count
            var farmerViewModels = farmers.Select(f => new FarmerWithCountViewModel
            {
                Id = f.Id,
                Name = f.Name ?? string.Empty,
                Phone = f.Phone ?? string.Empty,
                Email = f.Email,
                Address = f.Address,
                City = f.City,
                Area = f.Area,
                FarmType = f.FarmType,
                Certification = f.Certification,
                CreatedDate = f.CreatedDate,
                ProductCount = _context.Products.Count(p => p.FarmerId == f.Id)
            }).ToList();

            return View(farmerViewModels);
        }

        // Add Farmer - GET
        public IActionResult AddFarmer()
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Admin")
            {
                return RedirectToAction("Login", "Account");
            }
            return View();
        }

        // Add Farmer - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddFarmer(Farmer farmer)
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Admin")
            {
                return RedirectToAction("Login", "Account");
            }

            if (ModelState.IsValid)
            {
                var existingFarmer = _context.Farmers
                    .FirstOrDefault(f => f.Phone == farmer.Phone);

                if (existingFarmer != null)
                {
                    TempData["Error"] = "Farmer with this phone number already exists!";
                    return View(farmer);
                }

                farmer.CreatedDate = DateTime.Now;
                _context.Farmers.Add(farmer);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Farmer added successfully!";
                return RedirectToAction("ManageFarmers");
            }
            return View(farmer);
        }

        // Edit Farmer - GET
        public IActionResult EditFarmer(int id)
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Admin")
            {
                return RedirectToAction("Login", "Account");
            }

            var farmer = _context.Farmers.Find(id);
            if (farmer == null)
            {
                return NotFound();
            }
            return View(farmer);
        }

        // Edit Farmer - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditFarmer(Farmer farmer)
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Admin")
            {
                return RedirectToAction("Login", "Account");
            }

            if (ModelState.IsValid)
            {
                var existingFarmer = _context.Farmers.Find(farmer.Id);
                if (existingFarmer != null)
                {
                    existingFarmer.Name = farmer.Name;
                    existingFarmer.Phone = farmer.Phone;
                    existingFarmer.Email = farmer.Email;
                    existingFarmer.Address = farmer.Address;
                    existingFarmer.City = farmer.City;
                    existingFarmer.Area = farmer.Area;
                    existingFarmer.FarmType = farmer.FarmType;
                    existingFarmer.Certification = farmer.Certification;

                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Farmer updated successfully!";
                }
                return RedirectToAction("ManageFarmers");
            }
            return View(farmer);
        }

        // Delete Farmer - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteFarmer(int id)
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Admin")
            {
                return RedirectToAction("Login", "Account");
            }

            var farmer = _context.Farmers.Find(id);
            if (farmer != null)
            {
                var hasProducts = _context.Products.Any(p => p.FarmerId == id);
                if (hasProducts)
                {
                    TempData["Error"] = "Cannot delete farmer with existing products!";
                    return RedirectToAction("ManageFarmers");
                }

                _context.Farmers.Remove(farmer);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Farmer deleted successfully!";
            }
            return RedirectToAction("ManageFarmers");
        }

        // View Farmer's Products
        public IActionResult FarmerProducts(int id)
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Admin")
            {
                return RedirectToAction("Login", "Account");
            }

            var farmer = _context.Farmers.Find(id);
            if (farmer == null)
            {
                return NotFound();
            }

            var products = _context.Products
                .Where(p => p.FarmerId == id)
                .OrderBy(p => p.Name)
                .ToList();

            ViewBag.FarmerName = farmer.Name;
            ViewBag.FarmerId = farmer.Id;

            return View(products);
        }

        // Delete Farmer Product
        [HttpPost]
        public async Task<IActionResult> DeleteFarmerProduct(int productId, int farmerId)
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Admin")
            {
                return RedirectToAction("Login", "Account");
            }

            var product = _context.Products.Find(productId);
            if (product != null)
            {
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Product deleted successfully!";
            }

            return RedirectToAction("FarmerProducts", new { id = farmerId });
        }

        // ==================== PRODUCTS ====================

        public IActionResult ManageProducts()
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Admin")
            {
                return RedirectToAction("Login", "Account");
            }

            var products = _context.Products
                .OrderByDescending(p => p.CreatedDate)
                .ToList();
            return View(products);
        }

        // ==================== ORDERS ====================

        public IActionResult ViewOrders()
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Admin")
            {
                return RedirectToAction("Login", "Account");
            }

            var orders = _context.Orders
                .OrderByDescending(o => o.OrderDate)
                .ToList();
            return View(orders);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateOrderStatus(int id, string status)
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Admin")
            {
                return RedirectToAction("Login", "Account");
            }

            var order = _context.Orders.Find(id);
            if (order != null)
            {
                order.Status = status;
                await _context.SaveChangesAsync();
                TempData["Success"] = "Order status updated successfully!";
            }
            return RedirectToAction("ViewOrders");
        }

        // ==================== SALES DASHBOARD ====================

        public IActionResult SalesDashboard()
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Admin")
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                ViewBag.TotalOrders = _context.Orders.Count();
                ViewBag.TotalProducts = _context.Products.Count();
                ViewBag.TotalFarmers = _context.Farmers.Count();
                ViewBag.TotalRevenue = _context.Orders.Sum(o => o.TotalAmount ?? 0);

                var productSales = _context.OrderDetails
                    .Include(od => od.Product)
                    .GroupBy(od => od.Product != null ? od.Product.Name : "Unknown")
                    .Select(g => new SalesViewModel
                    {
                        ProductName = g.Key,
                        TotalSold = g.Sum(x => x.Quantity),
                        TotalRevenue = g.Sum(x => x.Price * x.Quantity)
                    })
                    .ToList();

                var monthlySales = _context.Orders
                    .Where(o => o.OrderDate.HasValue)
                    .GroupBy(o => o.OrderDate.Value.Month)
                    .Select(g => new
                    {
                        Month = g.Key,
                        Total = g.Sum(x => x.TotalAmount ?? 0),
                        OrderCount = g.Count()
                    })
                    .OrderBy(g => g.Month)
                    .ToList();

                ViewBag.MonthlySales = monthlySales;

                return View(productSales);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error loading sales data: " + ex.Message;
                return View(new List<SalesViewModel>());
            }
        }
    }
}