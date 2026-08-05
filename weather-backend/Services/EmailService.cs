using System;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using weather_backend.Models;
using weather_backend.Services.Interfaces;

namespace weather_backend.Services
{
    public sealed class EmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;
        private readonly ISecretService _secretService;

        public EmailService(IConfiguration configuration, ISecretService secretService, ILogger<EmailService> logger)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _secretService = secretService ?? throw new ArgumentNullException(nameof(secretService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Sends an email. The returned task completes only once the message has been handed to the SMTP server.
        /// </summary>
        public async Task SendEmail(string subject, string body, string receiver)
        {
            // Credentials are fetched here rather than in the constructor: FetchSpecificSecret is async,
            // and blocking on it during construction stalls every request that resolves this service.
            var senderEmailAddress = await _secretService.FetchSpecificSecret(nameof(AllSecrets.SMTPUsername))
                                     ?? throw new InvalidOperationException("SMTP username is not configured.");
            var senderPassword = await _secretService.FetchSpecificSecret(nameof(AllSecrets.SMTPPassword))
                                 ?? throw new InvalidOperationException("SMTP password is not configured.");

            var emailHost = _configuration.GetValue<string>("SMTPHost")
                            ?? throw new InvalidOperationException("SMTPHost is not configured.");
            var emailPort = _configuration.GetValue<int>("SMTPPort");

            // One client per send: SmtpClient rejects concurrent sends on a single instance, which a
            // shared instance would hit as soon as two requests overlap.
            using var smtpClient = new SmtpClient
            {
                Host = emailHost,
                Port = emailPort,
                Credentials = new NetworkCredential(senderEmailAddress, senderPassword),
                EnableSsl = true
            };

            using var mailMessage = new MailMessage(new MailAddress(senderEmailAddress), new MailAddress(receiver))
            {
                Subject = subject,
                Body = body,
                BodyEncoding = Encoding.UTF8,
                SubjectEncoding = Encoding.UTF8
            };

            await smtpClient.SendMailAsync(mailMessage);

            _logger.LogInformation("Email with subject {Subject} sent", subject);
        }
    }
}
