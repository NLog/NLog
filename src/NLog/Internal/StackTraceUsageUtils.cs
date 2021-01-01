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

namespace NLog.Internal
{
    using System;
    using System.Diagnostics;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using NLog.Config;

    /// <summary>
    /// Utilities for dealing with <see cref="StackTraceUsage"/> values.
    /// </summary>
    internal static class StackTraceUsageUtils
    {
        private static readonly Assembly nlogAssembly = typeof(StackTraceUsageUtils).GetAssembly();
        private static readonly Assembly mscorlibAssembly = typeof(string).GetAssembly();
        private static readonly Assembly systemAssembly = typeof(Debug).GetAssembly();

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
#if !NETSTANDARD1_3 && !NETSTANDARD1_5
            return strackTrace.FrameCount;
#else
            return strackTrace.GetFrames().Length;
#endif
        }

        public static string GetStackFrameMethodName(MethodBase method, bool includeMethodInfo, bool cleanAsyncMoveNext, bool cleanAnonymousDelegates)
        {
            if (method == null)
                return null;

            string methodName = method.Name;

            var callerClassType = method.DeclaringType;
            if (cleanAsyncMoveNext && methodName == "MoveNext" && callerClassType?.DeclaringType != null && callerClassType.Name.StartsWith("<"))
            {
                // NLog.UnitTests.LayoutRenderers.CallSiteTests+<CleanNamesOfAsyncContinuations>d_3'1.MoveNext
                int endIndex = callerClassType.Name.IndexOf('>', 1);
                if (endIndex > 1)
                {
                    methodName = callerClassType.Name.Substring(1, endIndex - 1);
                    if (methodName.StartsWith("<"))
                        methodName = methodName.Substring(1, methodName.Length - 1);    // Local functions, and anonymous-methods in Task.Run()
                }
            }

            // Clean up the function name if it is an anonymous delegate
            // <.ctor>b__0
            // <Main>b__2
            if (cleanAnonymousDelegates && (methodName.StartsWith("<") && methodName.Contains("__") && methodName.Contains(">")))
            {
                int startIndex = methodName.IndexOf('<') + 1;
                int endIndex = methodName.IndexOf('>');

                methodName = methodName.Substring(startIndex, endIndex - startIndex);
            }

            if (includeMethodInfo && methodName == method.Name)
            {
                methodName = method.ToString();
            }

            return methodName;
        }

        public static string GetStackFrameMethodClassName(MethodBase method, bool includeNameSpace, bool cleanAsyncMoveNext, bool cleanAnonymousDelegates)
        {
            if (method == null)
                return null;

            var callerClassType = method.DeclaringType;
            if (cleanAsyncMoveNext && method.Name == "MoveNext" && callerClassType?.DeclaringType != null && callerClassType.Name.StartsWith("<"))
            {
                // NLog.UnitTests.LayoutRenderers.CallSiteTests+<CleanNamesOfAsyncContinuations>d_3'1
                int endIndex = callerClassType.Name.IndexOf('>', 1);
                if (endIndex > 1)
                {
                    callerClassType = callerClassType.DeclaringType;
                }
            }

            if (!includeNameSpace
                && callerClassType?.DeclaringType != null
                && callerClassType.IsNested
                && callerClassType.GetFirstCustomAttribute<CompilerGeneratedAttribute>() != null)
            {
                return callerClassType.DeclaringType.Name;
            }

            string className = includeNameSpace ? callerClassType?.FullName : callerClassType?.Name;

            if (cleanAnonymousDelegates && className != null)
            {
                // NLog.UnitTests.LayoutRenderers.CallSiteTests+<>c__DisplayClassa
                int index = className.IndexOf("+<>", StringComparison.Ordinal);
                if (index >= 0)
                {
                    className = className.Substring(0, index);
                }
            }

            return className;
        }

        /// <summary>
        /// Gets the fully qualified name of the class invoking the calling method, including the 
        /// namespace but not the assembly.    
        /// </summary>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static string GetClassFullName()
        {
            int framesToSkip = 2;

            string className = string.Empty;
#if !NETSTANDARD1_3 && !NETSTANDARD1_5
            var stackFrame = new StackFrame(framesToSkip, false);
            className = GetClassFullName(stackFrame);
#else
            var stackTrace = Environment.StackTrace;
            var stackTraceLines = stackTrace.Replace("\r", "").SplitAndTrimTokens('\n');
            for (int i = 0; i < stackTraceLines.Length; ++i)
            {
                var callingClassAndMethod = stackTraceLines[i].Split(new[] { " ", "<>", "(", ")" }, StringSplitOptions.RemoveEmptyEntries)[1];
                int methodStartIndex = callingClassAndMethod.LastIndexOf(".", StringComparison.Ordinal);
                if (methodStartIndex > 0)
                {
                    // Trim method name. 
                    var callingClass = callingClassAndMethod.Substring(0, methodStartIndex);
                    // Needed because of extra dot, for example if method was .ctor()
                    className = callingClass.TrimEnd('.');
                    if (!className.StartsWith("System.Environment") && framesToSkip != 0)
                    {
                        i += framesToSkip - 1;
                        framesToSkip = 0;
                        continue;
                    }
                    if (!className.StartsWith("System."))
                        break;
                }
            }
#endif
            return className;
        }

#if !NETSTANDARD1_3 && !NETSTANDARD1_5
        /// <summary>
        /// Gets the fully qualified name of the class invoking the calling method, including the 
        /// namespace but not the assembly.
        /// </summary>
        /// <param name="stackFrame">StackFrame from the calling method</param>
        /// <returns>Fully qualified class name</returns>
        public static string GetClassFullName(StackFrame stackFrame)
        {
            string className = LookupClassNameFromStackFrame(stackFrame);
            if (string.IsNullOrEmpty(className))
            {
                var stackTrace = new StackTrace(false);
                className = GetClassFullName(stackTrace);
                if (string.IsNullOrEmpty(className))
                    className = stackFrame.GetMethod()?.Name ?? string.Empty;
            }
            return className;
        }
#endif

        private static string GetClassFullName(StackTrace stackTrace)
        {
            foreach (StackFrame frame in stackTrace.GetFrames())
            {
                string className = LookupClassNameFromStackFrame(frame);
                if (!string.IsNullOrEmpty(className))
                {
                    return className;
                }
            }
            return string.Empty;
        }

        /// <summary>
        /// Returns the assembly from the provided StackFrame (If not internal assembly)
        /// </summary>
        /// <returns>Valid assembly, or null if assembly was internal</returns>
        public static Assembly LookupAssemblyFromStackFrame(StackFrame stackFrame)
        {
            var method = stackFrame.GetMethod();
            if (method == null)
            {
                return null;
            }

            var assembly = method.DeclaringType?.GetAssembly() ?? method.Module?.Assembly;
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
        public static string LookupClassNameFromStackFrame(StackFrame stackFrame)
        {
            var method = stackFrame.GetMethod();
            if (method != null && LookupAssemblyFromStackFrame(stackFrame) != null)
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
