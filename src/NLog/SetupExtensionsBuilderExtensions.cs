﻿// 
// Copyright (c) 2004-2021 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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
    using System.ComponentModel;
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
        /// Disabled by default as it can give a huge performance hit during startup. Recommended to keep it disabled especially when running in the cloud.
        /// </remarks>
        [Obsolete("AutoLoadAssemblies(true) has been replaced by AutoLoadExtensions(), that matches the name of nlog-attribute in NLog.config. Marked obsolete on NLog 5.0")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static ISetupExtensionsBuilder AutoLoadAssemblies(this ISetupExtensionsBuilder setupBuilder, bool enable)
        {
            if (enable)
                AutoLoadExtensions(setupBuilder);
            return setupBuilder;
        }

        /// <summary>
        /// Enable/disables autoloading of NLog extensions by scanning and loading available assemblies
        /// </summary>
        /// <remarks>
        /// Disabled by default as it can give a huge performance hit during startup. Recommended to keep it disabled especially when running in the cloud.
        /// </remarks>
        public static ISetupExtensionsBuilder AutoLoadExtensions(this ISetupExtensionsBuilder setupBuilder)
        {
            ConfigurationItemFactory.ScanForAutoLoadExtensions(setupBuilder.LogFactory);
            return setupBuilder;
        }

        /// <summary>
        /// Registers NLog extensions from the assembly.
        /// </summary>
        public static ISetupExtensionsBuilder RegisterAssembly(this ISetupExtensionsBuilder setupBuilder, Assembly assembly)
        {
            ConfigurationItemFactory.Default.RegisterItemsFromAssembly(assembly);
            return setupBuilder;
        }

        /// <summary>
        /// Registers NLog extensions from the assembly type name
        /// </summary>
        public static ISetupExtensionsBuilder RegisterAssembly(this ISetupExtensionsBuilder setupBuilder, string assemblyName)
        {
            Assembly assembly = AssemblyHelpers.LoadFromName(assemblyName);
            ConfigurationItemFactory.Default.RegisterItemsFromAssembly(assembly);
            return setupBuilder;
        }

        /// <summary>
        /// Register a custom NLog Target.
        /// </summary>
        /// <typeparam name="T">Type of the Target.</typeparam>
        /// <param name="setupBuilder">Fluent interface parameter.</param>
        /// <param name="name">The target type-alias for use in NLog configuration. Will extract from class-attribute when unassigned.</param>
        public static ISetupExtensionsBuilder RegisterTarget<T>(this ISetupExtensionsBuilder setupBuilder, string name = null) where T : Target
        {
            var targetType = typeof(T);
            name = string.IsNullOrEmpty(name) ? (targetType.GetFirstCustomAttribute<TargetAttribute>()?.Name ?? typeof(T).Name) : name;
            return RegisterTarget(setupBuilder, name, targetType);
        }

        /// <summary>
        /// Register a custom NLog Target.
        /// </summary>
        /// <param name="setupBuilder">Fluent interface parameter.</param>
        /// <param name="name">Type name of the Target</param>
        /// <param name="targetType">The target type-alias for use in NLog configuration</param>
        public static ISetupExtensionsBuilder RegisterTarget(this ISetupExtensionsBuilder setupBuilder, string name, Type targetType)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("Missing NLog Target type-alias", nameof(name));
            if (!typeof(Target).IsAssignableFrom(targetType))
                throw new ArgumentException("Not of type NLog Target", nameof(targetType));
            ConfigurationItemFactory.Default.Targets.RegisterDefinition(name, targetType);
            return setupBuilder;
        }

        /// <summary>
        /// Register a custom NLog Layout.
        /// </summary>
        /// <typeparam name="T">Type of the layout renderer.</typeparam>
        /// <param name="setupBuilder">Fluent interface parameter.</param>
        /// <param name="typeAlias">The layout type-alias for use in NLog configuration. Will extract from class-attribute when unassigned.</param>
        public static ISetupExtensionsBuilder RegisterLayout<T>(this ISetupExtensionsBuilder setupBuilder, string typeAlias = null)
            where T : Layout
        {
            var layoutRendererType = typeof(T);
            typeAlias = string.IsNullOrEmpty(typeAlias) ? (layoutRendererType.GetFirstCustomAttribute<LayoutAttribute>()?.Name ?? typeof(T).Name) : typeAlias;
            return RegisterLayout(setupBuilder, typeAlias, layoutRendererType);
        }

        /// <summary>
        /// Register a custom NLog Layout.
        /// </summary>
        /// <param name="setupBuilder">Fluent interface parameter.</param>
        /// <param name="layoutType">Type of the layout.</param>
        /// <param name="typeAlias">The layout type-alias for use in NLog configuration</param>
        public static ISetupExtensionsBuilder RegisterLayout(this ISetupExtensionsBuilder setupBuilder, string typeAlias, Type layoutType)
        {
            if (string.IsNullOrEmpty(typeAlias))
                throw new ArgumentException("Missing NLog Layout type-alias", nameof(typeAlias));
            if (!typeof(Layout).IsAssignableFrom(layoutType))
                throw new ArgumentException("Not of type NLog Layout", nameof(layoutType));
            ConfigurationItemFactory.Default.Layouts.RegisterDefinition(typeAlias, layoutType);
            return setupBuilder;
        }

        /// <summary>
        /// Register a custom NLog LayoutRenderer.
        /// </summary>
        /// <typeparam name="T">Type of the layout renderer.</typeparam>
        /// <param name="setupBuilder">Fluent interface parameter.</param>
        /// <param name="name">The layout-renderer type-alias for use in NLog configuration - without '${ }'. Will extract from class-attribute when unassigned.</param>
        public static ISetupExtensionsBuilder RegisterLayoutRenderer<T>(this ISetupExtensionsBuilder setupBuilder, string name = null)
            where T : LayoutRenderer
        {
            var layoutRendererType = typeof(T);
            name = string.IsNullOrEmpty(name) ? (layoutRendererType.GetFirstCustomAttribute<LayoutRendererAttribute>()?.Name ?? typeof(T).Name) : name;
            return RegisterLayoutRenderer(setupBuilder, name, layoutRendererType);
        }

        /// <summary>
        /// Register a custom NLog LayoutRenderer.
        /// </summary>
        /// <param name="setupBuilder">Fluent interface parameter.</param>
        /// <param name="layoutRendererType">Type of the layout renderer.</param>
        /// <param name="name">The layout-renderer type-alias for use in NLog configuration - without '${ }'</param>
        public static ISetupExtensionsBuilder RegisterLayoutRenderer(this ISetupExtensionsBuilder setupBuilder, string name, Type layoutRendererType)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("Missing NLog LayoutRenderer type-alias", nameof(name));
            if (!typeof(LayoutRenderer).IsAssignableFrom(layoutRendererType))
                throw new ArgumentException("Not of type NLog LayoutRenderer", nameof(layoutRendererType));
            ConfigurationItemFactory.Default.LayoutRenderers.RegisterDefinition(name, layoutRendererType);
            return setupBuilder;
        }

        /// <summary>
        /// Register a custom NLog LayoutRenderer with a callback function <paramref name="layoutMethod"/>. The callback receives the logEvent.
        /// </summary>
        /// <param name="setupBuilder">Fluent interface parameter.</param>
        /// <param name="name">The layout-renderer type-alias for use in NLog configuration - without '${ }'</param>
        /// <param name="layoutMethod">Callback that returns the value for the layout renderer.</param>
        public static ISetupExtensionsBuilder RegisterLayoutRenderer(this ISetupExtensionsBuilder setupBuilder, string name, Func<LogEventInfo, object> layoutMethod)
        {
            return RegisterLayoutRenderer(setupBuilder, name, (info, configuration) => layoutMethod(info));
        }

        /// <summary>
        /// Register a custom NLog LayoutRenderer with a callback function <paramref name="layoutMethod"/>. The callback receives the logEvent and the current configuration.
        /// </summary>
        /// <param name="setupBuilder">Fluent interface parameter.</param>
        /// <param name="name">The layout-renderer type-alias for use in NLog configuration - without '${ }'</param>
        /// <param name="layoutMethod">Callback that returns the value for the layout renderer.</param>
        public static ISetupExtensionsBuilder RegisterLayoutRenderer(this ISetupExtensionsBuilder setupBuilder, string name, Func<LogEventInfo, LoggingConfiguration, object> layoutMethod)
        {
            return RegisterLayoutRenderer(setupBuilder, name, layoutMethod, LayoutRenderOptions.None);
        }

        /// <summary>
        /// Register a custom NLog LayoutRenderer with a callback function <paramref name="layoutMethod"/>. The callback receives the logEvent.
        /// </summary>
        /// <param name="setupBuilder">Fluent interface parameter.</param>
        /// <param name="name">The layout-renderer type-alias for use in NLog configuration - without '${ }'</param>
        /// <param name="layoutMethod">Callback that returns the value for the layout renderer.</param>
        /// <param name="options">Options of the layout renderer.</param>
        public static ISetupExtensionsBuilder RegisterLayoutRenderer(this ISetupExtensionsBuilder setupBuilder, string name, Func<LogEventInfo, object> layoutMethod, LayoutRenderOptions options)
        {
            return RegisterLayoutRenderer(setupBuilder, name, (info, configuration) => layoutMethod(info), options);
        }

        /// <summary>
        /// Register a custom NLog LayoutRenderer with a callback function <paramref name="layoutMethod"/>. The callback receives the logEvent and the current configuration.
        /// </summary>
        /// <param name="setupBuilder">Fluent interface parameter.</param>
        /// <param name="name">The layout-renderer type-alias for use in NLog configuration - without '${ }'</param>
        /// <param name="layoutMethod">Callback that returns the value for the layout renderer.</param>
        /// <param name="options">Options of the layout renderer.</param>
        public static ISetupExtensionsBuilder RegisterLayoutRenderer(this ISetupExtensionsBuilder setupBuilder, string name, Func<LogEventInfo, LoggingConfiguration, object> layoutMethod, LayoutRenderOptions options)
        {
            FuncLayoutRenderer layoutRenderer = Layout.CreateFuncLayoutRenderer(layoutMethod, options, name);
            ConfigurationItemFactory.Default.GetLayoutRenderers().RegisterFuncLayout(name, layoutRenderer);
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
            ArgumentNullException.ThrowIfNull(conditionMethod);

            if (!conditionMethod.IsStatic)
                throw new ArgumentException($"{conditionMethod.Name} must be static", nameof(conditionMethod));

            ConfigurationItemFactory.Default.ConditionMethods.RegisterDefinition(name, conditionMethod);
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
            ArgumentNullException.ThrowIfNull(conditionMethod);
            ReflectionHelpers.LateBoundMethod lateBound = (target, args) => conditionMethod((LogEventInfo)args[0]);
            return RegisterConditionMethod(setupBuilder, name, conditionMethod, lateBound);
        }

        /// <summary>
        /// Register a custom condition method, that can use in condition filters
        /// </summary>
        /// <param name="setupBuilder">Fluent interface parameter.</param>
        /// <param name="name">Name of the condition filter method</param>
        /// <param name="conditionMethod">Lambda method.</param>
        public static ISetupExtensionsBuilder RegisterConditionMethod(this ISetupExtensionsBuilder setupBuilder, string name, Func<object> conditionMethod)
        {
            ArgumentNullException.ThrowIfNull(conditionMethod);
            
            ReflectionHelpers.LateBoundMethod lateBound = (target, args) => conditionMethod();
            return RegisterConditionMethod(setupBuilder, name, conditionMethod, lateBound);
        }

        private static ISetupExtensionsBuilder RegisterConditionMethod(this ISetupExtensionsBuilder setupBuilder, string name, Delegate conditionMethod, ReflectionHelpers.LateBoundMethod lateBoundMethod)
        {
            ConfigurationItemFactory.Default.ConditionMethodDelegates.RegisterDefinition(name, conditionMethod.GetDelegateInfo(), lateBoundMethod);
            return setupBuilder;
        }

        /// <summary>
        /// Register (or replaces) singleton-object for the specified service-type
        /// </summary>
        /// <typeparam name="T">Service interface type</typeparam>
        /// <param name="setupBuilder">Fluent interface parameter.</param>
        /// <param name="singletonService">Implementation of interface.</param>
        public static ISetupExtensionsBuilder RegisterSingletonService<T>(this ISetupExtensionsBuilder setupBuilder, T singletonService) where T : class
        {
            ArgumentNullException.ThrowIfNull(singletonService);

            setupBuilder.LogFactory.ServiceRepository.RegisterSingleton<T>(singletonService);
            return setupBuilder;
        }

        /// <summary>
        /// Register (or replaces) singleton-object for the specified service-type
        /// </summary>
        /// <param name="setupBuilder">Fluent interface parameter.</param>
        /// <param name="interfaceType">Service interface type.</param>
        /// <param name="singletonService">Implementation of interface.</param>
        public static ISetupExtensionsBuilder RegisterSingletonService(this ISetupExtensionsBuilder setupBuilder, Type interfaceType, object singletonService)
        {
            ArgumentNullException.ThrowIfNull(interfaceType);

            ArgumentNullException.ThrowIfNull(singletonService);
            
            if (!interfaceType.IsAssignableFrom(singletonService.GetType()))
                throw new ArgumentException("Service instance not matching type", nameof(singletonService));
            setupBuilder.LogFactory.ServiceRepository.RegisterService(interfaceType, singletonService);
            return setupBuilder;
        }

        /// <summary>
        /// Register (or replaces) external service-repository for resolving dependency injection
        /// </summary>
        /// <param name="setupBuilder">Fluent interface parameter.</param>
        /// <param name="serviceProvider">External dependency injection repository</param>
        public static ISetupExtensionsBuilder RegisterServiceProvider(this ISetupExtensionsBuilder setupBuilder, IServiceProvider serviceProvider)
        {
            ArgumentNullException.ThrowIfNull(serviceProvider);

            setupBuilder.LogFactory.ServiceRepository.RegisterSingleton(serviceProvider);
            return setupBuilder;
        }
    }
}
