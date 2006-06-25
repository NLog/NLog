using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows.Forms;
using NLogViewer.Events;
using System.Collections.Generic;
using NLogViewer.Configuration;

namespace NLogViewer.UI
{
	/// <summary>
	/// Summary description for SessionTabPage.
	/// </summary>
	public class SessionTabPage : System.Windows.Forms.UserControl
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        private System.Windows.Forms.Splitter splitter1;
        private System.Windows.Forms.Splitter splitter2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        public System.Windows.Forms.TreeView treeView;
        public System.Windows.Forms.ListView listViewLogMessages;
        private System.Windows.Forms.Panel panelTop;
        private System.Windows.Forms.Panel panelSelectedItemMessage;
        private System.Windows.Forms.Panel panelSelectedItemProperties;
        public System.Windows.Forms.ListView listviewSelectedLogEventProperties;
        public System.Windows.Forms.TextBox textBoxSelectedMessageText;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private IContainer components;
        private Timer timer1;
        private Panel panel2;
        private Panel panel1;
        private StatusStrip statusStrip1;
        private ToolStripStatusLabel toolStripStatusLabel1;
        private ToolStripStatusLabel toolStripStatusLabel3;
        private ToolStripSplitButton toolStripStatusEventsInBuffer;
        private ToolStripSplitButton toolStripStatusEventsDisplayed;
        private ToolStripMenuItem toolStripMenuItem2;
        private ToolStripMenuItem clearToolStripMenuItem1;
        private ToolStripMenuItem clearToolStripMenuItem;
        private ToolStripStatusLabel toolStripStatusLabel2;
        private ToolStripStatusLabel toolStripStatusTotalEvents;
        private ToolStripStatusLabel toolStripStatusLabel5;
        private ToolStripStatusLabel toolStripStatusLastEvent;
        private Session _session;
        private Button buttonPreviousEvent;
        private Button buttonNextEvent;
        private TableLayoutPanel tableLayoutPanel1;
        private ContextMenuStrip contextMenuStrip1;
        private ToolStripMenuItem chooseColumnsToolStripMenuItem;
        private ToolStripMenuItem resetToDefaultsToolStripMenuItem;
        private LogEvent _selectedLogEvent = null;

