using System;
using System.ComponentModel;
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
        private readonly ISecretService _secretService;
        private readonly ILogger<EmailService> _logger;

        private readonly SmtpClient _smtpClient;

        public EmailService(IConfiguration configuration, ISecretService secretService, ILogger<EmailService> logger)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _secretService = secretService ?? throw new ArgumentNullException(nameof(secretService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            var emailUsername = _secretService.FetchSpecificSecret(nameof(AllSecrets.SMTPUsername))?.Result;
            var emailPassword = _secretService.FetchSpecificSecret(nameof(AllSecrets.SMTPPassword))?.Result;
            var emailHost = configuration.GetValue<string>("SMTPHost") ?? throw new NullReferenceException("Email host is null");
            var emailPort = configuration.GetValue<int>("SMTPPort");

            _smtpClient = new SmtpClient {Host = emailHost, Port = emailPort, Credentials = new NetworkCredential(emailUsername, emailPassword), EnableSsl = true};
            _smtpClient.SendCompleted += SendCompletedCallback;
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
            var senderEmailAddress = await _secretService.FetchSpecificSecret(nameof(AllSecrets.SMTPUsername));
            if (senderEmailAddress is null)
            {
                throw new Exception("Sender email in secret is null");
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
            _smtpClient.SendAsync(mailMessage, userState);
        }
    }
}
