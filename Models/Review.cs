using CarBook.Models;

namespace CarBook.Models
{
    public class Review
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public int Rating { get; set; }
        public string Comment { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public virtual Product Product { get; set; }
    }

}
