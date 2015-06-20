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

namespace NLog.Targets
{
    using System;

    internal class DateAndSequenceArchive
    {
        private readonly string _fileName;
        private readonly DateTime _date;
        private readonly string _dateFormat;
        private readonly int _sequence;
        private readonly string _formattedDate;

        public string FileName
        {
            get { return _fileName; }
        }

        public DateTime Date
        {
            get { return _date; }
        }

        public int Sequence
        {
            get { return _sequence; }
        }

        public bool HasSameArchiveDate(DateTime date)
        {
            return date.ToString(_dateFormat) == _formattedDate;
        }
        
        public DateAndSequenceArchive(string fileName, DateTime date, string dateFormat, int sequence)
        {
            if (fileName == null) throw new ArgumentNullException("fileName");
            if (dateFormat == null) throw new ArgumentNullException("dateFormat");

            _date = date;
            _dateFormat = dateFormat;
            _sequence = sequence;
            _fileName = fileName;
            _formattedDate = date.ToString(dateFormat);
        }
    }
}