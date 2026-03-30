using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication_shravani.Models
{
    public class LoginHistory
    {
        [Key]
        public int Id { get; set; }

        public string UserId { get; set; } = string.Empty;
        public DateTime? LoginTime { get; set; }
        public string? IpAddress { get; set; }

        [ForeignKey("UserId")]
        public virtual User? User { get; set; }
    }
}