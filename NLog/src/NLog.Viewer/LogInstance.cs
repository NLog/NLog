using System;
using System.Threading;
using System.Windows.Forms;
using System.Text;
using System.IO;
using System.Collections;
using System.Drawing;

namespace NLog.Viewer
{
	public abstract class LogInstance
	{
        private LogInstanceConfigurationInfo _logInstanceConfigurationInfo;
        private Thread _inputThread = null;
        private bool _quitThread;
        private ListView _listView;
        private TreeView _treeView;

		public LogInstance(LogInstanceConfigurationInfo logInstanceConfigurationInfo)
		{
            _logInstanceConfigurationInfo = logInstanceConfigurationInfo;
		}

        public LogInstanceConfigurationInfo LogInstanceConfigurationInfo
        {
            get { return _logInstanceConfigurationInfo; }
        }

        public TabPage CreateTab(MainForm form)
        {
            TabPage page = new TabPage(LogInstanceConfigurationInfo.Name);
            page.ImageIndex = 1;

            TreeView treeView = new TreeView();
            treeView.Nodes.Add(new TreeNode("Loggers"));
            treeView.Dock = DockStyle.Left;
            treeView.ContextMenu = form.treeContextMenu;

            Splitter splitter = new Splitter();
            splitter.Dock = DockStyle.Left;

            ListView listView = new ListView();
            listView.Dock = DockStyle.Fill;
            listView.View = View.Details;
            listView.FullRowSelect = true;
            listView.GridLines = true;
            listView.Font = new Font("Tahoma", 8);

            listView.Columns.Add("Time", 120, HorizontalAlignment.Left);
            listView.Columns.Add("Logger", 200, HorizontalAlignment.Left);
            listView.Columns.Add("Level", 50, HorizontalAlignment.Left);
            listView.Columns.Add("Text", 300, HorizontalAlignment.Left);

            _listView = listView;
            _treeView = treeView;

            page.Controls.Add(listView);
            page.Controls.Add(splitter);
            page.Controls.Add(treeView);

            return page;
        }

        public void Start()
        {
            _quitThread = false;
            _inputThread = new Thread(new ThreadStart(InputThread));
            _inputThread.IsBackground = true;
            _inputThread.Start();
        }

        public void Stop()
        {
            if (_inputThread != null)
            {
                _quitThread = true;
                if (!_inputThread.Join(2000))
                {
                    _inputThread.Abort();
                }
            }
        }

        public bool IsRunning
        {
            get { return _inputThread.IsAlive; }
        }

        public abstract void InputThread();

        protected bool QuitInputThread
        {
            get { return _quitThread; }
        }

        private Hashtable _logger2node = new Hashtable();

        private void LoggerToNode(string logger, out TreeNode treeNode, out LoggerConfigInfo loggerConfigInfo)
        {
            object o = _logger2node[logger];
            if (o != null)
            {
                treeNode = (TreeNode)o;
                loggerConfigInfo = (LoggerConfigInfo)treeNode.Tag;
                return;
            }

            TreeNode parentNode;
            LoggerConfigInfo parentLoggerConfigInfo;

            string baseName;
            int rightmostDot = logger.LastIndexOf('.');
            if (rightmostDot < 0)
            {
                parentNode = _treeView.Nodes[0];
                baseName = logger;
                parentLoggerConfigInfo = LogInstanceConfigurationInfo.GetLoggerConfigInfo("");
            }
            else
            {
                string parentLoggerName = logger.Substring(0, rightmostDot);
                baseName = logger.Substring(rightmostDot + 1);
                LoggerToNode(parentLoggerName, out parentNode, out parentLoggerConfigInfo);
            }

            LoggerConfigInfo lci = LogInstanceConfigurationInfo.GetLoggerConfigInfo(logger);

            TreeNode newNode = new TreeNode(baseName);
            _logger2node[logger] = newNode;
            newNode.Tag = lci;
            _treeView.Invoke(new AddTreeNodeDelegate(this.AddTreeNode), new object[] { parentNode, newNode });

            treeNode = newNode;
            loggerConfigInfo = lci;
        }

        delegate void AddTreeNodeDelegate(TreeNode parentNode, TreeNode childNode);

        private void AddTreeNode(TreeNode parentNode, TreeNode childNode)
        {
            parentNode.Nodes.Add(childNode);
            parentNode.Expand();
        }

        protected void ProcessLogEvent(LogEventInfo eventInfo)
        {
            ListViewItem item =_listView.Items.Insert(0, eventInfo.ReceivedTime.ToString());
            item.SubItems.Add(eventInfo.Logger);
            item.SubItems.Add(eventInfo.Level);
            item.SubItems.Add(eventInfo.MessageText);

            switch (eventInfo.Level[0])
            {
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

            LoggerConfigInfo loggerConfigInfo;
            TreeNode treeNode;

            LoggerToNode(eventInfo.Logger, out treeNode, out loggerConfigInfo);
        }
    }
}
