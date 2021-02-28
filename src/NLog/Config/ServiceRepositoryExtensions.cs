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
    using JetBrains.Annotations;
    using NLog.Internal;
    using NLog.Targets;

    internal static class ServiceRepositoryExtensions
    {
        internal static ServiceRepository GetServiceProvider([CanBeNull] this LoggingConfiguration loggingConfiguration)
        {
            return loggingConfiguration?.LogFactory?.ServiceRepository ?? LogManager.LogFactory.ServiceRepository;
        }

        internal static T ResolveService<T>(this ServiceRepository serviceProvider, bool ignoreExternalProvider = true) where T : class
        {
            IServiceProvider externalServiceProvider;

            try
            {
                return serviceProvider.GetService<T>();
            }
            catch (NLogDependencyResolveException)
            {
                if (ignoreExternalProvider)
                    throw;

                externalServiceProvider = serviceProvider.GetService<IServiceProvider>();
                if (ReferenceEquals(externalServiceProvider, serviceProvider))
                {
                    throw;
                }
            }

            // External IServiceProvider can be dangerous to use from Logging-library and can lead to deadlock or stackoverflow
            // But during initialization of Logging-library then use of external IServiceProvider is probably safe
            var service = externalServiceProvider.GetService<T>();
            // Cache singleton so also available when logging-library has been fully initialized
            serviceProvider.RegisterService(typeof(T), service);
            return service;
        }

        internal static T GetService<T>(this IServiceProvider serviceProvider) where T : class
        {
            try
            {
                var service = (serviceProvider ?? LogManager.LogFactory.ServiceRepository).GetService(typeof(T)) as T;
                if (service != null)
                    return service;
                
                throw new NLogDependencyResolveException($"Service type {typeof(T)} cannot be resolved", typeof(T));
            }
            catch (NLogDependencyResolveException ex)
            {
                if (ex.ServiceType == typeof(T))
                    throw;

                throw new NLogDependencyResolveException(ex.Message, ex, typeof(T));
            }
            catch (Exception ex)
            {
                if (ex.MustBeRethrown())
                    throw;

                throw new NLogDependencyResolveException(ex.Message, ex, typeof(T));
            }
        }

        /// <summary>
        /// Registers singleton-object as implementation of specific interface.
        /// </summary>
        /// <remarks>
        /// If the same single-object implements multiple interfaces then it must be registered for each interface
        /// </remarks>
        /// <typeparam name="T">Type of interface</typeparam>
        /// <param name="serviceRepository">The repo</param>
        /// <param name="singleton">Singleton object to use for override</param>
        public static ServiceRepository RegisterSingleton<T>(this ServiceRepository serviceRepository, T singleton) where T : class
        {
            serviceRepository.RegisterService(typeof(T), singleton);
            return serviceRepository;
        }

        /// <summary>
        /// Registers the string serializer to use with <see cref="LogEventInfo.MessageTemplateParameters"/>
        /// </summary>
        public static ServiceRepository RegisterValueFormatter(this ServiceRepository serviceRepository, [NotNull] IValueFormatter valueFormatter)
        {
            if (valueFormatter == null)
            {
                throw new ArgumentNullException(nameof(valueFormatter));
            }

            serviceRepository.RegisterSingleton(valueFormatter);
            return serviceRepository;
        }

        public static ServiceRepository RegisterJsonConverter(this ServiceRepository serviceRepository, [NotNull] IJsonConverter jsonConverter)
        {
            if (jsonConverter == null)
            {
                throw new ArgumentNullException(nameof(jsonConverter));
            }

            serviceRepository.RegisterSingleton(jsonConverter);
            return serviceRepository;
        }

        public static ServiceRepository RegisterPropertyTypeConverter(this ServiceRepository serviceRepository, [NotNull] IPropertyTypeConverter converter)
        {
            if (converter == null)
            {
                throw new ArgumentNullException(nameof(converter));
            }

            serviceRepository.RegisterSingleton(converter);
            return serviceRepository;
        }

        public static ServiceRepository RegisterObjectTypeTransformer(this ServiceRepository serviceRepository, [NotNull] IObjectTypeTransformer transformer)
        {
            if (transformer == null)
            {
                throw new ArgumentNullException(nameof(transformer));
            }

            serviceRepository.RegisterSingleton(transformer);
            return serviceRepository;
        }

        public static ServiceRepository RegisterDefaults(this ServiceRepository serviceRepository)
        {
            serviceRepository.RegisterSingleton<IServiceProvider>(serviceRepository);
            serviceRepository.RegisterSingleton<ILogMessageFormatter>(new LogMessageTemplateFormatter(serviceRepository, false, false));
            serviceRepository.RegisterJsonConverter(new DefaultJsonSerializer(serviceRepository));
            serviceRepository.RegisterValueFormatter(new MessageTemplates.ValueFormatter(serviceRepository));
            serviceRepository.RegisterPropertyTypeConverter(PropertyTypeConverter.Instance);
            serviceRepository.RegisterObjectTypeTransformer(new ObjectReflectionCache(serviceRepository));
            return serviceRepository;
        }
    }
}
