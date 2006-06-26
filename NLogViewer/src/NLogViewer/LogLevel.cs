using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace NLogViewer
{
    public class LogLevel : IComparable<LogLevel>, IComparable
    {
        private int _ordinal;
        private string _name;
        private int _imageIndex;
        private Color _color;
        private Color _backColor;

        public LogLevel(int ordinal, string name, int imageIndex, Color color, Color backColor)
        {
            _ordinal = ordinal;
            _name = name;
            _imageIndex = imageIndex;
            _color = color;
            _backColor = backColor;
        }

        public override string ToString()
        {
            return _name;
        }

        public string Name
        {
            get { return _name; }
        }

        public int Ordinal
        {
            get { return _ordinal; }
        }

        public int ImageIndex
        {
            get { return _imageIndex; }
        }

        public Color Color
        {
            get { return _color; }
        }

        public Color BackColor
        {
            get { return _backColor; }
        }

        int IComparable.CompareTo(object obj)
        {
            return CompareTo((LogLevel)obj);
        }

        public int CompareTo(LogLevel other)
        {
            return Ordinal - other.Ordinal;
        }

        public override bool Equals(object obj)
        {
            LogLevel other = obj as LogLevel;
            if (other == null)
                return false;

            return Ordinal == other.Ordinal;
        }

        public override int GetHashCode()
        {
            return Ordinal;
        }
    }
}
