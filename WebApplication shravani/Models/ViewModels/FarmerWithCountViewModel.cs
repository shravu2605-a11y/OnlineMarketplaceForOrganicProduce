using System;

namespace WebApplication_shravani.Models.ViewModels
{
    public class FarmerWithCountViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? Area { get; set; }
        public string? FarmType { get; set; }
        public string? Certification { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int ProductCount { get; set; }
    }
}