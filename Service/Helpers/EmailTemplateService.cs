using Data.Models;

namespace Service.Helpers
{
    public interface IEmailTemplateService
    {
        bool SendRegistrationEmail(ApplicationUser user);
        bool SendReportApprovalEmail(ApplicationUser user);
        bool SendReportRejectionEmail(ApplicationUser user);
    }

    public class EmailTemplateService : IEmailTemplateService
    {
        private readonly IEmailService _emailService;
        private readonly string supportEmail = "";
        public EmailTemplateService(IEmailService emailService)
        {
            _emailService = emailService;
        }

        public bool SendReportApprovalEmail(ApplicationUser user)
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
                            Your report titled has been approved.<br/>
                            You can log in to your account to view the details.
                        </p>
                       
                        <p style='color: #333333; font-size: 16px;'>
                            Need help? Contact us at <a href='mailto:{supportEmail}' style='color: #004aad;'>{supportEmail}</a>.
                        </p>
                        <p><b>Kind regards,</b><br/>Medics Archive Team</p>
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

        public bool SendReportRejectionEmail(ApplicationUser user)
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
                            Unfortunately, your report titled was rejected.
                        </p>
                        <p style='color: #333333; font-size: 16px;'>
                            Need help? Contact us at <a href='mailto:{supportEmail}' style='color: #d32f2f;'>{supportEmail}</a>.
                        </p>
                        <p><b>Kind regards,</b><br/>Medics Archive Team</p>
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

        public bool SendRegistrationEmail(ApplicationUser user)
        {
            if (user == null || string.IsNullOrWhiteSpace(user.Email))
            {
                return false;
            }

            string subject = "Welcome to Medics Archive!";
            string message = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; background-color: #f4f4f4;'>
                    <div style='background-color: #ffffff; padding: 20px; border: 1px solid #e0e0e0; text-align: center;'>
                        <h1 style='color: #004aad; font-size: 24px;'>Welcome!</h1>
                        <p style='color: #333333; font-size: 16px;'>
                            Dear {user.FullName},<br/>
                            Your account has been successfully created.<br/>
                            And you have access to the Medics Archive platform for {user.PassWordType.ToString()}.<br/>
                            Please click the button below to log in and access your dashboard.
                        </p>

                        <p style='color: #333333;'>
                            Need help? Contact us at <a href='mailto:{supportEmail}' style='color: #004aad;'>{supportEmail}</a>.
                        </p>
                        <p><b>Kind regards,</b><br/>Medics Archive Team</p>
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
