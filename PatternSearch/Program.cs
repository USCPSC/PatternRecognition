using CommandLine;
using System;
using System.Collections.Concurrent;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
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
			[Option('r', "recursive", Required = false, HelpText = "Recursively process directory")]
			public bool Recursive { get; set; }

			[Option('c', "csv", Required = false, HelpText = "Output the results in a CSV format")]
			public bool CSVOuput { get; set; }

			[Option('v', "verbose", Required = false, HelpText = "Verbosity level: B=Basic, M=Moderate, V=Verbose")]
			public OutputLevel Verbosity { get; set; }

			[Option('d', "directory", Required = true, HelpText = "Input directory to be processed.")]
			public string Directory { get; set; }

			[Option('i', "imagescan", Required = false, HelpText = "Scan file to determine if images exists.")]
			public bool ImageScan { get; set; }
		}

		/// <summary>
		/// Main processor
		/// </summary>
		/// <param name="args"></param>
		static void Main(string[] args)
		{
			// Parse command line
			var cmdline = (Parsed<Options>)Parser.Default.ParseArguments<Options>(args)
						.WithParsed<Options>(o =>
						 {
							 // Make sure it is a valid directory before we do anything
							 if (Directory.Exists(o.Directory) == false)
							 {
								 Console.Error.WriteLine($"Invalid directory: '{o.Directory}'");
								 Environment.Exit(-1);
							 }
						 })
						 .WithNotParsed<Options>(e =>
						 {
							 Environment.Exit(-2);
						 });

			// Load the FileManagers
			var fmgr = new FileManager();
			fmgr.ImportFileManagers();
			if (fmgr.FileManagers.Count() == 0)
			{
				Console.Error.WriteLine("No file readers found");
				Environment.Exit(-3);
			}

			// Load the Scan engine and the patterns
			var s = new Scanner.ScanEngine();
			if ( s.LoadPatterns() == 0 )
			{
				Console.Error.WriteLine("No patterns found");
				Environment.Exit(-4);
			}

			// Load the files to be processed
			var so = cmdline.Value.Recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
			var files = Directory.GetFiles(cmdline.Value.Directory, "*.*", so);
			if (files.Length == 0)
			{
				Console.Error.WriteLine("No files found");
				Environment.Exit(-5);
			}

			// Print header
			var starttime = DateTime.Now;
			if (cmdline.Value.CSVOuput == false)
			{
				Console.WriteLine("*************************************************************************");
				Console.WriteLine($"({starttime}) Processing files with the following parameters:");
				Console.WriteLine($"Recursive='{cmdline.Value.Recursive}' Directory='{cmdline.Value.Directory}' Verbosity='{cmdline.Value.Verbosity}'");
				Console.WriteLine($"Looking for the following patterns: '{s.GetPatternNames()}'");
				Console.WriteLine($"In the following file types: '{fmgr.GetFileExtentions()}'");
				Console.WriteLine("*************************************************************************\n");
			}
			else
			{
				switch (cmdline.Value.Verbosity)
				{
					case OutputLevel.B:
						if (cmdline.Value.ImageScan == true)
							Console.WriteLine("File Name,Possible Match Count,Has Images");
						else
							Console.WriteLine("File Name,Possible Match Count");
						break;
					case OutputLevel.M:
						if (cmdline.Value.ImageScan == true)
							Console.WriteLine("File Name,Pattern(s) Found,Total Found,Has Images");
						else
							Console.WriteLine("File Name,Pattern(s) Found,Total Found");
						break;
					case OutputLevel.V:
						if (cmdline.Value.ImageScan == true)
							Console.WriteLine("File Name,Has Images,Patterns Found,Pattern Name,Pattern");
						else
							Console.WriteLine("File Name,Patterns Found,Pattern Name,Pattern");
						break;
				}
			}

			// Process files
			var processedfiles = 0;
			var rangePartitioner = Partitioner.Create(0, files.Length);
			Parallel.ForEach(rangePartitioner, (range, loopState) => 
			{
				for (int i = range.Item1; i < range.Item2; i++)
				{
					try
					{
						// Store names of items found 
						var foundNames = new StringCollection();
						var VmodeSep = cmdline.Value.ImageScan ? ",,," : ",,";
						// If there is a file processor for a give file extension, process the file..
						foreach (var fm in from fm in fmgr.FileManagers where fmgr.SupportFileExtension(fm, Path.GetExtension(files[i])) select fm)
						{
							++processedfiles;

							var fc = fm.ReadAllText(files[i], cmdline.Value.ImageScan);
							if (s.Scan(fc.Text))
							{
								if (cmdline.Value.Verbosity == OutputLevel.V)
								{
									if (cmdline.Value.CSVOuput == false)
										Console.WriteLine($"**********Processing file {files[i]} ...**********");
									else if (cmdline.Value.ImageScan == true)
										Console.WriteLine($"{files[i]},{fc.HasImages},{s.Matches.Count}");
									else
										Console.WriteLine($"{files[i]},{s.Matches.Count}");
								}

								foundNames.Clear();

								// Output results
								foreach (var key in s.Matches)
								{
									switch (cmdline.Value.Verbosity)
									{
										case OutputLevel.M:
											if (foundNames.Contains(key.Name) == false)
												foundNames.Add(key.Name);
											break;
										case OutputLevel.V:
											if (cmdline.Value.CSVOuput == false)
												Console.WriteLine($"{key.Name} was found at {key.Index} with a value of {key.Value}");
											Console.WriteLine($"{VmodeSep}{key.Name},{key.Value.Replace(',', '.')}");
											break;
									}
								}
							}
							switch (cmdline.Value.Verbosity)
							{
								case OutputLevel.B:
									if (cmdline.Value.CSVOuput == false)
										Console.WriteLine($"Number of possible patterns found: {s.Matches.Count} in {files[i]}");
									else if (cmdline.Value.ImageScan == true)
										Console.WriteLine($"{files[i]},{s.Matches.Count},{fc.HasImages}");
									else
										Console.WriteLine($"{files[i]},{s.Matches.Count}");
									break;
								case OutputLevel.M:
									if (cmdline.Value.CSVOuput == false)
										Console.WriteLine($"Found {s.GetMatchNames()} in {files[i]}");
									else if (cmdline.Value.ImageScan == true)
										Console.WriteLine($"{files[i]},{s.GetMatchNames()},{s.Matches.Count},{fc.HasImages}");
									else
										Console.WriteLine($"{files[i]},{s.GetMatchNames()},{s.Matches.Count}");
									break;
								case OutputLevel.V:
									if (cmdline.Value.CSVOuput == false)
										Console.WriteLine($"Number of possible patterns found: {s.Matches.Count}\n");
									break;
							}
						}
					}
					catch (Exception e)
					{
						Console.Error.WriteLine($"An error occured while processing: {files[i]} => {e.Message}");
					}
				}
			});
			if (processedfiles > 0)
			{
				if (cmdline.Value.CSVOuput == false)
				{
					Console.WriteLine("*************************************************************************");
					Console.WriteLine($"({DateTime.Now}) Finished processing files:");
					Console.WriteLine($"Files Processed='{processedfiles}' Processing Time (seconds) ='{(DateTime.Now - starttime).TotalSeconds}'");
					Console.WriteLine("*************************************************************************\n");
				}
				else
				{
					Console.WriteLine($"\nScan Started at: {starttime}, Finished at: {DateTime.Now}");
					Console.WriteLine($"Files Processed='{processedfiles}', Processing Time (seconds) ='{(DateTime.Now - starttime).TotalSeconds}'");
				}
			}
		}
	}
}
