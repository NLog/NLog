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

#define NLOGC_EXPORTS

#include <stdio.h>

#include "NLogC.h"
#include <string.h>
#include <stdarg.h>

using namespace System;
using namespace System::Reflection;
using namespace System::IO;
using namespace System::Xml;
using namespace NLog;

#define NLOG_BUFFER_SIZE 8192

#pragma managed

static void WriteToA(NLogLevel level, const char * loggerName, const char * messageBuffer)
{
    Logger ^logger = LogManager::GetLogger(gcnew String(loggerName));

    switch (level)
    {
    case NLOG_TRACE:
        if (logger->IsTraceEnabled)
            logger->Trace(gcnew String(messageBuffer));
        break;
    case NLOG_DEBUG:
        if (logger->IsDebugEnabled)
            logger->Debug(gcnew String(messageBuffer));
        break;
    case NLOG_INFO:
        if (logger->IsInfoEnabled)
            logger->Info(gcnew String(messageBuffer));
        break;
    case NLOG_WARN:
        if (logger->IsWarnEnabled)
            logger->Warn(gcnew String(messageBuffer));
        break;
    case NLOG_ERROR:
        if (logger->IsErrorEnabled)
            logger->Error(gcnew String(messageBuffer));
        break;
    case NLOG_FATAL:
        if (logger->IsFatalEnabled)
            logger->Fatal(gcnew String(messageBuffer));
        break;
    }
}

static void WriteToW(NLogLevel level, const wchar_t * loggerName, const wchar_t * messageBuffer)
{
    Logger ^logger = LogManager::GetLogger(gcnew String(loggerName));

    switch (level)
    {
    case NLOG_TRACE:
        if (logger->IsTraceEnabled)
            logger->Trace(gcnew String(messageBuffer));
        break;
    case NLOG_DEBUG:
        if (logger->IsDebugEnabled)
            logger->Debug(gcnew String(messageBuffer));
        break;
    case NLOG_INFO:
        if (logger->IsInfoEnabled)
            logger->Info(gcnew String(messageBuffer));
        break;
    case NLOG_WARN:
        if (logger->IsWarnEnabled)
            logger->Warn(gcnew String(messageBuffer));
        break;
    case NLOG_ERROR:
        if (logger->IsErrorEnabled)
            logger->Error(gcnew String(messageBuffer));
        break;
    case NLOG_FATAL:
        if (logger->IsFatalEnabled)
            logger->Fatal(gcnew String(messageBuffer));
        break;
    }
}

static bool IsLogEnabledA(NLogLevel level, const char * loggerName)
{
    Logger ^logger = LogManager::GetLogger(gcnew String(loggerName));
    switch (level)
    {
    case NLOG_TRACE:
        return logger->IsTraceEnabled;

    case NLOG_DEBUG:
        return logger->IsDebugEnabled;

    case NLOG_INFO:
        return logger->IsInfoEnabled;

    case NLOG_WARN:
        return logger->IsWarnEnabled;

    case NLOG_ERROR:
        return logger->IsErrorEnabled;

    case NLOG_FATAL:
        return logger->IsFatalEnabled;

    default:
        return false;
    }
}

static bool IsLogEnabledW(NLogLevel level, const wchar_t * loggerName)
{
    Logger ^logger = LogManager::GetLogger(gcnew String(loggerName));
    switch (level)
    {
    case NLOG_TRACE:
        return logger->IsTraceEnabled;

    case NLOG_DEBUG:
        return logger->IsDebugEnabled;

    case NLOG_INFO:
        return logger->IsInfoEnabled;

    case NLOG_WARN:
        return logger->IsWarnEnabled;

    case NLOG_ERROR:
        return logger->IsErrorEnabled;

    case NLOG_FATAL:
        return logger->IsFatalEnabled;

    default:
        return false;
    }
}

static bool ConfigureFromFileA(const char * fileName)
{
    try
    {
        LogManager::Configuration = gcnew Config::XmlLoggingConfiguration(gcnew String(fileName));
        return true;
    }
    catch (Exception^)
    {
        return false;
    }
}

static bool ConfigureFromFileW(const wchar_t * fileName)
{
    try
    {
        LogManager::Configuration = gcnew Config::XmlLoggingConfiguration(gcnew String(fileName));
        return true;
    }
    catch (System::Exception^)
    {
        return false;
    }
}

