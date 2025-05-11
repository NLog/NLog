//
// Copyright (c) 2004-2024 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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

#nullable enable

namespace NLog.Targets.Wrappers
{
    using System;
    using NLog.Common;

    /// <summary>
    /// Base class for targets wrap other (single) targets.
    /// </summary>
    public abstract class WrapperTargetBase : Target
    {
        /// <summary>
        /// Gets or sets the target that is wrapped by this target.
        /// </summary>
        /// <docgen category='General Options' order='11' />
        public Target? WrappedTarget
        {
            get => _wrappedTarget;
            set
            {
                _wrappedTarget = value;
                _tostring = null;
            }
        }
        private Target? _wrappedTarget;

        /// <inheritdoc/>
        public override string ToString()
        {
            return _tostring ?? (_tostring = GenerateTargetToString());
        }

        private string GenerateTargetToString()
        {
            if (WrappedTarget is null)
                return GenerateTargetToString(true);
            else if (string.IsNullOrEmpty(Name))
                return $"{GenerateTargetToString(true, string.Empty)}_{WrappedTarget}";
            else
                return $"{GenerateTargetToString(true, string.Empty)}_{WrappedTarget.GenerateTargetToString(false, Name)}";
        }

        /// <inheritdoc/>
        protected override void FlushAsync(AsyncContinuation asyncContinuation)
        {
            if (WrappedTarget is null)
                asyncContinuation(null);
            else
                WrappedTarget.Flush(asyncContinuation);
        }

        /// <inheritdoc/>
        protected override void InitializeTarget()
        {
            if (WrappedTarget is null)
                throw new NLogConfigurationException($"{GetType().Name}(Name={Name}): No wrapped Target configured.");

            base.InitializeTarget();
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
