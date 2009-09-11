using System.Collections.Generic;
using System.Reflection;

namespace NLog.Config
{
    /// <summary>
    /// Represents a factory of named items (such as targets, layouts, layout renderers, etc.).
    /// </summary>
    /// <typeparam name="TInstanceType">Base type for each item instance.</typeparam>
    /// <typeparam name="TDefinitionType">Item definition type (typically <see cref="System.Type"/> or <see cref="MethodInfo"/>).</typeparam>
    public interface INamedItemFactory<TInstanceType, TDefinitionType>
        where TInstanceType : class
    {
        /// <summary>
        /// Gets a collection of all registered items in the factory.
        /// </summary>
        /// <returns>Sequence of key/value pairs where each key represents the name 
        /// of the item and value is the <typeparamref name="TDefinitionType"/> of
        /// the item.</returns>
        IDictionary<string, TDefinitionType> AllRegisteredItems { get; }

        /// <summary>
        /// Registers new item definition.
        /// </summary>
        /// <param name="itemName">Name of the item.</param>
        /// <param name="itemDefinition">Item definition.</param>
        void RegisterDefinition(string itemName, TDefinitionType itemDefinition);

        /// <summary>
        /// Tries to get registed item definition.
        /// </summary>
        /// <param name="itemName">Name of the item.</param>
        /// <param name="result">Reference to a variable which will store the item definition.</param>
        /// <returns>Item definition.</returns>
        bool TryGetDefinition(string itemName, out TDefinitionType result);

        /// <summary>
        /// Creates item instance.
        /// </summary>
        /// <param name="itemName">Name of the item.</param>
        /// <returns>Newly created item instance.</returns>
        TInstanceType CreateInstance(string itemName);

        /// <summary>
        /// Tries to create an item instance.
        /// </summary>
        /// <param name="itemName">Name of the item.</param>
        /// <param name="result">The result.</param>
        /// <returns>True if instance was created successfully, false otherwise.</returns>
        bool TryCreateInstance(string itemName, out TInstanceType result);
    }
}
