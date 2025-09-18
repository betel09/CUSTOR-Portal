namespace CustorPortalAPI.Models
{
    public class NotificationRequest
    {
        public int UserId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public int? RelatedId { get; set; }
        public string? RelatedType { get; set; }
    }
}
