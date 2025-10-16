//using System.IO;
//using Data.ViewModels;

//namespace Service.Helpers
//{
//    public static class PdfReportGenerator
//    {
//        public static byte[] GenerateReportPdf(ReportViewModel report)
//        {
//            using var stream = new MemoryStream();
//            var doc = new Document(PageSize.A4, 50, 50, 60, 60);
//            PdfWriter.GetInstance(doc, stream);
//            doc.Open();

//            // === COLORS & STYLES ===
//            var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18, BaseColor.BLACK);
//            var headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12, BaseColor.DARK_GRAY);
//            var normalFont = FontFactory.GetFont(FontFactory.HELVETICA, 11, BaseColor.BLACK);
//            var sectionTitleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 13, new BaseColor(243, 116, 56)); // orange tone

//            // === HEADER ===
//            var headerTable = new PdfPTable(1) { WidthPercentage = 100 };
//            headerTable.DefaultCell.Border = Rectangle.NO_BORDER;

//            headerTable.AddCell(new PdfPCell(new Phrase("GEM DIAGNOSTIC CENTER", titleFont))
//            {
//                Border = Rectangle.NO_BORDER,
//                HorizontalAlignment = Element.ALIGN_CENTER,
//                PaddingBottom = 5
//            });
//            headerTable.AddCell(new PdfPCell(new Phrase("# 18 Ridge Road, Behind Stock Exchange GRA, Onitsha", normalFont))
//            {
//                Border = Rectangle.NO_BORDER,
//                HorizontalAlignment = Element.ALIGN_CENTER
//            });
//            headerTable.AddCell(new PdfPCell(new Phrase("Phone: +2348184901972 | Email: contactmri@gemdiagnosticcenter.com", normalFont))
//            {
//                Border = Rectangle.NO_BORDER,
//                HorizontalAlignment = Element.ALIGN_CENTER,
//                PaddingBottom = 15
//            });
//            doc.Add(headerTable);

//            // Divider line
//            var line = new LineSeparator(1f, 100f, BaseColor.LIGHT_GRAY, Element.ALIGN_CENTER, -2);
//            doc.Add(new Chunk(line));
//            doc.Add(Chunk.NEWLINE);

//            // === PATIENT INFO TABLE ===
//            var infoTable = new PdfPTable(2) { WidthPercentage = 100 };
//            infoTable.SetWidths(new float[] { 1f, 2f });

//            void AddRow(string label, string value)
//            {
//                infoTable.AddCell(new PdfPCell(new Phrase(label, headerFont)) { Border = Rectangle.NO_BORDER });
//                infoTable.AddCell(new PdfPCell(new Phrase(value ?? "-", normalFont)) { Border = Rectangle.NO_BORDER });
//            }

//            AddRow("Patient Name:", report.PatientName);
//            AddRow("Patient ID:", report.PatientID);
//            AddRow("Sex:", report.Sex);
//            AddRow("Age:", report.Age?.ToString());
//            AddRow("Date of Birth:", report.DateOfBirth?.ToString("MMMM dd, yyyy"));
//            AddRow("Study Date:", report.StudyDate?.ToString());
//            AddRow("Institution:", report.Institution);
//            AddRow("Exam:", report.Exam);

//            doc.Add(infoTable);
//            doc.Add(Chunk.NEWLINE);

//            // === SECTIONS ===
//            void AddSection(string title, string content)
//            {
//                if (!string.IsNullOrEmpty(content))
//                {
//                    doc.Add(new Paragraph(title, sectionTitleFont));
//                    doc.Add(new Paragraph(content, normalFont));
//                    doc.Add(Chunk.NEWLINE);
//                }
//            }

//            AddSection("Clinical Information", report.ClinicalInformation);
//            AddSection("Findings", report.Findings);
//            AddSection("Conclusion", report.Conclusion);

//            // === FOOTER ===
//            doc.Add(new Chunk(line));
//            doc.Add(Chunk.NEWLINE);
//            doc.Add(new Paragraph("DR OBIEJE K.G (FWACS, FMCR)", normalFont));
//            doc.Add(new Paragraph("Consultant Radiologist", normalFont));
//            doc.Add(Chunk.NEWLINE);
//            doc.Add(new Paragraph("GEM DIAGNOSTIC CENTER", headerFont));
//            doc.Add(new Paragraph("# 18 Ridge Road, GRA, Onitsha | Phone: +2348184901972", normalFont));
//            doc.Add(new Paragraph("Email: contactmri@gemdiagnosticcenter.com", normalFont));

//            doc.Close();
//            return stream.ToArray();
//        }
//    }
//}
