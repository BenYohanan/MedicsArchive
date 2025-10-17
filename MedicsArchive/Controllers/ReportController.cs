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

        public async Task<IActionResult> DownloadReports(string ids)
        {
            var idList = ids.Split(',', StringSplitOptions.RemoveEmptyEntries)
                            .Select(long.Parse)
                            .ToList();

            if (!idList.Any())
                return BadRequest("No report IDs provided.");

            var reports = await _db.Reports
                .Where(r => idList.Contains(r.Id))
                .ToListAsync();

            if (reports.Count == 1)
            {
                var id = reports.First().Id;
                var site = $"https://{HttpContext.Request.Host}/Report/Result?id={id}";
                var fileName = "\\" + $"{id}-Result.pdf";
                var path = reportHelper.CreateFileServices(site, fileName);
                var filePath = Path.Combine(_webHostEnvironment.WebRootPath, path.Replace("/", "\\"));
                var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
                return File(fileBytes, "application/pdf", $"{id}-Result.pdf");
            }

            using var ms = new MemoryStream();
            using (var archive = new System.IO.Compression.ZipArchive(ms, System.IO.Compression.ZipArchiveMode.Create, true))
            {
                foreach (var report in reports)
                {
                    var site = $"https://{HttpContext.Request.Host}/Report/Result?id={report.Id}&isBulk=true";
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
            return File(ms.ToArray(), "application/zip", "Reports.zip");
        }

        public async Task<IActionResult> Result(int id, bool isBulk = false)
        {
            var report = await _db.Reports.FirstOrDefaultAsync(r => r.Id == id);
            if (report == null)
                return NotFound("Report not found.");

            var vm = new Data.ViewModels.ReportViewModel
            {
                PatientID = report.PatientID,
                PatientName = report.PatientName,
                DOB = report.DOB.Value.ToString("dd/MM/yyyy"),
                Sex = report.Sex,
                ClinicalInformation = report.ClinicalInformation,
                Conclusion = report.Conclusion,
                Exam = report.Exam,
                StudyDate = report.StudyDate.Value.ToString("dd/MM/yyyy"),
                DateCreated = report.DateCreated.Value.ToString("dd/MM/yyyy"),
                Findings = report.StudyDescription,
                Age = report.Age,
                Institution = report.Institution,
                Status = report.Status,
                Id = report.Id
            };

            ViewBag.IsBulk = isBulk;
            return View(vm);
        }

    }
}
