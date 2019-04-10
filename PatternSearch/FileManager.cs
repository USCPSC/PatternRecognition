using FileManagement;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Reflection;

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
