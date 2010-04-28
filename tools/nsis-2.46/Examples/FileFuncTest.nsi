;_____________________________________________________________________________
;
;                          File Functions Test
;_____________________________________________________________________________
;
; 2006 Shengalts Aleksander aka Instructor (Shengalts@mail.ru)

Name "File Functions Test"
OutFile "FileFuncTest.exe"
Caption "$(^Name)"
ShowInstDetails show
XPStyle on
RequestExecutionLevel user

Var FUNCTION
Var OUT1
Var OUT2
Var OUT3
Var OUT4
Var OUT5
Var OUT6
Var OUT7

!include "FileFunc.nsh"
!include "LogicLib.nsh"

;############### INSTALL ###############

!define StackVerificationStart `!insertmacro StackVerificationStart`
!macro StackVerificationStart _FUNCTION
	StrCpy $FUNCTION ${_FUNCTION}
	Call StackVerificationStart
!macroend

!define StackVerificationEnd `!insertmacro StackVerificationEnd`
!macro StackVerificationEnd
	Call StackVerificationEnd
!macroend

Function StackVerificationStart
	StrCpy $0 !0
	StrCpy $1 !1
	StrCpy $2 !2
	StrCpy $3 !3
	StrCpy $4 !4
	StrCpy $5 !5
	StrCpy $6 !6
	StrCpy $7 !7
	StrCpy $8 !8
	StrCpy $9 !9
	StrCpy $R0 !R0
	StrCpy $R1 !R1
	StrCpy $R2 !R2
	StrCpy $R3 !R3
	StrCpy $R4 !R4
	StrCpy $R5 !R5
	StrCpy $R6 !R6
	StrCpy $R7 !R7
	StrCpy $R8 !R8
	StrCpy $R9 !R9
FunctionEnd

Function StackVerificationEnd
	IfErrors +3
	DetailPrint 'PASSED $FUNCTION no errors'
	goto +2
	DetailPrint 'FAILED   $FUNCTION error'

	StrCmp $0 '!0' 0 error
	StrCmp $1 '!1' 0 error
	StrCmp $2 '!2' 0 error
	StrCmp $3 '!3' 0 error
	StrCmp $4 '!4' 0 error
	StrCmp $5 '!5' 0 error
	StrCmp $6 '!6' 0 error
	StrCmp $7 '!7' 0 error
	StrCmp $8 '!8' 0 error
	StrCmp $9 '!9' 0 error
	StrCmp $R0 '!R0' 0 error
	StrCmp $R1 '!R1' 0 error
	StrCmp $R2 '!R2' 0 error
	StrCmp $R3 '!R3' 0 error
	StrCmp $R4 '!R4' 0 error
	StrCmp $R5 '!R5' 0 error
	StrCmp $R6 '!R6' 0 error
	StrCmp $R7 '!R7' 0 error
	StrCmp $R8 '!R8' 0 error
	StrCmp $R9 '!R9' 0 error
	DetailPrint 'PASSED $FUNCTION stack'
	goto end

	error:
	DetailPrint 'FAILED   $FUNCTION stack'
;	MessageBox MB_OKCANCEL '$$0={$0}$\n$$1={$1}$\n$$2={$2}$\n$$3={$3}$\n$$4={$4}$\n$$5={$5}$\n$$6={$6}$\n$$7={$7}$\n$$8={$8}$\n$$9={$9}$\n$$R0={$R0}$\n$$R1={$R1}$\n$$R2={$R2}$\n$$R3={$R3}$\n$$R4={$R4}$\n$$R5={$R5}$\n$$R6={$R6}$\n$$R7={$R7}$\n$$R8={$R8}$\n$$R9={$R9}' IDOK +2
;	quit

	end:
FunctionEnd



Section Locate
	${StackVerificationStart} Locate

	${Locate} '$DOCUMENTS' '/L=FD /M=*.* /S=0B /G=0' 'LocateCallback'

	${StackVerificationEnd}
SectionEnd

Function LocateCallback
;	MessageBox MB_YESNO '$$0={$0}$\n$$1={$1}$\n$$2={$2}$\n$$3={$3}$\n$$4={$4}$\n$$5={$5}$\n$$6={$6}$\n$$7={$7}$\n$$8={$8}$\n$$9={$9}$\n$$R0={$R0}$\n$$R1={$R1}$\n$$R2={$R2}$\n$$R3={$R3}$\n$$R4={$R4}$\n$$R5={$R5}$\n$$R6={$R6}$\n$$R7={$R7}$\n$$R8={$R8}$\n$$R9={$R9}$\n$\nContinue?' IDYES +2
;	StrCpy $0 StopLocate

	Push $0
