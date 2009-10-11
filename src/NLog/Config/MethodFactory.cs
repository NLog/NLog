using System;
using System.Collections.Generic;
using System.Reflection;
using NLog.Common;

namespace NLog.Config
{
    /// <summary>
    /// Factory for locating methods.
    /// </summary>
    /// <typeparam name="TClassAttributeType">The type of the class marker attribute.</typeparam>
    /// <typeparam name="TMethodAttributeType">The type of the method marker attribute.</typeparam>
    internal class MethodFactory<TClassAttributeType, TMethodAttributeType> : INamedItemFactory<MethodInfo, MethodInfo>, IFactory
        where TClassAttributeType : Attribute
        where TMethodAttributeType : NameAttributeBase
    {
        private readonly Dictionary<string, MethodInfo> nameToMethodInfo = new Dictionary<string, MethodInfo>();

        /// <summary>
        /// Gets a collection of all registered items in the factory.
        /// </summary>
        /// <returns>
        /// Sequence of key/value pairs where each key represents the name
        /// of the item and value is the <see cref="MethodInfo"/> of
        /// the item.
        /// </returns>
        public IDictionary<string, MethodInfo> AllRegisteredItems
        {
            get { return this.nameToMethodInfo; }
        }

        /// <summary>
        /// Scans the assembly for classes marked with <typeparamref name="TClassAttributeType"/>
        /// and methods marked with <typeparamref name="TMethodAttributeType"/> and adds them 
        /// to the factory.
        /// </summary>
        /// <param name="theAssembly">The assembly.</param>
        /// <param name="prefix">The prefix to use for names.</param>
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
                            var methodAttributes = (TMethodAttributeType[])mi.GetCustomAttributes(typeof(TMethodAttributeType), false);
                            foreach (TMethodAttributeType attr in methodAttributes)
                            {
                                this.RegisterDefinition(prefix + attr.Name, mi);
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

        /// <summary>
        /// Clears contents of the factory.
        /// </summary>
        public void Clear()
        {
            this.nameToMethodInfo.Clear();
        }

        /// <summary>
        /// Registers the definition of a single method.
        /// </summary>
        /// <param name="name">The method name.</param>
        /// <param name="methodInfo">The method info.</param>
        public void RegisterDefinition(string name, MethodInfo methodInfo)
        {
            this.nameToMethodInfo.Add(name, methodInfo);
        }

        /// <summary>
        /// Tries to retrieve method by name.
        /// </summary>
        /// <param name="name">The method name.</param>
        /// <param name="result">The result.</param>
        /// <returns>A value of <c>true</c> if the method was found, <c>false</c> otherwise.</returns>
        public bool TryCreateInstance(string name, out MethodInfo result)
        {
            return this.nameToMethodInfo.TryGetValue(name, out result);
        }

        /// <summary>
        /// Retrieves method by name.
        /// </summary>
        /// <param name="name">Method name.</param>
        /// <returns>MethodInfo object.</returns>
        public MethodInfo CreateInstance(string name)
        {
            MethodInfo result;

            if (this.TryCreateInstance(name, out result))
            {
                return result;
            }

            throw new ArgumentException("Unknown function: '" + name + "'");
        }

        /// <summary>
        /// Tries to get method definition.
        /// </summary>
        /// <param name="name">The method .</param>
        /// <param name="result">The result.</param>
        /// <returns>A value of <c>true</c> if the method was found, <c>false</c> otherwise.</returns>
        public bool TryGetDefinition(string name, out MethodInfo result)
        {
            return this.nameToMethodInfo.TryGetValue(name, out result);
        }
    }
}
