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

        public static StackTraceUsage GetStackTraceUsage(bool includeFileName, int skipFrames, bool captureStackTrace)
        {
            if (!captureStackTrace)
            {
                return StackTraceUsage.None;
            }

            if (skipFrames != 0)
            {
                return includeFileName ? StackTraceUsage.Max : StackTraceUsage.WithStackTrace;
            }

            if (includeFileName)
            {
                return StackTraceUsage.WithCallSite | StackTraceUsage.WithFileNameAndLineNumber;
            }

            return StackTraceUsage.WithCallSite;
        }

        public static int GetFrameCount(this StackTrace strackTrace)
        {
            return strackTrace.FrameCount;
        }

        public static string GetStackFrameMethodName(MethodBase method, bool includeMethodInfo, bool cleanAsyncMoveNext, bool cleanAnonymousDelegates)
        {
            if (method is null)
                return string.Empty;

            string methodName = method.Name;

            if (cleanAsyncMoveNext && string.Equals(methodName, "MoveNext", StringComparison.Ordinal))
            {
                var callerClassType = GetAsyncStateMachineClassType(method.DeclaringType);
                if (callerClassType != null)
                {
                    // NLog.UnitTests.LayoutRenderers.CallSiteTests+<CleanNamesOfAsyncContinuations>d_3'1.MoveNext
                    int endIndex = callerClassType.Name.IndexOf('>', 1);
                    if (endIndex > 1)
                    {
                        methodName = callerClassType.Name.Substring(1, endIndex - 1);
                        if (methodName.IndexOf('<') == 0)
                            methodName = methodName.Substring(1);    // Local functions, and anonymous-methods in Task.Run()
                    }
                }
            }

            // Clean up compiler-generated anonymous delegate names:
            // <.ctor>b__0
            // <Main>b__2
            if (cleanAnonymousDelegates && methodName.Length > 0 && methodName[0] == '<')
            {
                int endIndex = methodName.IndexOf('>');
                if (endIndex > 1 && methodName.IndexOf("__", endIndex, StringComparison.Ordinal) >= 0)
                {
                    methodName = methodName.Substring(1, endIndex - 1);
                }
            }

            if (includeMethodInfo && string.Equals(methodName, method.Name, StringComparison.Ordinal))
            {
                methodName = method.ToString() ?? methodName;
            }

            return methodName;
        }

        public static string GetStackFrameMethodClassName(MethodBase method, bool includeNameSpace, bool cleanAsyncMoveNext, bool cleanAnonymousDelegates)
        {
            if (method is null)
                return string.Empty;

            var callerClassType = method.DeclaringType;
            if (cleanAsyncMoveNext && string.Equals(method.Name, "MoveNext", StringComparison.Ordinal))
            {
                callerClassType = GetAsyncStateMachineClassType(callerClassType)?.DeclaringType ?? callerClassType;
            }

            if (callerClassType is null)
                return string.Empty;

            string className = includeNameSpace ? callerClassType.FullName : callerClassType.Name;
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
                        className = className.Substring(0, index);
                }
            }

            if (includeNameSpace && className?.IndexOf('.') == -1)
            {
                var typeNamespace = GetNamespaceFromTypeAssembly(callerClassType);
                className = string.IsNullOrEmpty(typeNamespace) ? className : string.Concat(typeNamespace, ".", className);
            }

            return className ?? string.Empty;
        }

        private static Type? GetAsyncStateMachineClassType(Type callerClassType)
        {
            if (callerClassType is null || callerClassType.DeclaringType is null)
                return null;

            string name = callerClassType.Name;
            if (string.IsNullOrEmpty(name) || name[0] != '<' || name.IndexOf('>', 1) <= 1)
                return null;

            return callerClassType;
        }

        private static string GetNamespaceFromTypeAssembly(Type? callerClassType)
        {
            var classAssembly = callerClassType?.Assembly;
            if (classAssembly != null && classAssembly != mscorlibAssembly && classAssembly != systemAssembly)
            {
                var assemblyFullName = classAssembly.FullName;
                if (assemblyFullName?.IndexOf(',') >= 0)
                {
                     if (!assemblyFullName.StartsWith("System.", StringComparison.Ordinal)
                      && !assemblyFullName.StartsWith("Microsoft.", StringComparison.Ordinal))
                        return assemblyFullName.Substring(0, assemblyFullName.IndexOf(','));
                }
            }

            return string.Empty;
        }

        [System.Diagnostics.CodeAnalysis.UnconditionalSuppressMessage("Trimming - Allow callsite logic", "IL2026")]
        public static MethodBase? GetStackMethod(StackFrame? stackFrame)
        {
            return stackFrame?.GetMethod();
        }

        /// <summary>
        /// Gets the fully qualified name of the class invoking the calling method, including the
        /// namespace but not the assembly.
        /// </summary>
        /// <param name="stackFrame">StackFrame from the calling method</param>
        /// <returns>Fully qualified class name</returns>
        public static string GetClassFullName(StackFrame? stackFrame)
        {
            string className = LookupClassNameFromStackFrame(stackFrame);
            if (string.IsNullOrEmpty(className))
            {
                var stackTrace = new StackTrace(false);
                className = GetClassFullName(stackTrace);
                if (string.IsNullOrEmpty(className))
                {
                    var method = StackTraceUsageUtils.GetStackMethod(stackFrame);
                    className = method?.Name ?? string.Empty;
                }
            }
            return className;
        }

        private static string GetClassFullName(StackTrace stackTrace)
        {
            var stackFrames = stackTrace.GetFrames();
            if (stackFrames?.Length > 0)
            {
                foreach (StackFrame frame in stackFrames)
                {
                    string className = LookupClassNameFromStackFrame(frame);
                    if (!string.IsNullOrEmpty(className))
                        return className;
                }
            }
            return string.Empty;
        }

        /// <summary>
        /// Returns the assembly from the provided StackFrame (If not internal assembly)
        /// </summary>
        /// <returns>Valid assembly, or null if assembly was internal</returns>
        public static Assembly? LookupAssemblyFromMethod(MethodBase method)
        {
            var assembly = method.DeclaringType?.Assembly ?? method.Module?.Assembly;

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

        /// <summary>
        /// Returns the classname from the provided StackFrame (If not from internal assembly)
        /// </summary>
        /// <param name="stackFrame"></param>
        /// <returns>Valid class name, or empty string if assembly was internal</returns>
        public static string LookupClassNameFromStackFrame(StackFrame? stackFrame)
        {
            var method = GetStackMethod(stackFrame);
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
    }
}
