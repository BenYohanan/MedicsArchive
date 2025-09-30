using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.ViewModels
{
    public class AdminDashboardDTO
    {
        public int? ClientCount { get; set; }
        public string? UserName { get; set; }
        public int? AllResultCount { get; set; }
        public int? RejectedResultCount { get; set; }
        public int? PendingResultCount { get; set; }
        public List<ReportViewModel>? Reports { get; set; }
        public List<ReportViewModel>? PendingReports { get; set; }
    }
}
