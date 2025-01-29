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
    /// <summary>
    /// Modes of archiving files based on time.
    /// </summary>
    public enum FileArchiveEveryPeriod
    {
        /// <summary>
        /// Don't archive based on time.
        /// </summary>
        None,

        /// <summary>
        /// Archive every new year.
        /// </summary>
        Year,

        /// <summary>
        /// Archive every new month.
        /// </summary>
        Month,

        /// <summary>
        /// Archive every new day.
        /// </summary>
        Day,

        /// <summary>
        /// Archive every new hour.
        /// </summary>
        Hour,

        /// <summary>
        /// Archive every new minute.
        /// </summary>
        Minute,

        #region Weekdays
        /// <summary>
        /// Archive every Sunday.
        /// </summary>
        Sunday,

        /// <summary>
        /// Archive every Monday.
        /// </summary>
        Monday,

        /// <summary>
        /// Archive every Tuesday.
        /// </summary>
        Tuesday,

        /// <summary>
        /// Archive every Wednesday.
        /// </summary>
        Wednesday,

        /// <summary>
        /// Archive every Thursday.
        /// </summary>
        Thursday,

        /// <summary>
        /// Archive every Friday.
        /// </summary>
        Friday,

        /// <summary>
        /// Archive every Saturday.
        /// </summary>
        Saturday
        #endregion
    }
}
