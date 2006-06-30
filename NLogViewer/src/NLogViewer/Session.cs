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
using System.Threading;
using System.Windows.Forms;
using System.Text;
using System.IO;
using System.Collections;
using System.Drawing;

using NLogViewer.Receivers;
using NLogViewer.Configuration;
using NLogViewer.UI;
using NLogViewer.Events;
using System.Collections.Generic;
using NLogViewer.Parsers;

namespace NLogViewer
{
    public class Session : ILogEventProcessor
    {
        private SessionConfiguration _config;
        private ILogEventReceiver _receiver;
        private MainForm _mainForm;
        private TabPage _tabPage;
        private SessionTabPage _tabPanel;

        public SessionTabPage TabPanel
        {
            get { return _tabPanel; }
        }

        private TreeNode _threadsTreeNode;
        private TreeNode _assembliesTreeNode;
        private TreeNode _classesTreeNode;
        private TreeNode _loggersTreeNode;
        private TreeNode _levelsTreeNode;
        private TreeNode _applicationsTreeNode;
        private TreeNode _machinesTreeNode;
        private TreeNode _filesTreeNode;

        private Hashtable _logger2NodeCache = new Hashtable();
        private Hashtable _level2NodeCache = new Hashtable();
        private Hashtable _thread2NodeCache = new Hashtable();
        private Hashtable _assembly2NodeCache = new Hashtable();
        private Hashtable _class2NodeCache = new Hashtable();
        private Hashtable _application2NodeCache = new Hashtable();
        private Hashtable _machine2NodeCache = new Hashtable();
        private Hashtable _file2NodeCache = new Hashtable();

        private string _lastEventInfo = "none";

        private CyclicBuffer<LogEvent> _bufferedEvents;
        private List<LogEvent> _newEvents = new List<LogEvent>();
        private SortedList<LogEvent, LogEvent> _filteredEvents;
        private bool _haveNewEvents = false;

        delegate void AddTreeNodeDelegate(TreeNode parentNode, TreeNode childNode);
        private AddTreeNodeDelegate _addTreeNodeDelegate;
        private long _totalEvents = 0;

        public Session(SessionConfiguration config)
        {
            _config = config;
            _bufferedEvents = new CyclicBuffer<LogEvent>(config.MaxLogEntries);
            NewSortOrder();
            _receiver = LogReceiverFactory.CreateLogReceiver(config.ReceiverType, config.ReceiverParameters);
            if (_receiver is ILogEventReceiverWithParser)
            {
                ((ILogEventReceiverWithParser)_receiver).Parser = LogEventParserFactory.CreateLogParser(config.ParserType, config.ParserParameters);
            }
            _receiver.Connect(this);
            _addTreeNodeDelegate = new AddTreeNodeDelegate(this.AddTreeNode);
        }

        public SessionConfiguration Config
        {
            get { return _config; }
        }

        public ILogEventReceiver Receiver
        {
            get { return _receiver; }
        }

        public LogEvent GetDisplayedItemForIndex(int pos)
        {
            lock (this)
            {
                return _filteredEvents.Keys[pos];
            }
        }

        public void CreateTab(MainForm form)
        {
            _mainForm = form;

            TabPage page = new TabPage();
            _tabPage = page;
            page.ImageIndex = 1;
            page.Tag = this;

            SessionTabPage tabPanel = new SessionTabPage(this);
            tabPanel.Dock = DockStyle.Fill;
            page.Controls.Add(tabPanel);
            _tabPanel = tabPanel;

            _loggersTreeNode = new TreeNode("Loggers");
            _levelsTreeNode = new TreeNode("Levels");
            _threadsTreeNode = new TreeNode("Threads");
            _assembliesTreeNode = new TreeNode("Assemblies");
            _classesTreeNode = new TreeNode("Classes");
            _applicationsTreeNode = new TreeNode("Applications");
            _machinesTreeNode = new TreeNode("Machines");
            _filesTreeNode = new TreeNode("Files");

            TreeView treeView = _tabPanel.treeView;
            treeView.Nodes.Add(_loggersTreeNode);
            treeView.Nodes.Add(_levelsTreeNode);
            treeView.Nodes.Add(_threadsTreeNode);
            treeView.Nodes.Add(_assembliesTreeNode);
            treeView.Nodes.Add(_classesTreeNode);
            treeView.Nodes.Add(_filesTreeNode);
            treeView.Nodes.Add(_applicationsTreeNode);
            treeView.Nodes.Add(_machinesTreeNode);
            page.Text = Config.Name;
            TabPanel.ReloadColumns();
        }

        public void Start()
        {
            _receiver.Start();
        }

