// 
// Copyright (c) 2004,2005 Jaroslaw Kowalski <jkowalski@users.sourceforge.net>
// 
// 
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without 
// modification, are permitted provided that the following conditions 
// are met:
// 
// * Redistributions of source code must retain the above copyright notice, 
//   this list of conditions and the following disclaimer. 
// 
// * Redistributions in binary form must reproduce the above copyright notice,
//   this list of conditions and the following disclaimer in the documentation
//   and/or other materials provided with the distribution. 
// 
// * Neither the name of the Jaroslaw Kowalski nor the names of its 
//   contributors may be used to endorse or promote products derived from this
//   software without specific prior written permission. 
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE 
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE 
// ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE 
// LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR 
// CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
// SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS 
// INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN 
// CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) 
// ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF 
// THE POSSIBILITY OF SUCH DAMAGE.
// 

using System;
using System.Xml;
using System.Xml.Serialization;
using System.ComponentModel;

namespace NLogViewer.Configuration
{
    [Serializable]
    public class LogColumn
	{
        public LogColumn()
        {
        }

        public LogColumn(string name, int width)
        {
            this.Name = name;
            this.Width = width;
        }

        public LogColumn(string name, int width, bool visible)
        {
            this.Name = name;
            this.Width = width;
            this.Visible = visible;
        }

        private string _name;

        [XmlAttribute("name")]
        public string Name
        {
            get { return _name; }
            set
            {
                if (value == null || value.Length == 0)
                    throw new ArgumentException("Name cannot be empty.");
                _name = value;
            }
        }

        private int _width;

        [XmlAttribute("width")]
        public int Width
        {
            get { return _width; }
            set
            {
                if (value <= 0)
                    throw new ArgumentException("Width must be a positive integer.");
                _width = value;
            }
        }


        private LogColumnGrouping _grouping = LogColumnGrouping.None;

        [XmlAttribute("grouping")]
        [DefaultValue(LogColumnGrouping.None)]
        public LogColumnGrouping Grouping
        {
            get { return _grouping; }
            set { _grouping = value; }
        }

        private bool _visible = true;

        [XmlAttribute("visible")]
        [System.ComponentModel.DefaultValue(true)]

        public bool Visible
        {
            get { return _visible; }
            set { _visible = value; }
        }
	
        public override string ToString()
        {
            return Name;
        }

        [XmlIgnore]
        public int Ordinal;

        public LogColumn Clone()
        {
            return (LogColumn)MemberwiseClone();
        }
	}
}
