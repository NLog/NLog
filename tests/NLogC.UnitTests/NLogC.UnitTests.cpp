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

#include "targetver.h"

#define WIN32_LEAN_AND_MEAN
#define _CRT_SECURE_NO_WARNINGS

#include <stdio.h>
#include <tchar.h>
#include <windows.h>

#include <NLogC.h>

void GetOutputFileName(TCHAR *buffer, int bufferSize)
{
    TCHAR *p;

    // compute the name of the output file
    GetModuleFileName(NULL, buffer, bufferSize);
    p = _tcsrchr(buffer, _T('\\'));
    _tcscpy(p, _T("\\nlogc.out"));
}

int _tmain(int argc, _TCHAR* argv[])
{
    // NLog_Init(_T("NLog.dll"));
    // NLog_InitLocal(); 

    TCHAR outputFile[MAX_PATH];
    GetOutputFileName(outputFile, sizeof(outputFile) / sizeof(outputFile[0]));

    DeleteFile(outputFile);

    // output log messages using TCHAR strings
    NLog_Trace(_T("logger1"), _T("message %d %d '%s'"), 10, 20, _T("foo"));
    NLog_Debug(_T("logger2"), _T("message %d %d '%s'"), 10, 20, _T("foo"));
    NLog_Info(_T("logger3"), _T("message %d %d '%s'"), 10, 20, _T("foo"));
    NLog_Warn(_T("logger4"), _T("message %d %d '%s'"), 10, 20, _T("foo"));
    NLog_Error(_T("logger5"), _T("message %d %d '%s'"), 10, 20, _T("foo"));
    NLog_Fatal(_T("logger6"), _T("message %d %d '%s'"), 10, 20, _T("foo"));

    // output log messages using Unicode strings
    NLog_TraceW(L"logger1", L"message %d %d '%s'", 10, 20, L"foo");
    NLog_DebugW(L"logger2", L"message %d %d '%s'", 10, 20, L"foo");
    NLog_InfoW(L"logger3", L"message %d %d '%s'", 10, 20, L"foo");
    NLog_WarnW(L"logger4", L"message %d %d '%s'", 10, 20, L"foo");
    NLog_ErrorW(L"logger5", L"message %d %d '%s'", 10, 20, L"foo");
    NLog_FatalW(L"logger6", L"message %d %d '%s'", 10, 20, L"foo");

    // output log messages using ANSI strings
    NLog_TraceA("logger1", "message %d %d '%s'", 10, 20, "foo");
    NLog_DebugA("logger2", "message %d %d '%s'", 10, 20, "foo");
    NLog_InfoA("logger3", "message %d %d '%s'", 10, 20, "foo");
    NLog_WarnA("logger4", "message %d %d '%s'", 10, 20, "foo");
    NLog_ErrorA("logger5", "message %d %d '%s'", 10, 20, "foo");
    NLog_FatalA("logger6", "message %d %d '%s'", 10, 20, "foo");

    int errorCount = 0;
    FILE *in = _tfopen(outputFile, _T("rt"));
    if (in)
    {
        char line[512];
        int l = 1;
        
        while (fgets(line, 512, in))
        {
            char *tok = strtok(line, "|\n");
            char *tok2 = strtok(0, "|\n");
            char *tok3 = strtok(0, "|\n");

            if (0 != strcmp(tok2, "message 10 20 'foo'"))
            {
                printf("ERROR: Invalid message in line %d.\n", l);
                errorCount++;
            }

            int lineNumber;
            if (sscanf(tok3, "%d", &lineNumber) != 1)
            {
                printf("ERROR: Invalid counter number format in line %d.\n", l);
                errorCount++;
            }

            if (lineNumber != l)
            {
                printf("ERROR: Invalid counter in line %d.\n", l);
                errorCount++;
            }

            l++;
        }

        if (l != 19)
        {
            printf("ERROR: Invalid number of lines %d\n", l);
            errorCount++;
        }

        fclose(in);
    }
    else
    {
        _tprintf(_T("ERROR: Log file not found: %s!\n"), outputFile);
        errorCount++;
    }

    if (errorCount == 0)
    {
        printf("PASSED\n");
    }

    return errorCount;
}
