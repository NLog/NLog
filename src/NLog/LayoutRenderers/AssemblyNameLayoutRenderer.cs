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

#if !SILVERLIGHT2 && !NET_CF

namespace NLog.LayoutRenderers
{
    using System;
    using System.Globalization;
    using System.ComponentModel;
    using System.Text;
    using NLog.Config;
    using System.Collections.Generic;
    using NLog.Common;
#if SILVERLIGHT
	using System.Windows;
#if SILVERLIGHT5
	using System.Reflection;
#endif
#else
    using System.Reflection;
#endif

    /// <summary>
    /// Assembly version.
    /// </summary>
    [LayoutRenderer("assembly-name")]
    public class AssemblyNameLayoutRenderer : LayoutRenderer {
        private string format;
        private AssemblyNameProperty property;
        private AssemblyName assemblyName;
        private AssemblyNameAppenders[] assemblyNameAppenders;


        /// <summary>
        /// Initializes a new instance of the <see cref="AssemblyNameLayoutRenderer" /> class.
        /// </summary>
        public AssemblyNameLayoutRenderer() {
            this.property = AssemblyNameProperty.EntryAssembly;
            SetAssemblyMethod(this.property);
            this.Separator = ", ";
            this.Format = "name,version";
        }

        /// <summary>
        /// Gets or Sets the method used to obtain the Assembly.
        /// When setting this will also recompile the format to use the new assembly name.
        /// </summary>
        [DefaultValue("EntryAssembly"), DefaultParameter]
        public AssemblyNameProperty Property {
            get { return property; }
            set {
                property = value;
                SetAssemblyMethod(value);
                this.assemblyNameAppenders = CompileFormat(this.format);
            }
        }

        /// <summary>
        /// Gets or Sets the format of the output. Must be a comma-separated list of 
        /// assembly name properties: Name, Version, Culture, PublicKeyToken
        /// 
        /// This parameter is case-insensitive
        /// </summary>
        [DefaultParameter]
        public string Format { 
            get { return this.format; } 
            set { 
                this.format = value;
                if (assemblyName != null) {
                    this.assemblyNameAppenders = CompileFormat(value);
                }
            } 
        }

        private delegate void AssemblyNameAppenders(StringBuilder sb);


        /// <summary>
        /// Gets or Sets whether the output should include the display names for each property.
        /// For example when ShowDisplayNames=True: Name=NLog, Version=1.0.0.0
        /// If the value is false these are omitted using the example above the output would be: NLog, 1.0.0.0 
        /// </summary>
        [DefaultValue(false)]
        public bool ShowDisplayNames { get; set; }

        /// <summary>
        /// Gets or sets the separator used to concatenate parts specified in the Format.
        /// </summary>
        [DefaultValue(", ")]
        public string Separator { get; set; }

        /// <summary>
        /// Renders assembly version and appends it to the specified <see cref="StringBuilder" />.
        /// </summary>
        /// <param name="builder">The <see cref="StringBuilder"/> to append the rendered data to.</param>
        /// <param name="logEvent">Logging event.</param>
        protected override void Append(StringBuilder builder, LogEventInfo logEvent) {
            if(this.assemblyName == null) {
                return;
            }
            var sb = new StringBuilder(256);
            var separator = String.Empty;
            foreach (var appenderFunc in this.assemblyNameAppenders) {
                sb.Append(separator);
                appenderFunc(sb);
                separator = this.Separator;
            }
            builder.Append(sb.ToString());
        }

        /// <summary>
        /// Appends the assemblies name property to a string builder
        /// </summary>
        /// <param name="sb">The string builder to append to</param>
        private void AppendAssemblyName(StringBuilder sb) {
            if(this.ShowDisplayNames) {
                sb.Append("Name=");
            }
            sb.Append(this.assemblyName.Name);
        }
        /// <summary>
        /// Appends the version number property to a string builder
        /// </summary>
        /// <param name="sb">The string builder to append to</param>
        private void AppendVersion(StringBuilder sb) {
            if(this.ShowDisplayNames) {
                sb.Append("Version=");
            }
            sb.Append(this.assemblyName.Version);
        }

        /// <summary>
        /// Appends the Two Letter ISO Language Name from the Culture Info
        /// property in the assembly name to a string builder
        /// </summary>
        /// <param name="sb">The strinb builder to append to</param>
        private void AppendCulture(StringBuilder sb) {
            if(this.ShowDisplayNames) {
                sb.Append("Culture=");
            }
            sb.Append(this.assemblyName.CultureInfo.TwoLetterISOLanguageName);
        }

        /// <summary>
        /// Appends the Public Key Token property to a string builder
        /// </summary>
        /// <param name="sb">The string builder to append to</param>
        private void AppendPublicKeyToken(StringBuilder sb) {
            if(this.ShowDisplayNames) {
                sb.Append("PublicKeyToken=");
            }
            sb.Append(this.assemblyName.GetPublicKeyToken().ToString());
        }

        /// <summary>
        /// Compiles the format property string and sets the appropriate appender
        /// function to be used to append to the logs output.
        /// </summary>
        /// <param name="formatSpecifier">A comma-separated string representing the output format</param>
        /// <returns>An array of appender functions</returns>
        private AssemblyNameAppenders[] CompileFormat(string formatSpecifier) {
            string[] parts = formatSpecifier.Replace(" ", string.Empty).Split(',');
            var appenders = new List<AssemblyNameAppenders>();

            foreach(string s in parts) {
                switch(s.ToUpper(CultureInfo.InvariantCulture)) {
                    case "NAME":
                        appenders.Add(AppendAssemblyName);
                        break;
                    case "VERSION":
                        appenders.Add(AppendVersion);
                        break;
                    case "CULTURE":
                        appenders.Add(AppendCulture);
                        break;
                    case "PUBLICKEYTOKEN":
                        appenders.Add(AppendPublicKeyToken);
                        break;
                    default:
                        InternalLogger.Warn("Unknown format token: {0}",s);
                        break;
                }
            }
            return appenders.ToArray();
        }


        /// <summary>
        /// Retrieves the assembly name from an assembly.
        /// The assembly used depends on the property given.
        /// 
        /// For further detail on the property see <see cref="AssemblyNameProperty"/>
        /// </summary>
        /// <param name="property"></param>
        private void SetAssemblyMethod(AssemblyNameProperty property) {
            try {
#if SILVERLIGHT
                assemblyName = Assembly.GetEntryAssembly.GetName();
#else
                switch (this.Property) {
                    case AssemblyNameProperty.EntryAssembly:
                        assemblyName = Assembly.GetEntryAssembly().GetName();
                        break;
                    case AssemblyNameProperty.ExecutingAssembly:
                        assemblyName = Assembly.GetExecutingAssembly().GetName();
                        break;
                    case AssemblyNameProperty.CallingAssembly:
                        assemblyName = Assembly.GetCallingAssembly().GetName();
                        break;
                    case AssemblyNameProperty.TestAssembly:
                        assemblyName = new AssemblyName("ExampleAssembly, Version=1.0.0.0, Culture=en, PublicKeyToken=a5d015c7d5a0b012");
                        break;
                    case AssemblyNameProperty.EmptyAssembly:
                    default:
                        assemblyName = null;
                        break;
                }
#endif
            } catch (NullReferenceException ex) {
                InternalLogger.Error("Error getting entry assemblies name: {0}", ex);
                assemblyName = null;
            }
        }
    }
}
#endif