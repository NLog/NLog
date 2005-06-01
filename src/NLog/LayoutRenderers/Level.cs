// 
// Copyright (c) 2004 Jaroslaw Kowalski <jkowalski@users.sourceforge.net>
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
using System.Text;

namespace NLog.LayoutRenderers
{
    /// <summary>
    /// The log level.
    /// </summary>
    [LayoutRenderer("level")]
    public class LevelLayoutRenderer: LayoutRenderer
    {
        private static string[]LevelToString;
        private static string[]UpperCaseLevelToString;
        private static string[]LowerCaseLevelToString;
        private static int MaxLength;

        static LevelLayoutRenderer()
        {
            LevelToString = new string[(int)LogLevel.MaxLevel + 1];
            MaxLength = 0;
            for (int i = 0; i < LevelToString.Length; ++i)
            {
                LogLevel ll = (LogLevel)i;
                LevelToString[i] = Logger.LogLevelToString(ll);
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

        private string[]_nameTable = LevelToString;

        /// <summary>
        /// Render an upper-case string.
        /// </summary>
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

        /// <summary>
        /// Render a lower-case string.
        /// </summary>
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

        /// <summary>
        /// Returns the estimated number of characters that are needed to
        /// hold the rendered value for the specified logging event.
        /// </summary>
        /// <param name="ev">Logging event information.</param>
        /// <returns>The number of characters.</returns>
        /// <remarks>
        /// If the exact number is not known or
        /// expensive to calculate this function should return a rough estimate
        /// that's big enough in most cases, but not too big, in order to conserve memory.
        /// </remarks>
        protected internal override int GetEstimatedBufferSize(LogEventInfo ev)
        {
            return MaxLength;
        }

        /// <summary>
        /// Renders the current log level and appends it to the specified <see cref="StringBuilder" />.
        /// </summary>
        /// <param name="builder">The <see cref="StringBuilder"/> to append the rendered data to.</param>
        /// <param name="ev">Logging event.</param>
        protected internal override void Append(StringBuilder builder, LogEventInfo ev)
        {
            builder.Append(ApplyPadding(_nameTable[(int)ev.Level]));
        }
    }
}
