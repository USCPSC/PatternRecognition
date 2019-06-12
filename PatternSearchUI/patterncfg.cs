using System;
using System.Windows.Forms;
using Scanner;

namespace PatternSearchUI
{
	public partial class PatternCfg : Form
	{
		private ScanEngine se;

		public PatternCfg()
		{
			InitializeComponent();
		}

		private void Patterncfg_Load(object sender, EventArgs e)
		{
			se = new Scanner.ScanEngine();
			if (se.LoadPatterns() == 0)
				MessageBox.Show("No patterns found", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

			foreach (Pattern p in se.Patterns)
				chklbPatterns.Items.Add(p, p.enabled);
		}

		private void BtnSave_Click(object sender, EventArgs e)
		{
			for (int i = 0; i < chklbPatterns.Items.Count; i++)
				((Pattern)chklbPatterns.Items[i]).enabled = chklbPatterns.GetItemChecked(i);
			se.SavePatterns();
			this.Close();
		}
	}
}
