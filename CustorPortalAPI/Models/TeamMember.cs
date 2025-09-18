namespace CustorPortalAPI.Models
{
    public class TeamMember
    {
        public int TeamKey { get; set; }
        public int UserKey { get; set; }
        public DateTime Joined_At { get; set; }
        public bool Is_Active { get; set; } = true;
        
        // Navigation properties
        public virtual Team? Team { get; set; }
        public virtual User? User { get; set; }
    }
}
