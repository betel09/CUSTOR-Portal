
namespace CustorPortalAPI.Models
{

public class Comment
{
    public int Id { get; set; }
    public int? TaskId { get; set; }
    public int? DocumentId { get; set; }
    public string? Text { get; set; } 
    public int UserId { get; set; }
    public DateTime Timestamp { get; set; }
    public string? Mentions { get; set; } 
    public string? TargetUserId { get; set; } // New field for targeting specific users/teams
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }

        public Task? Task { get; set; }
        public File? Document { get; set; }
        public User? User { get; set; }
    }
}