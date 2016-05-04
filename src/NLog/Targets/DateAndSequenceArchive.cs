// 
// Copyright (c) 2004-2016 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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
    using System;

    /// <summary>
    /// A descriptor for an archive created with the DateAndSequence numbering mode.
    /// </summary>
    internal class DateAndSequenceArchive
    {
        private readonly string _dateFormat;
        private readonly string _formattedDate;

        /// <summary>
        /// The full name of the archive file.
        /// </summary>
        public string FileName { get; private set; }

        /// <summary>
        /// The parsed date contained in the file name.
        /// </summary>
        public DateTime Date { get; private set; }

        /// <summary>
        /// The parsed sequence number contained in the file name.
        /// </summary>
        public int Sequence { get; private set; }

        /// <summary>
        /// Determines whether <paramref name="date"/> produces the same string as the current instance's date once formatted with the current instance's date format.
        /// </summary>
        /// <param name="date">The date to compare the current object's date to.</param>
        /// <returns><c>True</c> if the formatted dates are equal, otherwise <c>False</c>.</returns>
        public bool HasSameFormattedDate(DateTime date)
        {
            return date.ToString(_dateFormat) == _formattedDate;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DateAndSequenceArchive"/> class.
        /// </summary>
        public DateAndSequenceArchive(string fileName, DateTime date, string dateFormat, int sequence)
        {
            if (fileName == null) throw new ArgumentNullException("fileName");
            if (dateFormat == null) throw new ArgumentNullException("dateFormat");

            Date = date;
            _dateFormat = dateFormat;
            Sequence = sequence;
            FileName = fileName;
            _formattedDate = date.ToString(dateFormat);
        }
    }
}