static bool ConfigureFromXmlA(const char * xml)
{
    try
    {
		StringReader^ reader = gcnew StringReader(gcnew String(xml));
		XmlReader^ xmlreader = XmlReader::Create(reader);
		LogManager::Configuration = gcnew Config::XmlLoggingConfiguration(xmlreader, "Dummy.config");
        return true;
    }
    catch (System::Exception^)
    {
        return false;
    }
}

static bool ConfigureFromXmlW(const wchar_t * xml)
{
    try
    {
		StringReader^ reader = gcnew StringReader(gcnew String(xml));
		XmlReader^ xmlreader = XmlReader::Create(reader);
		LogManager::Configuration = gcnew Config::XmlLoggingConfiguration(xmlreader, "Dummy.config");
        return true;
    }
    catch (System::Exception^)
    {
        return false;
    }
}

ref class MyResolver
{
    System::String^ _path;

public:
    MyResolver(String^ path)
    {
        _path = path;
    }

    Assembly^ ResolveAssembly(Object^, ResolveEventArgs^ args)
    {
        if (args->Name->StartsWith("NLog,"))
        {
            return Assembly::LoadFrom(_path);
        }
        else
        {
            return nullptr;
        }
    }
};

static void ForceLoadNLogDll()
{
    LogManager::CreateNullLogger();
}

static int Managed_Init2(String^ s)
{
    try
    {
        MyResolver^ resolver = gcnew MyResolver(s);
        ResolveEventHandler^ handler = gcnew ResolveEventHandler(resolver, &MyResolver::ResolveAssembly);
		AppDomain::CurrentDomain->AssemblyResolve += handler;
        ForceLoadNLogDll();
		AppDomain::CurrentDomain->AssemblyResolve -= handler;
        return 1;
    }
    catch (Exception^ )
    {
        return 0;
    }
}

static int Managed_Init(const char *s)
{
    return Managed_Init2(gcnew String(s));
}

static int Managed_Init(const wchar_t *s)
{
    return Managed_Init2(gcnew String(s));
}

int Managed_InitLocal()
{
    System::String^ myLocation = System::Reflection::Assembly::GetExecutingAssembly()->Location;
    myLocation = System::IO::Path::Combine(System::IO::Path::GetDirectoryName(myLocation), L"NLog.dll");
    return Managed_Init2(myLocation);
}

#pragma unmanaged

NLOGC_API void NLog_TraceA(const char * loggerName, const char * logMessage, ...)
{
    va_list args;
    va_start(args, logMessage);
    NLog_LogVA(NLOG_TRACE, loggerName, logMessage, args);
    va_end(args);
}

NLOGC_API void NLog_DebugA(const char * loggerName, const char * logMessage, ...)
{
    va_list args;
    va_start(args, logMessage);
    NLog_LogVA(NLOG_DEBUG, loggerName, logMessage, args);
    va_end(args);
}

NLOGC_API void NLog_InfoA(const char * loggerName, const char * logMessage, ...)
{
    va_list args;
    va_start(args, logMessage);
    NLog_LogVA(NLOG_INFO, loggerName, logMessage, args);
    va_end(args);
}

NLOGC_API void NLog_WarnA(const char * loggerName, const char * logMessage, ...)
{
    va_list args;
    va_start(args, logMessage);
    NLog_LogVA(NLOG_WARN, loggerName, logMessage, args);
    va_end(args);
}

NLOGC_API void NLog_ErrorA(const char * loggerName, const char * logMessage, ...)
{
    va_list args;
    va_start(args, logMessage);
    NLog_LogVA(NLOG_ERROR, loggerName, logMessage, args);
    va_end(args);
}

NLOGC_API void NLog_FatalA(const char * loggerName, const char * logMessage, ...)
{
    va_list args;
    va_start(args, logMessage);
    NLog_LogVA(NLOG_FATAL, loggerName, logMessage, args);
    va_end(args);
}

NLOGC_API void NLog_LogA(NLogLevel level, const char * loggerName, const char * logMessage, ...)
{
    va_list args;
    va_start(args, logMessage);
    NLog_LogVA(level, loggerName, logMessage, args);
    va_end(args);
}

