// 
// Copyright (c) 2004 Jaroslaw Kowalski <jkowalski@users.sourceforge.net>
// 
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
// * Neither the name of the Jaroslaw Kowalski nor the names of its 
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
using System.Diagnostics;

using NLog.Filters;
using NLog.Targets;
using NLog.Internal;

namespace NLog
{
    sealed class LoggerImpl
    {
        private const int STACK_TRACE_SKIP_METHODS = 3;

        internal static void Write(Logger logger, LogLevel level, TargetWithFilterChain targets, IFormatProvider formatProvider, string message, object[]args, Exception exception)
        {
            if (targets == null)
                return;

            LogEventInfo logMessage = new LogEventInfo(DateTime.Now, level, logger.Name, formatProvider, message, args, exception);

#if !NETCF            
            bool needTrace = false;
            bool needTraceSources = false;

            for (TargetWithFilterChain awf = targets; awf != null; awf = awf.Next)
            {
                // once we know we needTraceSources there's nothing more to look for
                //
                if (needTraceSources)
                    break;
                Target app = awf.Target;

                int nst = app.NeedsStackTrace();

                if (nst > 0)
                {
                    needTrace = true;
                }
                if (nst > 1)
                {
                    needTraceSources = true;
                    break;
                }

                FilterCollection filterChain = awf.FilterChain;

                for (int i = 0; i < filterChain.Count; ++i)
                {
                    Filter filter = filterChain[i];

                    nst = filter.NeedsStackTrace();

                    if (nst > 0)
                    {
                        needTrace = true;
                    }
                    if (nst > 1)
                    {
                        needTraceSources = true;
                    }
                }
            }

            StackTrace stackTrace = null;
            if (needTrace)
            {
                int firstUserFrame = 0;
                stackTrace = new StackTrace(STACK_TRACE_SKIP_METHODS, needTraceSources);

                for (int i = 0; i < stackTrace.FrameCount; ++i)
                {
                    System.Reflection.MethodBase mb = stackTrace.GetFrame(i).GetMethod();

                    if (!mb.DeclaringType.FullName.StartsWith("NLog."))
                    {
                        firstUserFrame = i;
                        break;
                    }
                    else
                    {
                        // Console.WriteLine("skipping stack frame: " + mb);
                    }
                }
                logMessage.SetStackTrace(stackTrace, firstUserFrame);
            }
#endif 
            for (TargetWithFilterChain awf = targets; awf != null; awf = awf.Next)
            {
                Target app = awf.Target;

                try
                {
                    FilterCollection filterChain = awf.FilterChain;
                    FilterResult result = FilterResult.Neutral;

                    for (int i = 0; i < filterChain.Count; ++i)
                    {
                        Filter f = filterChain[i];
                        result = f.Check(logMessage);
                        if (result != FilterResult.Neutral)
                            break;
                    }
                    if (result == FilterResult.Ignore)
                    {
                        if (InternalLogger.IsDebugEnabled)
                        {
                            InternalLogger.Debug("{0}.{1} Rejecting message because of a filter.", logger.Name, level);
                        }
                        continue;
                    }
                }
                catch (Exception ex)
                {
                    InternalLogger.Error("FilterChain exception: {0}", ex);
                    if (LogManager.ThrowExceptions)
                        throw;
                    else
                        continue;
                }

                try
                {
                    app.Append(logMessage);
                }
                catch (Exception ex)
                {
                    InternalLogger.Error("Target exception: {0}", ex);
                    if (LogManager.ThrowExceptions)
                        throw;
                    else
                        continue;
                }
            }
        }
    }
}
