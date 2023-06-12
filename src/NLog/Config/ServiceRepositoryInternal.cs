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
        private readonly Dictionary<Type, Func<object>> _creatorMap = new Dictionary<Type, Func<object>>();
        private readonly Dictionary<Type, CompiledConstructor> _lateBoundMap = new Dictionary<Type, CompiledConstructor>();
        private readonly object _lockObject = new object();
        public event EventHandler<ServiceRepositoryUpdateEventArgs> TypeRegistered;

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceRepositoryInternal"/> class.
        /// </summary>
        internal ServiceRepositoryInternal(bool resetGlobalCache = false)
        {
            if (resetGlobalCache)
                ConfigurationItemFactory.Default = null;    //build new global factory

            this.RegisterDefaults();
            // Maybe also include active TimeSource ? Could also be done with LogFactory extension-methods
        }

        public override void RegisterService(Type type, object instance)
        {
            Guard.ThrowIfNull(type);
            Guard.ThrowIfNull(instance);

            lock (_lockObject)
            {
                _creatorMap[type] = () => instance;
            }

            TypeRegistered?.Invoke(this, new ServiceRepositoryUpdateEventArgs(type));
        }

        public override object GetService(Type serviceType)
        {
            var serviceInstance = DefaultResolveInstance(serviceType, null);
            if (serviceInstance is null && serviceType.IsAbstract())
            {
                throw new NLogDependencyResolveException("Instance of class must be registered", serviceType);
            }
            return serviceInstance;
        }

        internal override bool TryGetService<T>(out T serviceInstance)
        {
            serviceInstance = DefaultResolveInstance(typeof(T), null) as T;
            return !(serviceInstance is null);
        }

        private object DefaultResolveInstance(Type itemType, HashSet<Type> seenTypes)
        {
            Guard.ThrowIfNull(itemType);

            Func<object> objectResolver = null;
            CompiledConstructor compiledConstructor = null;

            lock (_lockObject)
            {
                if (!_creatorMap.TryGetValue(itemType, out objectResolver))
                {
                    _lateBoundMap.TryGetValue(itemType, out compiledConstructor);
                }
            }

            if (objectResolver is null && compiledConstructor is null)
            {
                if (itemType.IsAbstract())
                {
                    if (seenTypes is null)
                        return null;
                    else
                        throw new NLogDependencyResolveException("Instance of class must be registered", itemType);
                }

                // Do not hold lock while resolving types to avoid deadlock on initialization of type static members
#pragma warning disable CS0618 // Type or member is obsolete
                var newCompiledConstructor = CreateCompiledConstructor(itemType);
#pragma warning restore CS0618 // Type or member is obsolete

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
            if (constructorParameters is null)
            {
                return objectResolver?.Invoke() ?? compiledConstructor?.Ctor(null);
            }
            else
            {
                seenTypes = seenTypes ?? new HashSet<Type>();
                var parameterValues = CreateCtorParameterValues(constructorParameters, seenTypes);
                return compiledConstructor?.Ctor(parameterValues);
            }
        }

        [Obsolete("Instead use NLog.LogManager.Setup().SetupExtensions(). Marked obsolete with NLog v5.2")]
        [System.Diagnostics.CodeAnalysis.UnconditionalSuppressMessage("Trimming - Ignore since obsolete", "IL2070")]
        private CompiledConstructor CreateCompiledConstructor(Type itemType)
        {
            try
            {
                InternalLogger.Debug("Object reflection needed to create instance of type: {0}", itemType);

                var defaultConstructor = itemType.GetConstructor(Type.EmptyTypes);
                if (defaultConstructor is null)
                {
                    InternalLogger.Trace("Resolves parameterized constructor for {0}", itemType);
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
                    var constructorMethod = ReflectionHelpers.CreateLateBoundConstructor(ctor);
                    return new CompiledConstructor(constructorMethod, ctor.GetParameters());
                }
                else
                {
                    InternalLogger.Trace("Resolves default constructor for {0}", itemType);
                    var constructorMethod = ReflectionHelpers.CreateLateBoundConstructor(defaultConstructor);
                    return new CompiledConstructor(constructorMethod);
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

        private sealed class CompiledConstructor
        {
            [NotNull] public ReflectionHelpers.LateBoundConstructor Ctor { get; }
            [CanBeNull] public ParameterInfo[] Parameters { get; }

            public CompiledConstructor([NotNull] ReflectionHelpers.LateBoundConstructor ctor, ParameterInfo[] parameters = null)
            {
                Ctor = Guard.ThrowIfNull(ctor);
                Parameters = parameters;
            }
        }
    }
}
