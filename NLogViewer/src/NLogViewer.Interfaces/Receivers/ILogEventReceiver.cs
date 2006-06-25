using System;
using System.Collections.Specialized;
using NLogViewer.Parsers;

namespace NLogViewer.Receivers
{
    public interface ILogEventReceiver
    {
        void Configure(NameValueCollection parameters);
        void Connect(ILogEventProcessor processor);
        void Disconnect();

        void Start();
        void Stop();
        bool IsRunning { get; }
        string StatusText { get; }
    }
}
