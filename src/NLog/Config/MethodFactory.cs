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
    using NLog.Common;
    using NLog.Internal;

    /// <summary>
    /// Factory for locating methods.
    /// </summary>
    internal class MethodFactory : INamedItemFactory<MethodInfo, MethodInfo>, INamedItemFactory<ReflectionHelpers.LateBoundMethod, MethodInfo>, IFactory
    {
        private readonly Dictionary<string, MethodInfo> _nameToMethodInfo = new Dictionary<string, MethodInfo>();
        private readonly Dictionary<string, ReflectionHelpers.LateBoundMethod> _nameToLateBoundMethod = new Dictionary<string, ReflectionHelpers.LateBoundMethod>();
        private readonly Func<Type, IList<KeyValuePair<string, MethodInfo>>> _methodExtractor;
        private readonly MethodFactory<TClassAttributeType, TMethodAttributeType> _globalDefaultFactory;

        public MethodFactory(MethodFactory<TClassAttributeType, TMethodAttributeType> globalDefaultFactory)
        {
            _globalDefaultFactory = globalDefaultFactory;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MethodFactory"/> class.
        /// </summary>
        /// <param name="methodExtractor">Helper method to extract relevant methods from type</param>
        public MethodFactory(Func<Type, IList<KeyValuePair<string, MethodInfo>>> methodExtractor)
        {
            _methodExtractor = methodExtractor;
        }

        /// <summary>
        /// Scans the assembly for classes marked with expected class <see cref="Attribute"/>
        /// and methods marked with expected <see cref="NameBaseAttribute"/> and adds them 
        /// to the factory.
        /// </summary>
        /// <param name="types">The types to scan.</param>
        /// <param name="prefix">The prefix to use for names.</param>
        public void ScanTypes(Type[] types, string prefix)
        {
            foreach (Type t in types)
            {
                try
                {
                    if (t.IsClass() || t.IsAbstract())
                    {
                        RegisterType(t, prefix);
                    }
                }
                catch (Exception exception)
                {
                    InternalLogger.Error(exception, "Failed to add type '{0}'.", t.FullName);

                    if (exception.MustBeRethrown())
                    {
                        throw;
                    }
                }
            }
        }

        /// <summary>
        /// Registers the type.
        /// </summary>
        /// <param name="type">The type to register.</param>
        /// <param name="itemNamePrefix">The item name prefix.</param>
        public void RegisterType(Type type, string itemNamePrefix)
        {
            var extractedMethods = _methodExtractor(type);
            if (extractedMethods?.Count > 0)
            {
                for (int i = 0; i < extractedMethods.Count; ++i)
                {
                    RegisterDefinition(itemNamePrefix + extractedMethods[i].Key, extractedMethods[i].Value);
                }
            }
        }

        /// <summary>
        /// Scans a type for relevant methods with their symbolic names
        /// </summary>
        /// <typeparam name="TClassAttributeType">Include types that are marked with this attribute</typeparam>
        /// <typeparam name="TMethodAttributeType">Include methods that are marked with this attribute</typeparam>
        /// <param name="type">Class Type to scan</param>
        /// <returns>Collection of methods with their symbolic names</returns>
        public static IList<KeyValuePair<string, MethodInfo>> ExtractClassMethods<TClassAttributeType, TMethodAttributeType>(Type type) 
            where TClassAttributeType : Attribute
            where TMethodAttributeType : NameBaseAttribute
        {
            if (!type.IsDefined(typeof(TClassAttributeType), false))
                return ArrayHelper.Empty<KeyValuePair<string, MethodInfo>>();

            var conditionMethods = new List<KeyValuePair<string, MethodInfo>>();
            foreach (MethodInfo mi in type.GetMethods())
            {
                var methodAttributes = (TMethodAttributeType[])mi.GetCustomAttributes(typeof(TMethodAttributeType), false);
                foreach (var attr in methodAttributes)
                {
                    conditionMethods.Add(new KeyValuePair<string, MethodInfo>(attr.Name, mi));
                }
            }

            return conditionMethods;
        }

        /// <summary>
        /// Clears contents of the factory.
        /// </summary>
        public void Clear()
        {
            _nameToMethodInfo.Clear();
            lock (_nameToLateBoundMethod)
                _nameToLateBoundMethod.Clear();
        }

        /// <summary>
        /// Registers the definition of a single method.
        /// </summary>
        /// <param name="itemName">The method name.</param>
        /// <param name="itemDefinition">The method info.</param>
        public void RegisterDefinition(string itemName, MethodInfo itemDefinition)
        {
            _nameToMethodInfo[itemName] = itemDefinition;
            lock (_nameToLateBoundMethod)
                _nameToLateBoundMethod.Remove(itemName);
        }

        /// <summary>
        /// Registers the definition of a single method.
        /// </summary>
        /// <param name="itemName">The method name.</param>
        /// <param name="itemDefinition">The method info.</param>
        /// <param name="lateBoundMethod">The precompiled method delegate.</param>
        internal void RegisterDefinition(string itemName, MethodInfo itemDefinition, ReflectionHelpers.LateBoundMethod lateBoundMethod)
        {
            _nameToMethodInfo[itemName] = itemDefinition;
            lock (_nameToLateBoundMethod)
                _nameToLateBoundMethod[itemName] = lateBoundMethod;
        }

        /// <summary>
        /// Tries to retrieve method by name.
        /// </summary>
        /// <param name="itemName">The method name.</param>
        /// <param name="result">The result.</param>
        /// <returns>A value of <c>true</c> if the method was found, <c>false</c> otherwise.</returns>
        public bool TryCreateInstance(string itemName, out MethodInfo result)
        {
            return TryGetDefinition(itemName, out result);
        }

        /// <summary>
        /// Tries to retrieve method-delegate by name.
        /// </summary>
        /// <param name="itemName">The method name.</param>
        /// <param name="result">The result.</param>
        /// <returns>A value of <c>true</c> if the method was found, <c>false</c> otherwise.</returns>
        public bool TryCreateInstance(string itemName, out ReflectionHelpers.LateBoundMethod result)
        {
            lock (_nameToLateBoundMethod)
            {
                if (_nameToLateBoundMethod.TryGetValue(itemName, out result))
                {
                    return true;
                }
            }

            if (_nameToMethodInfo.TryGetValue(itemName, out var methodInfo))
            {
                result = ReflectionHelpers.CreateLateBoundMethod(methodInfo);
                lock (_nameToLateBoundMethod)
                    _nameToLateBoundMethod[itemName] = result;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Retrieves method by name.
        /// </summary>
        /// <param name="itemName">Method name.</param>
        /// <returns>MethodInfo object.</returns>
        MethodInfo INamedItemFactory<MethodInfo, MethodInfo>.CreateInstance(string itemName)
        {
            if (TryCreateInstance(itemName, out MethodInfo result))
            {
                return result;
            }

            throw new NLogConfigurationException($"Unknown function: '{itemName}'");
        }

        /// <summary>
        /// Retrieves method by name.
        /// </summary>
        /// <param name="itemName">Method name.</param>
        /// <returns>Method delegate object.</returns>
        public ReflectionHelpers.LateBoundMethod CreateInstance(string itemName)
        {
            if (TryCreateInstance(itemName, out ReflectionHelpers.LateBoundMethod result))
            {
                return result;
            }

            throw new NLogConfigurationException($"Unknown function: '{itemName}'");
        }

        /// <summary>
        /// Tries to get method definition.
        /// </summary>
        /// <param name="itemName">The method name.</param>
        /// <param name="result">The result.</param>
        /// <returns>A value of <c>true</c> if the method was found, <c>false</c> otherwise.</returns>
        public bool TryGetDefinition(string itemName, out MethodInfo result)
        {
            if (_nameToMethodInfo.TryGetValue(itemName, out result))
            {
                return true;
            }

            return _globalDefaultFactory?.TryGetDefinition(itemName, out result) ?? false;
        }
    }
}
