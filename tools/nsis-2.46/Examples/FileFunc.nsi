;_____________________________________________________________________________
;
;                          File Functions
;_____________________________________________________________________________
;
; 2006 Shengalts Aleksander aka Instructor (Shengalts@mail.ru)

Name "File Functions"
OutFile "FileFunc.exe"
Caption "$(^Name)"
XPStyle on
RequestExecutionLevel user

!include "WinMessages.nsh"
!include "FileFunc.nsh"

Var INI
Var HWND
Var STATE
Var FUNCTION
Var LOCATE1
Var LOCATE2
Var GETSIZE1
Var GETSIZE2
Var GETSIZE3
Var GETSIZE4
Var GETSIZE5
Var GETSIZE6
Var DRIVESPACE1
Var DRIVESPACE2
Var GETDRIVES1
Var GETTIME1
Var GETTIME2
Var GETFILEATTRIBUTES1
Var GETFILEATTRIBUTES2
Var GETFILEVERSION1
Var GETOPTIONS1
Var GETOPTIONS2
Var GETROOT1
Var GETPARENT1
Var GETFILENAME1
Var GETBASENAME1
Var GETFILEEXT1
Var BANNERTRIMPATH1
Var BANNERTRIMPATH2
Var DIRSTATE1

Page Custom ShowCustom LeaveCustom

Function ShowCustom
	InstallOptions::initDialog "$INI"
	Pop $hwnd
	GetDlgItem $1 $HWND 1201
	ShowWindow $1 0
	GetDlgItem $1 $HWND 1202
	ShowWindow $1 0
	GetDlgItem $1 $HWND 1206
	EnableWindow $1 0
	SendMessage $1 ${WM_ENABLE} 1 0
	StrCpy $LOCATE1 $DOCUMENTS
	StrCpy $LOCATE2 '/L=FD /M=*.* /S=0B /G=1 /B=0'
	StrCpy $GETSIZE1 '$WINDIR'
	StrCpy $GETSIZE2 '/M=Explorer.exe /S=0K /G=0'
	StrCpy $GETSIZE3 '$PROGRAMFILES\Common Files'
	StrCpy $GETSIZE4 '/S=0M'
	StrCpy $GETSIZE5 '$WINDIR'
	StrCpy $GETSIZE6 '/G=0'
	StrCpy $DRIVESPACE1 'C:\'
	StrCpy $DRIVESPACE2 '/D=F /S=M'
	StrCpy $GETDRIVES1 'FDD+CDROM'
	StrCpy $GETTIME1 '$WINDIR\Explorer.exe'
	StrCpy $GETTIME2 'C'
	StrCpy $GETFILEATTRIBUTES1 'C:\IO.SYS'
	StrCpy $GETFILEATTRIBUTES2 'ALL'
	StrCpy $GETFILEVERSION1 '$WINDIR\Explorer.exe'
	StrCpy $GETOPTIONS1 '/SILENT=yes /INSTDIR="$PROGRAMFILES\Common Files"'
	StrCpy $GETOPTIONS2 '/INSTDIR='
	StrCpy $GETROOT1 'C:\path\file.dll'
	StrCpy $GETPARENT1 'C:\path\file.dll'
	StrCpy $GETFILENAME1 'C:\path\file.dll'
	StrCpy $GETBASENAME1 'C:\path\file.dll'
	StrCpy $GETFILEEXT1 'C:\path\file.dll'
	StrCpy $BANNERTRIMPATH1 'C:\Server\Documents\Terminal\license.htm'
	StrCpy $BANNERTRIMPATH2 '34A'
	StrCpy $DIRSTATE1 '$TEMP'

	GetDlgItem $1 $HWND 1203
	SendMessage $1 ${WM_SETTEXT} 1 "STR:$LOCATE1"
	GetDlgItem $1 $HWND 1205
	SendMessage $1 ${WM_SETTEXT} 1 "STR:$LOCATE2"
	InstallOptions::show
	Pop $0
FunctionEnd

