//
// Copyright (c) 2004-2024 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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
    using NLog.Internal;

    /// <summary>
    /// Repository of interfaces used by NLog to allow override for dependency injection
    /// </summary>
    internal sealed class ServiceRepositoryInternal : ServiceRepository
    {
        private readonly Dictionary<Type, Func<object>> _creatorMap = new Dictionary<Type, Func<object>>();
        private readonly object _lockObject = new object();
        public event EventHandler<ServiceRepositoryUpdateEventArgs>? TypeRegistered;

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceRepositoryInternal"/> class.
        /// </summary>
        internal ServiceRepositoryInternal(LogFactory logFactory)
        {
            this.RegisterDefaults(logFactory);
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
            object? serviceInstance = TryGetService(serviceType);
            if (serviceInstance is null)
                throw new NLogDependencyResolveException("Type not registered in Service Provider", serviceType);

            return serviceInstance;
        }

        private object? TryGetService(Type serviceType)
        {
            Guard.ThrowIfNull(serviceType);

            Func<object>? objectResolver = null;
            lock (_lockObject)
            {
                _creatorMap.TryGetValue(serviceType, out objectResolver);
            }

            return objectResolver?.Invoke();
        }

        internal override bool TryGetService<T>(out T? serviceInstance) where T : class
        {
            if (TryGetService(typeof(T)) is T service)
            {
                serviceInstance = service;
                return true;
            }

            serviceInstance = default(T);
            return false;
        }
    }
}
