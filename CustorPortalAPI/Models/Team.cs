using System.ComponentModel.DataAnnotations;

namespace CustorPortalAPI.Models
{
    public class Team
    {
        public int TeamKey { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = null!;
        
        // Add TeamName property for backward compatibility
        public string TeamName 
        { 
            get => Name; 
            set => Name = value; 
        }
        
        [MaxLength(500)]
        public string? Description { get; set; }
        
        public DateTime Created_At { get; set; }
        
        public DateTime? Updated_At { get; set; }
        
        public bool Is_Active { get; set; } = true;
        
        // Navigation properties
        public virtual ICollection<TeamMember>? TeamMembers { get; set; }
        
        // Add Users navigation property for backward compatibility
        public virtual ICollection<User>? Users { get; set; }
    }
}
