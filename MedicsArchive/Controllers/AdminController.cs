using Data.DbContext;
using Data.Models;
using Data.ViewModels;
using MedicsArchive.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Service.Helpers;

namespace MedicsArchive.Controllers
{
    [Authorize]
    public class AdminController : Controller
    {
        public readonly IReportHelper reportHelper;
        public readonly IOpenAIService openAIService;
        public readonly IUserHelper _userHelper;
        private UserManager<ApplicationUser> _userManager;
        public readonly AppDbContext _context;
        public readonly IEmailTemplateService _emailTemplateService;
        public AdminController(IReportHelper reportHelper, IUserHelper userHelper, AppDbContext appDbContext, IOpenAIService openAIService, IEmailTemplateService emailTemplateService, UserManager<ApplicationUser> userManager)
        {
            this.reportHelper = reportHelper;
            _userHelper = userHelper;
            _context = appDbContext;
            this.openAIService = openAIService;
            _emailTemplateService = emailTemplateService;
            _userManager = userManager;
        }
       
        public IActionResult Index()
        {
            var reports = reportHelper.PatientReports(true);
            var data = new AdminDashboardDTO
            {
                UserName = @User.FindFirst("FullName")?.Value,
                AllResultCount = reports.Count(x => x.Status == Status.Approved),
                PendingResultCount = reports.Count(x => x.Status == Status.Pending),
                RejectedResultCount = reports.Count(x => x.Status == Status.Rejected),
                ClientCount = _userHelper.GetUsers().Count,
                Reports = [.. reports.Where(x => x.Status == Status.Approved).Take(5)],
                PendingReports = [.. reports.Where(x => x.Status == Status.Pending).Take(5)]
            };
            return View(data);
        }

        [HttpPost]
        public IActionResult ExtendPasswordDuration(string userId, int extraDays)
        {
            if (string.IsNullOrEmpty(userId))
                return ResponseHelper.JsonError("Invalid input");

            var user = _context.ApplicationUsers.FirstOrDefault(u => u.Id == userId);
            if (user == null)
                return ResponseHelper.JsonError("User not found");

            if (user.PassWordType == PassWordType.DoNotExpire)
                return ResponseHelper.JsonError("This user's password does not expire.");

            if (!user.PasswordExpiryDate.HasValue)
                user.PasswordExpiryDate = DateTime.UtcNow;

            user.PasswordExpiryDate = user.PasswordExpiryDate.Value.AddDays(extraDays);
            _context.SaveChanges();

            return ResponseHelper.JsonSuccess("Password extended successfully");
        }

        [HttpPost]
        public async Task<JsonResult> RegisterUser(string userData)
        {
            if (string.IsNullOrEmpty(userData))
            {
                return ResponseHelper.ErrorMsg();
            }
            var applicationUserViewModel = JsonConvert.DeserializeObject<ApplicationUserViewModel>(userData);
            if (applicationUserViewModel == null)
            {
                return ResponseHelper.ErrorMsg();
            }
            var checkForUser = await _userHelper.FindByEmailAsync(applicationUserViewModel.Email).ConfigureAwait(false);
            if (checkForUser != null)
            {
                return ResponseHelper.JsonError("Email already in use by another user");
            }
            var user = await _userHelper.RegisterUser(applicationUserViewModel).ConfigureAwait(false);
            if (user == null)
            {
                return ResponseHelper.ErrorMsg();
            }

            var request = HttpContext.Request;
            string baseUrl = $"{request.Scheme}://{request.Host}";

            _emailTemplateService.SendRegistrationEmail(user, baseUrl);
            return ResponseHelper.JsonSuccess("User registered successfully");
        }

        [HttpGet]
        public IActionResult Researcher()
        {
            var users = _userHelper.GetUsers();
            return View(users);
        }

        [HttpPost]
        public async Task<IActionResult> ChangePasswordType(string userId, string newType)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return ResponseHelper.JsonError("User not found.");

            if (!Enum.TryParse<PassWordType>(newType, out var passType))
                return ResponseHelper.JsonError("Invalid password type.");

            user.PassWordType = passType;

            switch (passType)
            {
                case PassWordType.TwoWeeks:
                    user.PasswordExpiryDate = DateTime.UtcNow.AddDays(14);
                    break;
                case PassWordType.OneWeek:
                    user.PasswordExpiryDate = DateTime.UtcNow.AddDays(7);
                    break;
                case PassWordType.DoNotExpire:
                    user.PasswordExpiryDate = null;
                    break;
            }

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
                return ResponseHelper.JsonSuccess("Password type updated successfully.");

            return ResponseHelper.JsonError("Failed to update password type.");
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(string userId, string newPassword)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return Json(new { isError = true, msg = "User not found." });

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, newPassword);

            if (result.Succeeded)
                return ResponseHelper.JsonSuccess("Password changed successfully.");

            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return ResponseHelper.JsonError("Failed to change password");
        }

        [HttpPost]
        public async Task<JsonResult> MakeUserAdmin(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return ResponseHelper.JsonError("User not found");

            var isAlreadyAdmin = await _userManager.IsInRoleAsync(user, SeedItems.AdminRole);
            if (isAlreadyAdmin)
                return ResponseHelper.JsonError("User is already an admin");

            var result = await _userManager.AddToRoleAsync(user, SeedItems.AdminRole);
            if (result.Succeeded)
                return ResponseHelper.JsonSuccess("User promoted to admin successfully");

            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return ResponseHelper.JsonError($"Failed to promote user: {errors}");
        }

    }
}
