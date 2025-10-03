using Data.DbContext;
using Data.Models;
using Data.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Service.Helpers;

namespace MedicsArchive.Controllers
{
    public class AdminController : Controller
    {
        public readonly IReportHelper reportHelper;
        public readonly IOpenAIService openAIService;
        public readonly IUserHelper _userHelper;
        public readonly AppDbContext _appDbContext;
        public readonly IEmailTemplateService _emailTemplateService;
        public AdminController(IReportHelper reportHelper, IUserHelper userHelper, AppDbContext appDbContext, IOpenAIService openAIService, IEmailTemplateService emailTemplateService)
        {
            this.reportHelper = reportHelper;
            _userHelper = userHelper;
            _appDbContext = appDbContext;
            this.openAIService = openAIService;
            _emailTemplateService = emailTemplateService;
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
                Reports = [.. reports.Where(x => x.Status != Status.Rejected && x.Status != Status.Pending).Take(5)],
                PendingReports = [.. reports.Where(x => x.Status == Status.Pending).Take(5)]
            };
            return View(data);
        }
    }

    
}
