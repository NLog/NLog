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

namespace NLog
{
    using System;
    using System.Reflection;
    using NLog.Config;
    using NLog.Internal;
    using NLog.Layouts;
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
        public static ISetupExtensionsBuilder AutoLoadAssemblies(this ISetupExtensionsBuilder setupBuilder, bool enable)
        {
            ConfigurationItemFactory.Default = enable ? null : new ConfigurationItemFactory(typeof(SetupBuilderExtensions).GetAssembly());
            return setupBuilder;
        }

        /// <summary>
        /// Registers NLog extensions from the assembly.
        /// </summary>
        public static ISetupExtensionsBuilder RegisterAssembly(this ISetupExtensionsBuilder setupBuilder, Assembly assembly)
        {
            setupBuilder.LogFactory.ServiceRepository.ConfigurationItemFactory.RegisterItemsFromAssembly(assembly);
            return setupBuilder;
        }

        /// <summary>
        /// Registers NLog extensions from the assembly type name
        /// </summary>
        public static ISetupExtensionsBuilder RegisterAssembly(this ISetupExtensionsBuilder setupBuilder, string assemblyName)
        {
            Assembly assembly = AssemblyHelpers.LoadFromName(assemblyName);
            setupBuilder.LogFactory.ServiceRepository.ConfigurationItemFactory.RegisterItemsFromAssembly(assembly);
            return setupBuilder;
        }

        /// <summary>
        /// Register a custom Target.
        /// </summary>
        /// <param name="setupBuilder">Fluent interface parameter.</param>
        /// <typeparam name="T"> Type of the Target.</typeparam>
        /// <param name="name"> Name of the Target.</param>
        public static ISetupExtensionsBuilder RegisterTarget<T>(this ISetupExtensionsBuilder setupBuilder, string name) where T : Target
        {
            var layoutRendererType = typeof(T);
            return RegisterTarget(setupBuilder, name, layoutRendererType);
        }

        /// <summary>
        /// Register a custom Target.
        /// </summary>
        /// <param name="setupBuilder">Fluent interface parameter.</param>
        /// <param name="targetType"> Type of the Target.</param>
        /// <param name="name"> Name of the Target.</param>
        public static ISetupExtensionsBuilder RegisterTarget(this ISetupExtensionsBuilder setupBuilder, string name, Type targetType)
        {
            setupBuilder.LogFactory.ServiceRepository.ConfigurationItemFactory.Targets.RegisterDefinition(name, targetType);
            return setupBuilder;
        }

        /// <summary>
        /// Register a custom layout renderer.
        /// </summary>
        /// <param name="setupBuilder">Fluent interface parameter.</param>
        /// <typeparam name="T"> Type of the layout renderer.</typeparam>
        /// <param name="name"> Name of the layout renderer - without ${}.</param>
        public static ISetupExtensionsBuilder RegisterLayoutRenderer<T>(this ISetupExtensionsBuilder setupBuilder, string name)
            where T : LayoutRenderer
        {
            var layoutRendererType = typeof(T);
            return RegisterLayoutRenderer(setupBuilder, name, layoutRendererType);
        }

        /// <summary>
        /// Register a custom layout renderer.
        /// </summary>
        /// <param name="setupBuilder">Fluent interface parameter.</param>
        /// <param name="layoutRendererType"> Type of the layout renderer.</param>
        /// <param name="name"> Name of the layout renderer - without ${}.</param>
        public static ISetupExtensionsBuilder RegisterLayoutRenderer(this ISetupExtensionsBuilder setupBuilder, string name, Type layoutRendererType)
        {
            setupBuilder.LogFactory.ServiceRepository.ConfigurationItemFactory.LayoutRenderers.RegisterDefinition(name, layoutRendererType);
            return setupBuilder;
        }

        /// <summary>
        /// Register a custom layout renderer with a callback function <paramref name="layoutMethod"/>. The callback receives the logEvent.
        /// </summary>
        /// <param name="setupBuilder">Fluent interface parameter.</param>
        /// <param name="name">Name of the layout renderer - without ${}.</param>
        /// <param name="layoutMethod">Callback that returns the value for the layout renderer.</param>
        public static ISetupExtensionsBuilder RegisterLayoutRenderer(this ISetupExtensionsBuilder setupBuilder, string name, Func<LogEventInfo, object> layoutMethod)
        {
            return RegisterLayoutRenderer(setupBuilder, name, (info, configuration) => layoutMethod(info));
        }

        /// <summary>
        /// Register a custom layout renderer with a callback function <paramref name="layoutMethod"/>. The callback receives the logEvent and the current configuration.
        /// </summary>
        /// <param name="setupBuilder">Fluent interface parameter.</param>
        /// <param name="name">Name of the layout renderer - without ${}.</param>
        /// <param name="layoutMethod">Callback that returns the value for the layout renderer.</param>
        public static ISetupExtensionsBuilder RegisterLayoutRenderer(this ISetupExtensionsBuilder setupBuilder, string name, Func<LogEventInfo, LoggingConfiguration, object> layoutMethod)
        {
            return RegisterLayoutRenderer(setupBuilder, name, layoutMethod, LayoutRenderOptions.None);
        }

