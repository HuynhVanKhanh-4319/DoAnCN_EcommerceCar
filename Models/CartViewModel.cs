using CarBook.Models;
using Microsoft.EntityFrameworkCore;

namespace CarBook.Models
{
    [Keyless]
    public class CartViewModel
    {
        public virtual Cart? Cart { get; set; }
        public double TotalPrice { get; set; } 

        public virtual Bill? Bills { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
    }
}
