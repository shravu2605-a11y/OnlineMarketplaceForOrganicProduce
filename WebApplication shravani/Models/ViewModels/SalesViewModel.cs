namespace WebApplication_shravani.Models.ViewModels
{
    public class SalesViewModel
    {
        public string ProductName { get; set; } = string.Empty;
        public int TotalSold { get; set; }
        public decimal TotalRevenue { get; set; }  // Add this property
    }
}