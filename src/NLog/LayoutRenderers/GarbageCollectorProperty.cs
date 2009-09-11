namespace NLog.LayoutRenderers
{
    /// <summary>
    /// Gets or sets the property of System.GC to retrieve.
    /// </summary>
    public enum GarbageCollectorProperty
    {
        /// <summary>
        /// Total memory allocated.
        /// </summary>
        TotalMemory,

        /// <summary>
        /// Total memory allocated (perform full garbage collection first).
        /// </summary>
        TotalMemoryForceCollection,

        /// <summary>
        /// Gets the number of Gen0 collections.
        /// </summary>
        CollectionCount0,

        /// <summary>
        /// Gets the number of Gen1 collections.
        /// </summary>
        CollectionCount1,

        /// <summary>
        /// Gets the number of Gen2 collections.
        /// </summary>
        CollectionCount2,

        /// <summary>
        /// Maximum generation number supported by GC.
        /// </summary>
        MaxGeneration,
    }

}
