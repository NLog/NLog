namespace NLogViewer.UI
{
    partial class WizardForm
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.wizardButtonNext = new System.Windows.Forms.Button();
            this.wizardButtonFinish = new System.Windows.Forms.Button();
            this.wizardButtonBack = new System.Windows.Forms.Button();
            this.wizardButtonCancel = new System.Windows.Forms.Button();
            this.wizardContentPanel = new System.Windows.Forms.Panel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.panel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // wizardButtonNext
            // 
            this.wizardButtonNext.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.wizardButtonNext.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.wizardButtonNext.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.wizardButtonNext.Location = new System.Drawing.Point(404, 311);
            this.wizardButtonNext.Name = "wizardButtonNext";
            this.wizardButtonNext.Size = new System.Drawing.Size(75, 23);
            this.wizardButtonNext.TabIndex = 5;
            this.wizardButtonNext.Text = "&Next";
            this.wizardButtonNext.Click += new System.EventHandler(this.wizardButtonNext_Click);
            // 
            // wizardButtonFinish
            // 
            this.wizardButtonFinish.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.wizardButtonFinish.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.wizardButtonFinish.Location = new System.Drawing.Point(485, 311);
            this.wizardButtonFinish.Name = "wizardButtonFinish";
            this.wizardButtonFinish.Size = new System.Drawing.Size(75, 23);
            this.wizardButtonFinish.TabIndex = 6;
            this.wizardButtonFinish.Text = "&Finish";
            this.wizardButtonFinish.Click += new System.EventHandler(this.wizardButtonFinish_Click);
            // 
            // wizardButtonBack
            // 
            this.wizardButtonBack.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.wizardButtonBack.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.wizardButtonBack.Location = new System.Drawing.Point(323, 311);
            this.wizardButtonBack.Name = "wizardButtonBack";
            this.wizardButtonBack.Size = new System.Drawing.Size(75, 23);
            this.wizardButtonBack.TabIndex = 7;
            this.wizardButtonBack.Text = "&Back";
            this.wizardButtonBack.Click += new System.EventHandler(this.wizardButtonBack_Click);
            // 
            // wizardButtonCancel
            // 
            this.wizardButtonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.wizardButtonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.wizardButtonCancel.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.wizardButtonCancel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.wizardButtonCancel.Location = new System.Drawing.Point(242, 311);
            this.wizardButtonCancel.Name = "wizardButtonCancel";
            this.wizardButtonCancel.Size = new System.Drawing.Size(75, 23);
            this.wizardButtonCancel.TabIndex = 8;
            this.wizardButtonCancel.Text = "Cancel";
            this.wizardButtonCancel.Click += new System.EventHandler(this.wizardButtonCancel_Click);
            // 
            // wizardContentPanel
            // 
            this.wizardContentPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.wizardContentPanel.Location = new System.Drawing.Point(12, 71);
            this.wizardContentPanel.Name = "wizardContentPanel";
            this.wizardContentPanel.Size = new System.Drawing.Size(548, 231);
            this.wizardContentPanel.TabIndex = 9;
            // 
            // panel2
            // 
            this.panel2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.panel2.BackColor = System.Drawing.Color.White;
            this.panel2.Controls.Add(this.label2);
            this.panel2.Controls.Add(this.label1);
            this.panel2.Location = new System.Drawing.Point(0, 0);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(572, 65);
            this.panel2.TabIndex = 10;
            this.panel2.Paint += new System.Windows.Forms.PaintEventHandler(this.panel2_Paint);
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(27, 28);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(533, 31);
            this.label2.TabIndex = 1;
            this.label2.Text = "label2aaa alfdsjhfas aksjdfha lskfjhas dflkajshdf lasjkfdh alskjfdha skljdfha skl" +
                "jfdha sfdkljhasfda lkasdjkfahsfdla sdkjfha sf";
            // 
            // label1
            // 
            this.label1.AutoEllipsis = true;
            this.label1.BackColor = System.Drawing.Color.Transparent;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.label1.Location = new System.Drawing.Point(9, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(551, 19);
            this.label1.TabIndex = 0;
            this.label1.Text = "label1";
            // 
            // WizardForm
            // 
            this.AcceptButton = this.wizardButtonNext;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.wizardButtonCancel;
            this.ClientSize = new System.Drawing.Size(572, 346);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.wizardContentPanel);
            this.Controls.Add(this.wizardButtonNext);
            this.Controls.Add(this.wizardButtonFinish);
            this.Controls.Add(this.wizardButtonBack);
            this.Controls.Add(this.wizardButtonCancel);
            this.Name = "WizardForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "WizardForm";
            this.panel2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        protected System.Windows.Forms.Button wizardButtonNext;
        protected System.Windows.Forms.Button wizardButtonFinish;
        protected System.Windows.Forms.Button wizardButtonBack;
        protected System.Windows.Forms.Button wizardButtonCancel;
        private System.Windows.Forms.Panel wizardContentPanel;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
    }
}