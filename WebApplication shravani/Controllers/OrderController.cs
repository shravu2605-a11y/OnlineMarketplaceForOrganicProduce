using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication_shravani.Data;
using WebApplication_shravani.Models;
using WebApplication_shravani.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace WebApplication_shravani.Controllers
{
    public class OrderController : Controller
    {
        private readonly AppDbContext _context;

        public OrderController(AppDbContext context)
        {
            _context = context;
        }

        // ==================== BUY NOW ====================

        // Buy Now - Show Place Order page directly (no separate BuyNow page)
        [HttpGet]
        public IActionResult BuyNow(int id)
        {
            // Check if user is logged in
            if (HttpContext.Session.GetString("UserRole") == null)
            {
                TempData["Error"] = "Please login to purchase products!";
                return RedirectToAction("Login", "Account");
            }

            var product = _context.Products.FirstOrDefault(p => p.ProductId == id);
            if (product == null) return NotFound();

            // Store buy now data in session
            HttpContext.Session.SetInt32("BuyNowProductId", id);
            HttpContext.Session.SetString("IsBuyNow", "true");

            // Create a single cart item for Buy Now
            var buyNowItem = new CartItem
            {
                ProductId = product.ProductId,
                ProductName = product.Name,
                Price = product.Price,
                Quantity = 1,
                ImageUrl = product.ImageUrl
            };

            var buyNowCart = new List<CartItem> { buyNowItem };
            HttpContext.Session.SetObject("BuyNowCart", buyNowCart);

            // Go directly to Place Order page
            return RedirectToAction("PlaceOrder");
        }
        [HttpPost]
        [Authorize]
        public IActionResult ProceedBuyNow(int productId, int quantity)
        {
            if (HttpContext.Session.GetString("UserRole") == null)
            {
                TempData["Error"] = "Please login to purchase products!";
                return RedirectToAction("Login", "Account");
            }

            var product = _context.Products.Find(productId);
            if (product == null) return NotFound();

            // Store buy now data in session
            HttpContext.Session.SetInt32("BuyNowProductId", productId);
            HttpContext.Session.SetInt32("BuyNowQuantity", quantity);
            HttpContext.Session.SetString("IsBuyNow", "true");

            // Create a cart item for Buy Now
            var buyNowItem = new CartItem
            {
                ProductId = product.ProductId,
                ProductName = product.Name,
                Price = product.Price,
                Quantity = quantity,
                ImageUrl = product.ImageUrl
            };

            var buyNowCart = new List<CartItem> { buyNowItem };
            HttpContext.Session.SetObject("BuyNowCart", buyNowCart);

            // Go directly to PlaceOrder page (no extra page)
            return RedirectToAction("PlaceOrder");
        }

        // ==================== CART OPERATIONS ====================

        // Allow both GET and POST for AddToCart
        [HttpGet]
        [HttpPost]
        public IActionResult AddToCart(int id, int quantity = 1)
        {
            try
            {
                // Check if user is logged in
                if (HttpContext.Session.GetString("UserRole") == null)
                {
                    // Return JSON for AJAX, or redirect for direct access
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest" || Request.ContentType == "application/json")
                    {
                        return Json(new { success = false, message = "Please login to add items to cart!", redirect = "/Account/Login" });
                    }
                    TempData["Error"] = "Please login to add items to cart!";
                    return RedirectToAction("Login", "Account");
                }

                var product = _context.Products.Find(id);
                if (product == null)
                {
                    return Json(new { success = false, message = "Product not found!" });
                }

                var cart = HttpContext.Session.GetObject<List<CartItem>>("Cart") ?? new List<CartItem>();
                var existingItem = cart.FirstOrDefault(c => c.ProductId == id);

                if (existingItem != null)
                {
                    existingItem.Quantity += quantity;
                }
                else
                {
                    cart.Add(new CartItem
                    {
                        ProductId = product.ProductId,
                        ProductName = product.Name,
                        Price = product.Price,
                        Quantity = quantity,
                        ImageUrl = product.ImageUrl
                    });
                }

                HttpContext.Session.SetObject("Cart", cart);

                int totalItems = cart.Sum(c => c.Quantity);

                // Return JSON for AJAX requests
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest" || Request.ContentType == "application/json")
                {
                    return Json(new { success = true, message = $"{product.Name} added to cart!", cartCount = totalItems });
                }

                // Redirect for direct access
                TempData["Success"] = $"{product.Name} added to cart!";
                return RedirectToAction("Index", "Cart");
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }
        public IActionResult Index()
        {
            var cart = HttpContext.Session.GetObject<List<CartItem>>("Cart") ?? new List<CartItem>();
            return View(cart);
        }

        [HttpPost]
        public IActionResult UpdateCart(int productId, int quantity)
        {
            var cart = HttpContext.Session.GetObject<List<CartItem>>("Cart") ?? new List<CartItem>();
            var item = cart.FirstOrDefault(c => c.ProductId == productId);
            if (item != null)
            {
                if (quantity <= 0)
                    cart.Remove(item);
                else
                    item.Quantity = quantity;
            }
            HttpContext.Session.SetObject("Cart", cart);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult RemoveFromCart(int productId)
        {
            var cart = HttpContext.Session.GetObject<List<CartItem>>("Cart") ?? new List<CartItem>();
            var item = cart.FirstOrDefault(c => c.ProductId == productId);
            if (item != null)
            {
                cart.Remove(item);
                HttpContext.Session.SetObject("Cart", cart);
                return Json(new { success = true });
            }
            return Json(new { success = false });
        }

        // ==================== CHECKOUT FROM CART ====================

        [Authorize]
        public IActionResult CheckoutFromCart()
        {
            if (HttpContext.Session.GetString("UserRole") == null)
            {
                TempData["Error"] = "Login required!";
                return RedirectToAction("Login", "Account");
            }

            var cart = HttpContext.Session.GetObject<List<CartItem>>("Cart");
            if (cart == null || !cart.Any())
            {
                TempData["Error"] = "Your cart is empty!";
                return RedirectToAction("Index", "Home");
            }

            HttpContext.Session.SetObject("CheckoutCart", cart);
            HttpContext.Session.SetString("IsBuyNow", "false");

            return RedirectToAction("PlaceOrder");
        }

        // ==================== PLACE ORDER PAGE ====================

        [Authorize]
        public IActionResult PlaceOrder()
        {
            var isBuyNow = HttpContext.Session.GetString("IsBuyNow") == "true";
            List<CartItem> cartItems;

            if (isBuyNow)
            {
                cartItems = HttpContext.Session.GetObject<List<CartItem>>("BuyNowCart");
                if (cartItems == null || !cartItems.Any())
                {
                    return RedirectToAction("Index", "Home");
                }

                // Get full product details from database for Buy Now
                var productId = cartItems.First().ProductId;
                var product = _context.Products.Find(productId);
                if (product == null)
                {
                    return RedirectToAction("Index", "Home");
                }

                ViewBag.Product = product;
                ViewBag.Quantity = cartItems.First().Quantity;
                ViewBag.CartItems = cartItems;
                ViewBag.TotalAmount = product.Price * cartItems.First().Quantity;
                ViewBag.IsBuyNow = true;
            }
            else
            {
                cartItems = HttpContext.Session.GetObject<List<CartItem>>("CheckoutCart");
                if (cartItems == null || !cartItems.Any())
                {
                    TempData["Error"] = "Your cart is empty!";
                    return RedirectToAction("Index", "Cart");
                }

                decimal totalAmount = cartItems.Sum(item => item.Price * item.Quantity);

                ViewBag.CartItems = cartItems;
                ViewBag.TotalAmount = totalAmount;
                ViewBag.IsBuyNow = false;
            }

            return View();
        }

        [HttpPost]
        [Authorize]
        public IActionResult ProceedToLocation()
        {
            var isBuyNow = HttpContext.Session.GetString("IsBuyNow") == "true";
            List<CartItem> cartItems;

            if (isBuyNow)
            {
                cartItems = HttpContext.Session.GetObject<List<CartItem>>("BuyNowCart");
                if (cartItems == null || !cartItems.Any())
                {
                    return RedirectToAction("Index", "Home");
                }

                // Store the quantity for later
                var quantity = cartItems.First().Quantity;
                var productId = cartItems.First().ProductId;
                HttpContext.Session.SetInt32("BuyNowQuantity", quantity);
                HttpContext.Session.SetInt32("BuyNowProductId", productId);
            }
            else
            {
                cartItems = HttpContext.Session.GetObject<List<CartItem>>("CheckoutCart");
                if (cartItems == null || !cartItems.Any())
                {
                    TempData["Error"] = "Your cart is empty!";
                    return RedirectToAction("Index", "Cart");
                }
            }

            // Store cart for location confirmation
            HttpContext.Session.SetObject("OrderCart", cartItems);

            return RedirectToAction("ConfirmLocation");
        }
        // ==================== LOCATION CONFIRMATION ====================

        [Authorize]
        public IActionResult ConfirmLocation()
        {
            var cart = HttpContext.Session.GetObject<List<CartItem>>("OrderCart");

            if (cart == null || !cart.Any())
            {
                TempData["Error"] = "No items to checkout!";
                return RedirectToAction("Index", "Home");
            }

            decimal totalAmount = cart.Sum(item => item.Price * item.Quantity);

            ViewBag.CartItems = cart;
            ViewBag.TotalAmount = totalAmount;

            return View();
        }
        [HttpPost]
        public IActionResult ConfirmLocationProceed(string location, string address)
        {
            var cart = HttpContext.Session.GetObject<List<CartItem>>("OrderCart");
            var storedAddress = HttpContext.Session.GetString("OrderAddress");

            if (cart == null || !cart.Any())
            {
                TempData["Error"] = "No items to checkout!";
                return RedirectToAction("Index", "Home");
            }

            // Create order details
            var orderDetails = new List<OrderDetail>();
            decimal totalAmount = 0;

            foreach (var item in cart)
            {
                var product = _context.Products.Find(item.ProductId);
                if (product == null) continue;

                orderDetails.Add(new OrderDetail
                {
                    ProductId = product.ProductId,
                    Quantity = item.Quantity,
                    Price = product.Price
                });
                totalAmount += product.Price * item.Quantity;
            }

            var order = new Order
            {
                UserId = HttpContext.Session.GetString("UserId") ?? "guest",
                Name = HttpContext.Session.GetString("UserName"),
                Address = storedAddress ?? address,
                Location = location,
                Status = "Pending",
                OrderDate = DateTime.Now,
                TotalAmount = totalAmount,
                OrderDetails = orderDetails
            };

            _context.Orders.Add(order);
            _context.SaveChanges();

            // Clear session data
            HttpContext.Session.Remove("Cart");
            HttpContext.Session.Remove("CheckoutCart");
            HttpContext.Session.Remove("BuyNowCart");
            HttpContext.Session.Remove("OrderCart");
            HttpContext.Session.Remove("OrderAddress");
            HttpContext.Session.Remove("IsBuyNow");
            HttpContext.Session.SetInt32("LastOrderId", order.OrderId);

            return RedirectToAction("Payment", new { orderId = order.OrderId });
        }
        [HttpPost]
        public IActionResult ProceedToPayment(string location, string address)
        {
            var cart = HttpContext.Session.GetObject<List<CartItem>>("OrderCart");

            if (cart == null || !cart.Any())
            {
                TempData["Error"] = "No items to checkout!";
                return RedirectToAction("Index", "Home");
            }

            var orderDetails = new List<OrderDetail>();
            decimal totalAmount = 0;

            foreach (var item in cart)
            {
                var product = _context.Products.Find(item.ProductId);
                if (product == null) continue;

                orderDetails.Add(new OrderDetail
                {
                    ProductId = product.ProductId,
                    Quantity = item.Quantity,
                    Price = product.Price
                });
                totalAmount += product.Price * item.Quantity;
            }

            var order = new Order
            {
                UserId = HttpContext.Session.GetString("UserId") ?? "guest",
                Name = HttpContext.Session.GetString("UserName"),
                Address = address,
                Location = location,
                Status = "Pending",
                OrderDate = DateTime.Now,
                TotalAmount = totalAmount,
                OrderDetails = orderDetails
            };

            _context.Orders.Add(order);
            _context.SaveChanges();

            // Clear session
            HttpContext.Session.Remove("Cart");
            HttpContext.Session.Remove("CheckoutCart");
            HttpContext.Session.Remove("BuyNowCart");
            HttpContext.Session.Remove("OrderCart");
            HttpContext.Session.Remove("IsBuyNow");
            HttpContext.Session.SetInt32("LastOrderId", order.OrderId);

            return RedirectToAction("Payment", new { orderId = order.OrderId });
        }
        [HttpPost]
        public IActionResult UpdateLocationQuantity(int productId, int quantity)
        {
            var isBuyNow = HttpContext.Session.GetString("IsBuyNow") == "true";
            List<CartItem> cartItems;

            if (isBuyNow)
            {
                cartItems = HttpContext.Session.GetObject<List<CartItem>>("BuyNowCart");
            }
            else
            {
                cartItems = HttpContext.Session.GetObject<List<CartItem>>("CheckoutCart");
            }

            if (cartItems != null)
            {
                var item = cartItems.FirstOrDefault(c => c.ProductId == productId);
                if (item != null)
                {
                    item.Quantity = quantity;
                    HttpContext.Session.SetObject(isBuyNow ? "BuyNowCart" : "CheckoutCart", cartItems);
                    return Json(new { success = true });
                }
            }
            return Json(new { success = false });
        }
        // ==================== PAYMENT ====================

        public IActionResult Payment(int orderId)
        {
            var order = _context.Orders.FirstOrDefault(o => o.OrderId == orderId);
            if (order == null) return RedirectToAction("Index", "Home");
            return View(order);
        }

        [HttpPost]
        public IActionResult PaymentSuccess(int orderId)
        {
            var order = _context.Orders.FirstOrDefault(o => o.OrderId == orderId);
            if (order != null)
            {
                order.Status = "Paid";
                _context.SaveChanges();
            }
            TempData["Success"] = "Payment successful! Your order has been placed.";
            return RedirectToAction("TrackOrder", new { orderId = orderId });
        }

        public IActionResult TrackOrder(int orderId)
        {
            var order = _context.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .FirstOrDefault(o => o.OrderId == orderId);
            if (order == null) return RedirectToAction("Index", "Home");
            return View(order);
        }

        public IActionResult MyOrders()
        {
            var userId = HttpContext.Session.GetString("UserId");
            var orders = _context.Orders
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.OrderDate)
                .ToList();
            return View(orders);
        }
    }
}