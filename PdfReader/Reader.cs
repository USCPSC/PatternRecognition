using FileManagement;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using System.ComponentModel.Composition;
using System.Text;
using System.IO;
using System.Collections.Generic;
using System;

namespace PdfFileReader
{
	/// <summary>
	/// Implement IFileManager for PDF files
	/// </summary>
	[Export(typeof(IFileManager))]
	public class Reader : IFileManager
	{
		public string[] FileExtention => new string[] { ".pdf" };

		public FileContents ReadAllText(string filename)
		{
			bool hasImages = false;
			var text = new StringBuilder();
			using (var reader = new PdfReader(filename))
			{
				for (int page = 1; page <= reader.NumberOfPages; page++)
				{
					ITextExtractionStrategy strategy = new SimpleTextExtractionStrategy();
					string currentText = PdfTextExtractor.GetTextFromPage(reader, page, strategy);

					currentText = Encoding.UTF8.GetString(Encoding.Convert(Encoding.Default, Encoding.UTF8, Encoding.Default.GetBytes(currentText)));
					text.Append(currentText);

					if (hasImages == false)
						hasImages = PdfUtils.PdfImageExtractor.PageContainsImages(reader, page);
				}
			}

			return new FileContents(text.ToString(), hasImages);
		}
	}

}

