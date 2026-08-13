using System.Threading.Tasks;
using HybridWash.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using MimeKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using System;

namespace HybridWash.Services.Implementations
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var emailSettings = _configuration.GetSection("SmtpSettings");
            var emailMessage = new MimeMessage();

            emailMessage.From.Add(new MailboxAddress(emailSettings["SenderName"], emailSettings["SenderEmail"]));
            emailMessage.To.Add(new MailboxAddress("", toEmail));
            emailMessage.Subject = subject;

            var bodyBuilder = new BodyBuilder { HtmlBody = body };
            emailMessage.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();
            try
            {
                await client.ConnectAsync(emailSettings["Server"], int.Parse(emailSettings["Port"]), SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(emailSettings["Username"], emailSettings["Password"]);
                await client.SendAsync(emailMessage);
            }
            catch (Exception ex)
            {
                // Xử lý lỗi hoặc log lỗi
                throw new Exception($"Không thể gửi email: {ex.Message}");
            }
            finally
            {
                await client.DisconnectAsync(true);
            }
        }
    }
}
