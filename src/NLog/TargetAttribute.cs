// 
// Copyright (c) 2004-2006 Jaroslaw Kowalski <jaak@jkowalski.net>
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

namespace NLog
{
    /// <summary>
    /// Marks class as a logging target and assigns a name to it.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class TargetAttribute: Attribute
    {
        private string _name;
        private bool _ignoresLayout = false;
        private bool _isCompound = false;
        private bool _isWrapper = false;

        /// <summary>
        /// Creates a new instance of the TargetAttribute class and sets the name.
        /// </summary>
        /// <param name="name"></param>
        public TargetAttribute(string name)
        {
            _name = name;
        }

        /// <summary>
        /// The name of the logging target.
        /// </summary>
        public string Name
        {
            get { return _name; }
        }

        /// <summary>
        /// Determines whether the target ignores layout specification.
        /// </summary>
        public bool IgnoresLayout
        {
            get { return _ignoresLayout; }
            set { _ignoresLayout = value; }
        }

        /// <summary>
        /// Marks the target as 'wrapper' target (used to generate the target summary documentation page);
        /// </summary>
        public bool IsWrapper
        {
            get { return _isWrapper; }
            set { _isWrapper = value; }
        }

        /// <summary>
        /// Marks the target as 'compound' target (used to generate the target summary documentation page);
        /// </summary>
        public bool IsCompound
        {
            get { return _isCompound; }
            set { _isCompound = value; }
        }
    }
}
