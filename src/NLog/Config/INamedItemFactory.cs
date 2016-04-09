// 
// Copyright (c) 2004-2016 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
// 
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without 
// modification, are permitted provided that the following conditions 
// are met:
// 
// * Redistributions of source code must retain the above copyright notice, 
//   this list of conditions and the following disclaimer. 
// 
// * Redistributions in binary form must reproduce the above copyright notice,
//   this list of conditions and the following disclaimer in the documentation
//   and/or other materials provided with the distribution. 
// 
// * Neither the name of Jaroslaw Kowalski nor the names of its 
//   contributors may be used to endorse or promote products derived from this
//   software without specific prior written permission. 
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE 
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE 
// ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE 
// LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR 
// CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
// SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS 
// INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN 
// CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) 
// ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF 
// THE POSSIBILITY OF SUCH DAMAGE.
// 

namespace NLog.Config
{
    using System.Collections.Generic;
    using System.Reflection;

    /// <summary>
    /// Represents a factory of named items (such as targets, layouts, layout renderers, etc.).
    /// </summary>
    /// <typeparam name="TInstanceType">Base type for each item instance.</typeparam>
    /// <typeparam name="TDefinitionType">Item definition type (typically <see cref="System.Type"/> or <see cref="MethodInfo"/>).</typeparam>
    public interface INamedItemFactory<TInstanceType, TDefinitionType>
        where TInstanceType : class
    {
        /// <summary>
        /// Registers new item definition.
        /// </summary>
        /// <param name="itemName">Name of the item.</param>
        /// <param name="itemDefinition">Item definition.</param>
        void RegisterDefinition(string itemName, TDefinitionType itemDefinition);

        /// <summary>
        /// Tries to get registered item definition.
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
