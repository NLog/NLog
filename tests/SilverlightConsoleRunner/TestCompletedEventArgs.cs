using System;

namespace SilverlightConsoleRunner
{
    public class TestCompletedEventArgs : EventArgs
    {
        public string TrxFileContents { get; set; }
    }
}