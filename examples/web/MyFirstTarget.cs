using NLog;

namespace MyNamespace
{
    [Target("MyFirst")]
    public sealed class MyFirstTarget: TargetWithLayout
    {
        private string _host = "localhost";

        public string Host
        {
            get { return _host; }
            set { _host = value; }
        }

        protected override void Write(LogEventInfo logEvent)
        {
            string logMessage = CompiledLayout.GetFormattedMessage(logEvent);

            SendTheMessageToRemoteHost(this.Host, logMessage);
        }

        private void SendTheMessageToRemoteHost(string host, string message)
        {
            // TODO - write me
        }
    }
}
