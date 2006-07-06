using System;
using System.Windows.Forms;

using NLogViewer.UI;
using Microsoft.Win32;

namespace NLogViewer
{
	public class EntryPoint
	{
        static void Register()
        {
            using (RegistryKey reg = Registry.CurrentUser.CreateSubKey("Software\\Classes\\.nlv"))
            {
                reg.SetValue(null, "NLogViewerSessionFile");
            }
            using (RegistryKey regNLogViewerSessionFile = Registry.CurrentUser.CreateSubKey("Software\\Classes\\NLogViewerSessionFile\\shell\\Open\\Command"))
            {
                string path = String.Format("\"{0}\" \"%1\"", typeof(EntryPoint).Assembly.Location);
                regNLogViewerSessionFile.SetValue(null, path);
            }
        }

		[STAThread]
		static void Main(string[] args) 
		{
            Register();
            Application.EnableVisualStyles();
			Application.Run(new MainForm(args));
		}
	}
}
