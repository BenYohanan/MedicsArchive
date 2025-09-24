using System;
using System.IO;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf;
using Data.Models;

public static class FileDataHelper
{
	//public static string ExtractTextFromWord(string filePath)
	//{
	//	using (WordprocessingDocument wordDoc = WordprocessingDocument.Open(filePath, false))
	//	{
	//		return wordDoc.MainDocumentPart.Document.Body.InnerText;
	//	}
	//}

	//// Extract key report data from the extracted text
	//public static ReportData ExtractReportData(string text)
	//{
	//	var data = new ReportData();

	//	try
	//	{
	//		data.CompanyName = ExtractValue(text, "HOSPITAL", "\n").Trim();
	//		data.PatientNumber = ExtractValue(text, "MHN #", "\n");
	//		data.PatientName = $"{ExtractValue(text, "SURNAME", "\n")} {ExtractValue(text, "FORENAMES", "\n")}".Trim();
	//		data.DateOfBirth = DateTime.Parse(ExtractValue(text, "D.O.B", "\n"));
	//		data.ExamDate = DateTime.Parse(ExtractValue(text, "DATE", "\n"));
	//		data.Exam = ExtractValue(text, "CLINICAL SUMMARY", "TECHNIQUE");
	//		data.Conclusion = ExtractValue(text, "CONCLUSION", "DOCTOR");

	//		return data;
	//	}
	//	catch (Exception ex)
	//	{
	//		Console.WriteLine($"Error extracting report data: {ex.Message}");
	//		return data;
	//	}
	//}

	//private static string ExtractValue(string text, string startDelimiter, string endDelimiter)
	//{
	//	int startIndex = text.IndexOf(startDelimiter, StringComparison.OrdinalIgnoreCase);
	//	if (startIndex == -1) return string.Empty;

	//	startIndex += startDelimiter.Length;
	//	int endIndex = text.IndexOf(endDelimiter, startIndex, StringComparison.OrdinalIgnoreCase);
	//	if (endIndex == -1) endIndex = text.Length;

	//	return text.Substring(startIndex, endIndex - startIndex).Trim();
	//}

}


public class ReportData
{
	public string CompanyName { get; set; }
	public string PatientName { get; set; }
	public string PatientNumber { get; set; }
	public DateTime ExamDate { get; set; }
	public DateTime DateOfBirth { get; set; }
	public string Conclusion { get; set; }
	public string Exam { get; set; }
}
