using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using SmartRetail.Application.Common.Settings;
using SmartRetail.Application.Interfaces.Services;

namespace SmartRetail.Infrastructure.Services
{
    public class SendViaBrevoSmtpAsync : IEmailSender
    {
        private readonly BrevoSettings _brevoSettings;
        private readonly ILogger<SendViaBrevoSmtpAsync> _logger;

        public SendViaBrevoSmtpAsync(IOptions<BrevoSettings> brevoSettings, ILogger<SendViaBrevoSmtpAsync> logger)
        {
            _brevoSettings = brevoSettings.Value;
            _logger = logger;
        }

        public async Task SendEmailAsync(string to, string subject, string body, IList<IFormFile>? attachments = null)
        {
            try
            {
                var email = new MimeMessage();
                email.Subject = subject;
                email.To.Add(MailboxAddress.Parse(to));

                var senderEmail = !string.IsNullOrWhiteSpace(_brevoSettings.FromEmail)
                    ? _brevoSettings.FromEmail
                    : _brevoSettings.Login;
                var displayName = !string.IsNullOrWhiteSpace(_brevoSettings.DisplayName)
                    ? _brevoSettings.DisplayName
                    : "MUDAR";

                email.From.Add(new MailboxAddress(displayName, senderEmail));

                var builder = new BodyBuilder();

                if (attachments != null)
                {
                    foreach (var file in attachments)
                    {
                        if (file?.Length > 0)
                        {
                            using var ms = new MemoryStream();
                            await file.CopyToAsync(ms);
                            builder.Attachments.Add(file.FileName, ms.ToArray(), ContentType.Parse(file.ContentType));
                        }
                    }
                }

                builder.HtmlBody = body;
                email.Body = builder.ToMessageBody();

                using var smtp = new SmtpClient();
                smtp.Timeout = 15000;

                await smtp.ConnectAsync(_brevoSettings.Host, _brevoSettings.Port, SecureSocketOptions.StartTls);
                await smtp.AuthenticateAsync(_brevoSettings.Login, _brevoSettings.Password);
                await smtp.SendAsync(email);
                await smtp.DisconnectAsync(true);

                _logger.LogInformation("Email sent successfully via Brevo SMTP to {Recipient}", to);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email via Brevo SMTP to {Recipient}", to);
                throw;
            }
        }
    }
}
