using CarBook.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace  CarBook.Models
{
    public class DetailBill
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int BillId { get; set; }

        [ForeignKey("BillId")]
        [ValidateNever]
        public virtual Bill Bills { get; set; } // Thêm virtual để hỗ trợ lazy loading

        [Required]
        public int ProductId { get; set; }

        [ForeignKey("ProductId")]
        [ValidateNever]
        public virtual Product Products { get; set; } // Thêm virtual để hỗ trợ lazy loading

        public int Days { get; set; } // Số ngày thuê sản phẩm

       // public double TotalPrice => ProductPrice * Days;
        public double ProductPrice { get; set; }

    }
}
