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
    using System.Diagnostics.CodeAnalysis;
    using System.Reflection;
    using NLog.Common;
    using NLog.Internal;
    using NLog.LayoutRenderers;

    /// <summary>
    /// Factory for class-based items.
    /// </summary>
    /// <typeparam name="TBaseType">The base type of each item.</typeparam>
    /// <typeparam name="TAttributeType">The type of the attribute used to annotate items.</typeparam>
    internal class Factory<TBaseType, TAttributeType> :
        IFactory, IFactory<TBaseType>
        where TBaseType : class
        where TAttributeType : NameBaseAttribute
    {
        private readonly Dictionary<string, Func<TBaseType>> _items;
        private readonly ConfigurationItemFactory _parentFactory;

        internal Factory(ConfigurationItemFactory parentFactory)
        {
            _parentFactory = parentFactory;
            _items = new Dictionary<string, Func<TBaseType>>(16, StringComparer.OrdinalIgnoreCase);
        }

        private delegate Type GetTypeDelegate();

        /// <summary>
        /// Registers the type.
        /// </summary>
        /// <param name="type">The type to register.</param>
        /// <param name="itemNamePrefix">The item name prefix.</param>
        public void RegisterType([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor | DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.PublicMethods)] Type type, string itemNamePrefix)
        {
            if (typeof(TBaseType).IsAssignableFrom(type))
            {
                IEnumerable<TAttributeType> attributes = type.GetCustomAttributes<TAttributeType>(false);
                if (attributes != null)
                {
                    foreach (var attr in attributes)
                    {
                        RegisterDefinition(type, attr.Name, itemNamePrefix);
                    }
                }
            }
        }

        /// <summary>
        /// Registers the item based on a type name.
        /// </summary>
        /// <param name="itemName">Name of the item.</param>
        /// <param name="typeName">Name of the type.</param>
        [Obsolete("Instead use RegisterType<T>, as dynamic Assembly loading will be moved out. Marked obsolete with NLog v5.2")]
        [UnconditionalSuppressMessage("Trimming - Ignore since obsolete", "IL2072")]
        public void RegisterNamedType(string itemName, string typeName)
        {
            itemName = FactoryExtensions.NormalizeName(itemName);

            Type itemType = null;

            GetTypeDelegate typeLookup = () =>
            {
                if (itemType is null)
                {
                    InternalLogger.Debug("Object reflection needed to resolve type: {0}", typeName);
                    itemType = PropertyTypeConverter.ConvertToType(typeName, false);
                }
                return itemType;
            };

            Func<TBaseType> typeCreator = () =>
            {
                var type = typeLookup();
                return type != null ? (TBaseType)Activator.CreateInstance(type) : null;
            };

            lock (ConfigurationItemFactory.SyncRoot)
            {
                _items[itemName] = typeCreator;
            }
        }

        /// <summary>
        /// Clears the contents of the factory.
        /// </summary>
        public virtual void Clear()
        {
            lock (ConfigurationItemFactory.SyncRoot)
            {
                _items.Clear();
            }
        }

        [Obsolete("Instead use RegisterType<T>, as dynamic Assembly loading will be moved out. Marked obsolete with NLog v5.2")]
        public void RegisterDefinition(string typeAlias, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor | DynamicallyAccessedMemberTypes.PublicProperties)] Type itemType)
        {
            if (string.IsNullOrEmpty(typeAlias))
                throw new ArgumentException($"Missing NLog {typeof(TBaseType).Name} type-alias", nameof(typeAlias));

            if (!typeof(TBaseType).IsAssignableFrom(itemType))
                throw new ArgumentException($"Not of type NLog {typeof(TBaseType).Name}", nameof(itemType));

            RegisterDefinition(itemType, typeAlias, string.Empty);
        }

        private void RegisterDefinition([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor | DynamicallyAccessedMemberTypes.PublicProperties)] Type itemType, string typeAlias, string itemNamePrefix)
        {
            typeAlias = FactoryExtensions.NormalizeName(typeAlias);

            Func<TBaseType> itemCreator = () => (TBaseType)Activator.CreateInstance(itemType);
            lock (ConfigurationItemFactory.SyncRoot)
            {
                _parentFactory.RegisterTypeProperties(itemType, () => itemCreator.Invoke());
                _items[itemNamePrefix + typeAlias] = () => itemCreator.Invoke();
            }
        }

        public void RegisterType<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor | DynamicallyAccessedMemberTypes.PublicProperties)] TType>(string typeAlias) where TType : TBaseType, new()
        {
            typeAlias = FactoryExtensions.NormalizeName(typeAlias);

            lock (ConfigurationItemFactory.SyncRoot)
            {
                _parentFactory.RegisterTypeProperties<TType>(() => new TType());
                _items[typeAlias] = () => new TType();
            }
        }

        public void RegisterType<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor | DynamicallyAccessedMemberTypes.PublicProperties)] TType>(string typeAlias, Func<TType> itemCreator) where TType : TBaseType
        {
            typeAlias = FactoryExtensions.NormalizeName(typeAlias);

            lock (ConfigurationItemFactory.SyncRoot)
            {
                _parentFactory.RegisterTypeProperties<TType>(() => itemCreator());
                _items[typeAlias] = () => itemCreator();
            }
        }

        private bool TryGetItemFactory(string typeAlias, out Func<TBaseType> itemFactory)
        {
            lock (ConfigurationItemFactory.SyncRoot)
            {
                return _items.TryGetValue(typeAlias, out itemFactory);
            }
        }

        /// <inheritdoc/>
        public virtual bool TryCreateInstance(string typeAlias, out TBaseType result)
        {
            typeAlias = FactoryExtensions.NormalizeName(typeAlias);

            if (!TryGetItemFactory(typeAlias, out var itemFactory) || itemFactory is null)
            {
                result = null;
                return false;
            }

            result = itemFactory.Invoke();
            return !(result is null);
        }
    }

    internal static class FactoryExtensions
    {
        public static TBaseType CreateInstance<TBaseType>(this IFactory<TBaseType> factory, string typeAlias) where TBaseType : class
        {
            try
            {
                if (factory.TryCreateInstance(typeAlias, out var result))
                {
                    return result;
                }
            }
            catch (Exception ex)
            {
                throw new NLogConfigurationException($"Failed to create {typeof(TBaseType).Name} of type: '{typeAlias}' - {ex.Message}", ex);
            }

            var normalName = NormalizeName(typeAlias);
            var message = $"Failed to create {typeof(TBaseType).Name} with unknown type-alias: '{typeAlias}'";
            if (normalName != null && (normalName.StartsWith("aspnet", StringComparison.OrdinalIgnoreCase) ||
                                 normalName.StartsWith("iis", StringComparison.OrdinalIgnoreCase)))
            {
#if NETFRAMEWORK
                message += " - Extension NLog.Web not included?";
#else
                message += " - Extension NLog.Web.AspNetCore not included?";                
#endif
            }
            else if (normalName?.StartsWith("database", StringComparison.OrdinalIgnoreCase) == true)
            {
                message += " - Extension NLog.Database not included?";
            }
            else if (normalName?.StartsWith("eventlog", StringComparison.OrdinalIgnoreCase) == true)
            {
                message += " - Extension NLog.WindowsEventLog not included?";
            }
            else if (normalName?.StartsWith("windowsidentity", StringComparison.OrdinalIgnoreCase) == true)
            {
                message += " - Extension NLog.WindowsIdentity not included?";
            }
            else if (normalName?.StartsWith("outputdebugstring", StringComparison.OrdinalIgnoreCase) == true)
            {
                message += " - Extension NLog.OutputDebugString not included?";
            }
            else if (normalName?.StartsWith("performancecounter", StringComparison.OrdinalIgnoreCase) == true)
            {
                message += " - Extension NLog.PerformanceCounter not included?";
            }
            else
            {
                message += " - Verify type-alias and check extension is included.";
            }

            throw new NLogConfigurationException(message);
        }

        public static string NormalizeName(string itemName)
        {
            if (itemName is null)
            {
                return string.Empty;
            }

            var delimitIndex = itemName.IndexOf('-');
            if (delimitIndex < 0)
            {
                return itemName;
            }

            // Only for the first comma
            var commaIndex = itemName.IndexOf(',');
            if (commaIndex >= 0)
            {
                var left = itemName.Substring(0, commaIndex).Replace("-", string.Empty);
                var right = itemName.Substring(commaIndex);
                return left + right;
            }

            return itemName.Replace("-", string.Empty);
        }
    }

    /// <summary>
    /// Factory specialized for <see cref="LayoutRenderer"/>s.
    /// </summary>
    internal sealed class LayoutRendererFactory : Factory<LayoutRenderer, LayoutRendererAttribute>
    {
        private readonly Dictionary<string, FuncLayoutRenderer> _funcRenderers = new Dictionary<string, FuncLayoutRenderer>(StringComparer.OrdinalIgnoreCase);
        private readonly LayoutRendererFactory _globalDefaultFactory;

        public LayoutRendererFactory(ConfigurationItemFactory parentFactory, LayoutRendererFactory globalDefaultFactory)
            : base(parentFactory)
        {
            _globalDefaultFactory = globalDefaultFactory;
        }

        /// <inheritdoc/>
        public override void Clear()
        {
            _funcRenderers.Clear();
            base.Clear();
        }

        /// <summary>
        /// Register a layout renderer with a callback function.
        /// </summary>
        /// <param name="itemName">Name of the layoutrenderer, without ${}.</param>
        /// <param name="renderer">the renderer that renders the value.</param>
        public void RegisterFuncLayout(string itemName, FuncLayoutRenderer renderer)
        {
            itemName = FactoryExtensions.NormalizeName(itemName);
            lock (ConfigurationItemFactory.SyncRoot)
            {
                _funcRenderers[itemName] = renderer;
            }
        }

        /// <inheritdoc/>
        public override bool TryCreateInstance(string typeAlias, out LayoutRenderer result)
        {
            //first try func renderers, as they should have the possibility to overwrite a current one.
            FuncLayoutRenderer funcResult;
            typeAlias = FactoryExtensions.NormalizeName(typeAlias);

            if (_funcRenderers.Count > 0)
            {
                lock (ConfigurationItemFactory.SyncRoot)
                {
                    if (_funcRenderers.TryGetValue(typeAlias, out funcResult))
                    {
                        result = funcResult;
                        return true;
                    }
                }
            }

            if (_globalDefaultFactory?._funcRenderers?.Count > 0)
            {
                lock (ConfigurationItemFactory.SyncRoot)
                {
                    if (_globalDefaultFactory._funcRenderers.TryGetValue(typeAlias, out funcResult))
                    {
                        result = funcResult;
                        return true;
                    }
                }
            }

            return base.TryCreateInstance(typeAlias, out result);
        }
    }
}
