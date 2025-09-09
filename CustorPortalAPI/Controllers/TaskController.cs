using System.ComponentModel.DataAnnotations;
using CustorPortalAPI.Data;
using CustorPortalAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Task = CustorPortalAPI.Models.Task;

namespace CustorPortalAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TasksController : ControllerBase
    {
        private readonly CustorPortalDbContext _context;

        public TasksController(CustorPortalDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task <IActionResult> CreateTask([FromBody] TaskCreateRequest request)
        {
            if(!ModelState.IsValid)
            
                return BadRequest(ModelState);
            if (!await _context.Projects.AnyAsync(p => p.ProjectKey == request.ProjectKey))
                return NotFound($"Project with ID {request.ProjectKey} not found.");

            // Validate creator exists
            if (!await _context.Users.AnyAsync(u => u.UserKey == request.CreatorKey))
                return NotFound($"User with ID {request.CreatorKey} not found.");

            var task = new Task
            {
                Title = request.Title,
                Description = request.Description,
                Status = request.Status,
                Priority = request.Priority,
                Deadline = request.Deadline,
                Created_at = DateTime.UtcNow,
                Updated_at = null,
                ProjectKey = request.ProjectKey,
                CreatorKey = request.CreatorKey
            };

            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();
            var taskAssignee = new TaskAssignee
            {
                Taskkey = task.TaskKey,
                UserKey = request.AssigneeKey
            };
            _context.TaskAssignees.Add(taskAssignee);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetTasks), new { taskId = task.TaskKey }, new
            {
                task.TaskKey,
                task.Title,
                task.Description,
                task.Status,
                task.Priority,
                task.Deadline,
                task.Created_at,
                task.Updated_at,
                task.ProjectKey,
                task.CreatorKey
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetTasks()
        {
            var tasks = await _context.Tasks
                .Include(t => t.TaskAssignees)
                    .ThenInclude(ta => ta.User)
                .Select(t => new {
                    t.TaskKey,
                    t.Title,
                    t.Description,
                    t.Status,
                    t.Priority,
                    t.Deadline,
                    t.ProjectKey,
                    t.CreatorKey,
                    // If you only want one assignee per task:
                    Assignee = t.TaskAssignees.Select(ta => ta.User.Email).FirstOrDefault()
                    // Or for multiple assignees: Assignees = t.TaskAssignees.Select(ta => ta.User.Email).ToList()
                })
                .ToListAsync();

            return Ok(tasks);
        }
        [HttpPut("{taskId}")]
        public async Task<IActionResult> UpdateTask(int taskId, [FromBody] TaskUpdateRequest request)
        {
            var task = await _context.Tasks.FindAsync(taskId);
            if (task == null)
                return NotFound($"Task with ID {taskId} not found.");

            if (!string.IsNullOrWhiteSpace(request.Title))
                task.Title = request.Title;
            if (!string.IsNullOrWhiteSpace(request.Description))
                task.Description = request.Description;
            if (!string.IsNullOrWhiteSpace(request.Status))
                task.Status = request.Status;
            if (request.Deadline.HasValue)
                task.Deadline = request.Deadline;
            task.Updated_at = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                task.TaskKey,
                task.ProjectKey,
                task.Title,
                task.Description,
                task.Status,
                task.Deadline,
                task.Created_at,
                task.Updated_at
            });
        }

        // New DELETE endpoint to delete a task
        [HttpDelete("{taskId}")]
        public async Task<IActionResult> DeleteTask(int taskId)
        {
            var task = await _context.Tasks
                .Include(t => t.Comments)
                .Include(t => t.TaskAssignees)
                .FirstOrDefaultAsync(t => t.TaskKey == taskId);
            if (task == null)
                return NotFound($"Task with ID {taskId} not found.");

            // Check for dependencies
            if (task.Comments.Any())
                return BadRequest("Cannot delete task with existing comments.");
            if (task.TaskAssignees.Any())
                return BadRequest("Cannot delete task with assigned users.");

            _context.Tasks.Remove(task);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }

    public class TaskCreateRequest
    {
        [Required]
        public int AssigneeKey { get; set; }
        [Required]
        [MaxLength(100)]
        public string Title { get; set; }
        public string? Description { get; set; }
        [Required]
        [RegularExpression("^(To Do|In Progress|In Review|Done)$")]
        public string Status { get; set; }
        [Required]
        [RegularExpression("^(Low|Medium|High)$")]
        public string Priority { get; set; }
        public DateTime? Deadline { get; set; }
        [Required]
        public int ProjectKey { get; set; } // Reference to existing project
        [Required]
        public int CreatorKey { get; set; }
    }

    public class TaskUpdateRequest
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? Status { get; set; }
        public DateTime? Deadline { get; set; }
    }
}
    
