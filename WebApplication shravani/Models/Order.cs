using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication_shravani.Models
{
    public class Order
    {
        [Key]
        public int OrderId { get; set; }

        public string? UserId { get; set; }

        public string? Name { get; set; }

        public string? Address { get; set; }

        public string? Location { get; set; }  // ← Add this property

        public string? Status { get; set; }

        public decimal? TotalAmount { get; set; }

        public DateTime? OrderDate { get; set; }

        public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();

        [ForeignKey("UserId")]
        public virtual User? User { get; set; }
    }
}