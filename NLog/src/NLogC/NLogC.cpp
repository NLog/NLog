#include "stdafx.h"
#include <stdio.h>
#include "NLogC.h"
#include <string.h>
#include <stdarg.h>

#define NLOG_BUFFER_SIZE 8192

#pragma managed

#ifdef __cplusplus_cli

#define MANAGED_REFERENCE(a) a^
#define NEW_MANAGED_OBJECT gcnew

inline System::String^ CharArrayToString(const char *str)
{
    return gcnew System::String(str);        
}

inline System::String^ CharArrayToString(const wchar_t *str)
{
    return gcnew System::String(str);        
}

#else

#define MANAGED_REFERENCE(a) a *
#define NEW_MANAGED_OBJECT new
#define CharArrayToString(a) a

#endif

static void WriteToA(NLogLevel level, const char * loggerName, const char * messageBuffer)
{
    MANAGED_REFERENCE(NLog::Logger) logger = NLog::LogManager::GetLogger(CharArrayToString(loggerName));

    switch (level)
    {
    case NLOG_TRACE:
        if (logger->IsTraceEnabled)
            logger->Trace(CharArrayToString(messageBuffer));
        break;
    case NLOG_DEBUG:
        if (logger->IsDebugEnabled)
            logger->Debug(CharArrayToString(messageBuffer));
        break;
    case NLOG_INFO:
        if (logger->IsInfoEnabled)
            logger->Info(CharArrayToString(messageBuffer));
        break;
    case NLOG_WARN:
        if (logger->IsWarnEnabled)
            logger->Warn(CharArrayToString(messageBuffer));
        break;
    case NLOG_ERROR:
        if (logger->IsErrorEnabled)
            logger->Error(CharArrayToString(messageBuffer));
        break;
    case NLOG_FATAL:
        if (logger->IsFatalEnabled)
            logger->Fatal(CharArrayToString(messageBuffer));
        break;
    }
}

static void WriteToW(NLogLevel level, const wchar_t * loggerName, const wchar_t * messageBuffer)
{
    MANAGED_REFERENCE(NLog::Logger) logger = NLog::LogManager::GetLogger(CharArrayToString(loggerName));
    switch (level)
    {
    case NLOG_TRACE:
        if (logger->IsTraceEnabled)
            logger->Trace(CharArrayToString(messageBuffer));
        break;
    case NLOG_DEBUG:
        if (logger->IsDebugEnabled)
            logger->Debug(CharArrayToString(messageBuffer));
        break;
    case NLOG_INFO:
        if (logger->IsInfoEnabled)
            logger->Info(CharArrayToString(messageBuffer));
        break;
    case NLOG_WARN:
        if (logger->IsWarnEnabled)
            logger->Warn(CharArrayToString(messageBuffer));
        break;
    case NLOG_ERROR:
        if (logger->IsErrorEnabled)
            logger->Error(CharArrayToString(messageBuffer));
        break;
    case NLOG_FATAL:
        if (logger->IsFatalEnabled)
            logger->Fatal(CharArrayToString(messageBuffer));
        break;
    }
}

static bool IsLogEnabledA(NLogLevel level, const char * loggerName)
{
    MANAGED_REFERENCE(NLog::Logger) logger = NLog::LogManager::GetLogger(CharArrayToString(loggerName));
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
    MANAGED_REFERENCE(NLog::Logger) logger = NLog::LogManager::GetLogger(CharArrayToString(loggerName));
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
        NLog::LogManager::Configuration = NEW_MANAGED_OBJECT NLog::Config::XmlLoggingConfiguration(CharArrayToString(fileName));
        return true;
    }
    catch (MANAGED_REFERENCE(System::Exception))
    {
        return false;
    }
}

static bool ConfigureFromFileW(const wchar_t * fileName)
{
    try
    {
        NLog::LogManager::Configuration = NEW_MANAGED_OBJECT NLog::Config::XmlLoggingConfiguration(CharArrayToString(fileName));
        return true;
    }
    catch (MANAGED_REFERENCE(System::Exception))
    {
        return false;
    }
}

#pragma unmanaged

