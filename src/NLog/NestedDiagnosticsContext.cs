// 
// Copyright (c) 2004-2011 Jaroslaw Kowalski <jaak@jkowalski.net>
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
                Stack<string> stack = ThreadStack;
                if (stack.Count > 0)
                {
                    return stack.Peek();
                }
                else
                {
                    return String.Empty;
                }
            }
        }

        private static Stack<string> ThreadStack
        {
            get { return ThreadLocalStorageHelper.GetDataForSlot<Stack<string>>(dataSlot); }
        }

        /// <summary>
        /// Pushes the specified text on current thread NDC.
        /// </summary>
        /// <param name="text">The text to be pushed.</param>
        /// <returns>An instance of the object that implements IDisposable that returns the stack to the previous level when IDisposable.Dispose() is called. To be used with C# using() statement.</returns>
        public static IDisposable Push(string text)
        {
            Stack<string> stack = ThreadStack;
            int previousCount = stack.Count;
            stack.Push(text);
            return new StackPopper(stack, previousCount);
        }

        /// <summary>
        /// Pops the top message off the NDC stack.
        /// </summary>
        /// <returns>The top message which is no longer on the stack.</returns>
        public static string Pop()
        {
            Stack<string> stack = ThreadStack;
            if (stack.Count > 0)
            {
                return stack.Pop();
            }
            else
            {
                return String.Empty;
            }
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
            return ThreadStack.ToArray();
        }

        /// <summary>
        /// Resets the stack to the original count during <see cref="IDisposable.Dispose"/>.
        /// </summary>
        private class StackPopper : IDisposable
        {
            private Stack<string> stack;
            private int previousCount;

            /// <summary>
            /// Initializes a new instance of the <see cref="StackPopper" /> class.
            /// </summary>
            /// <param name="stack">The stack.</param>
            /// <param name="previousCount">The previous count.</param>
            public StackPopper(Stack<string> stack, int previousCount)
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
