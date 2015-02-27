// 
// Copyright (c) 2004-2011 Jaroslaw Kowalski <jaak@jkowalski.net>
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
using JetBrains.Annotations;

namespace NLog.Internal
{
    internal static class EncodingHelpers
    {
        /// <summary>
        /// Fix encoding so it has/hasn't a BOM (Byte-order-mark)
        /// </summary>
        /// <param name="encoding">encoding to be converted</param>
        /// <param name="includeBOM">should we include the BOM (Byte-order-mark) for UTF? <c>true</c> for BOM, <c>false</c> for no BOM</param>
        /// <returns>new or current encoding</returns>
        /// <remarks>.net has default a BOM included with UTF-8</remarks>
        public static Encoding ConvertEncodingBOM([NotNull] this Encoding encoding, bool includeBOM)
        {
            if (encoding == null) throw new ArgumentNullException("encoding");
            if (!includeBOM)
            {
                //default .net uses BOM, so we have to create a new instance to exlucde this.
                if (encoding.EncodingName.Equals(Encoding.UTF8.EncodingName, StringComparison.InvariantCulture))
                {
                    return new UTF8Encoding(false);

                }
            }

            return encoding;
        }

        /// <summary>
        /// UTF-8 BOM 239, 187, 191
        /// </summary>
        internal static readonly byte[] Utf8BOM = { 0xEF, 0xBB, 0xBF };
    }
}
