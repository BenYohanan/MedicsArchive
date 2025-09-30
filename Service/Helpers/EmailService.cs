using Hangfire;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;
using MimeKit.Text;
using System.ComponentModel.DataAnnotations;

namespace Service.Helpers
{
	public interface IEmailService
    {
        void CallHangfire(string toEmail, string subject, string message);
        void SendEmail(string toEmail, string subject, string message);
    }

    public class EmailService : IEmailService
    {
        private readonly IConfiguration _emailConfiguration;
        private readonly string? address;
        private readonly string? server;
        private readonly int? port;
        private readonly string? password;
        public EmailService(IConfiguration configuration)
        {
            _emailConfiguration = configuration;
            address = _emailConfiguration["EmailConfiguration:SmtpUsername"];
            server = _emailConfiguration["EmailConfiguration:SmtpServer"];
            port = int.Parse(_emailConfiguration["EmailConfiguration:SmtpPort"]);
            password = _emailConfiguration["EmailConfiguration:SmtpPassword"];
        }
        
        public void SendEmail(string toEmail, string subject, string message)
        {
           
            var fromAddress = new EmailAddress
            {
                Name = "Medics Archive",
                Address = address
            };

            List<EmailAddress> fromAddressList = new List<EmailAddress>
            {
                        fromAddress
            };
            EmailAddress toAddress = new EmailAddress()
            {
                Address = toEmail
            };
            List<EmailAddress> toAddressList = new List<EmailAddress>
            {
                    toAddress
            };

            EmailMessage emailMessage = new EmailMessage()
            {
                FromAddresses = fromAddressList,
                ToAddresses = toAddressList,
                Subject = subject,
                Content = message,
                CompanyEmail = "okoronkwomarvelous@hotmail.com",
                CompanyName =  "Medics Archive"
            };

            CallHangfire(emailMessage);
        }

        public void CallHangfire(string toEmail, string subject, string message)
        {
            BackgroundJob.Enqueue(() => SendEmail(toEmail, subject, message));
        }

        private void CallHangfire(EmailMessage emailMessage)
        {
            BackgroundJob.Enqueue(() => Send(emailMessage));
        }

        private void Send(EmailMessage emailMessage)
        {
            var message = new MimeMessage();
            message.To.AddRange(emailMessage.ToAddresses.Select(x => new MailboxAddress(x.Name, x.Address)));
            message.From.AddRange(emailMessage.FromAddresses.Select(x => new MailboxAddress(x.Name, x.Address)));

            if (!string.IsNullOrEmpty(emailMessage.CompanyName) && !string.IsNullOrEmpty(emailMessage.CompanyEmail))
            {
                message.Cc.Add(new MailboxAddress(emailMessage.CompanyName, emailMessage.CompanyEmail));
                message.ReplyTo.Add(new MailboxAddress(emailMessage.CompanyName, emailMessage.CompanyEmail));
            }
            
            message.Subject = emailMessage.Subject;

            message.Body = new TextPart(TextFormat.Html)
            {
                Text = emailMessage.Content
            };
            if (message.To.Any(f => f.Name == null))
            {
                //Be careful that the SmtpClient class is the one from Mailkit not the framework!
                using (var emailClient = new MailKit.Net.Smtp.SmtpClient())
                {
                    emailClient.ServerCertificateValidationCallback = (s, c, h, e) => true;
                    emailClient.Connect(server, (int)port, SecureSocketOptions.Auto); 
                    emailClient.AuthenticationMechanisms.Remove("XOAUTH2");
                    emailClient.Authenticate(address,password);
                    emailClient.Send(message);
                    emailClient.Disconnect(true);
                }
            }
        }
    }
    public class EmailAddress
    {
        public string Name { get; set; }

        public string Address { get; set; }
    }
    public class EmailMessage
    {
        public EmailMessage()
        {
            ToAddresses = new List<EmailAddress>();
            FromAddresses = new List<EmailAddress>();
        }

        [EmailAddress(ErrorMessage = "Invalid email address.")]
        public List<EmailAddress> ToAddresses { get; set; }

        public List<EmailAddress> FromAddresses { get; set; }

        public string Subject { get; set; }

        public string Content { get; set; }
        public string CompanyName { get; set; }
        public string CompanyEmail { get; set; }
    }
}
