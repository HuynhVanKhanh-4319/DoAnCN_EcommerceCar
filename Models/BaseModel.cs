using CarBook.Models;

namespace CarBook.Models
{
    public class BaseModel
    {
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get;set; }
        public virtual ApplicationUser?  CreatedBy { get; set; }
        public virtual ApplicationUser?  UpdatedBy { get;set; } 
    }
}
