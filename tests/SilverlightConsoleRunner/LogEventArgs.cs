using System;

namespace SilverlightConsoleRunner
{
    public class LogEventArgs : EventArgs
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Exception { get; set; }
        public string ClassName { get; set; }
        public string MethodName { get; set;  }
        public string Result { get; set; }
    }
}