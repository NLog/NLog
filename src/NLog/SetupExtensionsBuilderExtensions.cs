// 
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
    using System.Diagnostics.CodeAnalysis;

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
            ConfigurationItemFactory.Default.AssemblyLoader.ScanForAutoLoadExtensions(ConfigurationItemFactory.Default);
            return setupBuilder;
        }

        /// <summary>
        /// Registers NLog extensions from the assembly.
        /// </summary>
        public static ISetupExtensionsBuilder RegisterAssembly(this ISetupExtensionsBuilder setupBuilder, Assembly assembly)
        {
#pragma warning disable CS0618 // Type or member is obsolete
            ConfigurationItemFactory.Default.RegisterItemsFromAssembly(assembly);
#pragma warning restore CS0618 // Type or member is obsolete
            return setupBuilder;
        }

        /// <summary>
        /// Registers NLog extensions from the assembly type name
        /// </summary>
        public static ISetupExtensionsBuilder RegisterAssembly(this ISetupExtensionsBuilder setupBuilder, string assemblyName)
        {
            ConfigurationItemFactory.Default.AssemblyLoader.LoadAssemblyFromName(ConfigurationItemFactory.Default, assemblyName, string.Empty);
            return setupBuilder;
        }

        /// <summary>
        /// Register a custom NLog Configuration Type.
        /// </summary>
        /// <typeparam name="T">Type of the NLog configuration item</typeparam>
        /// <param name="setupBuilder">Fluent interface parameter.</param>
        public static ISetupExtensionsBuilder RegisterType<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor | DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.PublicMethods)] T>(this ISetupExtensionsBuilder setupBuilder)
            where T : class, new()
        {
            ConfigurationItemFactory.Default.RegisterType<T>();
            return setupBuilder;
        }

        /// <summary>
        /// Register a custom NLog Target.
        /// </summary>
        /// <typeparam name="T">Type of the Target.</typeparam>
        /// <param name="setupBuilder">Fluent interface parameter.</param>
        /// <param name="name">The target type-alias for use in NLog configuration. Will extract from class-attribute when unassigned.</param>
        public static ISetupExtensionsBuilder RegisterTarget<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor | DynamicallyAccessedMemberTypes.PublicProperties)] T>(this ISetupExtensionsBuilder setupBuilder, string name = null)
            where T : Target, new()
        {
            return RegisterTarget<T>(setupBuilder, () => new T(), name);
        }

        /// <summary>
        /// Register a custom NLog Target.
        /// </summary>
        /// <typeparam name="T">Type of the Target.</typeparam>
        /// <param name="setupBuilder">Fluent interface parameter.</param>
        /// <param name="factory">The factory method for creating instance of NLog Target</param>
        /// <param name="typeAlias">The target type-alias for use in NLog configuration. Will extract from class-attribute when unassigned.</param>
        public static ISetupExtensionsBuilder RegisterTarget<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor | DynamicallyAccessedMemberTypes.PublicProperties)] T>(this ISetupExtensionsBuilder setupBuilder, Func<T> factory, string typeAlias = null)
            where T : Target
        {
            typeAlias = string.IsNullOrEmpty(typeAlias) ? typeof(T).GetFirstCustomAttribute<TargetAttribute>()?.Name : typeAlias;
            if (string.IsNullOrEmpty(typeAlias))
            {
                typeAlias = ResolveTypeAlias<T>("TargetWrapper", "Target");
                if (typeof(NLog.Targets.Wrappers.WrapperTargetBase).IsAssignableFrom(typeof(T)) && !typeAlias.EndsWith("Wrapper", StringComparison.OrdinalIgnoreCase))
                {
                    typeAlias += "Wrapper";
                }
            }
            ConfigurationItemFactory.Default.GetTargetFactory().RegisterType<T>(typeAlias, factory);
            return setupBuilder;
        }

        /// <summary>
        /// Register a custom NLog Target.
        /// </summary>
        /// <param name="setupBuilder">Fluent interface parameter.</param>
        /// <param name="name">Type name of the Target</param>
        /// <param name="targetType">The target type-alias for use in NLog configuration</param>
        public static ISetupExtensionsBuilder RegisterTarget(this ISetupExtensionsBuilder setupBuilder, string name, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor | DynamicallyAccessedMemberTypes.PublicProperties)] Type targetType)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("Missing NLog Target type-alias", nameof(name));
            if (!typeof(Target).IsAssignableFrom(targetType))
                throw new ArgumentException("Not of type NLog Target", nameof(targetType));

            ConfigurationItemFactory.Default.GetTargetFactory().RegisterType(name, () => (Target)Activator.CreateInstance(targetType));
            return setupBuilder;
        }

        /// <summary>
        /// Register a custom NLog Layout.
        /// </summary>
        /// <typeparam name="T">Type of the layout renderer.</typeparam>
        /// <param name="setupBuilder">Fluent interface parameter.</param>
        /// <param name="typeAlias">The layout type-alias for use in NLog configuration. Will extract from class-attribute when unassigned.</param>
        public static ISetupExtensionsBuilder RegisterLayout<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor | DynamicallyAccessedMemberTypes.PublicProperties)] T>(this ISetupExtensionsBuilder setupBuilder, string typeAlias = null)
            where T : Layout, new()
        {
            return RegisterLayout<T>(setupBuilder, () => new T(), typeAlias);
        }

        /// <summary>
        /// Register a custom NLog Layout.
        /// </summary>
        /// <typeparam name="T">Type of the layout renderer.</typeparam>
        /// <param name="setupBuilder">Fluent interface parameter.</param>
        /// <param name="factory">The factory method for creating instance of NLog Layout</param>
        /// <param name="typeAlias">The layout type-alias for use in NLog configuration. Will extract from class-attribute when unassigned.</param>
        public static ISetupExtensionsBuilder RegisterLayout<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor | DynamicallyAccessedMemberTypes.PublicProperties)] T>(this ISetupExtensionsBuilder setupBuilder, Func<T> factory, string typeAlias = null)
            where T : Layout
        {
            typeAlias = string.IsNullOrEmpty(typeAlias) ? ResolveTypeAlias<T, LayoutAttribute>(ArrayHelper.Empty<string>()) : typeAlias;
            ConfigurationItemFactory.Default.GetLayoutFactory().RegisterType<T>(typeAlias, factory);
            return setupBuilder;
        }

        /// <summary>
        /// Register a custom NLog Layout.
        /// </summary>
        /// <param name="setupBuilder">Fluent interface parameter.</param>
        /// <param name="layoutType">Type of the layout.</param>
        /// <param name="typeAlias">The layout type-alias for use in NLog configuration</param>
        public static ISetupExtensionsBuilder RegisterLayout(this ISetupExtensionsBuilder setupBuilder, string typeAlias, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor | DynamicallyAccessedMemberTypes.PublicProperties)] Type layoutType)
        {
            if (string.IsNullOrEmpty(typeAlias))
                throw new ArgumentException("Missing NLog Layout type-alias", nameof(typeAlias));
            if (!typeof(Layout).IsAssignableFrom(layoutType))
                throw new ArgumentException("Not of type NLog Layout", nameof(layoutType));

            ConfigurationItemFactory.Default.GetLayoutFactory().RegisterType(typeAlias, () => (Layout)Activator.CreateInstance(layoutType));
            return setupBuilder;
        }

        /// <summary>
        /// Register a custom NLog LayoutRenderer.
        /// </summary>
        /// <typeparam name="T">Type of the layout renderer.</typeparam>
        /// <param name="setupBuilder">Fluent interface parameter.</param>
        /// <param name="name">The layout-renderer type-alias for use in NLog configuration - without '${ }'. Will extract from class-attribute when unassigned.</param>
        public static ISetupExtensionsBuilder RegisterLayoutRenderer<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor | DynamicallyAccessedMemberTypes.PublicProperties)] T>(this ISetupExtensionsBuilder setupBuilder, string name = null)
            where T : LayoutRenderer, new()
        {
            return RegisterLayoutRenderer<T>(setupBuilder, () => new T(), name);
        }

        /// <summary>
        /// Register a custom NLog LayoutRenderer.
        /// </summary>
        /// <typeparam name="T">Type of the layout renderer.</typeparam>
        /// <param name="setupBuilder">Fluent interface parameter.</param>
        /// <param name="factory">The factory method for creating instance of NLog LayoutRenderer</param>
        /// <param name="typeAlias">The layout-renderer type-alias for use in NLog configuration - without '${ }'. Will extract from class-attribute when unassigned.</param>
        public static ISetupExtensionsBuilder RegisterLayoutRenderer<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor | DynamicallyAccessedMemberTypes.PublicProperties)] T>(this ISetupExtensionsBuilder setupBuilder, Func<T> factory, string typeAlias = null)
            where T : LayoutRenderer
        {
            typeAlias = string.IsNullOrEmpty(typeAlias) ? ResolveTypeAlias<T, LayoutRendererAttribute>("LayoutRendererWrapper", "LayoutRenderer") : typeAlias;
            ConfigurationItemFactory.Default.GetLayoutRendererFactory().RegisterType<T>(typeAlias, factory);
            return setupBuilder;
        }

        private static string ResolveTypeAlias<T, TNameAttribute>(params string[] trimEndings) where TNameAttribute : NameBaseAttribute
        {
            var typeAlias = typeof(T).GetFirstCustomAttribute<TNameAttribute>()?.Name;
            if (!string.IsNullOrEmpty(typeAlias))
                return typeAlias;

            return ResolveTypeAlias<T>(trimEndings);
        }

        private static string ResolveTypeAlias<T>(params string[] trimEndings)
        {
            var typeAlias = typeof(T).Name;

            foreach (var ending in trimEndings)
            {
                int endingPosition = typeAlias.IndexOf(ending, StringComparison.OrdinalIgnoreCase);
                if (endingPosition > 0)
                {
                    return typeAlias.Substring(endingPosition);
                }
            }

            return typeAlias;
        }

        /// <summary>
        /// Register a custom NLog LayoutRenderer.
        /// </summary>
        /// <param name="setupBuilder">Fluent interface parameter.</param>
        /// <param name="layoutRendererType">Type of the layout renderer.</param>
        /// <param name="name">The layout-renderer type-alias for use in NLog configuration - without '${ }'</param>
        public static ISetupExtensionsBuilder RegisterLayoutRenderer(this ISetupExtensionsBuilder setupBuilder, string name, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor | DynamicallyAccessedMemberTypes.PublicProperties)] Type layoutRendererType)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("Missing NLog LayoutRenderer type-alias", nameof(name));
            if (!typeof(LayoutRenderer).IsAssignableFrom(layoutRendererType))
                throw new ArgumentException("Not of type NLog LayoutRenderer", nameof(layoutRendererType));

            ConfigurationItemFactory.Default.GetLayoutRendererFactory().RegisterType(name, () => (LayoutRenderer)Activator.CreateInstance(layoutRendererType));
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
            return setupBuilder.RegisterLayoutRenderer(layoutRenderer);
        }

        /// <summary>
        /// Register a custom NLog LayoutRenderer with a callback function
        /// </summary>
        /// <param name="setupBuilder">Fluent interface parameter.</param>
        /// <param name="layoutRenderer">LayoutRenderer instance with type-alias and callback-method.</param>
        public static ISetupExtensionsBuilder RegisterLayoutRenderer(this ISetupExtensionsBuilder setupBuilder, FuncLayoutRenderer layoutRenderer)
        {
            ConfigurationItemFactory.Default.GetLayoutRendererFactory().RegisterFuncLayout(layoutRenderer.LayoutRendererName, layoutRenderer);
            return setupBuilder;
        }

        /// <summary>
        /// Register a custom condition method, that can use in condition filters
        /// </summary>
        /// <param name="setupBuilder">Fluent interface parameter.</param>
        /// <param name="name">Name of the condition filter method</param>
        /// <param name="conditionMethod">MethodInfo extracted by reflection - typeof(MyClass).GetMethod("MyFunc", BindingFlags.Static).</param>
        [Obsolete("Instead use RegisterConditionMethod with delegate, as type reflection will be moved out. Marked obsolete with NLog v5.2")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static ISetupExtensionsBuilder RegisterConditionMethod(this ISetupExtensionsBuilder setupBuilder, string name, MethodInfo conditionMethod)
        {
            Guard.ThrowIfNull(conditionMethod);
            if (!conditionMethod.IsStatic)
                throw new ArgumentException($"{conditionMethod.Name} must be static", nameof(conditionMethod));

            ConfigurationItemFactory.Default.ConditionMethodFactory.RegisterDefinition(name, conditionMethod);
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
            Guard.ThrowIfNull(conditionMethod);
            ConfigurationItemFactory.Default.ConditionMethodFactory.RegisterNoParameters(name, (logEvent) => conditionMethod(logEvent));
            return setupBuilder;
        }

        /// <summary>
        /// Register a custom condition method, that can use in condition filters
        /// </summary>
        /// <param name="setupBuilder">Fluent interface parameter.</param>
        /// <param name="name">Name of the condition filter method</param>
        /// <param name="conditionMethod">Lambda method.</param>
        public static ISetupExtensionsBuilder RegisterConditionMethod(this ISetupExtensionsBuilder setupBuilder, string name, Func<object> conditionMethod)
        {
            Guard.ThrowIfNull(conditionMethod);
            ConfigurationItemFactory.Default.ConditionMethodFactory.RegisterNoParameters(name, (logEvent) => conditionMethod());
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
            Guard.ThrowIfNull(singletonService);
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
            Guard.ThrowIfNull(interfaceType);
            Guard.ThrowIfNull(singletonService);

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
            Guard.ThrowIfNull(serviceProvider);

            setupBuilder.LogFactory.ServiceRepository.RegisterSingleton(serviceProvider);
            return setupBuilder;
        }
    }
}
