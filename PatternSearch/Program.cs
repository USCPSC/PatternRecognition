using CommandLine;
using System;
using System.Collections.Specialized;
using System.IO;

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
							 if (o.Verbosity != OutputLevel.B)
							 {
								 Console.WriteLine("*************************************************************************");
								 Console.WriteLine($"({DateTime.Now}) Processing files with the following parameters:");
								 Console.WriteLine($"Recursive='{o.Recursive}' Directory='{o.Directory}' Verbosity='{o.Verbosity}'");
								 Console.WriteLine("*************************************************************************\n");
							 }
						 })
						 .WithNotParsed<Options>(e =>
						 {
							 Environment.Exit(-1);
						 });

			// Load the FileManagers. 
			FileManager fmgr = new FileManager();
			fmgr.ImportFileManagers();

			// Load the Scan engine and the patterns
			Scanner.ScanEngine s = new Scanner.ScanEngine();
			s.LoadPatterns();

			// Load the files to be processed
			SearchOption so = cmdline.Value.Recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
			string[] files = Directory.GetFiles(cmdline.Value.Directory, "*.*", so);

			// Process files
			foreach (string file in files)
			{
				try
				{
					// Store names of items found 
					StringCollection foundNames = new StringCollection();

					foreach (var fm in fmgr.FileManagers)
					{
						// If there is a file processor for a give file extension, process the file..
						if (Path.GetExtension(file) == fm.FileExtention)
						{
							if (cmdline.Value.Verbosity == OutputLevel.V)
								Console.WriteLine($"**********Processing file {file} ...**********");

							string filecontents = fm.ReadAllText(file);
							if (s.Scan(filecontents))
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
												Console.WriteLine($"{key.Name} was found in {file}");
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
									Console.WriteLine($"Number of possible PII items found: {s.Matches.Count} in {file}\n");
									break;
								case OutputLevel.V:
									Console.WriteLine($"Number of possible PII items found: {s.Matches.Count}\n");
									break;
							}
						}
					}
				}
				catch (Exception e)
				{
					Console.WriteLine("An error occured while processing: {0} => {1}", file, e.Message);
				}
			}
		}
	}
}
