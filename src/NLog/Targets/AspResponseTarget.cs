// 
// Copyright (c) 2004-2016 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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

#if !SILVERLIGHT && !__IOS__

namespace NLog.Targets
{
    using System.Runtime.InteropServices;
    using NLog.Internal;

    /// <summary>
    /// Outputs log messages through the ASP Response object.
    /// </summary>
    /// <seealso href="https://github.com/nlog/nlog/wiki/AspResponse-target">Documentation on NLog Wiki</seealso>
    [Target("AspResponse")]
    public sealed class AspResponseTarget : TargetWithLayout
    {
        /// <summary>
        /// Gets or sets a value indicating whether to add &lt;!-- --&gt; comments around all written texts.
        /// </summary>
        /// <docgen category='Layout Options' order='100' />
        public bool AddComments { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AspResponseTarget" /> class.
        /// </summary>
        /// <remarks>
        /// The default value of the layout is: <code>${longdate}|${level:uppercase=true}|${logger}|${message}</code>
        /// </remarks>
        public AspResponseTarget() : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AspResponseTarget"/> class with a name.
        /// </summary>
        /// <remarks>
        /// The default value of the layout is: <code>${longdate}|${level:uppercase=true}|${logger}|${message}</code>
        /// </remarks>
        /// <param name="name">Name of the target.</param>
        public AspResponseTarget(string name) : this()
        {
            this.Name = name;
        }

        /// <summary>
        /// Outputs the rendered logging event through the <c>OutputDebugString()</c> Win32 API.
        /// </summary>
        /// <param name="logEvent">The logging event.</param>
        protected override void Write(LogEventInfo logEvent)
        {
            AspHelper.IResponse response = AspHelper.GetResponseObject();
            if (response != null)
            {
                if (this.AddComments)
                {
                    response.Write("<!-- " + this.Layout.Render(logEvent) + "-->");
                }
                else
                {
                    response.Write(this.Layout.Render(logEvent));
                }

                Marshal.ReleaseComObject(response);
            }
        }
    }
}

#endif
