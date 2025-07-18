//
// Copyright (c) 2004-2024 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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

namespace NLog.LayoutRenderers
{
    using System.Text;
    using NLog.Config;
    using NLog.Internal;
    using NLog.Layouts;

    /// <summary>
    /// The environment variable.
    /// </summary>
    /// <remarks>
    /// <a href="https://github.com/NLog/NLog/wiki/Environment-Layout-Renderer">See NLog Wiki</a>
    /// </remarks>
    /// <seealso href="https://github.com/NLog/NLog/wiki/Environment-Layout-Renderer">Documentation on NLog Wiki</seealso>
    [LayoutRenderer("environment")]
    [ThreadAgnostic]
    public class EnvironmentLayoutRenderer : LayoutRenderer, IStringValueRenderer
    {
        /// <summary>
        /// Gets or sets the name of the environment variable.
        /// </summary>
        /// <remarks><b>[Required]</b> Default: <see cref="string.Empty"/></remarks>
        /// <docgen category='Layout Options' order='10' />
        [DefaultParameter]
        public string Variable { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the default value to be used when the environment variable is not set.
        /// </summary>
        /// <remarks>Default: <see cref="string.Empty"/></remarks>
        /// <docgen category='Layout Options' order='10' />
        public string Default { get; set; } = string.Empty;

        private System.Collections.Generic.KeyValuePair<string, SimpleLayout> _cachedValue;

        /// <inheritdoc/>
        protected override void InitializeLayoutRenderer()
        {
            base.InitializeLayoutRenderer();

            if (string.IsNullOrEmpty(Variable))
                throw new NLogConfigurationException("Environment-LayoutRenderer Variable-property must be assigned. Lookup blank value not supported.");
        }

        /// <inheritdoc/>
        protected override void Append(StringBuilder builder, LogEventInfo logEvent)
        {
            GetSimpleLayout()?.Render(logEvent, builder);
        }

        string? IStringValueRenderer.GetFormattedString(LogEventInfo logEvent)
        {
            var simpleLayout = GetSimpleLayout();
            if (simpleLayout is null)
                return string.Empty;
            if (simpleLayout.IsFixedText || simpleLayout.IsSimpleStringText)
                return simpleLayout.Render(logEvent);
            return null;
        }

        private SimpleLayout? GetSimpleLayout()
        {
            if (Variable != null)
            {
                var environmentVariable = EnvironmentHelper.GetSafeEnvironmentVariable(Variable);
                if (string.IsNullOrEmpty(environmentVariable))
                    environmentVariable = Default;

                if (!string.IsNullOrEmpty(environmentVariable))
                {
                    var cachedValue = _cachedValue;
                    if (string.CompareOrdinal(cachedValue.Key, environmentVariable) != 0)
                    {
                        cachedValue = new System.Collections.Generic.KeyValuePair<string, SimpleLayout>(environmentVariable,
                            new SimpleLayout(environmentVariable));
                        _cachedValue = cachedValue;
                    }

                    return cachedValue.Value;
                }
            }

            return null;
        }
    }
}
