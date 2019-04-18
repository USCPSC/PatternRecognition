using FileManagement;
using System.ComponentModel.Composition;
using System.IO;
using System.Text;

namespace TextFileReader
{
	/// <summary>
	/// Implement file manager for text files
	/// </summary>
	[Export(typeof(IFileManager))]
	public class Reader : IFileManager
	{
		public string[] FileExtention => new string[] { ".txt", ".csv" };

		public FileContents ReadAllText(string filename, bool imageScan) => new FileContents(File.ReadAllText(filename, Encoding.ASCII), imageScan? "None": "N/A");
	}
}
