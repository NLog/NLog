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

using JetBrains.Annotations;

namespace NLog.Config
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using NLog.Internal;
    using NLog.Common;

    /// <summary>
    /// Repository of interfaces used by NLog to allow override for dependency injection
    /// </summary>
    internal sealed class ServiceRepository : IServiceRepository
    {
        private readonly Dictionary<Type, ConfigurationItemCreator> _creatorMap = new Dictionary<Type, ConfigurationItemCreator>();
        private readonly Dictionary<Type, CompiledConstructor> _lateBoundMap = new Dictionary<Type, CompiledConstructor>();
        private readonly object _lockObject = new object();
        private ConfigurationItemFactory _localItemFactory;
        public event EventHandler<RepositoryUpdateEventArgs> TypeRegistered;

        public ConfigurationItemFactory ConfigurationItemFactory
        {
            get => _localItemFactory ?? (_localItemFactory = new ConfigurationItemFactory(this, ConfigurationItemFactory.Default, ArrayHelper.Empty<Assembly>()));
            set => _localItemFactory = value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceRepository"/> class.
        /// </summary>
        internal ServiceRepository(bool resetGlobalCache = false)
        {
            if (resetGlobalCache)
                ConfigurationItemFactory.Default = null;    //build new global factory

            this.RegisterDefaults();
            CreateInstance = DefaultResolveInstanceTop;
            // Maybe also include active TimeSource ? Could also be done with LogFactory extension-methods
        }

        public void RegisterType(Type type, [NotNull] ConfigurationItemCreator objectResolver)
        {
            _creatorMap[type] = objectResolver ?? throw new ArgumentNullException(nameof(objectResolver));
            TypeRegistered?.Invoke(this, new RepositoryUpdateEventArgs(type));
        }

        public object ResolveService(Type itemType)
        {
            var createInstance = CreateInstance;
            if (createInstance != null)
            {
                return createInstance(itemType);
            }

            return DefaultResolveInstanceTop(itemType);
        }

        private object DefaultResolveInstanceTop(Type itemType)
        {
            lock (_lockObject)
            {
                return DefaultResolveInstance(itemType, null);
            }
        }

        private object DefaultResolveInstance(Type itemType, HashSet<Type> seenTypes)
        {
            InternalLogger.Trace("Resolve {0}", itemType.FullName);
            if (_creatorMap.TryGetValue(itemType, out var objectResolver))
            {
                InternalLogger.Trace("Resolve {0} done", itemType.FullName);
                return objectResolver(itemType);
            }

            if (_lateBoundMap.TryGetValue(itemType, out var compiledConstructor))
            {
                return CreateNewInstance(compiledConstructor, seenTypes);
            }

            //todo? lock (_lateBoundMapLock)
            {
                if (_lateBoundMap.TryGetValue(itemType, out var compiledConstructor1))
                {
                    return CreateNewInstance(compiledConstructor1, seenTypes);
                }

                return CreateFromConstructor(itemType, seenTypes);
            }
        }

        private object CreateFromConstructor(Type itemType, HashSet<Type> seenTypes)
        {
            try
            {
                //todo DI find upfront
                var defaultConstructor = itemType.GetConstructor(Type.EmptyTypes);
                if (defaultConstructor != null)
                {
                    InternalLogger.Trace("Found public default ctor");

                    return CreateFromDefaultConstructor(itemType, defaultConstructor);
                }

                return CreateFromParameterizedConstructor(itemType, seenTypes);
            }
            catch (MissingMethodException exception)
            {
                throw new NLogResolveException("Is the required permission granted?", exception, itemType);
            }
            finally
            {
                InternalLogger.Trace("Resolve {0} done", itemType.FullName);
            }

        }

        private object CreateNewInstance(CompiledConstructor compiledConstructor, HashSet<Type> seenTypes)
        {
            var constructorParameters = compiledConstructor.Parameters;
            if (constructorParameters == null)
            {
                return compiledConstructor.Ctor(null);
            }

            seenTypes = seenTypes ?? new HashSet<Type>();
            var parameterValues = CreateCtorParameterValues(constructorParameters, seenTypes);
            return compiledConstructor.Ctor(parameterValues);
        }

        private object CreateFromDefaultConstructor(Type itemType, ConstructorInfo defaultConstructor)
        {
            var compiledCtor = ReflectionHelpers.CreateLateBoundConstructor(defaultConstructor);
            _lateBoundMap.Add(itemType, new CompiledConstructor(compiledCtor));
            return compiledCtor(null);
        }

        private object CreateFromParameterizedConstructor(Type itemType, HashSet<Type> seenTypes)
        {
            seenTypes = seenTypes ?? new HashSet<Type>();

            var ctor = GetParameterizedConstructor(itemType);
            var constructorParameters = ctor.GetParameters();
            var parameterValues = CreateCtorParameterValues(constructorParameters, seenTypes);
            var compiledParameterizedCtor = ReflectionHelpers.CreateLateBoundConstructor(ctor);
            _lateBoundMap.Add(itemType, new CompiledConstructor(compiledParameterizedCtor, constructorParameters));
            return compiledParameterizedCtor.Invoke(parameterValues);
        }

        private static ConstructorInfo GetParameterizedConstructor(Type itemType)
        {
            var ctors = itemType.GetConstructors();

            if (ctors.Length == 0)
            {
                throw new NLogResolveException("No public constructor", itemType);
            }

            if (ctors.Length > 1)
            {
                throw new NLogResolveException("Multiple public constructor are not supported if there isn't a default constructor'", itemType);
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
                    throw new NLogResolveException("There is a cycle", parameterType);
                }

                seenTypes.Add(parameterType);

                var paramValue = DefaultResolveInstance(parameterType, seenTypes);
                parameterValues[i] = paramValue;
            }

            return parameterValues;
        }

        /// <summary>
        /// Gets or sets the creator delegate used to instantiate configuration objects.
        /// </summary>
        /// <remarks>
        /// By overriding this property, one can enable dependency injection or interception for created objects.
        /// </remarks>
        public ConfigurationItemCreator CreateInstance { get; set; }

        private class CompiledConstructor
        {

            [NotNull] public ReflectionHelpers.LateBoundConstructor Ctor { get; }
            [CanBeNull] public ParameterInfo[] Parameters { get; }

            public bool ParameterLess => Parameters == null || Parameters.Length == 0;


            public CompiledConstructor([NotNull] ReflectionHelpers.LateBoundConstructor ctor, ParameterInfo[] parameters = null)
            {
                Ctor = ctor ?? throw new ArgumentNullException(nameof(ctor));
                Parameters = parameters;
            }
        }
    }
}
