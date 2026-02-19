using Aspose.Words;
using Data.DbContext;
using Data.Models;
using Data.ViewModels;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Text;
using System.Text.Json;

namespace Service.Helpers
{
	public interface IOpenAIService
	{
		Task<bool> ExtractPatientDataFromFilesAsync(IEnumerable<string> filePaths, bool isAdmin, string userId);
	}

	public class OpenAIService : IOpenAIService
	{
		private readonly AppDbContext _db;
		private readonly ILogger<OpenAIService> _logger;
		private readonly HttpClient _httpClient;

		public OpenAIService(
			AppDbContext db,
			ILogger<OpenAIService> logger,
			HttpClient httpClient)
		{
			_db = db;
			_logger = logger;
			_httpClient = httpClient;
		}

		public async Task<bool> ExtractPatientDataFromFilesAsync(
			IEnumerable<string> filePaths,
			bool isAdmin,
			string userId)
		{
			var semaphore = new SemaphoreSlim(5); 
			var tasks = new List<Task<bool>>();

			foreach (var filePath in filePaths)
			{
				tasks.Add(ProcessFileAsync(filePath, isAdmin, userId, semaphore));
			}

			var results = await Task.WhenAll(tasks);
			return results.Any(x => x);
		}

		private async Task<bool> ProcessFileAsync(
			string filePath,
			bool isAdmin,
			string userId,
			SemaphoreSlim semaphore)
		{
			await semaphore.WaitAsync();

			try
			{
				var patientInfo = await ExtractPatientDataAsync(filePath);
				if (patientInfo == null)
					return false;

				DateTime.TryParseExact(patientInfo.DOB, "MM/dd/yyyy",
					CultureInfo.InvariantCulture, DateTimeStyles.None, out var dob);

				DateTime.TryParseExact(patientInfo.StudyDate, "MM/dd/yyyy",
					CultureInfo.InvariantCulture, DateTimeStyles.None, out var studyDate);

				long? age = null;

				if (!string.IsNullOrWhiteSpace(patientInfo.Age) &&
					long.TryParse(patientInfo.Age, out var parsedAge))
				{
					age = parsedAge;
				}
				else if (dob != default && studyDate != default)
				{
					age = CalculateAge(dob, studyDate);
				}

				var report = new Report
				{
					PatientID = patientInfo.PatientID,
					PatientName = patientInfo.PatientName,
					DOB = dob == default ? null : dob,
					Sex = patientInfo.Sex,
					StudyDescription = patientInfo.Findings,
					StudyDate = studyDate == default ? null : studyDate,
					Exam = patientInfo.Exam,
					ClinicalInformation = patientInfo.ClinicalInformation,
					Conclusion = patientInfo.Conclusion,
					Age = age,
					Institution = patientInfo.Institution,
					Status = isAdmin ? Status.Approved : Status.Pending,
					UserId = userId
				};

				_db.Reports.Add(report);
				await _db.SaveChangesAsync();

				_logger.LogInformation($"Processed: {filePath}");
				return true;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"Failed: {filePath}");
				return false;
			}
			finally
			{
				semaphore.Release();
			}
		}

		private int CalculateAge(DateTime dob, DateTime studyDate)
		{
			int age = studyDate.Year - dob.Year;
			if (studyDate < dob.AddYears(age))
				age--;
			return age;
		}

		private async Task<ReportViewModel?> ExtractPatientDataAsync(string filePath)
		{
			string extension = Path.GetExtension(filePath).ToLower();

			string text = extension switch
			{
				".pdf" => ExtractTextFromPdf(filePath),
				".doc" => ExtractTextFromWord(filePath),
				".docx" => ExtractTextFromWord(filePath),
				_ => throw new NotSupportedException($"Unsupported file type: {filePath}")
			};

			return await ParsePatientInfoAsync(text);
		}

		private string ExtractTextFromPdf(string filePath)
		{
			using var pdfReader = new PdfReader(filePath);
			using var pdfDoc = new PdfDocument(pdfReader);
			StringBuilder text = new();

			for (int i = 1; i <= pdfDoc.GetNumberOfPages(); i++)
			{
				text.Append(PdfTextExtractor.GetTextFromPage(pdfDoc.GetPage(i)));
				text.AppendLine();
			}

			return text.ToString();
		}

		private string ExtractTextFromWord(string filePath)
		{
			var doc = new Document(filePath);
			return doc.ToString(SaveFormat.Text);
		}

		private async Task<ReportViewModel?> ParsePatientInfoAsync(string text)
		{
			var requestBody = new
			{
				model = "gpt-4.1-mini",
				input = new[]
				{
					new {
						role = "system",
						content = "Extract structured patient data and return JSON only."
					},
					new {
						role = "user",
						content = $@"
							Extract:
							- PatientID
							- PatientName
							- DOB (MM/DD/YYYY)
							- Sex
							- Findings
							- StudyDate (MM/DD/YYYY)
							- Exam
							- ClinicalInformation
							- Conclusion
							- Institution
							- Age

							Return JSON only.

							Document:
							{text}"
					}
				}
			};

			var content = new StringContent(
				JsonSerializer.Serialize(requestBody),
				Encoding.UTF8,
				"application/json");

			using var response = await _httpClient.PostAsync(
				"https://api.openai.com/v1/responses",
				content);

			if (!response.IsSuccessStatusCode)
			{
				var error = await response.Content.ReadAsStringAsync();
				throw new Exception($"OpenAI API Error: {error}");
			}

			var json = await response.Content.ReadAsStringAsync();
			using var doc = JsonDocument.Parse(json);

			var outputText = doc.RootElement
				.GetProperty("output")[0]
				.GetProperty("content")[0]
				.GetProperty("text")
				.GetString();

			if (string.IsNullOrWhiteSpace(outputText))
				return null;

			outputText = outputText.Trim();
			if (outputText.StartsWith("```json") && outputText.EndsWith("```"))
			{
				outputText = outputText[7..^3].Trim();
			}
			else if (outputText.StartsWith("```") && outputText.EndsWith("```"))
			{
				outputText = outputText[3..^3].Trim();
			}

			try
			{
				return JsonSerializer.Deserialize<ReportViewModel>(outputText);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "JSON Deserialization failed");
				return null;
			}
		}
	}
}
