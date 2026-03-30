using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WebApplication_shravani.Models
{
    public class Farmer
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string Phone { get; set; } = string.Empty;

        public string? Email { get; set; }
        public string? Location { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? Area { get; set; }
        public string? FarmType { get; set; }
        public string? Certification { get; set; }
        public DateTime? CertificationDate { get; set; }
        public string? CertificateNumber { get; set; }
        public string? CertificateImage { get; set; }
        public DateTime? CreatedDate { get; set; }

        public virtual ICollection<Product> Products { get; set; } = new List<Product>();
    }
}