		public SessionTabPage(Session instance)
		{
            _session = instance;
			InitializeComponent();
            listViewLogMessages.SmallImageList = GlobalImageList.Instance.ImageList;
            listViewLogMessages.Font = AppPreferences.LogMessagesFont;
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SessionTabPage));
            this.treeView = new System.Windows.Forms.TreeView();
            this.panelTop = new System.Windows.Forms.Panel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.listViewLogMessages = new System.Windows.Forms.ListView();
            this.splitter1 = new System.Windows.Forms.Splitter();
            this.panel1 = new System.Windows.Forms.Panel();
            this.splitter2 = new System.Windows.Forms.Splitter();
            this.panelSelectedItemMessage = new System.Windows.Forms.Panel();
            this.textBoxSelectedMessageText = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.panelSelectedItemProperties = new System.Windows.Forms.Panel();
            this.listviewSelectedLogEventProperties = new System.Windows.Forms.ListView();
            this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader2 = new System.Windows.Forms.ColumnHeader();
            this.buttonPreviousEvent = new System.Windows.Forms.Button();
            this.buttonNextEvent = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusEventsInBuffer = new System.Windows.Forms.ToolStripSplitButton();
            this.clearToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripStatusLabel3 = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusEventsDisplayed = new System.Windows.Forms.ToolStripSplitButton();
            this.clearToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripStatusLabel2 = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusTotalEvents = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel5 = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLastEvent = new System.Windows.Forms.ToolStripStatusLabel();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.resetToDefaultsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.chooseColumnsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.panelTop.SuspendLayout();
            this.panel2.SuspendLayout();
            this.panel1.SuspendLayout();
            this.panelSelectedItemMessage.SuspendLayout();
            this.panelSelectedItemProperties.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.contextMenuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // treeView
            // 
            this.treeView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeView.Location = new System.Drawing.Point(0, 0);
            this.treeView.Name = "treeView";
            this.treeView.Size = new System.Drawing.Size(200, 254);
            this.treeView.TabIndex = 0;
            // 
            // panelTop
            // 
            this.panelTop.Controls.Add(this.panel2);
            this.panelTop.Controls.Add(this.splitter1);
            this.panelTop.Controls.Add(this.panel1);
            this.panelTop.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelTop.Location = new System.Drawing.Point(0, 0);
            this.panelTop.Name = "panelTop";
            this.panelTop.Size = new System.Drawing.Size(656, 254);
            this.panelTop.TabIndex = 1;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.listViewLogMessages);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(204, 0);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(452, 254);
            this.panel2.TabIndex = 4;
            // 
            // listViewLogMessages
            // 
            this.listViewLogMessages.AllowColumnReorder = true;
            this.listViewLogMessages.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listViewLogMessages.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.listViewLogMessages.FullRowSelect = true;
            this.listViewLogMessages.GridLines = true;
            this.listViewLogMessages.Location = new System.Drawing.Point(0, 0);
            this.listViewLogMessages.MultiSelect = false;
            this.listViewLogMessages.Name = "listViewLogMessages";
            this.listViewLogMessages.Size = new System.Drawing.Size(452, 254);
            this.listViewLogMessages.TabIndex = 2;
            this.listViewLogMessages.UseCompatibleStateImageBehavior = false;
            this.listViewLogMessages.View = System.Windows.Forms.View.Details;
            this.listViewLogMessages.VirtualMode = true;
            this.listViewLogMessages.ColumnWidthChanged += new System.Windows.Forms.ColumnWidthChangedEventHandler(this.listViewLogMessages_ColumnWidthChanged);
            this.listViewLogMessages.ItemActivate += new System.EventHandler(this.listViewLogMessages_ItemActivate);
            this.listViewLogMessages.VirtualItemsSelectionRangeChanged += new System.Windows.Forms.ListViewVirtualItemsSelectionRangeChangedEventHandler(this.listViewLogMessages_VirtualItemsSelectionRangeChanged);
            this.listViewLogMessages.SelectedIndexChanged += new System.EventHandler(this.listViewLogMessages_SelectedIndexChanged);
            this.listViewLogMessages.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.listViewLogMessages_ColumnClick);
            this.listViewLogMessages.RetrieveVirtualItem += new System.Windows.Forms.RetrieveVirtualItemEventHandler(this.listViewLogMessages_RetrieveVirtualItem);
            this.listViewLogMessages.ColumnReordered += new System.Windows.Forms.ColumnReorderedEventHandler(this.listViewLogMessages_ColumnReordered);
            // 
            // splitter1
            // 
            this.splitter1.Location = new System.Drawing.Point(200, 0);
            this.splitter1.Name = "splitter1";
            this.splitter1.Size = new System.Drawing.Size(4, 254);
            this.splitter1.TabIndex = 1;
            this.splitter1.TabStop = false;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.treeView);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Left;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(200, 254);
            this.panel1.TabIndex = 3;
            // 
            // splitter2
            // 
            this.splitter2.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.splitter2.Location = new System.Drawing.Point(0, 254);
            this.splitter2.Name = "splitter2";
            this.splitter2.Size = new System.Drawing.Size(656, 3);
            this.splitter2.TabIndex = 3;
            this.splitter2.TabStop = false;
            // 
            // panelSelectedItemMessage
            // 
            this.panelSelectedItemMessage.Controls.Add(this.textBoxSelectedMessageText);
            this.panelSelectedItemMessage.Controls.Add(this.label2);
            this.panelSelectedItemMessage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelSelectedItemMessage.Location = new System.Drawing.Point(331, 3);
            this.panelSelectedItemMessage.Name = "panelSelectedItemMessage";
            this.panelSelectedItemMessage.Size = new System.Drawing.Size(322, 155);
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
            this.textBoxSelectedMessageText.Size = new System.Drawing.Size(322, 132);
            this.textBoxSelectedMessageText.TabIndex = 3;
            this.textBoxSelectedMessageText.WordWrap = false;
            // 
            // label2
            // 
            this.label2.Dock = System.Windows.Forms.DockStyle.Top;
            this.label2.Location = new System.Drawing.Point(0, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(322, 23);
            this.label2.TabIndex = 2;
            this.label2.Text = "Selected Log Event Message Text:";
            this.label2.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
            // 
            // panelSelectedItemProperties
            // 
            this.panelSelectedItemProperties.Controls.Add(this.listviewSelectedLogEventProperties);
            this.panelSelectedItemProperties.Controls.Add(this.buttonPreviousEvent);
            this.panelSelectedItemProperties.Controls.Add(this.buttonNextEvent);
            this.panelSelectedItemProperties.Controls.Add(this.label1);
            this.panelSelectedItemProperties.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelSelectedItemProperties.Location = new System.Drawing.Point(3, 3);
            this.panelSelectedItemProperties.Name = "panelSelectedItemProperties";
            this.panelSelectedItemProperties.Size = new System.Drawing.Size(322, 155);
            this.panelSelectedItemProperties.TabIndex = 0;
            // 
            // listviewSelectedLogEventProperties
            // 
            this.listviewSelectedLogEventProperties.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.listviewSelectedLogEventProperties.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2});
            this.listviewSelectedLogEventProperties.FullRowSelect = true;
            this.listviewSelectedLogEventProperties.GridLines = true;
            this.listviewSelectedLogEventProperties.Location = new System.Drawing.Point(0, 23);
            this.listviewSelectedLogEventProperties.Name = "listviewSelectedLogEventProperties";
            this.listviewSelectedLogEventProperties.Size = new System.Drawing.Size(322, 132);
            this.listviewSelectedLogEventProperties.TabIndex = 0;
            this.listviewSelectedLogEventProperties.UseCompatibleStateImageBehavior = false;
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
            // buttonPreviousEvent
            // 
            this.buttonPreviousEvent.Image = ((System.Drawing.Image)(resources.GetObject("buttonPreviousEvent.Image")));
            this.buttonPreviousEvent.Location = new System.Drawing.Point(0, 0);
            this.buttonPreviousEvent.Name = "buttonPreviousEvent";
            this.buttonPreviousEvent.Size = new System.Drawing.Size(20, 23);
            this.buttonPreviousEvent.TabIndex = 2;
            this.buttonPreviousEvent.UseVisualStyleBackColor = true;
            // 
            // buttonNextEvent
            // 
            this.buttonNextEvent.Image = ((System.Drawing.Image)(resources.GetObject("buttonNextEvent.Image")));
            this.buttonNextEvent.Location = new System.Drawing.Point(19, 0);
            this.buttonNextEvent.Name = "buttonNextEvent";
            this.buttonNextEvent.Size = new System.Drawing.Size(20, 23);
            this.buttonNextEvent.TabIndex = 2;
            this.buttonNextEvent.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(40, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(360, 23);
            this.label1.TabIndex = 1;
            this.label1.Text = "Selected Log Event Properties:";
            this.label1.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
            // 
            // timer1
            // 
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel1,
            this.toolStripStatusEventsInBuffer,
            this.toolStripStatusLabel3,
            this.toolStripStatusEventsDisplayed,
            this.toolStripStatusLabel2,
            this.toolStripStatusTotalEvents,
            this.toolStripStatusLabel5,
            this.toolStripStatusLastEvent});
            this.statusStrip1.Location = new System.Drawing.Point(0, 418);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(656, 22);
            this.statusStrip1.TabIndex = 2;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(88, 17);
            this.toolStripStatusLabel1.Text = "Events in buffer:";
            // 
            // toolStripStatusEventsInBuffer
            // 
            this.toolStripStatusEventsInBuffer.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.clearToolStripMenuItem1,
            this.toolStripMenuItem2});
            this.toolStripStatusEventsInBuffer.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold);
            this.toolStripStatusEventsInBuffer.Name = "toolStripStatusEventsInBuffer";
            this.toolStripStatusEventsInBuffer.Size = new System.Drawing.Size(30, 20);
            this.toolStripStatusEventsInBuffer.Text = "0";
            // 
            // clearToolStripMenuItem1
            // 
            this.clearToolStripMenuItem1.Name = "clearToolStripMenuItem1";
            this.clearToolStripMenuItem1.Size = new System.Drawing.Size(199, 22);
            this.clearToolStripMenuItem1.Text = "&Clear";
            this.clearToolStripMenuItem1.Click += new System.EventHandler(this.clearToolStripMenuItem1_Click);
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new System.Drawing.Size(199, 22);
            this.toolStripMenuItem2.Text = "Change Buffer &Size...";
            this.toolStripMenuItem2.Click += new System.EventHandler(this.toolStripMenuItem2_Click);
            // 
            // toolStripStatusLabel3
            // 
            this.toolStripStatusLabel3.Name = "toolStripStatusLabel3";
            this.toolStripStatusLabel3.Size = new System.Drawing.Size(92, 17);
            this.toolStripStatusLabel3.Text = "Events displayed:";
            // 
            // toolStripStatusEventsDisplayed
            // 
            this.toolStripStatusEventsDisplayed.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.clearToolStripMenuItem});
            this.toolStripStatusEventsDisplayed.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold);
            this.toolStripStatusEventsDisplayed.Name = "toolStripStatusEventsDisplayed";
            this.toolStripStatusEventsDisplayed.Size = new System.Drawing.Size(30, 20);
            this.toolStripStatusEventsDisplayed.Text = "0";
            // 
            // clearToolStripMenuItem
            // 
            this.clearToolStripMenuItem.Name = "clearToolStripMenuItem";
            this.clearToolStripMenuItem.Size = new System.Drawing.Size(114, 22);
            this.clearToolStripMenuItem.Text = "&Clear";
            // 
            // toolStripStatusLabel2
            // 
            this.toolStripStatusLabel2.Name = "toolStripStatusLabel2";
            this.toolStripStatusLabel2.Size = new System.Drawing.Size(115, 17);
            this.toolStripStatusLabel2.Text = "Total events received:";
            // 
            // toolStripStatusTotalEvents
            // 
            this.toolStripStatusTotalEvents.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold);
            this.toolStripStatusTotalEvents.Name = "toolStripStatusTotalEvents";
            this.toolStripStatusTotalEvents.Size = new System.Drawing.Size(14, 17);
            this.toolStripStatusTotalEvents.Text = "0";
            // 
            // toolStripStatusLabel5
            // 
            this.toolStripStatusLabel5.Name = "toolStripStatusLabel5";
            this.toolStripStatusLabel5.Size = new System.Drawing.Size(62, 17);
            this.toolStripStatusLabel5.Text = "Last event:";
            // 
            // toolStripStatusLastEvent
            // 
            this.toolStripStatusLastEvent.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold);
            this.toolStripStatusLastEvent.Name = "toolStripStatusLastEvent";
            this.toolStripStatusLastEvent.Size = new System.Drawing.Size(35, 17);
            this.toolStripStatusLastEvent.Text = "none";
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Controls.Add(this.panelSelectedItemMessage, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.panelSelectedItemProperties, 0, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 257);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(656, 161);
            this.tableLayoutPanel1.TabIndex = 3;
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.resetToDefaultsToolStripMenuItem,
            this.chooseColumnsToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.contextMenuStrip1.ShowImageMargin = false;
            this.contextMenuStrip1.Size = new System.Drawing.Size(221, 48);
            // 
            // resetToDefaultsToolStripMenuItem
            // 
            this.resetToDefaultsToolStripMenuItem.Name = "resetToDefaultsToolStripMenuItem";
            this.resetToDefaultsToolStripMenuItem.Size = new System.Drawing.Size(220, 22);
            this.resetToDefaultsToolStripMenuItem.Text = "Reset To Defaults";
            this.resetToDefaultsToolStripMenuItem.Click += new System.EventHandler(this.resetToDefaultsToolStripMenuItem_Click);
            // 
            // chooseColumnsToolStripMenuItem
            // 
            this.chooseColumnsToolStripMenuItem.Name = "chooseColumnsToolStripMenuItem";
            this.chooseColumnsToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift)
                        | System.Windows.Forms.Keys.C)));
            this.chooseColumnsToolStripMenuItem.Size = new System.Drawing.Size(220, 22);
            this.chooseColumnsToolStripMenuItem.Text = "Choose Columns...";
            this.chooseColumnsToolStripMenuItem.Click += new System.EventHandler(this.chooseColumnsToolStripMenuItem_Click);
            // 
            // SessionTabPage
            // 
            this.ContextMenuStrip = this.contextMenuStrip1;
            this.Controls.Add(this.panelTop);
            this.Controls.Add(this.splitter2);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Controls.Add(this.statusStrip1);
            this.Name = "SessionTabPage";
            this.Size = new System.Drawing.Size(656, 440);
            this.Load += new System.EventHandler(this.SessionTabPage_Load);
            this.panelTop.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panelSelectedItemMessage.ResumeLayout(false);
            this.panelSelectedItemMessage.PerformLayout();
            this.panelSelectedItemProperties.ResumeLayout(false);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.contextMenuStrip1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }
		#endregion

        private void listViewLogMessages_RetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e)
        {
            e.Item = LogEventToListViewItem(_session.GetDisplayedItemForIndex(e.ItemIndex));
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            listViewLogMessages.Refresh();
        }

        private ListViewItem LogEventToListViewItem(LogEvent logEvent)
        {
            ListViewItem item = new ListViewItem();
            item.Tag = logEvent;
            item.Text = "";
            foreach (LogColumn lc in _session.Config.Columns)
            {
                if (lc.Visible)
                    item.SubItems.Add(logEvent[lc.Name]);
            }
            //item.Font = new Font("Tahoma", SystemInformation.IconSize.Height);
            item.ImageIndex = GlobalImageList.Instance.GetImageForLevel(logEvent["Level"]);
            
            if (true)
            {
                switch (logEvent["Level"][0])
                {
                    case 'T':
                        item.ForeColor = Color.Gray;
                        break;

                    case 'D':
                        item.ForeColor = Color.Navy;
                        break;

                    case 'I':
                        item.ForeColor = Color.Black;
                        break;

                    case 'W':
                        item.ForeColor = Color.Brown;
                        break;

                    case 'E':
                        item.ForeColor = Color.Red;
                        break;

                    case 'F':
                        item.ForeColor = Color.Orange;
                        break;
                }
            }

            return item;
        }

        private void listViewLogMessages_VirtualItemsSelectionRangeChanged(object sender, ListViewVirtualItemsSelectionRangeChangedEventArgs e)
        {
        }

        public void UpdateCounters(int eventsInBuffer, int bufferCapacity, int filteredEvents, long totalEvents, string lastEvent)
        {
            toolStripStatusEventsInBuffer.Text = eventsInBuffer.ToString() + "/" + bufferCapacity.ToString();
            toolStripStatusEventsDisplayed.Text = filteredEvents.ToString();
            toolStripStatusTotalEvents.Text = totalEvents.ToString();
            toolStripStatusLastEvent.Text = lastEvent;
        }

        private void SessionTabPage_Load(object sender, EventArgs e)
        {
            listViewLogMessages.SmallImageList = GlobalImageList.Instance.ImageList;
        }

        private void clearToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            _session.Clear();
        }

        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            _session.DisplayChangeBufferSizeDialog();
        }

        public void LogEventSelected(LogEvent evt)
        {
            textBoxSelectedMessageText.Text = evt["Text"];
            ListViewItem item;

            listviewSelectedLogEventProperties.Items.Clear();

            foreach (string key in evt.Properties.Keys)
            {
                item = new ListViewItem(new string[] { key, evt[key] });
                listviewSelectedLogEventProperties.Items.Add(item);
            }
        }

        private void listViewLogMessages_SelectedIndexChanged(object sender, EventArgs e)
        {
            ListView.SelectedIndexCollection sic = listViewLogMessages.SelectedIndices;

            if (sic.Count == 1)
                LogEventSelected(_session.GetDisplayedItemForIndex(sic[0]));
        }

        private void listViewLogMessages_ItemActivate(object sender, EventArgs e)
        {
        }

        private void listViewLogMessages_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            ColumnHeader column = listViewLogMessages.Columns[e.Column];
            string columnName = column.Text;

            if (_session.Config.OrderBy == columnName)
            {
                _session.Config.SortAscending = !_session.Config.SortAscending;
            }
            else
            {
                _session.Config.SortAscending = true;
                _session.Config.OrderBy = columnName;
            }
            UpdateSortArrows();
        }

        private void UpdateSortArrows()
        {
            foreach (ColumnHeader ch in listViewLogMessages.Columns)
            {
                if (ch.Text == _session.Config.OrderBy)
                {
                    ch.ImageIndex = _session.Config.SortAscending ? 0 : 1;
                }
                else
                {
                    ch.ImageIndex = -1;
                }
            }
        }

        public void UpdateFonts()
        {
            listViewLogMessages.Font = AppPreferences.LogMessagesFont;
        }

        public void FitColumnWidths()
        {
            listViewLogMessages.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
        }

        public void ReloadColumns()
        {
            listViewLogMessages.Columns.Clear();
            listViewLogMessages.Columns.Add("#", 24, HorizontalAlignment.Center);
            int oldSize = listViewLogMessages.VirtualListSize;

            listViewLogMessages.VirtualListSize = 0;
            foreach (LogColumn lc in _session.Config.Columns)
            {
                if (lc.Visible)
                {
                    ColumnHeader ch = new ColumnHeader();
                    ch.Text = lc.Name;
                    ch.Width = lc.Width;
                    ch.Tag = lc;
                    listViewLogMessages.Columns.Add(ch);
                };
            }
            listViewLogMessages.VirtualListSize = oldSize;
            listViewLogMessages.Invalidate();
        }

        public void ChooseColumns()
        {
            using (ChooseColumnsDialog dlg = new ChooseColumnsDialog())
            {
                dlg.Configuration = _session.Config;
                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    ReloadColumns();
                }
            }
        }

        private void chooseColumnsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ChooseColumns();
        }

        private void resetToDefaultsToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }

        private void listViewLogMessages_ColumnWidthChanged(object sender, ColumnWidthChangedEventArgs e)
        {
            LogColumn lc = listViewLogMessages.Columns[e.ColumnIndex].Tag as LogColumn;
            if (lc != null)
            {
                lc.Width = listViewLogMessages.Columns[e.ColumnIndex].Width;
            }
        }

        private void listViewLogMessages_ColumnReordered(object sender, ColumnReorderedEventArgs e)
        {
            LogColumn lc = e.Header.Tag as LogColumn;
            if (lc == null)
            {
                e.Cancel = true;
                return;
            }

            logger.Debug("Moving '{0}' from {1} to {2}", lc.Name, e.OldDisplayIndex, e.NewDisplayIndex);
            if (e.NewDisplayIndex > e.OldDisplayIndex)
            {
                _session.Config.Columns.Insert(e.NewDisplayIndex, lc);
                _session.Config.Columns.RemoveAt(e.OldDisplayIndex - 1);
            }
            else
            {
                _session.Config.Columns.RemoveAt(e.OldDisplayIndex - 1);
                _session.Config.Columns.Insert(e.NewDisplayIndex - 1, lc);
            }

            e.Cancel = true;
            ReloadColumns();
        }
    }
}
