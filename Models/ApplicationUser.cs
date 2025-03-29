using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace CarBook.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        public string? Name { get; set; } = string.Empty;
        public string? Address { get; set; } = string.Empty;
        public string? CitizenID { get; set; } = string.Empty;
        public string? Image { get; set; } = string.Empty;


    }
}
