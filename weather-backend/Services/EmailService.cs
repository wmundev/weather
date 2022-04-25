using System;
using System.ComponentModel;
using System.Net;
using System.Net.Mail;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace weather_backend.Services
{
    public class EmailService
    {
        private readonly IConfiguration _configuration;
        private readonly SmtpClient _smtpClient;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;

            var emailUsername = configuration.GetValue<string>("SMTPUsername");
            var emailPassword = configuration.GetValue<string>("SMTPPassword");
            var emailHost = configuration.GetValue<string>("SMTPHost");
            var emailPort = configuration.GetValue<int>("SMTPPort");


            _smtpClient = new SmtpClient
            {
                Host = emailHost,
                Port = emailPort,
                Credentials = new NetworkCredential(emailUsername, emailPassword),
                EnableSsl = true
            };
            _smtpClient.SendCompleted += SendCompletedCallback;
        }

        private void SendCompletedCallback(object sender, AsyncCompletedEventArgs e)
        {
            // Get the unique identifier for this asynchronous operation.
            var token = (string) e.UserState;

            if (e.Cancelled) Console.WriteLine("[{0}] Send canceled.", token);

            if (e.Error != null)
                Console.WriteLine("[{0}] {1}", token, e.Error);
            else
                Console.WriteLine("Message sent.");
        }

        public void SendEmail(string subject, string body, string receiver)
        {
            var senderEmailAddress = new MailAddress(_configuration.GetValue<string>("SMTPUsername"));
            var toEmailAddress = new MailAddress(receiver);

            var mailMessage = new MailMessage(senderEmailAddress, toEmailAddress);
            mailMessage.Subject = subject;
            mailMessage.Body = body;

            mailMessage.BodyEncoding = Encoding.UTF8;
            mailMessage.SubjectEncoding = Encoding.UTF8;

            // The userState can be any object that allows your callback
            // method to identify this send operation.
            // For this example, the userToken is a string constant.
            var userState = "test message1";
            _smtpClient.SendAsync(mailMessage, userState);

            //clean up
            // mailMessage.Dispose();
        }
    }
}