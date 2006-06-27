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
        private MenuStrip menuStrip1;
        private Panel panel1;
        private ToolStripContainer toolStripContainer1;
        private ToolStripMenuItem viewToolStripMenuItem;
        private ToolStripMenuItem alwaysOnTopToolStripMenuItem;
        private NotifyIcon notifyIcon1;
        private ImageList imageList1;
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
        private ToolStripMenuItem toolsToolStripMenuItem;
        private ToolStripMenuItem optionsToolStripMenuItem;
        private ToolStripMenuItem toolStripMenuItemCloseSession;
        private ToolStripMenuItem toolStripMenuItemCloseAllSessions;
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
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemNewSessionLogReceiver = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemNewSessionLogFile = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemOpenSession = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemSaveSession = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemSaveSessionAs = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemCloseSession = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemCloseAllSessions = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripMenuItemRecentSessions = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem3 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
            this.viewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.alwaysOnTopToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.optionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.panel1 = new System.Windows.Forms.Panel();
            this.toolStripContainer1 = new System.Windows.Forms.ToolStripContainer();
            this.notifyIcon1 = new System.Windows.Forms.NotifyIcon(this.components);
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.menuStrip1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.toolStripContainer1.ContentPanel.SuspendLayout();
            this.toolStripContainer1.TopToolStripPanel.SuspendLayout();
            this.toolStripContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.HotTrack = true;
            this.tabControl1.Location = new System.Drawing.Point(2, 2);
            this.tabControl1.Multiline = true;
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.Padding = new System.Drawing.Point(6, 6);
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(688, 438);
            this.tabControl1.TabIndex = 6;
            this.tabControl1.SelectedIndexChanged += new System.EventHandler(this.tabControl1_SelectedIndexChanged);
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
            this.viewToolStripMenuItem,
            this.toolsToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.menuStrip1.Size = new System.Drawing.Size(692, 24);
            this.menuStrip1.TabIndex = 7;
            this.menuStrip1.Text = "menuStrip1";
            this.menuStrip1.MenuActivate += new System.EventHandler(this.menuStrip1_MenuActivate);
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem1,
            this.toolStripMenuItemOpenSession,
            this.toolStripMenuItemSaveSession,
            this.toolStripMenuItemSaveSessionAs,
            this.toolStripMenuItemCloseSession,
            this.toolStripMenuItemCloseAllSessions,
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
            this.toolStripMenuItemOpenSession.Click += new System.EventHandler(this.OpenSession);
            // 
            // toolStripMenuItemSaveSession
            // 
            this.toolStripMenuItemSaveSession.Image = ((System.Drawing.Image)(resources.GetObject("toolStripMenuItemSaveSession.Image")));
            this.toolStripMenuItemSaveSession.Name = "toolStripMenuItemSaveSession";
            this.toolStripMenuItemSaveSession.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
            this.toolStripMenuItemSaveSession.Size = new System.Drawing.Size(202, 22);
            this.toolStripMenuItemSaveSession.Text = "Save Session";
            this.toolStripMenuItemSaveSession.Click += new System.EventHandler(this.toolStripMenuItemSaveSession_Click);
            // 
            // toolStripMenuItemSaveSessionAs
            // 
            this.toolStripMenuItemSaveSessionAs.Name = "toolStripMenuItemSaveSessionAs";
            this.toolStripMenuItemSaveSessionAs.Size = new System.Drawing.Size(202, 22);
            this.toolStripMenuItemSaveSessionAs.Text = "Save Session As...";
            this.toolStripMenuItemSaveSessionAs.Click += new System.EventHandler(this.toolStripMenuItemSaveSessionAs_Click);
            // 
            // toolStripMenuItemCloseSession
            // 
            this.toolStripMenuItemCloseSession.Name = "toolStripMenuItemCloseSession";
            this.toolStripMenuItemCloseSession.Size = new System.Drawing.Size(202, 22);
            this.toolStripMenuItemCloseSession.Text = "Close Session";
            this.toolStripMenuItemCloseSession.Click += new System.EventHandler(this.toolStripMenuItemCloseSession_Click);
            // 
            // toolStripMenuItemCloseAllSessions
            // 
            this.toolStripMenuItemCloseAllSessions.Name = "toolStripMenuItemCloseAllSessions";
            this.toolStripMenuItemCloseAllSessions.Size = new System.Drawing.Size(202, 22);
            this.toolStripMenuItemCloseAllSessions.Text = "Close All Sessions";
            this.toolStripMenuItemCloseAllSessions.Click += new System.EventHandler(this.toolStripMenuItemCloseAllSessions_Click);
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
            // toolsToolStripMenuItem
            // 
            this.toolsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.optionsToolStripMenuItem});
            this.toolsToolStripMenuItem.Name = "toolsToolStripMenuItem";
            this.toolsToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
            this.toolsToolStripMenuItem.Text = "&Tools";
            // 
            // optionsToolStripMenuItem
            // 
            this.optionsToolStripMenuItem.Name = "optionsToolStripMenuItem";
            this.optionsToolStripMenuItem.Size = new System.Drawing.Size(134, 22);
            this.optionsToolStripMenuItem.Text = "&Options...";
            this.optionsToolStripMenuItem.Click += new System.EventHandler(this.optionsToolStripMenuItem_Click);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.tabControl1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Padding = new System.Windows.Forms.Padding(2);
            this.panel1.Size = new System.Drawing.Size(692, 442);
            this.panel1.TabIndex = 2;
            this.panel1.Paint += new System.Windows.Forms.PaintEventHandler(this.panel1_Paint);
            // 
            // toolStripContainer1
            // 
            // 
            // toolStripContainer1.ContentPanel
            // 
            this.toolStripContainer1.ContentPanel.Controls.Add(this.panel1);
            this.toolStripContainer1.ContentPanel.Size = new System.Drawing.Size(692, 442);
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
            this.toolStripContainer1.TopToolStripPanel.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
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
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.toolStripContainer1.ContentPanel.ResumeLayout(false);
            this.toolStripContainer1.TopToolStripPanel.ResumeLayout(false);
            this.toolStripContainer1.TopToolStripPanel.PerformLayout();
            this.toolStripContainer1.ResumeLayout(false);
            this.toolStripContainer1.PerformLayout();
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
        }

        private string GetLogsDirectory()
        {
            return Path.Combine(_baseConfigurationPath, "Logs");
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

        private string GetUniqueSessionName()
        {
            for (int i = 1; i < 1000; ++i)
            {
                string proposedName = "Session" + i;
                bool conflict = false;

                foreach (Session s in _sessions)
                {
                    if (s.Config.Name == proposedName)
                    {
                        conflict = true;
                        break;
                    }
                }
                if (!conflict)
                    return proposedName;
            }
            throw new Exception("Too much sessions.");
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
                    lici.ReceiverParameters.Add(new ConfigurationParameter("FileName", ofd.FileName));
                    lici.Name = GetUniqueSessionName();
                    lici.Resolve();
                    lici.Dirty = true;

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
            }
            else
            {
                this.Text = String.Format("{0} - NLog Viewer", currentLogInstance.Config.Name);
            }
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateUIForSelectedTab();
        }

        private void alwaysOnTopToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AppPreferences.AlwaysOnTop =  this.TopMost = alwaysOnTopToolStripMenuItem.Checked = !alwaysOnTopToolStripMenuItem.Checked;
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

                        case DialogResult.Retry:
                            OpenSession(null, null);
                            break;
                    }
                }
            }
        }

        private void menuStrip1_MenuActivate(object sender, EventArgs e)
        {
            Session li = SelectedSession;

            toolStripMenuItemSaveSession.Enabled = (li != null);
            toolStripMenuItemSaveSessionAs.Enabled = (li != null);

            toolStripMenuItemRecentSessions.DropDownItems.Clear();

            int pos = 1;

            foreach (string s in AppPreferences.GetRecentFileList())
            {
                ToolStripMenuItem newItem = new ToolStripMenuItem();
                newItem.Text = "&" + pos + ". " + s;
                newItem.Tag = s;
                newItem.Click += new EventHandler(recentItem_Click);

                toolStripMenuItemRecentSessions.DropDownItems.Add(newItem);
                pos++;
            }
        }

        private void recentItem_Click(object sender, EventArgs e)
        {
            string fileName = ((ToolStripMenuItem)sender).Tag as string;
            if (fileName != null)
                OpenSession(fileName);
        }

        private void toolStripMenuItemNewSessionLogFile_Click(object sender, EventArgs e)
        {
            OpenLogFile(sender, e);
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            foreach (Session s in new List<Session>(_sessions))
            {
                if (!s.Close())
                {
                    e.Cancel = true;
                    return;
                }
            }
        }

        private void OpenSession(string fileName)
        {
            SessionConfiguration sc = SessionConfiguration.Load(fileName);
            Session instance = new Session(sc);

            instance.CreateTab(this);
            _sessions.Add(instance);
            ReloadTabPages();
            instance.Start();
            tabControl1.SelectedTab = instance.TabPage;
            AppPreferences.AddToRecentFileList(fileName);
        }

        private void OpenSession(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "NLogViewer Sessions (*.nlv)|*.nlv|All Files (*.*)|*.*";
                if (ofd.ShowDialog(this) == DialogResult.OK)
                {
                    OpenSession(ofd.FileName);
                }
            }
        }

        private void toolStripMenuItemSaveSession_Click(object sender, EventArgs e)
        {
            Session li = SelectedSession;
            if (li == null)
                return;

            li.Save(this);
        }

        private void toolStripMenuItemSaveSessionAs_Click(object sender, EventArgs e)
        {
            Session li = SelectedSession;
            if (li == null)
                return;

            li.SaveAs(this);
        }

        private void optionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (OptionsDialog od = new OptionsDialog())
            {
                if (od.ShowDialog(this) == DialogResult.OK)
                {
                }
            }
        }

        private void toolStripMenuItemCloseSession_Click(object sender, EventArgs e)
        {
            Session li = SelectedSession;
            if (li == null)
                return;

            li.Close();
        }

        private void toolStripMenuItemCloseAllSessions_Click(object sender, EventArgs e)
        {
            foreach (Session s in _sessions)
            {
                if (!s.Close())
                    return;
            }
        }

        public void RemoveSession(Session s)
        {
            _sessions.Remove(s);
            ReloadTabPages();
        }
	}
}
