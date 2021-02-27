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

namespace NLog.Config
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using JetBrains.Annotations;
    using NLog.Internal;
    using NLog.Common;

    /// <summary>
    /// Repository of interfaces used by NLog to allow override for dependency injection
    /// </summary>
    internal sealed class ServiceRepositoryInternal : ServiceRepository
    {
        private readonly Dictionary<Type, ConfigurationItemCreator> _creatorMap = new Dictionary<Type, ConfigurationItemCreator>();
        private readonly Dictionary<Type, CompiledConstructor> _lateBoundMap = new Dictionary<Type, CompiledConstructor>();
        private readonly object _lockObject = new object();
        private ConfigurationItemFactory _localItemFactory;
        public event EventHandler<ServiceRepositoryUpdateEventArgs> TypeRegistered;

        public override ConfigurationItemFactory ConfigurationItemFactory
        {
            get => _localItemFactory ?? (_localItemFactory = new ConfigurationItemFactory(this, ConfigurationItemFactory.Default, ArrayHelper.Empty<Assembly>()));
            internal set => _localItemFactory = value;
        }

        internal override ConfigurationItemCreator ConfigurationItemCreator { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceRepositoryInternal"/> class.
        /// </summary>
        internal ServiceRepositoryInternal(bool resetGlobalCache = false)
        {
            if (resetGlobalCache)
                ConfigurationItemFactory.Default = null;    //build new global factory

            ConfigurationItemCreator = GetService;

            this.RegisterDefaults();
            // Maybe also include active TimeSource ? Could also be done with LogFactory extension-methods
        }

        public override void RegisterService(Type type, object instance)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));
            if (instance == null)
                throw new ArgumentNullException(nameof(instance));

            lock (_lockObject)
            {
                _creatorMap[type] = new ConfigurationItemCreator(t => instance);
            }

            TypeRegistered?.Invoke(this, new ServiceRepositoryUpdateEventArgs(type));
        }

        public override object GetService(Type serviceType)
        {
            return DefaultResolveInstance(serviceType, null);
        }

        private object DefaultResolveInstance(Type itemType, HashSet<Type> seenTypes)
        {
            if (itemType == null)
                throw new ArgumentNullException(nameof(itemType));

            ConfigurationItemCreator objectResolver = null;
            CompiledConstructor compiledConstructor = null;

            lock (_lockObject)
            {
                if (!_creatorMap.TryGetValue(itemType, out objectResolver))
                {
                    _lateBoundMap.TryGetValue(itemType, out compiledConstructor);
                }
            }

            if (objectResolver == null && compiledConstructor == null)
            {
                if (itemType.IsAbstract())
                    throw new NLogDependencyResolveException("Instance of class must be registered", itemType);

                // Do not hold lock while resolving types to avoid deadlock on initialization of type static members
                var newCompiledConstructor = CreateCompiledConstructor(itemType);

                lock (_lockObject)
                {
                    if (!_lateBoundMap.TryGetValue(itemType, out compiledConstructor))
                    {
                        _lateBoundMap.Add(itemType, newCompiledConstructor);
                        compiledConstructor = newCompiledConstructor;
                    }
                }
            }

            // Do not hold lock while calling constructor (or resolving parameter values) to avoid deadlock
            var constructorParameters = compiledConstructor?.Parameters;
            if (constructorParameters != null)
            {
                seenTypes = seenTypes ?? new HashSet<Type>();
                var parameterValues = CreateCtorParameterValues(constructorParameters, seenTypes);
                return compiledConstructor.Ctor(parameterValues);
            }

            return objectResolver?.Invoke(itemType) ?? compiledConstructor?.Ctor(null);
        }

        private CompiledConstructor CreateCompiledConstructor(Type itemType)
        {
            try
            {
                var defaultConstructor = itemType.GetConstructor(Type.EmptyTypes);
                if (defaultConstructor != null)
                {
                    InternalLogger.Trace("Resolves default constructor for {0}", itemType);
                    var constructorMethod = ReflectionHelpers.CreateLateBoundConstructor(defaultConstructor);
                    return new CompiledConstructor(constructorMethod);
                }
                else
                {
                    InternalLogger.Trace("Resolves parameterized constructor for {0}", itemType);
                    var ctor = GetParameterizedConstructor(itemType);
                    var constructorMethod = ReflectionHelpers.CreateLateBoundConstructor(ctor);
                    return new CompiledConstructor(constructorMethod, ctor.GetParameters());
                }
            }
            catch (MissingMethodException exception)
            {
                throw new NLogDependencyResolveException("Is the required permission granted?", exception, itemType);
            }
            finally
            {
                InternalLogger.Trace("Resolve {0} done", itemType.FullName);
            }
        }

        private static ConstructorInfo GetParameterizedConstructor(Type itemType)
        {
            var ctors = itemType.GetConstructors();

            if (ctors.Length == 0)
            {
                throw new NLogDependencyResolveException("No public constructor", itemType);
            }

            if (ctors.Length > 1)
            {
                throw new NLogDependencyResolveException("Multiple public constructor are not supported if there isn't a default constructor'", itemType);
            }

            var ctor = ctors[0];
            return ctor;
        }

        private object[] CreateCtorParameterValues(ParameterInfo[] parameterInfos, HashSet<Type> seenTypes)
        {
            var parameterValues = new object[parameterInfos.Length];

            for (var i = 0; i < parameterInfos.Length; i++)
            {
                var param = parameterInfos[i];

                var parameterType = param.ParameterType;
                if (seenTypes.Contains(parameterType))
                {
                    throw new NLogDependencyResolveException("There is a cycle", parameterType);
                }

                seenTypes.Add(parameterType);

                var paramValue = DefaultResolveInstance(parameterType, seenTypes);
                parameterValues[i] = paramValue;
            }

            return parameterValues;
        }

        private class CompiledConstructor
        {
            [NotNull] public ReflectionHelpers.LateBoundConstructor Ctor { get; }
            [CanBeNull] public ParameterInfo[] Parameters { get; }

            public CompiledConstructor([NotNull] ReflectionHelpers.LateBoundConstructor ctor, ParameterInfo[] parameters = null)
            {
                Ctor = ctor ?? throw new ArgumentNullException(nameof(ctor));
                Parameters = parameters;
            }
        }
    }
}
