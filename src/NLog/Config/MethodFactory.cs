using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using NLog.Internal;

namespace NLog.Config
{
    internal class MethodFactory<TClassAttributeType,TMethodAttributeType> : IFactory<MethodInfo,MethodInfo>
        where TClassAttributeType : Attribute
        where TMethodAttributeType : NameAttributeBase
    {
        private static Dictionary<string, MethodInfo> _nameToType = new Dictionary<string, MethodInfo>();

        public MethodFactory()
        {
            foreach (Assembly a in ExtensionUtils.GetExtensionAssemblies())
            {
                ScanAssembly(a, "");
            }
        }

        public void ScanAssembly(Assembly theAssembly, string prefix)
        {
            try
            {
                InternalLogger.Debug("ScanAssembly('{0}','{1}','{2}')", theAssembly.FullName, typeof(TClassAttributeType), typeof(TMethodAttributeType));
                foreach (Type t in theAssembly.GetTypes())
                {
                    if (t.IsDefined(typeof(TClassAttributeType), false))
                    {
                        foreach (MethodInfo mi in t.GetMethods())
                        {
                            TMethodAttributeType[] methodAttributes = (TMethodAttributeType[])mi.GetCustomAttributes(typeof(TMethodAttributeType), false);
                            foreach (TMethodAttributeType attr in methodAttributes)
                            {
                                Add(prefix + attr.Name, mi);
                            }
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
            _nameToType.Clear();
        }

        public void Add(string name, MethodInfo methodInfo)
        {
            _nameToType.Add(name, methodInfo);
        }

        public bool TryCreate(string name, out MethodInfo result)
        {
            return _nameToType.TryGetValue(name, out result);
        }

        public MethodInfo Create(string name)
        {
            MethodInfo result;

            if (TryCreate(name, out result))
                return result;
            throw new ArgumentException("Unknown function: '" + name + "'");
        }

        public IEnumerable<MethodInfo> RegisteredItems
        {
            get { return _nameToType.Values; }
        }

        public bool TryGetType(string name, out Type result)
        {
            result = null;
            return false;
        }
    }
}
