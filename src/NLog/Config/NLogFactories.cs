using System;
using System.Collections.Generic;
using System.Text;
using NLog.Internal;
using System.Reflection;
using NLog.Layouts;
using NLog.Targets;
using NLog.Filters;

using NLog.Config;
using NLog.LayoutRenderers;

namespace NLog.Config
{
    public class NLogFactories
    {
        public static readonly IFactory<Target,Type> TargetFactory = new Factory<Target, TargetAttribute>(false);
        public static readonly IFactory<Filter, Type> FilterFactory = new Factory<Filter, FilterAttribute>(false);
        public static readonly IFactory<LayoutRenderer, Type> LayoutRendererFactory = new Factory<LayoutRenderer, LayoutRendererAttribute>(false);
        public static readonly IFactory<Layout, Type> LayoutFactory = new Factory<Layout, LayoutAttribute>(false);
        public static readonly IFactory<MethodInfo,MethodInfo> ConditionMethodFactory = new MethodFactory<ConditionMethodsAttribute, ConditionMethodAttribute>();

        public static readonly ICollection<IFactory> AllFactories = new List<IFactory>();

        static NLogFactories()
        {
            AllFactories.Add(TargetFactory);
            AllFactories.Add(FilterFactory);
            AllFactories.Add(LayoutRendererFactory);
            AllFactories.Add(LayoutFactory);
            AllFactories.Add(ConditionMethodFactory);
        }

        public static void ScanAssembly(Assembly asm, string prefix)
        {
            foreach (IFactory f in AllFactories)
            {
                f.ScanAssembly(asm, prefix);
            }
        }

        public static void Clear()
        {
            foreach (IFactory f in AllFactories)
            {
                f.Clear();
            }
        }
    }
}
