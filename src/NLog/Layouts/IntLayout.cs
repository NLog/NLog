// 
// Copyright (c) 2004-2019 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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
using System.ComponentModel;
using System.Linq;
using NLog.Common;
using NLog.Internal;

namespace NLog.Layouts
{
    /// <summary>
    /// Layout rendering to int
    /// </summary>
    public class IntLayout : Layout
    {
        private readonly int? _number;
        private readonly Layout _layout;

        /// <summary>
        /// Layout rendering to int
        /// </summary>
        /// <param name="layout"></param>
        public IntLayout(Layout layout)
        {
            //todo test layout if fixed and if non-number
            _layout = layout;
            _number = null;
        }

        /// <summary>
        /// Layout with fixed int
        /// </summary>
        /// <param name="number"></param>
        public IntLayout(int? number)
        {
            _number = number;
            _layout = null;
        }

        /// <summary>
        /// Is fixed?
        /// </summary>
        public bool IsFixed => _number.HasValue;

        /// <summary>
        /// Converts a given text to a <see cref="Layout" />.
        /// </summary>
        /// <param name="number">Text to be converted.</param>
        /// <returns><see cref="SimpleLayout"/> object represented by the text.</returns>
        public static implicit operator IntLayout(int number)
        {
            return new IntLayout(number);
        }
        /// <summary>
        /// Converts a given text to a <see cref="Layout" />.
        /// </summary>
        /// <param name="layout">Text to be converted.</param>
        /// <returns><see cref="SimpleLayout"/> object represented by the text.</returns>
        public static implicit operator IntLayout([Localizable(false)] string layout)
        {
            return new IntLayout(layout);
        }

        /// <summary>
        /// Render To int
        /// </summary>
        /// <returns></returns>
        public int? RenderToInt(LogEventInfo logEvent)
        {
            if (_number.HasValue)
            {
                return _number;
            }

            if (_layout == null)
            {
                return null;
            }

            if (_layout.TryGetRawValue(logEvent, out var raw))
            {
                if (raw is int i1)
                {
                    return i1;
                }
                //todo use IPropertyTypeConverter?
                InternalLogger.Warn("rawvalue isn't a int ");
            }

            var text = _layout.Render(logEvent);
            if (int.TryParse(text, out var i)) //todo use IPropertyTypeConverter?
            {
                return i;
            }
            else
            {
                InternalLogger.Warn("Parse {0} to int failed", text);
                return null;
            }

        }

      

        #region Overrides of Layout

        /// <inheritdoc />
        protected override string GetFormattedMessage(LogEventInfo logEvent)
        {
            return _number?.ToString(LoggingConfiguration.DefaultCultureInfo) ?? _layout.Render(logEvent);
        }

        #endregion
    }
}
