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

#if NETSTANDARD1_3 || NETSTANDARD1_5

namespace NLog.Internal
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Reflection;
    using System.Threading;

    static class NetStandardHelpers
    {
        public static void Close(this IDisposable disposable)
        {
            disposable.Dispose();
        }

#pragma warning disable S2953 // Methods named "Dispose" should implement "IDisposable.Dispose"
        public static bool Dispose(this IDisposable disposable, EventWaitHandle waitHandle)
#pragma warning restore S2953 // Methods named "Dispose" should implement "IDisposable.Dispose"
        {
            disposable.Dispose();
            return false;
        }

        public static bool IsDefined(this Type type, Type other, bool inherit)
        {
            return type.GetTypeInfo().IsDefined(other, inherit);
        }

        public static bool IsSubclassOf(this Type type, Type other)
        {
            return type.GetTypeInfo().IsSubclassOf(other);
        }

        public static MethodInfo GetMethod(this Type type, string name, BindingFlags bindingAttr, object binder, Type[] types, object[] modifiers)
        {
            if (binder != null)
                throw new ArgumentException("Not supported", nameof(binder));
            if (modifiers != null)
                throw new ArgumentException("Not supported", nameof(modifiers));

            foreach (MethodInfo method in type.GetMethods(bindingAttr))
            {
                if (method.Name != name)
                    continue;

                var parameters = method.GetParameters();
                if (parameters == null || parameters.Length != types.Length)
                    continue;

                for (int i = 0; i < parameters.Length; ++i)
                {
                    if (parameters[i].ParameterType != types[i])
                    {
                        parameters = null;
                        break;
                    }
                }

                if (parameters != null)
                {
                    return method;
                }
            }

            return null;
        }

        public static byte[] GetBuffer(this MemoryStream memoryStream)
        {
            ArraySegment<byte> bytes;
            if (memoryStream.TryGetBuffer(out bytes))
            {
                return bytes.Array;
            }
            return memoryStream.ToArray();
        }

        public static StackFrame GetFrame(this StackTrace strackTrace, int number)
        {
            return strackTrace.GetFrames()[number];
        }
    }
}

#endif