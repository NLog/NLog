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

using NLog.Layouts;

#if !SILVERLIGHT && !__IOS__ && !__ANDROID__ && !NETSTANDARD

namespace NLog.LayoutRenderers
{
    using System;
    using System.Text;
    using NLog.Config;
    using NLog.Internal;

    /// <summary>
    /// Application setting.
    /// </summary>
    /// <remarks>
    /// Use this layout renderer to insert the value of an application setting
    /// stored in the application's App.config or Web.config file.
    /// </remarks>
    /// <code lang="NLog Layout Renderer">
    /// ${appsetting:item=mysetting:default=mydefault} - produces "mydefault" if no appsetting
    /// </code>
    [LayoutRenderer("appsetting")]
    [ThreadAgnostic]
    [ThreadSafe]
    public sealed class AppSettingLayoutRenderer2 : LayoutRenderer, IStringValueRenderer
    {
        ///<summary>
        /// The AppSetting item-name
        ///</summary>
        /// <docgen category='Rendering Options' order='10' />
        [RequiredParameter]
        [DefaultParameter]
        public Layout Item { get; set; }

        ///<summary>
        /// The AppSetting item-name
        ///</summary>
        [Obsolete("Allows easier conversion from NLog.Extended. Instead use Item-property")]
        public Layout Name { get => Item; set => Item = value; }

        ///<summary>
        /// The default value to render if the AppSetting value is null.
        ///</summary>
        /// <docgen category='Rendering Options' order='10' />
        public Layout Default { get; set; }

        internal IConfigurationManager ConfigurationManager { get; set; } = new ConfigurationManager();

        /// <summary>
        /// Renders the specified application setting or default value and appends it to the specified <see cref="StringBuilder" />.
        /// </summary>
        /// <param name="builder">The <see cref="StringBuilder"/> to append the rendered data to.</param>
        /// <param name="logEvent">Logging event.</param>
        protected override void Append(StringBuilder builder, LogEventInfo logEvent)
        {
            builder.Append(GetStringValue(logEvent));
        }

        string IStringValueRenderer.GetFormattedString(LogEventInfo logEvent) => GetStringValue(logEvent);

        private string GetStringValue(LogEventInfo logEvent)
        {
            var item = Item.Render(logEvent);
            if (string.IsNullOrEmpty(item))
                return Default.Render(logEvent);

            string value = ConfigurationManager.AppSettings[item];
            if (value == null && Default != null)
                value = Default.Render(logEvent);

            return value ?? string.Empty;
        }
    }
}

#endif