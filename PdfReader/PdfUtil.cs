using System.Collections.Generic;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;

namespace PdfUtils
{
	public static class PdfImageChecker
	{
		public static Dictionary<string, int> PageContainsImages(PdfReader reader, int pageNumber)
		{
			var listener = new ImageCheckListener();
			var parser = new PdfReaderContentParser(reader);
			parser.ProcessContent(pageNumber, listener);
			return listener.Images;
		}

		/// <summary>Checks whether a specified page of a PDF file contains images.</summary>
		/// <returns>True if the page contains at least one image; false otherwise.</returns>
		public static Dictionary<string, int> PageContainsImages(string filename, int pageNumber)
		{
			using (var reader = new PdfReader(filename))
			{
				var parser = new PdfReaderContentParser(reader);
				ImageCheckListener listener = null;
				parser.ProcessContent(pageNumber, (listener = new ImageCheckListener()));
				return listener.Images;
			}
		}
	}

	internal class ImageCheckListener : IRenderListener
	{
		private Dictionary<string, int> images = new Dictionary<string, int>();

		public Dictionary<string, int> Images
		{
			get { return images; }
		}

		public void BeginTextBlock() { }

		public void EndTextBlock() { }

		public void RenderImage(ImageRenderInfo renderInfo)
		{
			PdfImageObject image = renderInfo.GetImage();
			string fileType = image.GetFileType();
			if (images.ContainsKey(fileType))
				images[fileType] = images[fileType] + 1;
			else
				images.Add(fileType, 1);
		}

		public void RenderText(TextRenderInfo renderInfo) { }
	}
}
