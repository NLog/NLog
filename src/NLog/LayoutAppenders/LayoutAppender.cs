// 
// Copyright (c) 2004 Jaroslaw Kowalski <jaak@polbox.com>
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
using System.Collections;
using System.Text;
using System.Globalization;

namespace NLog.LayoutAppenders
{
    public abstract class LayoutAppender
    {
        protected LayoutAppender() { }

        protected internal abstract int GetEstimatedBufferSize(LogEventInfo ev);
        protected internal virtual int NeedsStackTrace() { return 0; }
        protected internal abstract void Append(StringBuilder builder, LogEventInfo ev);

        private int _padding = 0;
        private bool _fixedLength = false;
        private int _absolutePadding = 0;
        private bool _upperCase = false;
        private bool _lowerCase = false;
        private char _padCharacter = ' ';
        private CultureInfo _cultureInfo = CultureInfo.InvariantCulture;

        public int Padding
        {
            get { return _padding; }
            set { _padding = value; _absolutePadding = Math.Abs(_padding); }
        }

        public int AbsolutePadding
        {
            get { return _absolutePadding; }
        }

        public char PadCharacter
        {
            get { return _padCharacter; }
            set { _padCharacter = value; }
        }

        public bool FixedLength
        {
            get { return _fixedLength; }
            set { _fixedLength = value; }
        }

        public bool UpperCase
        {
            get { return _upperCase; }
            set { _upperCase = value; }
        }

        public bool LowerCase
        {
            get { return _lowerCase; }
            set { _lowerCase = value; }
        }
        
        public string Culture
        {
            get { return _cultureInfo.Name; }
            set { _cultureInfo = new CultureInfo(value); }
        }

        public CultureInfo CultureInfo
        {
            get { return _cultureInfo; }
            set { _cultureInfo = value; }
        }

        protected string ApplyPadding(string s) 
        {
            if (Padding != 0) 
            {
                if (Padding > 0) 
                {
                    s = s.PadLeft(Padding, PadCharacter);
                } 
                else 
                {
                    s = s.PadRight(-Padding, PadCharacter);
                }
                if (FixedLength && s.Length > AbsolutePadding)
                {
                    s = s.Substring(0, AbsolutePadding);
                }
            }
            if (UpperCase) 
            {
                s = s.ToUpper(CultureInfo);
            } 
            else if (LowerCase) 
            {
                s = s.ToLower(CultureInfo);
            }
            return s;
        }
    }
}
