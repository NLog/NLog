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
    /// A column in the CSV.
    /// </summary>
    /// <remarks>
    /// <a href="https://github.com/NLog/NLog/wiki/CsvLayout">See NLog Wiki</a>
    /// </remarks>
    /// <seealso href="https://github.com/NLog/NLog/wiki/CsvLayout">Documentation on NLog Wiki</seealso>
    [NLogConfigurationItem]
    public class CsvColumn
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CsvColumn" /> class.
        /// </summary>
        public CsvColumn()
            : this(string.Empty, Layout.Empty)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CsvColumn" /> class.
        /// </summary>
        /// <param name="name">The name of the column.</param>
        /// <param name="layout">The layout of the column.</param>
        public CsvColumn(string name, Layout layout)
        {
            Name = name;
            Layout = layout;
        }

        /// <summary>
        /// Gets or sets the name of the column.
        /// </summary>
        /// <remarks><b>[Required]</b> Default: <see cref="string.Empty"/></remarks>
        /// <docgen category='Layout Options' order='1' />
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the layout used for rendering the column value.
        /// </summary>
        /// <remarks><b>[Required]</b> Default: <see cref="Layout.Empty"/></remarks>
        /// <docgen category='Layout Options' order='10' />
        public Layout Layout { get; set; }

        /// <summary>
        /// Gets or sets the override of Quoting mode
        /// </summary>
        /// <remarks>
        /// Default: <see cref="CsvQuotingMode.Auto"/> .
        ///
        /// For faster performance then consider <see cref="CsvQuotingMode.All"/> and <see cref="CsvQuotingMode.Nothing"/>
        /// </remarks>
        /// <docgen category='Layout Options' order='50' />
        public CsvQuotingMode Quoting { get => _quoting ?? CsvQuotingMode.Auto; set => _quoting = value; }
        internal CsvQuotingMode? _quoting;
    }
}
