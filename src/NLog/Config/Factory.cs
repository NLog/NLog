using System;
using System.Collections.Generic;
using System.Reflection;
using NLog.Common;

namespace NLog.Config
{
    /// <summary>
    /// Factory for class-based items.
    /// </summary>
    /// <typeparam name="TBaseType">The base type of each item.</typeparam>
    /// <typeparam name="TAttributeType">The type of the attribute used to annotate itemss.</typeparam>
    internal class Factory<TBaseType, TAttributeType> : INamedItemFactory<TBaseType, Type>, IFactory
        where TBaseType : class 
        where TAttributeType : NameAttributeBase
    {
        private readonly Dictionary<string, Type> items = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Initializes a new instance of the Factory class.
        /// </summary>
        public Factory()
        {
        }

        /// <summary>
        /// Gets a collection of all registered items in the factory.
        /// </summary>
        /// <returns>Sequence of key/value pairs where each key represents the name 
        /// of the item and value is the <see cref="Type"/> of
        /// the item.</returns>
        public IDictionary<string, Type> AllRegisteredItems
        {
            get { return this.items; }
        }

        /// <summary>
        /// Scans the assembly.
        /// </summary>
        /// <param name="theAssembly">The assembly.</param>
        /// <param name="prefix">The prefix.</param>
        public void ScanAssembly(Assembly theAssembly, string prefix)
        {
            try
            {
                InternalLogger.Debug("ScanAssembly('{0}','{1}','{2}')", theAssembly.FullName, typeof(TAttributeType), typeof(TBaseType));
                foreach (Type t in theAssembly.GetTypes())
                {
                    TAttributeType[] attributes = (TAttributeType[])t.GetCustomAttributes(typeof(TAttributeType), false);
                    if (attributes != null)
                    {
                        foreach (TAttributeType attr in attributes)
                        {
                            this.RegisterDefinition(prefix + attr.Name, t);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                InternalLogger.Error("Failed to add targets from '" + theAssembly.FullName + "': {0}", ex);
            }
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
            this.items.Add(name, type);
        }

        /// <summary>
        /// Tries to get registed item definition.
        /// </summary>
        /// <param name="itemName">Name of the item.</param>
        /// <param name="result">Reference to a variable which will store the item definition.</param>
        /// <returns>Item definition.</returns>
        public bool TryGetDefinition(string itemName, out Type result)
        {
            return this.items.TryGetValue(itemName, out result);
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

            if (!this.items.TryGetValue(itemName, out type))
            {
                result = null;
                return false;
            }

            result = (TBaseType)Activator.CreateInstance(type);
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
