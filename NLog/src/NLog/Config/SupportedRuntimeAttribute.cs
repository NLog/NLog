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

namespace NLog.Config
{
    /// <summary>
    /// Marks classes and properties as supporting particular runtime framework 
    /// and operating system.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class SupportedRuntimeAttribute: Attribute
    {
        private RuntimeFramework _framework = RuntimeFramework.Any;
        private RuntimeOS _os = RuntimeOS.Any;
        private Version _minRuntimeVersion = null;
        private Version _maxRuntimeVersion = null;
        private Version _minOSVersion = null;
        private Version _maxOSVersion = null;

        /// <summary>
        /// Creates a new instance of <see cref="SupportedRuntimeAttribute"/>.
        /// </summary>
        public SupportedRuntimeAttribute()
        {
        }

        /// <summary>
        /// Creates a new instance of <see cref="SupportedRuntimeAttribute"/>
        /// and sets the supported framework.
        /// </summary>
        public SupportedRuntimeAttribute(RuntimeFramework framework)
        {
            Framework = framework;
        }

        /// <summary>
        /// Creates a new instance of <see cref="SupportedRuntimeAttribute"/>
        /// and sets the supported operating system.
        /// </summary>
        public SupportedRuntimeAttribute(RuntimeOS operatingSystem)
        {
            OS = operatingSystem;
        }

        /// <summary>
        /// Creates a new instance of <see cref="SupportedRuntimeAttribute"/>
        /// and sets the supported framework and operating system combination.
        /// </summary>
        public SupportedRuntimeAttribute(RuntimeFramework framework, RuntimeOS operatingSystem)
        {
            Framework = framework;
            OS = operatingSystem;
        }

        /// <summary>
        /// Supported runtime framework.
        /// </summary>
        public RuntimeFramework Framework
        {
            get { return _framework; }
            set { _framework = value; }
        }

        /// <summary>
        /// Supported operating system.
        /// </summary>
        public RuntimeOS OS
        {
            get { return _os; }
            set { _os = value; }
        }

        /// <summary>
        /// Minimum runtime version supported.
        /// </summary>
        public Version MinRuntimeVersion
        {
            get { return _minRuntimeVersion; }
            set { _minRuntimeVersion = value; }
        }

        /// <summary>
        /// Maximum runtime version supported.
        /// </summary>
        public Version MaxRuntimeVersion
        {
            get { return _maxRuntimeVersion; }
            set { _maxRuntimeVersion = value; }
        }

        /// <summary>
        /// Minimum operating system version supported.
        /// </summary>
        public Version MinOSVersion
        {
            get { return _minOSVersion; }
            set { _minOSVersion = value; }
        }

        /// <summary>
        /// Maximum operating system version supported.
        /// </summary>
        public Version MaxOSVersion
        {
            get { return _maxOSVersion; }
            set { _maxOSVersion = value; }
        }
    }
}
