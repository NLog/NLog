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
using System.Text;

namespace NLog.LayoutAppenders
{
    [LayoutAppender("level")]
    public class LevelLayoutAppender: LayoutAppender
    {
        private static string[]LevelToString;
        private static string[]UpperCaseLevelToString;
        private static string[]LowerCaseLevelToString;
        private static int MaxLength;

        static LevelLayoutAppender()
        {
            LevelToString = new string[(int)LogLevel.MaxLevel + 1];
            MaxLength = 0;
            for (int i = 0; i < LevelToString.Length; ++i)
            {
                LogLevel ll = (LogLevel)i;
                LevelToString[i] = ll.ToString();
                if (LevelToString[i].Length > MaxLength)
                    MaxLength = LevelToString[i].Length;
            }

            for (int i = 0; i < LevelToString.Length; ++i)
            {
                if (LevelToString[i].Length < MaxLength)
                {
                    LevelToString[i] = LevelToString[i] + new string(' ', MaxLength - LevelToString[i].Length);
                }
            }
            UpperCaseLevelToString = new string[(int)LogLevel.MaxLevel + 1];
            LowerCaseLevelToString = new string[(int)LogLevel.MaxLevel + 1];
            for (int i = 0; i < LevelToString.Length; ++i)
            {
                UpperCaseLevelToString[i] = LevelToString[i].ToUpper();
                LowerCaseLevelToString[i] = LevelToString[i].ToLower();
            }
        }

        public LevelLayoutAppender()
        {
            Padding = MaxLength;
        }

        private string[]_nameTable = LevelToString;

        public new bool UpperCase
        {
            set
            {
                if (value)
                    _nameTable = UpperCaseLevelToString;
                else
                    _nameTable = LevelToString;
            }
            get
            {
                return _nameTable == UpperCaseLevelToString;
            }
        }

        public new bool LowerCase
        {
            set
            {
                if (value)
                    _nameTable = LowerCaseLevelToString;
                else
                    _nameTable = LevelToString;
            }
            get
            {
                return _nameTable == LowerCaseLevelToString;
            }
        }

        protected internal override int GetEstimatedBufferSize(LogEventInfo ev)
        {
            return MaxLength;
        }

        protected internal override void Append(StringBuilder builder, LogEventInfo ev)
        {
            builder.Append(ApplyPadding(_nameTable[(int)ev.Level]));
        }
    }
}
