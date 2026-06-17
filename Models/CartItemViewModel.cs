using System;

namespace haru.market.Models
{
    public class CartItemViewModel
    {
        public string ProductId { get; set; } = "";
        public string ProductName { get; set; } = "";
        public decimal Price { get; set; }
        public string ImageUrl { get; set; } = "";
        public int Quantity { get; set; }
        public string Size { get; set; } = "M"; 
    }
}