namespace haru.market.Models
{
    public class FavoriteItemViewModel
    {
        public string ProductId { get; set; } = "";
        public string ProductName { get; set; } = "";
        public decimal Price { get; set; }
        public string ImageUrl { get; set; } = "";
        public string Color { get; set; } = "";
        public string GroupKey { get; set; } = "";
    }
}
