using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace haru.market.Models
{
    public class OrderPlacementViewModel
    {
        [Required(ErrorMessage = "Full name is required for delivery optimization.")]
        public string CustomerName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Shipping destination address is required.")]
        public string ShippingAddress { get; set; } = string.Empty;

        [Required(ErrorMessage = "Contact email is required.")]
        [EmailAddress(ErrorMessage = "Invalid formatting string for contact details.")]
        public string CustomerEmail { get; set; } = string.Empty;

        // The active shopping basket items to be registered
        public List<OrderItemModel> Items { get; set; } = new List<OrderItemModel>();
    }
}