using System.ComponentModel.DataAnnotations;

namespace CustorPortalAPI.Models
{
    public class Notification
    {
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(500)]
        public string Message { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(50)]
        public string Type { get; set; } = string.Empty; // file_upload, comment, task_assigned, task_updated
        
        public bool IsRead { get; set; } = false;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public int? RelatedId { get; set; } // File ID, Comment ID, or Task ID
        
        [MaxLength(50)]
        public string? RelatedType { get; set; } // file, comment, task
        
        // Navigation property
        public User User { get; set; } = null!;
    }
}