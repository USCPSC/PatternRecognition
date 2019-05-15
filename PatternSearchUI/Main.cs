using System;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using FileManager;

namespace PatternSearchUI
{
	public partial class Main : Form
	{
		const string Waiting = "Waiting";
		const int ListItemIdxDirectory = 0;
		const int ListItemIdxImageScan = 1;
		const int ListItemIdxStatus = 2;
		private string lastDir;

		public Main()
		{
			InitializeComponent();
			lastDir = Environment.CurrentDirectory;
		}

		private void btnBrowse_Click(object sender, EventArgs e)
		{
			FolderBrowserDialog fld = new FolderBrowserDialog();
			fld.SelectedPath = lastDir;
			if (fld.ShowDialog() == DialogResult.OK)
			{
				txtFolder.Text = fld.SelectedPath;
				lastDir = fld.SelectedPath;
			}
			btnAdd.Enabled = true;
		}

		private void BtnAdd_Click(object sender, EventArgs e)
		{
			string[] listItems = { txtFolder.Text, cbImageScan.Checked ? "True" : "False", Waiting };
			var lvi = new ListViewItem(listItems);
			lvi.SubItems[ListItemIdxImageScan].Tag = cbImageScan.Checked;
			lstBatch.Items.Add(lvi);
			txtFolder.Text = "";
			btnAdd.Enabled = false;
			btnStart.Enabled = true;
		}

		private void LstBatch_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Delete && lstBatch.SelectedItems?.Count > 0)
			{
				lstBatch.Items.Remove(lstBatch.SelectedItems[0]);
			}
			if (lstBatch.Items.Count == 0)
				btnStart.Enabled = false;
		}

		private void BtnClear_Click(object sender, EventArgs e)
		{
			lstBatch.Items.Clear();
			btnStart.Enabled = false;
		}

		private async void BtnStart_Click(object sender, EventArgs e)
		{
			// Load the FileManagers
			var smgr = new SearchManager();
			smgr.ImportFileReaders();
			if (smgr.FileReaders.Count() == 0)
			{
				MessageBox.Show("No file readers found", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			// Load the Scan engine and the patterns
			var s = new Scanner.ScanEngine();
			if (s.LoadPatterns() == 0)
			{
				MessageBox.Show("No patterns found", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}
			btnStart.Enabled = false;
			btnClear.Enabled = false;
			foreach (ListViewItem i in lstBatch.Items)
			{
				string dir = i.SubItems[ListItemIdxDirectory].Text;

				string outFile = dir + ".csv";
				if (File.Exists(outFile))
					File.Delete(outFile);

				string errFile = dir + ".err";
				if (File.Exists(errFile))
					File.Delete(errFile);

				bool imageScan = (bool)i.SubItems[ListItemIdxImageScan].Tag;

				// Load the files to be processed
				var files = Directory.GetFiles(dir);
				if (files.Length == 0)
					File.AppendAllText(errFile, "No files found\n");
				else if (i.SubItems[ListItemIdxStatus].Text == Waiting)
				{
					i.SubItems[ListItemIdxStatus].Text = "Processing";
					await Task.Run(() => { ProcessDirectories(smgr, s, outFile, errFile, imageScan, files); });
					i.SubItems[ListItemIdxStatus].Text = "Processed";
				}
			}
			btnClear.Enabled = true;
		}

		private void ProcessDirectories(SearchManager smgr, Scanner.ScanEngine s, string outFile, string errFile, bool imageScan, string[] files)
		{
			var starttime = DateTime.Now;

			// Print header
			PrintHeader(outFile, imageScan);

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
						var fc = fm.ReadAllText(file, imageScan);

						// Scan the text for patterns
						s.Scan(fc.Text);

						// Output start
						PrintProcessingStart(outFile, s, file, fc, imageScan);

						foreach (var match in s.PatternsFound.OrderBy(idx => idx.Index))
							PrintMatch(outFile, match, imageScan);
					}
				}
				catch (Exception err)
				{
					File.AppendAllText(errFile, $"An error occured while processing: {file} => {err.Message}\n");
				}
			}
			if (processedfiles > 0)
				PrintFooter(outFile, starttime, processedfiles, s.GetPatternNames());
		}

		private void PrintHeader(string outFile, bool imageScan)
		{
			if (imageScan == true)
				File.AppendAllText(outFile, "File Name,Text Size,Has Images,Patterns Found,Pattern Name,Pattern\n");
			else
				File.AppendAllText(outFile, "File Name,Text Size,Patterns Found,Pattern Name,Pattern\n");
		}
		private static void PrintProcessingStart(string outFile, Scanner.ScanEngine s, string file, FileContents fc, bool imageScan)
		{
			string fileprefix = ConfigurationManager.AppSettings["FilePrefix"] ?? "";
			if (imageScan == true)
				File.AppendAllText(outFile, $"{fileprefix}{file},{fc.Text?.Length},{fc.HasImages},{s.PatternsFound.Count}\n");
			else
				File.AppendAllText(outFile, $"{fileprefix}{file},{fc.Text?.Length},{s.PatternsFound.Count}\n");
		}
		private static void PrintMatch(string outFile, Scanner.PatternFound match, bool imageScan)
		{
			// Comma offset variable to align output in CSV output
			var commaOffset = imageScan ? ",,,," : ",,,";

			// Replace comma with a period for comma separated output
			string str = match.Value.Replace(',', '.');

			// Quote numeric value so excel won't try to convert it to a number
			if (str.All(char.IsDigit))
				str = string.Format($"'{str}'");

			File.AppendAllText(outFile, $"{commaOffset}{match.Name},{str}\n");
		}
		private static void PrintFooter(string outFile, DateTime starttime, int processedfiles, string patterns)
		{
			var procduration = DateTime.Now - starttime;
			var proctime = (procduration.TotalSeconds > 60) ? procduration.TotalMinutes : procduration.TotalSeconds;
			string increment = (procduration.TotalSeconds > 60) ? "minutes" : "seconds";

			File.AppendAllText(outFile, $"\nScan Started at: {starttime}, Finished at: {DateTime.Now}\n");
			File.AppendAllText(outFile, $"Files Processed='{processedfiles}', Processing Time ({increment}) ='{proctime}'\n");
			File.AppendAllText(outFile, $"Patterns searched: {patterns}\n");
		}
	}
}