FunctionEnd


Section GetSize
	${StackVerificationStart} GetSize

	${GetSize} '$WINDIR' '/M=Explorer.exe /S=0K /G=0' $OUT1 $OUT2 $OUT3

	${StackVerificationEnd}
SectionEnd


Section DriveSpace
	${StackVerificationStart} DriveSpace

	${DriveSpace} 'C:\' '/D=F /S=M' $OUT1

	${StackVerificationEnd}
SectionEnd


Section GetDrives
	${StackVerificationStart} GetDrives

	${GetDrives} 'FDD+CDROM' 'GetDrivesCallback'

	${StackVerificationEnd}
SectionEnd

Function GetDrivesCallback
;	MessageBox MB_YESNO '$$0={$0}$\n$$1={$1}$\n$$2={$2}$\n$$3={$3}$\n$$4={$4}$\n$$5={$5}$\n$$6={$6}$\n$$7={$7}$\n$$8={$8}$\n$$9={$9}$\n$$R0={$R0}$\n$$R1={$R1}$\n$$R2={$R2}$\n$$R3={$R3}$\n$$R4={$R4}$\n$$R5={$R5}$\n$$R6={$R6}$\n$$R7={$R7}$\n$$R8={$R8}$\n$$R9={$R9}$\n$\nContinue?' IDYES +2
;	StrCpy $0 StopGetDrives

	Push $0
FunctionEnd


Section GetTime
	${StackVerificationStart} GetTime

	${GetTime} '' 'L' $OUT1 $OUT2 $OUT3 $OUT4 $OUT5 $OUT6 $OUT7

	${StackVerificationEnd}
SectionEnd


Section GetFileAttributes
	${StackVerificationStart} GetFileAttributes

	${GetFileAttributes} '$WINDIR\explorer.exe' 'ALL' $OUT1

	${StackVerificationEnd}
SectionEnd


Section GetFileVersion
	${StackVerificationStart} GetFileVersion

	${GetFileVersion} '$WINDIR\explorer.exe' $OUT1

	${StackVerificationEnd}
SectionEnd


Section GetExeName
	${StackVerificationStart} GetExeName

	${GetExeName} $OUT1

	${StackVerificationEnd}
SectionEnd


Section GetExePath
	${StackVerificationStart} GetExePath

	${GetExePath} $OUT1

	${StackVerificationEnd}
SectionEnd


Section GetParameters
	${StackVerificationStart} GetParameters

	# basic stuff

	StrCpy $CMDLINE '"$PROGRAMFILES\Something\Hello.exe"'
	${GetParameters} $OUT1
	StrCpy $CMDLINE '"$PROGRAMFILES\Something\Hello.exe" test'
	${GetParameters} $OUT2
	StrCpy $CMDLINE '"$PROGRAMFILES\Something\Hello.exe" "test"'
	${GetParameters} $OUT3
	StrCpy $CMDLINE 'C:\Hello.exe'
	${GetParameters} $OUT4
	StrCpy $CMDLINE 'C:\Hello.exe test'
	${GetParameters} $OUT5
	StrCpy $CMDLINE 'C:\Hello.exe "test"'
	${GetParameters} $OUT6
	StrCpy $CMDLINE 'C:\Hello.exe       test test  '
	${GetParameters} $OUT7

	${If} $OUT1 != ""
	${OrIf} $OUT2 != "test"
	${OrIf} $OUT3 != '"test"'
	${OrIf} $OUT4 != ""
	${OrIf} $OUT5 != "test"
	${OrIf} $OUT6 != '"test"'
	${OrIf} $OUT7 != 'test test'
		SetErrors
	${EndIf}

	# some corner cases

	StrCpy $CMDLINE ''
	${GetParameters} $OUT1
	StrCpy $CMDLINE '"'
	${GetParameters} $OUT2
	StrCpy $CMDLINE '""'
	${GetParameters} $OUT3
	StrCpy $CMDLINE '"" test'
	${GetParameters} $OUT4
	StrCpy $CMDLINE ' test'
	${GetParameters} $OUT5
	StrCpy $CMDLINE '  test' # left over bug(?) from old GetParameters
	                         # it starts looking for ' ' from the third char
	${GetParameters} $OUT6
	StrCpy $CMDLINE ' '
	${GetParameters} $OUT7

	${If} $OUT1 != ""
	${OrIf} $OUT2 != ""
	${OrIf} $OUT3 != ""
	${OrIf} $OUT4 != ""
	${OrIf} $OUT5 != ""
	${OrIf} $OUT6 != ""
	${OrIf} $OUT7 != ""
		SetErrors
	${EndIf}

	${StackVerificationEnd}
