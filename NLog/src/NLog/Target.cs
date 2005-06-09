// 
// Copyright (c) 2004,2005 Jaroslaw Kowalski <jkowalski@users.sourceforge.net>
// 
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
// * Neither the name of the Jaroslaw Kowalski nor the names of its 
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
using System.Text;

using NLog.Config;

namespace NLog
{
    /// <summary>
    /// Represents logging target.
    /// </summary>
    public abstract class Target
    {
        /// <summary>
        /// Creates a new instance of the logging target and initializes
        /// default layout.
        /// </summary>
        /// <remarks>
        /// The default value of the layout is: <code>${longdate}|${level:uppercase=true}|${logger}|${message}</code>
        /// </remarks>
        protected Target()
        {
            Layout = "${longdate}|${level:uppercase=true}|${logger}|${message}";
        }

        private Layout _compiledlayout;
        private string _name;

        /// <summary>
        /// The name of the target.
        /// </summary>
        [RequiredParameter]
        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                _name = value;
            }
        }

        /// <summary>
        /// The text to be rendered.
        /// </summary>
        [RequiredParameter]
        [AcceptsLayout]
        [System.ComponentModel.DefaultValue("${longdate}|${level:uppercase=true}|${logger}|${message}")]
        public string Layout
        {
            get
            {
                return _compiledlayout.Text;
            }
            set
            {
                _compiledlayout = new Layout(value);
            }
        }

        /// <summary>
        /// The compiled layout to be rendered.
        /// </summary>
        protected Layout CompiledLayout
        {
            get
            {
                return _compiledlayout;
            }
            set
            {
                _compiledlayout = value;
            }
        }

        /// <summary>
        /// Writes logging event to the log target. Must be overridden in inheriting
        /// classes.
        /// </summary>
        /// <param name="ev">Logging event to be written out.</param>
        protected internal abstract void Append(LogEventInfo ev);

        /// <summary>
        /// Determines whether stack trace information should be gathered
        /// during log event processing. By default it calls <see cref="NLog.Layout.NeedsStackTrace" /> on
        /// <see cref="Target.CompiledLayout" />.
        /// </summary>
        /// <returns>0 - don't include stack trace<br/>1 - include stack trace without source file information<br/>2 - include full stack trace</returns>
        protected internal virtual int NeedsStackTrace()
        {
            return CompiledLayout.NeedsStackTrace();
        }

        /// <summary>
        /// Returns the text representation of the object. Used for diagnostics.
        /// </summary>
        /// <returns>A string that describes the target.</returns>
        public override string ToString()
        {
            return String.Format("{0}: {1}", Name, this.GetType().FullName);
        }
    }
}
