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
    /// <summary>Syslog facilities</summary>
    public enum SyslogFacility
    {
        /// <summary>Kernel messages - LOG_KERN</summary>
        Kernel = 0,

        /// <summary>Random user-level messages - LOG_USER</summary>
        User = 1,

        /// <summary>Mail system - LOG_MAIL</summary>
        Mail = 2,

        /// <summary>System daemons - LOG_DAEMON</summary>
        Daemons = 3,

        /// <summary>Security/authorization messages - LOG_AUTH</summary>
        Authorization = 4,

        /// <summary>Messages generated internally by syslogd - LOG_SYSLOG</summary>
        Syslog = 5,

        /// <summary>Line printer subsystem - LOG_LPR</summary>
        Printer = 6,

        /// <summary>Network news subsystem - LOG_NEWS</summary>
        News = 7,

        /// <summary>UUCP subsystem - LOG_UUCP</summary>
        Uucp = 8,

        /// <summary>Clock (cron/at) daemon - LOG_CRON</summary>
        Clock = 9,

        /// <summary>Security/authorization messages (private) - LOG_AUTHPRIV</summary>
        Authorization2 = 10,

        /// <summary>FTP daemon - LOG_FTP</summary>
        Ftp = 11,

        /// <summary>NTP subsystem - LOG_NTP</summary>
        Ntp = 12,

        /// <summary>Log audit - LOG_SECURITY</summary>
        Audit = 13,

        /// <summary>Log alert - LOG_CONSOLE</summary>
        Alert = 14,

        /// <summary>Clock daemon / Scheduling daemon</summary>
        Clock2 = 15,

        /// <summary>Reserved for local use - LOG_LOCAL0</summary>
        Local0 = 16,

        /// <summary>Reserved for local use - LOG_LOCAL1</summary>
        Local1 = 17,

        /// <summary>Reserved for local use - LOG_LOCAL2</summary>
        Local2 = 18,

        /// <summary>Reserved for local use - LOG_LOCAL3</summary>
        Local3 = 19,

        /// <summary>Reserved for local use - LOG_LOCAL4</summary>
        Local4 = 20,

        /// <summary>Reserved for local use - LOG_LOCAL5</summary>
        Local5 = 21,

        /// <summary>Reserved for local use - LOG_LOCAL6</summary>
        Local6 = 22,

        /// <summary>Reserved for local use - LOG_LOCAL7</summary>
        Local7 = 23
    }
}
