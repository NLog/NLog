using System;

namespace NLog.Targets
{
    /// <summary>
    /// Interface for serialization of values, maybe even objects to compact JSON format. 
    /// Useful for wrappers for existing serializers.
    /// </summary>
    public interface ICompactJsonSerializer
    {
        /// <summary>
        /// Returns a serialization of an object
        /// int compact JSON format.
        /// </summary>
        /// <param name="value">The object to serialize to compact JSON.</param>
        string SerializeValue(object value);
    }
}
