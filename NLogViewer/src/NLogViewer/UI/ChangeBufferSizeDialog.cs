using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace NLogViewer.UI
{
    public partial class ChangeBufferSizeDialog : Form
    {
        public ChangeBufferSizeDialog()
        {
            InitializeComponent();
        }

        private int _bufferSize;

        public int BufferSize
        {
            get { return _bufferSize; }
            set
            {
                _bufferSize = value;
                comboBox1.Text = value.ToString();
            }
        }
    }
}