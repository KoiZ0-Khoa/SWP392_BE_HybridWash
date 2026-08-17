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
        private readonly string _server;
        private readonly int _port;
        private readonly string _senderName;
        private readonly string _senderEmail;
        private readonly string _username;
        private readonly string _password;

        public EmailService(IConfiguration configuration)
        {
            var settings = configuration.GetSection("SmtpSettings");
            _server = GetRequiredSetting(settings, "Server");
            _senderName = GetRequiredSetting(settings, "SenderName");
            _senderEmail = GetRequiredSetting(settings, "SenderEmail");
            _username = GetRequiredSetting(settings, "Username");
            _password = GetRequiredSetting(settings, "Password");
            if (!int.TryParse(settings["Port"], out _port))
                throw new InvalidOperationException("SmtpSettings:Port is not configured correctly.");
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var emailMessage = new MimeMessage();

            emailMessage.From.Add(new MailboxAddress(_senderName, _senderEmail));
            emailMessage.To.Add(new MailboxAddress("", toEmail));
            emailMessage.Subject = subject;

            var bodyBuilder = new BodyBuilder { HtmlBody = body };
            emailMessage.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();
            try
            {
                await client.ConnectAsync(_server, _port, SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(_username, _password);
                await client.SendAsync(emailMessage);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Không thể gửi email.", ex);
            }
            finally
            {
                if (client.IsConnected)
                    await client.DisconnectAsync(true);
            }
        }

        private static string GetRequiredSetting(IConfigurationSection settings, string key)
        {
            var value = settings[key];
            return string.IsNullOrWhiteSpace(value)
                ? throw new InvalidOperationException($"SmtpSettings:{key} is not configured.")
                : value;
        }
    }
}
