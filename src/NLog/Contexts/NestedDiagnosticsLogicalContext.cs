// 
// Copyright (c) 2004-2020 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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


namespace NLog
{
    using System;
    using System.Linq;
    using NLog.Internal;

    /// <summary>
    /// Async version of <see cref="NestedDiagnosticsContext" /> - a logical context structure that keeps a stack
    /// Allows for maintaining scope across asynchronous tasks and call contexts.
    /// </summary>
    [Obsolete("Replaced by ScopeContext.PushNestedState or Logger.PushScopeState using ${scopenested}. Marked obsolete on NLog 5.0")]
    public static class NestedDiagnosticsLogicalContext
    {
        /// <summary>
        /// Pushes the specified value on current stack
        /// </summary>
        /// <param name="value">The value to be pushed.</param>
        /// <returns>An instance of the object that implements IDisposable that returns the stack to the previous level when IDisposable.Dispose() is called. To be used with C# using() statement.</returns>
        [Obsolete("Replaced by ScopeContext.PushNestedState or Logger.PushScopeState using ${scopenested}. Marked obsolete on NLog 5.0")]
        public static IDisposable Push<T>(T value)
        {
            return ScopeContext.PushNestedState(value);
        }

        /// <summary>
        /// Pushes the specified value on current stack
        /// </summary>
        /// <param name="value">The value to be pushed.</param>
        /// <returns>An instance of the object that implements IDisposable that returns the stack to the previous level when IDisposable.Dispose() is called. To be used with C# using() statement.</returns>
        [Obsolete("Replaced by ScopeContext.PushNestedState or Logger.PushScopeState using ${scopenested}. Marked obsolete on NLog 5.0")]
        public static IDisposable PushObject(object value)
        {
            return Push(value);
        }

        /// <summary>
        /// Pops the top message off the NDLC stack.
        /// </summary>
        /// <returns>The top message which is no longer on the stack.</returns>
        /// <remarks>this methods returns a object instead of string, this because of backwards-compatibility</remarks>
        [Obsolete("Replaced by dispose of return value from ScopeContext.PushNestedState or Logger.PushScopeState. Marked obsolete on NLog 5.0")]
        public static object Pop()
        {
            return PopObject();
        }

        /// <summary>
        /// Pops the top message from the NDLC stack.
        /// </summary>
        /// <param name="formatProvider">The <see cref="IFormatProvider"/> to use when converting the value to a string.</param>
        /// <returns>The top message, which is removed from the stack, as a string value.</returns>
        [Obsolete("Replaced by dispose of return value from ScopeContext.PushNestedState or Logger.PushScopeState. Marked obsolete on NLog 5.0")]
        public static string Pop(IFormatProvider formatProvider)
        {
            return FormatHelper.ConvertToString(PopObject() ?? string.Empty, formatProvider);
        }

        /// <summary>
        /// Pops the top message off the current NDLC stack
        /// </summary>
        /// <returns>The object from the top of the NDLC stack, if defined; otherwise <c>null</c>.</returns>
        [Obsolete("Replaced by dispose of return value from ScopeContext.PushNestedState or Logger.PushScopeState. Marked obsolete on NLog 5.0")]
        public static object PopObject()
        {
            return ScopeContext.PopNestedContextLegacy();
        }

        /// <summary>
        /// Peeks the top object on the current NDLC stack
        /// </summary>
        /// <returns>The object from the top of the NDLC stack, if defined; otherwise <c>null</c>.</returns>
        [Obsolete("Replaced by ScopeContext.PeekNestedState. Marked obsolete on NLog 5.0")]
        public static object PeekObject()
        {
            return ScopeContext.PeekNestedState();
        }

        /// <summary>
        /// Clears current stack.
        /// </summary>
        [Obsolete("Replaced by ScopeContext.Clear. Marked obsolete on NLog 5.0")]
        public static void Clear()
        {
            ScopeContext.ClearNestedContextLegacy();
        }

        /// <summary>
        /// Gets all messages on the stack.
        /// </summary>
        /// <returns>Array of strings on the stack.</returns>
        [Obsolete("Replaced by ScopeContext.GetAllNestedStates. Marked obsolete on NLog 5.0")]
        public static string[] GetAllMessages()
        {
            return GetAllMessages(null);
        }

        /// <summary>
        /// Gets all messages from the stack, without removing them.
        /// </summary>
        /// <param name="formatProvider">The <see cref="IFormatProvider"/> to use when converting a value to a string.</param>
        /// <returns>Array of strings.</returns>
        [Obsolete("Replaced by ScopeContext.GetAllNestedStates. Marked obsolete on NLog 5.0")]
        public static string[] GetAllMessages(IFormatProvider formatProvider)
        {
            return GetAllObjects().Select((o) => FormatHelper.ConvertToString(o, formatProvider)).ToArray();
        }

        /// <summary>
        /// Gets all objects on the stack. The objects are not removed from the stack.
        /// </summary>
        /// <returns>Array of objects on the stack.</returns>
        [Obsolete("Replaced by ScopeContext.GetAllNestedStates. Marked obsolete on NLog 5.0")]
        public static object[] GetAllObjects()
        {
            return ScopeContext.GetAllNestedStates();
        }

        [Obsolete("Required to be compatible with legacy NLog versions, when using remoting. Marked obsolete on NLog 5.0")]
        interface INestedContext : IDisposable
        {
            INestedContext Parent { get; }
            int FrameLevel { get; }
            object Value { get; }
            long CreatedTimeUtcTicks { get; }
        }

#if !NETSTANDARD1_3 && !NETSTANDARD1_5
        [Serializable]
#endif
        [Obsolete("Required to be compatible with legacy NLog versions, when using remoting. Marked obsolete on NLog 5.0")]
        sealed class NestedContext<T> : INestedContext
        {
            public INestedContext Parent { get; }
            public T Value { get; }
            public long CreatedTimeUtcTicks { get; }
            public int FrameLevel { get; }
            private int _disposed;

            object INestedContext.Value
            {
                get
                {
                    object value = Value;
#if NET35 || NET40 || NET45
                    if (value is ObjectHandleSerializer objectHandle)
                    {
                        return objectHandle.Unwrap();
                    }
#endif
                    return value;
                }
            }

            public NestedContext(INestedContext parent, T value)
            {
                Parent = parent;
                Value = value;
                CreatedTimeUtcTicks = DateTime.UtcNow.Ticks; // Low time resolution, but okay fast
                FrameLevel = parent?.FrameLevel + 1 ?? 1;
            }

            void IDisposable.Dispose()
            {
                if (System.Threading.Interlocked.Exchange(ref _disposed, 1) != 1)
                {
                    PopObject();
                }
            }

            public override string ToString()
            {
                object value = Value;
                return value?.ToString() ?? "null";
            }
        }
    }
}