SectionEnd


Section GetOptions
	${StackVerificationStart} GetOptions

	${GetOptions} '/INSTDIR=C:\Program Files\Common Files /SILENT=yes' '/INSTDIR=' $OUT1
	StrCmp $OUT1 'C:\Program Files\Common Files' 0 error

	${GetOptions} '-TMP=temp.tmp -INSTDIR="C:/Program Files/Common Files" -SILENT=yes' '-INSTDIR=' $OUT1
	StrCmp $OUT1 'C:/Program Files/Common Files' 0 error

	${GetOptions} "/INSTDIR='C:/Program Files/Common Files' /SILENT=yes" '/INSTDIR=' $OUT1
	StrCmp $OUT1 'C:/Program Files/Common Files' 0 error

	StrCpy $OUT1 '/INSTDIR=`C:/Program Files/Common Files` /SILENT=yes'
	${GetOptions} '$OUT1' '/INSTDIR=' $OUT1
	StrCmp $OUT1 'C:/Program Files/Common Files' 0 error

	${GetOptions} '/SILENT=yes /INSTDIR=C:\Program Files\Common Files' '/INSTDIR=' $OUT1
	StrCmp $OUT1 'C:\Program Files\Common Files' 0 error

	${GetOptions} "/INSTDIR=common directory: 'C:\Program Files\Common Files' /SILENT=yes" '/INSTDIR=' $OUT1
	StrCmp $OUT1 "common directory: 'C:\Program Files\Common Files'" 0 error

	${GetOptions} '/INSTDIR=WxxxW /SILENT=yes' '/INSTDIR=' $OUT1
	StrCmp $OUT1 'WxxxW' 0 error

	${GetOptions} "/Prm='/D=True' /D=1" '/D=' $OUT1
	StrCmp $OUT1 "1" 0 error

	${GetOptions} "/D=1 /Prm='/D=True'" '/Prm=' $OUT1
	StrCmp $OUT1 "/D=True" 0 error

	${GetOptions} `/D=1 /Prm='/D="True" /S="/Temp"'` '/Prm=' $OUT1
	StrCmp $OUT1 '/D="True" /S="/Temp"' 0 error

	${GetOptions} `/INSTDIR='"C:/Program Files/Common Files"' /SILENT=yes` '/INSTDIR=' $OUT1
	StrCmp $OUT1 '"C:/Program Files/Common Files"' 0 error

	${GetOptions} `/INSTDIR='"C:/Program Files/Common Files"' /SILENT=yes` '/INSTDIR*=' $OUT1
	IfErrors 0 error
	StrCmp $OUT1 '' 0 error

	${GetOptions} `/INSTDIR="C:/Program Files/Common Files" /SILENT=yes` '' $OUT1
	IfErrors 0 error
	StrCmp $OUT1 '' 0 error

	${GetOptionsS} '/INSTDIR=C:\Program Files\Common Files /SILENT' '/SILENT' $OUT1
	IfErrors error
	StrCmp $OUT1 '' 0 error

	goto +2
	error:
	SetErrors

	${StackVerificationEnd}
SectionEnd


Section GetOptionsS
	${StackVerificationStart} GetOptionsS

	${GetOptionsS} '/INSTDIR=C:\Program Files\Common Files /SILENT=yes' '/INSTDIR=' $OUT1
	IfErrors error
	StrCmp $OUT1 'C:\Program Files\Common Files' 0 error

	${GetOptionsS} '/INSTDIR=C:\Program Files\Common Files /SILENT=yes' '/Instdir=' $OUT1
	IfErrors 0 error
	StrCmp $OUT1 '' 0 error

	${GetOptionsS} '/INSTDIR=C:\Program Files\Common Files /SILENT' '/SILENT' $OUT1
	IfErrors error
	StrCmp $OUT1 '' 0 error

	goto +2
	error:
	SetErrors

	${StackVerificationEnd}