        public void Stop()
        {
            _receiver.Stop();
        }

        public string StatusText
        {
            get { return _receiver.StatusText; }
        }

        public TabPage TabPage
        {
            get { return _tabPage; }
        }

        private TreeNode LogEventAttributeToNode(string attributeValue, TreeNode rootNode, Hashtable cache, char separatorChar)
        {
            if (attributeValue == null)
                return null;

            object o = cache[attributeValue];
            if (o != null)
            {
                return (TreeNode)o;
            }

            TreeNode parentNode;

            string baseName;
            int rightmostDot = -1;
            if (separatorChar != 0)
                rightmostDot = attributeValue.LastIndexOf(separatorChar);
            if (rightmostDot < 0)
            {
                parentNode = rootNode;
                baseName = attributeValue;
            }
            else
            {
                string parentLoggerName = attributeValue.Substring(0, rightmostDot);
                baseName = attributeValue.Substring(rightmostDot + 1);
                parentNode = LogEventAttributeToNode(parentLoggerName, rootNode, cache, separatorChar);
            }

            TreeNode newNode = new TreeNode(baseName);
            cache[attributeValue] = newNode;
            TabPanel.treeView.Invoke(_addTreeNodeDelegate, new object[] { parentNode, newNode });
            return newNode;
        }

        private void AddTreeNode(TreeNode parentNode, TreeNode childNode)
        {
            parentNode.Nodes.Add(childNode);
            //if (parentNode.Parent == null || parentNode.Parent.Parent == null)
            //    parentNode.Expand();
        }

        private void ProcessNewEvents()
        {
            List<LogEvent> logEventsToProcess = null;

            lock (this)
            {
                if (_haveNewEvents)
                {
                    logEventsToProcess = _newEvents;
                    _newEvents = new List<LogEvent>();
                    _haveNewEvents = false;
                }
            }

            if (logEventsToProcess != null)
            {
                foreach (LogEvent logEvent in logEventsToProcess)
                {
                    LogEvent removedEvent = _bufferedEvents.AddAndRemoveLast(logEvent);
                    if (removedEvent != null)
                        _filteredEvents.Remove(removedEvent);

                    if (TryFilters(logEvent))
                    {
                        _filteredEvents.Add(logEvent, logEvent);
                    }

                    foreach (string s in logEvent.Properties.Keys)
                    {
                        if (!_config.ContainsColumn(s))
                        {
                            LogColumn lc = new LogColumn();
                            lc.Name = s;
                            lc.Visible = false;
                            lc.Width = 100;
                            _config.Columns.Add(lc);
                        }
                    }

                    _totalEvents++;

                    // LogEventAttributeToNode(logEvent["Level"], _levelsTreeNode, _level2NodeCache, (char)0);
                    LogEventAttributeToNode((string)logEvent["Logger"], _loggersTreeNode, _logger2NodeCache, '.');
                    LogEventAttributeToNode((string)logEvent["SourceAssembly"], _assembliesTreeNode, _assembly2NodeCache, (char)0);
                    TreeNode node = LogEventAttributeToNode((string)logEvent["SourceType"], _classesTreeNode, _class2NodeCache, '.');
                    // LogEventAttributeToNode(logEvent.SourceMethod, node, 
                    LogEventAttributeToNode((string)logEvent["Thread"], _threadsTreeNode, _thread2NodeCache, (char)0);
                    LogEventAttributeToNode((string)logEvent["SourceApplication"], _applicationsTreeNode, _application2NodeCache, (char)0);
                    LogEventAttributeToNode((string)logEvent["SourceMachine"], _machinesTreeNode, _machine2NodeCache, (char)0);
                    LogEventAttributeToNode((string)logEvent["SourceFile"], _filesTreeNode, _file2NodeCache, (char)'\\');
                }
                TabPanel.listViewLogMessages.VirtualListSize = _filteredEvents.Count;
                TabPanel.listViewLogMessages.Invalidate();
                UpdateStatusBar();
            }
        }

        private void UpdateStatusBar()
        {
            TabPanel.UpdateCounters(_bufferedEvents.Count, _bufferedEvents.Capacity, _filteredEvents.Count, _totalEvents, _lastEventInfo);
        }

        public void OnTimer()
        {
            ProcessNewEvents();
        }

        private bool TryFilters(LogEvent logEvent)
        {
            //if (logEvent.Level == "TRACE")
            //    return false;
            return true;
        }

        private static int _globalEventID = 0;

