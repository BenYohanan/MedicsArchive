using Data.DbContext;
using Data.Models;
using Data.ViewModels;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Service.Helpers
{
    public interface IOpenAIService
    {
        Task<bool> ExtractPatientDataFromFilesAsync(IEnumerable<string> filePaths, bool isAdmin);
    }
    public class OpenAIService: IOpenAIService
    {
        private readonly AppDbContext _db;
        private readonly IConfiguration _configuration;
        private readonly ILogger<OpenAIService> _logger;
        private readonly string _apiKey;

        public OpenAIService(AppDbContext db, IConfiguration configuration, ILogger<OpenAIService> logger)
        {
            _db = db;
            _configuration = configuration;
            _logger = logger;
            _apiKey = configuration["OpenAI:ApiKey"];
        }

        public async Task<bool> ExtractPatientDataFromFilesAsync(IEnumerable<string> filePaths, bool isAdmin)
        {
            try
            {
                var patientInfos = new List<ReportViewModel>();

                foreach (var filePath in filePaths)
                {
                    var patientInfo = await ExtractPatientDataAsync(filePath);
                    if (patientInfo != null)
                    {
                        patientInfos.Add(patientInfo);
                    }
                }

                var reports = patientInfos.Select(r => new Report
                {
                    PatientID = r.PatientID,
                    PatientName = r.PatientName,
                    DOB = DateTime.ParseExact(r.DOB, "MM/dd/yyyy", CultureInfo.InvariantCulture),
                    Sex = r.Sex,
                    StudyDescription = r.Findings,
                    StudyDate = DateTime.ParseExact(r.StudyDate, "MM/dd/yyyy", CultureInfo.InvariantCulture),
                    Exam = r.Exam,
                    ClinicalInformation = r.ClinicalInformation,
                    Conclusion = r.Conclusion,
                    Age = CalculateAge(
                        DateTime.ParseExact(r.DOB, "MM/dd/yyyy", CultureInfo.InvariantCulture),
                        DateTime.ParseExact(r.StudyDate, "MM/dd/yyyy", CultureInfo.InvariantCulture)),
                    Institution = r.Institution,
                    Status = isAdmin ? Status.Approved : Status.Pending,
                }).ToList();

                _db.AddRange(reports);
                _db.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {

                throw ex;
            }
            
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

        private async Task<ReportViewModel?> ExtractPatientDataAsync(string filePath)
        {
            try
            {
                string text = Path.GetExtension(filePath).ToLower() switch
                {
                    ".pdf" => ExtractTextFromPdf(filePath),
                    ".docx" => ExtractTextFromDocx(filePath),
                    _ => throw new NotSupportedException($"Unsupported file type: {filePath}")
                };

                return await ParsePatientInfoAsync(text);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error processing file {filePath}");
                return null;
            }
        }

        private string ExtractTextFromPdf(string filePath)
        {
            using var pdfReader = new PdfReader(filePath);
            using var pdfDoc = new PdfDocument(pdfReader);
            string text = string.Empty;

            for (int i = 1; i <= pdfDoc.GetNumberOfPages(); i++)
            {
                text += PdfTextExtractor.GetTextFromPage(pdfDoc.GetPage(i));
            }

            return text;
        }

        private string ExtractTextFromDocx(string filePath)
        {
            StringBuilder text = new StringBuilder();
            using (WordprocessingDocument doc = WordprocessingDocument.Open(filePath, false))
            {
                var body = doc.MainDocumentPart.Document.Body;
                foreach (var para in body.Elements<Paragraph>())
                {
                    foreach (var run in para.Elements<Run>())
                    {
                        foreach (var textElement in run.Elements<Text>())
                        {
                            text.Append(textElement.Text);
                            text.Append(" ");
                        }
                    }
                    text.AppendLine();
                }

                foreach (var table in body.Elements<Table>())
                {
                    foreach (var row in table.Elements<TableRow>())
                    {
                        foreach (var cell in row.Elements<TableCell>())
                        {
                            foreach (var para in cell.Elements<Paragraph>())
                            {
                                foreach (var run in para.Elements<Run>())
                                {
                                    foreach (var textElement in run.Elements<Text>())
                                    {
                                        text.Append(textElement.Text);
                                        text.Append(" ");
                                    }
                                }
                                text.AppendLine();
                            }
                        }
                    }
                }
            }
            return text.ToString().Trim();
        }
        
        private async Task<ReportViewModel?> ParsePatientInfoAsync(string text)
        {
            string systemPrompt = "You are a helpful assistant that extracts structured patient data from text.";
            string userPrompt = @$"
                Extract the following fields from the provided document text as a JSON object:
                - PatientID (or 'Patient ID')
                - PatientName (or 'Patient Name')
                - DOB (or 'Date of Birth')
                - Sex (or 'Gender')
                - Findings
                - StudyDate (or 'Study Date')
                - Exam
                - ClinicalInformation (or 'Clinical Information' or 'CLINICAL SUMMARY')
                - Conclusion
                - Institution

                Return the result in JSON format. If a field is missing, set its value to null. Ensure date fields (DOB, StudyDate) are formatted as MM/DD/YYYY.

                Here is the document text:
                {text}";

            var messages = new object[]
            {
                new { role = "system", content = systemPrompt },
                new { role = "user", content = userPrompt }
            };

            var requestBody = new
            {
                model = "gpt-4o",
                messages,
                temperature = 0.2,
                response_format = new { type = "json_object" }
            };

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };

            var jsonContent = JsonSerializer.Serialize(requestBody, options);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
            client.Timeout = TimeSpan.FromSeconds(180);

            using var response = await client.PostAsync("https://api.openai.com/v1/chat/completions", content);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"OpenAI API call failed: {response.StatusCode} - {error}");
            }

            using var stream = await response.Content.ReadAsStreamAsync();
            using var jsonDoc = await JsonDocument.ParseAsync(stream);
            var root = jsonDoc.RootElement;

            var choices = root.GetProperty("choices");
            if (choices.GetArrayLength() == 0)
                throw new Exception("No choices returned from OpenAI.");

            var message = choices[0].GetProperty("message");
            var resultContent = message.GetProperty("content").GetString();

            if (string.IsNullOrWhiteSpace(resultContent))
                return null;

            resultContent = resultContent.Trim();
            if (resultContent.StartsWith("```json") && resultContent.EndsWith("```"))
            {
                resultContent = resultContent[7..^3].Trim();
            }
            else if (resultContent.StartsWith("```") && resultContent.EndsWith("```"))
            {
                resultContent = resultContent[3..^3].Trim();
            }

            try
            {
                return JsonSerializer.Deserialize<ReportViewModel>(resultContent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to deserialize AI response");
                return null;
            }
        }
    }
}
