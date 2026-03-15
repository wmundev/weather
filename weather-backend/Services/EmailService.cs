using System;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using weather_backend.Models;
using weather_backend.Services.Interfaces;

namespace weather_backend.Services
{
    public sealed class EmailService : IDisposable
    {
        private readonly IConfiguration _configuration;
        private readonly ISecretService _secretService;
        private readonly ILogger<EmailService> _logger;

        private SmtpClient? _smtpClient;
        private readonly SemaphoreSlim _smtpInitLock = new(1, 1);

        public EmailService(IConfiguration configuration, ISecretService secretService, ILogger<EmailService> logger)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _secretService = secretService ?? throw new ArgumentNullException(nameof(secretService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        private async Task<SmtpClient> GetSmtpClientAsync()
        {
            if (_smtpClient != null) return _smtpClient;

            await _smtpInitLock.WaitAsync();
            try
            {
                if (_smtpClient != null) return _smtpClient;

                var emailUsername = await _secretService.FetchSpecificSecret(nameof(AllSecrets.SMTPUsername));
                var emailPassword = await _secretService.FetchSpecificSecret(nameof(AllSecrets.SMTPPassword));
                var emailHost = _configuration.GetValue<string>("SMTPHost") ?? throw new InvalidOperationException("Email host is not configured");
                var emailPort = _configuration.GetValue<int>("SMTPPort");

                _smtpClient = new SmtpClient {Host = emailHost, Port = emailPort, Credentials = new NetworkCredential(emailUsername, emailPassword), EnableSsl = true};
            }
            finally
            {
                _smtpInitLock.Release();
            }

            return _smtpClient;
        }

        public async Task SendEmail(string subject, string body, string receiver)
        {
            var senderEmailAddress = await _secretService.FetchSpecificSecret(nameof(AllSecrets.SMTPUsername));
            if (senderEmailAddress is null)
            {
                throw new Exception("Sender email in secret is null");
            }

            var smtpClient = await GetSmtpClientAsync();

            var senderEmailAddressMailAddress = new MailAddress(senderEmailAddress);
            var toEmailAddress = new MailAddress(receiver);

            using var mailMessage = new MailMessage(senderEmailAddressMailAddress, toEmailAddress)
            {
                Subject = subject,
                Body = body,
                BodyEncoding = Encoding.UTF8,
                SubjectEncoding = Encoding.UTF8
            };

            await smtpClient.SendMailAsync(mailMessage);
            _logger.LogInformation("Message sent.");
        }

        public void Dispose()
        {
            _smtpClient?.Dispose();
            _smtpInitLock.Dispose();
        }
    }
}