        /// <summary>
        /// Register a custom layout renderer with a callback function <paramref name="layoutMethod"/>. The callback receives the logEvent.
        /// </summary>
        /// <param name="setupBuilder">Fluent interface parameter.</param>
        /// <param name="name">Name of the layout renderer - without ${}.</param>
        /// <param name="layoutMethod">Callback that returns the value for the layout renderer.</param>
        /// <param name="options">Options of the layout renderer.</param>
        public static ISetupExtensionsBuilder RegisterLayoutRenderer(this ISetupExtensionsBuilder setupBuilder, string name, Func<LogEventInfo, object> layoutMethod, LayoutRenderOptions options)
        {
            return RegisterLayoutRenderer(setupBuilder, name, (info, configuration) => layoutMethod(info), options);
        }

        /// <summary>
        /// Register a custom layout renderer with a callback function <paramref name="layoutMethod"/>. The callback receives the logEvent and the current configuration.
        /// </summary>
        /// <param name="setupBuilder">Fluent interface parameter.</param>
        /// <param name="name">Name of the layout renderer - without ${}.</param>
        /// <param name="layoutMethod">Callback that returns the value for the layout renderer.</param>
        /// <param name="options">Options of the layout renderer.</param>
        public static ISetupExtensionsBuilder RegisterLayoutRenderer(this ISetupExtensionsBuilder setupBuilder, string name, Func<LogEventInfo, LoggingConfiguration, object> layoutMethod, LayoutRenderOptions options)
        {
            FuncLayoutRenderer layoutRenderer = Layout.CreateFuncLayoutRenderer(layoutMethod, options, name);
            setupBuilder.LogFactory.ServiceRepository.ConfigurationItemFactory.GetLayoutRenderers().RegisterFuncLayout(name, layoutRenderer);
            return setupBuilder;
        }

        /// <summary>
        /// Register a custom condition method, that can use in condition filters
        /// </summary>
        /// <param name="setupBuilder">Fluent interface parameter.</param>
        /// <param name="name">Name of the condition filter method</param>
        /// <param name="conditionMethod">MethodInfo extracted by reflection - typeof(MyClass).GetMethod("MyFunc", BindingFlags.Static).</param>
        public static ISetupExtensionsBuilder RegisterConditionMethod(this ISetupExtensionsBuilder setupBuilder, string name, MethodInfo conditionMethod)
        {
            if (conditionMethod == null)
                throw new ArgumentNullException(nameof(conditionMethod));
            if (!conditionMethod.IsStatic)
                throw new ArgumentException($"{conditionMethod.Name} must be static", nameof(conditionMethod));

            setupBuilder.LogFactory.ServiceRepository.ConfigurationItemFactory.ConditionMethods.RegisterDefinition(name, conditionMethod);
            return setupBuilder;
        }

#if !NETSTANDARD1_3 && !NETSTANDARD1_5
        private static ISetupExtensionsBuilder RegisterConditionMethod(this ISetupExtensionsBuilder setupBuilder, string name, MethodInfo conditionMethod, ReflectionHelpers.LateBoundMethod lateBoundMethod)
        {
            setupBuilder.LogFactory.ServiceRepository.ConfigurationItemFactory.ConditionMethodDelegates.RegisterDefinition(name, conditionMethod, lateBoundMethod);
            return setupBuilder;
        }

        /// <summary>
        /// Register a custom condition method, that can use in condition filters
        /// </summary>
        /// <param name="setupBuilder">Fluent interface parameter.</param>
        /// <param name="name">Name of the condition filter method</param>
        /// <param name="conditionMethod">Lambda method.</param>
        public static ISetupExtensionsBuilder RegisterConditionMethod(this ISetupExtensionsBuilder setupBuilder, string name, Func<LogEventInfo, object> conditionMethod)
        {
            if (conditionMethod == null)
                throw new ArgumentNullException(nameof(conditionMethod));
            ReflectionHelpers.LateBoundMethod lateBound = (target, args) => conditionMethod((LogEventInfo)args[0]);
            return RegisterConditionMethod(setupBuilder, name, conditionMethod.Method, lateBound);
        }

        /// <summary>
        /// Register a custom condition method, that can use in condition filters
        /// </summary>
        /// <param name="setupBuilder">Fluent interface parameter.</param>
        /// <param name="name">Name of the condition filter method</param>
        /// <param name="conditionMethod">Lambda method.</param>
        public static ISetupExtensionsBuilder RegisterConditionMethod(this ISetupExtensionsBuilder setupBuilder, string name, Func<object> conditionMethod)
        {
            if (conditionMethod == null)
                throw new ArgumentNullException(nameof(conditionMethod));
            ReflectionHelpers.LateBoundMethod lateBound = (target, args) => conditionMethod();
            return RegisterConditionMethod(setupBuilder, name, conditionMethod.Method, lateBound);
        }
#endif
    }
}
