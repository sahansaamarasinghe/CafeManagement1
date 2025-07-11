using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace WebApplication2.Services
{
   
    public class EmailSender : IEmailSender
    {
        private readonly SmtpSettings? _smtp;
        private readonly ILogger<EmailSender> _log;

        public EmailSender(IConfiguration cfg, ILogger<EmailSender> log)
        {
            _smtp = cfg.GetSection("Smtp").Get<SmtpSettings>();
            _log = log;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            
            if (_smtp is null || string.IsNullOrWhiteSpace(_smtp.Host))
            {
                _log.LogInformation("DEV-MAIL → {Email} | {Subject}\n{Html}",
                                    email, subject, htmlMessage);
                return;
            }

            
            var msg = new MailMessage
            {
                From = new MailAddress(_smtp.From ?? _smtp.User),
                Subject = subject,
                Body = htmlMessage,
                IsBodyHtml = true
            };
            msg.To.Add(email);

            using var client = new SmtpClient(_smtp.Host, _smtp.Port)
            {
                EnableSsl = true,
                Credentials = new NetworkCredential(_smtp.User, _smtp.Pass)
            };

            await client.SendMailAsync(msg);
        }

        private sealed class SmtpSettings
        {
            public string? Host { get; init; }
            public int Port { get; init; } = 587;
            public string User { get; init; } = "";
            public string Pass { get; init; } = "";
            public string? From { get; init; }   
        }
    }
}
