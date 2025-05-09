//
// Copyright (c) 2004-2024 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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

#nullable enable

namespace NLog
{
    using System.Diagnostics;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Specialized LogFactory that can return instances of custom logger types.
    /// </summary>
    /// <remarks>Use this only when a custom Logger type is defined.</remarks>
    /// <typeparam name="T">The type of the logger to be returned. Must inherit from <see cref="Logger"/>.</typeparam>
    public class LogFactory<T> : LogFactory
        where T : Logger, new()
    {
        /// <summary>
        /// Gets the logger with type <typeparamref name="T"/>.
        /// </summary>
        /// <param name="name">The logger name.</param>
        /// <returns>An instance of <typeparamref name="T"/>.</returns>
        public new T GetLogger(string name)
        {
            return GetLogger<T>(name);
        }

        /// <summary>
        /// Gets a custom logger with the full name of the current class (so namespace and class name) and type <typeparamref name="T"/>.
        /// </summary>
        /// <returns>An instance of <typeparamref name="T"/>.</returns>
        /// <remarks>This is a slow-running method.
        /// Make sure you're not doing this in a loop.</remarks>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public new T GetCurrentClassLogger()
        {
            var className = Internal.StackTraceUsageUtils.GetClassFullName(new StackFrame(1, false));
            return GetLogger(className);
        }
    }
}
