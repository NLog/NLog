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

namespace NLog.LayoutRenderers
{
    using System.Text;
    using NLog.Config;
#if SILVERLIGHT
	using System.Windows;
#endif
    using System.Reflection;

    /// <summary>
    /// Assembly version.
    /// </summary>
    /// <remarks>The entry assembly can't be found in some cases e.g. ASP.NET, Unit tests etc.</remarks>
    [LayoutRenderer("assembly-version")]
    public class AssemblyVersionLayoutRenderer : LayoutRenderer
    {
        /// <summary>
        /// The (full) name of the assembly. If <c>null</c>, using the entry assembly.
        /// </summary>
        [DefaultParameter]
        public string Name { get; set; }

        /// <summary>
        /// Renders assembly version and appends it to the specified <see cref="StringBuilder" />.
        /// </summary>
        /// <param name="builder">The <see cref="StringBuilder"/> to append the rendered data to.</param>
        /// <param name="logEvent">Logging event.</param>
        protected override void Append(StringBuilder builder, LogEventInfo logEvent)
        {
#if SILVERLIGHT
            string name;
#else
            Assembly assembly;
#endif
            

            var nameNotEmpty = !string.IsNullOrEmpty(Name);
            if (nameNotEmpty)
            {
#if SILVERLIGHT
                name = Name;
#else
                assembly = Assembly.Load(new AssemblyName(Name));
#endif
            }
            else
            {

#if SILVERLIGHT
			    var assembly = Application.Current.GetType().Assembly;
                
                name = assembly.FullName;
#else
                assembly = Assembly.GetEntryAssembly();

#endif
            }
            var message = string.Format("Could not find {0}", nameNotEmpty ? "assembly " + Name : "entry assembly");

#if !SILVERLIGHT
            var assemblyVersion = assembly == null ? message : assembly.GetName().Version.ToString();
#else
            var assemblyVersion = name == null ? message : new AssemblyName(name).Version.ToString();
#endif
            builder.Append(assemblyVersion);
        }
    }
}
