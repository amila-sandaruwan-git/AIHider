using System;
using System.IO;
using System.Text;

namespace AIHider
{
    public static class FileLoader
    {
        // Max characters sent to the AI (keeps token usage reasonable)
        private const int MaxChars = 20000;

        public static string LoadText(string path)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException("File not found", path);

            string ext = Path.GetExtension(path).ToLowerInvariant();

            string text = ext switch
            {
                ".txt" => File.ReadAllText(path),
                ".md" => File.ReadAllText(path),
                ".pdf" => ReadPdf(path),
                ".docx" => ReadDocx(path),
                _ => throw new NotSupportedException($"Unsupported file type: {ext}")
            };

            // Trim to avoid blowing the token limit
            if (text.Length > MaxChars)
            {
                text = text.Substring(0, MaxChars) +
                       "\n\n[...truncated for length...]";
            }

            return text;
        }

        private static string ReadPdf(string path)
        {
            var sb = new StringBuilder();
            using var doc = UglyToad.PdfPig.PdfDocument.Open(path);
            foreach (var page in doc.GetPages())
            {
                sb.AppendLine(page.Text);
                sb.AppendLine();
            }
            return sb.ToString();
        }

        private static string ReadDocx(string path)
        {
            var sb = new StringBuilder();
            using var doc = DocumentFormat.OpenXml.Packaging.WordprocessingDocument.Open(path, false);
            var body = doc.MainDocumentPart?.Document?.Body;
            if (body != null)
            {
                foreach (var para in body.Descendants<DocumentFormat.OpenXml.Wordprocessing.Paragraph>())
                {
                    sb.AppendLine(para.InnerText);
                }
            }
            return sb.ToString();
        }
    }
}