Function LeaveCustom
	ReadINIStr $STATE $INI "Field 1" "State"
	ReadINIStr $R1 $INI "Field 2" "State"
	ReadINIStr $R2 $INI "Field 3" "State"
	ReadINIStr $R3 $INI "Field 4" "State"
	ReadINIStr $R4 $INI "Field 5" "State"
	ReadINIStr $0 $INI "Settings" "State"
	StrCmp $0 6 view
	StrCmp $0 0 Enter
	goto main

	view:
	StrCpy $0 '$$'
	StrCpy $1 'n'
	StrCpy $2 'r'
	StrCmp $R4 "LocateCallback" 0 +3
	StrCpy $R0 `Function LocateCallback$\r$\n	MessageBox MB_OKCANCEL '$0$$R9    "path\name"=[$$R9]$0\$1$0$$R8    "path"          =[$$R8]$0\$1$0$$R7    "name"        =[$$R7]$0\$1$0$$R6    "size"           =[$$R6]' IDOK +2$\r$\n	StrCpy $$R0 StopLocate$\r$\n$\r$\n	Push $$R0$\r$\nFunctionEnd`
	goto send
	StrCmp $R4 "GetDrivesCallback" 0 error
	StrCpy $R0 `Function GetDrivesCallback$\r$\n	MessageBox MB_OKCANCEL '$0$$9    "drive letter"=[$$9]$0\$1$0$$8    "drive type" =[$$8]' IDOK +2$\r$\n	StrCpy $$R0 StopGetDrives$\r$\n	StrCpy $$R5 '$$R5$$9  [$$8 Drive]$$\$2$$\$1'$\r$\n$\r$\n	Push $$R0$\r$\nFunctionEnd`
	goto send

	main:
	StrCmp $FUNCTION '' DefaultSend
	StrCmp $FUNCTION Locate 0 +4
	StrCpy $LOCATE1 $R2
	StrCpy $LOCATE2 $R3
	goto DefaultSend
	StrCmp $FUNCTION GetSize1 0 +4
	StrCpy $GETSIZE1 $R2
	StrCpy $GETSIZE2 $R3
	goto DefaultSend
	StrCmp $FUNCTION GetSize2 0 +4
	StrCpy $GETSIZE3 $R2
	StrCpy $GETSIZE4 $R3
	goto DefaultSend
	StrCmp $FUNCTION GetSize3 0 +4
	StrCpy $GETSIZE5 $R2
	StrCpy $GETSIZE6 $R3
	goto DefaultSend
	StrCmp $FUNCTION DriveSpace 0 +4
	StrCpy $DRIVESPACE1 $R1
	StrCpy $DRIVESPACE2 $R3
	goto DefaultSend
	StrCmp $FUNCTION GetDrives 0 +3
	StrCpy $GETDRIVES1 $R1
	goto DefaultSend
	StrCmp $FUNCTION GetTime 0 +4
	StrCpy $GETTIME1 $R1
	StrCpy $GETTIME2 $R3
	goto DefaultSend
	StrCmp $FUNCTION GetFileAttributes 0 +4
	StrCpy $GETFILEATTRIBUTES1 $R1
	StrCpy $GETFILEATTRIBUTES2 $R3
	goto DefaultSend
	StrCmp $FUNCTION GetFileVersion 0 +3
	StrCpy $GETFILEVERSION1 $R1
	goto DefaultSend
	StrCmp $FUNCTION GetOptions 0 +4
	StrCpy $GETOPTIONS1 $R1
	StrCpy $GETOPTIONS2 $R3
	goto DefaultSend
	StrCmp $FUNCTION GetRoot 0 +3
	StrCpy $GETROOT1 $R1
	goto DefaultSend
	StrCmp $FUNCTION GetParent 0 +3
	StrCpy $GETPARENT1 $R1
	goto DefaultSend
	StrCmp $FUNCTION GetFileName 0 +3
	StrCpy $GETFILENAME1 $R1
	goto DefaultSend
	StrCmp $FUNCTION GetBaseName 0 +3
	StrCpy $GETBASENAME1 $R1
	goto DefaultSend
	StrCmp $FUNCTION GetFileExt 0 +3
	StrCpy $GETFILEEXT1 $R1
	goto DefaultSend
	StrCmp $FUNCTION BannerTrimPath 0 +4
	StrCpy $BANNERTRIMPATH1 $R1
	StrCpy $BANNERTRIMPATH2 $R3
	goto DefaultSend
	StrCmp $FUNCTION DirState 0 +2
	StrCpy $DIRSTATE1 $R2

	DefaultSend:
	GetDlgItem $1 $HWND 1201
	EnableWindow $1 1
	ShowWindow $1 0
	SendMessage $1 ${WM_SETTEXT} 1 "STR:"
	GetDlgItem $1 $HWND 1202
	EnableWindow $1 1
	ShowWindow $1 0
	GetDlgItem $1 $HWND 1203
	EnableWindow $1 1
	ShowWindow $1 0
	SendMessage $1 ${WM_SETTEXT} 1 "STR:"
	GetDlgItem $1 $HWND 1204
	EnableWindow $1 1
	ShowWindow $1 0
	GetDlgItem $1 $HWND 1205
	EnableWindow $1 1
	GetDlgItem $1 $HWND 1206
	ShowWindow $1 0
	EnableWindow $1 0
	GetDlgItem $1 $HWND 1207
	ShowWindow $1 0
	GetDlgItem $1 $HWND 1208
	SendMessage $1 ${WM_SETTEXT} 1 "STR:"
	GetDlgItem $1 $HWND 1209
	SendMessage $1 ${WM_SETTEXT} 1 "STR:"
	GetDlgItem $1 $HWND 1210
	SendMessage $1 ${WM_SETTEXT} 1 "STR:"
	GetDlgItem $1 $HWND 1211
	SendMessage $1 ${WM_SETTEXT} 1 "STR:"

	ReadINIStr $0 $INI "Field 1" "State"
	StrCmp $0 "  1. Locate" 0 GetSize1Send
	StrCpy $FUNCTION Locate
	GetDlgItem $1 $HWND 1203
	ShowWindow $1 1
	SendMessage $1 ${WM_SETTEXT} 1 "STR:$LOCATE1"
	GetDlgItem $1 $HWND 1204
	ShowWindow $1 1
	GetDlgItem $1 $HWND 1205
	ShowWindow $1 1
	SendMessage $1 ${WM_SETTEXT} 1 "STR:$LOCATE2"
	GetDlgItem $1 $HWND 1206
	ShowWindow $1 1
	SendMessage $1 ${WM_SETTEXT} 1 "STR:LocateCallback"
	GetDlgItem $1 $HWND 1207
	ShowWindow $1 1
	GetDlgItem $1 $HWND 1209
	SendMessage $1 ${WM_SETTEXT} 1 "STR:Path"
	GetDlgItem $1 $HWND 1210
	SendMessage $1 ${WM_SETTEXT} 1 "STR:Options"
	GetDlgItem $1 $HWND 1211
	SendMessage $1 ${WM_SETTEXT} 1 "STR:Function"
	abort

	GetSize1Send:
	StrCmp $0 "  2. GetSize                 (file)" 0 GetSize2Send
	StrCpy $FUNCTION 'GetSize1'
	GetDlgItem $1 $HWND 1203
	ShowWindow $1 1
	SendMessage $1 ${WM_SETTEXT} 1 "STR:$GETSIZE1"
	GetDlgItem $1 $HWND 1204
	ShowWindow $1 1
	GetDlgItem $1 $HWND 1205
	ShowWindow $1 1
	SendMessage $1 ${WM_SETTEXT} 1 "STR:$GETSIZE2"
	GetDlgItem $1 $HWND 1209
	SendMessage $1 ${WM_SETTEXT} 1 "STR:File"
	GetDlgItem $1 $HWND 1210
	SendMessage $1 ${WM_SETTEXT} 1 "STR:Options"
	Abort

	GetSize2Send:
	StrCmp $0 "                                   (directory)" 0 GetSize3Send
	StrCpy $FUNCTION 'GetSize2'
	GetDlgItem $1 $HWND 1203
	ShowWindow $1 1
	SendMessage $1 ${WM_SETTEXT} 1 "STR:$GETSIZE3"
	GetDlgItem $1 $HWND 1204
	ShowWindow $1 1
	GetDlgItem $1 $HWND 1205
	ShowWindow $1 1
	SendMessage $1 ${WM_SETTEXT} 1 "STR:$GETSIZE4"
	GetDlgItem $1 $HWND 1209
	SendMessage $1 ${WM_SETTEXT} 1 "STR:Directory"
	GetDlgItem $1 $HWND 1210
	SendMessage $1 ${WM_SETTEXT} 1 "STR:Options"
	Abort

	GetSize3Send:
	StrCmp $0 "                                   (no size, no subdir)" 0 DriveSpaceSend
	StrCpy $FUNCTION 'GetSize3'
	GetDlgItem $1 $HWND 1203
	ShowWindow $1 1
	SendMessage $1 ${WM_SETTEXT} 1 "STR:$GETSIZE5"
	GetDlgItem $1 $HWND 1204
	ShowWindow $1 1
	GetDlgItem $1 $HWND 1205
	ShowWindow $1 1
	SendMessage $1 ${WM_SETTEXT} 1 "STR:$GETSIZE6"
	GetDlgItem $1 $HWND 1209
	SendMessage $1 ${WM_SETTEXT} 1 "STR:Directory"
	GetDlgItem $1 $HWND 1210
	SendMessage $1 ${WM_SETTEXT} 1 "STR:Options"
	Abort

	DriveSpaceSend:
	StrCmp $0 "  3. DriveSpace" 0 GetDrivesSend
	StrCpy $FUNCTION DriveSpace
	GetDlgItem $1 $HWND 1201
	ShowWindow $1 1
	SendMessage $1 ${WM_SETTEXT} 1 "STR:$DRIVESPACE1"
	GetDlgItem $1 $HWND 1202
	ShowWindow $1 1
	EnableWindow $1 0
	GetDlgItem $1 $HWND 1205
	ShowWindow $1 1
	SendMessage $1 ${WM_SETTEXT} 1 "STR:$DRIVESPACE2"
	GetDlgItem $1 $HWND 1206
	ShowWindow $1 0
	SendMessage $1 ${WM_SETTEXT} 1 "STR:"
	GetDlgItem $1 $HWND 1207
	ShowWindow $1 0
	GetDlgItem $1 $HWND 1209
	SendMessage $1 ${WM_SETTEXT} 1 "STR:Drive"
	GetDlgItem $1 $HWND 1210
	SendMessage $1 ${WM_SETTEXT} 1 "STR:Options"
	abort

	GetDrivesSend:
	StrCmp $0 "  4. GetDrives             (by type)" 0 GetDrives2Send
	StrCpy $FUNCTION GetDrives
	GetDlgItem $1 $HWND 1201
	ShowWindow $1 1
	SendMessage $1 ${WM_SETTEXT} 1 "STR:$GETDRIVES1"
	GetDlgItem $1 $HWND 1202
	ShowWindow $1 1
	EnableWindow $1 0
	GetDlgItem $1 $HWND 1205
	ShowWindow $1 0
	GetDlgItem $1 $HWND 1206
	ShowWindow $1 1
	SendMessage $1 ${WM_SETTEXT} 1 "STR:GetDrivesCallback"
	GetDlgItem $1 $HWND 1207
	ShowWindow $1 1
	GetDlgItem $1 $HWND 1209
	SendMessage $1 ${WM_SETTEXT} 1 "STR:Option"
	GetDlgItem $1 $HWND 1211
	SendMessage $1 ${WM_SETTEXT} 1 "STR:Function"
	abort

	GetDrives2Send:
	StrCmp $0 "                                   (all by letter)" 0 GetTime1Send
	StrCpy $FUNCTION ''
	GetDlgItem $1 $HWND 1201
	ShowWindow $1 1
	EnableWindow $1 0
	SendMessage $1 ${WM_ENABLE} 1 0
	SendMessage $1 ${WM_SETTEXT} 1 "STR:ALL"
	GetDlgItem $1 $HWND 1202
	ShowWindow $1 1
	EnableWindow $1 0
	GetDlgItem $1 $HWND 1205
	ShowWindow $1 0
	GetDlgItem $1 $HWND 1206
	ShowWindow $1 1
	SendMessage $1 ${WM_SETTEXT} 1 "STR:GetDrivesCallback"
	GetDlgItem $1 $HWND 1207
	ShowWindow $1 1
	GetDlgItem $1 $HWND 1209
	SendMessage $1 ${WM_SETTEXT} 1 "STR:Option"
	GetDlgItem $1 $HWND 1211
	SendMessage $1 ${WM_SETTEXT} 1 "STR:Function"
	abort

	GetTime1Send:
	StrCmp $0 "  5. GetTime                (local time)" 0 GetTime2Send
	StrCpy $FUNCTION ''
	GetDlgItem $1 $HWND 1201
	ShowWindow $1 1
	EnableWindow $1 0
	SendMessage $1 ${WM_ENABLE} 1 0
	GetDlgItem $1 $HWND 1202
	ShowWindow $1 1
	EnableWindow $1 0
	GetDlgItem $1 $HWND 1205
	ShowWindow $1 1
	EnableWindow $1 1
	SendMessage $1 ${WM_SETTEXT} 1 "STR:L"
	GetDlgItem $1 $HWND 1210
	SendMessage $1 ${WM_SETTEXT} 1 "STR:Option"
	Abort

	GetTime2Send:
	StrCmp $0 "                                   (file time)" 0 GetFileAttributesSend
	StrCpy $FUNCTION GetTime
	GetDlgItem $1 $HWND 1201
	ShowWindow $1 1
	SendMessage $1 ${WM_SETTEXT} 1 "STR:$GETTIME1"
	GetDlgItem $1 $HWND 1202
	ShowWindow $1 1
	GetDlgItem $1 $HWND 1205
	ShowWindow $1 1
	SendMessage $1 ${WM_SETTEXT} 1 "STR:$GETTIME2"
	GetDlgItem $1 $HWND 1209
	SendMessage $1 ${WM_SETTEXT} 1 "STR:File"
	GetDlgItem $1 $HWND 1210
	SendMessage $1 ${WM_SETTEXT} 1 "STR:Option"
	Abort

	GetFileAttributesSend:
	StrCmp $0 "  6. GetFileAttributes" 0 GetFileVersionSend
	StrCpy $FUNCTION GetFileAttributes
	GetDlgItem $1 $HWND 1201
	ShowWindow $1 1
	SendMessage $1 ${WM_SETTEXT} 1 "STR:$GETFILEATTRIBUTES1"
	GetDlgItem $1 $HWND 1202
	ShowWindow $1 1
	GetDlgItem $1 $HWND 1205
	ShowWindow $1 1
	SendMessage $1 ${WM_SETTEXT} 1 "STR:$GETFILEATTRIBUTES2"
	GetDlgItem $1 $HWND 1209
	SendMessage $1 ${WM_SETTEXT} 1 "STR:Path"
	GetDlgItem $1 $HWND 1210
	SendMessage $1 ${WM_SETTEXT} 1 "STR:Attrib"
	Abort

	GetFileVersionSend:
	StrCmp $0 "  7. GetFileVersion" 0 GetCmdSend
	StrCpy $FUNCTION GetFileVersion
	GetDlgItem $1 $HWND 1201
	ShowWindow $1 1
	SendMessage $1 ${WM_SETTEXT} 1 "STR:$GETFILEVERSION1"
	GetDlgItem $1 $HWND 1202
	ShowWindow $1 1
	GetDlgItem $1 $HWND 1205
	ShowWindow $1 0
	GetDlgItem $1 $HWND 1209
	SendMessage $1 ${WM_SETTEXT} 1 "STR:File"
	Abort

	GetCmdSend:
	StrCmp $0 "  8. GetExeName" +3
	StrCmp $0 "  9. GetExePath" +2
	StrCmp $0 "10. GetParameters" 0 GetOptionsSend
	StrCpy $FUNCTION ''
	GetDlgItem $1 $HWND 1205
	ShowWindow $1 0
	Abort

	GetOptionsSend:
	StrCmp $0 "11. GetOptions" 0 GetRootSend
	StrCpy $FUNCTION GetOptions
	GetDlgItem $1 $HWND 1201
	ShowWindow $1 1
	SendMessage $1 ${WM_SETTEXT} 1 "STR:$GETOPTIONS1"
	GetDlgItem $1 $HWND 1202
	ShowWindow $1 1
	EnableWindow $1 0
	GetDlgItem $1 $HWND 1205
	ShowWindow $1 1
	SendMessage $1 ${WM_SETTEXT} 1 "STR:$GETOPTIONS2"
	GetDlgItem $1 $HWND 1209
	SendMessage $1 ${WM_SETTEXT} 1 "STR:Parameters"
	GetDlgItem $1 $HWND 1210
	SendMessage $1 ${WM_SETTEXT} 1 "STR:Option"
	Abort

	GetRootSend:
	StrCmp $0 "12. GetRoot" 0 GetParentSend
	StrCpy $FUNCTION GetRoot
	GetDlgItem $1 $HWND 1201
	ShowWindow $1 1
	SendMessage $1 ${WM_SETTEXT} 1 "STR:$GETROOT1"
	GetDlgItem $1 $HWND 1202
	ShowWindow $1 1
	GetDlgItem $1 $HWND 1205
	ShowWindow $1 0
	GetDlgItem $1 $HWND 1209
	SendMessage $1 ${WM_SETTEXT} 1 "STR:FullPath"
	Abort

	GetParentSend:
	StrCmp $0 "13. GetParent" 0 GetFileNameSend
	StrCpy $FUNCTION GetParent
	GetDlgItem $1 $HWND 1201
	ShowWindow $1 1
	SendMessage $1 ${WM_SETTEXT} 1 "STR:$GETPARENT1"
	GetDlgItem $1 $HWND 1202
	ShowWindow $1 1
	GetDlgItem $1 $HWND 1205
	ShowWindow $1 0
	GetDlgItem $1 $HWND 1209
	SendMessage $1 ${WM_SETTEXT} 1 "STR:PathString"
	Abort

	GetFileNameSend:
	StrCmp $0 "14. GetFileName" 0 GetBaseNameSend
	StrCpy $FUNCTION GetFileName
	GetDlgItem $1 $HWND 1201
	ShowWindow $1 1
	SendMessage $1 ${WM_SETTEXT} 1 "STR:$GETFILENAME1"
	GetDlgItem $1 $HWND 1202
	ShowWindow $1 1
	GetDlgItem $1 $HWND 1205
	ShowWindow $1 0
	GetDlgItem $1 $HWND 1209
	SendMessage $1 ${WM_SETTEXT} 1 "STR:PathString"
	Abort

	GetBaseNameSend:
	StrCmp $0 "15. GetBaseName" 0 GetFileExtSend
	StrCpy $FUNCTION GetBaseName
	GetDlgItem $1 $HWND 1201
	ShowWindow $1 1
	SendMessage $1 ${WM_SETTEXT} 1 "STR:$GETBASENAME1"
	GetDlgItem $1 $HWND 1202
	ShowWindow $1 1
	GetDlgItem $1 $HWND 1205
	ShowWindow $1 0
	GetDlgItem $1 $HWND 1209
	SendMessage $1 ${WM_SETTEXT} 1 "STR:FileString"
	Abort

	GetFileExtSend:
	StrCmp $0 "16. GetFileExt" 0 BannerTrimPathSend
	StrCpy $FUNCTION GetFileExt
	GetDlgItem $1 $HWND 1201
	ShowWindow $1 1
	SendMessage $1 ${WM_SETTEXT} 1 "STR:$GETFILEEXT1"
	GetDlgItem $1 $HWND 1202
	ShowWindow $1 1
	GetDlgItem $1 $HWND 1205
	ShowWindow $1 0
	GetDlgItem $1 $HWND 1209
	SendMessage $1 ${WM_SETTEXT} 1 "STR:FileString"
	Abort

	BannerTrimPathSend:
	StrCmp $0 "17. BannerTrimPath" 0 DirStateSend
	StrCpy $FUNCTION BannerTrimPath
	GetDlgItem $1 $HWND 1201
	ShowWindow $1 1
	SendMessage $1 ${WM_SETTEXT} 1 "STR:$BANNERTRIMPATH1"
	GetDlgItem $1 $HWND 1202
	ShowWindow $1 1
	EnableWindow $1 0
	GetDlgItem $1 $HWND 1205
	ShowWindow $1 1
	SendMessage $1 ${WM_SETTEXT} 1 "STR:$BANNERTRIMPATH2"
	GetDlgItem $1 $HWND 1209
	SendMessage $1 ${WM_SETTEXT} 1 "STR:PathString"
	GetDlgItem $1 $HWND 1210
	SendMessage $1 ${WM_SETTEXT} 1 "STR:Option"
	Abort

	DirStateSend:
	StrCmp $0 "18. DirState" 0 RefreshShellIconsSend
	StrCpy $FUNCTION DirState
	GetDlgItem $1 $HWND 1203
	ShowWindow $1 1
	SendMessage $1 ${WM_SETTEXT} 1 "STR:$DIRSTATE1"
	GetDlgItem $1 $HWND 1204
	ShowWindow $1 1
	GetDlgItem $1 $HWND 1205
	ShowWindow $1 0
	GetDlgItem $1 $HWND 1209
	SendMessage $1 ${WM_SETTEXT} 1 "STR:Directory"
	Abort

	RefreshShellIconsSend:
	StrCmp $0 "19. RefreshShellIcons" 0 Abort
	StrCpy $FUNCTION ''
	GetDlgItem $1 $HWND 1205
	ShowWindow $1 0

	Abort:
	Abort

