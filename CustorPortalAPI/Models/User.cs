
namespace CustorPortalAPI.Models
  {
public  class User
{
    public int UserKey { get; set; }

    public string Email { get; set; } = null!;

    public string Password_Hash { get; set; } = null!;

    public string? First_Name { get; set; }

    public string? Last_Name { get; set; }

    public int RoleKey { get; set; }

    public DateTime Created_At { get; set; }

    public DateTime? Updated_At { get; set; }

    public bool? Is_Active { get; set; }

    public int? TeamKey { get; set; }

    public Role? Role { get; set; }
    public Team? Team { get; set; }
    }

 }
