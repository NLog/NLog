// 
// Copyright (c) 2004-2011 Jaroslaw Kowalski <jaak@jkowalski.net>
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

#if (SILVERLIGHT || DOCUMENTATION) && !WINDOWS_PHONE

namespace NLog.LayoutRenderers
{
    using System;
    using System.ComponentModel;
    using System.Text;
#if !DOCUMENTATION
    using System.Windows;
    using System.Windows.Browser;
#endif

    using NLog.Config;

    /// <summary>
    /// Information about Silverlight application.
    /// </summary>
    [LayoutRenderer("sl-appinfo")]
    public class SilverlightApplicationInfoLayoutRenderer : LayoutRenderer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SilverlightApplicationInfoLayoutRenderer"/> class.
        /// </summary>
        public SilverlightApplicationInfoLayoutRenderer()
        {
            this.Option = SilverlightApplicationInfoOption.XapUri;
        }

        /// <summary>
        /// Gets or sets specific information to display.
        /// </summary>
        /// <docgen category='Rendering Options' order='10' />
        [DefaultParameter]
        [DefaultValue(SilverlightApplicationInfoOption.XapUri)]
        public SilverlightApplicationInfoOption Option { get; set; }

        /// <summary>
        /// Renders the specified environmental information and appends it to the specified <see cref="StringBuilder"/>.
        /// </summary>
        /// <param name="builder">The <see cref="StringBuilder"/> to append the rendered data to.</param>
        /// <param name="logEvent">Logging event.</param>
        protected override void Append(StringBuilder builder, LogEventInfo logEvent)
        {
#if !DOCUMENTATION
            switch (this.Option)
            {
                case SilverlightApplicationInfoOption.XapUri:
                    builder.Append(Application.Current.Host.Source);
                    break;

#if !SILVERLIGHT2
                case SilverlightApplicationInfoOption.IsOutOfBrowser:
                    builder.Append(Application.Current.IsRunningOutOfBrowser ? "1" : "0");
                    break;

                case SilverlightApplicationInfoOption.InstallState:
                    builder.Append(Application.Current.InstallState);
                    break;
#if !SILVERLIGHT3
                case SilverlightApplicationInfoOption.HasElevatedPermissions:
                    builder.Append(Application.Current.HasElevatedPermissions ? "1" : "0");
                    break;
#endif

#endif
            }
#endif
        }
    }
}

#endif
