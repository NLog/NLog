using System;

namespace NLogViewer
{
    public delegate void InternalLogWriteDelegate(string s, params object[] p);

    public class NLogViewerTrace
	{
        static InternalLogWriteDelegate _writeDelegate;

        public static InternalLogWriteDelegate WriteDelegate
        {
            get { return _writeDelegate; }
            set { _writeDelegate = value; }
        }

        public static void Write(string s, params object[] p)
        {
            if (_writeDelegate != null)
            {
                _writeDelegate(s, p);
            }
        }
	}
}
