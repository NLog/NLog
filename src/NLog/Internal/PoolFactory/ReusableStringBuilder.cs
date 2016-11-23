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
using System.Text;

namespace NLog.Internal.PoolFactory
{
    /// <summary>
    /// Wraps a preallocated <see cref="StringBuilder"/>. So it can
    /// be recycled by <see cref="LogEventPoolFactory"/>
    /// </summary>
    internal class ReusableStringBuilder : IPoolObject, IDisposable
    {
        private readonly StringBuilder _result;
        private ILogEventObjectFactory _owner;
        private char[] _workBuffer;

        public StringBuilder Result { get { return _result; } }

        public char[] GetWorkBuffer()
        {
            if (_owner != null)
            {
                if (_workBuffer == null)
                    _workBuffer = new char[4096];
            }
            return _workBuffer;
        }

        object IPoolObject.OwnerPool { get { return _owner; } set { _owner = (ILogEventObjectFactory)value; } }

        internal ReusableStringBuilder(StringBuilder result)
        {
            _result = result;
        }

        public void CopyTo(StringBuilder builder)
        {
            StringBuilderExt.CopyToBuilder(_result, builder, GetWorkBuffer());
        }

        /// <summary>
        /// Clears the StringBuilder before it is used again
        /// </summary>
        public void Clear()
        {
            StringBuilderExt.ClearBuilder(_result);
        }

        /// <summary>
        /// Puts object back in the pool
        /// </summary>
        void IDisposable.Dispose()
        {
            if (_owner != null)
            {
                _owner.ReleaseStringBuilder(this);
            }
        }

        public new string ToString()
        {
            return _result.ToString();
        }
    }
}
