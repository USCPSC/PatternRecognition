namespace FileManagement
{
	public class FileContents
	{
		public string Text { get; private set; }
		public string HasImages { get; private set; }
		public FileContents(string txt, string imgs) { Text = txt; HasImages = imgs; }
	}
	public interface IFileManager
	{
		string[] FileExtention { get; }
		FileContents ReadAllText(string filename, bool imageScan);
    }
}
