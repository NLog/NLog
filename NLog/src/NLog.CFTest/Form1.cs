using System;
using System.Drawing;
using System.Collections;
using System.Windows.Forms;
using System.Data;

using NLog;

namespace NLog.CFTest
{
	/// <summary>
	/// Summary description for Form1.
	/// </summary>
	public class Form1 : System.Windows.Forms.Form
	{
        private static Logger logger = LogManager.GetLogger("LoggerName");

		private System.Windows.Forms.MainMenu mainMenu1;

		public Form1()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//
		}
		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			base.Dispose( disposing );
		}
		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            this.mainMenu1 = new System.Windows.Forms.MainMenu();
            // 
            // Form1
            // 
            this.Menu = this.mainMenu1;
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);

        }
		#endregion

		/// <summary>
		/// The main entry point for the application.
		/// </summary>

		static void Main() 
		{
			Application.Run(new Form1());
		}

        private void Form1_Load(object sender, System.EventArgs e)
        {
            MessageBox.Show("loaded: " + System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);
            Close();
        }
	}
}
