using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using FileManager;

namespace PatternSearch
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
						sb = (i == 0 && sb.Length == 0)? sb.Append(fm.FileExtention[i]): sb.AppendFormat($", {fm.FileExtention[i]}");
					}
				}
			}
			return sb.ToString();
		}
		public string[] SupportedFiles(string[] files)
		{
			string[] results = null;
			if (files.Length > 0)
			{
				StringCollection supportedFiles = new StringCollection();
				foreach (var fm in FileReaders)
					for (int i = 0; i < fm.FileExtention.Length; i++)
						for (int f = 0; f < files.Length; f++)
							if (string.Compare(fm.FileExtention[i], Path.GetExtension(files[f]), true) == 0)
								supportedFiles.Add(files[f]);
				if ( supportedFiles.Count > 0)
				{
					results = new string[supportedFiles.Count];
					supportedFiles.CopyTo(results, 0);
				}
			}
			return results;
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
