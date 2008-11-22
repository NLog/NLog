using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using NLog.Internal;

namespace NLog.Config
{
    internal class Factory<TBaseType,TAttributeType> : IFactory<TBaseType,Type>
        where TBaseType : class 
        where TAttributeType : NameAttributeBase
    {
        private static Dictionary<string, Type> _items;

        public Factory(bool caseSensitive)
        {
            _items = new Dictionary<string, Type>(StringComparer.InvariantCultureIgnoreCase);
            foreach (Assembly a in ExtensionUtils.GetExtensionAssemblies())
            {
                ScanAssembly(a, "");
            }
        }

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
                            Add(prefix + attr.Name, t);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                InternalLogger.Error("Failed to add targets from '" + theAssembly.FullName + "': {0}", ex);
            }
        }

        public void Clear()
        {
            _items.Clear();
        }

        public void Add(string name, Type type)
        {
            _items.Add(name, type);
        }

        public bool TryGetType(string name, out Type result)
        {
            return _items.TryGetValue(name, out result);
        }

        public bool TryCreate(string name, out TBaseType result)
        {
            Type type;

            if (!_items.TryGetValue(name, out type))
            {
                result = null;
                return false;
            }
            result = (TBaseType)Activator.CreateInstance(type);
            return true;
        }

        public TBaseType Create(string name)
        {
            TBaseType result;

            if (TryCreate(name, out result))
                return result;
            throw new ArgumentException(typeof(TBaseType).Name + " cannot be found: '" + name + "'");
        }

        public IEnumerable<Type> RegisteredItems
        {
            get { return _items.Values; }
        }
    }
}
