using Microsoft.AspNetCore.Identity;
using System;

namespace WebApplication_shravani.Models
{
    public class User : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? Area { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string? Role { get; set; }
    }
}