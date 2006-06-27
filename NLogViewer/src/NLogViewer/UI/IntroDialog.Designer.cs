namespace NLogViewer.UI
{
    partial class IntroDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(IntroDialog));
            this.label1 = new System.Windows.Forms.Label();
            this.buttonOpenLogFile = new System.Windows.Forms.Button();
            this.buttonNewLiveReceiver = new System.Windows.Forms.Button();
            this.listViewRecentFiles = new System.Windows.Forms.ListView();
            this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader2 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader3 = new System.Windows.Forms.ColumnHeader();
            this.buttonOpen = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.buttonBrowse = new System.Windows.Forms.Button();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.BackColor = System.Drawing.Color.Transparent;
            this.label1.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.label1.Location = new System.Drawing.Point(11, 27);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(186, 19);
            this.label1.TabIndex = 0;
            this.label1.Text = "Create a New Session";
            // 
            // buttonOpenLogFile
            // 
            this.buttonOpenLogFile.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(255)))));
            this.buttonOpenLogFile.FlatAppearance.BorderColor = System.Drawing.SystemColors.GradientActiveCaption;
            this.buttonOpenLogFile.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(192)))), ((int)(((byte)(255)))));
            this.buttonOpenLogFile.Image = ((System.Drawing.Image)(resources.GetObject("buttonOpenLogFile.Image")));
            this.buttonOpenLogFile.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.buttonOpenLogFile.Location = new System.Drawing.Point(3, 3);
            this.buttonOpenLogFile.Name = "buttonOpenLogFile";
            this.buttonOpenLogFile.Padding = new System.Windows.Forms.Padding(8);
            this.buttonOpenLogFile.Size = new System.Drawing.Size(208, 78);
            this.buttonOpenLogFile.TabIndex = 1;
            this.buttonOpenLogFile.Text = "Open Log &File";
            this.buttonOpenLogFile.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.buttonOpenLogFile.UseVisualStyleBackColor = false;
            // 
            // buttonNewLiveReceiver
            // 
            this.buttonNewLiveReceiver.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(255)))));
            this.buttonNewLiveReceiver.FlatAppearance.BorderColor = System.Drawing.SystemColors.GradientActiveCaption;
            this.buttonNewLiveReceiver.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(192)))), ((int)(((byte)(255)))));
            this.buttonNewLiveReceiver.Image = ((System.Drawing.Image)(resources.GetObject("buttonNewLiveReceiver.Image")));
            this.buttonNewLiveReceiver.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.buttonNewLiveReceiver.Location = new System.Drawing.Point(217, 3);
            this.buttonNewLiveReceiver.Name = "buttonNewLiveReceiver";
            this.buttonNewLiveReceiver.Padding = new System.Windows.Forms.Padding(8);
            this.buttonNewLiveReceiver.Size = new System.Drawing.Size(206, 78);
            this.buttonNewLiveReceiver.TabIndex = 2;
            this.buttonNewLiveReceiver.Text = "&Receive Live Log Events";
            this.buttonNewLiveReceiver.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.buttonNewLiveReceiver.UseVisualStyleBackColor = false;
            // 
            // listViewRecentFiles
            // 
            this.listViewRecentFiles.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.listViewRecentFiles.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(255)))));
            this.listViewRecentFiles.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3});
            this.listViewRecentFiles.FullRowSelect = true;
            this.listViewRecentFiles.HideSelection = false;
            this.listViewRecentFiles.Location = new System.Drawing.Point(16, 168);
            this.listViewRecentFiles.Name = "listViewRecentFiles";
            this.listViewRecentFiles.Size = new System.Drawing.Size(430, 177);
            this.listViewRecentFiles.TabIndex = 4;
            this.listViewRecentFiles.UseCompatibleStateImageBehavior = false;
            this.listViewRecentFiles.View = System.Windows.Forms.View.Details;
            this.listViewRecentFiles.SelectedIndexChanged += new System.EventHandler(this.listView1_SelectedIndexChanged);
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Name";
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "Session File";
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "Last Saved";
            // 
            // buttonOpen
            // 
            this.buttonOpen.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOpen.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonOpen.Location = new System.Drawing.Point(200, 351);
            this.buttonOpen.Name = "buttonOpen";
            this.buttonOpen.Size = new System.Drawing.Size(151, 23);
            this.buttonOpen.TabIndex = 5;
            this.buttonOpen.Text = "&Open Selected Session(s)";
            this.buttonOpen.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.BackColor = System.Drawing.Color.Transparent;
            this.label2.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.label2.Location = new System.Drawing.Point(12, 146);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(212, 19);
            this.label2.TabIndex = 0;
            this.label2.Text = "Open an Existing Session";
            // 
            // buttonBrowse
            // 
            this.buttonBrowse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonBrowse.Location = new System.Drawing.Point(357, 351);
            this.buttonBrowse.Name = "buttonBrowse";
            this.buttonBrowse.Size = new System.Drawing.Size(89, 23);
            this.buttonBrowse.TabIndex = 5;
            this.buttonBrowse.Text = "&Browse...";
            this.buttonBrowse.UseVisualStyleBackColor = true;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.BackColor = System.Drawing.Color.Transparent;
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.Controls.Add(this.buttonOpenLogFile, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.buttonNewLiveReceiver, 1, 0);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(16, 49);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(568, 94);
            this.tableLayoutPanel1.TabIndex = 6;
            // 
            // IntroDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
            this.Controls.Add(this.tableLayoutPanel1);
            this.Controls.Add(this.buttonBrowse);
            this.Controls.Add(this.buttonOpen);
            this.Controls.Add(this.listViewRecentFiles);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Name = "IntroDialog";
            this.Size = new System.Drawing.Size(458, 386);
            this.Load += new System.EventHandler(this.IntroDialog_Load);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        public System.Windows.Forms.Button buttonOpenLogFile;
        public System.Windows.Forms.Button buttonNewLiveReceiver;
        public System.Windows.Forms.ListView listViewRecentFiles;
        public System.Windows.Forms.Button buttonOpen;
        public System.Windows.Forms.Button buttonBrowse;
    }
}