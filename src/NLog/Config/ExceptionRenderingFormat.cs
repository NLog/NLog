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


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NLog.Config
{
    /// <summary>
    /// Format of the excpetion output to the specific target.
    /// </summary>
    public enum ExceptionRenderingFormat
    {
        /// <summary>
        /// Appends the Message of an Exception to the specified target.
        /// </summary>
        Message = 0,
        /// <summary>
        /// Appends the type of an Exception to the specified target.
        /// </summary>
        Type = 1,
        /// <summary>
        /// Appends the short type of an Exception to the specified target.
        /// </summary>
        ShortType = 2,
        /// <summary>
        /// Appends the result of calling ToString() on an Exception to the specified target.
        /// </summary>
        ToString = 3,
        /// <summary>
        /// Appends the method name from Exception's stack trace to the specified target.
        /// </summary>
        Method = 4,
        /// <summary>
        /// Appends the stack trace from an Exception to the specified target.
        /// </summary>
        StackTrace = 5,
        /// <summary>
        /// Appends the contents of an Exception's Data property to the specified target.
        /// </summary>
        Data = 6
    }
}
