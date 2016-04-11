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

namespace NLog
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using NLog.Internal;

    /// <summary>
    /// Nested Diagnostics Context - a thread-local structure that keeps a stack
    /// of strings and provides methods to output them in layouts
    /// Mostly for compatibility with log4net.
    /// </summary>
    public static class NestedDiagnosticsContext
    {
        private static readonly object dataSlot = ThreadLocalStorageHelper.AllocateDataSlot();

        /// <summary>
        /// Gets the top NDC message but doesn't remove it.
        /// </summary>
        /// <returns>The top message. .</returns>
        public static string TopMessage
        {
            get
            {
                return FormatHelper.ConvertToString(TopObject, null);
            }
        }

        /// <summary>
        /// Gets the top NDC object but doesn't remove it.
        /// </summary>
        /// <returns>The object at the top of the NDC stack if defined; otherwise <c>null</c>.</returns>
        public static object TopObject
        {
            get 
            {
                Stack<object> stack = ThreadStack;
                return (stack.Count > 0) ? stack.Peek() : null;
            }
        }

        private static Stack<object> ThreadStack
        {
            get { return ThreadLocalStorageHelper.GetDataForSlot<Stack<object>>(dataSlot); }
        }

        /// <summary>
        /// Pushes the specified text on current thread NDC.
        /// </summary>
        /// <param name="text">The text to be pushed.</param>
        /// <returns>An instance of the object that implements IDisposable that returns the stack to the previous level when IDisposable.Dispose() is called. To be used with C# using() statement.</returns>
        public static IDisposable Push(string text)
        {
            return Push((object)text);
        }

        /// <summary>
        /// Pushes the specified object on current thread NDC.
        /// </summary>
        /// <param name="value">The object to be pushed.</param>
        /// <returns>An instance of the object that implements IDisposable that returns the stack to the previous level when IDisposable.Dispose() is called. To be used with C# using() statement.</returns>
        public static IDisposable Push(object value)
        {
            Stack<object> stack = ThreadStack;
            int previousCount = stack.Count;
            stack.Push(value);
            return new StackPopper(stack, previousCount);
        }

        /// <summary>
        /// Pops the top message off the NDC stack.
        /// </summary>
        /// <returns>The top message which is no longer on the stack.</returns>
        public static string Pop()
        {
            return Pop(null);
        }

        /// <summary>
        /// Pops the top message from the NDC stack.
        /// </summary>
        /// <param name="formatProvider">The <see cref="IFormatProvider"/> to use when converting the value to a string.</param>
        /// <returns>The top message, which is removed from the stack, as a string value.</returns>
        public static string Pop(IFormatProvider formatProvider)
        {
            return FormatHelper.ConvertToString(PopObject(), formatProvider);
        }

        /// <summary>
        /// Pops the top object off the NDC stack.
        /// </summary>
        /// <returns>The object from the top of the NDC stack, if defined; otherwise <c>null</c>.</returns>
        public static object PopObject()
        {
            Stack<object> stack = ThreadStack;
            return (stack.Count > 0) ? stack.Pop() : null;
        }

        /// <summary>
        /// Clears current thread NDC stack.
        /// </summary>
        public static void Clear()
        {
            ThreadStack.Clear();
        }

        /// <summary>
        /// Gets all messages on the stack.
        /// </summary>
        /// <returns>Array of strings on the stack.</returns>
        public static string[] GetAllMessages()
        {
            return GetAllMessages(null);
        }

        /// <summary>
        /// Gets all messages from the stack, without removing them.
        /// </summary>
        /// <param name="formatProvider">The <see cref="IFormatProvider"/> to use when converting a value to a string.</param>
        /// <returns>Array of strings.</returns>
        public static string[] GetAllMessages(IFormatProvider formatProvider) 
        {
            return ThreadStack.Select((o) => FormatHelper.ConvertToString(o, formatProvider)).ToArray();
        }

        /// <summary>
        /// Gets all objects on the stack.
        /// </summary>
        /// <returns>Array of objects on the stack.</returns>
        public static object[] GetAllObjects() 
        {
            return ThreadStack.ToArray();
        }

        /// <summary>
        /// Resets the stack to the original count during <see cref="IDisposable.Dispose"/>.
        /// </summary>
        private class StackPopper : IDisposable
        {
            private Stack<object> stack;
            private int previousCount;

            /// <summary>
            /// Initializes a new instance of the <see cref="StackPopper" /> class.
            /// </summary>
            /// <param name="stack">The stack.</param>
            /// <param name="previousCount">The previous count.</param>
            public StackPopper(Stack<object> stack, int previousCount)
            {
                this.stack = stack;
                this.previousCount = previousCount;
            }

            /// <summary>
            /// Reverts the stack to original item count.
            /// </summary>
            void IDisposable.Dispose()
            {
                while (this.stack.Count > this.previousCount)
                {
                    this.stack.Pop();
                }
            }
        }
    }
}
