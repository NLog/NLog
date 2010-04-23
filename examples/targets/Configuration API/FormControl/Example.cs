using System;
using System.Windows.Forms;
using NLog;
using NLog.Targets;

namespace RichTextBox2
{
    static class Example
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
            // for NLog configuration look in Form1.cs
        }
    }
}