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
    using NLog.Config;
    using NLog.Internal;

    /// <summary>
    /// A value from the Registry.
    /// </summary>
    [LayoutRenderer("registry")]
    public class RegistryLayoutRenderer : LayoutRenderer
    {
        private string key;
        private RegistryKey rootKey = Registry.LocalMachine;
        private string subKey;

        /// <summary>
        /// Gets or sets the registry value name.
        /// </summary>
        /// <docgen category='Registry Options' order='10' />
        public string Value { get; set; }

        /// <summary>
        /// Gets or sets the value to be output when the specified registry key or value is not found.
        /// </summary>
        /// <docgen category='Registry Options' order='10' />
        public string DefaultValue { get; set; }

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
        /// </ul>
        /// </remarks>
        /// <docgen category='Registry Options' order='10' />
        [RequiredParameter]
        public string Key
        {
            get
            {
                return this.key;
            }

            set
            {
                this.key = value;
                int pos = this.key.IndexOfAny(new char[] { '\\', '/' });

                if (pos >= 0)
                {
                    string root = this.key.Substring(0, pos);
                    switch (root.ToUpper(CultureInfo.InvariantCulture))
                    {
                        case "HKEY_LOCAL_MACHINE":
                        case "HKLM":
                            this.rootKey = Registry.LocalMachine;
                            break;

                        case "HKEY_CURRENT_USER":
                        case "HKCU":
                            this.rootKey = Registry.CurrentUser;
                            break;

                        default:
                            throw new ArgumentException("Key name is invalid. Root hive not recognized.");
                    }

                    this.subKey = this.key.Substring(pos + 1).Replace('/', '\\');
                }
                else
                {
                    throw new ArgumentException("Key name is invalid");
                }
            }
        }

        /// <summary>
        /// Reads the specified registry key and value and appends it to
        /// the passed <see cref="StringBuilder"/>.
        /// </summary>
        /// <param name="builder">The <see cref="StringBuilder"/> to append the rendered data to.</param>
        /// <param name="logEvent">Logging event. Ignored.</param>
        protected override void Append(StringBuilder builder, LogEventInfo logEvent)
        {
            string value;

            try
            {
                using (RegistryKey registryKey = this.rootKey.OpenSubKey(this.subKey))
                {
                    value = Convert.ToString(registryKey.GetValue(this.Value, this.DefaultValue), CultureInfo.InvariantCulture);
                }
            }
            catch (Exception ex)
            {
                if (ex.MustBeRethrown())
                {
                    throw;
                }

                value = this.DefaultValue;
            }

            builder.Append(value);
        }
    }
}

#endif
