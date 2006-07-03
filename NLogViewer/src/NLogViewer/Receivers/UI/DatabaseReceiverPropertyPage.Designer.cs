namespace NLogViewer.Receivers.UI
{
    partial class DatabaseReceiverPropertyPage
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.textBoxConnectionType = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.textBoxConnectionString = new System.Windows.Forms.TextBox();
            this.buttonBuildConnectionString = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.textBoxQueryString = new System.Windows.Forms.TextBox();
            this.buttonTest = new System.Windows.Forms.Button();
            this.comboBox2 = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // comboBox1
            // 
            this.comboBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBox1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Location = new System.Drawing.Point(7, 25);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(380, 21);
            this.comboBox1.TabIndex = 0;
            this.comboBox1.SelectedIndexChanged += new System.EventHandler(this.comboBox1_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(4, 4);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(91, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Connection Type:";
            // 
            // textBoxConnectionType
            // 
            this.textBoxConnectionType.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxConnectionType.Location = new System.Drawing.Point(7, 52);
            this.textBoxConnectionType.Name = "textBoxConnectionType";
            this.textBoxConnectionType.Size = new System.Drawing.Size(380, 20);
            this.textBoxConnectionType.TabIndex = 2;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(4, 75);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(94, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Connection String:";
            // 
            // textBoxConnectionString
            // 
            this.textBoxConnectionString.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxConnectionString.Location = new System.Drawing.Point(7, 95);
            this.textBoxConnectionString.Name = "textBoxConnectionString";
            this.textBoxConnectionString.Size = new System.Drawing.Size(299, 20);
            this.textBoxConnectionString.TabIndex = 2;
            // 
            // buttonBuildConnectionString
            // 
            this.buttonBuildConnectionString.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonBuildConnectionString.Location = new System.Drawing.Point(312, 93);
            this.buttonBuildConnectionString.Name = "buttonBuildConnectionString";
            this.buttonBuildConnectionString.Size = new System.Drawing.Size(75, 23);
            this.buttonBuildConnectionString.TabIndex = 4;
            this.buttonBuildConnectionString.Text = "Build...";
            this.buttonBuildConnectionString.UseVisualStyleBackColor = true;
            this.buttonBuildConnectionString.Click += new System.EventHandler(this.buttonBuildConnectionString_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 158);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(65, 13);
            this.label3.TabIndex = 5;
            this.label3.Text = "Query String";
            // 
            // textBoxQueryString
            // 
            this.textBoxQueryString.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxQueryString.Location = new System.Drawing.Point(7, 174);
            this.textBoxQueryString.Multiline = true;
            this.textBoxQueryString.Name = "textBoxQueryString";
            this.textBoxQueryString.Size = new System.Drawing.Size(380, 154);
            this.textBoxQueryString.TabIndex = 6;
            // 
            // buttonTest
            // 
            this.buttonTest.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonTest.Location = new System.Drawing.Point(277, 334);
            this.buttonTest.Name = "buttonTest";
            this.buttonTest.Size = new System.Drawing.Size(109, 23);
            this.buttonTest.TabIndex = 7;
            this.buttonTest.Text = "&Test Connection";
            this.buttonTest.UseVisualStyleBackColor = true;
            this.buttonTest.Click += new System.EventHandler(this.buttonTest_Click);
            // 
            // comboBox2
            // 
            this.comboBox2.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox2.FormattingEnabled = true;
            this.comboBox2.Location = new System.Drawing.Point(7, 134);
            this.comboBox2.Name = "comboBox2";
            this.comboBox2.Size = new System.Drawing.Size(377, 21);
            this.comboBox2.TabIndex = 8;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(6, 118);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(134, 13);
            this.label4.TabIndex = 5;
            this.label4.Text = "Transaction Isolation Level";
            // 
            // DatabaseReceiverPropertyPage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.comboBox2);
            this.Controls.Add(this.buttonTest);
            this.Controls.Add(this.textBoxQueryString);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.buttonBuildConnectionString);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.textBoxConnectionString);
            this.Controls.Add(this.textBoxConnectionType);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.comboBox1);
            this.Label1 = "Configure Your Log Receiver";
            this.Label2 = "Set options that control how your receiver works.";
            this.Name = "DatabaseReceiverPropertyPage";
            this.NextButtonIsDefault = false;
            this.Size = new System.Drawing.Size(400, 360);
            this.Load += new System.EventHandler(this.DatabaseReceiverPropertyPage_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox comboBox1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBoxConnectionType;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBoxConnectionString;
        private System.Windows.Forms.Button buttonBuildConnectionString;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBoxQueryString;
        private System.Windows.Forms.Button buttonTest;
        private System.Windows.Forms.ComboBox comboBox2;
        private System.Windows.Forms.Label label4;

    }
}
