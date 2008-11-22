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
using NLog.Conditions;

namespace NLog.Config
{
    public class NLogFactories
    {
        private static IFactory<Target, Type> _targetFactory;
        private static IFactory<Filter, Type> _filterFactory;
        private static IFactory<LayoutRenderer, Type> _layoutRendererFactory;
        private static IFactory<Layout, Type> _layoutFactory;
        private static IFactory<MethodInfo, MethodInfo> _conditionMethodFactory;
        private static IFactory<LayoutRenderer, Type> _ambientPropertyFactory;

        public static IFactory<Target, Type> TargetFactory
        {
            get { return _targetFactory; }
            private set { _targetFactory = value; }
        }

        public static IFactory<Filter, Type> FilterFactory
        {
            get { return _filterFactory; }
            private set { _filterFactory = value; }
        }

        public static IFactory<LayoutRenderer, Type> LayoutRendererFactory
        {
            get { return _layoutRendererFactory; }
            private set { _layoutRendererFactory = value; }
        }

        public static IFactory<Layout, Type> LayoutFactory
        {
            get { return _layoutFactory; }
            private set { _layoutFactory = value; }
        }

        public static IFactory<LayoutRenderer, Type> AmbientPropertyFactory
        {
            get { return _ambientPropertyFactory; }
            private set { _ambientPropertyFactory = value; }
        }

        public static IFactory<MethodInfo, MethodInfo> ConditionMethodFactory
        {
            get { return _conditionMethodFactory; }
            private set { _conditionMethodFactory = value; }
        }

        private static ICollection<IFactory> _allFactories;

        static NLogFactories()
        {
            _allFactories = new List<IFactory>();
            _allFactories.Add(TargetFactory = new Factory<Target, TargetAttribute>(false));
            _allFactories.Add(FilterFactory = new Factory<Filter, FilterAttribute>(false));
            _allFactories.Add(LayoutRendererFactory = new Factory<LayoutRenderer, LayoutRendererAttribute>(false));
            _allFactories.Add(LayoutFactory = new Factory<Layout, LayoutAttribute>(false));
            _allFactories.Add(ConditionMethodFactory = new MethodFactory<ConditionMethodsAttribute, ConditionMethodAttribute>());
            _allFactories.Add(AmbientPropertyFactory = new Factory<LayoutRenderer, AmbientPropertyAttribute>(false));
        }

        public static void ScanAssembly(Assembly asm, string prefix)
        {
            foreach (IFactory f in _allFactories)
            {
                f.ScanAssembly(asm, prefix);
            }
        }

        public static void Clear()
        {
            foreach (IFactory f in _allFactories)
            {
                f.Clear();
            }
        }
    }
}
