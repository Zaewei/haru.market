using System;

namespace haru.market.Models
{
    public class OrderViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string Status { get; set; } = "pending";
        public string PaymentMethod { get; set; } = "Gcash";
        public decimal Total { get; set; }
        public string Address { get; set; } = string.Empty;
    }
}