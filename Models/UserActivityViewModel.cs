using System;

namespace haru.market.Models
{
    public class UserActivityViewModel
    {
        public int TotalOrders { get; set; } = 0;
        public int ProductsPurchased { get; set; } = 0;
        public double TotalSpent { get; set; } = 0;
        public int LookbooksViewed { get; set; } = 0;
        public int WishlistItems { get; set; } = 0;
        public List<OrderViewModel> RecentOrders { get; set; } = new List<OrderViewModel>();
    }
}