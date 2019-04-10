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
		public string FileExtention => ".txt";

		public string ReadAllText(string filename)
		{
			return File.ReadAllText(filename, Encoding.ASCII);
		}
	}
}
