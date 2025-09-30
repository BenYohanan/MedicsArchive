using Data.DbContext;
using Data.Models;
using Data.ViewModels;
using MedicsArchive.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Service.Helpers;

namespace MedicsArchive.Controllers
{
	public class ReportController : Controller
	{
		public readonly IReportHelper reportHelper;
		public readonly IOpenAIService openAIService;
		public readonly IUserHelper _userHelper;
		public readonly AppDbContext _appDbContext;
		public readonly IEmailTemplateService _emailTemplateService;
        public ReportController(IReportHelper reportHelper, IUserHelper userHelper, AppDbContext appDbContext, IOpenAIService openAIService, IEmailTemplateService emailTemplateService)
        {
            this.reportHelper = reportHelper;
            _userHelper = userHelper;
            _appDbContext = appDbContext;
            this.openAIService = openAIService;
            _emailTemplateService = emailTemplateService;
        }

        [HttpGet]
		public IActionResult Index()
		{
			var isAdmin = User.IsInRole(SeedItems.AdminRole);
			ViewBag.IsAdmin = isAdmin;
			var data = reportHelper.PatientReports(isAdmin);
			return View(data);
		}

		[HttpPost]
		public async Task<JsonResult> UploadFiles(List<IFormFile> files, bool isAdmin)
		{
			if (files == null || !files.Any())
			{
				return ResponseHelper.ErrorMsg();
			}

			var filePaths = new List<string>();

			try
			{
				foreach (var file in files)
				{
					var uniqueFileName = $"{Guid.NewGuid()}_{file.FileName}";
					var filePath = Path.Combine(Path.GetTempPath(), uniqueFileName);
					using (var stream = new FileStream(filePath, FileMode.Create))
					{
						await file.CopyToAsync(stream);
					}
					filePaths.Add(filePath);
				}

				var isSaved = await openAIService.ExtractPatientDataFromFilesAsync(filePaths, isAdmin).ConfigureAwait(false);
				//var isSaved = reportHelper.ExtractPatientDataFromPdfs(filePaths, isAdmin);
				var msg = isAdmin ? "All files processed successfully!" : "All files processed successfully, admin will approve when verified";
				foreach (var filePath in filePaths)
				{
					if (System.IO.File.Exists(filePath))
						System.IO.File.Delete(filePath);
				}
				return isSaved ? ResponseHelper.JsonSuccess(msg) : ResponseHelper.JsonError("Unable to upload file, contance admin if error persit");
			}
			catch (Exception ex)
			{
				return ResponseHelper.ErrorMsg();
				throw;
			}
		}
		[HttpGet]
		public IActionResult Researcher()
		{
			var users = _userHelper.GetUsers();
			return View(users);
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
			_emailTemplateService.SendRegistrationEmail(user);
            return ResponseHelper.JsonSuccess("User registered successfully");
		}
		
        [HttpPost]
        public JsonResult DecideResultStatus(long reportId, bool isAccept)
        {
            if (reportId <= 0)
            {
                return ResponseHelper.ErrorMsg();
            }
			var status = isAccept ? Status.Approved : Status.Rejected;

            int rowsAffected = _appDbContext.Reports
                .Where(r => r.Id == reportId && r.Active)
                .ExecuteUpdate(update => update
                    .SetProperty(r => r.Status, status)
                );

            if (rowsAffected == 0)
            {
                return ResponseHelper.JsonError("Unable to approve");
            }

			var user = _appDbContext.Reports.Include(r => r.User).FirstOrDefault(r => r.Id == reportId)?.User;
            if (isAccept)
            {
                _emailTemplateService.SendReportApprovalEmail(user);
            }
            else
            {
                _emailTemplateService.SendReportRejectionEmail(user);
            }

            return ResponseHelper.JsonSuccess($"Report {(isAccept ? "approved" : "rejected")} successfully.");
        }
        [HttpPost]
		public JsonResult Delete(long reportId)
		{
			if (reportId <= 0)
			{
				return ResponseHelper.ErrorMsg();
			}

			int rowsAffected = _appDbContext.Reports
				.Where(r => r.Id == reportId && r.Active)
				.ExecuteUpdate(update => update
					.SetProperty(r => r.Active, false)
				);

			if (rowsAffected == 0)
			{
				return ResponseHelper.JsonError("Unable to delete");
			}

			return ResponseHelper.JsonSuccess("Report deleted successfully.");
		}
	}
}
