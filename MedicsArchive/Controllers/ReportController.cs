using Data.DbContext;
using Data.Models;
using Data.ViewModels;
using MedicsArchive.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Service.Helpers;

namespace MedicsArchive.Controllers
{
    [Authorize]
    public class ReportController : Controller
	{
		public readonly IReportHelper reportHelper;
		public readonly IOpenAIService openAIService;
		public readonly IUserHelper _userHelper;
        public readonly AppDbContext _db;
		public readonly IEmailTemplateService _emailTemplateService;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public ReportController(IReportHelper reportHelper, IUserHelper userHelper, AppDbContext appDbContext, IOpenAIService openAIService, IEmailTemplateService emailTemplateService, IWebHostEnvironment webHostEnvironment)
        {
            this.reportHelper = reportHelper;
            _userHelper = userHelper;
            _db = appDbContext;
            this.openAIService = openAIService;
            _emailTemplateService = emailTemplateService;
            _webHostEnvironment = webHostEnvironment;
        }

        [HttpGet]
		public IActionResult Index(IPageListModel<ReportViewModel> model, int page = 1)
		{
			var isAdmin = User.IsInRole(SeedItems.AdminRole);
			ViewBag.IsAdmin = isAdmin;
			var reports = reportHelper.PatientReports(isAdmin, model, page);
			model.Model = reports;
			model.SearchAction = "Index";
			model.SearchController = "Report";
			return View(model);
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

			var report = _db.Reports.Include(r => r.User).FirstOrDefault(r => r.Id == reportId);
            if (isAccept)
            {
                _emailTemplateService.SendReportApprovalEmail(report.User, report.Exam);
            }
            else
            {
                _emailTemplateService.SendReportRejectionEmail(report.User, report.Exam);
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
        public JsonResult BulkDelete(List<long> ids)
        {
            var reports = _db.Reports
                .Where(r => ids.Contains(r.Id) && r.Active)
                .ExecuteUpdate(update => update
                    .SetProperty(r => r.Active, false)
                );
            if (reports == 0)
            {
                return ResponseHelper.JsonError("Unable to delete");
            }

            return ResponseHelper.JsonSuccess("Reports deleted successfully.");
        }

        public async Task<IActionResult> DownloadReports(string ids)
        {
            var idList = ids.Split(',', StringSplitOptions.RemoveEmptyEntries)
                            .Select(long.Parse)
                            .ToList();

            if (!idList.Any())
                return BadRequest("No report IDs provided.");
            string redirectUrl = string.Empty;
            var reports = await _db.Reports
                .Where(r => idList.Contains(r.Id))
                .ToListAsync();

            if (reports.Count == 1)
            {
                var id = reports.First().Id;
                var site = $"https://{HttpContext.Request.Host}/Home/Result?id={id}";
                var fileName = "\\" + $"{id}-Result.pdf";
                var path = reportHelper.CreateFileServices(site, fileName);
                var filePath = Path.Combine(_webHostEnvironment.WebRootPath, path.Replace("/", "\\"));
                redirectUrl = $"https://{HttpContext.Request.Host}/{path}";
                return Json(new { isError = false, redirectUrl });
            }

            using var ms = new MemoryStream();
            using (var archive = new System.IO.Compression.ZipArchive(ms, System.IO.Compression.ZipArchiveMode.Create, true))
            {
                foreach (var report in reports)
                {
                    var site = $"https://{HttpContext.Request.Host}/Home/Result?id={report.Id}&isBulk=true";
                    var fileName = "\\" + $"{report.Id}-Result.pdf";
                    var path = reportHelper.CreateFileServices(site, fileName);
                    var fullPath = Path.Combine(_webHostEnvironment.WebRootPath, path.Replace("/", "\\"));

                    if (System.IO.File.Exists(fullPath))
                    {
                        var entry = archive.CreateEntry($"{report.Id}-Result.pdf");
                        using var entryStream = entry.Open();
                        using var fileStream = System.IO.File.OpenRead(fullPath);
                        await fileStream.CopyToAsync(entryStream);
                    }
                }
            }

            ms.Position = 0;

            var reportsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "reports");
            if (!Directory.Exists(reportsFolder))
                Directory.CreateDirectory(reportsFolder);

            var zipName = $"Reports-{DateTime.Now:yyyyMMddHHmmss}.zip";
            var zipPath = Path.Combine(reportsFolder, zipName);

            await System.IO.File.WriteAllBytesAsync(zipPath, ms.ToArray());

            redirectUrl = $"https://{HttpContext.Request.Host}/reports/{zipName}";
            return Json(new { isError = false, redirectUrl });
        }
    }
}
