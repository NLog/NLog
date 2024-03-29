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
#pragma warning disable CS0618 // Type or member is obsolete
        ,INamedItemFactory<TBaseType, Type>
#pragma warning restore CS0618 // Type or member is obsolete
        where TBaseType : class
        where TAttributeType : NameBaseAttribute
    {
        private struct ItemFactory
        {
            public readonly GetTypeDelegate ItemType;
            public readonly Func<TBaseType> ItemCreator;

            public ItemFactory(GetTypeDelegate type, Func<TBaseType> itemCreator)
            {
                ItemType = type;
                ItemCreator = itemCreator;
            }
        }

        private readonly Dictionary<string, ItemFactory> _items;
        private readonly ConfigurationItemFactory _parentFactory;
        private readonly Factory<TBaseType, TAttributeType> _globalDefaultFactory;

        internal Factory(ConfigurationItemFactory parentFactory, Factory<TBaseType, TAttributeType> globalDefaultFactory)
        {
            _parentFactory = parentFactory;
            _globalDefaultFactory = globalDefaultFactory;
            _items = new Dictionary<string, ItemFactory>(globalDefaultFactory is null ? 16 : 0, StringComparer.OrdinalIgnoreCase);
        }

        private delegate Type GetTypeDelegate();

        /// <summary>
        /// Scans the assembly.
        /// </summary>
        /// <param name="types">The types to scan.</param>
        /// <param name="assemblyName">The assembly name for the types.</param>
        /// <param name="itemNamePrefix">The prefix.</param>
        [Obsolete("Instead use RegisterType<T>, as dynamic Assembly loading will be moved out. Marked obsolete with NLog v5.2")]
        [UnconditionalSuppressMessage("Trimming - Ignore since obsolete", "IL2072")]
        [UnconditionalSuppressMessage("Trimming - Ignore since obsolete", "IL2062")]
        public void ScanTypes(Type[] types, string assemblyName, string itemNamePrefix)
        {
            foreach (Type t in types)
            {
                try
                {
                    RegisterType(t, itemNamePrefix);
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
                _items[itemName] = new ItemFactory(typeLookup, typeCreator);
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

        /// <inheritdoc/>
        [Obsolete("Instead use RegisterType<T>, as dynamic Assembly loading will be moved out. Marked obsolete with NLog v5.2")]
        [UnconditionalSuppressMessage("Trimming - Ignore since obsolete", "IL2067")]
        void INamedItemFactory<TBaseType, Type>.RegisterDefinition(string itemName, Type itemDefinition)
        {
            if (string.IsNullOrEmpty(itemName))
                throw new ArgumentException($"Missing NLog {typeof(TBaseType).Name} type-alias", nameof(itemName));

            if (!typeof(TBaseType).IsAssignableFrom(itemDefinition))
                throw new ArgumentException($"Not of type NLog {typeof(TBaseType).Name}", nameof(itemDefinition));

            RegisterDefinition(itemDefinition, itemName, string.Empty);
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
            var itemFactory = new ItemFactory(() => itemType, itemCreator);
            lock (ConfigurationItemFactory.SyncRoot)
            {
                _parentFactory.RegisterTypeProperties(itemType, () => itemCreator.Invoke());
                _items[itemNamePrefix + typeAlias] = itemFactory;
            }
        }

        public void RegisterType<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor | DynamicallyAccessedMemberTypes.PublicProperties)] TType>(string typeAlias) where TType : TBaseType, new()
        {
            typeAlias = FactoryExtensions.NormalizeName(typeAlias);

            var itemFactory = new ItemFactory(() => typeof(TType), () => new TType());
            lock (ConfigurationItemFactory.SyncRoot)
            {
                _parentFactory.RegisterTypeProperties<TType>(() => new TType());
                _items[typeAlias] = itemFactory;
            }
        }

        public void RegisterType<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor | DynamicallyAccessedMemberTypes.PublicProperties)] TType>(string typeAlias, Func<TType> itemCreator) where TType : TBaseType
        {
            typeAlias = FactoryExtensions.NormalizeName(typeAlias);

            var itemFactory = new ItemFactory(() => typeof(TType), () => itemCreator());
            lock (ConfigurationItemFactory.SyncRoot)
            {
                _parentFactory.RegisterTypeProperties<TType>(() => itemCreator());
                _items[typeAlias] = itemFactory;
            }
        }

        /// <inheritdoc/>
        [Obsolete("Use TryCreateInstance instead. Marked obsolete with NLog v5.2")]
        public bool TryGetDefinition(string itemName, out Type result)
        {
            itemName = FactoryExtensions.NormalizeName(itemName);

            if (!TryGetItemFactory(itemName, out var itemFactory) || itemFactory.ItemType is null)
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
                result = itemFactory.ItemType.Invoke();
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

        private bool TryGetItemFactory(string typeAlias, out ItemFactory itemFactory)
        {
            lock (ConfigurationItemFactory.SyncRoot)
            {
                return _items.TryGetValue(typeAlias, out itemFactory);
            }
        }

        bool INamedItemFactory<TBaseType, Type>.TryCreateInstance(string itemName, out TBaseType result)
        {
            return TryCreateInstance(itemName, out result);
        }

        /// <inheritdoc/>
        public virtual bool TryCreateInstance(string typeAlias, out TBaseType result)
        {
#pragma warning disable CS0618 // Type or member is obsolete
            if (TryCreateInstanceLegacy(typeAlias, out result))
                return true;
#pragma warning restore CS0618 // Type or member is obsolete

            typeAlias = FactoryExtensions.NormalizeName(typeAlias);

            if (!TryGetItemFactory(typeAlias, out var itemFactory) || itemFactory.ItemCreator is null)
            {
                result = null;
                return false;
            }

            result = itemFactory.ItemCreator.Invoke();
            return !(result is null);
        }

        [Obsolete("Use TryCreateInstance instead. Marked obsolete with NLog v5.2")]
        private bool TryCreateInstanceLegacy(string itemName, out TBaseType result)
        {
            if (!ReferenceEquals(_parentFactory.CreateInstance, FactoryHelper.CreateInstance)) 
            {
                if (TryGetDefinition(itemName, out var itemType))
                {
                    result = (TBaseType)_parentFactory.CreateInstance(itemType);
                    return true;
                }
            }

            result = null;
            return false;
        }

        /// <inheritdoc/>
        [Obsolete("Use TryCreateInstance instead. Marked obsolete with NLog v5.2")]
        TBaseType INamedItemFactory<TBaseType, Type>.CreateInstance(string itemName)
        {
            return FactoryExtensions.CreateInstance(this, itemName);
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
#if NETSTANDARD
                message += " - Extension NLog.Web.AspNetCore not included?";
#else
                message += " - Extension NLog.Web not included?";
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
            : base(parentFactory, globalDefaultFactory)
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
