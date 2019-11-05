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

using System;
using JetBrains.Annotations;
using NLog.MessageTemplates;
using NLog.Targets;

namespace NLog.Config
{
    using NLog.Internal;

    internal static class ServiceRepositoryExtensions
    {
        internal static IServiceResolver GetServiceResolver(this LoggingConfiguration loggingConfiguration)
        {
            return loggingConfiguration?.LogFactory?.ServiceRepository ?? LogManager.LogFactory.ServiceRepository;
        }

        public static T ResolveService<T>(this IServiceResolver serviceResolver) where T : class
        {
            return (serviceResolver ?? LogManager.LogFactory.ServiceRepository)?.ResolveInstance(typeof(T)) as T;
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
        public static IServiceRepository RegisterSingleton<T>(this IServiceRepository serviceRepository, T singleton) where T : class
        {
            serviceRepository.RegisterType(typeof(T), (t) => singleton);
            return serviceRepository;
        }

        /// <summary>
        /// Registers the string serializer to use with <see cref="LogEventInfo.MessageTemplateParameters"/>
        /// </summary>
        public static IServiceRepository RegisterValueFormatter(this IServiceRepository serviceRepository, [NotNull] IValueFormatter valueFormatter)
        {
            if (valueFormatter == null)
            {
                throw new ArgumentNullException(nameof(valueFormatter));
            }

            serviceRepository.RegisterSingleton(valueFormatter); //todo not good? Parameter could be non static registered?
            return serviceRepository;
        }

        public static IServiceRepository RegisterJsonConverter(this IServiceRepository serviceRepository, [NotNull] IJsonConverter jsonConverter)
        {
            if (jsonConverter == null)
            {
                throw new ArgumentNullException(nameof(jsonConverter));
            }

            serviceRepository.RegisterSingleton(jsonConverter);
            return serviceRepository;
        }

        public static IServiceRepository RegisterPropertyTypeConverter(this IServiceRepository serviceRepository, [NotNull] IPropertyTypeConverter converter)
        {
            if (converter == null)
            {
                throw new ArgumentNullException(nameof(converter));
            }

            serviceRepository.RegisterSingleton(converter);
            return serviceRepository;
        }

        public static IServiceRepository RegisterDefaults(this IServiceRepository serviceRepository)
        {
            serviceRepository.RegisterJsonConverter(DefaultJsonSerializer.Instance);
          //todo  serviceRepository.RegisterType<IValueFormatter>(typeof(ValueFormatter));
            serviceRepository.RegisterPropertyTypeConverter(PropertyTypeConverter.Instance);
            return serviceRepository;
        }
    }
}
