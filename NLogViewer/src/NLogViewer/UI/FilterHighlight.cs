using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace NLogViewer.UI
{
	/// <summary>
	/// Summary description for FilterHighlight.
	/// </summary>
	public class FilterHighlight : System.Windows.Forms.Form
	{
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPageInclude;
        private System.Windows.Forms.TabPage tabPageExclude;
        private System.Windows.Forms.TabPage tabPageHighlight;
        private System.Windows.Forms.ListView listView1;
        private System.Windows.Forms.Button buttonAddInclude;
        private System.Windows.Forms.Button buttonRemoveInclude;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.ListView listView2;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Button button4;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public FilterHighlight()
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
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPageInclude = new System.Windows.Forms.TabPage();
            this.label1 = new System.Windows.Forms.Label();
            this.buttonAddInclude = new System.Windows.Forms.Button();
            this.listView1 = new System.Windows.Forms.ListView();
            this.buttonRemoveInclude = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.tabPageExclude = new System.Windows.Forms.TabPage();
            this.label2 = new System.Windows.Forms.Label();
            this.button2 = new System.Windows.Forms.Button();
            this.listView2 = new System.Windows.Forms.ListView();
            this.button3 = new System.Windows.Forms.Button();
            this.button4 = new System.Windows.Forms.Button();
            this.tabPageHighlight = new System.Windows.Forms.TabPage();
            this.tabControl1.SuspendLayout();
            this.tabPageInclude.SuspendLayout();
            this.tabPageExclude.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
                | System.Windows.Forms.AnchorStyles.Left) 
                | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl1.Controls.Add(this.tabPageInclude);
            this.tabControl1.Controls.Add(this.tabPageExclude);
            this.tabControl1.Controls.Add(this.tabPageHighlight);
            this.tabControl1.Location = new System.Drawing.Point(8, 8);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(480, 322);
            this.tabControl1.TabIndex = 0;
            // 
            // tabPageInclude
            // 
            this.tabPageInclude.Controls.Add(this.label1);
            this.tabPageInclude.Controls.Add(this.buttonAddInclude);
            this.tabPageInclude.Controls.Add(this.listView1);
            this.tabPageInclude.Controls.Add(this.buttonRemoveInclude);
            this.tabPageInclude.Controls.Add(this.button1);
            this.tabPageInclude.Location = new System.Drawing.Point(4, 22);
            this.tabPageInclude.Name = "tabPageInclude";
            this.tabPageInclude.Size = new System.Drawing.Size(472, 296);
            this.tabPageInclude.TabIndex = 0;
            this.tabPageInclude.Text = "Include Condtitions";
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(8, 8);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(456, 48);
            this.label1.TabIndex = 2;
            this.label1.Text = "The following conditions are checked to see whether a message should be displayed" +
                ". If ANY of the Include conditions are met and NONE of the Exclude conditions ar" +
                "e met, the message is accepted.";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // buttonAddInclude
            // 
            this.buttonAddInclude.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonAddInclude.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonAddInclude.Location = new System.Drawing.Point(8, 266);
            this.buttonAddInclude.Name = "buttonAddInclude";
            this.buttonAddInclude.Size = new System.Drawing.Size(128, 24);
            this.buttonAddInclude.TabIndex = 1;
            this.buttonAddInclude.Text = "&Add Condition...";
            // 
            // listView1
            // 
            this.listView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
                | System.Windows.Forms.AnchorStyles.Left) 
                | System.Windows.Forms.AnchorStyles.Right)));
            this.listView1.Location = new System.Drawing.Point(8, 56);
            this.listView1.Name = "listView1";
            this.listView1.Size = new System.Drawing.Size(456, 202);
            this.listView1.TabIndex = 0;
            // 
            // buttonRemoveInclude
            // 
            this.buttonRemoveInclude.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonRemoveInclude.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonRemoveInclude.Location = new System.Drawing.Point(144, 266);
            this.buttonRemoveInclude.Name = "buttonRemoveInclude";
            this.buttonRemoveInclude.Size = new System.Drawing.Size(128, 24);
            this.buttonRemoveInclude.TabIndex = 1;
            this.buttonRemoveInclude.Text = "&Remove Condition...";
            // 
            // button1
            // 
            this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button1.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.button1.Location = new System.Drawing.Point(336, 266);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(128, 24);
            this.button1.TabIndex = 1;
            this.button1.Text = "Reset To &Defaults";
            // 
            // tabPageExclude
            // 
            this.tabPageExclude.Controls.Add(this.label2);
            this.tabPageExclude.Controls.Add(this.button2);
            this.tabPageExclude.Controls.Add(this.listView2);
            this.tabPageExclude.Controls.Add(this.button3);
            this.tabPageExclude.Controls.Add(this.button4);
            this.tabPageExclude.Location = new System.Drawing.Point(4, 22);
            this.tabPageExclude.Name = "tabPageExclude";
            this.tabPageExclude.Size = new System.Drawing.Size(472, 296);
            this.tabPageExclude.TabIndex = 1;
            this.tabPageExclude.Text = "Exclude Conditions";
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(8, 8);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(456, 48);
            this.label2.TabIndex = 7;
            this.label2.Text = "The following conditions are checked to see whether a message should be displayed" +
                ". If ANY of the Include conditions are met and NONE of the Exclude conditions ar" +
                "e met, the message is accepted.";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // button2
            // 
            this.button2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.button2.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.button2.Location = new System.Drawing.Point(8, 266);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(128, 24);
            this.button2.TabIndex = 6;
            this.button2.Text = "Add Condition...";
            // 
            // listView2
            // 
            this.listView2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
                | System.Windows.Forms.AnchorStyles.Left) 
                | System.Windows.Forms.AnchorStyles.Right)));
            this.listView2.Location = new System.Drawing.Point(8, 56);
            this.listView2.Name = "listView2";
            this.listView2.Size = new System.Drawing.Size(456, 202);
            this.listView2.TabIndex = 3;
            // 
            // button3
            // 
            this.button3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.button3.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.button3.Location = new System.Drawing.Point(144, 266);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(128, 24);
            this.button3.TabIndex = 4;
            this.button3.Text = "Remove Condition...";
            // 
            // button4
            // 
            this.button4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button4.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.button4.Location = new System.Drawing.Point(336, 266);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(128, 24);
            this.button4.TabIndex = 5;
            this.button4.Text = "Reset To Defaults";
            // 
            // tabPageHighlight
            // 
            this.tabPageHighlight.Location = new System.Drawing.Point(4, 22);
            this.tabPageHighlight.Name = "tabPageHighlight";
            this.tabPageHighlight.Size = new System.Drawing.Size(472, 296);
            this.tabPageHighlight.TabIndex = 2;
            this.tabPageHighlight.Text = "Highlight";
            // 
            // FilterHighlight
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(496, 336);
            this.Controls.Add(this.tabControl1);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(500, 370);
            this.Name = "FilterHighlight";
            this.Text = "Filter / Highlight";
            this.tabControl1.ResumeLayout(false);
            this.tabPageInclude.ResumeLayout(false);
            this.tabPageExclude.ResumeLayout(false);
            this.ResumeLayout(false);

        }
		#endregion
	}
}
