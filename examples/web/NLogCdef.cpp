//
// TCHAR macros
// 
#ifdef UNICODE

#define NLog_Log NLog_LogW
#define NLog_LogV NLog_LogVW
#define NLog_Trace NLog_TraceW
#define NLog_Debug NLog_DebugW
#define NLog_Info NLog_InfoW
#define NLog_Warn NLog_WarnW
#define NLog_Error NLog_ErrorW
#define NLog_Fatal NLog_FatalW
#define NLog_ConfigureFromFile NLog_ConfigureFromFileW

#else

#define NLog_Log NLog_LogA
#define NLog_LogV NLog_LogVA
#define NLog_Trace NLog_TraceA
#define NLog_Debug NLog_DebugA
#define NLog_Info NLog_InfoA
#define NLog_Warn NLog_WarnA
#define NLog_Error NLog_ErrorA
#define NLog_Fatal NLog_FatalA
#define NLog_ConfigureFromFile NLog_ConfigureFromFileA

#endif
