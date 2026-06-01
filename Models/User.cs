using System;

namespace haru.market.Models
{
    public class User
    {
        public string Id { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string DeliveryAddress { get; set; } = string.Empty;
        public string ContactDetails { get; set; } = string.Empty;
        public string Role { get; set; } = "buyer"; 
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}