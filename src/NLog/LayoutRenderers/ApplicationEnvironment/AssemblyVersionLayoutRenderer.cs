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

namespace NLog.LayoutRenderers
{
    using System;
    using System.ComponentModel;
    using System.Text;
    using NLog.Config;
    using NLog.Internal;

    /// <summary>
    /// Renders the assembly version information for the entry assembly or a named assembly.
    /// </summary>
    /// <remarks>
    /// As this layout renderer uses reflection and version information is unlikely to change during application execution,
    /// it is recommended to use it in conjunction with the <see cref="NLog.LayoutRenderers.Wrappers.CachedLayoutRendererWrapper"/>.
    /// </remarks>
    /// <remarks>
    /// The entry assembly can't be found in some cases e.g. ASP.NET, unit tests, etc.
    /// </remarks>
    [LayoutRenderer("assembly-version")]
    [ThreadAgnostic]
    [ThreadSafe]
    public class AssemblyVersionLayoutRenderer : LayoutRenderer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AssemblyVersionLayoutRenderer" /> class.
        /// </summary>
        public AssemblyVersionLayoutRenderer()
        {
            Type = AssemblyVersionType.Assembly;
            Format = DefaultFormat;
        }

        /// <summary>
        /// The (full) name of the assembly. If <c>null</c>, using the entry assembly.
        /// </summary>
        /// <docgen category='Rendering Options' order='10' />
        [DefaultParameter]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the type of assembly version to retrieve.
        /// </summary>
        /// <remarks>
        /// Some version type and platform combinations are not fully supported.
        /// - UWP earlier than .NET Standard 1.5: Value for <see cref="AssemblyVersionType.Assembly"/> is always returned unless the <see cref="Name"/> parameter is specified.
        /// </remarks>
        /// <docgen category='Rendering Options' order='10' />
        [DefaultValue(nameof(AssemblyVersionType.Assembly))]
        public AssemblyVersionType Type { get; set; }

        private const string DefaultFormat = "major.minor.build.revision";

        private string _format;

        /// <summary>
        /// Gets or sets the custom format of the assembly version output.
        /// </summary>
        /// <remarks>
        /// Supported placeholders are 'major', 'minor', 'build' and 'revision'.
        /// The default .NET template for version numbers is 'major.minor.build.revision'. See
        /// https://docs.microsoft.com/en-gb/dotnet/api/system.version?view=netframework-4.7.2#remarks
        /// for details.
        /// </remarks>
        /// <docgen category='Rendering Options' order='10' />
        [DefaultValue(DefaultFormat)]
        public string Format
        {
            get => _format;
            set => _format = value?.ToLowerInvariant();
        }

        /// <summary>
        /// Initializes the layout renderer.
        /// </summary>
        protected override void InitializeLayoutRenderer()
        {
            _assemblyVersion = null;
            base.InitializeLayoutRenderer();
        }

        /// <summary>
        /// Closes the layout renderer.
        /// </summary>
        protected override void CloseLayoutRenderer()
        {
            _assemblyVersion = null;
            base.CloseLayoutRenderer();
        }

        private string _assemblyVersion;

        /// <summary>
        /// Renders an assembly version and appends it to the specified <see cref="StringBuilder" />.
        /// </summary>
        /// <param name="builder">The <see cref="StringBuilder"/> to append the rendered data to.</param>
        /// <param name="logEvent">Logging event.</param>
        protected override void Append(StringBuilder builder, LogEventInfo logEvent)
        {
            var version = _assemblyVersion ?? (_assemblyVersion = ApplyFormatToVersion(GetVersion()));

            if (string.IsNullOrEmpty(version))
            {
                version = $"Could not find value for {(string.IsNullOrEmpty(Name) ? "entry" : Name)} assembly and version type {Type}";
            }

            builder.Append(version);
        }

        private string ApplyFormatToVersion(string version)
        {
            if (Format.Equals(DefaultFormat, StringComparison.OrdinalIgnoreCase) || string.IsNullOrEmpty(version))
            {
                return version;
            }

            var versionParts = version.SplitAndTrimTokens('.');
            version = Format.Replace("major", versionParts[0])
                .Replace("minor", versionParts.Length > 1 ? versionParts[1] : "0")
                .Replace("build", versionParts.Length > 2 ? versionParts[2] : "0")
                .Replace("revision", versionParts.Length > 3 ? versionParts[3] : "0");

            return version;
        }

#if NETSTANDARD1_3

        private string GetVersion()
        {
            if (string.IsNullOrEmpty(Name))
            {
                return Microsoft.Extensions.PlatformAbstractions.PlatformServices.Default.Application.ApplicationVersion;
            }
            else
            {
                var assembly = GetAssembly();
                return assembly?.GetName().Version.ToString();
            }
        }

        private System.Reflection.Assembly GetAssembly()
        {
            return System.Reflection.Assembly.Load(new System.Reflection.AssemblyName(Name));
        }

#else

        private string GetVersion()
        {
            var assembly = GetAssembly();
            return GetVersion(assembly);
        }

        /// <summary>
        /// Gets the assembly specified by <see cref="Name"/>, or entry assembly otherwise
        /// </summary>
        /// <returns>Found assembly</returns>
        protected virtual System.Reflection.Assembly GetAssembly()
        {
            if (string.IsNullOrEmpty(Name))
            {
                return System.Reflection.Assembly.GetEntryAssembly();
            }
            else
            {
                return System.Reflection.Assembly.Load(new System.Reflection.AssemblyName(Name));
            }
        }

        private string GetVersion(System.Reflection.Assembly assembly)
        {
            switch (Type)
            {
                case AssemblyVersionType.File:
                    return assembly?.GetFirstCustomAttribute<System.Reflection.AssemblyFileVersionAttribute>()?.Version;

                case AssemblyVersionType.Informational:
                    return assembly?.GetFirstCustomAttribute<System.Reflection.AssemblyInformationalVersionAttribute>()?.InformationalVersion;

                default:
                    return assembly?.GetName().Version?.ToString();
            }
        }

#endif

    }
}
