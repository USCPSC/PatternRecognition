using FileManagement;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using System.ComponentModel.Composition;
using System.Text;

namespace PdfFileReader
{
	/// <summary>
	/// Implement IFileManager for PDF files
	/// </summary>
	[Export(typeof(IFileManager))]
	public class Reader : IFileManager
	{
		public string FileExtention => ".pdf";

		public string ReadAllText(string filename)
		{
			var text = new StringBuilder();
			using (var reader = new PdfReader(filename))
			{
				for (int page = 1; page <= reader.NumberOfPages; page++)
				{
					ITextExtractionStrategy strategy = new SimpleTextExtractionStrategy();
					string currentText = PdfTextExtractor.GetTextFromPage(reader, page, strategy);

					currentText = Encoding.UTF8.GetString(Encoding.Convert(Encoding.Default, Encoding.UTF8, Encoding.Default.GetBytes(currentText)));
					text.Append(currentText);
				}
			}

			return text.ToString();
		}
	}
}
