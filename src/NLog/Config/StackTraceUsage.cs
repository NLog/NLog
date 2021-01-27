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

namespace NLog.Config
{
    using System;

    /// <summary>
    /// Value indicating how stack trace should be captured when processing the log event.
    /// </summary>
    [Flags]
    public enum StackTraceUsage
    {
        /// <summary>
        /// No Stack trace needs to be captured.
        /// </summary>
        None = 0,

        /// <summary>
        /// Stack trace should be captured. This option won't add the filenames and linenumbers
        /// </summary>
        WithStackTrace = 1,

        /// <summary>
        /// Capture also filenames and linenumbers
        /// </summary>
        WithFileNameAndLineNumber = 2,

        /// <summary>
        /// Capture the location of the call
        /// </summary>
        WithCallSite = 4,

        /// <summary>
        /// Capture the class name for location of the call
        /// </summary>
        WithCallSiteClassName = 8,

        /// <summary>
        /// Stack trace should be captured. This option won't add the filenames and linenumbers.
        /// </summary>
        [Obsolete("Replace with `WithStackTrace`. Marked obsolete on NLog 5.0")]
        WithoutSource = WithStackTrace,

        /// <summary>
        /// Stack trace should be captured including filenames and linenumbers.
        /// </summary>
        WithSource = WithStackTrace | WithFileNameAndLineNumber,

        /// <summary>
        /// Capture maximum amount of the stack trace information supported on the platform.
        /// </summary>
        Max = WithSource,
    }
}