SectionEnd


Section GetRoot
	${StackVerificationStart} GetRoot

	${GetRoot} 'C:\Program Files\NSIS' $OUT1
	StrCmp $OUT1 'C:' 0 error

	${GetRoot} '\\SuperPimp\NSIS\Source\exehead\Ui.c' $OUT1
	StrCmp $OUT1 '\\SuperPimp\NSIS' 0 error

	${GetRoot} '\\Program Files\NSIS' $OUT1
	StrCmp $OUT1 '\\Program Files\NSIS' 0 error

	${GetRoot} '\\Program Files\NSIS\' $OUT1
	StrCmp $OUT1 '\\Program Files\NSIS' 0 error

	${GetRoot} '\\Program Files\NSIS\Source\exehead\Ui.c' $OUT1
	StrCmp $OUT1 '\\Program Files\NSIS' 0 error

	${GetRoot} '\Program Files\NSIS' $OUT1
	StrCmp $OUT1 '' 0 error

	goto +2
	error:
	SetErrors

	${StackVerificationEnd}
SectionEnd


Section GetParent
	${StackVerificationStart} GetParent

	${GetParent} 'C:\Program Files\Winamp\uninstwa.exe' $OUT1
	StrCmp $OUT1 'C:\Program Files\Winamp' 0 error

	${GetParent} 'C:\Program Files\Winamp\plugins' $OUT1
	StrCmp $OUT1 'C:\Program Files\Winamp' 0 error

	${GetParent} 'C:\Program Files\Winamp\plugins\' $OUT1
	StrCmp $OUT1 'C:\Program Files\Winamp' 0 error

	${GetParent} 'C:\' $OUT1
	StrCmp $OUT1 '' 0 error

	goto +2
	error:
	SetErrors

	${StackVerificationEnd}
SectionEnd


Section GetFileName
	${StackVerificationStart} GetFileName

	${GetFileName} 'C:\Program Files\Winamp\uninstwa.exe' $OUT1
	StrCmp $OUT1 'uninstwa.exe' 0 error

	${GetFileName} 'uninstwa.exe' $OUT1
	StrCmp $OUT1 'uninstwa.exe' 0 error

	${GetFileName} 'C:\Program Files\Winamp\plugins' $OUT1
	StrCmp $OUT1 'plugins' 0 error

	${GetFileName} 'C:\Program Files\Winamp\plugins\' $OUT1
	StrCmp $OUT1 'plugins' 0 error

	goto +2
	error:
	SetErrors

	${StackVerificationEnd}
SectionEnd


Section GetBaseName
	${StackVerificationStart} GetBaseName

	${GetBaseName} 'C:\Program Files\Winamp\uninstwa.exe' $OUT1
	StrCmp $OUT1 'uninstwa' 0 error

	${GetBaseName} 'uninstwa.exe' $OUT1
	StrCmp $OUT1 'uninstwa' 0 error

	${GetBaseName} 'C:\Program Files\Winamp\plugins' $OUT1
	StrCmp $OUT1 'plugins' 0 error

	${GetBaseName} 'C:\Program Files\Winamp\plugins\' $OUT1
	StrCmp $OUT1 '' 0 error

	goto +2
	error:
	SetErrors

	${StackVerificationEnd}
SectionEnd


Section GetFileExt
	${StackVerificationStart} GetFileExt

	${GetFileExt} 'C:\Program Files\Winamp\uninstwa.exe' $OUT1
	StrCmp $OUT1 'exe' 0 error

	${GetFileExt} 'uninstwa.exe' $OUT1
	StrCmp $OUT1 'exe' 0 error

	${GetFileExt} 'C:\Program Files\Winamp\plugins' $OUT1
	StrCmp $OUT1 '' 0 error

	${GetFileExt} 'C:\Program Files\Winamp\plugins\' $OUT1
	StrCmp $OUT1 '' 0 error

	goto +2
	error:
	SetErrors

	${StackVerificationEnd}
SectionEnd


