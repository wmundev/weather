using System;
using System.ComponentModel;
using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;

namespace weather_backend.Services
{
    public class EmailService
    {
        private SmtpClient _smtpClient;
        private IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;

            string emailUsername = configuration.GetValue<string>("SMTPUsername");
            string emailPassword = configuration.GetValue<string>("SMTPPassword");
            string emailHost = configuration.GetValue<string>("SMTPHost");
            int emailPort = configuration.GetValue<int>("SMTPPort");

            
            _smtpClient = new SmtpClient
            {
                Host = emailHost,
                Port = emailPort,
                Credentials = new NetworkCredential(emailUsername, emailPassword),
                EnableSsl = true
            };
            _smtpClient.SendCompleted += new SendCompletedEventHandler(SendCompletedCallback);
        }

        private void SendCompletedCallback(object sender, AsyncCompletedEventArgs e)
        {
            // Get the unique identifier for this asynchronous operation.
            String token = (string) e.UserState;

            if (e.Cancelled)
            {
                Console.WriteLine("[{0}] Send canceled.", token);
            }
            if (e.Error != null)
            {
                Console.WriteLine("[{0}] {1}", token, e.Error.ToString());
            } else
            {
                Console.WriteLine("Message sent.");
            }
        }

        public void SendEmail(string subject, string body, string receiver)
        {
            MailAddress senderEmailAddress = new MailAddress(_configuration.GetValue<string>("SMTPUsername"));
            MailAddress toEmailAddress = new MailAddress(receiver);

            MailMessage mailMessage = new MailMessage(senderEmailAddress, toEmailAddress);
            mailMessage.Subject = subject;
            mailMessage.Body = body;           
            
            mailMessage.BodyEncoding =  System.Text.Encoding.UTF8;
            mailMessage.SubjectEncoding = System.Text.Encoding.UTF8;
            
            // The userState can be any object that allows your callback
            // method to identify this send operation.
            // For this example, the userToken is a string constant.
            string userState = "test message1";
            _smtpClient.SendAsync(mailMessage, userState);
            
            //clean up
            // mailMessage.Dispose();
        }
    }
}