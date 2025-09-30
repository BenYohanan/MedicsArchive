using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Models
{
	public class Report
	{
		public Report()
		{
			DateCreated = DateTime.Now;
			Active = true;
			Status = Models.Status.Pending;
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
		public Status? Status { get; set; }
		public DateTime? DateCreated { get; set; }
		public string? UserId { get; set; }
		[ForeignKey(nameof(UserId))]
		public virtual ApplicationUser? User { get; set; }
    }

	public enum Status
	{
		Approved,
		Rejected,
		Pending
	}
}
