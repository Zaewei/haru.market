using System.ComponentModel.DataAnnotations;

namespace haru.market.Models
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Name is required")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email structure (missing @ or domain)")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Delivery address is required")]
        public string DeliveryAddress { get; set; } = string.Empty;

        [Required(ErrorMessage = "Contact details are required")]
        public string ContactDetails { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters long")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
    }
}