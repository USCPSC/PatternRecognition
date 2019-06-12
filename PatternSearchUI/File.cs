using System;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using FileManager;
using Scanner;

namespace PatternSearchUI
{
	public partial class FileProcessing : Form
	{
		const string Waiting = "Waiting";
		const string Processed = "Processed";
		const int ListItemIdxFile = 0;
		const int ListItemIdxImageScan = 1;
		const int ListItemIdxStatus = 2;
		private readonly SearchManager smgr = new SearchManager();
		private readonly ScanEngine scanEngine = new ScanEngine();

		public FileProcessing()
		{
			InitializeComponent();
		}
		private void FileProcessing_Load(object sender, EventArgs e)
		{
			// Load the FileManagers
			smgr.ImportFileReaders();
			if (smgr.FileReaders.Count() == 0)
			{
				MessageBox.Show("No file readers found", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			// Load the Scan engine and the patterns
			if (scanEngine.LoadPatterns() == 0)
			{
				MessageBox.Show("No patterns found", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}
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
			btnStart.Enabled = false;
			btnClear.Enabled = false;

			if (Directory.Exists("Results") == false)
				Directory.CreateDirectory("Results");
			var csvFile = string.Format($"Results\\SearchResults {DateTime.Now.Ticks}.csv");
			string errFile = Path.ChangeExtension(csvFile, ".err");

			var starttime = DateTime.Now;

			// Print header
			PrintHeader(csvFile);

			// Load the files to be processed
			foreach (ListViewItem i in lstBatch.Items)
			{
				if (i.SubItems[ListItemIdxStatus].Text == Waiting)
				{
					var filename = i.SubItems[ListItemIdxFile].Text;
					bool imageScan = (bool)i.SubItems[ListItemIdxImageScan].Tag;

					i.SubItems[ListItemIdxStatus].Text = "Processing";
					await Task.Run(() => { ProcessFile(csvFile, errFile, imageScan, filename); });
					i.SubItems[ListItemIdxStatus].Text = Processed;
					i.Tag = csvFile;
				}
			}
			PrintFooter(csvFile, starttime, lstBatch.Items.Count, scanEngine.GetPatternNames());

			btnClear.Enabled = true;
		}
		private void BtnConfig_Click(object sender, EventArgs e)
		{
			PatternCfg pcfg = new PatternCfg();
			pcfg.ShowDialog();
		}

		private void LstBatch_DoubleClick(object sender, EventArgs e)
		{
			if (lstBatch.SelectedItems.Count == 1)
			{
				if (lstBatch.SelectedItems[0].SubItems[ListItemIdxStatus].Text == Processed)
					Process.Start((string)lstBatch.SelectedItems[0].Tag);
				else
					lstBatch.SelectedItems[0].Selected = false;
			}
		}

		private void LstBatch_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (lstBatch.SelectedItems.Count == 1)
			{
				if (lstBatch.SelectedItems[0].SubItems[ListItemIdxStatus].Text != Processed)
					lstBatch.SelectedItems[0].Selected = false;
			}
		}

		private void ProcessFile(string outFile, string errFile, bool imageScan, string file)
		{
			// Process files
			try
			{
				// If there is a file processor for a give file extension, process the file..
				foreach (var fm in from fm in smgr.FileReaders where smgr.SupportFileExtension(fm, Path.GetExtension(file)) select fm)
				{
					// Read the text
					var fc = fm.ReadAllText(file, imageScan);

					// Scan the text for patterns
					scanEngine.Scan(fc.Text);

					// Output start
					PrintProcessingStart(outFile, file, fc, imageScan);

					foreach (var match in scanEngine.PatternsFound.OrderBy(idx => idx.Index))
						PrintMatch(outFile, match, imageScan);
				}
			}
			catch (Exception err)
			{
				File.AppendAllText(errFile, $"An error occured while processing: {file} => {err.Message}\n");
			}
		}

		private void PrintHeader(string outFile)
		{
			File.AppendAllText(outFile, "File Name,Text Size,Has Images,Patterns Found,Pattern Name,Pattern\n");
		}
		private void PrintProcessingStart(string outFile, string file, FileContents fc, bool imageScan)
		{
			var fileprefix = ConfigurationManager.AppSettings["FilePrefix"] ?? "";
			if (imageScan == true)
				File.AppendAllText(outFile, $"{fileprefix}{file},{fc.Text?.Length},{fc.HasImages},{scanEngine.PatternsFound.Count}\n");
			else
				File.AppendAllText(outFile, $"{fileprefix}{file},{fc.Text?.Length},NA,{scanEngine.PatternsFound.Count}\n");
		}
		private static void PrintMatch(string outFile, Scanner.PatternFound match, bool imageScan)
		{
			// Comma offset variable to align output in CSV output
			var commaOffset = ",,,,";

			// Replace comma with a period for comma separated output
			var str = match.Value.Replace(',', '.');

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

		private void LstBatch_DragEnter(object sender, DragEventArgs e)
		{
			foreach (var s in (string[])e.Data.GetData(DataFormats.FileDrop, false))
			{
				if ( smgr.SupportFileExtension(Path.GetExtension(s)) )
					e.Effect = DragDropEffects.All;
				else
				{
					e.Effect = DragDropEffects.None;
					return;
				}
			}
		}

		private void LstBatch_DragDrop(object sender, DragEventArgs e)
		{
			foreach (var s in (string[])e.Data.GetData(DataFormats.FileDrop, false))
			{
				if (smgr.SupportFileExtension(Path.GetExtension(s)))
					AddListItem(s, cbImageScan.Checked);
			}
		}

		private void AddListItem(string s, bool ImageScan)
		{
			var items = from ListViewItem l in lstBatch.Items where l.Text == s select l;
			if (items?.Count() == 0)
			{
				var lvi = new ListViewItem(new string[] { s, ImageScan ? "True" : "False", Waiting });
				lvi.SubItems[ListItemIdxImageScan].Tag = ImageScan;
				lstBatch.Items.Add(lvi);
				btnStart.Enabled = true;
			}
		}

		private void BatchToolStripMenuItem_Click(object sender, EventArgs e)
		{
			this.Hide();
			var frm = new BatchProcessing();
			Program.UpdateLastDialog(LastDialog.Batch);
			frm.ShowDialog();
			this.Close();
		}
	}
}
