// 
// Copyright (c) 2004-2006 Jaroslaw Kowalski <jaak@jkowalski.net>
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
// * Neither the name of Jaroslaw Kowalski nor the names of its 
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
using System.IO;
using NLog.Internal;
using System.ComponentModel;
using NLog.Config;
using NLog.Layouts;

namespace NLog.LayoutRenderers.Wrappers
{
    /// <summary>
    /// Applies padding to another layout output.
    /// </summary>
    [LayoutRenderer("pad")]
    [AmbientProperty("Padding")]
    [AmbientProperty("PadCharacter")]
    [AmbientProperty("FixedLength")]
    public sealed class PaddngLayoutRendererWrapper : WrapperLayoutRendererBase
    {
        private int _padding = 0;
        private bool _fixedLength = false;
        private int _absolutePadding = 0;
        private char _padCharacter = ' ';

        /// <summary>
        /// Padding value.
        /// </summary>
        public int Padding
        {
            get { return _padding; }
            set
            {
                _padding = value;
                _absolutePadding = Math.Abs(_padding);
            }
        }

        /// <summary>
        /// The absolute value of the <see cref="Padding"/> property.
        /// </summary>
        public int AbsolutePadding
        {
            get { return _absolutePadding; }
        }

        /// <summary>
        /// The padding character.
        /// </summary>
        public char PadCharacter
        {
            get { return _padCharacter; }
            set { _padCharacter = value; }
        }

        /// <summary>
        /// Trim the rendered text to the AbsolutePadding value.
        /// </summary>
        [DefaultValue(false)]
        public bool FixedLength
        {
            get { return _fixedLength; }
            set { _fixedLength = value; }
        }

        /// <summary>
        /// Post-processes the rendered message by applying padding and/or trimming. 
        /// </summary>
        /// <param name="s">The text to be post-processed.</param>
        /// <returns>Padded and trimmed string.</returns>
        protected override string Transform(string text)
        {
            string s = text ?? String.Empty;

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
            return s;
        }

    }
}
