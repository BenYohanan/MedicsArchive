using Data.Models;
using Microsoft.Extensions.Configuration;

namespace Service.Helpers
{
    public interface IEmailTemplateService
    {
        bool SendRegistrationEmail(ApplicationUser user, string baseUrl);
        bool SendReportApprovalEmail(ApplicationUser user, string exam);
        bool SendReportRejectionEmail(ApplicationUser user, string exam);
    }

    public class EmailTemplateService : IEmailTemplateService
    {
        private readonly IEmailService _emailService;
        private readonly IConfiguration _emailConfiguration;
        private readonly string supportEmail = string.Empty;
        public EmailTemplateService(IEmailService emailService, IConfiguration configuration)
        {
            _emailConfiguration = configuration;
            _emailService = emailService;
            supportEmail = _emailConfiguration["EmailConfiguration:CompanyEmail"] ?? "okoronkwomarvelous@hotmail.com";
        }

        public bool SendReportApprovalEmail(ApplicationUser user, string exam)
        {
            if (user == null || string.IsNullOrWhiteSpace(user.Email))
            {
                return false;
            }

            string subject = "Your Report Has Been Approved";
            string message = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; background-color: #f4f4f4;'>
                    <div style='background-color: #ffffff; padding: 20px; border: 1px solid #e0e0e0; text-align: center;'>
                        <h1 style='color: #004aad; font-size: 24px;'>Report Approved!</h1>
                        <p style='color: #333333; font-size: 16px; line-height: 1.5;'>
                            Dear {user.FullName},<br/>
                            Your report with exam name {exam} has been approved.<br/>
                        </p>
                       
                        <p style='color: #333333; font-size: 16px;'>
                            Need help? Contact us at <a href='mailto:{supportEmail}' style='color: #004aad;'>{supportEmail}</a>.
                        </p>
                        <p><b>Kind regards,</b><br/>Medimaging Databank Team</p>
                    </div>
                </div>";

            try
            {
                _emailService.CallHangfire(user.Email, subject, message);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool SendReportRejectionEmail(ApplicationUser user, string exam)
        {
            if (user == null || string.IsNullOrWhiteSpace(user.Email))
            {
                return false;
            }

            string subject = "Your Report Has Been Rejected";
            string message = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; background-color: #f4f4f4;'>
                    <div style='background-color: #ffffff; padding: 20px; border: 1px solid #e0e0e0; text-align: center;'>
                        <h1 style='color: #d32f2f; font-size: 24px;'>Report Rejected</h1>
                        <p style='color: #333333; font-size: 16px; line-height: 1.5;'>
                            Dear {user.FullName},<br/>
                            Unfortunately, your report with exam name {exam} was rejected.
                        </p>
                        <p style='color: #333333; font-size: 16px;'>
                            Need help? Contact us at <a href='mailto:{supportEmail}' style='color: #d32f2f;'>{supportEmail}</a>.
                        </p>
                        <p><b>Kind regards,</b><br/>Medimaging Databank Team</p>
                    </div>
                </div>";

            try
            {
                _emailService.CallHangfire(user.Email, subject, message);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool SendRegistrationEmail(ApplicationUser user, string baseUrl)
        {
            if (user == null || string.IsNullOrWhiteSpace(user.Email))
            {
                return false;
            }
            string loginLink = $"{baseUrl}/Account/Login";
            string subject = "Welcome to Medimaging Databank!";
            string message = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; background-color: #f4f4f4;'>
                    <div style='background-color: #ffffff; padding: 20px; border: 1px solid #e0e0e0; text-align: center;'>
                        <h1 style='color: #004aad; font-size: 24px;'>Welcome!</h1>
                        <p style='color: #333333; font-size: 16px;'>
                            Dear {user.FullName},<br/>
                            Your account has been successfully created,<br/>
                            and you have access to Medimaging Databank platform for {user.PassWordType}.<br/>
                            Your login details is<br/>
                            Email: {user.Email}<br/>
                            Password: 11111<br/>
                            Please click the button below to log in and access your dashboard.
                        </p>
                        <p style='color: #333333;'>
                            Login Link: <a href='{loginLink}' style='color: #004aad;'>Login</a>.
                        </p>
                        <p style='color: #333333;'>
                            Need help? Contact us at <a href='mailto:{supportEmail}' style='color: #004aad;'>{supportEmail}</a>.
                        </p>
                        <p><b>Kind regards,</b><br/>Medimaging Databank Team</p>
                    </div>
                </div>";

            try
            {
                _emailService.CallHangfire(user.Email, subject, message);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
