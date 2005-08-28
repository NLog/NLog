// 
// Copyright (c) 2004,2005 Jaroslaw Kowalski <jkowalski@users.sourceforge.net>
// 
// 
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without 
// modification, are permitted provided that the following conditions 
// are met:
// 
// * Redistributions of source code must retain the above copyright notice, 
//   this list of conditions and the following disclaimer. 
// 
// * Redistributions in binary form must reproduce the above copyright notice,
//   this list of conditions and the following disclaimer in the documentation
//   and/or other materials provided with the distribution. 
// 
// * Neither the name of the Jaroslaw Kowalski nor the names of its 
//   contributors may be used to endorse or promote products derived from this
//   software without specific prior written permission. 
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE 
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE 
// ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE 
// LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR 
// CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
// SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS 
// INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN 
// CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) 
// ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF 
// THE POSSIBILITY OF SUCH DAMAGE.
// 

using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.IO;
using System.Xml.Serialization;
using System.Xml;

using NLog.Viewer.UI;
using NLog.Viewer.Configuration;

namespace NLog.Viewer.UI
{
	/// <summary>
	/// Summary description for Form1.
	/// </summary>
	public class MainForm : System.Windows.Forms.Form
	{
        private System.Windows.Forms.Splitter splitter1;
        private System.Windows.Forms.Splitter splitter2;
        private System.Windows.Forms.MainMenu mainMenu1;
        private System.Windows.Forms.MenuItem menuItemFileExit;
        private System.Windows.Forms.MenuItem menuItemHelpAbout;
        private System.Windows.Forms.MenuItem menuItemFile;
        private System.Windows.Forms.MenuItem menuItemHelp;
        private System.Windows.Forms.ToolBar toolBar1;
        private System.Windows.Forms.ToolBarButton toolBarButton1;
        private System.Windows.Forms.ImageList imageList1;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.ToolBarButton toolBarButton2;
        private System.Windows.Forms.MenuItem menuItem3;
        private System.Windows.Forms.MenuItem menuItem6;
        private System.Windows.Forms.MenuItem menuItem2;
        private System.Windows.Forms.MenuItem menuItem5;
        private System.Windows.Forms.MenuItem menuItem10;
        private System.Windows.Forms.MenuItem menuItem11;
        private System.Windows.Forms.MenuItem menuItem12;
        private System.Windows.Forms.MenuItem menuItem13;
        private System.ComponentModel.IContainer components;
        private System.Windows.Forms.ImageList imageList2;
        public System.Windows.Forms.ContextMenu treeContextMenu;
        private System.Windows.Forms.MenuItem menuItem14;
        private System.Windows.Forms.MenuItem menuItem16;
        private System.Windows.Forms.MenuItem menuItem17;
        private System.Windows.Forms.MenuItem menuItem18;
        private System.Windows.Forms.MenuItem menuItem19;
        private System.Windows.Forms.MenuItem menuItem20;
        private System.Windows.Forms.MenuItem menuItem26;
        private System.Windows.Forms.MenuItem menuItem27;
        private System.Windows.Forms.MenuItem menuItem15;
        private System.Windows.Forms.MenuItem menuItem21;
        private System.Windows.Forms.MenuItem menuItem22;
        private System.Windows.Forms.MenuItem menuItem23;
        private System.Windows.Forms.MenuItem menuItem24;
        private System.Windows.Forms.MenuItem menuItemManageLogs;
        private System.Windows.Forms.MenuItem menuItem1;
        private System.Windows.Forms.MenuItem menuItemNewLog;
        private System.Windows.Forms.MenuItem menuItem7;
        private System.Windows.Forms.MenuItem menuItem9;