;=Enter=
	Enter:
	StrCpy $R0 ''
	StrCpy $R5 ''

	StrCmp $STATE "  1. Locate" Locate
	StrCmp $STATE "  2. GetSize                 (file)" GetSize
	StrCmp $STATE "                                   (directory)" GetSize
	StrCmp $STATE "                                   (no size, no subdir)" GetSize
	StrCmp $STATE "  3. DriveSpace" DriveSpace
	StrCmp $STATE "  4. GetDrives             (by type)" GetDrives
	StrCmp $STATE "                                   (all by letter)" GetDrives
	StrCmp $STATE "  5. GetTime                (local time)" GetTime
	StrCmp $STATE "                                   (file time)" GetTime
	StrCmp $STATE "  6. GetFileAttributes" GetFileAttributes
	StrCmp $STATE "  7. GetFileVersion" GetFileVersion
	StrCmp $STATE "  8. GetExeName" GetExeName
	StrCmp $STATE "  9. GetExePath" GetExePath
	StrCmp $STATE "10. GetParameters" GetParameters
	StrCmp $STATE "11. GetOptions" GetOptions
	StrCmp $STATE "12. GetRoot" GetRoot
	StrCmp $STATE "13. GetParent" GetParent
	StrCmp $STATE "14. GetFileName" GetFileName
	StrCmp $STATE "15. GetBaseName" GetBaseName
	StrCmp $STATE "16. GetFileExt" GetFileExt
	StrCmp $STATE "17. BannerTrimPath" BannerTrimPath
	StrCmp $STATE "18. DirState" DirState
	StrCmp $STATE "19. RefreshShellIcons" RefreshShellIcons
	Abort

	Locate:
	${Locate} "$R2" "$R3" "LocateCallback"
	IfErrors error
	StrCmp $R0 StopLocate 0 +3
	StrCpy $R0 'stopped'
	goto send
	StrCpy $R0 'done'
	goto send

	GetSize:
	${GetSize} "$R2" "$R3" $0 $1 $2
	IfErrors error
	StrCpy $R0 "Size=$0$\r$\nFiles=$1$\r$\nFolders=$2"
	goto send

	DriveSpace:
	${DriveSpace} "$R1" "$R3" $0
	IfErrors error
	StrCpy $R0 "$0"
	goto send

	GetDrives:
	${GetDrives} "$R1" "GetDrivesCallback"
	StrCmp $R0 StopGetDrives 0 +3
	StrCpy $R0 '$R5stopped'
	goto send
	StrCpy $R0 '$R5done'
	goto send

	GetTime:
	${GetTime} "$R1" "$R3" $0 $1 $2 $3 $4 $5 $6
	IfErrors error
	StrCpy $R0 'Date=$0/$1/$2 ($3)$\r$\nTime=$4:$5:$6'
	goto send

	GetFileAttributes:
	${GetFileAttributes} "$R1" "$R3" $0
	IfErrors error
	StrCpy $R0 '$0'
	goto send

	GetFileVersion:
	${GetFileVersion} "$R1" $0
	IfErrors error
	StrCpy $R0  '$0'
	goto send

	GetExeName:
	${GetExeName} $0
	StrCpy $R0 '$0'
	goto send

	GetExePath:
	${GetExePath} $0
	StrCpy $R0 '$0'
	goto send

	GetParameters:
	${GetParameters} $0
	StrCpy $R0 '$0'
	StrCmp $R0 '' 0 send
	StrCpy $R0 'no parameters'
	goto send

	GetOptions:
	${GetOptions} "$R1" "$R3" $0
	IfErrors error
	StrCpy $R0  '$0'
	goto send

	GetRoot:
	${GetRoot} "$R1" $0
	StrCpy $R0  '$0'
	goto send

	GetParent:
	${GetParent} "$R1" $0
	StrCpy $R0  '$0'
	goto send

	GetFileName:
	${GetFileName} "$R1" $0
	StrCpy $R0  '$0'
	goto send

	GetBaseName:
	${GetBaseName} "$R1" $0
	StrCpy $R0  '$0'
	goto send

	GetFileExt:
	${GetFileExt} "$R1" $0
	StrCpy $R0  '$0'
	goto send

	BannerTrimPath:
	${BannerTrimPath} "$R1" "$R3" $0
	StrCpy $R0  '$0'
	goto send

	DirState:
	${DirState} "$R2" $0
	StrCpy $R0  '$0'
	goto send

	RefreshShellIcons:
	${RefreshShellIcons}
	StrCpy $R0 'done'
	goto send

	error:
	StrCpy $R0 'error'

	send:
	GetDlgItem $1 $HWND 1208
	SendMessage $1 ${WM_SETTEXT} 1 "STR:$R0"

	abort
FunctionEnd

Function LocateCallback
	MessageBox MB_OKCANCEL '$$R9    "path\name"=[$R9]$\n$$R8    "path"          =[$R8]$\n$$R7    "name"        =[$R7]$\n$$R6    "size"           =[$R6]' IDOK +2
	StrCpy $R0 StopLocate

	Push $R0
FunctionEnd

Function GetDrivesCallback
	MessageBox MB_OKCANCEL '$$9    "drive letter"=[$9]$\n$$8    "drive type" =[$8]' IDOK +2
	StrCpy $R0 StopGetDrives
	StrCpy $R5 '$R5$9  [$8 Drive]$\r$\n'

	Push $R0
FunctionEnd

Function .onInit
	InitPluginsDir
	GetTempFileName $INI $PLUGINSDIR
	File /oname=$INI "FileFunc.ini"
FunctionEnd

Page instfiles

Section "Empty"
SectionEnd
