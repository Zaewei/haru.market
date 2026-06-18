using System.Collections.Generic;

namespace haru.market.Models
{
    public class CustomerInsightViewModel
    {
        public int TotalCustomers { get; set; }
        public int ActiveCustomers { get; set; }
        public int InactiveCustomers { get; set; }
        public int UniqueDeliveryZones { get; set; }
        public string TopDeliveryZone { get; set; } = "N/A";

        public Dictionary<string, int> DeliveryZoneDistribution { get; set; } = new();

        public Dictionary<string, int> RegistrationTrend { get; set; } = new();

        public Dictionary<string, int> StatusDistribution { get; set; } = new();

        public List<DeliveryZoneSummary> TopDeliveryZones { get; set; } = new();

        public List<AdminUserViewModel> RecentCustomers { get; set; } = new();
    }

    public class DeliveryZoneSummary
    {
        public string Zone { get; set; } = string.Empty;
        public int CustomerCount { get; set; }
        public int OrderCount { get; set; }
        public double Percentage { get; set; }
    }
}
