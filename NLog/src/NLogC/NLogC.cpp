#include "stdafx.h"
#include <stdio.h>
#include "NLogC.h"

#define NLOG_BUFFER_SIZE 8192

#pragma managed

static void WriteToA(NLogLevel level, LPCSTR loggerName, LPCSTR messageBuffer)
{
    NLog::Logger *logger = NLog::LogManager::GetLogger(loggerName);
    logger->Log((NLog::LogLevel)(int)level, messageBuffer);
}

static void WriteToW(NLogLevel level, LPCWSTR loggerName, LPCWSTR messageBuffer)
{
    NLog::Logger *logger = NLog::LogManager::GetLogger(loggerName);
    logger->Log((NLog::LogLevel)(int)level, messageBuffer);
}

#pragma unmanaged

NLOGC_API void NLog_DebugA(LPCSTR loggerName, LPCSTR logMessage, ...)
{
    char messageBuffer[NLOG_BUFFER_SIZE];
    va_list args;

    va_start(args, loggerName);
    _vsnprintf(messageBuffer, sizeof(messageBuffer), logMessage, args);
    va_end(args);
    WriteToA(NLOG_DEBUG, loggerName, messageBuffer);
}

NLOGC_API void NLog_InfoA(LPCSTR loggerName, LPCSTR logMessage, ...)
{
    char messageBuffer[NLOG_BUFFER_SIZE];
    va_list args;

    va_start(args, loggerName);
    _vsnprintf(messageBuffer, sizeof(messageBuffer), logMessage, args);
    va_end(args);
    WriteToA(NLOG_INFO, loggerName, messageBuffer);
}

NLOGC_API void NLog_WarnA(LPCSTR loggerName, LPCSTR logMessage, ...)
{
    char messageBuffer[NLOG_BUFFER_SIZE];
    va_list args;

    va_start(args, loggerName);
    _vsnprintf(messageBuffer, sizeof(messageBuffer), logMessage, args);
    va_end(args);
    WriteToA(NLOG_WARN, loggerName, messageBuffer);
}

NLOGC_API void NLog_ErrorA(LPCSTR loggerName, LPCSTR logMessage, ...)
{
    char messageBuffer[NLOG_BUFFER_SIZE];
    va_list args;

    va_start(args, loggerName);
    _vsnprintf(messageBuffer, sizeof(messageBuffer), logMessage, args);
    va_end(args);
    WriteToA(NLOG_ERROR, loggerName, messageBuffer);
}

NLOGC_API void NLog_FatalA(LPCSTR loggerName, LPCSTR logMessage, ...)
{
    char messageBuffer[NLOG_BUFFER_SIZE];
    va_list args;

    va_start(args, loggerName);
    _vsnprintf(messageBuffer, sizeof(messageBuffer), logMessage, args);
    va_end(args);
    WriteToA(NLOG_FATAL, loggerName, messageBuffer);
}

NLOGC_API void NLog_LogA(NLogLevel level, LPCSTR loggerName, LPCSTR logMessage, ...)
{
    char messageBuffer[NLOG_BUFFER_SIZE];
    va_list args;

    va_start(args, loggerName);
    _vsnprintf(messageBuffer, sizeof(messageBuffer), logMessage, args);
    va_end(args);
    WriteToA(level, loggerName, messageBuffer);
}

NLOGC_API void NLog_LogVA(NLogLevel level, LPCSTR loggerName, LPCSTR logMessage, va_list args)
{
    char messageBuffer[NLOG_BUFFER_SIZE];
    _vsnprintf(messageBuffer, sizeof(messageBuffer), logMessage, args);
    WriteToA(level, loggerName, messageBuffer);
}


NLOGC_API void NLog_DebugW(LPCWSTR loggerName, LPCWSTR logMessage, ...)
{
    wchar_t messageBuffer[NLOG_BUFFER_SIZE];
    va_list args;

    va_start(args, loggerName);
    _vsnwprintf(messageBuffer, sizeof(messageBuffer), logMessage, args);
    va_end(args);
    WriteToW(NLOG_DEBUG, loggerName, messageBuffer);
}

NLOGC_API void NLog_InfoW(LPCWSTR loggerName, LPCWSTR logMessage, ...)
{
    wchar_t messageBuffer[NLOG_BUFFER_SIZE];
    va_list args;

    va_start(args, loggerName);
    _vsnwprintf(messageBuffer, sizeof(messageBuffer), logMessage, args);
    va_end(args);
    WriteToW(NLOG_INFO, loggerName, messageBuffer);
}

NLOGC_API void NLog_WarnW(LPCWSTR loggerName, LPCWSTR logMessage, ...)
{
    wchar_t messageBuffer[NLOG_BUFFER_SIZE];
    va_list args;

    va_start(args, loggerName);
    _vsnwprintf(messageBuffer, sizeof(messageBuffer), logMessage, args);
    va_end(args);
    WriteToW(NLOG_WARN, loggerName, messageBuffer);
}

NLOGC_API void NLog_ErrorW(LPCWSTR loggerName, LPCWSTR logMessage, ...)
{
    wchar_t messageBuffer[NLOG_BUFFER_SIZE];
    va_list args;

    va_start(args, loggerName);
    _vsnwprintf(messageBuffer, sizeof(messageBuffer), logMessage, args);
    va_end(args);
    WriteToW(NLOG_ERROR, loggerName, messageBuffer);
}

NLOGC_API void NLog_FatalW(LPCWSTR loggerName, LPCWSTR logMessage, ...)
{
    wchar_t messageBuffer[NLOG_BUFFER_SIZE];
    va_list args;

    va_start(args, loggerName);
    _vsnwprintf(messageBuffer, sizeof(messageBuffer), logMessage, args);
    va_end(args);
    WriteToW(NLOG_FATAL, loggerName, messageBuffer);
}

NLOGC_API void NLog_LogW(NLogLevel level, LPCWSTR loggerName, LPCWSTR logMessage, ...)
{
    wchar_t messageBuffer[NLOG_BUFFER_SIZE];
    va_list args;

    va_start(args, loggerName);
    _vsnwprintf(messageBuffer, sizeof(messageBuffer), logMessage, args);
    va_end(args);
    WriteToW(level, loggerName, messageBuffer);
}

NLOGC_API void NLog_LogVW(NLogLevel level, LPCWSTR loggerName, LPCWSTR logMessage, va_list args)
{
    wchar_t messageBuffer[NLOG_BUFFER_SIZE];
    _vsnwprintf(messageBuffer, sizeof(messageBuffer), logMessage, args);
    WriteToW(level, loggerName, messageBuffer);
}