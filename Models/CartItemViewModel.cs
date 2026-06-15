namespace haru.market.Models
{
    public class CartItemViewModel
    {
        public string ProductId { get; set; } = string.Empty;

        public string ProductName { get; set; } = string.Empty;

        public decimal Price { get; set; }

        public string ImageUrl { get; set; } = string.Empty;

        public int Quantity { get; set; }
    }
}
