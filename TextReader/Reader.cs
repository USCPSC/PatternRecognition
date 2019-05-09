using System.ComponentModel.Composition;
using System.IO;
using System.Text;
using FileManager;

namespace TextFileReader
{
	/// <summary>
	/// Implement file manager for text files
	/// </summary>
	[Export(typeof(IFileReader))]
	public class Reader : IFileReader
	{
		public string[] FileExtention => new string[] { ".txt", ".csv" };

		public FileContents ReadAllText(string filename, bool imageScan) => new FileContents(File.ReadAllText(filename, Encoding.ASCII), imageScan? FileContents.None: FileContents.NotApplicable);
	}
}
