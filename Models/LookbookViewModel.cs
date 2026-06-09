using System;
using System.ComponentModel.DataAnnotations;

namespace haru.market.Models
{
    public class LookbookViewModel
    {
        public string? Id { get; set; } // Firestore Document ID tracking reference

        [Required(ErrorMessage = "Campaign theme title is required.")]
        [StringLength(100, ErrorMessage = "Campaign title cannot exceed 100 characters.")]
        public string ThemeTitle { get; set; } = string.Empty;

        [Required(ErrorMessage = "Story narrative / description is required.")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Media attachment link is required.")]
        [Url(ErrorMessage = "Please provide a valid image or video URL layout.")]
        public string MediaUrl { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}