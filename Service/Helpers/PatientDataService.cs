using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Service.Helpers
{
	//public class PatientDataService
	//{
	//	public static List<Report> ProcessPatientDataInDirectory(string targetDirectory)
	//	{
	//		var patientDatas = new List<Report>();
	//		string[] fileEntries;

	//		// Ensure the path doesn't contain invalid characters
	//		string sanitizedPath = targetDirectory.Replace(":", "_").Replace(" ", "_");

	//		if (sanitizedPath.Length > 260)  // Check if path exceeds Windows max length
	//		{
	//			sanitizedPath = @"\\?\" + sanitizedPath;  // Prefix for long paths
	//		}

	//		if (!Directory.Exists(targetDirectory))
	//		{
	//			Console.WriteLine($"Directory does not exist or is invalid: {sanitizedPath}");
	//			return new List<Report>();
	//		}

	//		fileEntries = Directory.GetFiles(sanitizedPath);


	//		foreach (string fileName in fileEntries)
	//		{
	//			string normalizedFilePath = fileName.Replace("\\", "/");
	//			if (normalizedFilePath.Contains(":/") && !normalizedFilePath.Contains(":\\"))
	//			{
	//				normalizedFilePath = normalizedFilePath.Replace(":/", ":\\");
	//			}

	//			var report = ProcessPatientData(normalizedFilePath);
	//			if (report != null)
	//			{
	//				patientDatas.Add(report);
	//			}
	//		}
	//		return patientDatas;
	//	}


	//	public static Report ProcessPatientData(string path)
	//	{
	//		if (string.IsNullOrEmpty(path) || !File.Exists(path))
	//			return null;

	//		Application wordApp = new Application();
	//		Document doc = null;
	//		try
	//		{
	//			object filename = path;
	//			object readOnly = false;
	//			object isVisible = false;
	//			object missing = System.Reflection.Missing.Value;

	//			// Open the Word document
	//			doc = wordApp.Documents.Open(ref filename, ref missing, ref readOnly, ref missing, ref missing, ref missing,
	//										  ref missing, ref missing, ref missing, ref missing, ref missing,
	//										  ref isVisible, ref missing, ref missing, ref missing);

	//			if (doc != null && doc.Content.Text != null)
	//			{
	//				List<string> lines = doc.Content.Text.Replace("\a", "").Split('\r').ToList();
	//				List<string> nonEmptyLines = lines.Where(s => !string.IsNullOrEmpty(s)).ToList();

	//				var patientData = new Report();
	//				patientData = ExtractPatientData(nonEmptyLines, patientData);

	//				return patientData;
	//			}
	//		}
	//		catch (Exception ex)
	//		{
	//			// Handle any specific exceptions if needed
	//			Console.WriteLine($"Error processing file {path}: {ex.Message}");
	//		}
	//		finally
	//		{
	//			// Clean up resources
	//			doc?.Close();
	//			wordApp.Quit();
	//		}

	//		return null;
	//	}

	//	private static Report ExtractPatientData(List<string> nonEmptyLines, Report patientData)
	//	{
	//		//patientData.SurName = ExtractDataByKeyword(nonEmptyLines, "SURNAME");
	//		//patientData.LastName = ExtractDataByKeyword(nonEmptyLines, "FORENAMES");
	//		patientData.TestDetails = ExtractTestDetails(nonEmptyLines);
	//		patientData.StudyNote = ExtractDataByKeyword(nonEmptyLines, "CLINICAL SUMMARY");
	//		patientData.Conclusion = ExtractDataByKeyword(nonEmptyLines, "CONCLUSION");
	//		//patientData.DOB = ExtractDate(nonEmptyLines, "D.O.B");
	//		patientData.DateCollected = ExtractDate(nonEmptyLines, "DATE");
	//		//patientData.Sex = ExtractDataByKeyword(nonEmptyLines, "SEX");
	//		//patientData.Address = ExtractDataByKeyword(nonEmptyLines, "ADDRESS");
	//		//patientData.PatientNo = ExtractDataByKeyword(nonEmptyLines, "MHN #");

	//		return patientData;
	//	}

	//	private static string ExtractDataByKeyword(List<string> lines, string keyword)
	//	{
	//		int index = lines.FindIndex(s => s.Equals(keyword, StringComparison.OrdinalIgnoreCase));
	//		if (index != -1 && index + 1 < lines.Count)
	//		{
	//			return lines[index + 1];
	//		}
	//		return string.Empty;
	//	}

	//	private static string ExtractTestDetails(List<string> lines)
	//	{
	//		int testDetailsIndex = lines.FindIndex(s => s.Equals("CLINICAL SUMMARY", StringComparison.OrdinalIgnoreCase));
	//		if (testDetailsIndex != -1 && testDetailsIndex - 1 < lines.Count)
	//		{
	//			var testDetails = lines[testDetailsIndex - 1];
	//			if (!testDetails.Contains("am") && !testDetails.Contains("pm") && !testDetails.Contains(":"))
	//			{
	//				return testDetails;
	//			}
	//		}
	//		return string.Empty;
	//	}

	//	private static DateTime ExtractDate(List<string> lines, string keyword)
	//	{
	//		int dateIndex = lines.FindIndex(s => s.Equals(keyword, StringComparison.OrdinalIgnoreCase));
	//		if (dateIndex != -1 && dateIndex + 1 < lines.Count)
	//		{
	//			DateTime dateTime;
	//			return DateTime.TryParse(lines[dateIndex + 1], out dateTime) ? dateTime : DateTime.MinValue;
	//		}
	//		return DateTime.MinValue;
	//	}
	//}
}
