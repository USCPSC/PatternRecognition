using FileManagement;
using System.ComponentModel.Composition;
using Xceed.Words.NET;

namespace DocxFileReader
{
	[Export(typeof(IFileManager))]
	public class Reader : IFileManager
	{
		public string[] FileExtention => new string[] { ".docx" };

		public FileContents ReadAllText(string filename)
		{
			using (DocX document = DocX.Load(filename))
			{
				return new FileContents(document.Text, false);
			}
		}
	}
}
