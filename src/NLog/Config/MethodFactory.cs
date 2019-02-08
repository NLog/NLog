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

namespace NLog.Config
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using Common;
    using Internal;

    /// <summary>
    /// Factory for locating methods.
    /// </summary>
    /// <typeparam name="TClassAttributeType">The type of the class marker attribute.</typeparam>
    /// <typeparam name="TMethodAttributeType">The type of the method marker attribute.</typeparam>
    internal class MethodFactory<TClassAttributeType, TMethodAttributeType> : INamedItemFactory<MethodInfo, MethodInfo>, IFactory
        where TClassAttributeType : Attribute
        where TMethodAttributeType : NameBaseAttribute
    {
        private readonly Dictionary<string, MethodInfo> _nameToMethodInfo = new Dictionary<string, MethodInfo>();

        /// <summary>
        /// Gets a collection of all registered items in the factory.
        /// </summary>
        /// <returns>
        /// Sequence of key/value pairs where each key represents the name
        /// of the item and value is the <see cref="MethodInfo"/> of
        /// the item.
        /// </returns>
        public IDictionary<string, MethodInfo> AllRegisteredItems => _nameToMethodInfo;

        /// <summary>
        /// Scans the assembly for classes marked with <typeparamref name="TClassAttributeType"/>
        /// and methods marked with <typeparamref name="TMethodAttributeType"/> and adds them 
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
                    RegisterType(t, prefix);
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
            if (type.IsDefined(typeof(TClassAttributeType), false))
            {
                foreach (MethodInfo mi in type.GetMethods())
                {
                    var methodAttributes = (TMethodAttributeType[])mi.GetCustomAttributes(typeof(TMethodAttributeType), false);
                    foreach (TMethodAttributeType attr in methodAttributes)
                    {
                        RegisterDefinition(itemNamePrefix + attr.Name, mi);
                    }
                }
            }
        }

        /// <summary>
        /// Clears contents of the factory.
        /// </summary>
        public void Clear()
        {
            _nameToMethodInfo.Clear();
        }

        /// <summary>
        /// Registers the definition of a single method.
        /// </summary>
        /// <param name="itemName">The method name.</param>
        /// <param name="itemDefinition">The method info.</param>
        public void RegisterDefinition(string itemName, MethodInfo itemDefinition)
        {
            _nameToMethodInfo[itemName] = itemDefinition;
        }

        /// <summary>
        /// Tries to retrieve method by name.
        /// </summary>
        /// <param name="itemName">The method name.</param>
        /// <param name="result">The result.</param>
        /// <returns>A value of <c>true</c> if the method was found, <c>false</c> otherwise.</returns>
        public bool TryCreateInstance(string itemName, out MethodInfo result)
        {
            return _nameToMethodInfo.TryGetValue(itemName, out result);
        }

        /// <summary>
        /// Retrieves method by name.
        /// </summary>
        /// <param name="itemName">Method name.</param>
        /// <returns>MethodInfo object.</returns>
        public MethodInfo CreateInstance(string itemName)
        {
            MethodInfo result;

            if (TryCreateInstance(itemName, out result))
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
            return _nameToMethodInfo.TryGetValue(itemName, out result);
        }
    }
}