NLOGC_API void NLog_TraceA(const char * loggerName, const char * logMessage, ...)
{
    va_list args;
    va_start(args, loggerName);
    NLog_LogVA(NLOG_TRACE, loggerName, logMessage, args);
    va_end(args);
}

NLOGC_API void NLog_DebugA(const char * loggerName, const char * logMessage, ...)
{
    va_list args;
    va_start(args, loggerName);
    NLog_LogVA(NLOG_DEBUG, loggerName, logMessage, args);
    va_end(args);
}

NLOGC_API void NLog_InfoA(const char * loggerName, const char * logMessage, ...)
{
    va_list args;
    va_start(args, loggerName);
    NLog_LogVA(NLOG_INFO, loggerName, logMessage, args);
    va_end(args);
}

NLOGC_API void NLog_WarnA(const char * loggerName, const char * logMessage, ...)
{
    va_list args;
    va_start(args, loggerName);
    NLog_LogVA(NLOG_WARN, loggerName, logMessage, args);
    va_end(args);
}

NLOGC_API void NLog_ErrorA(const char * loggerName, const char * logMessage, ...)
{
    va_list args;
    va_start(args, loggerName);
    NLog_LogVA(NLOG_ERROR, loggerName, logMessage, args);
    va_end(args);
}

NLOGC_API void NLog_FatalA(const char * loggerName, const char * logMessage, ...)
{
    va_list args;
    va_start(args, loggerName);
    NLog_LogVA(NLOG_FATAL, loggerName, logMessage, args);
    va_end(args);
}

NLOGC_API void NLog_LogA(NLogLevel level, const char * loggerName, const char * logMessage, ...)
{
    va_list args;
    va_start(args, loggerName);
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
            _vsnprintf(messageBuffer, sizeof(messageBuffer), logMessage, args);
            WriteToA(level, loggerName, messageBuffer);
        }
    }
    else
    {
        WriteToA(level, loggerName, logMessage);
    }
}

NLOGC_API void NLog_TraceW(const wchar_t * loggerName, const wchar_t * logMessage, ...)
{
    va_list args;
    va_start(args, loggerName);
    NLog_LogVW(NLOG_TRACE, loggerName, logMessage, args);
    va_end(args);
}

NLOGC_API void NLog_DebugW(const wchar_t * loggerName, const wchar_t * logMessage, ...)
{
    va_list args;
    va_start(args, loggerName);
    NLog_LogVW(NLOG_DEBUG, loggerName, logMessage, args);
    va_end(args);
}

NLOGC_API void NLog_InfoW(const wchar_t * loggerName, const wchar_t * logMessage, ...)
{
    va_list args;
    va_start(args, loggerName);
    NLog_LogVW(NLOG_INFO, loggerName, logMessage, args);
    va_end(args);
}

NLOGC_API void NLog_WarnW(const wchar_t * loggerName, const wchar_t * logMessage, ...)
{
    va_list args;
    va_start(args, loggerName);
    NLog_LogVW(NLOG_WARN, loggerName, logMessage, args);
    va_end(args);
}

NLOGC_API void NLog_ErrorW(const wchar_t * loggerName, const wchar_t * logMessage, ...)
{
    va_list args;
    va_start(args, loggerName);
    NLog_LogVW(NLOG_ERROR, loggerName, logMessage, args);
    va_end(args);
}

NLOGC_API void NLog_FatalW(const wchar_t * loggerName, const wchar_t * logMessage, ...)
{
    va_list args;
    va_start(args, loggerName);
    NLog_LogVW(NLOG_FATAL, loggerName, logMessage, args);
    va_end(args);
}

NLOGC_API void NLog_LogW(NLogLevel level, const wchar_t * loggerName, const wchar_t * logMessage, ...)
{
    va_list args;
    va_start(args, loggerName);
    NLog_LogVW(level, loggerName, logMessage, args);
    va_end(args);
}

NLOGC_API void NLog_LogVW(NLogLevel level, const wchar_t * loggerName, const wchar_t * logMessage, va_list args)
{
    wchar_t messageBuffer[NLOG_BUFFER_SIZE];
    if (0 != wcschr(logMessage, L'%'))
    {
        if (IsLogEnabledW(level, loggerName))
        {
            _vsnwprintf(messageBuffer, sizeof(messageBuffer), logMessage, args);
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
