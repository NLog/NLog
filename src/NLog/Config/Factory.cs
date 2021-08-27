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
    using System.Collections.Generic;
    using System.Reflection;
    using NLog.Common;
    using NLog.Internal;
    using NLog.LayoutRenderers;

    /// <summary>
    /// Factory for class-based items.
    /// </summary>
    /// <typeparam name="TBaseType">The base type of each item.</typeparam>
    /// <typeparam name="TAttributeType">The type of the attribute used to annotate items.</typeparam>
    internal class Factory<TBaseType, TAttributeType> : INamedItemFactory<TBaseType, Type>, IFactory
        where TBaseType : class
        where TAttributeType : NameBaseAttribute
    {
        private readonly Dictionary<string, GetTypeDelegate> _items = new Dictionary<string, GetTypeDelegate>(StringComparer.OrdinalIgnoreCase);
        private readonly ConfigurationItemFactory _parentFactory;
        private readonly Factory<TBaseType, TAttributeType> _globalDefaultFactory;

        internal Factory(ConfigurationItemFactory parentFactory, Factory<TBaseType, TAttributeType> globalDefaultFactory)
        {
            _parentFactory = parentFactory;
            _globalDefaultFactory = globalDefaultFactory;
        }

        private delegate Type GetTypeDelegate();

        /// <summary>
        /// Scans the assembly.
        /// </summary>
        /// <param name="types">The types to scan.</param>
        /// <param name="assemblyName">The assembly name for the types.</param>
        /// <param name="itemNamePrefix">The prefix.</param>
        public void ScanTypes(Type[] types, string assemblyName, string itemNamePrefix)
        {
            foreach (Type t in types)
            {
                try
                {
                    RegisterType(t, assemblyName, itemNamePrefix);
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
            RegisterType(type, string.Empty, itemNamePrefix);
        }

        /// <summary>
        /// Registers the type.
        /// </summary>
        /// <param name="type">The type to register.</param>
        /// <param name="assemblyName">The assembly name for the type.</param>
        /// <param name="itemNamePrefix">The item name prefix.</param>
        public void RegisterType(Type type, string assemblyName, string itemNamePrefix)
        {
            if (typeof(TBaseType).IsAssignableFrom(type))
            {
                IEnumerable<TAttributeType> attributes = type.GetCustomAttributes<TAttributeType>(false);
                if (attributes != null)
                {
                    foreach (var attr in attributes)
                    {
                        RegisterDefinition(attr.Name, type, assemblyName, itemNamePrefix);
                    }
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
            itemName = NormalizeName(itemName);
            _items[itemName] = () => Type.GetType(typeName, false);
        }

        /// <summary>
        /// Clears the contents of the factory.
        /// </summary>
        public void Clear()
        {
            _items.Clear();
        }

        /// <summary>
        /// Registers a single type definition.
        /// </summary>
        /// <param name="itemName">The item name.</param>
        /// <param name="itemDefinition">The type of the item.</param>
        public void RegisterDefinition(string itemName, Type itemDefinition)
        {
            RegisterDefinition(itemName, itemDefinition, string.Empty, string.Empty);
        }

        /// <summary>
        /// Registers a single type definition.
        /// </summary>
        /// <param name="itemName">The item name.</param>
        /// <param name="itemDefinition">The type of the item.</param>
        /// <param name="assemblyName">The assembly name for the types.</param>
        /// <param name="itemNamePrefix">The item name prefix.</param>
        private void RegisterDefinition(string itemName, Type itemDefinition, string assemblyName, string itemNamePrefix)
        {
            GetTypeDelegate typeLookup = () => itemDefinition;
            itemName = NormalizeName(itemName);
            _items[itemNamePrefix + itemName] = typeLookup;
            if (!string.IsNullOrEmpty(assemblyName))
            {
                _items[itemName + ", " + assemblyName] = typeLookup;
                _items[itemDefinition.Name + ", " + assemblyName] = typeLookup;
                _items[itemDefinition.ToString() + ", " + assemblyName] = typeLookup;
            }
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

            itemName = NormalizeName(itemName);
            if (!_items.TryGetValue(itemName, out getTypeDelegate))
            {
                if (_globalDefaultFactory != null && _globalDefaultFactory.TryGetDefinition(itemName, out result))
                {
                    return true;
                }

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
        public virtual bool TryCreateInstance(string itemName, out TBaseType result)
        {
            itemName = NormalizeName(itemName);
            if (!TryGetDefinition(itemName, out var itemType))
            {
                result = null;
                return false;
            }

            result = (TBaseType)_parentFactory.CreateInstance(itemType);
            return true;
        }

        /// <summary>
        /// Creates an item instance.
        /// </summary>
        /// <param name="itemName">The name of the item.</param>
        /// <returns>Created item.</returns>
        public virtual TBaseType CreateInstance(string itemName)
        {
            itemName = NormalizeName(itemName);
            if (TryCreateInstance(itemName, out TBaseType result))
            {
                return result;
            }

            var message = typeof(TBaseType).Name + " cannot be found: '" + itemName + "'";
            if (itemName != null && (itemName.StartsWith("aspnet", StringComparison.OrdinalIgnoreCase) ||
                                 itemName.StartsWith("iis", StringComparison.OrdinalIgnoreCase)))
            {
#if NETSTANDARD
                //common mistake and probably missing NLog.Web
                message += ". Extension NLog.Web.AspNetCore not included?";
#else
                //common mistake and probably missing NLog.Web
                message += ". Extension NLog.Web not included?";
#endif
            }
            else if (itemName?.StartsWith("database", StringComparison.OrdinalIgnoreCase) == true)
            {
                //common mistake and probably missing NLog.Web
                message += ". Extension NLog.Database not included?";
            }
            else if (itemName?.StartsWith("windows-identity", StringComparison.OrdinalIgnoreCase) == true)
            {
                //common mistake and probably missing NLog.Web
                message += ". Extension NLog.WindowsIdentity not included?";
            }
            else if (itemName?.StartsWith("outputdebugstring", StringComparison.OrdinalIgnoreCase) == true)
            {
                //common mistake and probably missing NLog.Web
                message += ". Extension NLog.OutputDebugString not included?";
            }
            else if (itemName?.StartsWith("performancecounter", StringComparison.OrdinalIgnoreCase) == true)
            {
                //common mistake and probably missing NLog.Web
                message += ". Extension NLog.PerformanceCounter not included?";
            }

            throw new ArgumentException(message);
        }

        protected static string NormalizeName(string itemName)
        {
            var itemToBeRemoved = "-";
            if (!itemName.Contains(itemToBeRemoved))
            {
                return itemName;
            }

            return itemName.Replace(itemToBeRemoved, string.Empty);
        }
    }

    /// <summary>
    /// Factory specialized for <see cref="LayoutRenderer"/>s. 
    /// </summary>
    internal class LayoutRendererFactory : Factory<LayoutRenderer, LayoutRendererAttribute>
    {
        private Dictionary<string, FuncLayoutRenderer> _funcRenderers;
        private readonly LayoutRendererFactory _globalDefaultFactory;

        public LayoutRendererFactory(ConfigurationItemFactory parentFactory, LayoutRendererFactory globalDefaultFactory)
            : base(parentFactory, globalDefaultFactory)
        {
            _globalDefaultFactory = globalDefaultFactory;
        }

        /// <summary>
        /// Clear all func layouts
        /// </summary>
        public void ClearFuncLayouts()
        {
            _funcRenderers?.Clear();
        }

        /// <summary>
        /// Register a layout renderer with a callback function.
        /// </summary>
        /// <param name="itemName">Name of the layoutrenderer, without ${}.</param>
        /// <param name="renderer">the renderer that renders the value.</param>
        public void RegisterFuncLayout(string itemName, FuncLayoutRenderer renderer)
        {
            itemName = NormalizeName(itemName);
            _funcRenderers = _funcRenderers ?? new Dictionary<string, FuncLayoutRenderer>(StringComparer.OrdinalIgnoreCase);

            //overwrite current if there is one
            _funcRenderers[itemName] = renderer;
        }

        /// <summary>
        /// Tries to create an item instance.
        /// </summary>
        /// <param name="itemName">Name of the item.</param>
        /// <param name="result">The result.</param>
        /// <returns>True if instance was created successfully, false otherwise.</returns>
        public override bool TryCreateInstance(string itemName, out LayoutRenderer result)
        {
            //first try func renderers, as they should have the possibility to overwrite a current one.
            FuncLayoutRenderer funcResult;
            itemName = NormalizeName(itemName);
            if (_funcRenderers != null)
            {
                var successAsFunc = _funcRenderers.TryGetValue(itemName, out funcResult);
                if (successAsFunc)
                {
                    result = funcResult;
                    return true;
                }
            }

            if (_globalDefaultFactory?._funcRenderers != null && _globalDefaultFactory._funcRenderers.TryGetValue(itemName, out funcResult))
            {
                result = funcResult;
                return true;
            }

            return base.TryCreateInstance(itemName, out result);
        }
    }
}
