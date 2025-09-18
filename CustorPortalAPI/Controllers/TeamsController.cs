using CustorPortalAPI.Data;
using CustorPortalAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace CustorPortalAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TeamsController : ControllerBase
    {
        private readonly CustorPortalDbContext _context;

        public TeamsController(CustorPortalDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetTeams()
        {
            var teams = await _context.Teams
                .Include(t => t.TeamMembers)
                    .ThenInclude(tm => tm.User)
                .Where(t => t.Is_Active)
                .Select(t => new
                {
                    t.TeamKey,
                    t.Name,
                    t.Description,
                    t.Created_At,
                    t.Updated_At,
                    MemberCount = t.TeamMembers.Count(tm => tm.Is_Active),
                    Members = t.TeamMembers
                        .Where(tm => tm.Is_Active)
                        .Select(tm => new
                        {
                            tm.UserKey,
                            tm.User.Email,
                            tm.User.First_Name,
                            tm.User.Last_Name,
                            tm.Joined_At
                        })
                })
                .ToListAsync();

            return Ok(teams);
        }

        [HttpGet("{teamId}")]
        public async Task<IActionResult> GetTeam(int teamId)
        {
            var team = await _context.Teams
                .Include(t => t.TeamMembers)
                    .ThenInclude(tm => tm.User)
                .FirstOrDefaultAsync(t => t.TeamKey == teamId && t.Is_Active);

            if (team == null)
                return NotFound($"Team with ID {teamId} not found.");

            var result = new
            {
                team.TeamKey,
                team.Name,
                team.Description,
                team.Created_At,
                team.Updated_At,
                Members = team.TeamMembers
                    .Where(tm => tm.Is_Active)
                    .Select(tm => new
                    {
                        tm.UserKey,
                        tm.User.Email,
                        tm.User.First_Name,
                        tm.User.Last_Name,
                        tm.Joined_At
                    })
            };

            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> CreateTeam([FromBody] TeamCreateRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var team = new Team
            {
                Name = request.Name,
                Description = request.Description,
                Created_At = DateTime.UtcNow,
                Is_Active = true
            };

            _context.Teams.Add(team);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetTeam), new { teamId = team.TeamKey }, new
            {
                team.TeamKey,
                team.Name,
                team.Description,
                team.Created_At
            });
        }

        [HttpPost("{teamId}/members")]
        public async Task<IActionResult> AddTeamMember(int teamId, [FromBody] AddTeamMemberRequest request)
        {
            var team = await _context.Teams.FindAsync(teamId);
            if (team == null)
                return NotFound($"Team with ID {teamId} not found.");

            var user = await _context.Users.FindAsync(request.UserKey);
            if (user == null)
                return NotFound($"User with ID {request.UserKey} not found.");

            // Check if user is already a member
            var existingMember = await _context.TeamMembers
                .FirstOrDefaultAsync(tm => tm.TeamKey == teamId && tm.UserKey == request.UserKey);

            if (existingMember != null)
            {
                if (existingMember.Is_Active)
                    return BadRequest("User is already a member of this team.");
                else
                {
                    // Reactivate existing membership
                    existingMember.Is_Active = true;
                    existingMember.Joined_At = DateTime.UtcNow;
                }
            }
            else
            {
                var teamMember = new TeamMember
                {
                    TeamKey = teamId,
                    UserKey = request.UserKey,
                    Joined_At = DateTime.UtcNow,
                    Is_Active = true
                };
                _context.TeamMembers.Add(teamMember);
            }

            await _context.SaveChangesAsync();
            return Ok("Team member added successfully.");
        }

        [HttpDelete("{teamId}/members/{userId}")]
        public async Task<IActionResult> RemoveTeamMember(int teamId, int userId)
        {
            var teamMember = await _context.TeamMembers
                .FirstOrDefaultAsync(tm => tm.TeamKey == teamId && tm.UserKey == userId);

            if (teamMember == null)
                return NotFound("Team member not found.");

            teamMember.Is_Active = false;
            await _context.SaveChangesAsync();

            return Ok("Team member removed successfully.");
        }
    }

    public class TeamCreateRequest
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = null!;
        
        [MaxLength(500)]
        public string? Description { get; set; }
    }

    public class AddTeamMemberRequest
    {
        [Required]
        public int UserKey { get; set; }
    }
}
