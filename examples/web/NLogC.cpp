
// 
// ANSI functions
// 
int NLog_ConfigureFromFileA(const char *fileName);
void NLog_LogA(NLogLevel level, const char *loggerName, const char *logMessage, ...); 
void NLog_TraceA(const char *loggerName, const char *logMessage, ...); 
void NLog_DebugA(const char *loggerName, const char *logMessage, ...); 
void NLog_InfoA(const char *loggerName, const char *logMessage, ...); 
void NLog_WarnA(const char *loggerName, const char *logMessage, ...); 
void NLog_ErrorA(const char *loggerName, const char *logMessage, ...); 
void NLog_FatalA(const char *loggerName, const char *logMessage, ...); 
void NLog_LogVA(NLogLevel level, const char *loggerName, const char *logMessage, va_list args);

// 
// Unicode functions
// 
int NLog_ConfigureFromFileW(const wchar_t *fileName);
void NLog_LogW(NLogLevel level, const wchar_t *loggerName, const wchar_t *logMessage, ...); 
void NLog_TraceW(const wchar_t *loggerName, const wchar_t *logMessage, ...); 
void NLog_DebugW(const wchar_t *loggerName, const wchar_t *logMessage, ...); 
void NLog_InfoW(const wchar_t *loggerName, const wchar_t *logMessage, ...); 
void NLog_WarnW(const wchar_t *loggerName, const wchar_t *logMessage, ...); 
void NLog_ErrorW(const wchar_t *loggerName, const wchar_t *logMessage, ...); 
void NLog_FatalW(const wchar_t *loggerName, const wchar_t *logMessage, ...); 
void NLog_LogVW(NLogLevel level, const wchar_t *loggerName, const wchar_t *logMessage, va_list args);
