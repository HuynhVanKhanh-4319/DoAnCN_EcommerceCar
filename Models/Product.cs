using CarBook.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CarBook.Models
{

    public partial class Product : BaseModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Giá phải lớn hơn hoặc bằng 0.")]
        public decimal Price { get; set; }

        public int Amount { get; set; }

        [ForeignKey(nameof(Category))]
        public int CategoryId { get; set; }
        public virtual Category? Category { get; set; }

        [ForeignKey(nameof(Brand))]
        public int BrandId { get; set; }
        public virtual Brand? Brand { get; set; }

        public virtual ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
        public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();

        public bool IsActive { get; set; }

        // Liên kết với bảng Discount
        public virtual ICollection<Discount> Discounts { get; set; } = new List<Discount>();

        // Tính giá cuối cùng sau khi áp dụng giảm giá
        [NotMapped]
        public decimal FinalPrice
        {
            get
            {
                var activeDiscount = Discounts.FirstOrDefault(d => d.IsActive);
                if (activeDiscount != null)
                {
                    // Áp dụng giảm giá theo phần trăm
                    return Price * (1 - activeDiscount.Percentage / 100);
                }

                // Nếu không có giảm giá, trả về giá gốc
                return Price;
            }
        }
        public string Status { get; set; } = "Available";
    }

}
