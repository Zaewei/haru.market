using System;
using Google.Cloud.Firestore;

namespace haru.market.Models
{
    [FirestoreData]
    public class AdminUserViewModel
    {
        [FirestoreProperty("uid")]
        public string Id { get; set; } = string.Empty;

        [FirestoreProperty("FullName")]
        public string Name { get; set; } = string.Empty;

        [FirestoreProperty("Email")]
        public string Email { get; set; } = string.Empty;

        [FirestoreProperty("JoinedAt")]
        public DateTime JoinedAt { get; set; }

        [FirestoreProperty("Role")]
        public string Role { get; set; } = "Customer";

        [FirestoreProperty("ContactDetails")]
        public string Phone { get; set; } = "N/A";

        [FirestoreProperty("DeliveryAddress")]
        public string Address { get; set; } = "No address on file";

        [FirestoreProperty("Status")]
        public string Status { get; set; } = "Active";

        [FirestoreProperty("LastActive")]
        public DateTime LastActive { get; set; }
    }
}