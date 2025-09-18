using CustorPortalAPI.Data;
using CustorPortalAPI.Helpers;
using CustorPortalAPI.Models;
using CustorPortalAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LoginRequest = CustorPortalAPI.Models.LoginRequest;

namespace CustorPortalAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly CustorPortalDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;

        public UsersController(CustorPortalDbContext context, IConfiguration configuration, IEmailService emailService)
        {
            _context = context;
            _configuration = configuration;
            _emailService = emailService;
        }
        public record ForgotPasswordRequest(string Email);
        public record ResetPasswordRequest(string Token, string NewPassword);
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
                return BadRequest("Email and Password are required.");

            var user=await _context.Users
                .Include(u=>u.Role)
                .FirstOrDefaultAsync(u=>u.Email.ToLower()==request.Email.ToLower());

            if (user == null)
                return Unauthorized("Invalid email or password.");

            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.Password_Hash))
                return Unauthorized("Invalid email or password.");

            var token = JwtHelper.GenerateJwtToken(user.UserKey, user.Email, user.Role.Role_Name, _configuration);

            return Ok(new
                
               {
                Token= token,
                User = new 
                {
                    user.UserKey,
                    user.Email,
                    Role=user.Role.Role_Name
                }
            });
        }
        [HttpPost("forgot-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email))
                return BadRequest("Email is required.");

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == request.Email.ToLower());
            if (user == null)
            {
                // Do not reveal user existence
                return Ok(new { message = "If the email exists, a reset link has been sent." });
            }

            var resetToken = JwtHelper.GeneratePasswordResetToken(user.Email, _configuration, TimeSpan.FromMinutes(30));
            var frontendBase = _configuration["Frontend:BaseUrl"] ?? "http://localhost:4200";
            var resetLink = $"{frontendBase}/reset-password?token={Uri.EscapeDataString(resetToken)}";

            // Send email
            await _emailService.SendPasswordResetEmailAsync(user.Email, resetLink);

            return Ok(new { message = "If the email exists, a reset link has been sent." });
        }

        [HttpPost("reset-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Token) || string.IsNullOrWhiteSpace(request.NewPassword))
                return BadRequest("Token and new password are required.");

            if (!IsValidPassword(request.NewPassword))
                return BadRequest("Password must meet complexity requirements.");

            if (!JwtHelper.TryValidatePasswordResetToken(request.Token, _configuration, out var email) || string.IsNullOrWhiteSpace(email))
                return BadRequest("Invalid or expired token.");

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
            if (user == null) return BadRequest("Invalid user.");

            user.Password_Hash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Password reset successful." });
        }
        [HttpPost("register")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
                    return BadRequest("Email and Password are required.");

                if (string.IsNullOrWhiteSpace(request.FirstName) || string.IsNullOrWhiteSpace(request.LastName))
                    return BadRequest("First Name and Last Name are required.");

                if (request.RoleKey <= 0)
                    return BadRequest("Valid Role is required.");

            // Password validation
            if (!IsValidPassword(request.Password))
                return BadRequest("Password must be at least 8 characters long and contain at least one uppercase letter, one lowercase letter, one number, and one special character.");

            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == request.Email.ToLower());
            if (existingUser != null)
                return BadRequest("User already exists.");

            var user = new User
            {
                Email = request.Email,
                Password_Hash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                First_Name = request.FirstName,
                Last_Name = request.LastName,
                RoleKey = request.RoleKey,
                Created_At = DateTime.UtcNow,
                Is_Active = true
            };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                return Ok(new { message = "User registered successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Registration failed", error = ex.Message });
            }
        }
        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _context.Users
                .Include(u => u.Role)
                .Select(u => new
                {
                    u.UserKey,
                    u.Email,
                    Role = u.Role.Role_Name
                })
                .ToListAsync();

            return Ok(users); // This should return a plain array
        }

        [HttpGet("roles")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetRoles()
        {
            var roles = await _context.Roles
                .Select(r => new
                {
                    r.RoleKey,
                    r.Role_Name,
                    r.Description
                })
                .ToListAsync();

            return Ok(roles);
        }

        [HttpGet("test-auth")]
        [Authorize]
        public IActionResult TestAuth()
        {
            var userClaims = User.Claims.Select(c => new { c.Type, c.Value }).ToList();
            return Ok(new { 
                message = "Authentication successful", 
                claims = userClaims,
                isAdmin = User.IsInRole("Admin"),
                isMentor = User.IsInRole("Mentor"),
                isIntern = User.IsInRole("Intern")
            });
        }

        [HttpGet("test-admin")]
        [Authorize(Roles = "Admin")]
        public IActionResult TestAdmin()
        {
            return Ok(new { message = "Admin access confirmed!" });
        }

        [HttpPut("{userId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateUserRole(int userId, [FromBody] UpdateUserRoleRequest request)
        {
            try
            {
                var user = await _context.Users
                    .Include(u => u.Role)
                    .FirstOrDefaultAsync(u => u.UserKey == userId);

                if (user == null)
                    return NotFound($"User with ID {userId} not found.");

                // Find the role by name
                var role = await _context.Roles
                    .FirstOrDefaultAsync(r => r.Role_Name == request.Role);

                if (role == null)
                    return BadRequest($"Role '{request.Role}' not found.");

                // Update the user's role
                user.RoleKey = role.RoleKey;
                user.Updated_At = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return Ok(new { 
                    message = "User role updated successfully.",
                    user = new {
                        user.UserKey,
                        user.Email,
                        Role = role.Role_Name
                    }
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Failed to update user role", error = ex.Message });
            }
        }

        
        private bool IsValidPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password) || password.Length < 8)
                return false;

            bool hasUpperCase = password.Any(char.IsUpper);
            bool hasLowerCase = password.Any(char.IsLower);
            bool hasDigit = password.Any(char.IsDigit);
            bool hasSpecialChar = password.Any(c => !char.IsLetterOrDigit(c));

            return hasUpperCase && hasLowerCase && hasDigit && hasSpecialChar;
        }

    }

    public class RegisterRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public int RoleKey { get; set; }
    }

    public class UpdateUserRoleRequest
    {
        public string Role { get; set; } = string.Empty;
    }
}
