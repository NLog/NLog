#if !NET_CF && !MONO && !SILVERLIGHT

namespace NLog.LayoutRenderers {
    /// <summary>
    /// 
    /// </summary>
    public enum AssemblyNameProperty {
        /// <summary>
        /// Specifies that we want to get the assembly name
        /// of default executable.
        /// </summary>
        EntryAssembly,
        /// <summary>
        /// Specifies that we want the assembly name
        /// for the assembly that contains the currently
        /// executing code
        /// </summary>
        ExecutingAssembly,
        /// <summary>
        /// Specifies that we want the assembly name
        /// for the assembly that invoked the current executing method
        /// </summary>
        CallingAssembly,
        /// <summary>
        /// Specifies that we want to use a pre-defined assembly name
        /// This should only be used for unit-testing as unit tests
        /// may not be able to retrieve an assembly name.
        /// 
        /// The value for this is: "ExampleAssembly, Version=1.0.0.0, Culture=en, PublicKeyToken=a5d015c7d5a0b012"
        /// </summary>
        TestAssembly,
        /// <summary>
        /// Specifies that we want to use a null assembly name.
        /// This is only useful for unit testing purposes.
        /// </summary>
        EmptyAssembly
    }
}
#endif