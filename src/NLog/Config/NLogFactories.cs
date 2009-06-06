using System;
using System.Collections.Generic;
using System.Reflection;

using NLog.Conditions;
using NLog.Filters;
using NLog.LayoutRenderers;
using NLog.Layouts;
using NLog.Targets;

namespace NLog.Config
{
    /// <summary>
    /// Provides registration information for named items (targets, layouts, layout renderers, etc.) managed by NLog.
    /// </summary>
    public class NLogFactories
    {
        private ICollection<object> allFactories;

        /// <summary>
        /// Initializes static members of the NLogFactories class.
        /// </summary>
        static NLogFactories()
        {
            Default = new NLogFactories();
        }

        /// <summary>
        /// Initializes a new instance of the NLogFactories class.
        /// </summary>
        public NLogFactories()
        {
            this.allFactories = new List<object>();

            this.allFactories.Add(this.TargetFactory = new Factory<Target, TargetAttribute>());
            this.allFactories.Add(this.FilterFactory = new Factory<Filter, FilterAttribute>());
            this.allFactories.Add(this.LayoutRendererFactory = new Factory<LayoutRenderer, LayoutRendererAttribute>());
            this.allFactories.Add(this.LayoutFactory = new Factory<Layout, LayoutAttribute>());
            this.allFactories.Add(this.ConditionMethodFactory = new MethodFactory<ConditionMethodsAttribute, ConditionMethodAttribute>());
            this.allFactories.Add(this.AmbientPropertyFactory = new Factory<LayoutRenderer, AmbientPropertyAttribute>());

            this.RegisterItemsFromAssembly(typeof(Logger).Assembly);
        }

        /// <summary>
        /// Gets or sets default singleton instance of <see cref="NLogFactories"/>.
        /// </summary>
        public static NLogFactories Default { get; set; }

        /// <summary>
        /// Gets the <see cref="Target"/> factory.
        /// </summary>
        /// <value>The target factory.</value>
        public INamedItemFactory<Target, Type> TargetFactory { get; private set; }

        /// <summary>
        /// Gets the <see cref="Filter"/> factory.
        /// </summary>
        /// <value>The filter factory.</value>
        public INamedItemFactory<Filter, Type> FilterFactory { get; private set; }

        /// <summary>
        /// Gets the <see cref="LayoutRenderer"/> factory.
        /// </summary>
        /// <value>The layout renderer factory.</value>
        public INamedItemFactory<LayoutRenderer, Type> LayoutRendererFactory { get; private set; }

        /// <summary>
        /// Gets the <see cref="LayoutRenderer"/> factory.
        /// </summary>
        /// <value>The layout factory.</value>
        public INamedItemFactory<Layout, Type> LayoutFactory { get; private set; }

        /// <summary>
        /// Gets the ambient property factory.
        /// </summary>
        /// <value>The ambient property factory.</value>
        public INamedItemFactory<LayoutRenderer, Type> AmbientPropertyFactory { get; private set; }

        /// <summary>
        /// Gets the condition method factory.
        /// </summary>
        /// <value>The condition method factory.</value>
        public INamedItemFactory<MethodInfo, MethodInfo> ConditionMethodFactory { get; private set; }

        /// <summary>
        /// Registers named items from the assembly.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        public void RegisterItemsFromAssembly(Assembly assembly)
        {
            this.RegisterItemsFromAssembly(assembly, string.Empty);
        }

        /// <summary>
        /// Registers named items from the assembly.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        /// <param name="itemNamePrefix">Item name prefix.</param>
        public void RegisterItemsFromAssembly(Assembly assembly, string itemNamePrefix)
        {
            foreach (IFactory f in this.allFactories)
            {
                f.ScanAssembly(assembly, itemNamePrefix);
            }
        }

        /// <summary>
        /// Clears the contents of all factories.
        /// </summary>
        public void Clear()
        {
            foreach (IFactory f in this.allFactories)
            {
                f.Clear();
            }
        }
    }
}
