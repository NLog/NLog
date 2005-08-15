using System;
using System.Windows.Forms;

using NLog.Viewer.UI;

namespace NLog.Viewer
{
	public class EntryPoint
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main() 
		{
			Application.Run(new MainForm());
		}
	}
}
