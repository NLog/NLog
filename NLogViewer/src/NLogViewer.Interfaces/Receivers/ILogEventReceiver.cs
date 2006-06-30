using System;
using System.Collections.Specialized;
using NLogViewer.Parsers;

namespace NLogViewer.Receivers
{
    public interface ILogEventReceiver
    {
        void Connect(ILogEventProcessor processor);
        void Disconnect();

        void Start();
        void Stop();
        void Pause();
        void Resume();
        void Refresh();
        bool CanStart();
        bool CanStop();
        bool CanPause();
        bool CanResume();
        bool CanRefresh();
        string StatusText { get; }
    }
}
