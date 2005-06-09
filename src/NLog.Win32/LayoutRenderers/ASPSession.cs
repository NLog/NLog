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
using System.Text;
using System.Runtime.InteropServices;

using NLog.LayoutRenderers;
using NLog.Config;

namespace NLog.Win32.LayoutRenderers
{
    /// <summary>
    /// ASP Session variable.
    /// </summary>
    [LayoutRenderer("asp-session")]
    public class ASPSessionValueLayoutRenderer: LayoutRenderer
    {
        private string _sessionVariable = null;

        /// <summary>
        /// Session variable name.
        /// </summary>
        [RequiredParameter]
        public string Variable
        {
            get
            {
                return _sessionVariable;
            }
            set
            {
                _sessionVariable = value;
            }
        }

        /// <summary>
        /// Returns the estimated number of characters that are needed to
        /// hold the rendered value for the specified logging event.
        /// </summary>
        /// <param name="ev">Logging event information.</param>
        /// <returns>The number of characters.</returns>
        /// <remarks>
        /// Because ASP target uses COM Interop which is quite expensive, this method always returns 64.
        /// </remarks>
        protected override int GetEstimatedBufferSize(LogEventInfo ev)
        {
            return 64;
        }

        /// <summary>
        /// Renders the specified ASP Session variable and appends it to the specified <see cref="StringBuilder" />.
        /// </summary>
        /// <param name="builder">The <see cref="StringBuilder"/> to append the rendered data to.</param>
        /// <param name="ev">Logging event.</param>
        protected override void Append(StringBuilder builder, LogEventInfo ev)
        {
            ASPHelper.ISessionObject session = ASPHelper.GetSessionObject();
            if (session != null)
            {
                if (_sessionVariable != null)
                {

                    object variableValue = session.GetValue(_sessionVariable);
                    builder.Append(Convert.ToString(variableValue));
                }
                Marshal.ReleaseComObject(session);
            }
        }
    }
}
