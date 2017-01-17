﻿// 
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

namespace NLog.Internal
{
    /// <summary>
    /// Controls a single allocated MemoryStream for reuse (only one active user)
    /// </summary>
    internal class ReusableStreamCreator : IDisposable
    {
        private System.IO.MemoryStream _memoryStream = new System.IO.MemoryStream();

        /// <summary>Empty handle when <see cref="Targets.Target.OptimizeBufferReuse"/> is disabled</summary>
        public readonly LockStream None = default(LockStream);

        /// <summary>
        /// Creates handle to the reusable MemoryStream for active usage
        /// </summary>
        /// <returns>Handle to the reusable item, that can release it again</returns>
        public LockStream Allocate()
        {
            return new LockStream(this);
        }

        public struct LockStream : IDisposable
        {
            /// <summary>
            /// Access the MemoryStream acquired
            /// </summary>
            public readonly System.IO.MemoryStream Result;
            private readonly ReusableStreamCreator _owner;

            public LockStream(ReusableStreamCreator owner)
            {
                Result = owner._memoryStream;
                owner._memoryStream = null;
                _owner = owner;
            }

            public void Dispose()
            {
                if (Result != null)
                {
                    Result.Position = 0;
                    Result.SetLength(0);
                    _owner._memoryStream = Result;
                }
            }
        }

        void IDisposable.Dispose()
        {
            _memoryStream.Dispose();
        }
    }
}
