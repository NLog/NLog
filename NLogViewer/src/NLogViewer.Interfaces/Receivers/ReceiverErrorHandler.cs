using System;
using System.Collections.Specialized;
using NLogViewer.Parsers;

namespace NLogViewer.Receivers
{
    public delegate void ReceiverErrorHandler(object sender, ReceiverErrorEventArgs args);

    public class ReceiverErrorEventArgs : EventArgs
    {
        public ReceiverErrorEventArgs(Exception ex)
        {
            _exception = ex;
        }

        private Exception _exception;

        public Exception Exception
        {
            get { return _exception; }
            set { _exception = value; }
        }
    }
}
