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

namespace NLog.Layouts
{
    using NLog.Config;

    /// <summary>
    /// Represents a parameter for the <see cref="Log4JXmlEventLayout"/>
    /// </summary>
    /// <remarks>
    /// <a href="https://github.com/NLog/NLog/wiki/Log4JXmlEventLayout">See NLog Wiki</a>
    /// </remarks>
    /// <seealso href="https://github.com/NLog/NLog/wiki/Log4JXmlEventLayout">Documentation on NLog Wiki</seealso>
    [NLogConfigurationItem]
    public class Log4JXmlEventParameter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Log4JXmlEventParameter" /> class.
        /// </summary>
        public Log4JXmlEventParameter()
        {
        }

        /// <summary>
        /// Gets or sets log4j:data property-name.
        /// </summary>
        /// <docgen category='Layout Options' order='1' />
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the layout used for rendering the log4j:data property-value.
        /// </summary>
        /// <docgen category='Layout Options' order='10' />
        public Layout Layout { get; set; } = Layout.Empty;

        /// <summary>
        /// Gets or sets whether empty property-value should be included in the output. Default = false
        /// </summary>
        /// <remarks>Empty value is either null or empty string</remarks>
        /// <docgen category='Layout Options' order='100' />
        public bool IncludeEmptyValue { get; set; }
    }
}
