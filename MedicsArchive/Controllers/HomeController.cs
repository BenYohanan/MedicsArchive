using Data.DbContext;
using MedicsArchive.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace MedicsArchive.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        public readonly AppDbContext _db;
        public HomeController(ILogger<HomeController> logger, AppDbContext db)
        {
            _logger = logger;
            _db = db;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
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
                Age = report.Age.ToString(),
                Institution = report.Institution,
                Status = report.Status,
                Id = report.Id
            };

            ViewBag.IsBulk = isBulk;
            return View(vm);
        }
    }
}
