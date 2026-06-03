using System;
using System.ComponentModel;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using weather_backend.Exceptions;
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
        private bool _disposed;
        private readonly SemaphoreSlim _initializationLock = new(1, 1);

        public EmailService(IConfiguration configuration, ISecretService secretService, ILogger<EmailService> logger)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _secretService = secretService ?? throw new ArgumentNullException(nameof(secretService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        private async Task<SmtpClient> GetSmtpClientAsync()
        {
            if (_smtpClient != null)
            {
                return _smtpClient;
            }

            await _initializationLock.WaitAsync();
            try
            {
                if (_smtpClient != null)
                {
                    return _smtpClient;
                }

                var emailUsername = await _secretService.FetchSpecificSecret(nameof(AllSecrets.SMTPUsername));
                var emailPassword = await _secretService.FetchSpecificSecret(nameof(AllSecrets.SMTPPassword));
                var emailHost = _configuration.GetValue<string>("SMTPHost") ?? throw new ConfigurationException("SMTP host is not configured");
                var emailPort = _configuration.GetValue<int>("SMTPPort");

                _smtpClient = new SmtpClient { Host = emailHost, Port = emailPort, Credentials = new NetworkCredential(emailUsername, emailPassword), EnableSsl = true };
                _smtpClient.SendCompleted += SendCompletedCallback;

                return _smtpClient;
            }
            finally
            {
                _initializationLock.Release();
            }
        }

        private void SendCompletedCallback(object sender, AsyncCompletedEventArgs e)
        {
            // Get the unique identifier for this asynchronous operation.
            var token = e.UserState as string;

            if (e.Cancelled)
            {
                _logger.LogError("[{0}] Send canceled.", token);
            }

            if (e.Error != null)
            {
                _logger.LogError("[{0}] {1}", token, e.Error);
            }
            else
            {
                _logger.LogInformation("Message sent.");
            }
        }

        public async Task SendEmail(string subject, string body, string receiver)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);

            var smtpClient = await GetSmtpClientAsync();

            var senderEmailAddress = await _secretService.FetchSpecificSecret(nameof(AllSecrets.SMTPUsername));
            if (senderEmailAddress is null)
            {
                throw new SecretNotFoundException(nameof(AllSecrets.SMTPUsername));
            }

            var senderEmailAddressMailAddress = new MailAddress(senderEmailAddress);
            var toEmailAddress = new MailAddress(receiver);

            var mailMessage = new MailMessage(senderEmailAddressMailAddress, toEmailAddress);
            mailMessage.Subject = subject;
            mailMessage.Body = body;

            mailMessage.BodyEncoding = Encoding.UTF8;
            mailMessage.SubjectEncoding = Encoding.UTF8;

            // The userState can be any object that allows your callback
            // method to identify this send operation.
            // For this example, the userToken is a string constant.
            var userState = Guid.NewGuid();
            smtpClient.SendAsync(mailMessage, userState);
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _smtpClient?.Dispose();
                _initializationLock?.Dispose();
                _disposed = true;
            }
        }
    }
}