        private string _baseConfigurationPath;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.MenuItem menuItemViewStatusMessages;
        private System.Windows.Forms.MenuItem menuItemLogEventDetails;
        private System.Windows.Forms.TextBox textBoxLog;
        private System.Windows.Forms.Splitter splitter3;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.TabPage tabPageNLogViewerTrace;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.TextBox textBoxSelectedMessageText;
        private System.Windows.Forms.ListView listviewSelectedLogEventProperties;
        private System.Windows.Forms.MenuItem menuItemFilterHighlight;
        private LogInstanceCollection _instances = new LogInstanceCollection();

		public MainForm()
		{
            _baseConfigurationPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "NLogViewer");
			InitializeComponent();
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if (components != null) 
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            this.components = new System.ComponentModel.Container();
            System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(MainForm));
            this.splitter1 = new System.Windows.Forms.Splitter();
            this.splitter2 = new System.Windows.Forms.Splitter();
            this.mainMenu1 = new System.Windows.Forms.MainMenu();
            this.menuItemFile = new System.Windows.Forms.MenuItem();
            this.menuItemNewLog = new System.Windows.Forms.MenuItem();
            this.menuItem10 = new System.Windows.Forms.MenuItem();
            this.menuItem3 = new System.Windows.Forms.MenuItem();
            this.menuItemManageLogs = new System.Windows.Forms.MenuItem();
            this.menuItem1 = new System.Windows.Forms.MenuItem();
            this.menuItemFileExit = new System.Windows.Forms.MenuItem();
            this.menuItem9 = new System.Windows.Forms.MenuItem();
            this.menuItemLogEventDetails = new System.Windows.Forms.MenuItem();
            this.menuItemViewStatusMessages = new System.Windows.Forms.MenuItem();
            this.menuItem6 = new System.Windows.Forms.MenuItem();
            this.menuItem12 = new System.Windows.Forms.MenuItem();
            this.menuItem13 = new System.Windows.Forms.MenuItem();
            this.menuItem11 = new System.Windows.Forms.MenuItem();
            this.menuItem2 = new System.Windows.Forms.MenuItem();
            this.menuItem5 = new System.Windows.Forms.MenuItem();
            this.menuItemFilterHighlight = new System.Windows.Forms.MenuItem();
            this.menuItemHelp = new System.Windows.Forms.MenuItem();
            this.menuItemHelpAbout = new System.Windows.Forms.MenuItem();
            this.toolBar1 = new System.Windows.Forms.ToolBar();
            this.toolBarButton1 = new System.Windows.Forms.ToolBarButton();
            this.toolBarButton2 = new System.Windows.Forms.ToolBarButton();
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPageNLogViewerTrace = new System.Windows.Forms.TabPage();
            this.textBoxLog = new System.Windows.Forms.TextBox();
            this.imageList2 = new System.Windows.Forms.ImageList(this.components);
            this.treeContextMenu = new System.Windows.Forms.ContextMenu();
            this.menuItem14 = new System.Windows.Forms.MenuItem();
            this.menuItem27 = new System.Windows.Forms.MenuItem();
            this.menuItem21 = new System.Windows.Forms.MenuItem();
            this.menuItem7 = new System.Windows.Forms.MenuItem();
            this.menuItem16 = new System.Windows.Forms.MenuItem();
            this.menuItem17 = new System.Windows.Forms.MenuItem();
            this.menuItem18 = new System.Windows.Forms.MenuItem();
            this.menuItem19 = new System.Windows.Forms.MenuItem();
            this.menuItem20 = new System.Windows.Forms.MenuItem();
            this.menuItem15 = new System.Windows.Forms.MenuItem();
            this.menuItem26 = new System.Windows.Forms.MenuItem();
            this.menuItem22 = new System.Windows.Forms.MenuItem();
            this.menuItem23 = new System.Windows.Forms.MenuItem();
            this.menuItem24 = new System.Windows.Forms.MenuItem();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.splitter3 = new System.Windows.Forms.Splitter();
            this.textBoxSelectedMessageText = new System.Windows.Forms.TextBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.label1 = new System.Windows.Forms.Label();
            this.listviewSelectedLogEventProperties = new System.Windows.Forms.ListView();
            this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader2 = new System.Windows.Forms.ColumnHeader();
            this.label2 = new System.Windows.Forms.Label();
            this.tabControl1.SuspendLayout();
            this.tabPageNLogViewerTrace.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitter1
            // 
            this.splitter1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.splitter1.Location = new System.Drawing.Point(0, 242);
            this.splitter1.Name = "splitter1";
            this.splitter1.Size = new System.Drawing.Size(742, 3);
            this.splitter1.TabIndex = 2;
            this.splitter1.TabStop = false;
            // 
            // splitter2
            // 
            this.splitter2.Location = new System.Drawing.Point(0, 32);
            this.splitter2.Name = "splitter2";
            this.splitter2.Size = new System.Drawing.Size(3, 210);
            this.splitter2.TabIndex = 4;
            this.splitter2.TabStop = false;
            // 
            // mainMenu1
            // 
            this.mainMenu1.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
                                                                                      this.menuItemFile,
                                                                                      this.menuItem9,
                                                                                      this.menuItem6,
                                                                                      this.menuItemHelp});
            // 
            // menuItemFile
            // 
            this.menuItemFile.Index = 0;
            this.menuItemFile.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
                                                                                         this.menuItemNewLog,
                                                                                         this.menuItem10,
                                                                                         this.menuItem3,
                                                                                         this.menuItemManageLogs,
                                                                                         this.menuItem1,
                                                                                         this.menuItemFileExit});
            this.menuItemFile.Text = "&File";
            // 
            // menuItemNewLog
            // 
            this.menuItemNewLog.Index = 0;
            this.menuItemNewLog.Text = "&New...\tCtrl-N";
            this.menuItemNewLog.Click += new System.EventHandler(this.menuItemNewLog_Click);
            // 
            // menuItem10
            // 
            this.menuItem10.Index = 1;
            this.menuItem10.Text = "C&lose";
            // 
            // menuItem3
            // 
            this.menuItem3.Index = 2;
            this.menuItem3.Text = "-";
            // 
            // menuItemManageLogs
            // 
            this.menuItemManageLogs.Index = 3;
            this.menuItemManageLogs.Text = "Manage logs...";
            // 
            // menuItem1
            // 
            this.menuItem1.Index = 4;
            this.menuItem1.Text = "-";
            // 
            // menuItemFileExit
            // 
            this.menuItemFileExit.Index = 5;
            this.menuItemFileExit.Text = "&Exit";
            this.menuItemFileExit.Click += new System.EventHandler(this.menuItemFileExit_Click);
            // 
            // menuItem9
            // 
            this.menuItem9.Index = 1;
            this.menuItem9.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
                                                                                      this.menuItemLogEventDetails,
                                                                                      this.menuItemViewStatusMessages});
            this.menuItem9.Text = "&View";
            // 
            // menuItemLogEventDetails
            // 
            this.menuItemLogEventDetails.Index = 0;
            this.menuItemLogEventDetails.Text = "Log Event Details";
            // 
            // menuItemViewStatusMessages
            // 
            this.menuItemViewStatusMessages.Index = 1;
            this.menuItemViewStatusMessages.Text = "Status Messages";
            // 
            // menuItem6
            // 
            this.menuItem6.Index = 2;
            this.menuItem6.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
                                                                                      this.menuItem12,
                                                                                      this.menuItem13,
                                                                                      this.menuItem11,
                                                                                      this.menuItem2,
                                                                                      this.menuItem5,
                                                                                      this.menuItemFilterHighlight});
            this.menuItem6.Text = "&Log";
            // 
            // menuItem12
            // 
            this.menuItem12.Index = 0;
            this.menuItem12.Text = "&Enabled";
            // 
            // menuItem13
            // 
            this.menuItem13.Index = 1;
            this.menuItem13.Text = "-";
            // 
            // menuItem11
            // 
            this.menuItem11.Index = 2;
            this.menuItem11.Text = "&Clear";
            // 
            // menuItem2
            // 
            this.menuItem2.Index = 3;
            this.menuItem2.Text = "&Save log to a file...";
            // 
            // menuItem5
            // 
            this.menuItem5.Index = 4;
            this.menuItem5.Text = "-";
            // 
            // menuItemFilterHighlight
            // 
            this.menuItemFilterHighlight.Index = 5;
            this.menuItemFilterHighlight.Text = "&Filter / Highlight...";
            this.menuItemFilterHighlight.Click += new System.EventHandler(this.menuItemFilterHighlight_Click);
            // 
            // menuItemHelp
            // 
            this.menuItemHelp.Index = 3;
            this.menuItemHelp.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
                                                                                         this.menuItemHelpAbout});
            this.menuItemHelp.Text = "&Help";
            // 
            // menuItemHelpAbout
            // 
            this.menuItemHelpAbout.Index = 0;
            this.menuItemHelpAbout.Text = "&About...";
            // 
            // toolBar1
            // 
            this.toolBar1.Appearance = System.Windows.Forms.ToolBarAppearance.Flat;
            this.toolBar1.Buttons.AddRange(new System.Windows.Forms.ToolBarButton[] {
                                                                                        this.toolBarButton1,
                                                                                        this.toolBarButton2});
            this.toolBar1.DropDownArrows = true;
            this.toolBar1.ImageList = this.imageList1;
            this.toolBar1.Location = new System.Drawing.Point(0, 0);
            this.toolBar1.Name = "toolBar1";
            this.toolBar1.ShowToolTips = true;
            this.toolBar1.Size = new System.Drawing.Size(742, 32);
            this.toolBar1.TabIndex = 5;
            // 
            // toolBarButton1
            // 
            this.toolBarButton1.ImageIndex = 0;
            // 
            // toolBarButton2
            // 
            this.toolBarButton2.ImageIndex = 2;
            // 
            // imageList1
            // 
            this.imageList1.ImageSize = new System.Drawing.Size(21, 20);
            this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPageNLogViewerTrace);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.HotTrack = true;
            this.tabControl1.ImageList = this.imageList1;
            this.tabControl1.Location = new System.Drawing.Point(3, 32);
            this.tabControl1.Multiline = true;
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(739, 210);
            this.tabControl1.TabIndex = 6;
            // 
            // tabPageNLogViewerTrace
            // 
            this.tabPageNLogViewerTrace.Controls.Add(this.textBoxLog);
            this.tabPageNLogViewerTrace.Location = new System.Drawing.Point(4, 27);
            this.tabPageNLogViewerTrace.Name = "tabPageNLogViewerTrace";
            this.tabPageNLogViewerTrace.Size = new System.Drawing.Size(731, 179);
            this.tabPageNLogViewerTrace.TabIndex = 0;
            this.tabPageNLogViewerTrace.Text = "NLog Viewer Trace";
            // 
            // textBoxLog
            // 
            this.textBoxLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBoxLog.Location = new System.Drawing.Point(0, 0);
            this.textBoxLog.Multiline = true;
            this.textBoxLog.Name = "textBoxLog";
            this.textBoxLog.ReadOnly = true;
            this.textBoxLog.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBoxLog.Size = new System.Drawing.Size(731, 179);
            this.textBoxLog.TabIndex = 1;
            this.textBoxLog.Text = "";
            // 
            // imageList2
            // 
            this.imageList2.ImageSize = new System.Drawing.Size(16, 16);
            this.imageList2.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // treeContextMenu
            // 
            this.treeContextMenu.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
                                                                                            this.menuItem14,
                                                                                            this.menuItem22,
                                                                                            this.menuItem23,
                                                                                            this.menuItem24});
            // 
            // menuItem14
            // 
            this.menuItem14.Index = 0;
            this.menuItem14.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
                                                                                       this.menuItem27,
                                                                                       this.menuItem21,
                                                                                       this.menuItem7,
                                                                                       this.menuItem16,
                                                                                       this.menuItem17,
                                                                                       this.menuItem18,
                                                                                       this.menuItem19,
                                                                                       this.menuItem20,
                                                                                       this.menuItem15,
                                                                                       this.menuItem26});
            this.menuItem14.Text = "Log &Level";
            // 
            // menuItem27
            // 
            this.menuItem27.Index = 0;
            this.menuItem27.Text = "None";
            // 
            // menuItem21
            // 
            this.menuItem21.Index = 1;
            this.menuItem21.Text = "-";
            // 
            // menuItem7
            // 
            this.menuItem7.Index = 2;
            this.menuItem7.Text = "Trace";
            // 
            // menuItem16
            // 
            this.menuItem16.Index = 3;
            this.menuItem16.Text = "&Debug";
            // 
            // menuItem17
            // 
            this.menuItem17.Index = 4;
            this.menuItem17.Text = "&Info";
            // 
            // menuItem18
            // 
            this.menuItem18.Index = 5;
            this.menuItem18.Text = "&Warn";
            // 
            // menuItem19
            // 
            this.menuItem19.Index = 6;
            this.menuItem19.Text = "&Error";
            // 
            // menuItem20
            // 
            this.menuItem20.Index = 7;
            this.menuItem20.Text = "&Fatal";
            // 
            // menuItem15
            // 
            this.menuItem15.Index = 8;
            this.menuItem15.Text = "-";
            // 
            // menuItem26
            // 
            this.menuItem26.Index = 9;
            this.menuItem26.Text = "&Custom...";
            // 
            // menuItem22
            // 
            this.menuItem22.Index = 1;
            this.menuItem22.Text = "&Highlight...";
            // 
            // menuItem23
            // 
            this.menuItem23.Index = 2;
            this.menuItem23.Text = "-";
            // 
            // menuItem24
            // 
            this.menuItem24.Index = 3;
            this.menuItem24.Text = "&Reset do defaults";
            // 
            // timer1
            // 
            this.timer1.Enabled = true;
            this.timer1.Interval = 500;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // splitter3
            // 
            this.splitter3.Dock = System.Windows.Forms.DockStyle.Right;
            this.splitter3.Location = new System.Drawing.Point(739, 0);
            this.splitter3.Name = "splitter3";
            this.splitter3.Size = new System.Drawing.Size(3, 250);
            this.splitter3.TabIndex = 1;
            this.splitter3.TabStop = false;
            // 
            // textBoxSelectedMessageText
            // 
            this.textBoxSelectedMessageText.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
                | System.Windows.Forms.AnchorStyles.Left) 
                | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxSelectedMessageText.Location = new System.Drawing.Point(456, 24);
            this.textBoxSelectedMessageText.Multiline = true;
            this.textBoxSelectedMessageText.Name = "textBoxSelectedMessageText";
            this.textBoxSelectedMessageText.ReadOnly = true;
            this.textBoxSelectedMessageText.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBoxSelectedMessageText.Size = new System.Drawing.Size(272, 218);
            this.textBoxSelectedMessageText.TabIndex = 0;
            this.textBoxSelectedMessageText.Text = "";
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.label1);
            this.panel1.Controls.Add(this.listviewSelectedLogEventProperties);
            this.panel1.Controls.Add(this.splitter3);
            this.panel1.Controls.Add(this.textBoxSelectedMessageText);
            this.panel1.Controls.Add(this.label2);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(0, 245);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(742, 250);
            this.panel1.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(8, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(440, 23);
            this.label1.TabIndex = 3;
            this.label1.Text = "Selected Log Event Properties:";
            this.label1.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
            // 
            // listviewSelectedLogEventProperties
            // 
            this.listviewSelectedLogEventProperties.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
                | System.Windows.Forms.AnchorStyles.Left)));
            this.listviewSelectedLogEventProperties.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
                                                                                                                 this.columnHeader1,
                                                                                                                 this.columnHeader2});
            this.listviewSelectedLogEventProperties.FullRowSelect = true;
            this.listviewSelectedLogEventProperties.GridLines = true;
            this.listviewSelectedLogEventProperties.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.listviewSelectedLogEventProperties.Location = new System.Drawing.Point(8, 24);
            this.listviewSelectedLogEventProperties.Name = "listviewSelectedLogEventProperties";
            this.listviewSelectedLogEventProperties.Size = new System.Drawing.Size(440, 218);
            this.listviewSelectedLogEventProperties.TabIndex = 2;
            this.listviewSelectedLogEventProperties.View = System.Windows.Forms.View.Details;
            this.listviewSelectedLogEventProperties.SelectedIndexChanged += new System.EventHandler(this.listView1_SelectedIndexChanged);
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
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(456, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(176, 23);
            this.label2.TabIndex = 3;
            this.label2.Text = "Message Text:";
            this.label2.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
            // 
            // MainForm
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(742, 495);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.splitter2);
            this.Controls.Add(this.splitter1);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.toolBar1);
            this.Menu = this.mainMenu1;
            this.Name = "MainForm";
            this.Text = "NLog Viewer";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.tabControl1.ResumeLayout(false);
            this.tabPageNLogViewerTrace.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }
		#endregion

        private void menuItemFileExit_Click(object sender, System.EventArgs e)
        {
            this.Close();
        }

        private void MainForm_Load(object sender, System.EventArgs e)
        {
            Log.SetTargetForm(this);
            LoadLogs();
        }

        public void LogWrite(string s, params object[] p)
        {
            string s2 = String.Format(s, p);
            textBoxLog.AppendText(s2 + "\r\n");
        }

        private void LoadLogs()
        {
            LoadLogs(GetLogsDirectory());
        }

        private void LoadLogs(string logsDir)
        {
            if (!Directory.Exists(logsDir))
                Directory.CreateDirectory(logsDir);

            XmlSerializer serializer = new XmlSerializer(typeof(LogInstanceConfiguration));

            Log.Write("Looking for '*.loginstance' files in '{0}'", logsDir);

            foreach (string logFile in Directory.GetFiles(logsDir, "*.loginstance"))
            {
                Log.Write("Found {0}", logFile);
                try
                {
                    LogInstanceConfiguration logInstanceConfig = LogInstanceConfiguration.Load(logFile);
                    LogInstance instance = new LogInstance(logInstanceConfig);
                    instance.CreateTab(this);

                    instance.Start();
                    _instances.Add(instance);
                }
                catch (Exception ex)
                {
                    Log.Write("Unable to read log instance configuration from {0}: {1}", logFile, ex.ToString());
                }
            }

            CreateInstanceFileName(logsDir);
            if (_instances.Count == 0)
            {
                Log.Write("No instance found. Creating default log on UDP port 4000");

                LogInstanceConfiguration lici = new LogInstanceConfiguration();
                lici.Name = "udp://localhost:4000";
                lici.ReceiverType = "UDP";
                lici.ReceiverParameters.Add(new ReceiverParameter("port", "4000"));
                lici.FileName = CreateInstanceFileName(logsDir);
                lici.Save();

                LogInstance instance = new LogInstance(lici);
                instance.CreateTab(this);
                instance.Start();
                _instances.Add(instance);

                lici = new LogInstanceConfiguration();
                lici.Name = "tcp://localhost:4001";
                lici.ReceiverType = "TCP";
                lici.ReceiverParameters.Add(new ReceiverParameter("port", "4001"));
                lici.FileName = CreateInstanceFileName(logsDir);
                lici.Save();

                instance = new LogInstance(lici);
                instance.CreateTab(this);
                _instances.Add(instance);
            }
            else
            {
                Log.Write("Found {0} instances.", _instances.Count);
            }
            ReloadTabPages();
        }

        private void ReloadTabPages()
        {
            this.tabControl1.TabPages.Clear();
            foreach (LogInstance i in _instances)
            {
                this.tabControl1.TabPages.Add(i.TabPage);
            }
            this.tabControl1.TabPages.Add(tabPageNLogViewerTrace);
        }

        private string GetLogsDirectory()
        {
            return Path.Combine(_baseConfigurationPath, "Logs");
        }

        private void menuItemNewLog_Click(object sender, System.EventArgs e)
        {
            using (CreateLogStep1 s1 = new CreateLogStep1())
            {
                s1.ShowDialog(this);
            }
        }

        private string CreateInstanceFileName(string baseDir)
        {
            for (int i = 0; i < 10000; ++i)
            {
                string fileName = String.Format("{0:0000}", i);
                string fullName = Path.Combine(baseDir, fileName) + ".loginstance";
                if (!File.Exists(fullName))
                    return fullName;
            }
            return Path.Combine(baseDir, Guid.NewGuid().ToString("N"));
        }

        private void timer1_Tick(object sender, System.EventArgs e)
        {
            for (int i = 0; i < _instances.Count; ++i)
            {
                _instances[i].OnTimer();
            }
        }

        private void listView1_SelectedIndexChanged(object sender, System.EventArgs e)
        {
        
        }

        private LogEvent _selectedLogEvent = null;

        public void LogEventSelected(LogInstance instance, LogEvent evt)
        {
            if (evt != _selectedLogEvent)
            {
                _selectedLogEvent = evt;

                textBoxSelectedMessageText.Text = evt.MessageText;
                ListViewItem item;

                listviewSelectedLogEventProperties.Items.Clear(); 

                item = new ListViewItem(new string[] { "Sent Time", evt.SentTime.ToString() });
                listviewSelectedLogEventProperties.Items.Add(item);

                item = new ListViewItem(new string[] { "Received Time", evt.ReceivedTime.ToString() });
                listviewSelectedLogEventProperties.Items.Add(item);

                item = new ListViewItem(new string[] { "Logger", evt.Logger });
                listviewSelectedLogEventProperties.Items.Add(item);

                item = new ListViewItem(new string[] { "Level", evt.Level });
                listviewSelectedLogEventProperties.Items.Add(item);

                item = new ListViewItem(new string[] { "Application", evt.SourceApplication });
                listviewSelectedLogEventProperties.Items.Add(item);

                item = new ListViewItem(new string[] { "Thread", evt.Thread });
                listviewSelectedLogEventProperties.Items.Add(item);

                item = new ListViewItem(new string[] { "Assembly", evt.SourceAssembly });
                listviewSelectedLogEventProperties.Items.Add(item);

                item = new ListViewItem(new string[] { "Class", evt.SourceType });
                listviewSelectedLogEventProperties.Items.Add(item);

                item = new ListViewItem(new string[] { "Method", evt.SourceMethod });
                listviewSelectedLogEventProperties.Items.Add(item);

                item = new ListViewItem(new string[] { "Source File", evt.SourceFile });
                listviewSelectedLogEventProperties.Items.Add(item);

                item = new ListViewItem(new string[] { "Source Line", evt.SourceLine.ToString() });
                listviewSelectedLogEventProperties.Items.Add(item);

                item = new ListViewItem(new string[] { "Source Column", evt.SourceColumn.ToString() });
                listviewSelectedLogEventProperties.Items.Add(item);

                item = new ListViewItem(new string[] { "Machine", evt.SourceMachine });
                listviewSelectedLogEventProperties.Items.Add(item);

                item = new ListViewItem(new string[] { "Stack Trace", evt.StackTrace });
                listviewSelectedLogEventProperties.Items.Add(item);

                foreach (LogEventProperty lep in evt.Properties)
                {
                    item = new ListViewItem(new string[] { lep.Name, lep.Value });
                    listviewSelectedLogEventProperties.Items.Add(item);
                }
            }
        }

        private void menuItemFilterHighlight_Click(object sender, System.EventArgs e)
        {
            using (FilterHighlight dlg = new FilterHighlight())
            {
                dlg.ShowDialog(this);
            }
        }
	}
}
