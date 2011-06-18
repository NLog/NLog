// 
// Copyright (c) 2004-2011 Jaroslaw Kowalski <jaak@jkowalski.net>
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

#if !SILVERLIGHT

namespace NLog.Targets
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data;
    using NLog.Config;
    using NLog.Layouts;

    /// <summary>
    /// Information about database command + parameters.
    /// </summary>
    [NLogConfigurationItem]
    public class DatabaseCommandInfo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseCommandInfo"/> class.
        /// </summary>
        public DatabaseCommandInfo()
        {
            this.Parameters = new List<DatabaseParameterInfo>();
            this.CommandType = CommandType.Text;
        }

        /// <summary>
        /// Gets or sets the type of the command.
        /// </summary>
        /// <value>The type of the command.</value>
        /// <docgen category='Command Options' order='10' />
        [RequiredParameter]
        [DefaultValue(CommandType.Text)]
        public CommandType CommandType { get; set; }

        /// <summary>
        /// Gets or sets the connection string to run the command against. If not provided, connection string from the target is used.
        /// </summary>
        /// <docgen category='Command Options' order='10' />
        public Layout ConnectionString { get; set; }

        /// <summary>
        /// Gets or sets the command text.
        /// </summary>
        /// <docgen category='Command Options' order='10' />
        [RequiredParameter]
        public Layout Text { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to ignore failures.
        /// </summary>
        /// <docgen category='Command Options' order='10' />
        public bool IgnoreFailures { get; set; }

        /// <summary>
        /// Gets the collection of parameters. Each parameter contains a mapping
        /// between NLog layout and a database named or positional parameter.
        /// </summary>
        /// <docgen category='Command Options' order='10' />
        [ArrayParameter(typeof(DatabaseParameterInfo), "parameter")]
        public IList<DatabaseParameterInfo> Parameters { get; private set; }
    }
}

#endif