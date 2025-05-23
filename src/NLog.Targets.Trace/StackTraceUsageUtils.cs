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

namespace NLog.Internal
{
    using System;
    using System.Diagnostics;
    using System.Reflection;
    using NLog.Config;

    /// <summary>
    /// Utilities for dealing with <see cref="StackTraceUsage"/> values.
    /// </summary>
    internal static class StackTraceUsageUtils
    {
        private static readonly Assembly nlogAssembly = typeof(StackTraceUsageUtils).Assembly;
        private static readonly Assembly mscorlibAssembly = typeof(string).Assembly;
        private static readonly Assembly systemAssembly = typeof(Debug).Assembly;

        /// <summary>
        /// Returns the classname from the provided StackFrame (If not from internal assembly)
        /// </summary>
        /// <param name="stackFrame"></param>
        /// <returns>Valid class name, or empty string if assembly was internal</returns>
        public static string LookupClassNameFromStackFrame(StackFrame stackFrame)
        {
            var method = stackFrame?.GetMethod();
            if (method != null && LookupAssemblyFromMethod(method) != null)
            {
                string className = GetStackFrameMethodClassName(method, true, true, true);
                if (!string.IsNullOrEmpty(className))
                {
                    if (!className.StartsWith("System.", StringComparison.Ordinal))
                        return className;
                }
                else
                {
                    className = method.Name ?? string.Empty;
                    if (className != "lambda_method" && className != "MoveNext")
                        return className;
                }
            }

            return string.Empty;
        }

        private static string GetStackFrameMethodClassName(MethodBase method, bool includeNameSpace, bool cleanAsyncMoveNext, bool cleanAnonymousDelegates)
        {
            if (method is null)
                return string.Empty;

            var callerClassType = method.DeclaringType;
            if (cleanAsyncMoveNext
              && method.Name == "MoveNext"
              && callerClassType?.DeclaringType != null
              && callerClassType.Name?.IndexOf('<') == 0
              && callerClassType.Name.IndexOf('>', 1) > 1)
            {
                // NLog.UnitTests.LayoutRenderers.CallSiteTests+<CleanNamesOfAsyncContinuations>d_3'1
                callerClassType = callerClassType.DeclaringType;
            }

            if (callerClassType is null)
                return string.Empty;

            var className = includeNameSpace ? callerClassType.FullName : callerClassType.Name;
            if (cleanAnonymousDelegates && className?.IndexOf("<>", StringComparison.Ordinal) >= 0)
            {
                if (!includeNameSpace && callerClassType.DeclaringType != null && callerClassType.IsNested)
                {
                    className = callerClassType.DeclaringType.Name;
                }
                else
                {
                    // NLog.UnitTests.LayoutRenderers.CallSiteTests+<>c__DisplayClassa
                    int index = className.IndexOf("+<>", StringComparison.Ordinal);
                    if (index >= 0)
                    {
                        className = className.Substring(0, index);
                    }
                }
            }

            if (includeNameSpace && className?.IndexOf('.') == -1)
            {
                var typeNamespace = GetNamespaceFromTypeAssembly(callerClassType);
                className = string.IsNullOrEmpty(typeNamespace) ? className : string.Concat(typeNamespace, ".", className);
            }

            return className ?? string.Empty;
        }

        private static string GetNamespaceFromTypeAssembly(Type callerClassType)
        {
            var classAssembly = callerClassType.Assembly;
            if (classAssembly != null && classAssembly != mscorlibAssembly && classAssembly != systemAssembly)
            {
                var assemblyFullName = classAssembly.FullName;
                if (assemblyFullName?.IndexOf(',') >= 0 && !assemblyFullName.StartsWith("System.", StringComparison.Ordinal) && !assemblyFullName.StartsWith("Microsoft.", StringComparison.Ordinal))
                {
                    return assemblyFullName.Substring(0, assemblyFullName.IndexOf(','));
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// Returns the assembly from the provided StackFrame (If not internal assembly)
        /// </summary>
        /// <returns>Valid assembly, or null if assembly was internal</returns>
        private static Assembly? LookupAssemblyFromMethod(MethodBase method)
        {
            var assembly = method?.DeclaringType?.Assembly ?? method?.Module?.Assembly;

            // skip stack frame if the method declaring type assembly is from hidden assemblies list
            if (assembly == nlogAssembly)
            {
                return null;
            }

            if (assembly == mscorlibAssembly)
            {
                return null;
            }

            if (assembly == systemAssembly)
            {
                return null;
            }

            return assembly;
        }
    }
}
