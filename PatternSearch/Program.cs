﻿using System;
using System.Configuration;
using System.IO;
using System.Linq;
using CommandLine;


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
							bool dir = string.IsNullOrEmpty(o.Directory);
							bool fil = string.IsNullOrEmpty(o.File);
							if (dir == true && fil == true)
							{
								Console.Error.WriteLine("You must provide either a directory or file to process");
								Environment.Exit(-1);
							}
							// Make sure it is a valid directory before we do anything
							if (dir == false && Directory.Exists(o.Directory) == false)
							{
								Console.Error.WriteLine($"Invalid directory: '{o.Directory}'");
								Environment.Exit(-1);
							}
							if (fil == false && File.Exists(o.File) == false)
							{
								Console.Error.WriteLine($"Invalid file: '{o.File}'");
								Environment.Exit(-1);
							}
						})
						 .WithNotParsed<Options>(e =>
						 {
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
			cmdline.Value.ErrFile = errFile;

			// Load the FileManagers
			var smgr = new SearchManager();
			smgr.ImportFileReaders();
			if (smgr.FileReaders.Count() == 0)
			{
				File.AppendAllText(cmdline.Value.ErrFile, "No file readers found");
				Environment.Exit(-3);
			}

			// Load the Scan engine and the patterns
			var s = new Scanner.ScanEngine();
			if (s.LoadPatterns() == 0)
			{
				File.AppendAllText(cmdline.Value.ErrFile, "No patterns found");
				Environment.Exit(-4);
			}

			// Load the files to be processed
			string[] files = null;
			var so = cmdline.Value.Recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
			if (string.IsNullOrEmpty(cmdline.Value.Directory) == false)
			{
				files = Directory.GetFiles(cmdline.Value.Directory, "*.*", so);
				Console.WriteLine($"Scanning {cmdline.Value.Directory} for files that contain sensitive information. Results will be stored in {cmdline.Value.OutFile}...");
			}
			if (string.IsNullOrEmpty(cmdline.Value.File) == false)
			{
				if (files == null || files.Length == 0)
				{
					files = new string[] { cmdline.Value.File };
					Console.WriteLine($"Scanning {cmdline.Value.File} for sensitive information. Results will be stored in {cmdline.Value.OutFile}...");
				}
				else
					files = files.Append(cmdline.Value.File).ToArray();
			}
			if (files.Length == 0)
			{
				File.AppendAllText(cmdline.Value.ErrFile, "No files found");
				Environment.Exit(-5);
			}

			var starttime = DateTime.Now;

			// Print header
			PrintHeader(cmdline, smgr, starttime);

			// Process files
			var processedfiles = 0;
			foreach (var file in files)
			{
				try
				{
					// If there is a file processor for a give file extension, process the file..
					foreach (var fm in from fm in smgr.FileReaders where smgr.SupportFileExtension(fm, Path.GetExtension(file)) select fm)
					{
						// Only count if we have a file processor
						++processedfiles;

						// Read the text
						var fc = fm.ReadAllText(file, cmdline.Value.ImageScan);

						// Scan the text for patterns
						s.Scan(fc.Text);

						// Output start
						PrintProcessingStart(cmdline, s, file, fc);

						// If verbose enabled, print more details for each match
						if (cmdline.Value.Verbosity == OutputLevel.V)
						{
							foreach (var match in s.PatternsFound.OrderBy(i => i.Index))
								PrintMatch(cmdline, match);
						}

						// Output Results
						PrintProcessingResults(cmdline, s, file, fc);
					}
				}
				catch (Exception e)
				{
					File.AppendAllText(cmdline.Value.ErrFile, $"An error occured while processing: {file} => {e.Message}");
				}
			}
			if (processedfiles > 0)
				PrintFooter(cmdline, starttime, processedfiles, s.GetPatternNames());
		}

		private static void PrintHeader(Parsed<Options> cmdline, SearchManager fmgr, DateTime starttime)
		{
			if (cmdline.Value.CSVOuput == false)
			{
				File.AppendAllText(cmdline.Value.OutFile, "*************************************************************************");
				File.AppendAllText(cmdline.Value.OutFile, $"({starttime}) Processing files with the following parameters:");
				File.AppendAllText(cmdline.Value.OutFile, $"Recursive='{cmdline.Value.Recursive}' Directory='{cmdline.Value.Directory}' Verbosity='{cmdline.Value.Verbosity}'");
				File.AppendAllText(cmdline.Value.OutFile, $"In the following file types: '{fmgr.GetFileExtentions()}'");
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

		private static void PrintProcessingStart(Parsed<Options> cmdline, Scanner.ScanEngine s, string file, global::FileManager.FileContents fc)
		{
			if (cmdline.Value.Verbosity == OutputLevel.V)
			{
				string fileprefix = ConfigurationManager.AppSettings["FilePrefix"] ?? "";
				if (cmdline.Value.CSVOuput == false)
					File.AppendAllText(cmdline.Value.OutFile, $"**********Processing file {file} ...**********\n");
				else if (cmdline.Value.ImageScan == true)
					File.AppendAllText(cmdline.Value.OutFile, $"{fileprefix}{file},{fc.Text?.Length},{fc.HasImages},{s.PatternsFound.Count}\n");
				else
					File.AppendAllText(cmdline.Value.OutFile, $"{fileprefix}{file},{fc.Text?.Length},{s.PatternsFound.Count}\n");
			}
		}

		private static void PrintMatch(Parsed<Options> cmdline, Scanner.PatternFound match)
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

		private static void PrintProcessingResults(Parsed<Options> cmdline, Scanner.ScanEngine s, string file, global::FileManager.FileContents fc)
		{
			string fileprefix = ConfigurationManager.AppSettings["FilePrefix"] ?? "";
			switch (cmdline.Value.Verbosity)
			{
				case OutputLevel.B:
					if (cmdline.Value.CSVOuput == false)
						File.AppendAllText(cmdline.Value.OutFile, $"Number of possible patterns found: {s.PatternsFound.Count} in {file}\n");
					else if (cmdline.Value.ImageScan == true)
						File.AppendAllText(cmdline.Value.OutFile, $"{fileprefix}{file},{fc.Text?.Length},{s.PatternsFound.Count},{fc.HasImages}\n");
					else
						File.AppendAllText(cmdline.Value.OutFile, $"{fileprefix}{file},{fc.Text?.Length},{s.PatternsFound.Count}\n");
					break;
				case OutputLevel.M:
					if (cmdline.Value.CSVOuput == false)
						File.AppendAllText(cmdline.Value.OutFile, $"Found {s.GetPatternsFound()} in {file}\n");
					else if (cmdline.Value.ImageScan == true)
						File.AppendAllText(cmdline.Value.OutFile, $"{fileprefix}{file},{fc.Text?.Length},{s.GetPatternsFound()},{s.PatternsFound.Count},{fc.HasImages}\n");
					else
						File.AppendAllText(cmdline.Value.OutFile, $"{fileprefix}{file},{fc.Text?.Length},{s.GetPatternsFound()},{s.PatternsFound.Count}\n");
					break;
				case OutputLevel.V:
					if (cmdline.Value.CSVOuput == false)
						File.AppendAllText(cmdline.Value.OutFile, $"Number of possible patterns found: {s.PatternsFound.Count}\n");
					break;
			}
		}

		private static void PrintFooter(Parsed<Options> cmdline, DateTime starttime, int processedfiles, string patterns)
		{
			var procduration = DateTime.Now - starttime;
			var proctime = (procduration.TotalSeconds > 60) ? procduration.TotalMinutes: procduration.TotalSeconds;
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
