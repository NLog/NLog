using System.Reflection;

namespace NLog.Config
{
    /// <summary>
    /// Provides means to populate factories of named items (such as targets, layouts, layout renderers, etc.).
    /// </summary>
    internal interface IFactory
    {
        void Clear();

        void ScanAssembly(Assembly theAssembly, string prefix);
    }
}
