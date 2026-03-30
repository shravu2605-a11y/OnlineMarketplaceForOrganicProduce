using System.Collections.Generic;

namespace WebApplication_shravani.Models.ViewModels
{
    public class FarmerDashboardViewModel
    {
        public int FarmerId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? Area { get; set; }
        public string? Location { get; set; }
        public string? FarmType { get; set; }
        public string? Certification { get; set; }
        public string? CertificateImage { get; set; }
        public int TotalProducts { get; set; }
        public int TotalOrders { get; set; }
        public decimal TotalEarnings { get; set; }
        public List<ProductSalesViewModel> ProductSales { get; set; } = new List<ProductSalesViewModel>();
    }

    public class ProductSalesViewModel
    {
        public string ProductName { get; set; } = string.Empty;
        public int QuantitySold { get; set; }
        public decimal TotalRevenue { get; set; }
    }
}