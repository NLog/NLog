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