        public void Clear()
        {
            lock (this)
            {
                _bufferedEvents.Clear();
                _filteredEvents.Clear();
                _haveNewEvents = true;
                _totalEvents = 0;
                TabPanel.listViewLogMessages.VirtualListSize = 0;
                UpdateStatusBar();
            }
        }

        public void ChangeBufferSize(int newBufferSize)
        {
            lock (this)
            {
                CyclicBuffer<LogEvent> newEvents = new CyclicBuffer<LogEvent>(newBufferSize);
                newEvents.CopyTo(newEvents);
                //if (newBufferSize > 
                _bufferedEvents = newEvents;
            }
        }

        public void ProcessLogEvent(LogEvent logEvent)
        {
            logEvent.ID = Interlocked.Increment(ref _globalEventID);
            logEvent["ID"] = logEvent.ID;

            lock (this)
            {
                _newEvents.Add(logEvent);
                _haveNewEvents = true;
            }

            //LogEventAttributeToNode(logEvent.SourceType, _typesTreeNode, _type);
        }
       
        class ItemComparer : IComparer<LogEvent>
        {
            private string _column;
            private bool _ascending;

            public ItemComparer(string column, bool ascending)
            {
                _column = column;
                _ascending = ascending;
            }

            public int Compare(LogEvent x, LogEvent y)
            {
                object v1 = x.Properties[_column];
                object v2 = y.Properties[_column];

                if (v1 == null)
                {
                    if (v2 != null)
                        return _ascending ? -1 : 1;
                }
                else
                {
                    if (v2 == null)
                        return _ascending ? 1 : -1;
                }

                if (v1 != null)
                {
                    int result = ((IComparable)v1).CompareTo(v2);
                    if (result != 0)
                    {
                        if (_ascending)
                            return result;
                        else
                            return -result;
                    }
                }

                // by default order by ID
                if (_ascending)
                    return (x.ID - y.ID);
                else
                    return -(x.ID - y.ID);
            }
        }

        public void DisplayChangeBufferSizeDialog()
        {
            using (ChangeBufferSizeDialog dlg = new ChangeBufferSizeDialog())
            {
                dlg.BufferSize = _bufferedEvents.Capacity;
                if (dlg.ShowDialog(TabPage) == DialogResult.OK)
                {
                    ChangeBufferSize(dlg.BufferSize);
                }
            }
        }

        public void UpdateFonts()
        {
            TabPanel.UpdateFonts();
        }

        private bool CaptureParametersAndSaveConfig(string fileName)
        {
            Config.ReceiverParameters = ConfigurationParameter.CaptureConfigurationParameters(_receiver);
            if (_receiver is ILogEventReceiverWithParser)
            {
                Config.ParserParameters = ConfigurationParameter.CaptureConfigurationParameters(((ILogEventReceiverWithParser)_receiver).Parser);
            }
            AppPreferences.AddToRecentFileList(fileName);
            return Config.Save(fileName);
        }

        public bool Save(IWin32Window parent)
        {
            if (Config.FileName == null)
                return SaveAs(parent);
            return CaptureParametersAndSaveConfig(Config.FileName);
        }

        public bool SaveAs(IWin32Window parent)
        {
            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Filter = "NLogViewer Sessions (*.nlv)|*.nlv|All Files (*.*)|*.*";
                if (Config.FileName != null)
                    sfd.FileName = Config.FileName;
                if (sfd.ShowDialog(parent) == DialogResult.OK)
                {
                    Config.Name = Path.GetFileNameWithoutExtension(sfd.FileName);
                    TabPage.Text = Config.Name;
                    return CaptureParametersAndSaveConfig(sfd.FileName);
                }
            }
            return false;
        }

        public void NewSortOrder()
        {
            lock (this)
            {
                SortedList<LogEvent, LogEvent> newFilteredEvents = new SortedList<LogEvent, LogEvent>(new ItemComparer(Config.OrderBy, Config.SortAscending));

                if (_filteredEvents != null)
                {
                    foreach (LogEvent ev in _filteredEvents.Keys)
                    {
                        newFilteredEvents.Add(ev, ev);
                    }
                }
                _filteredEvents = newFilteredEvents;
            }
        }

        public bool Close()
        {
            if (_receiver.CanStop())
                Stop();

            if (Config.Dirty)
            {
                switch (MessageBox.Show(TabPanel,
                    "Session '" + Config.Name + "' has unsaved changes. Save before exit?",
                    "NLogViewer",
                    MessageBoxButtons.YesNoCancel))
                {
                    case DialogResult.Yes:
                        if (!Save(TabPanel))
                            return false;
                        break;

                    case DialogResult.Cancel:
                        return false;

                }
            }
            _mainForm.RemoveSession(this);
            return true;
        }
    }
}
