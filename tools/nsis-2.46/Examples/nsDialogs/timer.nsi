!include LogicLib.nsh
!include nsDialogs.nsh

Name "nsDialogs Timer Example"
OutFile "nsDialogs Timer Example.exe"
XPStyle on

Var DIALOG
Var TEXT
Var PROGBAR
Var PROGBAR2
Var PROGBAR3
Var BUTTON
Var BUTTON2

Page custom nsDialogsPage

Function OnTimer

  	SendMessage $PROGBAR ${PBM_GETPOS} 0 0 $1
	${If} $1 = 100
		SendMessage $PROGBAR ${PBM_SETPOS} 0 0
	${Else}
		SendMessage $PROGBAR ${PBM_DELTAPOS} 10 0
	${EndIf}

FunctionEnd

Function OnTimer2

  	SendMessage $PROGBAR2 ${PBM_GETPOS} 0 0 $1
	${If} $1 = 100
		SendMessage $PROGBAR2 ${PBM_SETPOS} 0 0
	${Else}
		SendMessage $PROGBAR2 ${PBM_DELTAPOS} 5 0
	${EndIf}

FunctionEnd

Function OnTimer3

  	SendMessage $PROGBAR3 ${PBM_GETPOS} 0 0 $1
	${If} $1 >= 100
		${NSD_KillTimer} OnTimer3
 		MessageBox MB_OK "Timer 3 killed"
	${Else}
		SendMessage $PROGBAR3 ${PBM_DELTAPOS} 2 0
	${EndIf}

FunctionEnd

Function OnClick

	Pop $0

	${NSD_KillTimer} OnTimer

FunctionEnd

Function OnClick2

	Pop $0

	${NSD_KillTimer} OnTimer2

FunctionEnd

Function nsDialogsPage

	nsDialogs::Create 1018
	Pop $DIALOG

	${NSD_CreateLabel} 0u 0u 100% 9u "nsDialogs timer example"
	Pop $TEXT

	${NSD_CreateProgressBar} 0u 10u 100% 12u ""
	Pop $PROGBAR

	${NSD_CreateButton} 0u 25u 100u 14u "Kill Timer 1"
	Pop $BUTTON
	${NSD_OnClick} $BUTTON OnClick

	${NSD_CreateProgressBar} 0u 52u 100% 12u ""
	Pop $PROGBAR2

	${NSD_CreateButton} 0u 67u 100u 14u "Kill Timer 2"
	Pop $BUTTON2
	${NSD_OnClick} $BUTTON2 OnClick2

	${NSD_CreateProgressBar} 0u 114u 100% 12u ""
	Pop $PROGBAR3

	${NSD_CreateTimer} OnTimer 1000
	${NSD_CreateTimer} OnTimer2 100
	${NSD_CreateTimer} OnTimer3 200

	nsDialogs::Show

FunctionEnd

Section
SectionEnd
