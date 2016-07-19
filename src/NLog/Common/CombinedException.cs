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
using System.Runtime.Serialization;
using System.Text;

using NLog.Internal;

namespace NLog.Common
{
    /// <summary>
    /// Represents one or more exceptions thrown combined into one.
    /// </summary>
#if !SILVERLIGHT
    [Serializable]
#endif
    public class CombinedException : Exception
    {
        private readonly Exception[] exceptions;

        /// <summary>
        /// Initializes a new instance 
        /// </summary>
        public CombinedException(params Exception[] exceptions)
            : base("Got multiple exceptions:")
        {
            this.exceptions = exceptions;

        }
        /// <summary>
        /// Initializes a new instance 
        /// </summary>
        public CombinedException(string message)
            : base(message)
        {
        }
        /// <summary>
        /// Initializes a new instance 
        /// </summary>
        public CombinedException(string message, Exception inner)
            : base(message, inner)
        {
        }
#if !SILVERLIGHT
        /// <summary>
        /// Initializes a new instance 
        /// </summary>
        protected CombinedException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
#endif

        /// <summary>
        /// Creates and returns a string representation of the current exception.
        /// </summary>
        /// <returns>
        /// A string representation of the current exception.
        /// </returns>
        public override string ToString()
        {
            if (this.exceptions != null)
            {

                if (this.exceptions.Length == 0)
                {
                    return base.ToString();
                }

                if (this.exceptions.Length == 1 && this.exceptions[0] != null)
                {
                    return this.exceptions[0].ToString();
                }

                var sb = new StringBuilder();
                string separator = string.Empty;
                string newline = EnvironmentHelper.NewLine;
                foreach (var ex in this.exceptions)
                {
                    if (ex == null)
                    {
                        continue;
                    }

                    sb.Append(separator);
                    sb.Append(ex.ToString());
                    sb.Append(newline);
                    separator = newline;
                }

                return this.Message + sb.ToString();

            }
            return base.ToString();
        }
    }
}
