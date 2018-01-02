// 
// Copyright (c) 2004-2017 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Reflection;
    using System.Text;
    using NLog.Config;

    /// <summary>
    /// Assembly version.
    /// </summary>
    /// <remarks>The entry assembly can't be found in some cases e.g. ASP.NET, Unit tests etc.</remarks>
    [LayoutRenderer("assembly-version")]
    public class AssemblyVersionLayoutRenderer : LayoutRenderer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AssemblyVersionLayoutRenderer" /> class.
        /// </summary>
        public AssemblyVersionLayoutRenderer()
        {
            Type = AssemblyVersionType.Assembly;
        }

        /// <summary>
        /// The (full) name of the assembly. If <c>null</c>, using the entry assembly.
        /// </summary>
        [DefaultParameter]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the type of assembly version to retrieve.
        /// </summary>
        /// <remarks>
        /// Some version type and platform combinations are not fully supported.
        /// UWP versions prior to 10.0.16299: Value for <see cref="AssemblyVersionType.Assembly"/> is always returned unless <see cref="Name"/> is given.
        /// Silverlight: Value for <see cref="AssemblyVersionType.Assembly"/> is always returned.
        /// </remarks>
        /// <docgen category='Rendering Options' order='10' />
        [DefaultValue(nameof(AssemblyVersionType.Assembly))]
        public AssemblyVersionType Type { get; set; }

        /// <summary>
        /// Renders an assembly version and appends it to the specified <see cref="StringBuilder" />.
        /// </summary>
        /// <param name="builder">The <see cref="StringBuilder"/> to append the rendered data to.</param>
        /// <param name="logEvent">Logging event.</param>
        protected override void Append(StringBuilder builder, LogEventInfo logEvent)
        {
            var version = GetVersion();

            if (string.IsNullOrEmpty(version))
            {
                version = $"Could not find {(string.IsNullOrEmpty(Name) ? "entry" : Name)} assembly";
            }

            builder.Append(version);
        }

#if SILVERLIGHT

        private string GetVersion()
        {
            if (string.IsNullOrEmpty(Name))
            {
                return new AssemblyName(System.Windows.Application.Current.GetType().Assembly.FullName).Version.ToString();
            }
            else
            {
                return new AssemblyName(Name).Version.ToString();
            }
        }

#elif WINDOWS_UWP && !NETSTANDARD1_5

        private string GetVersion()
        {
            if (string.IsNullOrEmpty(Name))
            {
                return Microsoft.Extensions.PlatformAbstractions.PlatformServices.Default.Application.ApplicationVersion;
            }
            else
            {
                return GetVersion(GetAssembly());
            }
        }

        private Assembly GetAssembly()
        {
            return Assembly.Load(new AssemblyName(Name));
        }

#else

        private string GetVersion()
        {
            return GetVersion(GetAssembly());
        }

        private Assembly GetAssembly()
        {
            if (string.IsNullOrEmpty(Name))
            {
                return Assembly.GetEntryAssembly();
            }
            else
            {
                return Assembly.Load(new AssemblyName(Name));
            }
        }

#endif

        private string GetVersion(Assembly assembly)
        {
            switch (Type)
            {
                case AssemblyVersionType.File:
                    return assembly?.GetCustomAttribute<AssemblyFileVersionAttribute>()?.Version;

                case AssemblyVersionType.Informational:
                    return assembly?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;

                case AssemblyVersionType.Product:
#if NET45 || NETCOREAPP1_0 || NETSTANDARD2_0
                    return FileVersionInfo.GetVersionInfo(assembly?.Location)?.ProductVersion;
#else
                    if (!string.IsNullOrEmpty(Name))
                        return FileVersionInfo.GetVersionInfo(Name)?.ProductVersion;
                    else
                        return assembly?.GetName().Version?.ToString();
#endif

                default:
                    return assembly?.GetName().Version?.ToString();
            }
        }
    }
}
