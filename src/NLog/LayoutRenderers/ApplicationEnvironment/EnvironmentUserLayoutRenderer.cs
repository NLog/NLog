// 
// Copyright (c) 2004-2020 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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

#if !NETSTANDARD1_3 && !NETSTANDARD1_5

namespace NLog.LayoutRenderers
{
    using System;
    using System.ComponentModel;
    using System.Text;
    using NLog.Common;
    using NLog.Config;
    using NLog.Internal;

    /// <summary>
    /// Thread identity information (username).
    /// </summary>
    [LayoutRenderer("environment-user")]
    [ThreadSafe]
    public class EnvironmentUserLayoutRenderer : LayoutRenderer, IStringValueRenderer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EnvironmentUserLayoutRenderer" /> class.
        /// </summary>
        public EnvironmentUserLayoutRenderer()
        {
            UserName = true;
            Domain = false;
        }

        /// <summary>
        /// Gets or sets a value indicating whether username should be included.
        /// </summary>
        /// <docgen category='Rendering Options' order='10' />
        [DefaultValue(true)]
        public bool UserName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether domain name should be included.
        /// </summary>
        /// <docgen category='Rendering Options' order='10' />
        [DefaultValue(false)]
        public bool Domain { get; set; }

        /// <summary>
        /// Gets or sets the default value to be used when the User is not set.
        /// </summary>
        /// <docgen category='Rendering Options' order='10' />
        [DefaultValue("UserUnknown")]
        public string DefaultUser { get; set; } = "UserUnknown";

        /// <summary>
        /// Gets or sets the default value to be used when the Domain is not set.
        /// </summary>
        /// <docgen category='Rendering Options' order='10' />
        [DefaultValue("DomainUnknown")]
        public string DefaultDomain { get; set; } = "DomainUnknown";

        /// <inheritdoc/>
        protected override void Append(StringBuilder builder, LogEventInfo logEvent)
        {
            builder.Append(GetStringValue());
        }

        /// <inheritdoc/>
        string IStringValueRenderer.GetFormattedString(LogEventInfo logEvent) => GetStringValue();

        private string GetStringValue()
        {
            if (UserName)
            {
                return Domain ? string.Concat(GetDomainName(), "\\", GetUserName()) : GetUserName();
            }
            else
            {
                return Domain ? GetDomainName() : string.Empty;
            }
        }

        string GetUserName()
        {
            return GetValueSafe(() => Environment.UserName, DefaultUser);
        }

        string GetDomainName()
        {
            return GetValueSafe(() => Environment.UserDomainName, DefaultDomain);
        }

        private string GetValueSafe(Func<string> getValue, string defaultValue)
        {
            try
            {
                var value = getValue();
                return string.IsNullOrEmpty(value) ? (defaultValue ?? string.Empty) : value;
            }
            catch (Exception ex)
            {
                InternalLogger.Warn(ex, "Failed to lookup Environment-User. Fallback value={0}", defaultValue);
                return defaultValue ?? string.Empty;
            }
        }
    }
}

#endif