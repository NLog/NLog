using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace NLog.Config
{
    public interface IFactory
    {
        void Clear();
        void ScanAssembly(Assembly theAssembly, string prefix);
    }

    public interface IFactory<TBaseType,TItemType> : IFactory
        where TBaseType : class
    {
        void Add(string name, TItemType type);
        bool TryCreate(string name, out TBaseType result);
        bool TryGetType(string name, out Type result);
        TBaseType Create(string name);
        IEnumerable<TItemType> RegisteredItems { get; }
    }
}
