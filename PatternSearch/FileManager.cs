using FileManagement;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Reflection;
using System.Text;

namespace PatternSearch
{
	/// <summary>
	/// File Manager
	/// </summary>
	public class FileManager
	{
		[ImportMany(typeof(IFileManager))]
		public IEnumerable<IFileManager> FileManagers { get; private set; }

		/// <summary>
		/// Get all the file extentions
		/// </summary>
		/// <returns>Comma separarted collection of file extentions</returns>
		public string GetFileExtentions()
		{
			var sb = new StringBuilder();
			if (FileManagers != null)
			{
				foreach(var fm in FileManagers)
				{
					if (sb.Length == 0)
						sb.Append(fm.FileExtention);
					else
						sb.AppendFormat($", {fm.FileExtention}");
				}
			}
			return sb.ToString();
		}
		/// <summary>
		/// Import the MEF-based file managers
		/// </summary>
		public void ImportFileManagers()
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
