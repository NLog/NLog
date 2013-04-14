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

#ifndef __NLOGGER_H
#define __NLOGGER_H

#include <stdarg.h>

#include "NLogC.h"

class NLogger
{
    const char *_loggerName;

public:
    NLogger(const char *loggerName)
    {
        _loggerName = loggerName;
    }

    void Debug(const char *logMessage, ...)
    {
        va_list args;

        va_start(args, logMessage);
        NLog_LogVA(NLOG_DEBUG, _loggerName, logMessage, args);
        va_end(args);
    }
    void Info(const char *logMessage, ...)
    {
        va_list args;

        va_start(args, logMessage);
        NLog_LogVA(NLOG_INFO, _loggerName, logMessage, args);
        va_end(args);
    }
    void Warn(const char *logMessage, ...)
    {
        va_list args;

        va_start(args, logMessage);
        NLog_LogVA(NLOG_WARN, _loggerName, logMessage, args);
        va_end(args);
    }
    void Error(const char *logMessage, ...)
    {
        va_list args;

        va_start(args, logMessage);
        NLog_LogVA(NLOG_ERROR, _loggerName, logMessage, args);
        va_end(args);
    }
    void Fatal(const char *logMessage, ...)
    {
        va_list args;

        va_start(args, logMessage);
        NLog_LogVA(NLOG_FATAL, _loggerName, logMessage, args);
        va_end(args);
    }
};

#endif // __NLOGC_H
