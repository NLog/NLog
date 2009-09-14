using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace NLog.Config
{
    public sealed class NullInternalLog : IInternalLog
    {
        public static readonly IInternalLog Instance = new NullInternalLog();

        public void Trace(string message, params object[] parameters)
        {
        }

        public void Debug(string message, params object[] parameters)
        {
        }

        public void Info(string message, params object[] parameters)
        {
        }

        public void Warn(string message, params object[] parameters)
        {
        }

        public void Error(string message, params object[] parameters)
        {
        }

        public void Fatal(string message, params object[] parameters)
        {
        }

        #region IInternalLog Members

        public bool IsTraceEnabled
        {
            get { return false; }
        }

        public bool IsDebugEnabled
        {
            get { return false; }
        }

        public bool IsInfoEnabled
        {
            get { return false; }
        }

        public bool IsWarnEnabled
        {
            get { return false; }
        }

        public bool IsErrorEnabled
        {
            get { return false; }
        }

        public bool IsFatalEnabled
        {
            get { return false; }
        }

        #endregion
    }
}
