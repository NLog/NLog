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

#if !NETCF

using System;
using System.Text;
using System.Web;

using NLog.Config;
using System.ComponentModel;

namespace NLog.LayoutRenderers
{
    /// <summary>
    /// ASP.NET Application variable.
    /// </summary>
    /// <remarks>
    /// Use this layout renderer to insert the value of the specified variable stored 
    /// in the ASP.NET Application dictionary.
    /// </remarks>
    /// <example>
    /// <para>You can set the value of an ASP.NET Application variable by using the following code:</para>
    /// <code lang="C#">
    /// <![CDATA[
    /// HttpContext.Current.Application["myvariable"] = 123;
    /// HttpContext.Current.Application["stringvariable"] = "aaa BBB";
    /// HttpContext.Current.Application["anothervariable"] = DateTime.Now;
    /// ]]>
    /// </code>
    /// <para>Example usage of ${aspnet-application}:</para>
    /// <code lang="NLog Layout Renderer">
    /// ${aspnet-application:variable=myvariable} - produces "123"
    /// ${aspnet-application:variable=anothervariable} - produces "01/01/2006 00:00:00"
    /// ${aspnet-application:variable=anothervariable:culture=pl-PL} - produces "2006-01-01 00:00:00"
    /// ${aspnet-application:variable=myvariable:padding=5} - produces "  123"
    /// ${aspnet-application:variable=myvariable:padding=-5} - produces "123  "
    /// ${aspnet-application:variable=stringvariable:upperCase=true} - produces "AAA BBB"
    /// </code>
    /// </example>
    [LayoutRenderer("aspnet-application")]
    [NotSupportedRuntime(Framework=RuntimeFramework.DotNetCompactFramework)]
    public class ASPNETApplicationValueLayoutRenderer: LayoutRenderer
    {
        private string _variable = null;

        /// <summary>
        /// The variable name.
        /// </summary>
        [RequiredParameter]
        [DefaultParameter]
        public string Variable
        {
            get { return _variable; }
            set { _variable = value; }
        }
        
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
        protected internal override int GetEstimatedBufferSize(LogEventInfo logEvent)
        {
            if (_variable == null)
                return 0;
            else
                return 64;
        }

        /// <summary>
        /// Renders the specified ASP.NET Application variable and appends it to the specified <see cref="StringBuilder" />.
        /// </summary>
        /// <param name="builder">The <see cref="StringBuilder"/> to append the rendered data to.</param>
        /// <param name="logEvent">Logging event.</param>
        protected internal override void Append(StringBuilder builder, LogEventInfo logEvent)
        {
            if (Variable == null)
                return ;

            HttpContext context = HttpContext.Current;
            if (context == null)
                return ;

			if (context.Application == null)
				return ;
			builder.Append(ApplyPadding(Convert.ToString(context.Application[Variable], CultureInfo)));
        }
    }
}

#endif
