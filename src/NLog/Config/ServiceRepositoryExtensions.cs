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
            if (ignoreExternalProvider)
            {
                return serviceProvider.GetService<T>();
            }
            else
            {
                IServiceProvider externalServiceProvider;

                try
                {
                    if (serviceProvider.TryGetService<T>(out var service))
                    {
                        return service;
                    }
                    
                    externalServiceProvider = serviceProvider.GetService<IServiceProvider>();
                }
                catch (NLogDependencyResolveException)
                {
                    externalServiceProvider = serviceProvider.GetService<IServiceProvider>();
                    if (ReferenceEquals(externalServiceProvider, serviceProvider))
                    {
                        throw;
                    }
                }
                catch (Exception ex)
                {
                    if (ex.MustBeRethrown())
                        throw;

                    throw new NLogDependencyResolveException(ex.Message, ex, typeof(T));
                }

                if (ReferenceEquals(externalServiceProvider, serviceProvider))
                {
                    throw new NLogDependencyResolveException("Instance of class must be registered", typeof(T));
                }

                // External IServiceProvider can be dangerous to use from Logging-library and can lead to deadlock or stackoverflow
                // But during initialization of Logging-library then use of external IServiceProvider is probably safe
                var externalService = externalServiceProvider.GetService<T>();
                // Cache singleton so also available when logging-library has been fully initialized
                serviceProvider.RegisterService(typeof(T), externalService);
                return externalService;
            }
        }

        internal static T GetService<T>(this IServiceProvider serviceProvider) where T : class
        {
            try
            {
                var service = (serviceProvider ?? LogManager.LogFactory.ServiceRepository).GetService(typeof(T)) as T;
                if (service is null)
                    throw new NLogDependencyResolveException("Instance of class is unavailable", typeof(T));

                return service;
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
        internal static ServiceRepository RegisterSingleton<T>(this ServiceRepository serviceRepository, T singleton) where T : class
        {
            serviceRepository.RegisterService(typeof(T), singleton);
            return serviceRepository;
        }

        /// <summary>
        /// Registers the string serializer to use with <see cref="LogEventInfo.MessageTemplateParameters"/>
        /// </summary>
        internal static ServiceRepository RegisterValueFormatter(this ServiceRepository serviceRepository, [NotNull] IValueFormatter valueFormatter)
        {
            Guard.ThrowIfNull(valueFormatter);

            serviceRepository.RegisterSingleton(valueFormatter);
            return serviceRepository;
        }

        internal static ServiceRepository RegisterJsonConverter(this ServiceRepository serviceRepository, [NotNull] IJsonConverter jsonConverter)
        {
            Guard.ThrowIfNull(jsonConverter);

            serviceRepository.RegisterSingleton(jsonConverter);
            return serviceRepository;
        }

        internal static ServiceRepository RegisterPropertyTypeConverter(this ServiceRepository serviceRepository, [NotNull] IPropertyTypeConverter converter)
        {
            Guard.ThrowIfNull(converter);

            serviceRepository.RegisterSingleton(converter);
            return serviceRepository;
        }

        internal static ServiceRepository RegisterObjectTypeTransformer(this ServiceRepository serviceRepository, [NotNull] IObjectTypeTransformer transformer)
        {
            Guard.ThrowIfNull(transformer);

            serviceRepository.RegisterSingleton(transformer);
            return serviceRepository;
        }

        internal static ServiceRepository ParseMessageTemplates(this ServiceRepository serviceRepository, bool? enable)
        {
            if (enable == true)
            {
                NLog.Common.InternalLogger.Debug("Message Template Format always enabled");
                serviceRepository.RegisterSingleton<ILogMessageFormatter>(new LogMessageTemplateFormatter(serviceRepository, true, false));
            }
            else if (enable == false)
            {
                NLog.Common.InternalLogger.Debug("Message Template String Format always enabled");
                serviceRepository.RegisterSingleton<ILogMessageFormatter>(LogMessageStringFormatter.Default);
            }
            else
            {
                //null = auto
                NLog.Common.InternalLogger.Debug("Message Template Auto Format enabled");
                serviceRepository.RegisterSingleton<ILogMessageFormatter>(new LogMessageTemplateFormatter(serviceRepository, false, false));
            }
            return serviceRepository;
        }

        internal static bool? ResolveParseMessageTemplates(this ServiceRepository serviceRepository)
        {
            var messageFormatter = serviceRepository.GetService<ILogMessageFormatter>();
            return messageFormatter?.EnableMessageTemplateParser;
        }

        internal static ServiceRepository RegisterDefaults(this ServiceRepository serviceRepository)
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
