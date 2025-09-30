using Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.ViewModels
{
    public class ReportViewModel
    {
        public long? Id { get; set; }
        public string? PatientID { get; set; }
        public string? DOB { get; set; }
        public string? Findings { get; set; }
        public string? PatientName { get; set; }
        public string? Sex { get; set; }
        public string? StudyDate { get; set; }
        public string? DateCreated { get; set; }
        public string? Exam { get; set; }
        public string? ClinicalInformation { get; set; }
        public string? Conclusion { get; set; }
        public string? Institution { get; set; }
        public long? Age { get; set; }
        public Status? Status { get; set; }
    }
}
