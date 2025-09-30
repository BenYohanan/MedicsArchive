using Data.DbContext;
using Data.Models;
using Data.ViewModels;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;

namespace Service.Helpers
{
	public interface IReportHelper
	{
		bool ExtractPatientDataFromPdfs(IEnumerable<string> filePaths, bool isAdmin);
		List<ReportViewModel> PatientReports(bool isAdmin);
	}

	public class ReportHelper : IReportHelper
	{
		private readonly AppDbContext db;
		public ReportHelper(AppDbContext appDbContext)
		{
			this.db = appDbContext;
		}

		public List<ReportViewModel> PatientReports(bool isAdmin)
		{
			var query = db.Reports.Where(x => x.Active).AsQueryable();
			if (!isAdmin)
			{
				query = query.Where(x => x.Status != Status.Rejected).AsQueryable();
			}
			return query.OrderByDescending(x=>x.DateCreated).Select(r=> new ReportViewModel
			{
				PatientID = r.PatientID,
				PatientName = r.PatientName,
				DOB = r.DOB.Value.ToString("dd/MMM/yyyy"),
				Sex = r.Sex,
				ClinicalInformation = r.ClinicalInformation,
				Conclusion = r.Conclusion,
				Exam = r.Exam,
				StudyDate = r.StudyDate.Value.ToString("dd/MMM/yyyy"),
                DateCreated = r.DateCreated.Value.ToString("dd/MMM/yyyy"),
				Findings = r.StudyDescription,
				Age = r.Age,
				Institution = r.Institution,
                Status = r.Status,
				Id = r.Id
			}).ToList();
		}

		public bool ExtractPatientDataFromPdfs(IEnumerable<string> filePaths, bool isAdmin)
		{
			var patientInfos = new List<ReportViewModel>();

			foreach (var file in filePaths)
			{
				var patientInfo = ExtractPatientData(file);
				if (patientInfo != null)
				{
					patientInfos.Add(patientInfo);
				}
			}
			var reports = patientInfos.Select(r => new Report
			{
				PatientID = r.PatientID,
				PatientName = r.PatientName,
				DOB = DateTime.Parse(r.DOB),
				Sex = r.Sex,
				StudyDescription = r.Findings,
				StudyDate = DateTime.Parse(r.StudyDate),
				Exam = r.Exam,
				ClinicalInformation = r.ClinicalInformation,
				Conclusion = r.Conclusion,
				Age = CalculateAge(DateTime.Parse(r.DOB), DateTime.Parse(r.StudyDate)),
				Institution = r.Institution,
				Status = isAdmin ? Status.Approved : Status.Pending,
			}).ToList();

			db.AddRange(reports);
			db.SaveChanges();
			return true;
		}
		private int CalculateAge(DateTime dob, DateTime studyDate)
		{
			int age = studyDate.Year - dob.Year;

			if (studyDate < dob.AddYears(age))
			{
				age--;
			}

			return age;
		}
		private ReportViewModel ExtractPatientData(string filePath)
		{
			using (var pdfReader = new PdfReader(filePath))
			using (var pdfDoc = new PdfDocument(pdfReader))
			{
				string text = string.Empty;

				for (int i = 1; i <= pdfDoc.GetNumberOfPages(); i++)
				{
					text += PdfTextExtractor.GetTextFromPage(pdfDoc.GetPage(i));
				}

				return ParsePatientInfo(text);
			}
		}

		private ReportViewModel? ParsePatientInfo(string pdfText)
		{
			string ExtractField(string fieldName)
			{
				if (pdfText.Contains(fieldName))
				{
					var startIndex = pdfText.IndexOf(fieldName) + fieldName.Length;
					var subText = pdfText.Substring(startIndex).Trim();

					string[] nextFieldStart = { "Patient ID:", "Patient Name:", "DOB", "Sex:", "Study Description:", "Study Date:", "Referring Physician", "Institution:", "Exam:", "Clinical Information:", "Findings", "Conclusion:", "Patient:", "Page" };
					var endIndex = subText.Length;

					foreach (var nextField in nextFieldStart)
					{
						if (subText.Contains(nextField))
						{
							endIndex = subText.IndexOf(nextField);
							break;
						}
					}

					var fieldValue = subText.Substring(0, endIndex).Trim();
					return fieldValue;
				}
				return null;
			}

			var data = new ReportViewModel
			{
				PatientID = ExtractField("Patient ID:"),
				DOB = ExtractField("DOB:"),
				Findings = ExtractField("Study Description:"),
				PatientName = ExtractField("Patient Name:"),
				Sex = ExtractField("Sex:"),
				StudyDate = ExtractField("Study Date:"),
				Exam = ExtractField("Exam:"),
				ClinicalInformation = ExtractField("Clinical Information:"),
				Conclusion = ExtractField("Conclusion:").Split("Page")[0],
				Institution = ExtractField("Institution:").Split("\n")[0],
			};
			return data;
		}
	}

}
