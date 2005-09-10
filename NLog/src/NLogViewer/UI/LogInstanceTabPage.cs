using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows.Forms;

namespace NLogViewer.UI
{
	/// <summary>
	/// Summary description for LogInstanceTabPage.
	/// </summary>
	public class LogInstanceTabPage : System.Windows.Forms.UserControl
	{
        private System.Windows.Forms.StatusBar statusBar1;
        private System.Windows.Forms.Splitter splitter1;
        private System.Windows.Forms.Splitter splitter2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        public System.Windows.Forms.TreeView treeView;
        public System.Windows.Forms.ListView listViewLogMessages;
        private System.Windows.Forms.Panel panelTop;
        private System.Windows.Forms.Panel panelBody;
        private System.Windows.Forms.Panel panelSelectedItemMessage;
        private System.Windows.Forms.Panel panelSelectedItemProperties;
        private System.Windows.Forms.Splitter splitter3;
        public System.Windows.Forms.ListView listviewSelectedLogEventProperties;
        public System.Windows.Forms.TextBox textBoxSelectedMessageText;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public LogInstanceTabPage()
		{
			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();

			// TODO: Add any initialization after the InitializeComponent call

		}

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Component Designer generated code
		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            this.treeView = new System.Windows.Forms.TreeView();
            this.panelTop = new System.Windows.Forms.Panel();
            this.listViewLogMessages = new System.Windows.Forms.ListView();
            this.splitter1 = new System.Windows.Forms.Splitter();
            this.statusBar1 = new System.Windows.Forms.StatusBar();
            this.splitter2 = new System.Windows.Forms.Splitter();
            this.panelBody = new System.Windows.Forms.Panel();
            this.panelSelectedItemMessage = new System.Windows.Forms.Panel();
            this.textBoxSelectedMessageText = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.splitter3 = new System.Windows.Forms.Splitter();
            this.panelSelectedItemProperties = new System.Windows.Forms.Panel();
            this.listviewSelectedLogEventProperties = new System.Windows.Forms.ListView();
            this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader2 = new System.Windows.Forms.ColumnHeader();
            this.label1 = new System.Windows.Forms.Label();
            this.panelTop.SuspendLayout();
            this.panelBody.SuspendLayout();
            this.panelSelectedItemMessage.SuspendLayout();
            this.panelSelectedItemProperties.SuspendLayout();
            this.SuspendLayout();
            // 
            // treeView
            // 
            this.treeView.Dock = System.Windows.Forms.DockStyle.Left;
            this.treeView.ImageIndex = -1;
            this.treeView.Location = new System.Drawing.Point(0, 0);
            this.treeView.Name = "treeView";
            this.treeView.SelectedImageIndex = -1;
            this.treeView.Size = new System.Drawing.Size(240, 167);
            this.treeView.TabIndex = 0;
            // 
            // panelTop
            // 
            this.panelTop.Controls.Add(this.splitter1);
            this.panelTop.Controls.Add(this.listViewLogMessages);
            this.panelTop.Controls.Add(this.treeView);
            this.panelTop.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelTop.Location = new System.Drawing.Point(0, 0);
            this.panelTop.Name = "panelTop";
            this.panelTop.Size = new System.Drawing.Size(656, 167);
            this.panelTop.TabIndex = 1;
            // 
            // listViewLogMessages
            // 
            this.listViewLogMessages.AllowColumnReorder = true;
            this.listViewLogMessages.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listViewLogMessages.FullRowSelect = true;
            this.listViewLogMessages.GridLines = true;
            this.listViewLogMessages.Location = new System.Drawing.Point(240, 0);
            this.listViewLogMessages.MultiSelect = false;
            this.listViewLogMessages.Name = "listViewLogMessages";
            this.listViewLogMessages.Size = new System.Drawing.Size(416, 167);
            this.listViewLogMessages.TabIndex = 2;
            this.listViewLogMessages.View = System.Windows.Forms.View.Details;
            // 
            // splitter1
            // 
            this.splitter1.Location = new System.Drawing.Point(240, 0);
            this.splitter1.Name = "splitter1";
            this.splitter1.Size = new System.Drawing.Size(3, 167);
            this.splitter1.TabIndex = 1;
            this.splitter1.TabStop = false;
            // 
            // statusBar1
            // 
            this.statusBar1.Location = new System.Drawing.Point(0, 418);
            this.statusBar1.Name = "statusBar1";
            this.statusBar1.ShowPanels = true;
            this.statusBar1.Size = new System.Drawing.Size(656, 22);
            this.statusBar1.SizingGrip = false;
            this.statusBar1.TabIndex = 2;
            this.statusBar1.Text = "statusBar1";
            // 
            // splitter2
            // 
            this.splitter2.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.splitter2.Location = new System.Drawing.Point(0, 167);
            this.splitter2.Name = "splitter2";
            this.splitter2.Size = new System.Drawing.Size(656, 3);
            this.splitter2.TabIndex = 3;
            this.splitter2.TabStop = false;
            // 
            // panelBody
            // 
            this.panelBody.Controls.Add(this.panelSelectedItemMessage);
            this.panelBody.Controls.Add(this.splitter3);
            this.panelBody.Controls.Add(this.panelSelectedItemProperties);
            this.panelBody.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelBody.Location = new System.Drawing.Point(0, 170);
            this.panelBody.Name = "panelBody";
            this.panelBody.Size = new System.Drawing.Size(656, 248);
            this.panelBody.TabIndex = 4;
            // 
            // panelSelectedItemMessage
            // 
            this.panelSelectedItemMessage.Controls.Add(this.textBoxSelectedMessageText);
            this.panelSelectedItemMessage.Controls.Add(this.label2);
            this.panelSelectedItemMessage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelSelectedItemMessage.Location = new System.Drawing.Point(307, 0);
            this.panelSelectedItemMessage.Name = "panelSelectedItemMessage";
            this.panelSelectedItemMessage.Size = new System.Drawing.Size(349, 248);
            this.panelSelectedItemMessage.TabIndex = 1;
            // 
            // textBoxSelectedMessageText
            // 
            this.textBoxSelectedMessageText.BackColor = System.Drawing.SystemColors.Window;
            this.textBoxSelectedMessageText.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBoxSelectedMessageText.Location = new System.Drawing.Point(0, 23);
            this.textBoxSelectedMessageText.Multiline = true;
            this.textBoxSelectedMessageText.Name = "textBoxSelectedMessageText";
            this.textBoxSelectedMessageText.ReadOnly = true;
            this.textBoxSelectedMessageText.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBoxSelectedMessageText.Size = new System.Drawing.Size(349, 225);
            this.textBoxSelectedMessageText.TabIndex = 3;
            this.textBoxSelectedMessageText.Text = "";
            this.textBoxSelectedMessageText.WordWrap = false;
            // 
            // label2
            // 
            this.label2.Dock = System.Windows.Forms.DockStyle.Top;
            this.label2.Location = new System.Drawing.Point(0, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(349, 23);
            this.label2.TabIndex = 2;
            this.label2.Text = "Selected Log Event Message Text:";
            this.label2.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
            // 
            // splitter3
            // 
            this.splitter3.Location = new System.Drawing.Point(304, 0);
            this.splitter3.Name = "splitter3";
            this.splitter3.Size = new System.Drawing.Size(3, 248);
            this.splitter3.TabIndex = 2;
            this.splitter3.TabStop = false;
            // 
            // panelSelectedItemProperties
            // 
            this.panelSelectedItemProperties.Controls.Add(this.listviewSelectedLogEventProperties);
            this.panelSelectedItemProperties.Controls.Add(this.label1);
            this.panelSelectedItemProperties.Dock = System.Windows.Forms.DockStyle.Left;
            this.panelSelectedItemProperties.Location = new System.Drawing.Point(0, 0);
            this.panelSelectedItemProperties.Name = "panelSelectedItemProperties";
            this.panelSelectedItemProperties.Size = new System.Drawing.Size(304, 248);
            this.panelSelectedItemProperties.TabIndex = 0;
            // 
            // listviewSelectedLogEventProperties
            // 
            this.listviewSelectedLogEventProperties.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
                                                                                                                 this.columnHeader1,
                                                                                                                 this.columnHeader2});
            this.listviewSelectedLogEventProperties.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listviewSelectedLogEventProperties.FullRowSelect = true;
            this.listviewSelectedLogEventProperties.GridLines = true;
            this.listviewSelectedLogEventProperties.Location = new System.Drawing.Point(0, 23);
            this.listviewSelectedLogEventProperties.Name = "listviewSelectedLogEventProperties";
            this.listviewSelectedLogEventProperties.Size = new System.Drawing.Size(304, 225);
            this.listviewSelectedLogEventProperties.TabIndex = 0;
            this.listviewSelectedLogEventProperties.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Name";
            this.columnHeader1.Width = 100;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "Value";
            this.columnHeader2.Width = 500;
            // 
            // label1
            // 
            this.label1.Dock = System.Windows.Forms.DockStyle.Top;
            this.label1.Location = new System.Drawing.Point(0, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(304, 23);
            this.label1.TabIndex = 1;
            this.label1.Text = "Selected Log Event Properties:";
            this.label1.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
            // 
            // LogInstanceTabPage
            // 
            this.Controls.Add(this.panelTop);
            this.Controls.Add(this.splitter2);
            this.Controls.Add(this.panelBody);
            this.Controls.Add(this.statusBar1);
            this.Name = "LogInstanceTabPage";
            this.Size = new System.Drawing.Size(656, 440);
            this.panelTop.ResumeLayout(false);
            this.panelBody.ResumeLayout(false);
            this.panelSelectedItemMessage.ResumeLayout(false);
            this.panelSelectedItemProperties.ResumeLayout(false);
            this.ResumeLayout(false);

        }
		#endregion
	}
}
