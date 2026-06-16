using System;

namespace haru.market.Models
{
    public class AdminUserViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime JoinedAt { get; set; }
        public string Role { get; set; } = "Customer";
        public string Phone { get; set; } = "N/A";
        public string Address { get; set; } = "No address on file";
        public string Status { get; set; } = "Active";
        public DateTime LastActive { get; set; }

        
    }
}