Section BannerTrimPath
	${StackVerificationStart} BannerTrimPath

	${BannerTrimPath} 'C:\Server\Documents\Terminal\license.htm' '35A' $OUT1
	StrCmp $OUT1 'C:\Server\...\Terminal\license.htm' 0 error

	${BannerTrimPath} 'C:\12\3456\789' '3A' $OUT1
	StrCmp $OUT1 '' 0 error

	${BannerTrimPath} 'C:\12\3456\789' '4A' $OUT1
	StrCmp $OUT1 'C...' 0 error

	${BannerTrimPath} 'C:\12\3456\789' '11A' $OUT1
	StrCmp $OUT1 'C:\12\...' 0 error

	${BannerTrimPath} 'C:\12\3456\789' '13A' $OUT1
	StrCmp $OUT1 'C:\12\...\789' 0 error

	${BannerTrimPath} 'C:\12\3456\789' '14A' $OUT1
	StrCmp $OUT1 'C:\12\3456\789' 0 error

	${BannerTrimPath} 'C:\12\3456\789' '14A' $OUT1
	StrCmp $OUT1 'C:\12\3456\789' 0 error

	${BannerTrimPath} 'C:\12\3456\789' '11B' $OUT1
	StrCmp $OUT1 'C:\12\...' 0 error

	${BannerTrimPath} 'C:\12\3456\789' '5B' $OUT1
	StrCmp $OUT1 'C:...' 0 error

	${BannerTrimPath} 'C:\12\3456\789' '5B' $OUT1
	StrCmp $OUT1 'C:...' 0 error

	${BannerTrimPath} 'C:\12\3456\789' '11C' $OUT1
	StrCmp $OUT1 'C:\12\34...' 0 error

	${BannerTrimPath} 'C:\12\3456\789' '9D' $OUT1
	StrCmp $OUT1 'C:\12\...' 0 error

	${BannerTrimPath} 'C:\12\3456\789' '10D' $OUT1
	StrCmp $OUT1 'C:\...\789' 0 error

	${BannerTrimPath} 'C:\12\3456\789' '11D' $OUT1
	StrCmp $OUT1 'C:\1...\789' 0 error

	${BannerTrimPath} '123456789' '5D' $OUT1
	StrCmp $OUT1 '12...' 0 error

	goto +2
	error:
	SetErrors

	${StackVerificationEnd}
SectionEnd


Section DirState
	${StackVerificationStart} DirState

	${DirState} '$TEMP' $OUT1

	${StackVerificationEnd}
SectionEnd


Section RefreshShellIcons
	${StackVerificationStart} RefreshShellIcons

	${RefreshShellIcons}

	${StackVerificationEnd}
SectionEnd


Section WriteUninstaller
	goto +2
	WriteUninstaller '$EXEDIR\un.FileFuncTest.exe'
SectionEnd



;############### UNINSTALL ###############

Section un.Uninstall
	${Locate} '$DOCUMENTS' '/L=FD /M=*.* /S=0B /G=0' 'un.LocateCallback'
	${GetSize} '$WINDIR' '/M=Explorer.exe /S=0K /G=0' $OUT1 $OUT2 $OUT3
	${DriveSpace} 'C:\' '/D=F /S=M' $OUT1
	${GetDrives} 'FDD+CDROM' 'un.GetDrivesCallback'
	${GetTime} '' 'L' $OUT1 $OUT2 $OUT3 $OUT4 $OUT5 $OUT6 $OUT7
	${GetFileAttributes} '$WINDIR\explorer.exe' 'ALL' $OUT1
	${GetFileVersion} '$WINDIR\explorer.exe' $OUT1
	${GetExeName} $OUT1
	${GetExePath} $OUT1
	${GetParameters} $OUT1
	${GetOptions} '/INSTDIR=C:\Program Files\Common Files /SILENT=yes' '/INSTDIR=' $OUT1
	${GetOptionsS} '/INSTDIR=C:\Program Files\Common Files /SILENT=yes' '/INSTDIR=' $OUT1
	${GetRoot} 'C:\Program Files\NSIS' $OUT1
	${GetParent} 'C:\Program Files\Winamp\uninstwa.exe' $OUT1
	${GetFileName} 'C:\Program Files\Winamp\uninstwa.exe' $OUT1
	${GetBaseName} 'C:\Program Files\Winamp\uninstwa.exe' $OUT1
	${GetFileExt} 'C:\Program Files\Winamp\uninstwa.exe' $OUT1
	${BannerTrimPath} 'C:\Server\Documents\Terminal\license.htm' '35A' $OUT1
	${DirState} '$TEMP' $OUT1
	${RefreshShellIcons}
SectionEnd

Function un.LocateCallback
	Push $0
FunctionEnd

Function un.GetDrivesCallback
	Push $0
FunctionEnd
