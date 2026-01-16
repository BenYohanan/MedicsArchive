using Data.DbContext;
using Data.Models;
using Data.ViewModels;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using Microsoft.AspNetCore.Hosting;
using SelectPdf;
using X.PagedList;
using X.PagedList.Extensions;

namespace Service.Helpers
{
	public interface IReportHelper
	{
        string CreateFileServices(string site, string fileName);
        bool ExtractPatientDataFromPdfs(IEnumerable<string> filePaths, bool isAdmin);
		IPagedList<ReportViewModel> PatientReports(bool isAdmin, IPageListModel<ReportViewModel> model, int page);
		List<ReportViewModel> PatientReports(bool isAdmin);
	}

	public class ReportHelper : IReportHelper
	{
		private readonly AppDbContext db;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public ReportHelper(AppDbContext appDbContext, IWebHostEnvironment webHostEnvironment)
        {
            this.db = appDbContext;
            _webHostEnvironment = webHostEnvironment;
        }

        public List<ReportViewModel> PatientReports(bool isAdmin)
		{
			var query = db.Reports.Where(x => x.Active).AsQueryable();
			if (!isAdmin)
			{
				query = query.Where(x => x.Status != Status.Rejected).AsQueryable();
			}
			return [.. query.OrderByDescending(x=>x.DateCreated).Select(r=> new ReportViewModel
			{
				PatientID = r.PatientID,
				PatientName = r.PatientName,
				DOB = r.DOB.Value.ToString("dd/MM/yyyy"),
				Sex = r.Sex,
				ClinicalInformation = r.ClinicalInformation,
				Conclusion = r.Conclusion,
				Exam = r.Exam,
				StudyDate = r.StudyDate.Value.ToString("dd/MM/yyyy"),
                DateCreated = r.DateCreated.Value.ToString("dd/MM/yyyy"),
				Findings = r.StudyDescription,
				Age = r.Age,
				Institution = r.Institution,
                Status = r.Status,
				Id = r.Id
			})];
		}

		public IPagedList<ReportViewModel> PatientReports(bool isAdmin,IPageListModel<ReportViewModel> model,int page)
		{
			var query = db.Reports
				.Where(x => x.Active)
				.AsQueryable();

			if (!isAdmin)
			{
				query = query.Where(x => x.Status != Status.Rejected);
			}

			if (!string.IsNullOrWhiteSpace(model.Keyword))
			{
				var key = model.Keyword.ToLower();

				query = query.Where(p =>
					(p.PatientName ?? "").ToLower().Contains(key) ||
					(p.Sex ?? "").ToLower().Contains(key) ||
					(p.Exam ?? "").ToLower().Contains(key) ||
					(p.Institution ?? "").ToLower().Contains(key) ||
					(p.StudyDescription ?? "").ToLower().Contains(key) ||
					(p.ClinicalInformation ?? "").ToLower().Contains(key) ||
					(p.Conclusion ?? "").ToLower().Contains(key)
				);
			}

			if (model.StudyFromDate.HasValue)
			{
				query = query.Where(p => p.StudyDate >= model.StudyFromDate.Value);
			}

			if (model.StudyToDate.HasValue)
			{
				query = query.Where(p => p.StudyDate <= model.StudyToDate.Value);
			}

			if (model.AgeFrom.HasValue)
			{
				query = query.Where(p => p.Age >= model.AgeFrom.Value);
			}

			if (model.AgeTo.HasValue)
			{
				query = query.Where(p => p.Age <= model.AgeTo.Value);
			}

			if (!string.IsNullOrEmpty(model.Gender))
			{
				query = query.Where(p => p.Sex == model.Gender);
			}

			var reports = query
				.OrderByDescending(x => x.DateCreated)
				.Select(r => new ReportViewModel
				{
					Id = r.Id,
					PatientID = r.PatientID,
					PatientName = r.PatientName,
					DOB = r.DOB.HasValue ? r.DOB.Value.ToString("dd/MM/yyyy") : "",
					Sex = r.Sex,
					ClinicalInformation = r.ClinicalInformation,
					Conclusion = r.Conclusion,
					Exam = r.Exam,
					StudyDate = r.StudyDate.HasValue ? r.StudyDate.Value.ToString("dd/MM/yyyy") : "",
					DateCreated = r.DateCreated.HasValue ? r.DateCreated.Value.ToString("dd/MM/yyyy") : "",
					Findings = r.StudyDescription,
					Age = r.Age,
					Institution = r.Institution,
					Status = r.Status
				})
				.ToPagedList(page, 25);

			return reports;
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
			using (var pdfDoc = new iText.Kernel.Pdf.PdfDocument(pdfReader))
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
		public string CreateFileServices(string site, string fileName)
		{
			var folderToSaveFile = "Report";
			string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, folderToSaveFile);

			if (!Directory.Exists(uploadsFolder))
				Directory.CreateDirectory(uploadsFolder);

			var path = Path.Combine(uploadsFolder, fileName.TrimStart('\\', '/'));

			if (File.Exists(path))
				File.Delete(path);

			var converter = new HtmlToPdf();

			converter.Options.PdfPageSize = PdfPageSize.A4;
			converter.Options.PdfPageOrientation = PdfPageOrientation.Portrait;

			converter.Options.MarginTop = 20;
			converter.Options.MarginBottom = 20;
			converter.Options.MarginLeft = 20;
			converter.Options.MarginRight = 20;

			converter.Options.WebPageWidth = 794;
			converter.Options.WebPageHeight = 0;

			converter.Options.AllowContentHeightResize = true;
			converter.Options.ColorSpace = PdfColorSpace.RGB;

			converter.Options.JavaScriptEnabled = false;

			var pdfUrl = site.Contains("?")
				? site + "&_pdf=" + Guid.NewGuid()
				: site + "?_pdf=" + Guid.NewGuid();

			var doc = converter.ConvertUrl(pdfUrl);
			doc.Save(path);
			doc.Close();

			var pathParts = path.Split(Path.DirectorySeparatorChar);
			var folderIndex = Array.IndexOf(pathParts, folderToSaveFile);
			return string.Join("/", pathParts.Skip(folderIndex));
		}


	}

}
