using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace SilverlightConsoleRunner
{
    public partial class RunnerForm : Form
    {
        public RunnerForm()
        {
            InitializeComponent();
        }

        public string Url { get; set; }

        public event EventHandler<TestCompletedEventArgs> OnCompleted;
        public event EventHandler<LogEventArgs> OnLogEvent;

        private void RunnerForm_Load(object sender, EventArgs e)
        {
            webBrowser1.ObjectForScripting = new ExternalLog(this);
            webBrowser1.Navigate(this.Url);
        }

        [ComVisible(true)]
        public class ExternalLog
        {
            private RunnerForm form;

            public ExternalLog(RunnerForm form)
            {
                this.form = form;
            }

            public void TestCompleted(string trxContent)
            {
                var onCompleted = this.form.OnCompleted;
                if (onCompleted != null)
                {
                    onCompleted(this, new TestCompletedEventArgs {TrxFileContents = trxContent});
                }

                this.form.Close();
            }

            public void ScenarioResult(long startDateTicks, long endDateTicks, string className, string methodName, string result, string exceptionMessage)
            {
                var onLogEvent = this.form.OnLogEvent;
                if (onLogEvent != null)
                {
                    onLogEvent(this.form, new LogEventArgs
                                              {
                                                  StartDate = new DateTime(startDateTicks),
                                                  EndDate = new DateTime(endDateTicks),
                                                  ClassName = className,
                                                  MethodName = methodName,
                                                  Result = result,
                                                  Exception = exceptionMessage,

                                              });
                }
            }
        }
    }
}
