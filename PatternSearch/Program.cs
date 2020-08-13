using CommandLine;
using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PatternSearch
{
	class Program
	{
		/// <summary>
		/// Type of output to generate
		/// </summary>
		internal enum OutputLevel
		{
			B, // Basic
			M, // Moderate
			V  // Verbose
		}

		/// <summary>
		/// Command line options
		/// </summary>
		internal class Options
		{
			[Option('d', "directory", Required = false, HelpText = "Input directory to be processed.")]
			public string Directory { get; set; }

			[Option('f', "file", Required = false, HelpText = "Input file to be processed.")]
			public string File { get; set; }

			[Option('o', "outfile", Required = true, HelpText = "Output file to store results.")]
			public string OutFile { get; set; }

			[Option('e', "errfile", Required = false, HelpText = "Output file to store errors.")]
			public string ErrFile { get; set; }

			[Option('r', "recursive", Required = false, HelpText = "Recursively process directory.")]
			public bool Recursive { get; set; }

			[Option('c', "csv", Required = false, HelpText = "Output the results in a CSV format.")]
			public bool CSVOuput { get; set; }

			[Option('v', "verbosity", Required = false, HelpText = "Verbosity level: B=Basic, M=Moderate, V=Verbose.")]
			public OutputLevel Verbosity { get; set; }

			[Option('i', "imagescan", Required = false, HelpText = "Scan file to determine if images exists.")]
			public bool ImageScan { get; set; }
		}

		static public Parsed<Options> cmdline;
		static public SearchManager smgr = new SearchManager();
		static public Scanner.ScanEngine scanner = new Scanner.ScanEngine();
		static public int timeoutValue;

		/// <summary>
		/// Main processor
		/// </summary>
		/// <param name="args"></param>
		static void Main(string[] args)
		{
			// Parse command line
			cmdline = (Parsed<Options>)Parser.Default.ParseArguments<Options>(args)
						.WithParsed<Options>(o =>
						{
							bool dir = string.IsNullOrEmpty(o.Directory);
							bool fil = string.IsNullOrEmpty(o.File);
							if (dir == true && fil == true)
							{
								Console.Error.WriteLine("You must provide either a directory or file to process");
								Console.WriteLine("Press any key to continue...");
								Console.ReadKey();
								Environment.Exit(-1);
							}
							// Make sure it is a valid directory before we do anything
							if (dir == false && Directory.Exists(o.Directory) == false)
							{
								Console.Error.WriteLine($"Invalid directory: '{o.Directory}'");
								Console.WriteLine("Press any key to continue...");
								Console.ReadKey();
								Environment.Exit(-1);
							}
							if (fil == false && File.Exists(o.File) == false)
							{
								Console.Error.WriteLine($"Invalid file: '{o.File}'");
								Console.WriteLine("Press any key to continue...");
								Console.ReadKey();
								Environment.Exit(-1);
							}
						})
						 .WithNotParsed<Options>(e =>
						 {
							 Console.WriteLine("Press any key to continue...");
							 Console.ReadKey();
							 Environment.Exit(-2);
						 });

			var outFile = cmdline.Value.OutFile;
			try
			{
				if (File.Exists(outFile))
					File.Delete(outFile);
			}
			catch (Exception)
			{
				outFile = Path.ChangeExtension(outFile, DateTime.Now.Ticks + ".csv");
			}
			try
			{
				using (var fs = File.Create(outFile, 1, FileOptions.DeleteOnClose)) { }
			}
			catch (Exception e)
			{
				Console.Error.WriteLine($"Unable to create file {outFile}: '{e.Message}'");
				Console.WriteLine("Press any key to continue...");
				Console.ReadKey();
				Environment.Exit(-2);
			}
			cmdline.Value.OutFile = outFile;

			if (string.IsNullOrEmpty(cmdline.Value.ErrFile))
				cmdline.Value.ErrFile = Path.ChangeExtension(cmdline.Value.OutFile, ".err");

			string errFile = cmdline.Value.ErrFile;
			try
			{
				if (File.Exists(errFile))
					File.Delete(errFile);
			}
			catch (Exception)
			{
				errFile = Path.ChangeExtension(cmdline.Value.ErrFile, DateTime.Now.Ticks + ".err");
			}
			try
			{
				using (var fs = File.Create(errFile, 1, FileOptions.DeleteOnClose)) { }
			}
			catch (Exception e)
			{
				Console.Error.WriteLine($"Unable to create file {errFile}: '{e.Message}'");
				Console.WriteLine("Press any key to continue...");
				Console.ReadKey();
				Environment.Exit(-3);
			}
			cmdline.Value.ErrFile = errFile;

			// Load the FileManagers
			smgr.ImportFileReaders();
			if (smgr.FileReaders.Count() == 0)
			{
				File.AppendAllText(cmdline.Value.ErrFile, "No file readers found\n");
				Environment.Exit(-3);
			}

			// Load the Scan engine and the patterns
			if (scanner.LoadPatterns() == 0)
			{
				File.AppendAllText(cmdline.Value.ErrFile, "No patterns found\n");
				Environment.Exit(-4);
			}
			timeoutValue = int.Parse(ConfigurationManager.AppSettings["Timeout"] ?? "60");
			var starttime = DateTime.Now;
			var processedfiles = 0;

			// Print header
			PrintHeader(starttime);

			//Process
			if (string.IsNullOrEmpty(cmdline.Value.Directory) == false)
			{
				Console.WriteLine($"Scanning {cmdline.Value.Directory} for files that contain sensitive information. Results will be stored in {cmdline.Value.OutFile}...");
				processedfiles = ProcessDir(cmdline.Value.Directory, cmdline.Value.Recursive);
			}
			if (string.IsNullOrEmpty(cmdline.Value.File) == false)
			{
				processedfiles += ProcessFiles(new string[] { cmdline.Value.File });
			}

			if (processedfiles > 0)
				PrintFooter(starttime, processedfiles, scanner.GetPatternNames());
		}
		private static int ProcessDir(string dir, bool recurse = true)
		{
			string[] files = Directory.GetFiles(dir);

			int filesProcessed = ProcessFiles(files);

			if (recurse)
			{
				// Recurse into subdirectories of this directory.
				string[] subdirectoryEntries = Directory.GetDirectories(dir);
				foreach (string subdirectory in subdirectoryEntries)
					filesProcessed += ProcessDir(subdirectory);
			}
			return filesProcessed;
		}
		private static int ProcessFiles(string[] files)
		{
			// Process files
			var processedfiles = 0;
			foreach (var file in files)
			{
				// If there is a file processor for a give file extension, process the file..
				foreach (var fm in from fm in smgr.FileReaders where smgr.SupportFileExtension(fm, Path.GetExtension(file)) select fm)
				{
					Thread t = new Thread(() =>
					{
						try
						{
							// Read the text
							var fc = fm.ReadAllText(file, cmdline.Value.ImageScan);

							// Scan the text for patterns
							scanner.Scan(fc.Text);

							// Output start
							PrintProcessingStart(file, fc);

							// If verbose enabled, print more details for each match
							if (cmdline.Value.Verbosity == OutputLevel.V)
							{
								foreach (var match in scanner.PatternsFound.OrderBy(i => i.Index))
									PrintMatch(match);
							}

							// Output Results
							PrintProcessingResults(file, fc);

							// Only count if we have a file processor
							++processedfiles;
						}
						catch (Exception e)
						{
							File.AppendAllText(cmdline.Value.ErrFile, $"An error occured while processing: {file} => {e.Message}\n");
						}
					});
					t.Start();
					if (!t.Join(TimeSpan.FromSeconds(timeoutValue)))
					{
						t.Abort();
						File.AppendAllText(cmdline.Value.ErrFile, $"Unable to finish processing: {file} in the time allotted {timeoutValue}\n");
					}

				}
			}
			return processedfiles;
		}
		private static void PrintHeader(DateTime starttime)
		{
			if (cmdline.Value.CSVOuput == false)
			{
				File.AppendAllText(cmdline.Value.OutFile, "*************************************************************************");
				File.AppendAllText(cmdline.Value.OutFile, $"({starttime}) Processing files with the following parameters:");
				File.AppendAllText(cmdline.Value.OutFile, $"Recursive='{cmdline.Value.Recursive}' Directory='{cmdline.Value.Directory}' Verbosity='{cmdline.Value.Verbosity}'");
				File.AppendAllText(cmdline.Value.OutFile, $"In the following file types: '{smgr.GetFileExtentions()}'");
				File.AppendAllText(cmdline.Value.OutFile, "*************************************************************************\n");
			}
			else
			{
				switch (cmdline.Value.Verbosity)
				{
					case OutputLevel.B:
						if (cmdline.Value.ImageScan == true)
							File.AppendAllText(cmdline.Value.OutFile, "File Name,Text Size,Possible Match Count,Has Images\n");
						else
							File.AppendAllText(cmdline.Value.OutFile, "File Name,Text Size,Possible Match Count\n");
						break;
					case OutputLevel.M:
						if (cmdline.Value.ImageScan == true)
							File.AppendAllText(cmdline.Value.OutFile, "File Name,Text Size,Pattern(s) Found,Total Found,Has Images\n");
						else
							File.AppendAllText(cmdline.Value.OutFile, "File Name,Text Size,Pattern(s) Found,Total Found\n");
						break;
					case OutputLevel.V:
						if (cmdline.Value.ImageScan == true)
							File.AppendAllText(cmdline.Value.OutFile, "File Name,Text Size,Has Images,Patterns Found,Pattern Name,Pattern\n");
						else
							File.AppendAllText(cmdline.Value.OutFile, "File Name,Text Size,Patterns Found,Pattern Name,Pattern\n");
						break;
				}
			}
		}

		private static void PrintProcessingStart(string file, global::FileManager.FileContents fc)
		{
			if (cmdline.Value.Verbosity == OutputLevel.V)
			{
				string fileprefix = ConfigurationManager.AppSettings["FilePrefix"] ?? "";
				if (cmdline.Value.CSVOuput == false)
					File.AppendAllText(cmdline.Value.OutFile, $"**********Processing file {file} ...**********\n");
				else if (cmdline.Value.ImageScan == true)
					File.AppendAllText(cmdline.Value.OutFile, $"{fileprefix}{file},{fc.Text?.Length},{fc.HasImages},{scanner.PatternsFound.Count}\n");
				else
					File.AppendAllText(cmdline.Value.OutFile, $"{fileprefix}{file},{fc.Text?.Length},{scanner.PatternsFound.Count}\n");
			}
		}

		private static void PrintMatch(Scanner.PatternFound match)
		{
			// Comma offset variable to align output in CSV output
			var commaOffset = cmdline.Value.ImageScan ? ",,,," : ",,,";

			if (cmdline.Value.CSVOuput == false)
				File.AppendAllText(cmdline.Value.OutFile, $"{match.Name} was found at {match.Index} with a value of {match.Value}\n");
			else
			{
				// Replace comma with a period for comma separated output
				string str = match.Value.Replace(',', '.');

				// Quote numeric value so excel won't try to convert it to a number
				if (str.All(char.IsDigit))
					str = string.Format($"'{str}'");

				File.AppendAllText(cmdline.Value.OutFile, $"{commaOffset}{match.Name},{str}\n");
			}
		}

		private static void PrintProcessingResults(string file, global::FileManager.FileContents fc)
		{
			string fileprefix = ConfigurationManager.AppSettings["FilePrefix"] ?? "";
			switch (cmdline.Value.Verbosity)
			{
				case OutputLevel.B:
					if (cmdline.Value.CSVOuput == false)
						File.AppendAllText(cmdline.Value.OutFile, $"Number of possible patterns found: {scanner.PatternsFound.Count} in {file}\n");
					else if (cmdline.Value.ImageScan == true)
						File.AppendAllText(cmdline.Value.OutFile, $"{fileprefix}{file},{fc.Text?.Length},{scanner.PatternsFound.Count},{fc.HasImages}\n");
					else
						File.AppendAllText(cmdline.Value.OutFile, $"{fileprefix}{file},{fc.Text?.Length},{scanner.PatternsFound.Count}\n");
					break;
				case OutputLevel.M:
					if (cmdline.Value.CSVOuput == false)
						File.AppendAllText(cmdline.Value.OutFile, $"Found {scanner.PatternsFoundAsString} in {file}\n");
					else if (cmdline.Value.ImageScan == true)
						File.AppendAllText(cmdline.Value.OutFile, $"{fileprefix}{file},{fc.Text?.Length},{scanner.PatternsFoundAsString},{scanner.PatternsFound.Count},{fc.HasImages}\n");
					else
						File.AppendAllText(cmdline.Value.OutFile, $"{fileprefix}{file},{fc.Text?.Length},{scanner.PatternsFoundAsString},{scanner.PatternsFound.Count}\n");
					break;
				case OutputLevel.V:
					if (cmdline.Value.CSVOuput == false)
						File.AppendAllText(cmdline.Value.OutFile, $"Number of possible patterns found: {scanner.PatternsFound.Count}\n");
					break;
			}
		}

		private static void PrintFooter(DateTime starttime, int processedfiles, string patterns)
		{
			var procduration = DateTime.Now - starttime;
			var proctime = (procduration.TotalSeconds > 60) ? procduration.TotalMinutes : procduration.TotalSeconds;
			string increment = (procduration.TotalSeconds > 60) ? "minutes" : "seconds";

			if (cmdline.Value.CSVOuput == false)
			{
				File.AppendAllText(cmdline.Value.OutFile, "*************************************************************************\n");
				File.AppendAllText(cmdline.Value.OutFile, $"({DateTime.Now}) Finished processing files:\n");
				File.AppendAllText(cmdline.Value.OutFile, $"Files Processed='{processedfiles}' Processing Time ({increment}) ='{proctime}'\n");
				File.AppendAllText(cmdline.Value.OutFile, $"Searched for the following patterns: '{patterns}'\n");
				File.AppendAllText(cmdline.Value.OutFile, "*************************************************************************\n");
			}
			else
			{
				File.AppendAllText(cmdline.Value.OutFile, $"\nScan Started at: {starttime}, Finished at: {DateTime.Now}\n");
				File.AppendAllText(cmdline.Value.OutFile, $"Files Processed='{processedfiles}', Processing Time ({increment}) ='{proctime}'\n");
				File.AppendAllText(cmdline.Value.OutFile, $"Patterns searched: {patterns}\n");
			}
		}
	}
}
