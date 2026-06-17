using System.ComponentModel.DataAnnotations;

namespace haru.market.Models
{
    public class ProductViewModel
    {
        public string? Id { get; set; }

        [Required(ErrorMessage = "Product name is required.")]
        [StringLength(100, ErrorMessage = "Product name cannot exceed 100 characters.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Description is required.")]
        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters.")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Price is required.")]
        [Range(0.01, 10000.00, ErrorMessage = "Price must be a positive value between 0.01 and 10,000.00.")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Initial inventory stock count is required.")]
        [Range(0, 5000, ErrorMessage = "Initial stock must be between 0 and 5,000 units.")]
        public int StockQuantity { get; set; }

        [Url(ErrorMessage = "Please provide a valid image URL layout.")]
        public string ImageUrl { get; set; } = string.Empty;
        public string Imageurl2{ get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public string Color { get; set; } = "";
        public string Size { get; set; } = "";

        public string GroupKey { get; set; } = "";
    }
}