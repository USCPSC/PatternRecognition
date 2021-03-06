﻿namespace PatternSearchUI
{
	partial class FileProcessing
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
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FileProcessing));
			this.btnStart = new System.Windows.Forms.Button();
			this.lstBatch = new System.Windows.Forms.ListView();
			this.source = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.imageScan = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.status = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.cbImageScan = new System.Windows.Forms.CheckBox();
			this.btnClear = new System.Windows.Forms.Button();
			this.label2 = new System.Windows.Forms.Label();
			this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.batchToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.btnConfig = new System.Windows.Forms.Button();
			this.contextMenuStrip2 = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.openFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.openResultsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.contextMenuStrip1.SuspendLayout();
			this.contextMenuStrip2.SuspendLayout();
			this.SuspendLayout();
			// 
			// btnStart
			// 
			this.btnStart.Enabled = false;
			this.btnStart.Location = new System.Drawing.Point(470, 259);
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
			this.lstBatch.ContextMenuStrip = this.contextMenuStrip2;
			this.lstBatch.FullRowSelect = true;
			this.lstBatch.Location = new System.Drawing.Point(13, 38);
			this.lstBatch.MultiSelect = false;
			this.lstBatch.Name = "lstBatch";
			this.lstBatch.Size = new System.Drawing.Size(532, 215);
			this.lstBatch.TabIndex = 1;
			this.lstBatch.UseCompatibleStateImageBehavior = false;
			this.lstBatch.View = System.Windows.Forms.View.Details;
			this.lstBatch.DragDrop += new System.Windows.Forms.DragEventHandler(this.LstBatch_DragDrop);
			this.lstBatch.DragEnter += new System.Windows.Forms.DragEventHandler(this.LstBatch_DragEnter);
			this.lstBatch.KeyDown += new System.Windows.Forms.KeyEventHandler(this.LstBatch_KeyDown);
			// 
			// source
			// 
			this.source.Text = "File";
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
			// cbImageScan
			// 
			this.cbImageScan.AutoSize = true;
			this.cbImageScan.Location = new System.Drawing.Point(299, 13);
			this.cbImageScan.Name = "cbImageScan";
			this.cbImageScan.Size = new System.Drawing.Size(104, 21);
			this.cbImageScan.TabIndex = 6;
			this.cbImageScan.Text = "Image Scan";
			this.cbImageScan.UseVisualStyleBackColor = true;
			// 
			// btnClear
			// 
			this.btnClear.Enabled = false;
			this.btnClear.Location = new System.Drawing.Point(389, 259);
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
			this.label2.ContextMenuStrip = this.contextMenuStrip1;
			this.label2.Location = new System.Drawing.Point(13, 15);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(174, 17);
			this.label2.TabIndex = 13;
			this.label2.Text = "Files (drag and drop files):";
			// 
			// contextMenuStrip1
			// 
			this.contextMenuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
			this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.batchToolStripMenuItem});
			this.contextMenuStrip1.Name = "contextMenuStrip1";
			this.contextMenuStrip1.Size = new System.Drawing.Size(151, 28);
			// 
			// batchToolStripMenuItem
			// 
			this.batchToolStripMenuItem.Name = "batchToolStripMenuItem";
			this.batchToolStripMenuItem.Size = new System.Drawing.Size(150, 24);
			this.batchToolStripMenuItem.Text = "Directories";
			this.batchToolStripMenuItem.Click += new System.EventHandler(this.BatchToolStripMenuItem_Click);
			// 
			// btnConfig
			// 
			this.btnConfig.Image = global::PatternSearchUI.Properties.Resources.perm_group_system_tools;
			this.btnConfig.Location = new System.Drawing.Point(507, 5);
			this.btnConfig.Name = "btnConfig";
			this.btnConfig.Size = new System.Drawing.Size(38, 29);
			this.btnConfig.TabIndex = 14;
			this.btnConfig.UseVisualStyleBackColor = true;
			this.btnConfig.Click += new System.EventHandler(this.BtnConfig_Click);
			// 
			// contextMenuStrip2
			// 
			this.contextMenuStrip2.ImageScalingSize = new System.Drawing.Size(20, 20);
			this.contextMenuStrip2.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openFileToolStripMenuItem,
            this.openResultsToolStripMenuItem});
			this.contextMenuStrip2.Name = "contextMenuStrip2";
			this.contextMenuStrip2.Size = new System.Drawing.Size(165, 52);
			this.contextMenuStrip2.Opening += new System.ComponentModel.CancelEventHandler(this.ContextMenuStrip2_Opening);
			// 
			// openFileToolStripMenuItem
			// 
			this.openFileToolStripMenuItem.Name = "openFileToolStripMenuItem";
			this.openFileToolStripMenuItem.Size = new System.Drawing.Size(164, 24);
			this.openFileToolStripMenuItem.Text = "Open File";
			this.openFileToolStripMenuItem.Click += new System.EventHandler(this.OpenFileToolStripMenuItem_Click);
			// 
			// openResultsToolStripMenuItem
			// 
			this.openResultsToolStripMenuItem.Name = "openResultsToolStripMenuItem";
			this.openResultsToolStripMenuItem.Size = new System.Drawing.Size(164, 24);
			this.openResultsToolStripMenuItem.Text = "Open Results";
			this.openResultsToolStripMenuItem.Click += new System.EventHandler(this.OpenResultsToolStripMenuItem_Click);
			// 
			// FileProcessing
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(558, 299);
			this.Controls.Add(this.btnConfig);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.btnClear);
			this.Controls.Add(this.cbImageScan);
			this.Controls.Add(this.lstBatch);
			this.Controls.Add(this.btnStart);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "FileProcessing";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Pattern Search";
			this.Load += new System.EventHandler(this.FileProcessing_Load);
			this.contextMenuStrip1.ResumeLayout(false);
			this.contextMenuStrip2.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button btnStart;
		private System.Windows.Forms.ListView lstBatch;
		private System.Windows.Forms.ColumnHeader source;
		private System.Windows.Forms.ColumnHeader imageScan;
		private System.Windows.Forms.CheckBox cbImageScan;
		private System.Windows.Forms.Button btnClear;
		private System.Windows.Forms.ColumnHeader status;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Button btnConfig;
		private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
		private System.Windows.Forms.ToolStripMenuItem batchToolStripMenuItem;
		private System.Windows.Forms.ContextMenuStrip contextMenuStrip2;
		private System.Windows.Forms.ToolStripMenuItem openFileToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem openResultsToolStripMenuItem;
	}
}

