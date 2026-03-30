using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication_shravani.Data;
using WebApplication_shravani.Models;
using System.Threading.Tasks;
using System.Linq;

namespace WebApplication_shravani.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AccountController(
            AppDbContext context,
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
        }

        [HttpGet]
        public IActionResult Login()
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (!string.IsNullOrEmpty(userRole))
            {
                if (userRole == "Admin")
                    return RedirectToAction("Dashboard", "Admin");
                else if (userRole == "Farmer")
                    return RedirectToAction("Dashboard", "FarmerPanel");
                else if (userRole == "User")
                    return RedirectToAction("Index", "Home");
            }
            return View("LoginRegister");
        }

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password, string? returnUrl = null)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                TempData["Error"] = "Phone Number/Email and Password required!";
                return RedirectToAction("Login");
            }

            // ADMIN LOGIN
            if (email.Trim().ToLower() == "admin@gmail.com" && password == "admin123")
            {
                var adminUser = await _userManager.FindByEmailAsync(email);
                if (adminUser == null)
                {
                    adminUser = new User
                    {
                        UserName = "admin",
                        Email = "admin@gmail.com",
                        FullName = "Admin",
                        CreatedDate = DateTime.Now,
                        Role = "Admin"
                    };
                    await _userManager.CreateAsync(adminUser, "admin123");

                    if (!await _roleManager.RoleExistsAsync("Admin"))
                        await _roleManager.CreateAsync(new IdentityRole("Admin"));
                    await _userManager.AddToRoleAsync(adminUser, "Admin");
                }

                await _signInManager.SignInAsync(adminUser, false);
                HttpContext.Session.SetString("UserRole", "Admin");
                TempData["Success"] = "Welcome Admin!";
                return RedirectToAction("Dashboard", "Admin");
            }

            // FARMER LOGIN - Phone number
            var farmer = _context.Farmers.FirstOrDefault(f => f.Phone == email.Trim());

            if (farmer != null)
            {
                // Check password (simple check for farmer123)
                if (password == "farmer123")
                {
                    // Find or create farmer user
                    var farmerUser = await _userManager.FindByNameAsync(farmer.Phone);

                    if (farmerUser == null)
                    {
                        farmerUser = new User
                        {
                            UserName = farmer.Phone,
                            Email = farmer.Email ?? $"{farmer.Phone}@farmer.com",
                            FullName = farmer.Name,
                            CreatedDate = DateTime.Now,
                            Role = "Farmer"
                        };

                        var result = await _userManager.CreateAsync(farmerUser, "farmer123");
                        if (!result.Succeeded)
                        {
                            TempData["Error"] = "Failed to create farmer account";
                            return RedirectToAction("Login");
                        }

                        if (!await _roleManager.RoleExistsAsync("Farmer"))
                            await _roleManager.CreateAsync(new IdentityRole("Farmer"));
                        await _userManager.AddToRoleAsync(farmerUser, "Farmer");
                    }

                    await _signInManager.SignInAsync(farmerUser, false);
                    HttpContext.Session.SetString("UserRole", "Farmer");
                    HttpContext.Session.SetInt32("FarmerId", farmer.Id);
                    HttpContext.Session.SetString("FarmerName", farmer.Name);
                    TempData["Success"] = $"Welcome {farmer.Name}!";
                    return RedirectToAction("Dashboard", "FarmerPanel");
                }
                else
                {
                    TempData["Error"] = "Invalid password! Use: farmer123";
                    return RedirectToAction("Login");
                }
            }

            // USER LOGIN
            var user = await _userManager.FindByEmailAsync(email);
            if (user != null && await _userManager.CheckPasswordAsync(user, password))
            {
                await _signInManager.SignInAsync(user, false);
                HttpContext.Session.SetString("UserRole", "User");
                HttpContext.Session.SetString("UserId", user.Id);
                HttpContext.Session.SetString("UserName", user.UserName);
                TempData["Success"] = $"Welcome {user.UserName}!";

                if (!string.IsNullOrEmpty(returnUrl))
                    return Redirect(returnUrl);
                return RedirectToAction("Index", "Home");
            }

            TempData["Error"] = "Invalid credentials!";
            return RedirectToAction("Login");
        }

        [HttpPost]
        public async Task<IActionResult> Register(string username, string email, string password, string confirmPassword)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(username))
            {
                TempData["Error"] = "All fields required!";
                return RedirectToAction("Login");
            }

            if (password != confirmPassword)
            {
                TempData["Error"] = "Passwords do not match!";
                return RedirectToAction("Login");
            }

            if (await _userManager.FindByEmailAsync(email) != null)
            {
                TempData["Error"] = "Email already exists!";
                return RedirectToAction("Login");
            }

            if (await _userManager.FindByNameAsync(username) != null)
            {
                TempData["Error"] = "Username already taken!";
                return RedirectToAction("Login");
            }

            var user = new User
            {
                UserName = username,
                Email = email,
                FullName = username,
                CreatedDate = DateTime.Now,
                Role = "User"
            };

            var result = await _userManager.CreateAsync(user, password);

            if (result.Succeeded)
            {
                if (!await _roleManager.RoleExistsAsync("User"))
                    await _roleManager.CreateAsync(new IdentityRole("User"));
                await _userManager.AddToRoleAsync(user, "User");

                TempData["Success"] = "Registration successful! Please login.";
                return RedirectToAction("Login");
            }

            TempData["Error"] = string.Join(", ", result.Errors.Select(e => e.Description));
            return RedirectToAction("Login");
        }

        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            HttpContext.Session.Clear();
            TempData["Success"] = "Logged out successfully!";
            return RedirectToAction("Index", "Home");
        }
        [HttpGet]
        public IActionResult CheckLoginStatus()
        {
            var isLoggedIn = User.Identity?.IsAuthenticated == true;
            return Json(new { isLoggedIn = isLoggedIn });
        }
    }
}