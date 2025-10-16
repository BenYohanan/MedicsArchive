using Data.DbContext;
using Data.Models;
using MedicsArchive.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Service.Helpers;

namespace MedicsArchive.Controllers
{
	public class ReportController : Controller
	{
		public readonly IReportHelper reportHelper;
		public readonly IOpenAIService openAIService;
		public readonly IUserHelper _userHelper;
        public readonly AppDbContext _db;
		public readonly IEmailTemplateService _emailTemplateService;
        public ReportController(IReportHelper reportHelper, IUserHelper userHelper, AppDbContext appDbContext, IOpenAIService openAIService, IEmailTemplateService emailTemplateService)
        {
            this.reportHelper = reportHelper;
            _userHelper = userHelper;
            _db = appDbContext;
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
				var user =await _userHelper.FindByUserNameAsync(User.Identity.Name).ConfigureAwait(false);
				if (user == null)
				{
					return ResponseHelper.ErrorMsg();
                }
				foreach(var file in files)
				{
					var uniqueFileName = $"{Guid.NewGuid()}_{file.FileName}";
					var filePath = Path.Combine(Path.GetTempPath(), uniqueFileName);
					using (var stream = new FileStream(filePath, FileMode.Create))
					{
						await file.CopyToAsync(stream);
					}
					filePaths.Add(filePath);
				}

				var isSaved = await openAIService.ExtractPatientDataFromFilesAsync(filePaths, isAdmin, user.Id).ConfigureAwait(false);
				var msg = isAdmin ? "✅ All files processed successfully!" : "✅All files processed successfully, admin will approve when verified";
				foreach (var filePath in filePaths)
				{
					if (System.IO.File.Exists(filePath))
						System.IO.File.Delete(filePath);
				}
				return isSaved ? ResponseHelper.JsonSuccess(msg) : ResponseHelper.JsonError("❌ Unable to upload file, contance admin if error persit");
			}
			catch (Exception ex)
			{
				return ResponseHelper.ErrorMsg();
				throw;
			}
		}
		
        [HttpPost]
        public JsonResult DecideResultStatus(long reportId, bool isAccept)
        {
            if (reportId <= 0)
            {
                return ResponseHelper.ErrorMsg();
            }
			var status = isAccept ? Status.Approved : Status.Rejected;

            int rowsAffected = _db.Reports
                .Where(r => r.Id == reportId && r.Active)
                .ExecuteUpdate(update => update
                    .SetProperty(r => r.Status, status)
                );

            if (rowsAffected == 0)
            {
                return ResponseHelper.JsonError("Unable to approve");
            }

			var user = _db.Reports.Include(r => r.User).FirstOrDefault(r => r.Id == reportId)?.User;
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

			int rowsAffected = _db.Reports
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

        [HttpPost]
        public async Task<IActionResult> BulkUpdateStatus(List<long> ids, bool approve)
        {
            var reports = _db.Reports.Where(r => ids.Contains(r.Id)).ToList();
            foreach (var report in reports)
            {
                report.Status = approve ? Status.Approved : Status.Rejected;
            }

            await _db.SaveChangesAsync();
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> BulkDelete(List<long> ids)
        {
            var reports = _db.Reports.Where(r => ids.Contains(r.Id));
            _db.Reports.RemoveRange(reports);
            await _db.SaveChangesAsync();
            return Ok();
        }

        [HttpGet]
        public async Task<IActionResult> DownloadBulk(string ids)
        {
            var idList = ids.Split(',').Select(long.Parse).ToList();
            var reports = await _db.Reports.Where(r => idList.Contains(r.Id)).ToListAsync();

            // Generate ZIP of PDF reports, for example
            using var ms = new MemoryStream();
            using (var archive = new System.IO.Compression.ZipArchive(ms, System.IO.Compression.ZipArchiveMode.Create, true))
            {
                foreach (var report in reports)
                {
                    var entry = archive.CreateEntry($"{report.PatientName ?? "Report"}_{report.Id}.txt");
                    using var writer = new StreamWriter(entry.Open());
                    await writer.WriteAsync($"{report.PatientName}\n{report.StudyDescription}\n{report.Conclusion}");
                }
            }

            ms.Position = 0;
            return File(ms.ToArray(), "application/zip", "Reports.zip");
        }

    }
}
