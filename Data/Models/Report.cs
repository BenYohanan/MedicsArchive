using System.ComponentModel.DataAnnotations;

namespace Data.Models
{
	public class Report
	{
		public Report()
		{
			DateCreated = DateTime.Now;
			Active = true;
		}
		[Key]
		public long Id { get; set; }
		public string? PatientID { get; set; }
		public DateTime? DOB { get; set; }
		public string? StudyDescription { get; set; }
		public string? PatientName { get; set; }
		public string? Sex { get; set; }
		public DateTime? StudyDate { get; set; }
		public string? Exam { get; set; }
		public string? ClinicalInformation { get; set; }
		public string? Conclusion { get; set; }
		public string? Institution { get; set; }
		public long? Age { get; set; }
		public bool Active { get; set; }
		public bool IsApproved { get; set; }
		public DateTime? DateCreated { get; set; }
	}
	public class ReportViewModel
	{
		public long? Id { get; set; }
		public string PatientID { get; set; }
		public string DOB { get; set; }
		public string StudyDescription { get; set; }
		public string PatientName { get; set; }
		public string Sex { get; set; }
		public string StudyDate { get; set; }
		public string Exam { get; set; }
		public string ClinicalInformation { get; set; }
		public string Conclusion { get; set; }
		public string Institution { get; set; }
		public long? Age { get; set; }
		public bool IsApproved { get; set; }
	}
}
