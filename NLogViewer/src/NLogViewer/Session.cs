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
using System.Xml.Serialization;
using System.Xml;

namespace NLogViewer
{
    [XmlRoot("parameters")]
    public class Session : ILogEventProcessor, ILogEventColumns
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

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
        private FastSortedList<LogEvent> _filteredEvents;
        private bool _haveNewEvents = false;

        delegate void AddTreeNodeDelegate(TreeNode parentNode, TreeNode childNode);
        private AddTreeNodeDelegate _addTreeNodeDelegate;
        private long _totalEvents = 0;

        public Session()
        {
        }

        public LogEvent GetDisplayedItemForIndex(int pos)
        {
            lock (this)
            {
                return _filteredEvents[pos];
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
            page.Text = Name;
            TabPanel.ReloadColumns();
        }

        public void Start()
        {
            Receiver.Start();
        }

        public void Stop()
        {
            Receiver.Stop();
        }

        public string StatusText
        {
            get { return Receiver.StatusText; }
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
                logger.Info("Processing start {0} items.", logEventsToProcess.Count);
                int t0 = Environment.TickCount;
                foreach (LogEvent logEvent in logEventsToProcess)
                {
                    LogEvent removedEvent = _bufferedEvents.AddAndRemoveLast(logEvent);
                    if (removedEvent != null)
                        _filteredEvents.Remove(removedEvent);

                    if (TryFilters(logEvent))
                    {
                        _filteredEvents.Add(logEvent);
                    }

                    _totalEvents++;

#if A
                    // LogEventAttributeToNode(logEvent["Level"], _levelsTreeNode, _level2NodeCache, (char)0);
                    LogEventAttributeToNode((string)logEvent["Logger"], _loggersTreeNode, _logger2NodeCache, '.');
                    LogEventAttributeToNode((string)logEvent["SourceAssembly"], _assembliesTreeNode, _assembly2NodeCache, (char)0);
                    TreeNode node = LogEventAttributeToNode((string)logEvent["SourceType"], _classesTreeNode, _class2NodeCache, '.');
                    // LogEventAttributeToNode(logEvent.SourceMethod, node, 
                    LogEventAttributeToNode((string)logEvent["Thread"], _threadsTreeNode, _thread2NodeCache, (char)0);
                    LogEventAttributeToNode((string)logEvent["SourceApplication"], _applicationsTreeNode, _application2NodeCache, (char)0);
                    LogEventAttributeToNode((string)logEvent["SourceMachine"], _machinesTreeNode, _machine2NodeCache, (char)0);
                    LogEventAttributeToNode((string)logEvent["SourceFile"], _filesTreeNode, _file2NodeCache, (char)'\\');
#endif
                }
                int t1 = Environment.TickCount;
                int ips = -1;
                if (t1 > t0)
                {
                    ips = 1000 * logEventsToProcess.Count / (t1 - t0);
                }
                logger.Info("Processing finished {0} items. Total {1} ips: {2} time: {3}.", _filteredEvents.Count, logEventsToProcess.Count, ips, t1 - t0);
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
            private int _column;
            private bool _ascending;

            public ItemComparer(int column, bool ascending)
            {
                _column = column;
                _ascending = ascending;
            }

            public int Compare(LogEvent x, LogEvent y)
            {
                object v1 = x[_column];
                object v2 = y[_column];

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
            AppPreferences.RecentSessions.AddToList(fileName);
            return Save(fileName);
        }

        public bool Save(IWin32Window parent)
        {
            if (FileName == null)
                return SaveAs(parent);
            return CaptureParametersAndSaveConfig(FileName);
        }

        public bool SaveAs(IWin32Window parent)
        {
            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Filter = "NLogViewer Sessions (*.nlv)|*.nlv|All Files (*.*)|*.*";
                if (FileName != null)
                    sfd.FileName = FileName;
                if (sfd.ShowDialog(parent) == DialogResult.OK)
                {
                    Name = Path.GetFileNameWithoutExtension(sfd.FileName);
                    TabPage.Text = Name;
                    return CaptureParametersAndSaveConfig(sfd.FileName);
                }
            }
            return false;
        }

        public void NewSortOrder()
        {
            lock (this)
            {
                FastSortedList<LogEvent> newFilteredEvents = new FastSortedList<LogEvent>(new ItemComparer(GetOrAllocateOrdinal(OrderBy), SortAscending));

                if (_filteredEvents != null)
                {
                    foreach (LogEvent ev in _filteredEvents)
                    {
                        newFilteredEvents.Add(ev);
                    }
                }
                _filteredEvents = newFilteredEvents;
            }
        }

        public bool Close()
        {
            if (Receiver.CanStop())
                Stop();

            if (Dirty)
            {
                switch (MessageBox.Show(TabPanel,
                    "Session '" + Name + "' has unsaved changes. Save before exit?",
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
        private bool _dirty;

        [XmlIgnore]
        public string FileName;

        [XmlElement("name")]
        public string Name;

        [XmlArray("columns")]
        [XmlArrayItem("column")]
        public LogColumnCollection Columns = new LogColumnCollection();

        [XmlIgnore]
        public bool Dirty
        {
            get { return _dirty; }
            set { _dirty = value; }
        }

        [XmlElement("max-log-entries")]
        public int MaxLogEntries = 10000;

        [XmlElement("show-tree")]
        public bool ShowTree = true;

        [XmlElement("show-details")]
        public bool ShowDetails = true;

        [XmlElement("sort-by")]
        public string OrderBy = "ID";

        [XmlElement("sort-ascending")]
        public bool SortAscending = false;

        [XmlElement("receiver-type")]
        public string ReceiverType;

        [XmlElement("parser-type")]
        public string ParserType = "XML";

        [XmlArray("loggers")]
        [XmlArrayItem("logger", typeof(LoggerConfig))]
        public LoggerConfigCollection Loggers = new LoggerConfigCollection();

        private ILogEventReceiver _receiver;

        [XmlIgnore]
        public ILogEventReceiver Receiver
        {
            get { return _receiver; }
            set { _receiver = value; }
        }

        [XmlIgnore]
        public ILogEventParser Parser
        {
            get
            {
                ILogEventReceiverWithParser rp = Receiver as ILogEventReceiverWithParser;
                if (rp == null)
                    return null;
                return rp.Parser;
            }
            set
            {
                ILogEventReceiverWithParser rp = Receiver as ILogEventReceiverWithParser;
                if (rp != null)
                    rp.Parser = value;
            }
        }

        private StringToLoggerConfigMap _loggerName2LoggerConfig;

        public LoggerConfig GetLoggerConfig(string loggerName)
        {
            lock (this)
            {
                if (_loggerName2LoggerConfig == null)
                {
                    _loggerName2LoggerConfig = new StringToLoggerConfigMap();
                }
                return (LoggerConfig)_loggerName2LoggerConfig[loggerName];
            }
        }

        public void AddLoggerConfig(LoggerConfig lc)
        {
            lock (this)
            {
                _loggerName2LoggerConfig[lc.Name] = lc;
                Loggers.Add(lc);
            }
        }

        public void Resolve()
        {
            if (Columns.Count == 0)
            {
                Columns.Add(new LogColumn("ID", 120));
                Columns.Add(new LogColumn("Time", 120));
                Columns.Add(new LogColumn("Logger", 200));
                Columns.Add(new LogColumn("Level", 50));
                Columns.Add(new LogColumn("Text", 300));

                // invisible columns at the end

                Columns.Add(new LogColumn("Received Time", 120, false));
            }

            // initialize the ordinals
            for (int i = 0; i < Columns.Count; ++i)
                Columns[i].Ordinal = i;

            _bufferedEvents = new CyclicBuffer<LogEvent>(MaxLogEntries);
            NewSortOrder();
            _addTreeNodeDelegate = new AddTreeNodeDelegate(this.AddTreeNode);
            Receiver.Connect(this);
        }

        private static XmlSerializer _serializer = new XmlSerializer(typeof(Session));

        public bool Save(string fileName)
        {
            try
            {
                XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
                ns.Add("", "");
                using (FileStream fs = File.Create(fileName))
                {
                    XmlTextWriter xtw = new XmlTextWriter(fs, Encoding.UTF8);
                    xtw.Formatting = Formatting.Indented;
                    xtw.WriteStartDocument();
                    xtw.WriteStartElement("nlog-viewer");
                    _serializer.Serialize(xtw, this, ns);
                    XmlSerializer s1 = new XmlSerializer(Receiver.GetType());
                    s1.Serialize(xtw, Receiver, ns);
                    if (Receiver is ILogEventReceiverWithParser)
                    {
                        XmlSerializer s2 = new XmlSerializer(Parser.GetType());
                        s2.Serialize(xtw, Parser, ns);
                    }
                    xtw.WriteEndElement();
                    xtw.Flush();
                    FileName = fileName;
                    Dirty = false;
                }
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("ERROR: " + ex.ToString());
                return false;
            }
        }

        public static Session Load(string fileName)
        {
            //SoapFormatter formatter = new SoapFormatter();

            using (FileStream fs = File.OpenRead(fileName))
            {
                XmlTextReader xtr = new XmlTextReader(fs);
                xtr.ReadStartElement("nlog-viewer");

                Session c = (Session)_serializer.Deserialize(xtr);
                c.FileName = fileName;

                XmlSerializer s1 = new XmlSerializer(LogReceiverFactory.GetReceiverType(c.ReceiverType));
                c.Receiver = (ILogEventReceiver)s1.Deserialize(xtr);

                if (c.Receiver is ILogEventReceiverWithParser)
                {
                    XmlSerializer s2 = new XmlSerializer(LogEventParserFactory.GetParserType(c.ParserType));
                    c.Parser = (ILogEventParser)s2.Deserialize(xtr);
                }
                xtr.ReadEndElement();
                c.Resolve();
                return c;
            }
        }

        public bool ContainsColumn(string name)
        {
            foreach (LogColumn lc in Columns)
            {
                if (lc.Name == name)
                    return true;
            }
            return false;
        }

        public int GetOrAllocateOrdinal(string name)
        {
            for (int i = 0; i < Columns.Count; ++i)
            {
                if (Columns[i].Name == name)
                    return Columns[i].Ordinal;
            }

            LogColumn lc = new LogColumn();
            lc.Name = name;
            lc.Visible = false;
            lc.Width = 100;
            lc.Ordinal = Columns.Count;
            Columns.Add(lc);
            return lc.Ordinal;
        }

        int ILogEventColumns.Count
        {
            get { return Columns.Count; }
        }

        public LogEvent CreateLogEvent()
        {
            return new LogEvent(this);
        }
    }
}
