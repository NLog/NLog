using System;
using System.Windows.Forms;

using NLogViewer.UI;
using System.Drawing;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;

namespace NLogViewer
{
	public partial class GlobalImageList : UserControl
	{
        public static readonly GlobalImageList Instance = new GlobalImageList();

        private ImageList imageList1;
        private IContainer components;
        private Dictionary<string, int> _name2index = new Dictionary<string, int>();

        [Browsable(true)]
        public ImageList ImageList
        {
            get { return imageList1; }
        }

        static GlobalImageList()
        {
        }

        public GlobalImageList()
        {
            InitializeComponent();

            foreach (string icoFile in Directory.GetFiles(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Levels"), "*.ico"))
            {
                string baseName = Path.GetFileNameWithoutExtension(icoFile);
                _name2index[baseName.ToUpper()] = ImageList.Images.Count;
                ImageList.Images.Add(new Icon(icoFile, ImageList.ImageSize));
            }
            foreach (string pngFile in Directory.GetFiles(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Levels"), "*.png"))
            {
                string baseName = Path.GetFileNameWithoutExtension(pngFile);
                _name2index[baseName.ToUpper()] = ImageList.Images.Count;
                ImageList.Images.Add(Image.FromFile(pngFile, true));
            }
        }

        public int GetImageForLevel(string level)
        {
            if (!_name2index.ContainsKey(level))
                return -1;

            return _name2index[level];
        }

        public Icon GetIconForLevel(string level)
        {
            return null;
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(GlobalImageList));
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.SuspendLayout();
            // 
            // imageList1
            // 
            this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList1.Images.SetKeyName(0, "ArrowColUp.ico");
            this.imageList1.Images.SetKeyName(1, "ArrowColDown.ico");
            // 
            // GlobalImageList
            // 
            this.Name = "GlobalImageList";
            this.Size = new System.Drawing.Size(215, 157);
            this.ResumeLayout(false);

        }
	}
}