NLOGC_API void NLog_LogVA(NLogLevel level, const char * loggerName, const char * logMessage, va_list args)
{
    if (0 != strchr(logMessage, '%'))
    {
        if (IsLogEnabledA(level, loggerName))
        {
            char messageBuffer[NLOG_BUFFER_SIZE];
			_vsnprintf_s(messageBuffer, NLOG_BUFFER_SIZE - 1, logMessage, args);
            WriteToA(level, loggerName, messageBuffer);
        }
    }
    else
    {
		WriteToA(level, loggerName, logMessage);
    }
}

/// <summary>
/// AAA
/// </summary>
NLOGC_API void NLog_TraceW(const wchar_t * loggerName, const wchar_t * logMessage, ...)
{
    va_list args;
    va_start(args, logMessage);
    NLog_LogVW(NLOG_TRACE, loggerName, logMessage, args);
    va_end(args);
}

NLOGC_API void NLog_DebugW(const wchar_t * loggerName, const wchar_t * logMessage, ...)
{
    va_list args;
    va_start(args, logMessage);
    NLog_LogVW(NLOG_DEBUG, loggerName, logMessage, args);
    va_end(args);
}

NLOGC_API void NLog_InfoW(const wchar_t * loggerName, const wchar_t * logMessage, ...)
{
    va_list args;
    va_start(args, logMessage);
    NLog_LogVW(NLOG_INFO, loggerName, logMessage, args);
    va_end(args);
}

NLOGC_API void NLog_WarnW(const wchar_t * loggerName, const wchar_t * logMessage, ...)
{
    va_list args;
    va_start(args, logMessage);
    NLog_LogVW(NLOG_WARN, loggerName, logMessage, args);
    va_end(args);
}

NLOGC_API void NLog_ErrorW(const wchar_t * loggerName, const wchar_t * logMessage, ...)
{
    va_list args;
    va_start(args, logMessage);
    NLog_LogVW(NLOG_ERROR, loggerName, logMessage, args);
    va_end(args);
}

NLOGC_API void NLog_FatalW(const wchar_t * loggerName, const wchar_t * logMessage, ...)
{
    va_list args;
    va_start(args, logMessage);
    NLog_LogVW(NLOG_FATAL, loggerName, logMessage, args);
    va_end(args);
}

NLOGC_API void NLog_LogW(NLogLevel level, const wchar_t * loggerName, const wchar_t * logMessage, ...)
{
    va_list args;
    va_start(args, logMessage);
    NLog_LogVW(level, loggerName, logMessage, args);
    va_end(args);
}

NLOGC_API void NLog_LogVW(NLogLevel level, const wchar_t * loggerName, const wchar_t * logMessage, va_list args)
{
    if (0 != wcschr(logMessage, L'%'))
    {
        if (IsLogEnabledW(level, loggerName))
        {
		    wchar_t messageBuffer[NLOG_BUFFER_SIZE];

            _vsnwprintf_s(messageBuffer, NLOG_BUFFER_SIZE - 1, logMessage, args);
            WriteToW(level, loggerName, messageBuffer);
        }
    }
    else
    {
        WriteToW(level, loggerName, logMessage);
    }
}

NLOGC_API int NLog_ConfigureFromFileA(const char * fileName)
{
    return ConfigureFromFileA(fileName) ? 1 : 0;
}

NLOGC_API int NLog_ConfigureFromFileW(const wchar_t * fileName)
{
    return ConfigureFromFileW(fileName) ? 1 : 0;
}

NLOGC_API int NLog_ConfigureFromXmlA(const char *xml)
{
    return ConfigureFromXmlA(xml) ? 1 : 0;
}

NLOGC_API int NLog_ConfigureFromXmlW(const wchar_t *xml)
{
    return ConfigureFromXmlW(xml) ? 1 : 0;
}

NLOGC_API int NLog_InitA(const char *nlogDllPath)
{
    return Managed_Init(nlogDllPath);
}

NLOGC_API int NLog_InitW(const wchar_t *nlogDllPath)
{
    return Managed_Init(nlogDllPath);
}

NLOGC_API int NLog_InitLocal()
{
    return Managed_InitLocal();
}
