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
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Panel splitContainerTop;
        private System.Windows.Forms.Panel panelSelectedItemMessage;
        private System.Windows.Forms.Panel panelSelectedItemProperties;
        public System.Windows.Forms.ListView listviewSelectedLogEventProperties;
        public System.Windows.Forms.TextBox textBoxSelectedMessageText;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private IContainer components;
        private Timer timer1;
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
        private ContextMenuStrip contextMenuStrip1;
        private ToolStripMenuItem chooseColumnsToolStripMenuItem;
        private ToolStripMenuItem resetToDefaultsToolStripMenuItem;
        private SplitContainer splitContainer1;
        public TreeView treeView;
        public ListView listViewLogMessages;
        private SplitContainer splitContainerBottom;
        private SplitContainer mainSplitContainer;
        private ToolStrip toolStrip1;
        private ToolStripButton toolStripButtonShowTree;
        private ToolStripButton toolStripButtonShowDetails;
        private ToolStripLabel toolStripLabelStatus0;
        private ToolStripButton toolStripButtonStop;
        private ToolStripButton toolStripButtonStart;
        private ToolStripButton toolStripButtonRefresh;
        private ToolStripSeparator toolStripSeparator4;
        private ToolStripButton toolStripButtonClear;
        private ToolStripSplitButton toolStripButtonFilter;
        private ToolStripSplitButton toolStripButtonHighlight;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripButton toolStripButtonFind;
        private ToolStripButton toolStripButtonFindNext;
        private ToolStripTextBox toolStripTextBoxFindText;
        private ToolStripSeparator toolStripSeparator2;
        private ToolStripButton toolStripButtonFitColumnWidths;
        private ToolStripButton toolStripButtonChooseColumns;
        private ToolStripSeparator toolStripSeparator3;
        private ToolStripLabel toolStripLabelStatus;
        private ToolStripContainer toolStripContainer1;
        private ToolStripButton toolStripButtonProperties;
        private ToolStripButton toolStripButtonPause;
        private LogEvent _selectedLogEvent = null;

		public SessionTabPage(Session instance)
		{
            _session = instance;
			InitializeComponent();
            listViewLogMessages.SmallImageList = GlobalImageList.Instance.ImageList;
            treeView.ImageList = GlobalImageList.Instance.ImageList;
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
            this.splitContainerTop = new System.Windows.Forms.Panel();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.treeView = new System.Windows.Forms.TreeView();
            this.listViewLogMessages = new System.Windows.Forms.ListView();
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
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.resetToDefaultsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.chooseColumnsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.splitContainerBottom = new System.Windows.Forms.SplitContainer();
            this.mainSplitContainer = new System.Windows.Forms.SplitContainer();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.toolStripLabelStatus0 = new System.Windows.Forms.ToolStripLabel();
            this.toolStripLabelStatus = new System.Windows.Forms.ToolStripLabel();
            this.toolStripButtonStart = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonPause = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonStop = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonRefresh = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonProperties = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripButtonClear = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonFilter = new System.Windows.Forms.ToolStripSplitButton();
            this.toolStripButtonHighlight = new System.Windows.Forms.ToolStripSplitButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripButtonFind = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonFindNext = new System.Windows.Forms.ToolStripButton();
            this.toolStripTextBoxFindText = new System.Windows.Forms.ToolStripTextBox();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripButtonFitColumnWidths = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonChooseColumns = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripButtonShowTree = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonShowDetails = new System.Windows.Forms.ToolStripButton();
            this.toolStripContainer1 = new System.Windows.Forms.ToolStripContainer();
            this.splitContainerTop.SuspendLayout();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.panelSelectedItemMessage.SuspendLayout();
            this.panelSelectedItemProperties.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.contextMenuStrip1.SuspendLayout();
            this.splitContainerBottom.Panel1.SuspendLayout();
            this.splitContainerBottom.Panel2.SuspendLayout();
            this.splitContainerBottom.SuspendLayout();
            this.mainSplitContainer.Panel1.SuspendLayout();
            this.mainSplitContainer.Panel2.SuspendLayout();
            this.mainSplitContainer.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            this.toolStripContainer1.BottomToolStripPanel.SuspendLayout();
            this.toolStripContainer1.ContentPanel.SuspendLayout();
            this.toolStripContainer1.TopToolStripPanel.SuspendLayout();
            this.toolStripContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainerTop
            // 
            this.splitContainerTop.Controls.Add(this.splitContainer1);
            this.splitContainerTop.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainerTop.Location = new System.Drawing.Point(0, 0);
            this.splitContainerTop.Name = "splitContainerTop";
            this.splitContainerTop.Size = new System.Drawing.Size(656, 200);
            this.splitContainerTop.TabIndex = 1;
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.treeView);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.listViewLogMessages);
            this.splitContainer1.Size = new System.Drawing.Size(656, 200);
            this.splitContainer1.SplitterDistance = 218;
            this.splitContainer1.TabIndex = 1;
            // 
            // treeView
            // 
            this.treeView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeView.Location = new System.Drawing.Point(0, 0);
            this.treeView.Name = "treeView";
            this.treeView.Size = new System.Drawing.Size(218, 200);
            this.treeView.TabIndex = 0;
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
            this.listViewLogMessages.Size = new System.Drawing.Size(434, 200);
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
            // panelSelectedItemMessage
            // 
            this.panelSelectedItemMessage.Controls.Add(this.textBoxSelectedMessageText);
            this.panelSelectedItemMessage.Controls.Add(this.label2);
            this.panelSelectedItemMessage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelSelectedItemMessage.Location = new System.Drawing.Point(0, 0);
            this.panelSelectedItemMessage.Name = "panelSelectedItemMessage";
            this.panelSelectedItemMessage.Size = new System.Drawing.Size(434, 189);
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
            this.textBoxSelectedMessageText.Size = new System.Drawing.Size(434, 166);
            this.textBoxSelectedMessageText.TabIndex = 3;
            this.textBoxSelectedMessageText.WordWrap = false;
            // 
            // label2
            // 
            this.label2.Dock = System.Windows.Forms.DockStyle.Top;
            this.label2.Location = new System.Drawing.Point(0, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(434, 23);
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
            this.panelSelectedItemProperties.Location = new System.Drawing.Point(0, 0);
            this.panelSelectedItemProperties.Name = "panelSelectedItemProperties";
            this.panelSelectedItemProperties.Size = new System.Drawing.Size(218, 189);
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
            this.listviewSelectedLogEventProperties.Size = new System.Drawing.Size(218, 166);
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
            this.timer1.Enabled = true;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // statusStrip1
            // 
            this.statusStrip1.Dock = System.Windows.Forms.DockStyle.None;
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel1,
            this.toolStripStatusEventsInBuffer,
            this.toolStripStatusLabel3,
            this.toolStripStatusEventsDisplayed,
            this.toolStripStatusLabel2,
            this.toolStripStatusTotalEvents,
            this.toolStripStatusLabel5,
            this.toolStripStatusLastEvent});
            this.statusStrip1.Location = new System.Drawing.Point(0, 0);
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
            this.clearToolStripMenuItem.Click += new System.EventHandler(this.clearToolStripMenuItem_Click);
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
            // splitContainerBottom
            // 
            this.splitContainerBottom.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainerBottom.Location = new System.Drawing.Point(0, 0);
            this.splitContainerBottom.Name = "splitContainerBottom";
            // 
            // splitContainerBottom.Panel1
            // 
            this.splitContainerBottom.Panel1.Controls.Add(this.panelSelectedItemProperties);
            // 
            // splitContainerBottom.Panel2
            // 
            this.splitContainerBottom.Panel2.Controls.Add(this.panelSelectedItemMessage);
            this.splitContainerBottom.Size = new System.Drawing.Size(656, 189);
            this.splitContainerBottom.SplitterDistance = 218;
            this.splitContainerBottom.TabIndex = 3;
            // 
            // mainSplitContainer
            // 
            this.mainSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mainSplitContainer.Location = new System.Drawing.Point(0, 0);
            this.mainSplitContainer.Name = "mainSplitContainer";
            this.mainSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // mainSplitContainer.Panel1
            // 
            this.mainSplitContainer.Panel1.Controls.Add(this.splitContainerTop);
            this.mainSplitContainer.Panel1MinSize = 200;
            // 
            // mainSplitContainer.Panel2
            // 
            this.mainSplitContainer.Panel2.Controls.Add(this.splitContainerBottom);
            this.mainSplitContainer.Size = new System.Drawing.Size(656, 393);
            this.mainSplitContainer.SplitterDistance = 200;
            this.mainSplitContainer.TabIndex = 1;
            // 
            // toolStrip1
            // 
            this.toolStrip1.Dock = System.Windows.Forms.DockStyle.None;
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripLabelStatus0,
            this.toolStripLabelStatus,
            this.toolStripButtonStart,
            this.toolStripButtonPause,
            this.toolStripButtonStop,
            this.toolStripButtonRefresh,
            this.toolStripButtonProperties,
            this.toolStripSeparator4,
            this.toolStripButtonClear,
            this.toolStripButtonFilter,
            this.toolStripButtonHighlight,
            this.toolStripSeparator1,
            this.toolStripButtonFind,
            this.toolStripButtonFindNext,
            this.toolStripTextBoxFindText,
            this.toolStripSeparator2,
            this.toolStripButtonFitColumnWidths,
            this.toolStripButtonChooseColumns,
            this.toolStripSeparator3,
            this.toolStripButtonShowTree,
            this.toolStripButtonShowDetails});
            this.toolStrip1.Location = new System.Drawing.Point(3, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.toolStrip1.Size = new System.Drawing.Size(649, 25);
            this.toolStrip1.TabIndex = 2;
            // 
            // toolStripLabelStatus0
            // 
            this.toolStripLabelStatus0.Name = "toolStripLabelStatus0";
            this.toolStripLabelStatus0.Size = new System.Drawing.Size(87, 22);
            this.toolStripLabelStatus0.Text = "Receiver Status:";
            // 
            // toolStripLabelStatus
            // 
            this.toolStripLabelStatus.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.toolStripLabelStatus.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(192)))));
            this.toolStripLabelStatus.Name = "toolStripLabelStatus";
            this.toolStripLabelStatus.Size = new System.Drawing.Size(53, 22);
            this.toolStripLabelStatus.Text = "Running";
            // 
            // toolStripButtonStart
            // 
            this.toolStripButtonStart.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButtonStart.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButtonStart.Image")));
            this.toolStripButtonStart.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonStart.Name = "toolStripButtonStart";
            this.toolStripButtonStart.Size = new System.Drawing.Size(23, 22);
            this.toolStripButtonStart.Text = "Start/Continue";
            this.toolStripButtonStart.Click += new System.EventHandler(this.toolStripButtonStart_Click);
            // 
            // toolStripButtonPause
            // 
            this.toolStripButtonPause.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButtonPause.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButtonPause.Image")));
            this.toolStripButtonPause.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonPause.Name = "toolStripButtonPause";
            this.toolStripButtonPause.Size = new System.Drawing.Size(23, 22);
            this.toolStripButtonPause.Text = "Pause";
            // 
            // toolStripButtonStop
            // 
            this.toolStripButtonStop.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButtonStop.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButtonStop.Image")));
            this.toolStripButtonStop.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonStop.Name = "toolStripButtonStop";
            this.toolStripButtonStop.Size = new System.Drawing.Size(23, 22);
            this.toolStripButtonStop.Text = "Stop";
            this.toolStripButtonStop.Click += new System.EventHandler(this.toolStripButtonStop_Click);
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
            // toolStripButtonProperties
            // 
            this.toolStripButtonProperties.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButtonProperties.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButtonProperties.Image")));
            this.toolStripButtonProperties.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonProperties.Name = "toolStripButtonProperties";
            this.toolStripButtonProperties.Size = new System.Drawing.Size(23, 22);
            this.toolStripButtonProperties.Text = "Receiver Properties";
            this.toolStripButtonProperties.Click += new System.EventHandler(this.toolStripButtonProperties_Click);
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
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(6, 25);
            // 
            // toolStripButtonShowTree
            // 
            this.toolStripButtonShowTree.Checked = true;
            this.toolStripButtonShowTree.CheckState = System.Windows.Forms.CheckState.Checked;
            this.toolStripButtonShowTree.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButtonShowTree.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButtonShowTree.Image")));
            this.toolStripButtonShowTree.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonShowTree.Name = "toolStripButtonShowTree";
            this.toolStripButtonShowTree.Size = new System.Drawing.Size(23, 22);
            this.toolStripButtonShowTree.Text = "Show Tree";
            this.toolStripButtonShowTree.Click += new System.EventHandler(this.toolStripButtonShowTree_Click);
            // 
            // toolStripButtonShowDetails
            // 
            this.toolStripButtonShowDetails.Checked = true;
            this.toolStripButtonShowDetails.CheckState = System.Windows.Forms.CheckState.Checked;
            this.toolStripButtonShowDetails.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButtonShowDetails.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButtonShowDetails.Image")));
            this.toolStripButtonShowDetails.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonShowDetails.Name = "toolStripButtonShowDetails";
            this.toolStripButtonShowDetails.Size = new System.Drawing.Size(23, 22);
            this.toolStripButtonShowDetails.Text = "Show Details";
            this.toolStripButtonShowDetails.Click += new System.EventHandler(this.toolStripButtonShowDetails_Click);
            // 
            // toolStripContainer1
            // 
            // 
            // toolStripContainer1.BottomToolStripPanel
            // 
            this.toolStripContainer1.BottomToolStripPanel.Controls.Add(this.statusStrip1);
            // 
            // toolStripContainer1.ContentPanel
            // 
            this.toolStripContainer1.ContentPanel.Controls.Add(this.mainSplitContainer);
            this.toolStripContainer1.ContentPanel.Size = new System.Drawing.Size(656, 393);
            this.toolStripContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.toolStripContainer1.Location = new System.Drawing.Point(0, 0);
            this.toolStripContainer1.Name = "toolStripContainer1";
            this.toolStripContainer1.Size = new System.Drawing.Size(656, 440);
            this.toolStripContainer1.TabIndex = 3;
            this.toolStripContainer1.Text = "toolStripContainer1";
            // 
            // toolStripContainer1.TopToolStripPanel
            // 
            this.toolStripContainer1.TopToolStripPanel.Controls.Add(this.toolStrip1);
            this.toolStripContainer1.TopToolStripPanel.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            // 
            // SessionTabPage
            // 
            this.ContextMenuStrip = this.contextMenuStrip1;
            this.Controls.Add(this.toolStripContainer1);
            this.Name = "SessionTabPage";
            this.Size = new System.Drawing.Size(656, 440);
            this.Load += new System.EventHandler(this.SessionTabPage_Load);
            this.splitContainerTop.ResumeLayout(false);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.ResumeLayout(false);
            this.panelSelectedItemMessage.ResumeLayout(false);
            this.panelSelectedItemMessage.PerformLayout();
            this.panelSelectedItemProperties.ResumeLayout(false);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.contextMenuStrip1.ResumeLayout(false);
            this.splitContainerBottom.Panel1.ResumeLayout(false);
            this.splitContainerBottom.Panel2.ResumeLayout(false);
            this.splitContainerBottom.ResumeLayout(false);
            this.mainSplitContainer.Panel1.ResumeLayout(false);
            this.mainSplitContainer.Panel2.ResumeLayout(false);
            this.mainSplitContainer.ResumeLayout(false);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.toolStripContainer1.BottomToolStripPanel.ResumeLayout(false);
            this.toolStripContainer1.BottomToolStripPanel.PerformLayout();
            this.toolStripContainer1.ContentPanel.ResumeLayout(false);
            this.toolStripContainer1.TopToolStripPanel.ResumeLayout(false);
            this.toolStripContainer1.TopToolStripPanel.PerformLayout();
            this.toolStripContainer1.ResumeLayout(false);
            this.toolStripContainer1.PerformLayout();
            this.ResumeLayout(false);

        }
		#endregion

        private void listViewLogMessages_RetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e)
        {
            e.Item = LogEventToListViewItem(_session.GetDisplayedItemForIndex(e.ItemIndex));
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            toolStripButtonStart.Enabled = _session.Receiver.CanStart() || _session.Receiver.CanResume();
            toolStripButtonStop.Enabled = _session.Receiver.CanStop();
            toolStripButtonPause.Enabled = _session.Receiver.CanPause();
            toolStripButtonRefresh.Enabled = _session.Receiver.CanRefresh();
            toolStripLabelStatus.Text = _session.Receiver.StatusText;
            IWizardConfigurable wc = _session.Receiver as IWizardConfigurable;
            if (wc != null)
                toolStripButtonRefresh.Enabled = wc != null;
        }

        private ListViewItem LogEventToListViewItem(LogEvent logEvent)
        {
            ListViewItem item = new ListViewItem();
            item.Tag = logEvent;
            item.Text = "";
            foreach (LogColumn lc in _session.Columns)
            {
                if (lc.Visible)
                    item.SubItems.Add(Convert.ToString(logEvent[lc.Name]));
            }
            //item.Font = new Font("Tahoma", SystemInformation.IconSize.Height);
            LogLevel level = logEvent["Level"] as LogLevel;
            if (level != null)
            {
                item.ImageIndex = level.ImageIndex;
                if (level.Color != Color.Empty)
                    item.ForeColor = level.Color;
                if (level.BackColor != Color.Empty)
                    item.BackColor = level.BackColor;
            }
            else
                item.ImageIndex = -1;

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
            UpdateSortArrows();

            if (!_session.ShowDetails)
                toolStripButtonShowDetails_Click(null, null);
            if (!_session.ShowTree)
                toolStripButtonShowTree_Click(null, null);
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
            textBoxSelectedMessageText.Text = Convert.ToString(evt["Text"]);
            ListViewItem item;

            listviewSelectedLogEventProperties.Items.Clear();

            foreach (LogColumn lc in _session.Columns)
            {
                item = new ListViewItem(new string[] { lc.Name, Convert.ToString(evt[lc.Ordinal]) });
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

            if (_session.OrderBy == columnName)
            {
                _session.SortAscending = !_session.SortAscending;
            }
            else
            {
                _session.SortAscending = false;
                _session.OrderBy = columnName;
            }
            _session.NewSortOrder();
            listViewLogMessages.Invalidate();
            UpdateSortArrows();
        }

        private void UpdateSortArrows()
        {
            foreach (ColumnHeader ch in listViewLogMessages.Columns)
            {
                if (ch.Text == _session.OrderBy)
                {
                    ch.ImageIndex = _session.SortAscending ? 1 : 2;
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
            foreach (LogColumn lc in _session.Columns)
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
                dlg.Session = _session;
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
                _session.Columns.Insert(e.NewDisplayIndex, lc);
                _session.Columns.RemoveAt(e.OldDisplayIndex - 1);
            }
            else
            {
                _session.Columns.RemoveAt(e.OldDisplayIndex - 1);
                _session.Columns.Insert(e.NewDisplayIndex - 1, lc);
            }

            e.Cancel = true;
            ReloadColumns();
        }

        private void toolStripButtonShowDetails_Click(object sender, EventArgs e)
        {
            toolStripButtonShowDetails.Checked = !toolStripButtonShowDetails.Checked;
            mainSplitContainer.Panel2Collapsed = !toolStripButtonShowDetails.Checked;

            _session.ShowDetails = toolStripButtonShowDetails.Checked;
            _session.Dirty = true;
        }

        private void toolStripButtonShowTree_Click(object sender, EventArgs e)
        {
            toolStripButtonShowTree.Checked = !toolStripButtonShowTree.Checked;
            splitContainer1.Panel1Collapsed = !toolStripButtonShowTree.Checked;
            _session.ShowTree = toolStripButtonShowTree.Checked;
            _session.Dirty = true;
        }

        private void toolStripButtonChooseColumns_Click(object sender, EventArgs e)
        {
            ChooseColumns();
        }

        private void toolStripButtonFitColumnWidths_Click(object sender, EventArgs e)
        {
            FitColumnWidths();
        }

        private void toolStripButtonClear_Click(object sender, EventArgs e)
        {
            _session.Clear();
        }

        private void toolStripButtonStart_Click(object sender, EventArgs e)
        {
            _session.Start();
        }

        private void toolStripButtonStop_Click(object sender, EventArgs e)
        {
            _session.Stop();
        }

        private void clearToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _session.Clear();
        }

        private void toolStripButtonProperties_Click(object sender, EventArgs e)
        {
            using (ReceiverPropertiesDialog rpd = new ReceiverPropertiesDialog(_session.Receiver))
            {
                rpd.ShowDialog(this);
            }
        }
    }
}
