


using CustorPortalAPI.Data;
using CustorPortalAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CustorPortalAPI.Controllers
{
    [Route("api/team-management")]
    [ApiController]
    [Authorize]
    public class TeamManagementController : ControllerBase
    {
        private readonly CustorPortalDbContext _context;

        public TeamManagementController(CustorPortalDbContext context)
        {
            _context = context;
        }

        // Test endpoint to check Teams table
        [HttpGet("test-teams-table")]
        public async Task<IActionResult> TestTeamsTable()
        {
            try
            {
                var teamCount = await _context.Teams.CountAsync();
                var teamMembersCount = await _context.TeamMembers.CountAsync();
                
                return Ok(new { 
                    message = "Teams table test successful",
                    teamCount = teamCount,
                    teamMembersCount = teamMembersCount,
                    databaseConnected = true
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { 
                    message = "Teams table test failed", 
                    error = ex.Message,
                    databaseConnected = false
                });
            }
        }

        // Test endpoint to create a simple team
        [HttpPost("test-create-team")]
        public async Task<IActionResult> TestCreateTeam()
        {
            try
            {
                var testTeam = new Team
                {
                    Name = "Test Team " + DateTime.Now.Ticks,
                    Description = "Test team created at " + DateTime.Now,
                    Created_At = DateTime.UtcNow,
                    Is_Active = true
                };

                _context.Teams.Add(testTeam);
                await _context.SaveChangesAsync();

                return Ok(new { 
                    message = "Test team created successfully",
                    teamKey = testTeam.TeamKey,
                    teamName = testTeam.Name
                });
            }
            catch (Exception ex)
            {
                var innerException = ex.InnerException?.Message ?? ex.Message;
                return BadRequest(new { 
                    message = "Test team creation failed", 
                    error = ex.Message,
                    innerError = innerException,
                    fullStackTrace = ex.ToString()
                });
            }
        }

        // Get all teams
        [HttpGet("teams")]
        public async Task<IActionResult> GetTeams()
        {
            try
            {
                // Get teams with members
                var teams = await _context.Teams
                    .Include(t => t.TeamMembers)
                        .ThenInclude(tm => tm.User)
                            .ThenInclude(u => u.Role)
                    .Where(t => t.Is_Active)
                    .Select(t => new
                    {
                        teamKey = t.TeamKey,
                        teamName = t.Name,  // Use camelCase for frontend
                        description = t.Description,
                        memberCount = t.TeamMembers.Count(tm => tm.Is_Active),
                        members = t.TeamMembers
                            .Where(tm => tm.Is_Active)
                            .Select(tm => new
                            {
                                userKey = tm.User.UserKey,
                                email = tm.User.Email,
                                firstName = tm.User.First_Name,
                                lastName = tm.User.Last_Name,
                                role = tm.User.Role.Role_Name
                            })
                    })
                    .ToListAsync();

                return Ok(teams);
            }
            catch (Exception ex)
            {
                return BadRequest(new { 
                    message = "Error loading teams", 
                    error = ex.Message,
                    stackTrace = ex.StackTrace 
                });
            }
        }

        // Create new team
        [HttpPost("teams")]
        public async Task<IActionResult> CreateTeam([FromBody] CreateTeamRequest request)
        {
            try
        {
            if (string.IsNullOrWhiteSpace(request.TeamName))
                return BadRequest("Team name is required");

            var team = new Team
            {
                    Name = request.TeamName,  // Use Name instead of TeamName
                    Description = request.Description,
                    Created_At = DateTime.UtcNow,
                    Is_Active = true
            };

            _context.Teams.Add(team);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetTeams), new { id = team.TeamKey }, new
            {
                team.TeamKey,
                    TeamName = team.Name,  // Use Name instead of TeamName
                team.Description
            });
            }
            catch (Exception ex)
            {
                // Get the inner exception for more details
                var innerException = ex.InnerException?.Message ?? ex.Message;
                var fullError = ex.ToString();
                
                return BadRequest(new { 
                    message = "Error creating team", 
                    error = ex.Message,
                    innerError = innerException,
                    fullStackTrace = fullError
                });
            }
        }

        // Assign user to team
        [HttpPost("assign-user")]
        public async Task<IActionResult> AssignUserToTeam([FromBody] AssignUserRequest request)
        {
            var user = await _context.Users.FindAsync(request.UserKey);
            if (user == null)
                return NotFound("User not found");

            var team = await _context.Teams.FindAsync(request.TeamKey);
            if (team == null)
                return NotFound("Team not found");

            // Check if user is already assigned to this team
            var existingMember = await _context.TeamMembers
                .FirstOrDefaultAsync(tm => tm.TeamKey == request.TeamKey && tm.UserKey == request.UserKey);

            if (existingMember != null)
            {
                if (existingMember.Is_Active)
                    return BadRequest("User is already assigned to this team");
                else
                {
                    // Reactivate existing membership
                    existingMember.Is_Active = true;
                    existingMember.Joined_At = DateTime.UtcNow;
                }
            }
            else
            {
                // Create new team membership
                var teamMember = new TeamMember
                {
                    TeamKey = request.TeamKey,
                    UserKey = request.UserKey,
                    Joined_At = DateTime.UtcNow,
                    Is_Active = true
                };
                _context.TeamMembers.Add(teamMember);
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "User assigned to team successfully",
                user.UserKey,
                user.Email,
                team.TeamName
            });
        }

        // Remove user from team
        [HttpPost("remove-user")]
        public async Task<IActionResult> RemoveUserFromTeam([FromBody] RemoveUserRequest request)
        {
            var teamMember = await _context.TeamMembers
                .FirstOrDefaultAsync(tm => tm.UserKey == request.UserKey && tm.Is_Active);

            if (teamMember == null)
                return NotFound("User is not assigned to any team");

            teamMember.Is_Active = false;
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "User removed from team successfully",
                teamMember.UserKey
            });
        }

        // Get users not assigned to any team
        [HttpGet("unassigned-users")]
        public async Task<IActionResult> GetUnassignedUsers()
        {
            var assignedUserIds = await _context.TeamMembers
                .Where(tm => tm.Is_Active)
                .Select(tm => tm.UserKey)
                .ToListAsync();

            var users = await _context.Users
                .Where(u => !assignedUserIds.Contains(u.UserKey) && u.Role.Role_Name != "Admin")
                .Include(u => u.Role)
                .Select(u => new
                {
                    userKey = u.UserKey,
                    email = u.Email,
                    firstName = u.First_Name,
                    lastName = u.Last_Name,
                    role = u.Role.Role_Name
                })
                .ToListAsync();

            return Ok(users);
        }

        // Get mentor's assigned teams
        [HttpGet("mentor-teams/{mentorId}")]
        public async Task<IActionResult> GetMentorTeams(int mentorId)
        {
            var mentor = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.UserKey == mentorId);

            if (mentor == null)
                return NotFound("Mentor not found");

            if (mentor.Role.Role_Name != "Mentor")
                return BadRequest("User is not a mentor");

            // Get teams where this mentor is assigned
            var teams = await _context.Teams
                .Include(t => t.TeamMembers)
                    .ThenInclude(tm => tm.User)
                        .ThenInclude(u => u.Role)
                .Where(t => t.TeamMembers.Any(tm => tm.UserKey == mentorId && tm.Is_Active))
                .Select(t => new
                {
                    t.TeamKey,
                    TeamName = t.Name,  // Use t.Name instead of t.TeamName
                    t.Description,
                    MemberCount = t.TeamMembers.Count(tm => tm.Is_Active && tm.User.Role.Role_Name == "Intern")
                })
                .ToListAsync();

            return Ok(teams);
        }
    }

    public class CreateTeamRequest
    {
        public string TeamName { get; set; } = string.Empty;
        public string? Description { get; set; }
    }

    public class AssignUserRequest
    {
        public int UserKey { get; set; }
        public int TeamKey { get; set; }
    }

    public class RemoveUserRequest
    {
        public int UserKey { get; set; }
    }
}