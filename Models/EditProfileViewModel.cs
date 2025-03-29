using System.ComponentModel.DataAnnotations;

namespace CarBook.Models
{
    public class EditProfileViewModel
    {
        [Required]
        [Display(Name = "Full Name")]
        public string Name { get; set; }

        [Required]
        [Display(Name = "Address")]
        public string Address { get; set; }

        [Phone]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; }
        public string? CitizenID { get; set; }
        public string? ExistingImagePath { get; set; }

        [Display(Name = "Profile Picture")]
        public IFormFile? Image { get; set; }

    }
}
