namespace FileManagement
{
	public class FileContents
	{
		public string Text { get; private set; }
		public bool HasImages { get; private set; }
		public FileContents(string txt, bool imgs) { Text = txt; HasImages = imgs; }
	}
	public interface IFileManager
	{
		string[] FileExtention { get; }
		FileContents ReadAllText(string filename);
    }
}
