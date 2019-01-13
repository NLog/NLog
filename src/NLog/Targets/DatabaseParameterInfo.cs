// 
// Copyright (c) 2004-2018 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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

#if !SILVERLIGHT && !__IOS__ && !__ANDROID__

namespace NLog.Targets
{
    using System.ComponentModel;
    using Config;
    using Layouts;

    /// <summary>
    /// Represents a parameter to a Database target.
    /// </summary>
    [NLogConfigurationItem]
    public class DatabaseParameterInfo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseParameterInfo" /> class.
        /// </summary>
        public DatabaseParameterInfo()
            : this(null, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseParameterInfo" /> class.
        /// </summary>
        /// <param name="parameterName">Name of the parameter.</param>
        /// <param name="parameterLayout">The parameter layout.</param>
        public DatabaseParameterInfo(string parameterName, Layout parameterLayout)
        {
            Name = parameterName;
            Layout = parameterLayout;
        }

        /// <summary>
        /// Gets or sets the database parameter name.
        /// </summary>
        /// <docgen category='Parameter Options' order='0' />
        [RequiredParameter]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the layout that should be use to calcuate the value for the parameter.
        /// </summary>
        /// <docgen category='Parameter Options' order='1' />
        [RequiredParameter]
        public Layout Layout { get; set; }

        /// <summary>
        /// Use raw value
        ///
        /// If null, then rawValue will be used when the dbtype isn't a string-like (that's:
        /// <see cref="System.Data.DbType.String"/>
        /// <see cref="System.Data.DbType.AnsiString"/>
        /// <see cref="System.Data.DbType.StringFixedLength"/>
        /// <see cref="System.Data.DbType.AnsiStringFixedLength"/>
        /// )
        /// </summary>
        public bool? UseRawValue { get; set; }

        /// <summary>
        /// Gets or sets the database parameter DbType.
        /// </summary>
        /// <docgen category='Parameter Options' order='2' />
        [DefaultValue(null)]
        public string DbType { get; set; }

        /// <summary>
        /// Gets or sets convert format of the database parameter value .
        /// </summary>
        /// <docgen category='Parameter Options' order='3' />
        [DefaultValue(null)]
        public string Format { get; set; }

        /// <summary>
        /// Gets or sets the database parameter size.
        /// </summary>
        /// <docgen category='Parameter Options' order='4' />
        [DefaultValue(0)]
        public int Size { get; set; }

        /// <summary>
        /// Gets or sets the database parameter precision.
        /// </summary>
        /// <docgen category='Parameter Options' order='5' />
        [DefaultValue(0)]
        public byte Precision { get; set; }

        /// <summary>
        /// Gets or sets the database parameter scale.
        /// </summary>
        /// <docgen category='Parameter Options' order='6' />
        [DefaultValue(0)]
        public byte Scale { get; set; }
    }
}

#endif