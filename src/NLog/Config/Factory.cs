// 
// Copyright (c) 2004-2018 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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

using NLog.LayoutRenderers;

namespace NLog.Config
{
    using System;
    using System.Collections.Generic;
    using Common;
    using Internal;

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

        internal Factory(ConfigurationItemFactory parentFactory)
        {
            _parentFactory = parentFactory;
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
            IEnumerable<TAttributeType> attributes = type.GetCustomAttributes<TAttributeType>(false);
            if (attributes != null)
            {
                foreach (TAttributeType attr in attributes)
                {
                    RegisterDefinition(itemNamePrefix + attr.Name, type);
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
            _items[itemName] = () => itemDefinition;
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

            if (!_items.TryGetValue(itemName, out getTypeDelegate))
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
        public virtual bool TryCreateInstance(string itemName, out TBaseType result)
        {
            Type type;

            if (!TryGetDefinition(itemName, out type))
            {
                result = null;
                return false;
            }

            result = (TBaseType)_parentFactory.CreateInstance(type);
            return true;
        }

        /// <summary>
        /// Creates an item instance.
        /// </summary>
        /// <param name="name">The name of the item.</param>
        /// <returns>Created item.</returns>
        public virtual TBaseType CreateInstance(string name)
        {
            TBaseType result;

            if (TryCreateInstance(name, out result))
            {
                return result;
            }
            var message = typeof(TBaseType).Name + " cannot be found: '" + name + "'";
            if (name != null && (name.StartsWith("aspnet", StringComparison.OrdinalIgnoreCase) ||
                                 name.StartsWith("iis", StringComparison.OrdinalIgnoreCase)))
            {
                //common mistake and probably missing NLog.Web
                message += ". Is NLog.Web not included?";
            }


            throw new ArgumentException(message);
        }
    }

    /// <summary>
    /// Factory specialized for <see cref="LayoutRenderer"/>s. 
    /// </summary>
    class LayoutRendererFactory : Factory<LayoutRenderer, LayoutRendererAttribute>
    {
        public LayoutRendererFactory(ConfigurationItemFactory parentFactory) : base(parentFactory)
        {
        }

        private Dictionary<string, FuncLayoutRenderer> _funcRenderers;

        /// <summary>
        /// Clear all func layouts
        /// </summary>
        public void ClearFuncLayouts()
        {
            _funcRenderers = null;
        }

        /// <summary>
        /// Register a layout renderer with a callback function.
        /// </summary>
        /// <param name="name">Name of the layoutrenderer, without ${}.</param>
        /// <param name="renderer">the renderer that renders the value.</param>
        public void RegisterFuncLayout(string name, FuncLayoutRenderer renderer)
        {
            _funcRenderers = _funcRenderers ?? new Dictionary<string, FuncLayoutRenderer>(StringComparer.OrdinalIgnoreCase);

            //overwrite current if there is one
            _funcRenderers[name] = renderer;
        }


        /// <summary>
        /// Tries to create an item instance.
        /// </summary>
        /// <param name="itemName">Name of the item.</param>
        /// <param name="result">The result.</param>
        /// <returns>True if instance was created successfully, false otherwise.</returns>
        public override bool TryCreateInstance(string itemName, out LayoutRenderer result)
        {
            //first try func renderers, as they should have the possiblity to overwrite a current one.
            if (_funcRenderers != null)
            {
                FuncLayoutRenderer funcResult;
                var succesAsFunc = _funcRenderers.TryGetValue(itemName, out funcResult);
                if (succesAsFunc)
                {
                    result = funcResult;
                    return true;
                }
            }

            var success = base.TryCreateInstance(itemName, out result);

            return success;
        }

    }
}
