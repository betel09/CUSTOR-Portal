using CustorPortalAPI.Data;
using CustorPortalAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace CustorPortalAPI.Controllers
{
    [Route("api/tasks/{taskId:int}/comments")]
    [ApiController]
    [AllowAnonymous]
    public class CommentsController : ControllerBase
    {
        private readonly CustorPortalDbContext _context;

        public CommentsController(CustorPortalDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> CreateComment(int taskId, [FromBody] CommentRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Text))
                return BadRequest("Comment cannot be empty");

            // Validate TaskId
            var task = await _context.Tasks.FindAsync(taskId);
            if (task == null)
                return NotFound($"Task with ID {taskId} not found.");

            // Validate UserId
            var user = await _context.Users.FindAsync(request.UserId);
            if (user == null)
                return NotFound($"User with ID {request.UserId} not found.");

            var comment = new Comment
            {
                TaskId = taskId,
                Text = request.Text, // Map to Text
                UserId = request.UserId,
                Timestamp = DateTime.UtcNow, // Map to Timestamp
                Mentions = request.Mentions != null && request.Mentions.Any()
                    ? JsonSerializer.Serialize(request.Mentions)
                    : null
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            // Handle mentions and notifications
            if (request.Mentions != null && request.Mentions.Any())
            {
                foreach (var mention in request.Mentions)
                {
                    var mentionedUser = await _context.Users
                        .FirstOrDefaultAsync(u => (u.First_Name + " " + u.Last_Name).ToLower() == mention.ToLower());
                    if (mentionedUser != null)
                    {
                        _context.Notifications.Add(new Notification
                        {
                            UserId = mentionedUser.UserKey,
                            Title = "Mentioned in Task Comment",
                            Message = $"{user.First_Name} {user.Last_Name} mentioned you in a comment on task {task.Title}",
                            Type = "comment",
                            CreatedAt = DateTime.UtcNow
                        });
                    }
                }
                await _context.SaveChangesAsync();
            }

            return CreatedAtAction(nameof(GetComments), new { taskId = taskId }, new
            {
                id = comment.Id,
                text = comment.Text,
                userId = comment.UserId,
                timestamp = comment.Timestamp,
                mentions = comment.Mentions
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetComments(int taskId)
        {
            var task = await _context.Tasks.FindAsync(taskId);
            if (task == null)
                return NotFound($"Task with ID {taskId} not found.");

            var comments = await _context.Comments
                .Where(c => c.TaskId == taskId)
                .Include(c => c.User)
                .ThenInclude(u => u.Role)
                .Select(c => new
                {
                    c.Id,
                    Text = c.Text,
                    c.UserId,
                    Timestamp = c.Timestamp,
                    c.Mentions,
                    User = new
                    {
                        c.User.UserKey,
                        c.User.Email,
                        c.User.First_Name,
                        c.User.Last_Name,
                        Role = new
                        {
                            c.User.Role.RoleKey,
                            c.User.Role.Role_Name
                        }
                    }
                })
                .ToListAsync();

            return Ok(comments);
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteComment(int taskId, int id)
        {
            var comment = await _context.Comments
                .FirstOrDefaultAsync(c => c.Id == id && c.TaskId == taskId);
            if (comment == null)
                return NotFound($"Comment with ID {id} for Task {taskId} not found.");

            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();

            return NoContent();
        }

    }

    public class CommentRequest
    {
        public string Text { get; set; } = string.Empty;
        public List<string> Mentions { get; set; } = new List<string>();
        public int UserId { get; set; }
    }
}