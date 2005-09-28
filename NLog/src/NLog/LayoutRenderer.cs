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
using System.Collections;
using System.Text;
using System.Globalization;

namespace NLog
{
    /// <summary>
    /// Render environmental information related to logging events.
    /// </summary>
    public abstract class LayoutRenderer
    {
        /// <summary>
        /// Creates a new instance of <see cref="LayoutRenderer" />
        /// </summary>
        protected LayoutRenderer(){}

        /// <summary>
        /// Returns the estimated number of characters that are needed to
        /// hold the rendered value for the specified logging event.
        /// </summary>
        /// <param name="logEvent">Logging event information.</param>
        /// <returns>The number of characters.</returns>
        /// <remarks>
        /// If the exact number is not known or
        /// expensive to calculate this function should return a rough estimate
        /// that's big enough in most cases, but not too big, in order to conserve memory.
        /// </remarks>
        protected internal abstract int GetEstimatedBufferSize(LogEventInfo logEvent);

        /// <summary>
        /// Determines whether stack trace information should be gathered
        /// during log event processing. By default it calls <see cref="NLog.Layout.NeedsStackTrace" /> on
        /// <see cref="Target.CompiledLayout" />.
        /// </summary>
        /// <returns>0 - don't include stack trace<br/>1 - include stack trace without source file information<br/>2 - include full stack trace</returns>
        protected internal virtual int NeedsStackTrace()
        {
            return 0;
        }

        /// <summary>
        /// Determines whether the layout renderer is volatile.
        /// </summary>
        /// <returns>A boolean indicating whether the layout renderer is volatile.</returns>
        /// <remarks>
        /// Volatile layout renderers are dependent on information not contained 
        /// in <see cref="LogEventInfo"/> (such as thread-specific data, MDC data, NDC data).
        /// </remarks>
        protected internal virtual bool IsVolatile()
        {
            LayoutRendererAttribute attr = (LayoutRendererAttribute)Attribute.GetCustomAttribute(this.GetType(), typeof(LayoutRendererAttribute));
            return !attr.UsingLogEventInfo;
        }

        /// <summary>
        /// Renders the specified environmental information and appends it to the specified <see cref="StringBuilder" />.
        /// </summary>
        /// <param name="builder">The <see cref="StringBuilder"/> to append the rendered data to.</param>
        /// <param name="logEvent">Logging event.</param>
        protected internal abstract void Append(StringBuilder builder, LogEventInfo logEvent);

        private int _padding = 0;
        private bool _fixedLength = false;
        private int _absolutePadding = 0;
        private bool _upperCase = false;
        private bool _lowerCase = false;
        private char _padCharacter = ' ';
        private CultureInfo _cultureInfo = CultureInfo.InvariantCulture;

        /// <summary>
        /// Padding value.
        /// </summary>
        public int Padding
        {
            get
            {
                return _padding;
            }
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
            get
            {
                return _absolutePadding;
            }
        }

        /// <summary>
        /// The padding character.
        /// </summary>
        public char PadCharacter
        {
            get
            {
                return _padCharacter;
            }
            set
            {
                _padCharacter = value;
            }
        }

        /// <summary>
        /// Trim the rendered text to the AbsolutePadding value.
        /// </summary>
        [System.ComponentModel.DefaultValue(false)]
        public bool FixedLength
        {
            get
            {
                return _fixedLength;
            }
            set
            {
                _fixedLength = value;
            }
        }

        /// <summary>
        /// Render an upper-case string.
        /// </summary>
        [System.ComponentModel.DefaultValue(false)]
        public bool UpperCase
        {
            get
            {
                return _upperCase;
            }
            set
            {
                _upperCase = value;
            }
        }

        /// <summary>
        /// Render an upper-case string.
        /// </summary>
        [System.ComponentModel.DefaultValue(false)]
        public bool LowerCase
        {
            get
            {
                return _lowerCase;
            }
            set
            {
                _lowerCase = value;
            }
        }

        /// <summary>
        /// The culture name to be used for rendering.
        /// </summary>
        public string Culture
        {
            get
            {
                return _cultureInfo.Name;
            }
            set
            {
                _cultureInfo = new CultureInfo(value);
            }
        }

        /// <summary>
        /// The <see cref="System.Globalization.CultureInfo" /> to be used for rendering.
        /// </summary>
        public CultureInfo CultureInfo
        {
            get
            {
                return _cultureInfo;
            }
            set
            {
                _cultureInfo = value;
            }
        }

        /// <summary>
        /// Determines whether it's necessary to call <see cref="ApplyPadding" />.
        /// </summary>
        /// <returns><see langword="true"/> when there's any 
        /// trimming, padding or case conversion necessary, 
        /// <see langword="false"/> otherwise</returns>
        /// <remarks>
        /// Should this method return <see langword="true"/>,
        /// it's necessary to call ApplyPadding on a rendered text, 
        /// otherwise it's not necessary to do so.
        /// </remarks>
        protected bool NeedPadding()
        {
            return (Padding != 0) || UpperCase || LowerCase;
        }


        /// <summary>
        /// Post-processes the rendered message by applying padding, 
        /// upper- and lower-case conversion.
        /// </summary>
        /// <param name="s">The text to be post-processed.</param>
        /// <returns>Padded, trimmed, and case-converted string.</returns>
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
