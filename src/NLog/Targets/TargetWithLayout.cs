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

namespace NLog.Targets
{
    using NLog.Layouts;

    /// <summary>
    /// Represents target that supports string formatting using layouts.
    /// </summary>
    /// <remarks>
    /// <a href="https://github.com/NLog/NLog/wiki/How-to-write-a-custom-target">See NLog Wiki</a>
    /// </remarks>
    /// <seealso href="https://github.com/NLog/NLog/wiki/How-to-write-a-custom-target">Documentation on NLog Wiki</seealso>
    public abstract class TargetWithLayout : Target
    {
        private const string DefaultLayoutText = "${longdate}|${level:uppercase=true}|${logger}|${message:withexception=true}";
        private static NLog.LayoutRenderers.LayoutRenderer[] DefaultLayout => new NLog.LayoutRenderers.LayoutRenderer[]
        {
            new NLog.LayoutRenderers.LongDateLayoutRenderer(),
            new NLog.LayoutRenderers.LiteralLayoutRenderer("|"),
            new NLog.LayoutRenderers.LevelLayoutRenderer() { Uppercase = true },
            new NLog.LayoutRenderers.LiteralLayoutRenderer("|"),
            new NLog.LayoutRenderers.LoggerNameLayoutRenderer(),
            new NLog.LayoutRenderers.LiteralLayoutRenderer("|"),
            new NLog.LayoutRenderers.MessageLayoutRenderer() { WithException = true },
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="TargetWithLayout" /> class.
        /// </summary>
        /// <remarks>
        /// The default value of the layout is: <code>${longdate}|${level:uppercase=true}|${logger}|${message:withexception=true}</code>
        /// </remarks>
        protected TargetWithLayout()
        {
            Layout = new SimpleLayout(DefaultLayout, DefaultLayoutText);
        }

        /// <summary>
        /// Gets or sets the layout used to format log messages.
        /// </summary>
        /// <remarks><b>[Required]</b> Default: <code>${longdate}|${level:uppercase=true}|${logger}|${message:withexception=true}</code></remarks>
        /// <docgen category='Layout Options' order='1' />
        public virtual Layout Layout { get; set; }
    }
}
