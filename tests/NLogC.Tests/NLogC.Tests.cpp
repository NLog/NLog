// NLogC.Tests.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"

#include "NLogC.h"

int main()
{
    NLog_InitLocal();
    //NLog_ConfigureFromFile(_T("config.nlog"));
	NLog_ConfigureFromXml(_T(
		"<nlog>\
		  <targets>\
		  <target name='console' type='ColoredConsole' layout='${message}'/>\
	      </targets>\
		  <rules>\
		    <logger name='*' minLevel='Trace' writeTo='console' />\
		  </rules>\
         </nlog>"));
    int repeatCount = 500;
    int t0, t1;

    t0 = GetTickCount();
    for (int i = 0; i < repeatCount; ++i)
    {
		switch (i % 6)
		{
			case 0: NLog_TraceA("nnn", "message"); break;
			case 1: NLog_DebugA("nnn", "message"); break;
			case 2: NLog_InfoA("nnn", "message"); break;
			case 3: NLog_WarnA("nnn", "message"); break;
			case 4: NLog_ErrorA("nnn", "message"); break;
			case 5: NLog_FatalA("nnn", "message"); break;
		}
    }
    t1 = GetTickCount();
    printf("ANSI: %f nanoseconds per null-log\n", (t1 - t0) * 1000000.0 / repeatCount);

    t0 = GetTickCount();
    for (int i = 0; i < repeatCount; ++i)
    {
		switch (i % 6)
		{
			case 0: NLog_TraceA("nnn", "message with %s", "sprintf() formatting"); break;
			case 1: NLog_DebugA("nnn", "message with %s", "sprintf() formatting"); break;
			case 2: NLog_InfoA("nnn", "message with %s", "sprintf() formatting"); break;
			case 3: NLog_WarnA("nnn", "message with %s", "sprintf() formatting"); break;
			case 4: NLog_ErrorA("nnn", "message with %s", "sprintf() formatting"); break;
			case 5: NLog_FatalA("nnn", "message with %s", "sprintf() formatting"); break;
		}
    }
    t1 = GetTickCount();
    printf("ANSI: %f nanoseconds per null-log with sprintf() formatting\n", (t1 - t0) * 1000000.0 / repeatCount);

    t0 = GetTickCount();
    for (int i = 0; i < repeatCount; ++i)
    {
        NLog_DebugA("rrr", "message");
    }
    t1 = GetTickCount();
    printf("ANSI: %f nanoseconds per non-logging\n", (t1 - t0) * 1000000.0 / repeatCount);

    t0 = GetTickCount();
    for (int i = 0; i < repeatCount; ++i)
    {
        NLog_DebugA("rrr", "message with %s", "stringf() formatting");
    }
    t1 = GetTickCount();
    printf("ANSI: %f nanoseconds per non-logging with sprintf() formatting\n", (t1 - t0) * 1000000.0 / repeatCount);

    t0 = GetTickCount();
    for (int i = 0; i < repeatCount; ++i)
    {
        NLog_DebugW(L"nnn", L"message");
    }
    t1 = GetTickCount();
    printf("UNICODE: %f nanoseconds per null-log\n", (t1 - t0) * 1000000.0 / repeatCount);

    t0 = GetTickCount();
    for (int i = 0; i < repeatCount; ++i)
    {
        NLog_DebugW(L"nnn", L"message with %s", L"stringf() formatting");
    }
    t1 = GetTickCount();
    printf("UNICODE: %f nanoseconds per null-logging with sprintf() formatting\n", (t1 - t0) * 1000000.0 / repeatCount);

    t0 = GetTickCount();
    for (int i = 0; i < repeatCount; ++i)
    {
        NLog_DebugW(L"rrr", L"message");
    }
    t1 = GetTickCount();
    printf("UNICODE: %f nanoseconds per non-logging\n", (t1 - t0) * 1000000.0 / repeatCount);

    t0 = GetTickCount();
    for (int i = 0; i < repeatCount; ++i)
    {
        NLog_DebugW(L"rrr", L"message with %s", L"stringf() formatting");
    }
    t1 = GetTickCount();
    printf("UNICODE: %f nanoseconds per non-logging with sprintf() formatting\n", (t1 - t0) * 1000000.0 / repeatCount);

	return 0;
}