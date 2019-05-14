using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Reflection;
using System.Text;
using FileManager;

namespace PatternSearchUI
{
	/// <summary>
	/// File Manager
	/// </summary>
	public class SearchManager
	{
		[ImportMany(typeof(IFileReader))]
		public IEnumerable<IFileReader> FileReaders { get; private set; }

		/// <summary>
		/// Get all the file extentions
		/// </summary>
		/// <returns>Comma separarted collection of file extentions</returns>
		public string GetFileExtentions()
		{
			var sb = new StringBuilder();
			if (FileReaders != null)
			{
				foreach (var fm in FileReaders)
				{
					for (int i = 0; i < fm.FileExtention.Length; i++)
					{
						if (i == 0 && sb.Length == 0)
							sb.Append(fm.FileExtention[i]);
						else
							sb.AppendFormat($", {fm.FileExtention[i]}");
					}
				}
			}
			return sb.ToString();
		}

		public bool SupportFileExtension(IFileReader fm, string extension)
		{
			for (int i = 0; i < fm.FileExtention.Length; i++)
				if (string.Compare(fm.FileExtention[i], extension, true) == 0)
					return true;
			return false;
		}
		/// <summary>
		/// Import the MEF-based file managers
		/// </summary>
		public void ImportFileReaders()
		{
			//An aggregate catalog that combines multiple catalogs
			var catalog = new AggregateCatalog();
			//Adds all the parts found in all assemblies in 
			//the same directory as the executing program
			catalog.Catalogs.Add(new DirectoryCatalog(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)));

			//Create the CompositionContainer with the parts in the catalog
			CompositionContainer container = new CompositionContainer(catalog);

			//Fill the imports of this object
			container.ComposeParts(this);
		}
	}
}
