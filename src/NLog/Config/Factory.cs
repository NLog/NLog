// 
// Copyright (c) 2004-2016 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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
    using NLog.Common;
    using NLog.Internal;

    /// <summary>
    /// Factory for class-based items.
    /// </summary>
    /// <typeparam name="TBaseType">The base type of each item.</typeparam>
    /// <typeparam name="TAttributeType">The type of the attribute used to annotate items.</typeparam>
    internal class Factory<TBaseType, TAttributeType> : INamedItemFactory<TBaseType, Type>, IFactory
        where TBaseType : class 
        where TAttributeType : NameBaseAttribute
    {
        private readonly Dictionary<string, GetTypeDelegate> items = new Dictionary<string, GetTypeDelegate>(StringComparer.OrdinalIgnoreCase);
        private ConfigurationItemFactory parentFactory;

        internal Factory(ConfigurationItemFactory parentFactory)
        {
            this.parentFactory = parentFactory;
        }

        private delegate Type GetTypeDelegate();

        /// <summary>
        /// Scans the assembly.
        /// </summary>
        /// <param name="types">The types to scan.</param>
        /// <param name="prefix">The prefix.</param>
        public void ScanTypes(Type[] types, string prefix)
        {
            foreach (Type t in types)
            {
                try
                {
                    this.RegisterType(t, prefix);
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
            TAttributeType[] attributes = (TAttributeType[])type.GetCustomAttributes(typeof(TAttributeType), false);
            if (attributes != null)
            {
                foreach (TAttributeType attr in attributes)
                {
                    this.RegisterDefinition(itemNamePrefix + attr.Name, type);
                }
            }
        }

        /// <summary>
        /// Registers the item based on a type name.
        /// </summary>
        /// <param name="itemName">Name of the item.</param>
        /// <param name="typeName">Name of the type.</param>
        public void RegisterNamedType(string itemName, string typeName)
        {
            this.items[itemName] = () => Type.GetType(typeName, false);
        }

        /// <summary>
        /// Clears the contents of the factory.
        /// </summary>
        public void Clear()
        {
            this.items.Clear();
        }

        /// <summary>
        /// Registers a single type definition.
        /// </summary>
        /// <param name="name">The item name.</param>
        /// <param name="type">The type of the item.</param>
        public void RegisterDefinition(string name, Type type)
        {
            this.items[name] = () => type;
        }

        /// <summary>
        /// Tries to get registered item definition.
        /// </summary>
        /// <param name="itemName">Name of the item.</param>
        /// <param name="result">Reference to a variable which will store the item definition.</param>
        /// <returns>Item definition.</returns>
        public bool TryGetDefinition(string itemName, out Type result)
        {
            GetTypeDelegate getTypeDelegate;

            if (!this.items.TryGetValue(itemName, out getTypeDelegate))
            {
                result = null;
                return false;
            }

            try
            {
                result = getTypeDelegate();
                return result != null;
            }
            catch (Exception ex)
            {
                if (ex.MustBeRethrown())
                {
                    throw;
                }

                // delegate invocation failed - type is not available
                result = null;
                return false;
            }
        }

        /// <summary>
        /// Tries to create an item instance.
        /// </summary>
        /// <param name="itemName">Name of the item.</param>
        /// <param name="result">The result.</param>
        /// <returns>True if instance was created successfully, false otherwise.</returns>
        public bool TryCreateInstance(string itemName, out TBaseType result)
        {
            Type type;

            if (!this.TryGetDefinition(itemName, out type))
            {
                result = null;
                return false;
            }

            result = (TBaseType)this.parentFactory.CreateInstance(type);
            return true;
        }

        /// <summary>
        /// Creates an item instance.
        /// </summary>
        /// <param name="name">The name of the item.</param>
        /// <returns>Created item.</returns>
        public TBaseType CreateInstance(string name)
        {
            TBaseType result;

            if (this.TryCreateInstance(name, out result))
            {
                return result;
            }

            throw new ArgumentException(typeof(TBaseType).Name + " cannot be found: '" + name + "'");
        }
    }
}
