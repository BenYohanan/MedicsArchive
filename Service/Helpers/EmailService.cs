using Hangfire;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;
using MimeKit.Text;

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
        private readonly int port;
        private readonly string? password;
        private readonly string? companyEmail = string.Empty;
        public EmailService(IConfiguration configuration)
        {
            _emailConfiguration = configuration;
            address = _emailConfiguration["EmailConfiguration:SmtpUsername"];
            server = _emailConfiguration["EmailConfiguration:SmtpServer"];
            port = int.Parse(_emailConfiguration["EmailConfiguration:SmtpPort"] ?? "");
            password = _emailConfiguration["EmailConfiguration:SmtpPassword"];
            companyEmail = _emailConfiguration["EmailConfiguration:CompanyEmail"];
        }
        
        public void SendEmail(string toEmail, string subject, string message)
        {
           
            var fromAddress = new EmailAddress
            {
                Name = "Medimaging Databank",
                Address = address
            };

            List<EmailAddress> fromAddressList =
            [
                        fromAddress
            ];
            EmailAddress toAddress = new()
            {
                Address = toEmail
            };
            List<EmailAddress> toAddressList =
            [
                    toAddress
            ];

            EmailMessage emailMessage = new()
            {
                FromAddresses = fromAddressList,
                ToAddresses = toAddressList,
                Subject = subject,
                Content = message,
                CompanyEmail = companyEmail,
                CompanyName = "Medimaging Databank"
            };

            CallHangfire(emailMessage);
        }

        public void CallHangfire(string toEmail, string subject, string message)
        {
            BackgroundJob.Enqueue(() => SendEmail(toEmail, subject, message));
        }

        public void CallHangfire(EmailMessage emailMessage)
        {
            BackgroundJob.Enqueue(() => Send(emailMessage));
        }

        public void Send(EmailMessage emailMessage)
        {
            var message = new MimeMessage();
            message.To.AddRange(emailMessage.ToAddresses.Select(x => new MailboxAddress(x.Name, x.Address)));
            message.From.AddRange(emailMessage.FromAddresses.Select(x => new MailboxAddress(x.Name, x.Address)));

            if (!string.IsNullOrEmpty(emailMessage.CompanyName) && !string.IsNullOrEmpty(emailMessage.CompanyEmail))
            {
                message.ReplyTo.Add(new MailboxAddress(emailMessage.CompanyName, emailMessage.CompanyEmail));
            }
            
            message.Subject = emailMessage.Subject;

            message.Body = new TextPart(TextFormat.Html)
            {
                Text = emailMessage.Content
            };
            if (message.To.Any(f => f.Name == null))
            {
                using (var emailClient = new MailKit.Net.Smtp.SmtpClient())
                {
                    emailClient.ServerCertificateValidationCallback = (s, c, h, e) => true;
                    emailClient.Connect(server, port, SecureSocketOptions.Auto); 
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
        public string? Name { get; set; }

        public string? Address { get; set; }
    }
    public class EmailMessage
    {
        public EmailMessage()
        {
            ToAddresses = [];
            FromAddresses = [];
        }

        public List<EmailAddress> ToAddresses { get; set; }

        public List<EmailAddress> FromAddresses { get; set; }

        public string? Subject { get; set; }

        public string? Content { get; set; }
        public string? CompanyName { get; set; }
        public string? CompanyEmail { get; set; }
    }
}
