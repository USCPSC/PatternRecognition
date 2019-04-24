using FileManagement;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using System.ComponentModel.Composition;
using System.Text;
using System.Collections.Generic;

namespace PdfFileReader
{
	/// <summary>
	/// Implement IFileManager for PDF files
	/// </summary>
	[Export(typeof(IFileManager))]
	public class Reader : IFileManager
	{
		public string[] FileExtention => new string[] { ".pdf" };

		public FileContents ReadAllText(string filename, bool imageScan)
		{
			Dictionary<string, int> docimages =  new Dictionary<string, int>();
			Dictionary<string, int> pageimages = new Dictionary<string, int>();
			var text = new StringBuilder();
			using (var reader = new PdfReader(filename))
			{
				for (int page = 1; page <= reader.NumberOfPages; page++)
				{
					ITextExtractionStrategy strategy = new SimpleTextExtractionStrategy();
					string currentText = PdfTextExtractor.GetTextFromPage(reader, page, strategy);

					currentText = Encoding.UTF8.GetString(Encoding.Convert(Encoding.Default, Encoding.UTF8, Encoding.Default.GetBytes(currentText)));
					text.Append(currentText);

					if (imageScan)
					{
						pageimages = PdfUtils.PdfImageChecker.PageContainsImages(reader, page);
						foreach (var k in pageimages.Keys)
						{
							if (docimages.ContainsKey(k))
								docimages[k] += pageimages[k];
							else
								docimages.Add(k, pageimages[k]);
						}
					}
				}
			}
			StringBuilder sb = new StringBuilder();
			if (imageScan == false)
				sb.Append(FileContents.NotApplicable);
			else if (docimages.Count == 0)
				sb.Append(FileContents.None);
			else
			{
				foreach (var k in docimages.Keys)
					sb.AppendFormat($"{k}: {docimages[k]} ");
			}
			return new FileContents(text.ToString(), sb.ToString());
		}
	}

}

