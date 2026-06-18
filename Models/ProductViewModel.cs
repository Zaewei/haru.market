using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Linq;

namespace haru.market.Models
{
    public class ProductViewModel
    {
        public string? Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public decimal Price { get; set; }

        [Url]
        public string ImageUrl { get; set; } = string.Empty;
        public string Imageurl2 { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string Color { get; set; } = "";
        public string Size { get; set; } = "";
        public string GroupKey { get; set; } = "";
        public Dictionary<string, int> StockQuantity { get; set; } = new Dictionary<string, int>();
        public int TotalStock => StockQuantity != null ? StockQuantity.Values.Sum() : 0;
    }
}