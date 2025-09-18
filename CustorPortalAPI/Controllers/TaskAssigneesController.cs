using CustorPortalAPI.Data;
using CustorPortalAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CustorPortalAPI.Controllers
{
    /// <summary>
    /// Controller for managing task assignees
    /// </summary>
    [Route("api/tasks/{taskId}/assignees")]
    [ApiController]
    [AllowAnonymous]
    public class TaskAssigneesController : ControllerBase
    {
        private readonly CustorPortalDbContext _context;

        /// <summary>
        /// Initializes a new instance of the TaskAssigneesController
        /// </summary>
        /// <param name="context">The database context</param>
        public TaskAssigneesController(CustorPortalDbContext context)
        {
            _context = context;
        }

       

        /// <summary>
        /// Assigns a user to a task
        /// </summary>
        /// <param name="taskId">The ID of the task</param>
        /// <param name="request">The assignment request containing user information</param>
        /// <returns>Created result with assignment details</returns>
        [HttpPost]
        public async Task<IActionResult> AssignUser(int taskId, [FromBody] TaskAssigneeRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Ensure the task exists
            var taskExists = await _context.Tasks.AnyAsync(t => t.TaskKey == taskId);
            if (!taskExists)
                return NotFound($"Task with ID {taskId} not found.");

            // Ensure the user exists
            var userExists = await _context.Users.AnyAsync(u => u.UserKey == request.UserKey);
            if (!userExists)
                return NotFound($"User with ID {request.UserKey} not found.");

            // Check if the user is already assigned to the task
            var alreadyAssigned = await _context.TaskAssignees
                .AnyAsync(ta => ta.Taskkey == taskId && ta.UserKey == request.UserKey);
            if (alreadyAssigned)
                return Conflict($"User with ID {request.UserKey} is already assigned to Task with ID {taskId}.");

            var taskAssignee = new TaskAssignee
            {
                Taskkey = taskId,
                UserKey = request.UserKey
            };
            _context.TaskAssignees.Add(taskAssignee);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetTasks), new { taskId }, new
            {
                taskAssignee.Taskkey,
                taskAssignee.UserKey
            });
                
        }
        /// <summary>
        /// Gets all tasks with their assignees
        /// </summary>
        /// <returns>List of tasks with assignee information</returns>
        [HttpGet]
        public async Task<IActionResult> GetTasks()
        {
            var tasks = await _context.Tasks
                .Include(t => t.TaskAssignees!)
                    .ThenInclude(ta => ta.User)
                .Select(t => new {
                    t.TaskKey,
                    t.Title,
                    t.Description,
                    t.Status,
                    t.Priority,
                    t.Deadline,
                    // ... other fields ...
                    Assignee = t.TaskAssignees!.Select(ta => ta.User!.Email).FirstOrDefault() // or .ToList() for multiple
                })
                .ToListAsync();

            return Ok(tasks);
        }

        /// <summary>
        /// Unassigns a user from a task
        /// </summary>
        /// <param name="taskId">The ID of the task</param>
        /// <param name="userId">The ID of the user to unassign</param>
        /// <returns>No content result</returns>
        [HttpDelete("{userId}")]
        public async Task<IActionResult> UnassignUserFromTask(int taskId, int userId)
        {
            var taskAssignee = await _context.TaskAssignees
                .FirstOrDefaultAsync(ta => ta.Taskkey == taskId && ta.UserKey == userId);
            if (taskAssignee == null)
                return NotFound($"User {userId} is not assigned to Task {taskId}.");

            _context.TaskAssignees.Remove(taskAssignee);

            // Notify the user of unassignment
            var task = await _context.Tasks
                .Include(t => t.Project)
                .FirstOrDefaultAsync(t => t.TaskKey == taskId);
            
            if (task?.Project != null)
            {
                var notification = new Notification
                {
                    UserId = userId,
                    Title = "Task Unassigned",
                    Message = $"You have been unassigned from task '{task.Title}' in project '{task.Project.Name}'",
                    Type = "task_assigned",
                    CreatedAt = DateTime.UtcNow
                };
                _context.Notifications.Add(notification);
            }

            await _context.SaveChangesAsync();

            return NoContent();
        }
    }

    /// <summary>
    /// Request model for assigning a user to a task
    /// </summary>
    public class TaskAssigneeRequest
    {
        /// <summary>
        /// The key of the user to assign to the task
        /// </summary>
        public int UserKey { get; set; }
    }
}