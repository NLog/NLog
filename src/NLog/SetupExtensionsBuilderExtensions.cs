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
        /// Disabled by default as it can give a huge performance hit during startup. Recommended to keep it disabled especially when running in the cloud.
        /// </remarks>
        [Obsolete("AutoLoadAssemblies(true) has been replaced by AutoLoadExtensions(), that matches the name of nlog-attribute in NLog.config. Marked obsolete on NLog 5.0")]
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
        /// <typeparam name="T">Type of the Target.</typeparam>
        /// <param name="setupBuilder">Fluent interface parameter.</param>
        /// <param name="overrideName">Type name of the Target. Will extract from class-attribute when unassigned.</param>
        public static ISetupExtensionsBuilder RegisterTarget<T>(this ISetupExtensionsBuilder setupBuilder, string overrideName = null) where T : Target
        {
            var layoutRendererType = typeof(T);
            return RegisterTarget(setupBuilder, layoutRendererType, overrideName);
        }

        /// <summary>
        /// Register a custom Target.
        /// </summary>
        /// <param name="setupBuilder">Fluent interface parameter.</param>
        /// <param name="targetType">Type of the Target.</param>
        /// <param name="overrideName">Type name of the Target. Will extract from class-attribute when unassigned.</param>
        public static ISetupExtensionsBuilder RegisterTarget(this ISetupExtensionsBuilder setupBuilder, Type targetType, string overrideName = null)
        {
            overrideName = string.IsNullOrEmpty(overrideName) ? targetType.GetFirstCustomAttribute<TargetAttribute>()?.Name : overrideName;
            if (string.IsNullOrEmpty(overrideName))
                throw new ArgumentException("Empty type name. Missing class attribute?", nameof(overrideName));
            setupBuilder.LogFactory.ServiceRepository.ConfigurationItemFactory.Targets.RegisterDefinition(overrideName, targetType);
            return setupBuilder;
        }

        /// <summary>
        /// Register a custom layout renderer.
        /// </summary>
        /// <typeparam name="T">Type of the layout renderer.</typeparam>
        /// <param name="setupBuilder">Fluent interface parameter.</param>
        /// <param name="overrideName">Symbol-name of the layout renderer - without ${}. Will extract from class-attribute when unassigned.</param>
        public static ISetupExtensionsBuilder RegisterLayoutRenderer<T>(this ISetupExtensionsBuilder setupBuilder, string overrideName = null)
            where T : LayoutRenderer
        {
            var layoutRendererType = typeof(T);
            return RegisterLayoutRenderer(setupBuilder, layoutRendererType, overrideName);
        }

        /// <summary>
        /// Register a custom layout renderer.
        /// </summary>
        /// <param name="setupBuilder">Fluent interface parameter.</param>
        /// <param name="layoutRendererType">Type of the layout renderer.</param>
        /// <param name="overrideName">Symbol-name of the layout renderer - without ${}. Will extract from class-attribute when unassigned.</param>
        public static ISetupExtensionsBuilder RegisterLayoutRenderer(this ISetupExtensionsBuilder setupBuilder, Type layoutRendererType, string overrideName = null)
        {
            overrideName = string.IsNullOrEmpty(overrideName) ? layoutRendererType.GetFirstCustomAttribute<LayoutRendererAttribute>()?.Name : overrideName;
            if (string.IsNullOrEmpty(overrideName))
                throw new ArgumentException("Empty type name. Missing class attribute?", nameof(overrideName));
            setupBuilder.LogFactory.ServiceRepository.ConfigurationItemFactory.LayoutRenderers.RegisterDefinition(overrideName, layoutRendererType);
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

        private static ISetupExtensionsBuilder RegisterConditionMethod(this ISetupExtensionsBuilder setupBuilder, string name, MethodInfo conditionMethod, ReflectionHelpers.LateBoundMethod lateBoundMethod)
        {
            setupBuilder.LogFactory.ServiceRepository.ConfigurationItemFactory.ConditionMethodDelegates.RegisterDefinition(name, conditionMethod, lateBoundMethod);
            return setupBuilder;
        }
#endif

        /// <summary>
        /// Register (or replaces) singleton-object for the specified service-type
        /// </summary>
        /// <typeparam name="T">Service interface type</typeparam>
        /// <param name="setupBuilder">Fluent interface parameter.</param>
        /// <param name="singletonService">Implementation of interface.</param>
        public static ISetupExtensionsBuilder RegisterSingletonService<T>(this ISetupExtensionsBuilder setupBuilder, T singletonService) where T : class
        {
            if (singletonService == null)
                throw new ArgumentNullException(nameof(singletonService));
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
            if (interfaceType == null)
                throw new ArgumentNullException(nameof(interfaceType));
            if (singletonService == null)
                throw new ArgumentNullException(nameof(singletonService));
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
            if (serviceProvider == null)
                throw new ArgumentNullException(nameof(serviceProvider));

            setupBuilder.LogFactory.ServiceRepository.RegisterSingleton(serviceProvider);
            return setupBuilder;
        }
    }
}
