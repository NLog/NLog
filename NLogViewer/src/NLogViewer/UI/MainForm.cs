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

using NLogViewer.UI;
using NLogViewer.Configuration;
using System.Collections.Generic;

namespace NLogViewer.UI
{
	/// <summary>
	/// Summary description for Form1.
	/// </summary>
	public class MainForm : System.Windows.Forms.Form
    {
        private System.Windows.Forms.TabControl tabControl1;
        private System.ComponentModel.IContainer components;

        private string _baseConfigurationPath;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.TextBox textBoxLog;
        private System.Windows.Forms.TabPage tabPageNLogViewerTrace;
        private MenuStrip menuStrip1;
        private ToolStrip toolStrip1;
        private Panel panel1;
        private ToolStripButton toolStripButtonOpen;
        private ToolStripButton toolStripButton3;
        private ToolStripContainer toolStripContainer1;
        private ToolStripButton toolStripButtonNewLiveLogReceiver;
        private ToolStripSeparator toolStripSeparator3;
        private ToolStrip toolStrip2;
        private ToolStripButton toolStripButtonStop;
        private ToolStripButton toolStripButtonRecord;
        private ToolStripButton toolStripButtonRefresh;
        private ToolStripButton toolStripButtonClear;
        private ToolStripSplitButton toolStripButtonFilter;
        private ToolStripSplitButton toolStripButtonHighlight;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripButton toolStripButtonFind;
        private ToolStripButton toolStripButtonFindNext;
        private ToolStripTextBox toolStripTextBoxFindText;
        private ToolStripLabel toolStripLabelStatus0;
        private ToolStripLabel toolStripLabelStatus;
        private ToolStripSeparator toolStripSeparator4;
        private ToolStripMenuItem viewToolStripMenuItem;
        private ToolStripMenuItem alwaysOnTopToolStripMenuItem;
        private NotifyIcon notifyIcon1;
        private ImageList imageList1;
        private ToolStripButton toolStripButtonZoomIn;
        private ToolStripButton toolStripButtonZoomOut;
        private ToolStripButton toolStripButtonChooseFont;
        private ToolStripSeparator toolStripSeparator2;
        private ToolStripButton toolStripButtonFitColumnWidths;
        private ToolStripButton toolStripButtonChooseColumns;
        private ToolStripMenuItem fileToolStripMenuItem;
        private ToolStripMenuItem toolStripMenuItem1;
        private ToolStripMenuItem toolStripMenuItemNewSessionLogReceiver;
        private ToolStripMenuItem toolStripMenuItemNewSessionLogFile;
        private ToolStripMenuItem toolStripMenuItemOpenSession;
        private ToolStripMenuItem toolStripMenuItemSaveSession;
        private ToolStripMenuItem toolStripMenuItemSaveSessionAs;
        private ToolStripSeparator toolStripMenuItem3;
        private ToolStripMenuItem toolStripMenuItem2;
        private ToolStripSeparator toolStripSeparator5;
        private ToolStripMenuItem toolStripMenuItemRecentSessions;
        private List<Session> _sessions = new List<Session>();

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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPageNLogViewerTrace = new System.Windows.Forms.TabPage();
            this.textBoxLog = new System.Windows.Forms.TextBox();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemNewSessionLogReceiver = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemNewSessionLogFile = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemOpenSession = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemSaveSession = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemSaveSessionAs = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripMenuItemRecentSessions = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem3 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
            this.viewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.alwaysOnTopToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.toolStripButtonNewLiveLogReceiver = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonOpen = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton3 = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.panel1 = new System.Windows.Forms.Panel();
            this.toolStripContainer1 = new System.Windows.Forms.ToolStripContainer();
            this.toolStrip2 = new System.Windows.Forms.ToolStrip();
            this.toolStripLabelStatus0 = new System.Windows.Forms.ToolStripLabel();
            this.toolStripLabelStatus = new System.Windows.Forms.ToolStripLabel();
            this.toolStripButtonRecord = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonStop = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonRefresh = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripButtonClear = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonFilter = new System.Windows.Forms.ToolStripSplitButton();
            this.toolStripButtonHighlight = new System.Windows.Forms.ToolStripSplitButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripButtonFind = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonFindNext = new System.Windows.Forms.ToolStripButton();
            this.toolStripTextBoxFindText = new System.Windows.Forms.ToolStripTextBox();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripButtonChooseFont = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonZoomIn = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonZoomOut = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonFitColumnWidths = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonChooseColumns = new System.Windows.Forms.ToolStripButton();
            this.notifyIcon1 = new System.Windows.Forms.NotifyIcon(this.components);
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.tabControl1.SuspendLayout();
            this.tabPageNLogViewerTrace.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.toolStripContainer1.ContentPanel.SuspendLayout();
            this.toolStripContainer1.TopToolStripPanel.SuspendLayout();
            this.toolStripContainer1.SuspendLayout();
            this.toolStrip2.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPageNLogViewerTrace);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.HotTrack = true;
            this.tabControl1.Location = new System.Drawing.Point(2, 2);
            this.tabControl1.Multiline = true;
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.Padding = new System.Drawing.Point(6, 6);
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(688, 413);
            this.tabControl1.TabIndex = 6;
            this.tabControl1.SelectedIndexChanged += new System.EventHandler(this.tabControl1_SelectedIndexChanged);
            // 
            // tabPageNLogViewerTrace
            // 
            this.tabPageNLogViewerTrace.Controls.Add(this.textBoxLog);
            this.tabPageNLogViewerTrace.Location = new System.Drawing.Point(4, 28);
            this.tabPageNLogViewerTrace.Name = "tabPageNLogViewerTrace";
            this.tabPageNLogViewerTrace.Size = new System.Drawing.Size(680, 381);
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
            this.textBoxLog.Size = new System.Drawing.Size(680, 381);
            this.textBoxLog.TabIndex = 1;
            // 
            // timer1
            // 
            this.timer1.Enabled = true;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // menuStrip1
            // 
            this.menuStrip1.Dock = System.Windows.Forms.DockStyle.None;
            this.menuStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Visible;
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.viewToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.menuStrip1.Size = new System.Drawing.Size(692, 24);
            this.menuStrip1.TabIndex = 7;
            this.menuStrip1.Text = "menuStrip1";
            this.menuStrip1.MenuActivate += new System.EventHandler(this.menuStrip1_MenuActivate);
            this.menuStrip1.ItemClicked += new System.Windows.Forms.ToolStripItemClickedEventHandler(this.menuStrip1_ItemClicked);
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem1,
            this.toolStripMenuItemOpenSession,
            this.toolStripMenuItemSaveSession,
            this.toolStripMenuItemSaveSessionAs,
            this.toolStripSeparator5,
            this.toolStripMenuItemRecentSessions,
            this.toolStripMenuItem3,
            this.toolStripMenuItem2});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(55, 20);
            this.fileToolStripMenuItem.Text = "&Session";
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItemNewSessionLogReceiver,
            this.toolStripMenuItemNewSessionLogFile});
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(202, 22);
            this.toolStripMenuItem1.Text = "&New Session";
            // 
            // toolStripMenuItemNewSessionLogReceiver
            // 
            this.toolStripMenuItemNewSessionLogReceiver.Name = "toolStripMenuItemNewSessionLogReceiver";
            this.toolStripMenuItemNewSessionLogReceiver.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.L)));
            this.toolStripMenuItemNewSessionLogReceiver.Size = new System.Drawing.Size(218, 22);
            this.toolStripMenuItemNewSessionLogReceiver.Text = "&Live Log Receiver...";
            // 
            // toolStripMenuItemNewSessionLogFile
            // 
            this.toolStripMenuItemNewSessionLogFile.Name = "toolStripMenuItemNewSessionLogFile";
            this.toolStripMenuItemNewSessionLogFile.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift)
                        | System.Windows.Forms.Keys.O)));
            this.toolStripMenuItemNewSessionLogFile.Size = new System.Drawing.Size(218, 22);
            this.toolStripMenuItemNewSessionLogFile.Text = "Log &File...";
            this.toolStripMenuItemNewSessionLogFile.Click += new System.EventHandler(this.toolStripMenuItemNewSessionLogFile_Click);
            // 
            // toolStripMenuItemOpenSession
            // 
            this.toolStripMenuItemOpenSession.Image = ((System.Drawing.Image)(resources.GetObject("toolStripMenuItemOpenSession.Image")));
            this.toolStripMenuItemOpenSession.Name = "toolStripMenuItemOpenSession";
            this.toolStripMenuItemOpenSession.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
            this.toolStripMenuItemOpenSession.Size = new System.Drawing.Size(202, 22);
            this.toolStripMenuItemOpenSession.Text = "Open Session...";
            // 
            // toolStripMenuItemSaveSession
            // 
            this.toolStripMenuItemSaveSession.Image = ((System.Drawing.Image)(resources.GetObject("toolStripMenuItemSaveSession.Image")));
            this.toolStripMenuItemSaveSession.Name = "toolStripMenuItemSaveSession";
            this.toolStripMenuItemSaveSession.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
            this.toolStripMenuItemSaveSession.Size = new System.Drawing.Size(202, 22);
            this.toolStripMenuItemSaveSession.Text = "Save Session";
            // 
            // toolStripMenuItemSaveSessionAs
            // 
            this.toolStripMenuItemSaveSessionAs.Name = "toolStripMenuItemSaveSessionAs";
            this.toolStripMenuItemSaveSessionAs.Size = new System.Drawing.Size(202, 22);
            this.toolStripMenuItemSaveSessionAs.Text = "Save Session As...";
            // 
            // toolStripSeparator5
            // 
            this.toolStripSeparator5.Name = "toolStripSeparator5";
            this.toolStripSeparator5.Size = new System.Drawing.Size(199, 6);
            // 
            // toolStripMenuItemRecentSessions
            // 
            this.toolStripMenuItemRecentSessions.Name = "toolStripMenuItemRecentSessions";
            this.toolStripMenuItemRecentSessions.Size = new System.Drawing.Size(202, 22);
            this.toolStripMenuItemRecentSessions.Text = "&Recent Sessions";
            // 
            // toolStripMenuItem3
            // 
            this.toolStripMenuItem3.Name = "toolStripMenuItem3";
            this.toolStripMenuItem3.Size = new System.Drawing.Size(199, 6);
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.F4)));
            this.toolStripMenuItem2.Size = new System.Drawing.Size(202, 22);
            this.toolStripMenuItem2.Text = "&Exit";
            this.toolStripMenuItem2.Click += new System.EventHandler(this.FileExit);
            // 
            // viewToolStripMenuItem
            // 
            this.viewToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.alwaysOnTopToolStripMenuItem});
            this.viewToolStripMenuItem.Name = "viewToolStripMenuItem";
            this.viewToolStripMenuItem.Size = new System.Drawing.Size(41, 20);
            this.viewToolStripMenuItem.Text = "&View";
            // 
            // alwaysOnTopToolStripMenuItem
            // 
            this.alwaysOnTopToolStripMenuItem.Name = "alwaysOnTopToolStripMenuItem";
            this.alwaysOnTopToolStripMenuItem.Size = new System.Drawing.Size(155, 22);
            this.alwaysOnTopToolStripMenuItem.Text = "&Always on Top";
            this.alwaysOnTopToolStripMenuItem.Click += new System.EventHandler(this.alwaysOnTopToolStripMenuItem_Click);
            // 
            // toolStrip1
            // 
            this.toolStrip1.Dock = System.Windows.Forms.DockStyle.None;
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButtonNewLiveLogReceiver,
            this.toolStripButtonOpen,
            this.toolStripButton3,
            this.toolStripSeparator3});
            this.toolStrip1.Location = new System.Drawing.Point(3, 24);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.toolStrip1.Size = new System.Drawing.Size(87, 25);
            this.toolStrip1.TabIndex = 9;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // toolStripButtonNewLiveLogReceiver
            // 
            this.toolStripButtonNewLiveLogReceiver.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButtonNewLiveLogReceiver.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButtonNewLiveLogReceiver.Image")));
            this.toolStripButtonNewLiveLogReceiver.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonNewLiveLogReceiver.Name = "toolStripButtonNewLiveLogReceiver";
            this.toolStripButtonNewLiveLogReceiver.Size = new System.Drawing.Size(23, 22);
            this.toolStripButtonNewLiveLogReceiver.Text = "New Live Log Receiver";
            this.toolStripButtonNewLiveLogReceiver.Click += new System.EventHandler(this.NewLiveLogReceiver_Clicked);
            // 
            // toolStripButtonOpen
            // 
            this.toolStripButtonOpen.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButtonOpen.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButtonOpen.Image")));
            this.toolStripButtonOpen.ImageTransparentColor = System.Drawing.Color.Black;
            this.toolStripButtonOpen.Name = "toolStripButtonOpen";
            this.toolStripButtonOpen.Size = new System.Drawing.Size(23, 22);
            this.toolStripButtonOpen.Text = "Open Log File";
            this.toolStripButtonOpen.Click += new System.EventHandler(this.OpenLogFile);
            // 
            // toolStripButton3
            // 
            this.toolStripButton3.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton3.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton3.Image")));
            this.toolStripButton3.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton3.Name = "toolStripButton3";
            this.toolStripButton3.Size = new System.Drawing.Size(23, 22);
            this.toolStripButton3.Text = "Save Current Log To File";
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(6, 25);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.tabControl1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Padding = new System.Windows.Forms.Padding(2);
            this.panel1.Size = new System.Drawing.Size(692, 417);
            this.panel1.TabIndex = 2;
            this.panel1.Paint += new System.Windows.Forms.PaintEventHandler(this.panel1_Paint);
            // 
            // toolStripContainer1
            // 
            // 
            // toolStripContainer1.ContentPanel
            // 
            this.toolStripContainer1.ContentPanel.Controls.Add(this.panel1);
            this.toolStripContainer1.ContentPanel.Size = new System.Drawing.Size(692, 417);
            this.toolStripContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.toolStripContainer1.LeftToolStripPanelVisible = false;
            this.toolStripContainer1.Location = new System.Drawing.Point(0, 0);
            this.toolStripContainer1.Name = "toolStripContainer1";
            this.toolStripContainer1.RightToolStripPanelVisible = false;
            this.toolStripContainer1.Size = new System.Drawing.Size(692, 466);
            this.toolStripContainer1.TabIndex = 10;
            this.toolStripContainer1.Text = "toolStripContainer1";
            // 
            // toolStripContainer1.TopToolStripPanel
            // 
            this.toolStripContainer1.TopToolStripPanel.Controls.Add(this.menuStrip1);
            this.toolStripContainer1.TopToolStripPanel.Controls.Add(this.toolStrip1);
            this.toolStripContainer1.TopToolStripPanel.Controls.Add(this.toolStrip2);
            this.toolStripContainer1.TopToolStripPanel.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            // 
            // toolStrip2
            // 
            this.toolStrip2.Dock = System.Windows.Forms.DockStyle.None;
            this.toolStrip2.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripLabelStatus0,
            this.toolStripLabelStatus,
            this.toolStripButtonRecord,
            this.toolStripButtonStop,
            this.toolStripButtonRefresh,
            this.toolStripSeparator4,
            this.toolStripButtonClear,
            this.toolStripButtonFilter,
            this.toolStripButtonHighlight,
            this.toolStripSeparator1,
            this.toolStripButtonFind,
            this.toolStripButtonFindNext,
            this.toolStripTextBoxFindText,
            this.toolStripSeparator2,
            this.toolStripButtonChooseFont,
            this.toolStripButtonZoomIn,
            this.toolStripButtonZoomOut,
            this.toolStripButtonFitColumnWidths,
            this.toolStripButtonChooseColumns});
            this.toolStrip2.Location = new System.Drawing.Point(90, 24);
            this.toolStrip2.Name = "toolStrip2";
            this.toolStrip2.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.toolStrip2.Size = new System.Drawing.Size(544, 25);
            this.toolStrip2.TabIndex = 10;
            // 
            // toolStripLabelStatus0
            // 
            this.toolStripLabelStatus0.Name = "toolStripLabelStatus0";
            this.toolStripLabelStatus0.Size = new System.Drawing.Size(42, 22);
            this.toolStripLabelStatus0.Text = "Status:";
            // 
            // toolStripLabelStatus
            // 
            this.toolStripLabelStatus.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.toolStripLabelStatus.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(192)))));
            this.toolStripLabelStatus.Name = "toolStripLabelStatus";
            this.toolStripLabelStatus.Size = new System.Drawing.Size(53, 22);
            this.toolStripLabelStatus.Text = "Running";
            // 
            // toolStripButtonRecord
            // 
            this.toolStripButtonRecord.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButtonRecord.Enabled = false;
            this.toolStripButtonRecord.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButtonRecord.Image")));
            this.toolStripButtonRecord.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonRecord.Name = "toolStripButtonRecord";
            this.toolStripButtonRecord.Size = new System.Drawing.Size(23, 22);
            this.toolStripButtonRecord.Text = "Start/Continue";
            this.toolStripButtonRecord.Click += new System.EventHandler(this.StartCurrentInstance);
            // 
            // toolStripButtonStop
            // 
            this.toolStripButtonStop.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButtonStop.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButtonStop.Image")));
            this.toolStripButtonStop.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonStop.Name = "toolStripButtonStop";
            this.toolStripButtonStop.Size = new System.Drawing.Size(23, 22);
            this.toolStripButtonStop.Text = "Stop";
            this.toolStripButtonStop.Click += new System.EventHandler(this.StopCurrentInstance);
            // 
            // toolStripButtonRefresh
            // 
            this.toolStripButtonRefresh.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButtonRefresh.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButtonRefresh.Image")));
            this.toolStripButtonRefresh.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonRefresh.Name = "toolStripButtonRefresh";
            this.toolStripButtonRefresh.Size = new System.Drawing.Size(23, 22);
            this.toolStripButtonRefresh.Text = "Refresh";
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(6, 25);
            // 
            // toolStripButtonClear
            // 
            this.toolStripButtonClear.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButtonClear.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButtonClear.Image")));
            this.toolStripButtonClear.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonClear.Name = "toolStripButtonClear";
            this.toolStripButtonClear.Size = new System.Drawing.Size(23, 22);
            this.toolStripButtonClear.Text = "Clear trace";
            this.toolStripButtonClear.Click += new System.EventHandler(this.toolStripButtonClear_Click);
            // 
            // toolStripButtonFilter
            // 
            this.toolStripButtonFilter.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButtonFilter.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButtonFilter.Image")));
            this.toolStripButtonFilter.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonFilter.Name = "toolStripButtonFilter";
            this.toolStripButtonFilter.Size = new System.Drawing.Size(32, 22);
            this.toolStripButtonFilter.Text = "Filter";
            // 
            // toolStripButtonHighlight
            // 
            this.toolStripButtonHighlight.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButtonHighlight.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButtonHighlight.Image")));
            this.toolStripButtonHighlight.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonHighlight.Name = "toolStripButtonHighlight";
            this.toolStripButtonHighlight.Size = new System.Drawing.Size(32, 22);
            this.toolStripButtonHighlight.Text = "Highlight";
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // toolStripButtonFind
            // 
            this.toolStripButtonFind.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButtonFind.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButtonFind.Image")));
            this.toolStripButtonFind.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonFind.Name = "toolStripButtonFind";
            this.toolStripButtonFind.Size = new System.Drawing.Size(23, 22);
            this.toolStripButtonFind.Text = "Find";
            // 
            // toolStripButtonFindNext
            // 
            this.toolStripButtonFindNext.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButtonFindNext.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButtonFindNext.Image")));
            this.toolStripButtonFindNext.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonFindNext.Name = "toolStripButtonFindNext";
            this.toolStripButtonFindNext.Size = new System.Drawing.Size(23, 22);
            this.toolStripButtonFindNext.Text = "Find Next";
            // 
            // toolStripTextBoxFindText
            // 
            this.toolStripTextBoxFindText.Name = "toolStripTextBoxFindText";
            this.toolStripTextBoxFindText.Size = new System.Drawing.Size(100, 25);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 25);
            // 
            // toolStripButtonChooseFont
            // 
            this.toolStripButtonChooseFont.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButtonChooseFont.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButtonChooseFont.Image")));
            this.toolStripButtonChooseFont.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonChooseFont.Name = "toolStripButtonChooseFont";
            this.toolStripButtonChooseFont.Size = new System.Drawing.Size(23, 22);
            this.toolStripButtonChooseFont.Text = "Choose Font";
            this.toolStripButtonChooseFont.Click += new System.EventHandler(this.toolStripButtonChooseFont_Click);
            // 
            // toolStripButtonZoomIn
            // 
            this.toolStripButtonZoomIn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButtonZoomIn.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButtonZoomIn.Image")));
            this.toolStripButtonZoomIn.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonZoomIn.Name = "toolStripButtonZoomIn";
            this.toolStripButtonZoomIn.Size = new System.Drawing.Size(23, 22);
            this.toolStripButtonZoomIn.Text = "Zoom In";
            this.toolStripButtonZoomIn.Click += new System.EventHandler(this.toolStripButtonZoomIn_Click);
            // 
            // toolStripButtonZoomOut
            // 
            this.toolStripButtonZoomOut.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButtonZoomOut.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButtonZoomOut.Image")));
            this.toolStripButtonZoomOut.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonZoomOut.Name = "toolStripButtonZoomOut";
            this.toolStripButtonZoomOut.Size = new System.Drawing.Size(23, 22);
            this.toolStripButtonZoomOut.Text = "Zoom Out";
            this.toolStripButtonZoomOut.Click += new System.EventHandler(this.toolStripButtonZoomOut_Click);
            // 
            // toolStripButtonFitColumnWidths
            // 
            this.toolStripButtonFitColumnWidths.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButtonFitColumnWidths.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButtonFitColumnWidths.Image")));
            this.toolStripButtonFitColumnWidths.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonFitColumnWidths.Name = "toolStripButtonFitColumnWidths";
            this.toolStripButtonFitColumnWidths.Size = new System.Drawing.Size(23, 22);
            this.toolStripButtonFitColumnWidths.Text = "Fit Column Widths To Content";
            this.toolStripButtonFitColumnWidths.Click += new System.EventHandler(this.toolStripButtonFitColumnWidths_Click);
            // 
            // toolStripButtonChooseColumns
            // 
            this.toolStripButtonChooseColumns.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButtonChooseColumns.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButtonChooseColumns.Image")));
            this.toolStripButtonChooseColumns.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonChooseColumns.Name = "toolStripButtonChooseColumns";
            this.toolStripButtonChooseColumns.Size = new System.Drawing.Size(23, 22);
            this.toolStripButtonChooseColumns.Text = "Choose Columns";
            this.toolStripButtonChooseColumns.Click += new System.EventHandler(this.toolStripButtonChooseColumns_Click);
            // 
            // notifyIcon1
            // 
            this.notifyIcon1.Icon = ((System.Drawing.Icon)(resources.GetObject("notifyIcon1.Icon")));
            this.notifyIcon1.Text = "notifyIcon1";
            this.notifyIcon1.Visible = true;
            // 
            // imageList1
            // 
            this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList1.Images.SetKeyName(0, "App.ico");
            // 
            // MainForm
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(692, 466);
            this.Controls.Add(this.toolStripContainer1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "MainForm";
            this.Text = "NLog Viewer";
            this.Closed += new System.EventHandler(this.MainForm_Closed);
            this.Shown += new System.EventHandler(this.MainForm_Shown);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.tabControl1.ResumeLayout(false);
            this.tabPageNLogViewerTrace.ResumeLayout(false);
            this.tabPageNLogViewerTrace.PerformLayout();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.toolStripContainer1.ContentPanel.ResumeLayout(false);
            this.toolStripContainer1.TopToolStripPanel.ResumeLayout(false);
            this.toolStripContainer1.TopToolStripPanel.PerformLayout();
            this.toolStripContainer1.ResumeLayout(false);
            this.toolStripContainer1.PerformLayout();
            this.toolStrip2.ResumeLayout(false);
            this.toolStrip2.PerformLayout();
            this.ResumeLayout(false);

        }
		#endregion

        private void menuItemFileExit_Click(object sender, System.EventArgs e)
        {
            this.Close();
        }

        private void MainForm_Load(object sender, System.EventArgs e)
        {
            this.TopMost = viewToolStripMenuItem.Checked = AppPreferences.AlwaysOnTop;
        }

        private void LoadLogs()
        {
        }

        private void ReloadTabPages()
        {
            this.tabControl1.TabPages.Clear();
            foreach (Session i in _sessions)
            {
                this.tabControl1.TabPages.Add(i.TabPage);
            }
            this.tabControl1.TabPages.Add(tabPageNLogViewerTrace);
        }

        private string GetLogsDirectory()
        {
            return Path.Combine(_baseConfigurationPath, "Logs");
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
            for (int i = 0; i < _sessions.Count; ++i)
            {
                _sessions[i].OnTimer();
            }
            UpdateUIForSelectedTab();
        }

        private void MainForm_Closed(object sender, System.EventArgs e)
        {
            foreach (Session i in _sessions)
            {
                i.Stop();
            }
        }

        private void menuItem11_Click(object sender, EventArgs e)
        {
            foreach (Session i in _sessions)
            {
                i.Clear();
            }
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {
        }

        private void OpenLogFile(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "Log files (*.log;*.txt)|*.log;*.txt|All files (*.*)|*.*";
                if (DialogResult.OK == ofd.ShowDialog())
                {
                    SessionConfiguration lici = new SessionConfiguration();
                    lici.ReceiverType = "FILE";
                    lici.ReceiverParameters["FileName"] = ofd.FileName;
                    lici.Name = Path.GetFileName(ofd.FileName);
                    lici.Resolve();

                    Session instance = new Session(lici);
                    instance.CreateTab(this);
                    _sessions.Add(instance);
                    ReloadTabPages();
                    instance.Start();
                    tabControl1.SelectedTab = instance.TabPage;
                }
            }

        }

        private void FileExit(object sender, EventArgs e)
        {
            this.Close();
        }

        private void NewLiveLogReceiver_Clicked(object sender, EventArgs e)
        {
            using (NewReceiverDialog dlg = new NewReceiverDialog())
            {
                dlg.ShowDialog(this);
            }
        }

        private Session SelectedSession
        {
            get
            {
                Session currentLogInstance = null;

                if (tabControl1.SelectedTab != null)
                    currentLogInstance = tabControl1.SelectedTab.Tag as Session;
                return currentLogInstance;
            }

        }

        private void UpdateUIForSelectedTab()
        {
            Session currentLogInstance = null;

            if (tabControl1.SelectedTab != null)
                currentLogInstance = tabControl1.SelectedTab.Tag as Session;
            if (currentLogInstance == null)
            {
                this.Text = "NLog Viewer";
                toolStripLabelStatus.Enabled = false;
                toolStripButtonStop.Enabled = false;
                toolStripButtonRecord.Enabled = false;
                toolStripButtonChooseColumns.Enabled = false;
                toolStripButtonZoomIn.Enabled = false;
                toolStripButtonZoomOut.Enabled = false;
                toolStripButtonRefresh.Enabled = false;
                toolStripButtonClear.Enabled = false;
                toolStripButtonFitColumnWidths.Enabled = false;
                toolStripButtonFind.Enabled = false;
                toolStripButtonFindNext.Enabled = false;
                toolStripButtonFilter.Enabled = false;
                toolStripButtonHighlight.Enabled = false;
            }
            else
            {
                this.Text = String.Format("{0} - NLog Viewer", currentLogInstance.Config.Name);
                toolStripLabelStatus.Enabled = true;

                toolStripButtonStop.Enabled = currentLogInstance.IsRunning;
                toolStripButtonRecord.Enabled = !currentLogInstance.IsRunning;
                toolStripLabelStatus.Text = currentLogInstance.StatusText;
                toolStripButtonChooseColumns.Enabled = true;
                toolStripButtonZoomIn.Enabled = true;
                toolStripButtonZoomOut.Enabled = true;
                toolStripButtonRefresh.Enabled = true; // TODO
                toolStripButtonClear.Enabled = true;
                toolStripButtonFitColumnWidths.Enabled = true;
                toolStripButtonFind.Enabled = true;
                toolStripButtonFindNext.Enabled = true;
                toolStripButtonFilter.Enabled = true;
                toolStripButtonHighlight.Enabled = true;
            }
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateUIForSelectedTab();
        }

        private void StartCurrentInstance(object sender, EventArgs e)
        {
            Session li = SelectedSession;
            if (li == null)
                return;

            if (li.IsRunning)
                return;

            li.Start();
        }

        private void StopCurrentInstance(object sender, EventArgs e)
        {
            Session li = SelectedSession;
            if (li == null)
                return;

            if (!li.IsRunning)
                return;

            li.Stop();
        }

        private void alwaysOnTopToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AppPreferences.AlwaysOnTop =  this.TopMost = alwaysOnTopToolStripMenuItem.Checked = !alwaysOnTopToolStripMenuItem.Checked;
        }

        private void toolStripButtonClear_Click(object sender, EventArgs e)
        {
            Session li = SelectedSession;
            if (li == null)
                return;

            li.Clear();
        }

        private void MainForm_Shown(object sender, EventArgs e)
        {
            if (AppPreferences.ShowWelcomeScreenOnStartup)
            {
                using (IntroDialog intro = new IntroDialog())
                {
                    switch (intro.ShowDialog(this))
                    {
                        // open log file

                        case DialogResult.Yes:
                            OpenLogFile(null, null);
                            break;

                        case DialogResult.No:
                            NewLiveLogReceiver_Clicked(null, null);
                            break;
                    }
                }
            }
        }

        private void toolStripButtonZoomIn_Click(object sender, EventArgs e)
        {
            Font f = AppPreferences.LogMessagesFont;

            AppPreferences.LogMessagesFont = new Font(f.FontFamily.Name, f.Size + 1);
            UpdateFonts();
        }

        private void toolStripButtonZoomOut_Click(object sender, EventArgs e)
        {
            Font f = AppPreferences.LogMessagesFont;
            if (f.Size < 7)
                return;

            AppPreferences.LogMessagesFont = new Font(f.FontFamily.Name, f.Size - 1);
            UpdateFonts();
        }

        private void UpdateFonts()
        {
            foreach (Session instance in _sessions)
            {
                instance.UpdateFonts();
            }
        }

        private void toolStripButtonChooseFont_Click(object sender, EventArgs e)
        {
            using (FontDialog fd = new FontDialog())
            {
                fd.Font = AppPreferences.LogMessagesFont;
                if (fd.ShowDialog(this) == DialogResult.OK)
                {
                    AppPreferences.LogMessagesFont = fd.Font;
                    UpdateFonts();
                }
            }
        }

        private void toolStripButtonFitColumnWidths_Click(object sender, EventArgs e)
        {
            Session li = SelectedSession;
            if (li == null)
                return;

            li.TabPanel.FitColumnWidths();
        }

        private void toolStripButtonChooseColumns_Click(object sender, EventArgs e)
        {
            Session li = SelectedSession;
            if (li == null)
                return;

            li.TabPanel.ChooseColumns();
        }

        private void menuStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
        }

        private void menuStrip1_MenuActivate(object sender, EventArgs e)
        {
            Session li = SelectedSession;

            toolStripMenuItemSaveSession.Enabled = (li != null);
            toolStripMenuItemSaveSessionAs.Enabled = (li != null);
        }

        private void toolStripMenuItemNewSessionLogFile_Click(object sender, EventArgs e)
        {
            OpenLogFile(sender, e);
        }
	}
}
