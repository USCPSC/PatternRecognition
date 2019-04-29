namespace FileManager
{
	public class FileContents
	{
		// Constants for uniform output of HasImages property
		public const string NotApplicable = "N/A";
		public const string None = "None";

		// Contents of file
		public string Text { get; private set; }
		
		// Image extentions with count
		public string HasImages { get; private set; }

		// Constructor
		public FileContents(string txt, string imgs) { Text = txt; HasImages = imgs; }
	}
	public interface IFileReader
	{
		// File Extentions supported
		string[] FileExtention { get; }

		// Read a file and return its contents
		FileContents ReadAllText(string filename, bool imageScan);
    }
}
