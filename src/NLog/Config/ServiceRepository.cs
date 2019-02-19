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

namespace NLog.Config
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using NLog.Internal;
    using NLog.LayoutRenderers;
    using NLog.Layouts;
    using NLog.Targets;

    /// <summary>
    /// Repository of intefaces used by NLog to allow override for dependency injection
    /// </summary>
    public sealed class ServiceRepository
    {
        private readonly Dictionary<Type, object> _serviceRepository = new Dictionary<Type, object>();
        private ConfigurationItemFactory _localItemFactory;

        internal ConfigurationItemFactory ConfigurationItemFactory
        {
            get => _localItemFactory ?? (_localItemFactory = new ConfigurationItemFactory(this, ConfigurationItemFactory.Default, ArrayHelper.Empty<Assembly>()));
            set => _localItemFactory = null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceRepository"/> class.
        /// </summary>
        internal ServiceRepository()
        {
            RegisterService(typeof(IJsonConverter), DefaultJsonSerializer.Instance);
            RegisterService(typeof(IValueFormatter), new MessageTemplates.ValueFormatter(this));
            RegisterService(typeof(IPropertyTypeConverter), NLog.Config.PropertyTypeConverter.Instance);
            RegisterService(typeof(IConfigItemCreator), NLog.Config.ConfigItemCreator.Instance);
            // Maybe also include active TimeSource ? Could also be property on LogFactory
        }

        /// <summary>
        /// Registers singleton-object as implementation of specific interface.
        /// </summary>
        /// <remarks>
        /// If the same single-object implements multiple interfaces then it must be registered for each interface
        /// </remarks>
        /// <param name="interfaceType">Type of interface</param>
        /// <param name="serviceInstance">Singleton object to use for override</param>
        public void RegisterService(Type interfaceType, object serviceInstance)
        {
            if (!(interfaceType?.IsInterface() ?? false))
            {
                throw new ArgumentException($"{interfaceType} must be interface", nameof(interfaceType));
            }
            if (serviceInstance == null || !interfaceType.IsAssignableFrom(serviceInstance.GetType()))
            {
                throw new ArgumentException($"{serviceInstance} must be of type interface {interfaceType}", nameof(serviceInstance));
            }
            _serviceRepository[interfaceType] = serviceInstance;
        }

        /// <summary>
        /// Lookup of singleton object that implements the specific interface 
        /// </summary>
        /// <param name="interfaceType">Type of interface</param>
        /// <param name="serviceInstance">Singleton object found</param>
        /// <returns>Lookup was succesful</returns>
        public bool TryGetService(Type interfaceType, out object serviceInstance)
        {
            return _serviceRepository.TryGetValue(interfaceType, out serviceInstance);
        }

        private T TryGetService<T>() where T : class
        {
            if (TryGetService(typeof(T), out var serviceInstance))
            {
                return serviceInstance as T;
            }
            return null;
        }

        /// <summary>
        /// Register a custom layout renderer.
        /// </summary>
        /// <param name="layoutRendererType"> Type of the layout renderer.</param>
        /// <param name="name"> Name of the layout renderer - without ${}.</param>
        public void RegisterLayoutRenderer(string name, Type layoutRendererType)
        {
            ConfigurationItemFactory.LayoutRenderers.RegisterDefinition(name, layoutRendererType);
        }

        /// <summary>
        /// Register a custom layout renderer with a callback function <paramref name="func"/>. The callback recieves the logEvent.
        /// </summary>
        /// <param name="name">Name of the layout renderer - without ${}.</param>
        /// <param name="func">Callback that returns the value for the layout renderer.</param>
        public void RegisterLayoutRenderer(string name, Func<LogEventInfo, object> func)
        {
            RegisterLayoutRenderer(name, (info, configuration) => func(info));
        }

        /// <summary>
        /// Register a custom layout renderer with a callback function <paramref name="func"/>. The callback recieves the logEvent and the current configuration.
        /// </summary>
        /// <param name="name">Name of the layout renderer - without ${}.</param>
        /// <param name="func">Callback that returns the value for the layout renderer.</param>
        public void RegisterLayoutRenderer(string name, Func<LogEventInfo, LoggingConfiguration, object> func)
        {
            var layoutRenderer = new FuncLayoutRenderer(name, func);
            ConfigurationItemFactory.GetLayoutRenderers().RegisterFuncLayout(name, layoutRenderer);
        }

        /// <summary>
        /// Register a custom Target.
        /// </summary>
        /// <param name="targetType"> Type of the Target.</param>
        /// <param name="name"> Name of the Target.</param>
        public void RegisterTarget(string name, Type targetType)
        {
            ConfigurationItemFactory.Targets.RegisterDefinition(name, targetType);
        }

        /// <summary>
        /// Gets or sets the JSON serializer to use with <see cref="WebServiceTarget"/> or <see cref="JsonLayout"/>
        /// </summary>
        public IJsonConverter JsonConverter
        {
            get => TryGetService<IJsonConverter>() ?? DefaultJsonSerializer.Instance;
            set => RegisterService(typeof(IJsonConverter), value ?? DefaultJsonSerializer.Instance);
        }

        /// <summary>
        /// Gets or sets the string serializer to use with <see cref="LogEventInfo.MessageTemplateParameters"/>
        /// </summary>
        public IValueFormatter ValueFormatter
        {
            get => TryGetService<IValueFormatter>() ?? MessageTemplates.ValueFormatter.Instance;
            set
            {
                RegisterService(typeof(IValueFormatter), value ?? new MessageTemplates.ValueFormatter(this));
                if (ReferenceEquals(this, LogManager.LogFactory.ServiceRepository))
                {
                    // Because of performance-reasons (and maybe bad design). See also static LogEventInfo.DefaultMessageFormatter
                    MessageTemplates.ValueFormatter.Instance = value ?? new MessageTemplates.ValueFormatter(this);
                }
            }
        }

        /// <summary>
        /// Gets or sets the parameter converter to use with <see cref="DatabaseTarget"/>, <see cref="WebServiceTarget"/> or <see cref="TargetWithContext"/>
        /// </summary>
        public IPropertyTypeConverter PropertyTypeConverter
        {
            get => TryGetService<IPropertyTypeConverter>() ?? NLog.Config.PropertyTypeConverter.Instance;
            set => RegisterService(typeof(IPropertyTypeConverter), value ?? NLog.Config.PropertyTypeConverter.Instance);
        }

        /// <summary>
        /// Gets or sets the interface to instantiate configuration objects (target, layout, layout renderer, etc.)
        /// </summary>
        public IConfigItemCreator ConfigItemCreator
        {
            get => TryGetService<IConfigItemCreator>() ?? NLog.Config.ConfigItemCreator.Instance;
            set => RegisterService(typeof(IConfigItemCreator), value ?? NLog.Config.ConfigItemCreator.Instance);
        }

        /// <summary>
        /// Gets or sets the creator delegate used to instantiate configuration objects.
        /// </summary>
        /// <remarks>
        /// By overriding this property, one can enable dependency injection or interception for created objects.
        /// </remarks>
        public ConfigurationItemCreator CreateInstance
        {
            get => (ConfigItemCreator as ConfigItemCreator)?.CreateInstanceMethod ?? NLog.Config.ConfigItemCreator.Instance.CreateInstanceMethod;
            set => ConfigItemCreator = new ConfigItemCreator(value);
        }

        /// <summary>
        /// Perform mesage template parsing and formatting of LogEvent messages (True = Always, False = Never, Null = Auto Detect)
        /// </summary>
        /// <remarks>
        /// - Null (Auto Detect) : NLog-parser checks <see cref="LogEventInfo.Message"/> for positional parameters, and will then fallback to string.Format-rendering.
        /// - True: Always performs the parsing of <see cref="LogEventInfo.Message"/> and rendering of <see cref="LogEventInfo.FormattedMessage"/> using the NLog-parser (Allows custom formatting with <see cref="ValueFormatter"/>)
        /// - False: Always performs parsing and rendering using string.Format (Fastest if not using structured logging)
        /// </remarks>
        public bool? ParseMessageTemplates
        {
            get
            {
                if (ReferenceEquals(LogEventInfo.DefaultMessageFormatter, LogEventInfo.StringFormatMessageFormatter))
                {
                    return false;
                }
                else if (ReferenceEquals(LogEventInfo.DefaultMessageFormatter, LogMessageTemplateFormatter.Default.MessageFormatter))
                {
                    return true;
                }
                else
                {
                    return null;
                }
            }
            set => LogEventInfo.SetDefaultMessageFormatter(value);
        }
    }
}
