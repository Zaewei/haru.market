using System.Collections.Generic;

namespace haru.market.Models
{
    public class AdminDashboardViewModel
    {
        public int TotalProducts { get; set; }
        public int TotalUsers { get; set; }
        public int TotalLookbooks { get; set; } 
        public int TotalViews { get; set; }     

        public List<LookbookViewModel> RecentLookbooks { get; set; } = new List<LookbookViewModel>();
    
        public List<ProductViewModel> RecentProducts { get; set; } = new List<ProductViewModel>();
    }
}