using System;
using System.Text;
using System.Windows.Forms;
using NLog;
using NLog.Windows.Forms;

namespace RichTextBox2
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

            ToolStripItemTarget target = new ToolStripItemTarget();
            target.Layout = "${date:format=HH\\:MM\\:ss} ${logger} ${message}";
            target.ToolStripName = "statusStrip1";
            target.ItemName = "toolStripStatusLabel1";
            target.FormName = "Form1";

            NLog.Config.SimpleConfigurator.ConfigureForTargetLogging(target, LogLevel.Trace);

            Logger logger = LogManager.GetLogger("Example");
            logger.Trace("trace log message, ");
            logger.Debug("debug log message, ");
            logger.Info("info log message, ");
            logger.Warn("warn log message, ");
            logger.Error("error log message, ");
            logger.Fatal("fatal log message");
        }
    }
}