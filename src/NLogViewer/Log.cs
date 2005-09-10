using System;
using NLogViewer.UI;

namespace NLogViewer
{
	public class Log
	{
        private static MainForm _targetForm = null;

        public static void SetTargetForm(MainForm f)
        {
            _targetForm = f;
        }

        public static void Write(string s, params object[] p)
        {
            if (_targetForm != null)
            {
                _targetForm.LogWrite(s, p);
            }
        }
	}
}
