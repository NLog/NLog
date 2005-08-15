using System;
using NLog.Viewer.UI;

namespace NLog.Viewer
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
