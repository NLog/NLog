#ifndef __NLOGC_H
#define __NLOGC_H

#ifdef NLOGC_EXPORTS
#define NLOGC_API __declspec(dllexport)
#else
#define NLOGC_API __declspec(dllimport)
#endif

enum NLogLevel
{
    NLOG_DEBUG,
    NLOG_INFO,
    NLOG_WARN,
    NLOG_ERROR,
    NLOG_FATAL
};

extern "C" {

NLOGC_API void NLog_LogA(NLogLevel level, const char *loggerName, const char *logMessage, ...); 
NLOGC_API void NLog_DebugA(const char *loggerName, const char *logMessage, ...); 
NLOGC_API void NLog_InfoA(const char *loggerName, const char *logMessage, ...); 
NLOGC_API void NLog_WarnA(const char *loggerName, const char *logMessage, ...); 
NLOGC_API void NLog_ErrorA(const char *loggerName, const char *logMessage, ...); 
NLOGC_API void NLog_FatalA(const char *loggerName, const char *logMessage, ...); 

NLOGC_API void NLog_LogW(NLogLevel level, const wchar_t *loggerName, const wchar_t *logMessage, ...); 
NLOGC_API void NLog_DebugW(const wchar_t *loggerName, const wchar_t *logMessage, ...); 
NLOGC_API void NLog_InfoW(const wchar_t *loggerName, const wchar_t *logMessage, ...); 
NLOGC_API void NLog_WarnW(const wchar_t *loggerName, const wchar_t *logMessage, ...); 
NLOGC_API void NLog_ErrorW(const wchar_t *loggerName, const wchar_t *logMessage, ...); 
NLOGC_API void NLog_FatalW(const wchar_t *loggerName, const wchar_t *logMessage, ...); 
NLOGC_API void NLog_LogVA(NLogLevel level, const char *loggerName, const char *logMessage, va_list args);
NLOGC_API void NLog_LogVW(NLogLevel level, const wchar_t *loggerName, const wchar_t *logMessage, va_list args); 

}

#ifdef UNICODE

#define NLog_Log NLog_LogW
#define NLog_Debug NLog_DebugW
#define NLog_Info NLog_InfoW
#define NLog_Warn NLog_WarnW
#define NLog_Error NLog_ErrorW
#define NLog_Fatal NLog_FatalW

#else

#define NLog_Log NLog_LogA
#define NLog_Debug NLog_DebugA
#define NLog_Info NLog_InfoA
#define NLog_Warn NLog_WarnA
#define NLog_Error NLog_ErrorA
#define NLog_Fatal NLog_FatalA

#endif

#endif // __NLOGC_H