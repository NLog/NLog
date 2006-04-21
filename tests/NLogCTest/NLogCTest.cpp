#include "stdafx.h"

#include "NLogC.h"

int main()
{
    NLog_ConfigureFromFile("config.nlog");
    int repeatCount = 500000;
    int t0, t1;

    t0 = GetTickCount();
    for (int i = 0; i < repeatCount; ++i)
    {
        NLog_DebugA("nnn", "message");
    }
    t1 = GetTickCount();
    printf("ANSI: %f nanoseconds per null-log\n", (t1 - t0) * 1000000.0 / repeatCount);

    t0 = GetTickCount();
    for (int i = 0; i < repeatCount; ++i)
    {
        NLog_DebugA("nnn", "message with %s", "sprintf() formatting");
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