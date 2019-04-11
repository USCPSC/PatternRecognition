using CommandLine;
using System;
using System.Collections.Specialized;
using System.IO;
using System.Linq;

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

			[Option('v', "verbose", Required = false, HelpText = "Verbosity level: B=Basic, M=Moderate, V=Verbose")]
			public OutputLevel Verbosity { get; set; }

			[Option('d', "directory", Required = true, HelpText = "Input directory to be processed.")]
			public string Directory { get; set; }
		}

		/// <summary>
		/// Main processor
		/// </summary>
		/// <param name="args"></param>
		static void Main(string[] args)
		{
			// Parse command line
			Parsed<Options> cmdline = (Parsed<Options>)Parser.Default.ParseArguments<Options>(args)
						 .WithParsed<Options>(o =>
						 {
							 // Make sure it is a valid directory before we do anything
							 if (Directory.Exists(o.Directory) == false)
							 {
								 Console.WriteLine($"Invalid directory: '{o.Directory}'");
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

			// Load the Scan engine and the patterns
			var s = new Scanner.ScanEngine();
			s.LoadPatterns();

			// Load the files to be processed
			var so = cmdline.Value.Recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
			string[] files = Directory.GetFiles(cmdline.Value.Directory, "*.*", so);

			// Print header
			var starttime = DateTime.Now;
			if (files.Length > 0 && cmdline.Value.Verbosity != OutputLevel.B)
			{
				Console.WriteLine("*************************************************************************");
				Console.WriteLine($"({starttime}) Processing files with the following parameters:");
				Console.WriteLine($"Recursive='{cmdline.Value.Recursive}' Directory='{cmdline.Value.Directory}' Verbosity='{cmdline.Value.Verbosity}'");
				Console.WriteLine($"Looking for the following patterns: '{s.GetPatternNames()}'");
				Console.WriteLine($"In the following file types: '{fmgr.GetFileExtentions()}'");
				Console.WriteLine("*************************************************************************\n");
			}

			// Process files
			int processedfiles = 0;
			for (int i = 0; i < files.Length; i++)
			{
				try
				{
					// Store names of items found 
					var foundNames = new StringCollection();
					
					// If there is a file processor for a give file extension, process the file..
					foreach (var fm in from fm in fmgr.FileManagers where Path.GetExtension(files[i]) == fm.FileExtention select fm)
					{
						++processedfiles;
						if (cmdline.Value.Verbosity == OutputLevel.V)
							Console.WriteLine($"**********Processing file {files[i]} ...**********");

						if (s.Scan(fm.ReadAllText(files[i])))
						{
							foundNames.Clear();

							// Output results
							foreach (var key in s.Matches)
							{
								switch (cmdline.Value.Verbosity)
								{
									case OutputLevel.M:
										if (foundNames.Contains(key.Name) == false)
										{
											Console.WriteLine($"{key.Name} was found in {files[i]}");
											foundNames.Add(key.Name);
										}
										break;
									case OutputLevel.V:
										Console.WriteLine($"{key.Name} was found at {key.Index} with a value of {key.Value}");
										break;
								}
							}
						}

						switch (cmdline.Value.Verbosity)
						{
							case OutputLevel.B:
								Console.WriteLine($"Number of possible PII items found: {s.Matches.Count} in {files[i]}\n");
								break;
							case OutputLevel.V:
								Console.WriteLine($"Number of possible PII items found: {s.Matches.Count}\n");
								break;
						}
					}
				}
				catch (Exception e)
				{
					Console.WriteLine($"An error occured while processing: {files[i]} => {e.Message}");
				}
			}
			if (processedfiles > 0 && cmdline.Value.Verbosity != OutputLevel.B)
			{
				Console.WriteLine("*************************************************************************");
				Console.WriteLine($"({DateTime.Now}) Finished processing files:");
				Console.WriteLine($"Files Processed='{processedfiles}' Processing Time (seconds) ='{(DateTime.Now - starttime).TotalSeconds}'");
				Console.WriteLine("*************************************************************************\n");
			}
		}
	}
}
