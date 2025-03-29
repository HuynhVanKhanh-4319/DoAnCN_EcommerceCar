using CarBook.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CarBook.Models
{
    public class Bill
    {     
            [Key]
            public int Id { get; set; }

            public string ApplicationUserId { get; set; }

            [ForeignKey("ApplicationUserId")]
            [ValidateNever]
            public virtual ApplicationUser ApplicationUsers { get; set; } // Thêm virtual để hỗ trợ lazy loading

            public double Total { get; set; }
            public DateTime OrderDate { get; set; }
            public DateTime? StartDate { get; set; } // Ngày đặt
            public DateTime? EndDate { get; set; }   // Ngày trả
            public string? OrderStatus { get; set; } = string.Empty;
            public string PhoneNumber { get; set; } = string.Empty;
            public string Name { get; set; } = string.Empty;
           public string Address { get; set; } = string.Empty;
           public virtual ICollection<DetailBill> DetailBills { get; set; } = new List<DetailBill>();


    }
}
