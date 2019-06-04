namespace PatternSearchUI
{
	partial class Main
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Main));
			this.btnStart = new System.Windows.Forms.Button();
			this.lstBatch = new System.Windows.Forms.ListView();
			this.source = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.imageScan = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.status = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.label1 = new System.Windows.Forms.Label();
			this.txtFolder = new System.Windows.Forms.TextBox();
			this.btnBrowse = new System.Windows.Forms.Button();
			this.cbImageScan = new System.Windows.Forms.CheckBox();
			this.btnAdd = new System.Windows.Forms.Button();
			this.btnClear = new System.Windows.Forms.Button();
			this.label2 = new System.Windows.Forms.Label();
			this.btnConfig = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// btnStart
			// 
			this.btnStart.Enabled = false;
			this.btnStart.Location = new System.Drawing.Point(470, 338);
			this.btnStart.Name = "btnStart";
			this.btnStart.Size = new System.Drawing.Size(75, 23);
			this.btnStart.TabIndex = 0;
			this.btnStart.Text = "Start";
			this.btnStart.UseVisualStyleBackColor = true;
			this.btnStart.Click += new System.EventHandler(this.BtnStart_Click);
			// 
			// lstBatch
			// 
			this.lstBatch.AllowDrop = true;
			this.lstBatch.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.source,
            this.imageScan,
            this.status});
			this.lstBatch.FullRowSelect = true;
			this.lstBatch.Location = new System.Drawing.Point(13, 117);
			this.lstBatch.MultiSelect = false;
			this.lstBatch.Name = "lstBatch";
			this.lstBatch.Size = new System.Drawing.Size(532, 215);
			this.lstBatch.TabIndex = 1;
			this.lstBatch.UseCompatibleStateImageBehavior = false;
			this.lstBatch.View = System.Windows.Forms.View.Details;
			this.lstBatch.SelectedIndexChanged += new System.EventHandler(this.LstBatch_SelectedIndexChanged);
			this.lstBatch.DragDrop += new System.Windows.Forms.DragEventHandler(this.LstBatch_DragDrop);
			this.lstBatch.DragEnter += new System.Windows.Forms.DragEventHandler(this.LstBatch_DragEnter);
			this.lstBatch.DoubleClick += new System.EventHandler(this.LstBatch_DoubleClick);
			this.lstBatch.KeyDown += new System.Windows.Forms.KeyEventHandler(this.LstBatch_KeyDown);
			// 
			// source
			// 
			this.source.Text = "Directory";
			this.source.Width = 300;
			// 
			// imageScan
			// 
			this.imageScan.Text = "Image Scan";
			// 
			// status
			// 
			this.status.Text = "Status";
			this.status.Width = 150;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(24, 36);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(69, 17);
			this.label1.TabIndex = 2;
			this.label1.Text = "Directory:";
			// 
			// txtFolder
			// 
			this.txtFolder.Location = new System.Drawing.Point(101, 36);
			this.txtFolder.Name = "txtFolder";
			this.txtFolder.ReadOnly = true;
			this.txtFolder.Size = new System.Drawing.Size(410, 22);
			this.txtFolder.TabIndex = 3;
			// 
			// btnBrowse
			// 
			this.btnBrowse.Location = new System.Drawing.Point(517, 36);
			this.btnBrowse.Name = "btnBrowse";
			this.btnBrowse.Size = new System.Drawing.Size(28, 23);
			this.btnBrowse.TabIndex = 4;
			this.btnBrowse.Text = "...";
			this.btnBrowse.UseVisualStyleBackColor = true;
			this.btnBrowse.Click += new System.EventHandler(this.btnBrowse_Click);
			// 
			// cbImageScan
			// 
			this.cbImageScan.AutoSize = true;
			this.cbImageScan.Location = new System.Drawing.Point(102, 62);
			this.cbImageScan.Name = "cbImageScan";
			this.cbImageScan.Size = new System.Drawing.Size(104, 21);
			this.cbImageScan.TabIndex = 6;
			this.cbImageScan.Text = "Image Scan";
			this.cbImageScan.UseVisualStyleBackColor = true;
			// 
			// btnAdd
			// 
			this.btnAdd.Enabled = false;
			this.btnAdd.Location = new System.Drawing.Point(470, 88);
			this.btnAdd.Name = "btnAdd";
			this.btnAdd.Size = new System.Drawing.Size(75, 23);
			this.btnAdd.TabIndex = 11;
			this.btnAdd.Text = "Add";
			this.btnAdd.UseVisualStyleBackColor = true;
			this.btnAdd.Click += new System.EventHandler(this.BtnAdd_Click);
			// 
			// btnClear
			// 
			this.btnClear.Location = new System.Drawing.Point(389, 338);
			this.btnClear.Name = "btnClear";
			this.btnClear.Size = new System.Drawing.Size(75, 23);
			this.btnClear.TabIndex = 12;
			this.btnClear.Text = "Clear";
			this.btnClear.UseVisualStyleBackColor = true;
			this.btnClear.Click += new System.EventHandler(this.BtnClear_Click);
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(13, 94);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(48, 17);
			this.label2.TabIndex = 13;
			this.label2.Text = "Batch:";
			// 
			// btnConfig
			// 
			this.btnConfig.Image = global::PatternSearchUI.Properties.Resources.perm_group_system_tools;
			this.btnConfig.Location = new System.Drawing.Point(58, 87);
			this.btnConfig.Name = "btnConfig";
			this.btnConfig.Size = new System.Drawing.Size(38, 29);
			this.btnConfig.TabIndex = 14;
			this.btnConfig.UseVisualStyleBackColor = true;
			this.btnConfig.Click += new System.EventHandler(this.BtnConfig_Click);
			// 
			// Main
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(558, 377);
			this.Controls.Add(this.btnConfig);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.btnClear);
			this.Controls.Add(this.btnAdd);
			this.Controls.Add(this.cbImageScan);
			this.Controls.Add(this.btnBrowse);
			this.Controls.Add(this.txtFolder);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.lstBatch);
			this.Controls.Add(this.btnStart);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "Main";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Pattern Search";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button btnStart;
		private System.Windows.Forms.ListView lstBatch;
		private System.Windows.Forms.ColumnHeader source;
		private System.Windows.Forms.ColumnHeader imageScan;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox txtFolder;
		private System.Windows.Forms.Button btnBrowse;
		private System.Windows.Forms.CheckBox cbImageScan;
		private System.Windows.Forms.Button btnAdd;
		private System.Windows.Forms.Button btnClear;
		private System.Windows.Forms.ColumnHeader status;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Button btnConfig;
	}
}

