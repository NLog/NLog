#include "stdafx.h"
#include <stdio.h>
#include "NLogC.h"

#include <map>

#define NLOG_BUFFER_SIZE 8192

#pragma managed

static void WriteToA(NLogLevel level, LPCSTR loggerName, LPCSTR messageBuffer)
{
    NLog::Logger *logger = NLog::LogManager::GetLogger(loggerName);
    switch (level)
    {
    case NLOG_DEBUG:
        if (logger->IsDebugEnabled)
            logger->Debug(messageBuffer);
        break;
    case NLOG_INFO:
        if (logger->IsInfoEnabled)
            logger->Info(messageBuffer);
        break;
    case NLOG_WARN:
        if (logger->IsWarnEnabled)
            logger->Warn(messageBuffer);
        break;
    case NLOG_ERROR:
        if (logger->IsErrorEnabled)
            logger->Error(messageBuffer);
        break;
    case NLOG_FATAL:
        if (logger->IsFatalEnabled)
            logger->Fatal(messageBuffer);
        break;
    }
}

static void WriteToW(NLogLevel level, LPCWSTR loggerName, LPCWSTR messageBuffer)
{
    NLog::Logger *logger = NLog::LogManager::GetLogger(loggerName);
    switch (level)
    {
    case NLOG_DEBUG:
        if (logger->IsDebugEnabled)
            logger->Debug(messageBuffer);
        break;
    case NLOG_INFO:
        if (logger->IsInfoEnabled)
            logger->Info(messageBuffer);
        break;
    case NLOG_WARN:
        if (logger->IsWarnEnabled)
            logger->Warn(messageBuffer);
        break;
    case NLOG_ERROR:
        if (logger->IsErrorEnabled)
            logger->Error(messageBuffer);
        break;
    case NLOG_FATAL:
        if (logger->IsFatalEnabled)
            logger->Fatal(messageBuffer);
        break;
    }
}

static bool IsLogEnabledA(NLogLevel level, LPCSTR loggerName)
{
    NLog::Logger *logger = NLog::LogManager::GetLogger(loggerName);
    switch (level)
    {
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
    }
}

static bool IsLogEnabledW(NLogLevel level, LPCWSTR loggerName)
{
    NLog::Logger *logger = NLog::LogManager::GetLogger(loggerName);
    switch (level)
    {
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
    }
}

static BOOL ConfigureFromFileA(LPCSTR fileName)
{
    try
    {
        NLog::LogManager::Configuration = new NLog::Config::XmlLoggingConfiguration(fileName);
        return TRUE;
    }
    catch (System::Exception *)
    {
        return FALSE;
    }
}

static BOOL ConfigureFromFileW(LPCWSTR fileName)
{
    try
    {
        NLog::LogManager::Configuration = new NLog::Config::XmlLoggingConfiguration(fileName);
        return TRUE;
    }
    catch (System::Exception *)
    {
        return FALSE;
    }
}

#pragma unmanaged

NLOGC_API void NLog_DebugA(LPCSTR loggerName, LPCSTR logMessage, ...)
{
    va_list args;
    va_start(args, loggerName);
    NLog_LogVA(NLOG_DEBUG, loggerName, logMessage, args);
    va_end(args);
}

NLOGC_API void NLog_InfoA(LPCSTR loggerName, LPCSTR logMessage, ...)
{
    va_list args;
    va_start(args, loggerName);
    NLog_LogVA(NLOG_INFO, loggerName, logMessage, args);
    va_end(args);
}

NLOGC_API void NLog_WarnA(LPCSTR loggerName, LPCSTR logMessage, ...)
{
    va_list args;
    va_start(args, loggerName);
    NLog_LogVA(NLOG_WARN, loggerName, logMessage, args);
    va_end(args);
}

NLOGC_API void NLog_ErrorA(LPCSTR loggerName, LPCSTR logMessage, ...)
{
    va_list args;
    va_start(args, loggerName);
    NLog_LogVA(NLOG_ERROR, loggerName, logMessage, args);
    va_end(args);
}

NLOGC_API void NLog_FatalA(LPCSTR loggerName, LPCSTR logMessage, ...)
{
    va_list args;
    va_start(args, loggerName);
    NLog_LogVA(NLOG_FATAL, loggerName, logMessage, args);
    va_end(args);
}

NLOGC_API void NLog_LogA(NLogLevel level, LPCSTR loggerName, LPCSTR logMessage, ...)
{
    va_list args;
    va_start(args, loggerName);
    NLog_LogVA(level, loggerName, logMessage, args);
    va_end(args);
}

NLOGC_API void NLog_LogVA(NLogLevel level, LPCSTR loggerName, LPCSTR logMessage, va_list args)
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


NLOGC_API void NLog_DebugW(LPCWSTR loggerName, LPCWSTR logMessage, ...)
{
    va_list args;
    va_start(args, loggerName);
    NLog_LogVW(NLOG_DEBUG, loggerName, logMessage, args);
    va_end(args);
}

NLOGC_API void NLog_InfoW(LPCWSTR loggerName, LPCWSTR logMessage, ...)
{
    va_list args;
    va_start(args, loggerName);
    NLog_LogVW(NLOG_INFO, loggerName, logMessage, args);
    va_end(args);
}

NLOGC_API void NLog_WarnW(LPCWSTR loggerName, LPCWSTR logMessage, ...)
{
    va_list args;
    va_start(args, loggerName);
    NLog_LogVW(NLOG_WARN, loggerName, logMessage, args);
    va_end(args);
}

NLOGC_API void NLog_ErrorW(LPCWSTR loggerName, LPCWSTR logMessage, ...)
{
    va_list args;
    va_start(args, loggerName);
    NLog_LogVW(NLOG_ERROR, loggerName, logMessage, args);
    va_end(args);
}

NLOGC_API void NLog_FatalW(LPCWSTR loggerName, LPCWSTR logMessage, ...)
{
    va_list args;
    va_start(args, loggerName);
    NLog_LogVW(NLOG_FATAL, loggerName, logMessage, args);
    va_end(args);
}

NLOGC_API void NLog_LogW(NLogLevel level, LPCWSTR loggerName, LPCWSTR logMessage, ...)
{
    va_list args;
    va_start(args, loggerName);
    NLog_LogVW(level, loggerName, logMessage, args);
    va_end(args);
}

NLOGC_API void NLog_LogVW(NLogLevel level, LPCWSTR loggerName, LPCWSTR logMessage, va_list args)
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

NLOGC_API int NLog_ConfigureFromFileA(LPCSTR fileName)
{
    return ConfigureFromFileA(fileName);
}

NLOGC_API int NLog_ConfigureFromFileW(LPCWSTR fileName)
{
    return ConfigureFromFileW(fileName);
}