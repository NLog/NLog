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

namespace NLog
{
    using System;
    using System.Reflection;
    using NLog.Config;
    using NLog.Internal;
    using NLog.LayoutRenderers;
    using NLog.Targets;

    /// <summary>
    /// Extension methods to setup NLog extensions, so they are known when loading NLog LoggingConfiguration
    /// </summary>
    public static class SetupExtensionsBuilderExtensions
    {
        /// <summary>
        /// Enable/disables autoloading of NLog extensions by scanning and loading available assemblies
        /// </summary>
        /// <remarks>
        /// Enabled by default, and gives a huge performance hit during startup. Recommended to disable this when running in the cloud.
        /// </remarks>
        public static ISetupExtensionsBuilder AutoLoadAssemblies(this ISetupExtensionsBuilder extensionsBuilder, bool enable)
        {
            ConfigurationItemFactory.Default = enable ? null : new ConfigurationItemFactory(typeof(SetupBuilderExtensions).GetAssembly());
            return extensionsBuilder;
        }

        /// <summary>
        /// Registers NLog extensions from the assembly.
        /// </summary>
        public static ISetupExtensionsBuilder RegisterAssembly(this ISetupExtensionsBuilder extensionsBuilder, Assembly assembly)
        {
            ConfigurationItemFactory.Default.RegisterItemsFromAssembly(assembly);
            return extensionsBuilder;
        }

        /// <summary>
        /// Registers NLog extensions from the assembly type name
        /// </summary>
        public static ISetupExtensionsBuilder RegisterAssembly(this ISetupExtensionsBuilder extensionsBuilder, string assemblyName)
        {
            Assembly assembly = AssemblyHelpers.LoadFromName(assemblyName);
            ConfigurationItemFactory.Default.RegisterItemsFromAssembly(assembly);
            return extensionsBuilder;
        }

        /// <summary>
        /// Register a custom Target.
        /// </summary>
        /// <remarks>Short-cut for registering to default <see cref="ConfigurationItemFactory"/></remarks>
        /// <param name="extensionsBuilder">Fluent interface parameter.</param>
        /// <typeparam name="T"> Type of the Target.</typeparam>
        /// <param name="name"> Name of the Target.</param>
        public static ISetupExtensionsBuilder RegisterTarget<T>(this ISetupExtensionsBuilder extensionsBuilder, string name) where T : Target
        {
            var layoutRendererType = typeof(T);
            return RegisterTarget(extensionsBuilder, name, layoutRendererType);
        }

        /// <summary>
        /// Register a custom Target.
        /// </summary>
        /// <remarks>Short-cut for registering to default <see cref="ConfigurationItemFactory"/></remarks>
        /// <param name="extensionsBuilder">Fluent interface parameter.</param>
        /// <param name="targetType"> Type of the Target.</param>
        /// <param name="name"> Name of the Target.</param>
        public static ISetupExtensionsBuilder RegisterTarget(this ISetupExtensionsBuilder extensionsBuilder, string name, Type targetType)
        {
            ConfigurationItemFactory.Default.Targets.RegisterDefinition(name, targetType);
            return extensionsBuilder;
        }

        /// <summary>
        /// Register a custom layout renderer.
        /// </summary>
        /// <remarks>Short-cut for registering to default <see cref="ConfigurationItemFactory"/></remarks>
        /// <param name="extensionsBuilder">Fluent interface parameter.</param>
        /// <typeparam name="T"> Type of the layout renderer.</typeparam>
        /// <param name="name"> Name of the layout renderer - without ${}.</param>
        public static ISetupExtensionsBuilder RegisterLayoutRenderer<T>(this ISetupExtensionsBuilder extensionsBuilder, string name)
            where T : LayoutRenderer
        {
            var layoutRendererType = typeof(T);
            return RegisterLayoutRenderer(extensionsBuilder, name, layoutRendererType);
        }

        /// <summary>
        /// Register a custom layout renderer.
        /// </summary>
        /// <remarks>Short-cut for registering to default <see cref="ConfigurationItemFactory"/></remarks>
        /// <param name="extensionsBuilder">Fluent interface parameter.</param>
        /// <param name="layoutRendererType"> Type of the layout renderer.</param>
        /// <param name="name"> Name of the layout renderer - without ${}.</param>
        public static ISetupExtensionsBuilder RegisterLayoutRenderer(this ISetupExtensionsBuilder extensionsBuilder, string name, Type layoutRendererType)
        {
            ConfigurationItemFactory.Default.LayoutRenderers.RegisterDefinition(name, layoutRendererType);
            return extensionsBuilder;
        }

        /// <summary>
        /// Register a custom layout renderer with a callback function <paramref name="layoutMethod"/>. The callback receives the logEvent.
        /// </summary>
        /// <param name="extensionsBuilder">Fluent interface parameter.</param>
        /// <param name="name">Name of the layout renderer - without ${}.</param>
        /// <param name="layoutMethod">Callback that returns the value for the layout renderer.</param>
        public static ISetupExtensionsBuilder RegisterLayoutRenderer(this ISetupExtensionsBuilder extensionsBuilder, string name, Func<LogEventInfo, object> layoutMethod)
        {
            return RegisterLayoutRenderer(extensionsBuilder, name, (info, configuration) => layoutMethod(info));
        }

        /// <summary>
        /// Register a custom layout renderer with a callback function <paramref name="layoutMethod"/>. The callback receives the logEvent and the current configuration.
        /// </summary>
        /// <param name="extensionsBuilder">Fluent interface parameter.</param>
        /// <param name="name">Name of the layout renderer - without ${}.</param>
        /// <param name="layoutMethod">Callback that returns the value for the layout renderer.</param>
        public static ISetupExtensionsBuilder RegisterLayoutRenderer(this ISetupExtensionsBuilder extensionsBuilder, string name, Func<LogEventInfo, LoggingConfiguration, object> layoutMethod)
        {
            var layoutRenderer = new FuncLayoutRenderer(name, layoutMethod);
            ConfigurationItemFactory.Default.GetLayoutRenderers().RegisterFuncLayout(name, layoutRenderer);
            return extensionsBuilder;
        }
    }
}
