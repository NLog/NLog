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

namespace NLog.Targets.Wrappers
{
    using System;
    using NLog.Common;
    using NLog.Config;

    /// <summary>
    /// Base class for targets wrap other (single) targets.
    /// </summary>
    public abstract class WrapperTargetBase : Target
    {
        /// <summary>
        /// Gets or sets the target that is wrapped by this target.
        /// </summary>
        /// <docgen category='General Options' order='11' />
        [RequiredParameter]
        public Target WrappedTarget
        {
            get => _wrappedTarget;
            set
            {
                _wrappedTarget = value;
                _tostring = null;
            }
        }
        private Target _wrappedTarget;

        /// <summary>
        /// Returns the text representation of the object. Used for diagnostics.
        /// </summary>
        /// <returns>A string that describes the target.</returns>
        public override string ToString()
        {
            return _tostring ?? (_tostring = GenerateTargetToString());
        }

        private string GenerateTargetToString()
        {
            if (string.IsNullOrEmpty(Name))
                return $"{GenerateTargetToString(true)}_{WrappedTarget}";
            else if (WrappedTarget == null)
                return GenerateTargetToString(true);
            else
                return $"{GenerateTargetToString(true, "")}_{WrappedTarget.GenerateTargetToString(false, Name)}";
        }

        /// <summary>
        /// Flush any pending log messages (in case of asynchronous targets).
        /// </summary>
        /// <param name="asyncContinuation">The asynchronous continuation.</param>
        protected override void FlushAsync(AsyncContinuation asyncContinuation)
        {
            if (WrappedTarget != null)
                WrappedTarget.Flush(asyncContinuation);
            else
                asyncContinuation(null);
        }

        /// <summary>
        /// Writes logging event to the log target. Must be overridden in inheriting
        /// classes.
        /// </summary>
        /// <param name="logEvent">Logging event to be written out.</param>
        protected sealed override void Write(LogEventInfo logEvent)
        {
            throw new NotSupportedException("This target must not be invoked in a synchronous way.");
        }
    }
}