﻿using CommandLine;
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
			var smgr = new SearchManager();
			smgr.ImportFileReaders();
			if (smgr.FileReaders.Count() == 0)
			{
				Console.Error.WriteLine("No file readers found");
				Environment.Exit(-3);
			}

			// Load the Scan engine and the patterns
			var s = new Scanner.ScanEngine();
			if (s.LoadPatterns() == 0)
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

			var starttime = DateTime.Now;

			// Print header
			PrintHeader(cmdline, smgr, s, starttime);

			// Store names of items found 
			var foundNames = new StringCollection();

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

						// Process details
						foundNames.Clear();
						foreach (var match in s.PatternsFound)
							ProcessMatches(cmdline, foundNames, match);

						// Output Results
						PrintProcessingResults(cmdline, s, file, fc);
					}
				}
				catch (Exception e)
				{
					Console.Error.WriteLine($"An error occured while processing: {file} => {e.Message}");
				}
			}
			if (processedfiles > 0)
				PrintFooter(cmdline, starttime, processedfiles);
		}

		private static void ProcessMatches(Parsed<Options> cmdline, StringCollection foundNames, Scanner.PatternFound match)
		{
			// Comma offset variable to align output in CSV output
			var commaOffset = cmdline.Value.ImageScan ? ",,," : ",,";

			switch (cmdline.Value.Verbosity)
			{
				case OutputLevel.M:
					if (foundNames.Contains(match.Name) == false)
						foundNames.Add(match.Name);
					break;
				case OutputLevel.V:
					if (cmdline.Value.CSVOuput == false)
						Console.WriteLine($"{match.Name} was found at {match.Index} with a value of {match.Value}");
					else
					{
						// Replace comma with a period for comma separated output
						string str = match.Value.Replace(',', '.');

						// Quote numeric value so excel won't try to convert it to a number
						if (str.All(char.IsDigit))
							str = string.Format($"'{str}'");

						Console.WriteLine($"{commaOffset}{match.Name},{str}");
					}
					break;
			}
		}

		private static void PrintProcessingResults(Parsed<Options> cmdline, Scanner.ScanEngine s, string file, global::FileManager.FileContents fc)
		{
			switch (cmdline.Value.Verbosity)
			{
				case OutputLevel.B:
					if (cmdline.Value.CSVOuput == false)
						Console.WriteLine($"Number of possible patterns found: {s.PatternsFound.Count} in {file}");
					else if (cmdline.Value.ImageScan == true)
						Console.WriteLine($"{file},{s.PatternsFound.Count},{fc.HasImages}");
					else
						Console.WriteLine($"{file},{s.PatternsFound.Count}");
					break;
				case OutputLevel.M:
					if (cmdline.Value.CSVOuput == false)
						Console.WriteLine($"Found {s.GetPatternsFound()} in {file}");
					else if (cmdline.Value.ImageScan == true)
						Console.WriteLine($"{file},{s.GetPatternsFound()},{s.PatternsFound.Count},{fc.HasImages}");
					else
						Console.WriteLine($"{file},{s.GetPatternsFound()},{s.PatternsFound.Count}");
					break;
				case OutputLevel.V:
					if (cmdline.Value.CSVOuput == false)
						Console.WriteLine($"Number of possible patterns found: {s.PatternsFound.Count}\n");
					break;
			}
		}

		private static void PrintProcessingStart(Parsed<Options> cmdline, Scanner.ScanEngine s, string file, global::FileManager.FileContents fc)
		{
			if (cmdline.Value.Verbosity == OutputLevel.V)
			{
				if (cmdline.Value.CSVOuput == false)
					Console.WriteLine($"**********Processing file {file} ...**********");
				else if (cmdline.Value.ImageScan == true)
					Console.WriteLine($"{file},{fc.HasImages},{s.PatternsFound.Count}");
				else
					Console.WriteLine($"{file},{s.PatternsFound.Count}");
			}
		}

		private static void PrintFooter(Parsed<Options> cmdline, DateTime starttime, int processedfiles)
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

		private static void PrintHeader(Parsed<Options> cmdline, SearchManager fmgr, Scanner.ScanEngine s, DateTime starttime)
		{
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
		}
	}
}
