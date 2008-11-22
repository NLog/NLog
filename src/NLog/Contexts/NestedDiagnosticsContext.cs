// 
// Copyright (c) 2004-2006 Jaroslaw Kowalski <jaak@jkowalski.net>
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

using System;
using System.Collections;
using System.Collections.Specialized;
using System.Text;
using System.Collections.Generic;

namespace NLog.Contexts
{
    /// <summary>
    /// Nested Diagnostics Context - a thread-local structure that keeps a stack
    /// of strings and provides methods to output them in layouts
    /// Mostly for compatibility with log4net.
    /// </summary>
    public class NestedDiagnosticsContext
    {
        internal NestedDiagnosticsContext() { }

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
        /// Gets the top NDC message but doesn't remove it.
        /// </summary>
        /// <returns>The top message. </returns>
        public static string GetTopMessage()
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
        public static string[] GetAllMessages()
        {
            return ThreadStack.ToArray();
        }

        private class StackPopper: IDisposable
        {
            private Stack<string> _stack;
            private int _previousCount;

            public StackPopper(Stack<string> stack, int previousCount)
            {
                _stack = stack;
                _previousCount = previousCount;
            }

            void IDisposable.Dispose()
            {
                while (_stack.Count > _previousCount)
                {
                    _stack.Pop();
                }
            }
        }

#if SILVERLIGHT
        public static Stack<string> ThreadStack
        {
            get
            {
                if (_threadStack == null)
                    _threadStack = new Stack<string>();
                return _threadStack;
            }
        }

        [ThreadStatic]
        private static Stack<string> _threadStack;

#else
        private static Stack<string> ThreadStack
        {
            get
            {
                Stack<string> threadStack = (Stack<string>)System.Threading.Thread.GetData(_dataSlot);

                if (threadStack == null)
                {
                    threadStack = new Stack<string>();
                    System.Threading.Thread.SetData(_dataSlot, threadStack);
                }

                return threadStack;
            }
        }

        private static LocalDataStoreSlot _dataSlot = System.Threading.Thread.AllocateDataSlot();
#endif
    }

    /// <summary>
    /// Nested Diagnostics Context - for log4net compatibility
    /// </summary>
    [Obsolete("Use NestedDiagnosticsContext")]
    public class NDC : NestedDiagnosticsContext
    {
    }
}
