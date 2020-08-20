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

namespace NLog
{
    using System;
    using System.Reflection;
    using NLog.Config;

    /// <summary>
    /// Extension methods to setup NLog extensions, so they are known when loading NLog LoggingConfiguration
    /// </summary>
    public static class SetupSerializationBuilderExtensions
    {
        /// <summary>
        /// Overrides the active <see cref="IJsonConverter"/> with a new custom implementation
        /// </summary>
        public static ISetupSerializationBuilder RegisterJsonConverter(this ISetupSerializationBuilder setupBuilder, IJsonConverter jsonConverter)
        {
            setupBuilder.LogFactory.ServiceRepository.RegisterJsonConverter(jsonConverter ?? new NLog.Targets.DefaultJsonSerializer(setupBuilder.LogFactory.ServiceRepository));
            return setupBuilder;
        }

        /// <summary>
        /// Overrides the active <see cref="IValueFormatter"/> with a new custom implementation
        /// </summary>
        public static ISetupSerializationBuilder RegisterValueFormatter(this ISetupSerializationBuilder setupBuilder, IValueFormatter valueFormatter)
        {
            setupBuilder.LogFactory.ServiceRepository.RegisterValueFormatter(valueFormatter ?? new MessageTemplates.ValueFormatter(setupBuilder.LogFactory.ServiceRepository));
            return setupBuilder;
        }

        /// <summary>
        /// Registers object Type transformation from dangerous (massive) object to safe (reduced) object
        /// </summary>
        public static ISetupSerializationBuilder RegisterObjectTransformation<T>(this ISetupSerializationBuilder setupBuilder, Func<T, object> transformer)
        {
            if (transformer == null)
                throw new ArgumentNullException(nameof(transformer));

            var original = setupBuilder.LogFactory.ServiceRepository.GetService<IObjectTypeTransformer>();
            setupBuilder.LogFactory.ServiceRepository.RegisterObjectTypeTransformer(new ObjectTypeTransformation<T>(transformer, original));
            return setupBuilder;
        }

        /// <summary>
        /// Registers object Type transformation from dangerous (massive) object to safe (reduced) object
        /// </summary>
        public static ISetupSerializationBuilder RegisterObjectTransformation(this ISetupSerializationBuilder setupBuilder, Type objectType, Func<object, object> transformer)
        {
            if (objectType == null)
                throw new ArgumentNullException(nameof(objectType));
            if (transformer == null)
                throw new ArgumentNullException(nameof(transformer));

            var original = setupBuilder.LogFactory.ServiceRepository.GetService<IObjectTypeTransformer>();
            setupBuilder.LogFactory.ServiceRepository.RegisterObjectTypeTransformer(new ObjectTypeTransformation(objectType, transformer, original));
            return setupBuilder;
        }

        private class ObjectTypeTransformation<T> : IObjectTypeTransformer
        {
            private readonly IObjectTypeTransformer _original;
            private readonly Func<T, object> _transformer;

            public ObjectTypeTransformation(Func<T, object> transformer, IObjectTypeTransformer original)
            {
                _original = original;
                _transformer = transformer;
            }

            public object TryTransformObject(object obj)
            {
                if (obj is T rawObject)
                {
                    var wantedObject = _transformer(rawObject);
                    if (wantedObject != null)
                        return wantedObject;
                }
                return _original?.TryTransformObject(obj);
            }
        }

        private class ObjectTypeTransformation : IObjectTypeTransformer
        {
            private readonly IObjectTypeTransformer _original;
            private readonly Func<object, object> _transformer;
            private readonly Type _objectType;

            public ObjectTypeTransformation(Type objecType, Func<object, object> transformer, IObjectTypeTransformer original)
            {
                _original = original;
                _transformer = transformer;
                _objectType = objecType;
            }

            public object TryTransformObject(object obj)
            {
                if (_objectType.IsAssignableFrom(obj.GetType()))
                {
                    return _transformer(obj);
                }

                return _original?.TryTransformObject(obj);
            }
        }
    }
}
