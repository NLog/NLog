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

using NLog.Common;

namespace NLog.Config
{
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using System.Reflection;
    using NLog.Internal;
    using NLog.Targets;

    /// <summary>
    /// Repository of interfaces used by NLog to allow override for dependency injection
    /// </summary>
    internal sealed class ServiceRepository : IServiceRepository
    {
        private readonly Dictionary<Type, ConfigurationItemCreator> _serviceRepository = new Dictionary<Type, ConfigurationItemCreator>();
        private ConfigurationItemFactory _localItemFactory;

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
            CreateInstance = itemType => DefaultResolveInstance(itemType, new HashSet<Type>());
            // Maybe also include active TimeSource ? Could also be done with LogFactory extension-methods
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="objectResolver"><c>null</c> will unregister the type</param>
        public void RegisterType(Type type, ConfigurationItemCreator objectResolver)
        {
            if (objectResolver == null)
                _serviceRepository.Remove(type);
            else
                _serviceRepository[type] = objectResolver;
        }

        public object ResolveService(Type itemType)
        {
            var createInstance = CreateInstance;
            if (createInstance != null)
            {
                return createInstance(itemType);
            }

            return DefaultResolveInstance(itemType, new HashSet<Type>());
        }

        private object DefaultResolveInstance(Type itemType, HashSet<Type> seenTypes)
        {
            InternalLogger.Trace("Resolve {0}", itemType.FullName);
            if (_serviceRepository.TryGetValue(itemType, out var objectResolver))
            {
                InternalLogger.Trace("Resolve {0} done", itemType.FullName);
                return objectResolver(itemType);
            }

            try
            {
                //todo DI cache/find upfront
                var defaultConstructor = itemType.GetConstructor(Type.EmptyTypes);
                if (defaultConstructor != null)
                {
                    InternalLogger.Trace("Found public default ctor");
                    return defaultConstructor.Invoke(ArrayHelper.Empty<object>());
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

        private object CreateFromParameterizedConstructor(Type itemType, HashSet<Type> seenTypes)
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

            var ctor = ctors.First();

            var parameterInfos = ctor.GetParameters();

            if (parameterInfos.Length == 0)
            {
                ctor.Invoke(ArrayHelper.Empty<object>());
            }

            var parameterValues = CreateCtorParameterValues(parameterInfos, seenTypes);

            return ctor.Invoke(parameterValues);
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
    }
}
