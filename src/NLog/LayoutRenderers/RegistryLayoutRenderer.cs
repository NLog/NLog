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

#if !SILVERLIGHT
namespace NLog.LayoutRenderers
{
    using System;
    using System.Globalization;
    using System.Text;
    using Microsoft.Win32;
    using NLog;
    using NLog.Internal;
    using NLog.Config;
    using NLog.LayoutRenderers;
    using System.ComponentModel;
    using NLog.Layouts;

    /// <summary>
    /// A value from the Registry.
    /// </summary>
    [LayoutRenderer("registry")]
    public class RegistryLayoutRenderer : LayoutRenderer
    {
        /// <summary>
        /// Create new renderer
        /// </summary>
        public RegistryLayoutRenderer()
        {
            RequireEscapingSlashesInDefaultValue = true;
        }

        /// <summary>
        /// Gets or sets the registry value name.
        /// </summary>
        /// <docgen category='Registry Options' order='10' />
        public Layout Value { get; set; }

        /// <summary>
        /// Gets or sets the value to be output when the specified registry key or value is not found.
        /// </summary>
        /// <docgen category='Registry Options' order='10' />
        public Layout DefaultValue { get; set; }

        /// <summary>
        /// Require escaping backward slashes in <see cref="DefaultValue"/>. Need to be backwardscompatible.
        /// 
        /// When true:
        /// 
        /// `\` in value should be configured as `\\`
        /// `\\` in value should be configured as `\\\\`.
        /// </summary>
        /// <remarks>Default value wasn't a Layout before and needed an escape of the slash</remarks>
        [DefaultValue(true)]
        public bool RequireEscapingSlashesInDefaultValue { get; set; }

#if !NET3_5
        /// <summary>
        /// Gets or sets the registry view (see: https://msdn.microsoft.com/de-de/library/microsoft.win32.registryview.aspx). 
        /// Allowed values: Registry32, Registry64, Default 
        /// </summary>
        [DefaultValue("Default")]
        public RegistryView View { get; set; }
#endif
        /// <summary>
        /// Gets or sets the registry key.
        /// </summary>
        /// <remarks>
        /// Must have one of the forms:
        /// <ul>
        /// <li>HKLM\Key\Full\Name</li>
        /// <li>HKEY_LOCAL_MACHINE\Key\Full\Name</li>
        /// <li>HKCU\Key\Full\Name</li>
        /// <li>HKEY_CURRENT_USER\Key\Full\Name</li>
        /// <li>HKLM</li>
        /// <li>HKEY_LOCAL_MACHINE</li>
        /// </ul>
        /// </remarks>
        /// <docgen category='Registry Options' order='10' />
        [RequiredParameter]
        public Layout Key { get; set; }

        /// <summary>
        /// Reads the specified registry key and value and appends it to
        /// the passed <see cref="StringBuilder"/>.
        /// </summary>
        /// <param name="builder">The <see cref="StringBuilder"/> to append the rendered data to.</param>
        /// <param name="logEvent">Logging event. Ignored.</param>
        protected override void Append(StringBuilder builder, LogEventInfo logEvent)
        {
            Object registryValue = null;
            // Value = null is necessary for querying "unnamed values"
            string renderedValue = this.Value != null ? this.Value.Render(logEvent) : null;

            var parseResult = ParseKey(this.Key.Render(logEvent));
            try
            {
#if !NET3_5
                using (RegistryKey rootKey = RegistryKey.OpenBaseKey(parseResult.Hive, View))
#else                  
                var rootKey = MapHiveToKey(parseResult.Hive);

#endif

                {

                    if (parseResult.HasSubKey)
                    {
                        using (RegistryKey registryKey = rootKey.OpenSubKey(parseResult.SubKey))
                        {
                            if (registryKey != null) registryValue = registryKey.GetValue(renderedValue);
                        }
                    }
                    else
                    {
                        registryValue = rootKey.GetValue(renderedValue);
                    }
                }
            }
            catch (Exception ex)
            {
                if (ex.MustBeRethrown())
                {
                    throw;
                }
            }

            string value = null;
            if (registryValue != null) // valid value returned from registry will never be null
            {
                value = Convert.ToString(registryValue, CultureInfo.InvariantCulture);
            }
            else if (this.DefaultValue != null)
            {
                value = this.DefaultValue.Render(logEvent);

                if (RequireEscapingSlashesInDefaultValue)
                {
                    //remove escape slash
                    value = value.Replace("\\\\", "\\");
                }
            }
            builder.Append(value);
        }

        private class ParseResult
        {
            public string SubKey { get; set; }

            public RegistryHive Hive { get; set; }

            /// <summary>
            /// Has <see cref="SubKey"/>?
            /// </summary>
            public bool HasSubKey
            {
                get { return !string.IsNullOrEmpty(SubKey); }
            }
        }

        /// <summary>
        /// Parse key to <see cref="RegistryHive"/> and subkey.
        /// </summary>
        /// <param name="key">full registry key name</param>
        /// <returns>Result of parsing, never <c>null</c>.</returns>
        private static ParseResult ParseKey(string key)
        {
            string hiveName;
            int pos = key.IndexOfAny(new char[] { '\\', '/' });

            string subkey = null;
            if (pos >= 0)
            {
                hiveName = key.Substring(0, pos);

                //normalize slashes
                subkey = key.Substring(pos + 1).Replace('/', '\\');

                //remove starting slashes
                subkey = subkey.TrimStart('\\');

                //replace double slashes from pre-layout times
                subkey = subkey.Replace("\\\\", "\\");

            }
            else
            {
                hiveName = key;
            }

            RegistryHive hive;
            switch (hiveName.ToUpper(CultureInfo.InvariantCulture))
            {
                case "HKEY_LOCAL_MACHINE":
                case "HKLM":
                    hive = RegistryHive.LocalMachine;
                    break;

                case "HKEY_CURRENT_USER":
                case "HKCU":
                   hive = RegistryHive.CurrentUser;
                    break;

                default:
                    throw new ArgumentException("Key name is invalid. Root hive not recognized.");
            }

            return new ParseResult
            {
                SubKey = subkey,
                Hive = hive,
            };
        }

#if NET3_5
        private static RegistryKey MapHiveToKey(RegistryHive hive)
        {
            switch(hive)
            {
                case RegistryHive.LocalMachine:
                    return Registry.LocalMachine;
                case RegistryHive.CurrentUser:
                    return Registry.CurrentUser;
                default:
                    throw new ArgumentException("Only RegistryHive.LocalMachine and RegistryHive.CurrentUser are supported.", "hive");
            }
        }
#endif
    }
}

#endif
