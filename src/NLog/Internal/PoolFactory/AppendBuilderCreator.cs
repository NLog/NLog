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
    /// Allocates new builder and appends to the provided target builder on dispose
    /// </summary>
    internal struct AppendBuilderCreator : IDisposable
    {
        /// <summary>
        /// Access the new builder allocated
        /// </summary>
        public readonly StringBuilder Builder;
        private readonly ReusableStringBuilder _reusableBuilder;
        private readonly StringBuilder _appendTarget;

        public AppendBuilderCreator(StringBuilder appendTarget, ILogEventObjectFactory objectFactory, int initialSize)
        {
            _appendTarget = appendTarget;
            if (_appendTarget == null || _appendTarget.Length > 0)
            {
                _reusableBuilder = objectFactory != null && !ReferenceEquals(objectFactory, LogEventObjectFactory.Instance) ? objectFactory.CreateStringBuilder(initialSize) : null;
                Builder = _reusableBuilder != null ? _reusableBuilder.Result : new StringBuilder(initialSize);
            }
            else
            {
                _reusableBuilder = null;
                Builder = _appendTarget;
            }
        }

        public AppendBuilderCreator(StringBuilder appendTarget, LogEventInfo logEvent, int initialSize)
        {
            _appendTarget = appendTarget;
            if (_appendTarget == null || _appendTarget.Length > 0)
            {
                _reusableBuilder = logEvent.PoolReleaseContinuation != null ? logEvent.ObjectFactory.CreateStringBuilder(initialSize) : null;
                Builder = _reusableBuilder != null ? _reusableBuilder.Result : new StringBuilder(initialSize);
            }
            else
            {
                _reusableBuilder = null;
                Builder = _appendTarget;
            }
        }

        public char[] GetWorkBuffer()
        {
            if (_reusableBuilder != null)
                return _reusableBuilder.GetWorkBuffer();
            else
                return null;
        }

        public void Dispose()
        {
            if (_appendTarget == null)
            {
                if (_reusableBuilder != null)
                    ((IDisposable)_reusableBuilder).Dispose();
            }
            else if (_reusableBuilder != null)
            {
                _reusableBuilder.CopyTo(_appendTarget);
                ((IDisposable)_reusableBuilder).Dispose();
            }
            else if (!ReferenceEquals(Builder, _appendTarget))
            {
                StringBuilderExt.CopyToBuilder(Builder, _appendTarget);
            }
        }
    }
}
