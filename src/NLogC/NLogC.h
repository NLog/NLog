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

#ifndef __NLOGC_H
#define __NLOGC_H

#ifdef NLOGC_EXPORTS
#define NLOGC_API __declspec(dllexport)
#else
#define NLOGC_API __declspec(dllimport)
#endif

enum NLogLevel
{
    NLOG_TRACE,
    NLOG_DEBUG,
    NLOG_INFO,
    NLOG_WARN,
    NLOG_ERROR,
    NLOG_FATAL
};

extern "C" {

NLOGC_API int NLog_InitA(const char *nlogDllFileName);
NLOGC_API int NLog_ConfigureFromFileA(const char *fileName);
NLOGC_API void NLog_LogA(NLogLevel level, const char *loggerName, const char *logMessage, ...); 
NLOGC_API void NLog_TraceA(const char *loggerName, const char *logMessage, ...); 
NLOGC_API void NLog_DebugA(const char *loggerName, const char *logMessage, ...); 
NLOGC_API void NLog_InfoA(const char *loggerName, const char *logMessage, ...); 
NLOGC_API void NLog_WarnA(const char *loggerName, const char *logMessage, ...); 
NLOGC_API void NLog_ErrorA(const char *loggerName, const char *logMessage, ...); 
NLOGC_API void NLog_FatalA(const char *loggerName, const char *logMessage, ...); 
NLOGC_API void NLog_LogVA(NLogLevel level, const char *loggerName, const char *logMessage, va_list args);
NLOGC_API int NLog_ConfigureFromXmlA(const char *configFileContents);

NLOGC_API int NLog_InitW(const wchar_t *nlogDllFileName);
NLOGC_API int NLog_ConfigureFromFileW(const wchar_t *fileName);
NLOGC_API void NLog_LogW(NLogLevel level, const wchar_t *loggerName, const wchar_t *logMessage, ...); 
NLOGC_API void NLog_TraceW(const wchar_t *loggerName, const wchar_t *logMessage, ...); 
NLOGC_API void NLog_DebugW(const wchar_t *loggerName, const wchar_t *logMessage, ...); 
NLOGC_API void NLog_InfoW(const wchar_t *loggerName, const wchar_t *logMessage, ...); 
NLOGC_API void NLog_WarnW(const wchar_t *loggerName, const wchar_t *logMessage, ...); 
NLOGC_API void NLog_ErrorW(const wchar_t *loggerName, const wchar_t *logMessage, ...); 
NLOGC_API void NLog_FatalW(const wchar_t *loggerName, const wchar_t *logMessage, ...); 
NLOGC_API void NLog_LogVW(NLogLevel level, const wchar_t *loggerName, const wchar_t *logMessage, va_list args); 
NLOGC_API int NLog_ConfigureFromXmlW(const wchar_t *configFileContents);

NLOGC_API int NLog_InitLocal();
}

#ifdef UNICODE

#define NLog_Init NLog_InitW
#define NLog_Log NLog_LogW
#define NLog_Trace NLog_TraceW
#define NLog_Debug NLog_DebugW
#define NLog_Info NLog_InfoW
#define NLog_Warn NLog_WarnW
#define NLog_Error NLog_ErrorW
#define NLog_Fatal NLog_FatalW
#define NLog_ConfigureFromFile NLog_ConfigureFromFileW
#define NLog_ConfigureFromXml NLog_ConfigureFromXmlW

#else

#define NLog_Init NLog_InitA
#define NLog_Log NLog_LogA
#define NLog_Trace NLog_TraceA
#define NLog_Debug NLog_DebugA
#define NLog_Info NLog_InfoA
#define NLog_Warn NLog_WarnA
#define NLog_Error NLog_ErrorA
#define NLog_Fatal NLog_FatalA
#define NLog_ConfigureFromFile NLog_ConfigureFromFileA
#define NLog_ConfigureFromXml NLog_ConfigureFromXmlA

#endif

#endif // __NLOGC_H
