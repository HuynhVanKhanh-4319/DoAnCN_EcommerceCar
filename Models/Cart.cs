using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CarBook.Models
{
    public class Cart
    {
        [Key]
        public int Id { get; set; }

        public int ProductId { get; set; }

        [ForeignKey("ProductId")]
        [ValidateNever]
        public virtual Product Products { get; set; } // Thêm virtual để hỗ trợ lazy loading

        public string ApplicationUserId { get; set; } = string.Empty;

        [ForeignKey("ApplicationUserId")]
        [ValidateNever]
        public virtual ApplicationUser ApplicationUser { get; set; } // Thêm virtual để hỗ trợ lazy loading

        [NotMapped]
        public double ProductPrice { get; set; }
        public DateTime StartDate { get; set; } = DateTime.Now;// Ngày đặt
        public DateTime EndDate { get; set; } = DateTime.Now;  // Ngày trả
        public int Days => (EndDate - StartDate).Days ;

    }
}
