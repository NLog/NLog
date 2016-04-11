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

    /// <summary>
    /// Nested Diagnostics Context - for log4net compatibility.
    /// </summary>
    [Obsolete("Use NestedDiagnosticsContext")]
    public static class NDC
    {
        /// <summary>
        /// Gets the top NDC message but doesn't remove it.
        /// </summary>
        /// <returns>The top message. .</returns>
        public static string TopMessage
        {
            get { return NestedDiagnosticsContext.TopMessage; }
        }

        /// <summary>
        /// Gets the top NDC object but doesn't remove it.
        /// </summary>
        /// <returns>The object from the top of the NDC stack, if defined; otherwise <c>null</c>.</returns>
        public static object TopObject 
        {
            get { return NestedDiagnosticsContext.TopObject; }
        }

        /// <summary>
        /// Pushes the specified text on current thread NDC.
        /// </summary>
        /// <param name="text">The text to be pushed.</param>
        /// <returns>An instance of the object that implements IDisposable that returns the stack to the previous level when IDisposable.Dispose() is called. To be used with C# using() statement.</returns>
        public static IDisposable Push(string text)
        {
            return NestedDiagnosticsContext.Push(text);
        }

        /// <summary>
        /// Pops the top message off the NDC stack.
        /// </summary>
        /// <returns>The top message which is no longer on the stack.</returns>
        public static string Pop()
        {
            return NestedDiagnosticsContext.Pop();
        }

        /// <summary>
        /// Pops the top object off the NDC stack. The object is removed from the stack.
        /// </summary>
        /// <returns>The top object from the NDC stack, if defined; otherwise <c>null</c>.</returns>
        public static object PopObject()
        {
            return NestedDiagnosticsContext.PopObject();
        }

        /// <summary>
        /// Clears current thread NDC stack.
        /// </summary>
        public static void Clear()
        {
            NestedDiagnosticsContext.Clear();
        }

        /// <summary>
        /// Gets all messages on the stack.
        /// </summary>
        /// <returns>Array of strings on the stack.</returns>
        public static string[] GetAllMessages()
        {
            return NestedDiagnosticsContext.GetAllMessages();
        }

        /// <summary>
        /// Gets all objects on the NDC stack. The objects are not removed from the stack.
        /// </summary>
        /// <returns>Array of objects on the stack.</returns>
        public static object[] GetAllObjects()
        {
            return NestedDiagnosticsContext.GetAllObjects();
        }
    }
}
