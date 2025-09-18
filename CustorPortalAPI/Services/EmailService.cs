using System.Net;
using System.Net.Mail;

namespace CustorPortalAPI.Services
{
    public interface IEmailService
    {
        Task SendPasswordResetEmailAsync(string email, string resetLink);
    }

    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendPasswordResetEmailAsync(string email, string resetLink)
        {
            var smtpHost = _configuration["Email:SmtpHost"] ?? "smtp.gmail.com";
            var smtpPort = int.Parse(_configuration["Email:SmtpPort"] ?? "587");
            var smtpUsername = _configuration["Email:Username"];
            var smtpPassword = _configuration["Email:Password"];
            var fromEmail = _configuration["Email:FromEmail"] ?? smtpUsername;

            if (string.IsNullOrEmpty(smtpUsername) || string.IsNullOrEmpty(smtpPassword))
            {
                // For demo purposes, just log the email instead of sending
                Console.WriteLine($"DEMO: Password reset email for {email}");
                Console.WriteLine($"DEMO: Reset link: {resetLink}");
                return;
            }

            using var client = new SmtpClient(smtpHost, smtpPort);
            client.EnableSsl = true;
            client.Credentials = new NetworkCredential(smtpUsername, smtpPassword);

            var message = new MailMessage
            {
                From = new MailAddress(fromEmail, "Custor Portal"),
                Subject = "Password Reset - Custor Portal",
                Body = $@"
                    <html>
                    <body>
                        <h2>Password Reset Request</h2>
                        <p>You requested a password reset for your Custor Portal account.</p>
                        <p>Click the link below to reset your password:</p>
                        <p><a href='{resetLink}' style='background-color: #007bff; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px;'>Reset Password</a></p>
                        <p>Or copy and paste this link in your browser:</p>
                        <p>{resetLink}</p>
                        <p><strong>This link will expire in 30 minutes.</strong></p>
                        <p>If you didn't request this reset, please ignore this email.</p>
                        <br>
                        <p>Best regards,<br>Custor Portal Team</p>
                    </body>
                    </html>",
                IsBodyHtml = true
            };

            message.To.Add(email);

            await client.SendMailAsync(message);
        }
    }
}
