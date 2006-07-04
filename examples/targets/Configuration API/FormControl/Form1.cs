using System;
using System.Text;
using System.Windows.Forms;
using NLog;
using NLog.Targets;

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

            FormControlTarget target = new FormControlTarget();
            target.Layout = "${date:format=HH\\:MM\\:ss} ${logger} ${message}";
            target.ControlName = "textBox1";
            target.FormName = "Form1";
            target.Append = true;

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