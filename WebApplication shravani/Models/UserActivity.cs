using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication_shravani.Models
{
    public class UserActivity
    {
        [Key]
        public int Id { get; set; }

        public string UserId { get; set; } = string.Empty;
        public string? Action { get; set; }
        public DateTime? ActivityTime { get; set; }

        [ForeignKey("UserId")]
        public virtual User? User { get; set; }
    }
}