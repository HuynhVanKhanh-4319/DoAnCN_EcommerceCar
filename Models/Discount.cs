using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace CarBook.Models
{
    public class Discount
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [ForeignKey(nameof(Product))]
        public int ProductId { get; set; }
        public virtual Product Product { get; set; }

        [Required]
        [Range(0, 100, ErrorMessage = "Phần trăm giảm giá phải nằm trong khoảng từ 0% đến 100%.")]
        public decimal Percentage { get; set; } // Giảm giá theo phần trăm

        [Required]
        public DateTime StartDate { get; set; } // Ngày bắt đầu áp dụng giảm giá

        [Required]
        public DateTime EndDate { get; set; } // Ngày kết thúc áp dụng giảm giá

        [NotMapped]
        public bool IsActive => DateTime.Now >= StartDate && DateTime.Now <= EndDate; // Kiểm tra giảm giá có hiệu lực
    }
}
