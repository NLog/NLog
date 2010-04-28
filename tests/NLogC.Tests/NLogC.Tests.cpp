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
		  <target name='console' type='ColoredConsole' layout='${longdate} ${message}'/>\
		  <target name='file' type='File' fileName='${basedir}/logs/${level}/${logger}.log' layout='${message}' />\
	      </targets>\
		  <rules>\
		    <logger name='*' minLevel='Trace' writeTo='console,file' />\
		  </rules>\
         </nlog>"));
    int repeatCount = 10;

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
 
	return 0;
}