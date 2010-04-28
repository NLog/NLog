;_____________________________________________________________________________
;
;                          Text Functions
;_____________________________________________________________________________
;
; 2006 Shengalts Aleksander aka Instructor (Shengalts@mail.ru)

Name "Text Functions"
OutFile "TextFunc.exe"
Caption "$(^Name)"
XPStyle on
RequestExecutionLevel user

!include "WinMessages.nsh"
!include "TextFunc.nsh"

Var HWND
Var INI
Var LOG
Var PROJECT
Var CALLBACK
Var VIEW
Var FUNCTION
Var LINEFIND1
Var LINEFIND2
Var LINEFIND3
Var LINEREAD1
Var LINEREAD2
Var FILEREADFROMEND1
Var LINESUM1
Var FILEJOIN1
Var FILEJOIN2
Var FILEJOIN3
Var TEXTCOMPARE1
Var TEXTCOMPARE2
Var TEXTCOMPARE3
Var CONFIGREAD1
Var CONFIGREAD2
Var CONFIGWRITE1
Var CONFIGWRITE2
Var CONFIGWRITE3
Var FILERECODE1
Var FILERECODE2

Page Custom ShowCustom LeaveCustom

Function ShowCustom
	InstallOptions::initDialog "$INI"
	Pop $hwnd
	GetDlgItem $0 $HWND 1206
	ShowWindow $0 0
	GetDlgItem $0 $HWND 1208
	ShowWindow $0 0
	GetDlgItem $0 $HWND 1209
	ShowWindow $0 0
	StrCpy $FUNCTION LineFind
	StrCpy $LINEREAD2 10
	StrCpy $TEXTCOMPARE3 FastDiff
	StrCpy $CONFIGREAD1 "$WINDIR\system.ini"
	StrCpy $CONFIGREAD2 "shell="
	StrCpy $FILERECODE2 CharToOem
	InstallOptions::show
	Pop $0
FunctionEnd

Function LeaveCustom
	ReadINIStr $0 $INI "Settings" "State"
 	ReadINIStr $R0 $INI "Field 1" "State" 	
	ReadINIStr $R1 $INI "Field 2" "State"
 	ReadINIStr $R2 $INI "Field 3" "State"
 	ReadINIStr $R3 $INI "Field 4" "State"
 	ReadINIStr $R4 $INI "Field 5" "State"
 	ReadINIStr $R5 $INI "Field 6" "State"
	StrCpy $R4 $R4 8
	StrCpy $R5 $R5 8
	StrCpy $6 0
	StrCpy $7 '$${'
	StrCpy $8 'r'
	StrCpy $9 'n'

	StrCmp $0 10 Log
	StrCmp $0 9 ViewOrEdit
	StrCmp $0 0 Enter
	goto MainSend

	Log:
	Exec 'notepad.exe $LOG'
	Abort

	ViewOrEdit:
	StrCmp $FUNCTION FileReadFromEnd 0 Edit
	StrCmp $VIEW '' 0 ViewFileReadFromEndCallback
	GetTempFileName $VIEW $PLUGINSDIR
	StrCpy $7 '$$'
	FileOpen $0 $VIEW w
	FileWrite $0 `Function FileReadFromEndCallback$\r$\n`
	FileWrite $0 `	MessageBox MB_OKCANCEL '$7$$9       "Line"=[$$9]$7\$9$7$$8           "#"=[$$8]$7\$9$7$$7          "-#"=[$$7]' IDOK +2$\r$\n`
	FileWrite $0 `	StrCpy $$R0 StopFileReadFromEnd$\r$\n$\r$\n`
	FileWrite $0 `	Push $$R0$\r$\n`
	FileWrite $0 `FunctionEnd$\r$\n`
	FileClose $0
	StrCpy $7 '$${'
	SetFileAttributes $VIEW READONLY
	ViewFileReadFromEndCallback:
	Exec 'notepad.exe $VIEW'
	Abort

	Edit:
	StrCmp $CALLBACK '' +5
	StrCmp $6$R6 '0$R0$R4$R5' showproject
	StrCmp $R6 '$R0$R4$R5' +3
	Delete $CALLBACK
	StrCpy $CALLBACK ''
	StrCpy $R6 '$R0$R4$R5'

	#Project#
	StrCmp $6$R0 "01. LineFind" 0 +5
	IfFileExists $CALLBACK +2
	GetTempFileName $CALLBACK $PLUGINSDIR
	FileOpen $0 $CALLBACK w
	goto function
	IfFileExists $PROJECT +2
	GetTempFileName $PROJECT $PLUGINSDIR
	FileOpen $0 $PROJECT w

	#Name#
	FileWrite $0 'Name "$FUNCTION"$\r$\n'
	FileWrite $0 'OutFile "$PROJECT.exe"$\r$\n$\r$\n'

	#!include#
	StrCmp $R0$R4 '1. LineFindExample5' 0 TextFuncInclude
	IfFileExists '$EXEDIR\WordFunc.nsh' 0 +3
	FileWrite $0 '!include "$EXEDIR\WordFunc.nsh"$\r$\n'
	goto +2
	FileWrite $0 '!include "WordFunc.nsh"$\r$\n'
	FileWrite $0 '!insertmacro WordFind$\r$\n'
	FileWrite $0 '!insertmacro WordFindS$\r$\n'
	FileWrite $0 '!insertmacro WordFind2X$\r$\n'
	FileWrite $0 '!insertmacro WordFind2XS$\r$\n'
	FileWrite $0 '!insertmacro WordFind3X$\r$\n'
	FileWrite $0 '!insertmacro WordFind3XS$\r$\n'
	FileWrite $0 '!insertmacro WordReplace$\r$\n'
	FileWrite $0 '!insertmacro WordReplaceS$\r$\n'
	FileWrite $0 '!insertmacro WordAdd$\r$\n'
	FileWrite $0 '!insertmacro WordAddS$\r$\n'
	FileWrite $0 '!insertmacro WordInsert$\r$\n'
	FileWrite $0 '!insertmacro WordInsertS$\r$\n'
	FileWrite $0 '!insertmacro StrFilter$\r$\n'
	FileWrite $0 '!insertmacro StrFilterS$\r$\n'
	TextFuncInclude:
	IfFileExists '$EXEDIR\TextFunc.nsh' 0 +3
	FileWrite $0 '!include "$EXEDIR\TextFunc.nsh"$\r$\n'
	goto +2
	FileWrite $0 '!include "TextFunc.nsh"$\r$\n'
	FileWrite $0 '!insertmacro $FUNCTION$\r$\n'
	StrCmp $FUNCTION TextCompare +2
	FileWrite $0 '!insertmacro TrimNewLines$\r$\n'

	#Section#
	FileWrite $0 '$\r$\nSection -empty$\r$\n'
	FileWrite $0 'SectionEnd$\r$\n$\r$\n'

	#Function .onInit#
	FileWrite $0 'Function .onInit$\r$\n'
	StrCmp $R0$R5 "6. TextCompareExample1" 0 TextCompareExample235
	FileWrite $0 '	StrCpy $$R0 ""$\r$\n'
	FileWrite $0 '	$7TextCompare} "$R1" "$R2" "$R3" "$R5"$\r$\n'
	FileWrite $0 '	IfErrors error$\r$\n'
	FileWrite $0 '	StrCmp $$R0 NotEqual 0 +2$\r$\n'
	FileWrite $0 '	MessageBox MB_OK "             Files differ" IDOK +2$\r$\n'
	FileWrite $0 '	MessageBox MB_OK "           Files identical"$\r$\n'
	FileWrite $0 '	goto end$\r$\n$\r$\n'
	goto endoninit
	TextCompareExample235:
	StrCmp $R0$R5 "6. TextCompareExample2" +3
	StrCmp $R0$R5 "6. TextCompareExample3" +2
	StrCmp $R0$R5 "6. TextCompareExample5" 0 TextCompareExample4
	FileWrite $0 '	StrCpy $$R0 "$R1"$\r$\n'
	FileWrite $0 '	StrCpy $$R1 "$R2"$\r$\n$\r$\n'
	FileWrite $0 '	GetTempFileName $$R2$\r$\n'
	FileWrite $0 '	FileOpen $$R3 $$R2 w$\r$\n'
	FileWrite $0 '	FileWrite $$R3 "$$R0 | $$R1$$\$8$$\$9"$\r$\n'
	FileWrite $0 '	$7TextCompare} "$$R0" "$$R1" "$R3" "$R5"$\r$\n'
	FileWrite $0 '	IfErrors error$\r$\n'
	FileWrite $0 '	Exec "notepad.exe $$R2"$\r$\n'
	FileWrite $0 '	goto end$\r$\n$\r$\n'
	goto endoninit
	TextCompareExample4:
	StrCmp $R0$R5 "6. TextCompareExample4" 0 LineFindExample123456
	FileWrite $0 '	StrCpy $$R0 "$R1"$\r$\n'
	FileWrite $0 '	StrCpy $$R1 "$R2"$\r$\n$\r$\n'
	FileWrite $0 '	GetTempFileName $$R2$\r$\n'
	FileWrite $0 '	FileOpen $$R3 $$R2 w$\r$\n'
	FileWrite $0 '	FileWrite $$R3 "$$R0 | $$R1$$\$8$$\$9"$\r$\n'
	FileWrite $0 '	$7TextCompare} "$$R0" "$$R1" "$R3" "$R5"$\r$\n'
	FileWrite $0 '	IfErrors error$\r$\n'
	FileWrite $0 '	FileWrite $$R3 "$$\$8$$\$9$$R1 | $$R0$$\$8$$\$9"$\r$\n'
	FileWrite $0 '	$7TextCompare} "$$R1" "$$R0" "$R3" "$R5"$\r$\n'
	FileWrite $0 '	FileClose $$R3$\r$\n'
	FileWrite $0 '	IfErrors error$\r$\n'
	FileWrite $0 '	Exec "notepad.exe $$R2"$\r$\n$\r$\n'
	FileWrite $0 '	goto end$\r$\n$\r$\n'
	goto endoninit
	LineFindExample123456:
	FileWrite $0 '	$7$FUNCTION} "$R1" "$R2" "$R3" "$R4"$\r$\n'
	FileWrite $0 '	IfErrors error$\r$\n'
	FileWrite $0 '	MessageBox MB_YESNO "          Open output file?" IDNO end$\r$\n'
	FileWrite $0 '	StrCmp "$R2" "" 0 +3$\r$\n'
	FileWrite $0 `	Exec 'notepad.exe "$R1"'$\r$\n`
	FileWrite $0 '	goto end$\r$\n'
	FileWrite $0 '	SearchPath $$R2 "$R2"$\r$\n'
	FileWrite $0 `	Exec 'notepad.exe "$$R2"'$\r$\n`
	FileWrite $0 '	goto end$\r$\n$\r$\n'
	endoninit:
	FileWrite $0 '	error:$\r$\n'
	FileWrite $0 '	MessageBox MB_OK "Error"$\r$\n$\r$\n'
	FileWrite $0 '	end:$\r$\n'
	FileWrite $0 '	Quit$\r$\n'
	FileWrite $0 'FunctionEnd$\r$\n$\r$\n'
	#FunctionEnd#


	#Function CallBack#
	StrCmp $CALLBACK '' 0 close
	function:
	StrCmp $R0 '1. LineFind' 0 +8
	FileWrite $0 'Function $R4$\r$\n'
	StrCmp $R4 "Example1" Example1LF
	StrCmp $R4 "Example2" Example2LF
	StrCmp $R4 "Example3" Example3LF
	StrCmp $R4 "Example4" Example4LF
	StrCmp $R4 "Example5" Example5LF
	StrCmp $R4 "Example6" Example6LF

	FileWrite $0 'Function $R5$\r$\n'
	StrCmp $R5 "Example1" Example1TC
	StrCmp $R5 "Example2" Example2TC
	StrCmp $R5 "Example3" Example3TC
	StrCmp $R5 "Example4" Example4TC
	StrCmp $R5 "Example5" Example3TC

	Example1LF:
	FileWrite $0 "	$7TrimNewLines} '$$R9' $$R9$\r$\n"
	FileWrite $0 "	StrCpy $$R9 $$R9 '' 2       ;delete first two symbols$\r$\n"
	FileWrite $0 "	StrCpy $$R9 '$$R9$$\$8$$\$9'$\r$\n$\r$\n"
	goto endwrite
	Example2LF:
	FileWrite $0 "	$7TrimNewLines} '$$R9' $$R9$\r$\n"
	FileWrite $0 "	StrCpy $$R9 '$$R9   ~Changed line ($$R8)~$$\$8$$\$9'$\r$\n$\r$\n"
	goto endwrite
	Example3LF:
	FileWrite $0 "	StrCpy $$0 SkipWrite$\r$\n$\r$\n"
	goto endwrite
	Example4LF:
	FileWrite $0 "	FileWrite $$R4 '---First Line---$$\$8$$\$9'$\r$\n"
	FileWrite $0 "	FileWrite $$R4 '---Second Line ...---$$\$8$$\$9'$\r$\n$\r$\n"
	goto endwrite
	Example5LF:
	FileWrite $0 "	; You can use:$\r$\n"
	FileWrite $0 "	; $7WordFind}|$7WordFindS}|$7WordFind2X}|$7WordFind2XS}|$\r$\n"
	FileWrite $0 "	; $7WordFind3X}|$7WordFind3XS}|$7WordReplace}|$7WordReplaceS}|$\r$\n"
	FileWrite $0 "	; $7WordAdd}|$7WordAddS}|$7WordInsert}|$7WordInsertS}|$\r$\n"
	FileWrite $0 "	; $7StrFilter}|$7StrFilterS}$\r$\n$\r$\n"
	FileWrite $0 "	$7WordReplace} '$$R9' ' ' '_' '+*' $$R9$\r$\n$\r$\n"
	goto endwrite
	Example6LF:
	FileWrite $0 '	;(Cut lines from a line to another line (also including that line))$\r$\n'
	FileWrite $0 '	StrCmp $$R0 finish stop$\r$\n'
	FileWrite $0 '	StrCmp $$R0 start finish$\r$\n'
	FileWrite $0 '	StrCmp $$R9 "Start Line$$\$8$$\$9" 0 skip$\r$\n'
	FileWrite $0 '	StrCpy $$R0 start$\r$\n'
	FileWrite $0 '	StrCpy $$R1 $$R9$\r$\n'
	FileWrite $0 '	goto code$\r$\n'
	FileWrite $0 '	finish:$\r$\n'
	FileWrite $0 '	StrCmp $$R9 "Finish Line$$\$8$$\$9" 0 code$\r$\n'
	FileWrite $0 '	StrCpy $$R0 finish$\r$\n'
	FileWrite $0 '	StrCpy $$R2 $$R8$\r$\n'
	FileWrite $0 '	goto code$\r$\n'
	FileWrite $0 '	skip:$\r$\n'
	FileWrite $0 '	StrCpy $$0 SkipWrite$\r$\n'
	FileWrite $0 '	goto output$\r$\n'
	FileWrite $0 '	stop:$\r$\n'
	FileWrite $0 '	StrCpy $$0 StopLineFind$\r$\n'
	FileWrite $0 '	goto output$\r$\n$\r$\n'
	FileWrite $0 '	;;(Delete lines from a line to another line (also including that line))$\r$\n'
	FileWrite $0 '	; StrCmp $$R0 finish code$\r$\n'
	FileWrite $0 '	; StrCmp $$R0 start finish$\r$\n'
	FileWrite $0 '	; StrCmp $$R9 "Start Line$$\$8$$\$9" 0 code$\r$\n'
	FileWrite $0 '	; StrCpy $$R0 start$\r$\n'
	FileWrite $0 '	; StrCpy $$R1 $$R8$\r$\n'
	FileWrite $0 '	; goto skip$\r$\n'
	FileWrite $0 '	; finish:$\r$\n'
	FileWrite $0 '	; StrCmp $$R9 "Finish Line$$\$8$$\$9" 0 skip$\r$\n'
	FileWrite $0 '	; StrCpy $$R0 finish$\r$\n'
	FileWrite $0 '	; StrCpy $$R2 $$R8$\r$\n'
	FileWrite $0 '	; skip:$\r$\n'
	FileWrite $0 '	; StrCpy $$0 SkipWrite$\r$\n'
	FileWrite $0 '	; goto output$\r$\n$\r$\n'
	FileWrite $0 '	code:$\r$\n'
	FileWrite $0 '	;...$\r$\n$\r$\n'
	FileWrite $0 '	output:$\r$\n'
	goto endwrite
	Example1TC:
	FileWrite $0 "	StrCpy $$R0 NotEqual$\r$\n"
	FileWrite $0 "	StrCpy $$0 StopTextCompare$\r$\n$\r$\n"
	goto endwrite
	Example2TC:
	FileWrite $0 "	FileWrite $$R3 '$$8=$$9'$\r$\n"
	FileWrite $0 "	FileWrite $$R3 '$$6=$$7$$\$8$$\$9'$\r$\n$\r$\n"
	goto endwrite
	Example3TC:
	FileWrite $0 "	FileWrite $$R3 '$$8|$$6=$$9'$\r$\n$\r$\n"
	goto endwrite
	Example4TC:
	FileWrite $0 "	FileWrite $$R3 '$$8=$$9'$\r$\n$\r$\n"
	goto endwrite
	endwrite:
	FileWrite $0 '	Push $$0$\r$\n'
	FileWrite $0 'FunctionEnd$\r$\n'
	close:
	FileClose $0
	goto $6
	#FunctionEnd#

	showproject:
	StrCmp $R0 '1. LineFind' 0 +3
	ExecWait 'notepad.exe $CALLBACK'
	goto +4
	SetFileAttributes $PROJECT READONLY
	ExecWait 'notepad.exe $PROJECT'
	SetFileAttributes $PROJECT NORMAL
	Abort

	MainSend:
	GetDlgItem $0 $HWND 1210
	SendMessage $0 ${WM_SETTEXT} 1 "STR:"
	GetDlgItem $0 $HWND 1203
	ShowWindow $0 1
	EnableWindow $0 1
	GetDlgItem $0 $HWND 1204
	ShowWindow $0 1
	EnableWindow $0 1
	GetDlgItem $0 $HWND 1205
	EnableWindow $0 1
	GetDlgItem $0 $HWND 1206
	ShowWindow $0 0
	GetDlgItem $0 $HWND 1207
	ShowWindow $0 0
	GetDlgItem $0 $HWND 1208
	ShowWindow $0 0
	GetDlgItem $0 $HWND 1209
	ShowWindow $0 0
	GetDlgItem $0 $HWND 1211
	EnableWindow $0 1

	StrCmp $FUNCTION LineFind 0 +5
	StrCpy $LINEFIND1 $R1
	StrCpy $LINEFIND2 $R2
	StrCpy $LINEFIND3 $R3
	goto LineFindSend
	StrCmp $FUNCTION LineRead 0 +4
	StrCpy $LINEREAD1 $R1
	StrCpy $LINEREAD2 $R2
	goto LineFindSend
	StrCmp $FUNCTION FileReadFromEnd 0 +3
	StrCpy $FILEREADFROMEND1 $R1
	goto LineFindSend
	StrCmp $FUNCTION LineSum 0 +3
	StrCpy $LINESUM1 $R1
	goto LineFindSend
	StrCmp $FUNCTION FileJoin 0 +5
	StrCpy $FILEJOIN1 $R1
	StrCpy $FILEJOIN2 $R2
	StrCpy $FILEJOIN3 $R3
	goto LineFindSend
	StrCmp $FUNCTION TextCompare 0 +5
	StrCpy $TEXTCOMPARE1 $R1
	StrCpy $TEXTCOMPARE2 $R2
	StrCpy $TEXTCOMPARE3 $R3
	goto LineFindSend
	StrCmp $FUNCTION ConfigRead 0 +4
	StrCpy $CONFIGREAD1 $R1
	StrCpy $CONFIGREAD2 $R2
	goto LineFindSend
	StrCmp $FUNCTION ConfigWrite 0 +5
	StrCpy $CONFIGWRITE1 $R1
	StrCpy $CONFIGWRITE2 $R2
	StrCpy $CONFIGWRITE3 $R3
	goto LineFindSend
	StrCmp $FUNCTION FileRecode 0 +3
	StrCpy $FILERECODE1 $R1
	StrCpy $FILERECODE2 $R2

	LineFindSend:
	StrCmp $R0 "1. LineFind" 0 LineReadSend
	StrCmp $FUNCTION LineFind 0 LineFindSend2
	StrCmp $R4 "Example1" 0 +3
	StrCpy $LINEFIND3 "3:-1"
	goto LineFindSend2
	StrCmp $R4 "Example2" 0 +3
	StrCpy $LINEFIND3 "{5:12 15 -6:-5 -1}"
	goto LineFindSend2
	StrCmp $R4 "Example3" 0 +3
	StrCpy $LINEFIND3 "2:3 10:-5 -3:-2"
	goto LineFindSend2
	StrCmp $R4 "Example4" 0 +3
	StrCpy $LINEFIND3 "10"
	goto LineFindSend2
	StrCmp $R4 "Example5" 0 +3
	StrCpy $LINEFIND3 "1:-1"
	goto LineFindSend2
	StrCmp $R4 "Example6" 0 +3
	StrCpy $LINEFIND3 ""
	goto LineFindSend2
	StrCmp $R4 "Example7" 0 +2
	StrCpy $LINEFIND3 "1:-1"

	LineFindSend2:
	StrCpy $FUNCTION LineFind
	StrCmp $LINEFIND2 '/NUL' 0 +2
	StrCpy $LINEFIND2 ''
	GetDlgItem $0 $HWND 1201
	SendMessage $0 ${WM_SETTEXT} 1 "STR:$LINEFIND1"
	GetDlgItem $0 $HWND 1203
	SendMessage $0 ${WM_SETTEXT} 1 "STR:$LINEFIND2"
	GetDlgItem $0 $HWND 1205
	ShowWindow $0 1
	SendMessage $0 ${WM_SETTEXT} 1 "STR:$LINEFIND3"
	GetDlgItem $0 $HWND 1207
	ShowWindow $0 1
	GetDlgItem $0 $HWND 1211
	ShowWindow $0 1
	SendMessage $0 ${WM_SETTEXT} 1 "STR:Edit"
	GetDlgItem $0 $HWND 1212
	ShowWindow $0 1
	StrCmp $LOG '' +2
	EnableWindow $0 1
	GetDlgItem $0 $HWND 1213
	SendMessage $0 ${WM_SETTEXT} 1 "STR:InputFile"
	GetDlgItem $0 $HWND 1214
	SendMessage $0 ${WM_SETTEXT} 1 "STR:OutputFile"
	GetDlgItem $0 $HWND 1215
	SendMessage $0 ${WM_SETTEXT} 1 "STR:Range"
	GetDlgItem $0 $HWND 1216
	SendMessage $0 ${WM_SETTEXT} 1 "STR:Function"

	StrCmp $R4 "Example7" 0 +9
	GetDlgItem $0 $HWND 1203
	EnableWindow $0 0
	SendMessage $0 ${WM_ENABLE} 1 0
	SendMessage $0 ${WM_SETTEXT} 1 "STR:/NUL"
	GetDlgItem $0 $HWND 1204
	EnableWindow $0 0
	GetDlgItem $0 $HWND 1211
	EnableWindow $0 0
	abort


	LineReadSend:
	StrCmp $R0 "2. LineRead" 0 FileReadFromEndSend
	StrCpy $FUNCTION LineRead
	GetDlgItem $0 $HWND 1201
	SendMessage $0 ${WM_SETTEXT} 1 "STR:$LINEREAD1"
	GetDlgItem $0 $HWND 1203
	ShowWindow $0 1
	SendMessage $0 ${WM_SETTEXT} 1 "STR:$LINEREAD2"
	GetDlgItem $0 $HWND 1204
	ShowWindow $0 0
	GetDlgItem $0 $HWND 1205
	ShowWindow $0 0
	GetDlgItem $0 $HWND 1211
	ShowWindow $0 0
	GetDlgItem $0 $HWND 1212
	ShowWindow $0 0
	GetDlgItem $0 $HWND 1213
	SendMessage $0 ${WM_SETTEXT} 1 "STR:InputFile"
	GetDlgItem $0 $HWND 1214
	SendMessage $0 ${WM_SETTEXT} 1 "STR:Line #"
	GetDlgItem $0 $HWND 1215
	SendMessage $0 ${WM_SETTEXT} 1 "STR:"
	GetDlgItem $0 $HWND 1216
	SendMessage $0 ${WM_SETTEXT} 1 "STR:"
	Abort

	FileReadFromEndSend:
	StrCmp $R0 "3. FileReadFromEnd" 0 LineSumSend
	StrCpy $FUNCTION FileReadFromEnd
	GetDlgItem $0 $HWND 1201
	SendMessage $0 ${WM_SETTEXT} 1 "STR:$FILEREADFROMEND1"
	GetDlgItem $0 $HWND 1203
	ShowWindow $0 0
	GetDlgItem $0 $HWND 1204
	ShowWindow $0 0
	GetDlgItem $0 $HWND 1205
	ShowWindow $0 0
	GetDlgItem $0 $HWND 1209
	ShowWindow $0 1
	GetDlgItem $0 $HWND 1211
	ShowWindow $0 1
	SendMessage $0 ${WM_SETTEXT} 1 "STR:View"
	GetDlgItem $0 $HWND 1212
	ShowWindow $0 0
	GetDlgItem $0 $HWND 1213
	SendMessage $0 ${WM_SETTEXT} 1 "STR:InputFile"
	GetDlgItem $0 $HWND 1214
	SendMessage $0 ${WM_SETTEXT} 1 "STR:"
	GetDlgItem $0 $HWND 1215
	SendMessage $0 ${WM_SETTEXT} 1 "STR:"
	GetDlgItem $0 $HWND 1216
	SendMessage $0 ${WM_SETTEXT} 1 "STR:Function"
	Abort

	LineSumSend:
	StrCmp $R0 "4. LineSum" 0 FileJoinSend
	StrCpy $FUNCTION LineSum
	GetDlgItem $0 $HWND 1201
	SendMessage $0 ${WM_SETTEXT} 1 "STR:$LINESUM1"
	GetDlgItem $0 $HWND 1203
	ShowWindow $0 0
	GetDlgItem $0 $HWND 1204
	ShowWindow $0 0
	GetDlgItem $0 $HWND 1205
	ShowWindow $0 0
	GetDlgItem $0 $HWND 1211
	ShowWindow $0 0
	GetDlgItem $0 $HWND 1212
	ShowWindow $0 0
	GetDlgItem $0 $HWND 1213
	SendMessage $0 ${WM_SETTEXT} 1 "STR:InputFile"
	GetDlgItem $0 $HWND 1214
	SendMessage $0 ${WM_SETTEXT} 1 "STR:"
	GetDlgItem $0 $HWND 1215
	SendMessage $0 ${WM_SETTEXT} 1 "STR:"
	GetDlgItem $0 $HWND 1216
	SendMessage $0 ${WM_SETTEXT} 1 "STR:"
	Abort

	FileJoinSend:
	StrCmp $R0 "5. FileJoin" 0 TextCompareSend
	StrCpy $FUNCTION FileJoin
	GetDlgItem $0 $HWND 1201
	SendMessage $0 ${WM_SETTEXT} 1 "STR:$FILEJOIN1"
	GetDlgItem $0 $HWND 1203
	ShowWindow $0 1
	SendMessage $0 ${WM_SETTEXT} 1 "STR:$FILEJOIN2"
	GetDlgItem $0 $HWND 1204
	ShowWindow $0 1
	GetDlgItem $0 $HWND 1205
	ShowWindow $0 1
	EnableWindow $0 1
	SendMessage $0 ${WM_SETTEXT} 1 "STR:$FILEJOIN3"
	GetDlgItem $0 $HWND 1206
	ShowWindow $0 1
	GetDlgItem $0 $HWND 1211
	ShowWindow $0 0
	GetDlgItem $0 $HWND 1212
	ShowWindow $0 0
	GetDlgItem $0 $HWND 1213
	SendMessage $0 ${WM_SETTEXT} 1 "STR:InputFile1"
	GetDlgItem $0 $HWND 1214
	SendMessage $0 ${WM_SETTEXT} 1 "STR:InputFile2"
	GetDlgItem $0 $HWND 1215
	SendMessage $0 ${WM_SETTEXT} 1 "STR:OutputFile"
	GetDlgItem $0 $HWND 1216
	SendMessage $0 ${WM_SETTEXT} 1 "STR:"
	Abort

	TextCompareSend:
	StrCmp $R0 "6. TextCompare" 0 ConfigReadSend
	StrCmp $FUNCTION TextCompare 0 TextCompareSend2
	StrCmp $R5 "Example1" 0 +3
	StrCpy $TEXTCOMPARE3 "FastDiff"
	goto TextCompareSend2
	StrCmp $R5 "Example2" 0 +3
	StrCpy $TEXTCOMPARE3 "FastDiff"
	goto TextCompareSend2
	StrCmp $R5 "Example3" 0 +3
	StrCpy $TEXTCOMPARE3 "FastEqual"
	goto TextCompareSend2
	StrCmp $R5 "Example4" 0 +3
	StrCpy $TEXTCOMPARE3 "SlowDiff"
	goto TextCompareSend2
	StrCmp $R5 "Example5" 0 +2
	StrCpy $TEXTCOMPARE3 "SlowEqual"

	TextCompareSend2:
	StrCpy $FUNCTION TextCompare
	GetDlgItem $0 $HWND 1201
	SendMessage $0 ${WM_SETTEXT} 1 "STR:$TEXTCOMPARE1"
	GetDlgItem $0 $HWND 1203
	SendMessage $0 ${WM_SETTEXT} 1 "STR:$TEXTCOMPARE2"
	GetDlgItem $0 $HWND 1204
	ShowWindow $0 1
	GetDlgItem $0 $HWND 1205
	ShowWindow $0 1
	EnableWindow $0 0
	SendMessage $0 ${WM_ENABLE} 1 0
	SendMessage $0 ${WM_SETTEXT} 1 "STR:$TEXTCOMPARE3"
	GetDlgItem $0 $HWND 1208
	ShowWindow $0 1
	GetDlgItem $0 $HWND 1211
	ShowWindow $0 1
	SendMessage $0 ${WM_SETTEXT} 1 "STR:View"
	GetDlgItem $0 $HWND 1212
	ShowWindow $0 1
	GetDlgItem $0 $HWND 1213
	SendMessage $0 ${WM_SETTEXT} 1 "STR:TextFile1"
	GetDlgItem $0 $HWND 1214
	SendMessage $0 ${WM_SETTEXT} 1 "STR:TextFile2"
	GetDlgItem $0 $HWND 1215
	SendMessage $0 ${WM_SETTEXT} 1 "STR:Option"
	GetDlgItem $0 $HWND 1216
	SendMessage $0 ${WM_SETTEXT} 1 "STR:Function"
	abort

	ConfigReadSend:
	StrCmp $R0 "7. ConfigRead" 0 ConfigWriteSend
	StrCpy $FUNCTION ConfigRead
	GetDlgItem $0 $HWND 1201
	SendMessage $0 ${WM_SETTEXT} 1 "STR:$CONFIGREAD1"
	GetDlgItem $0 $HWND 1203
	ShowWindow $0 1
	SendMessage $0 ${WM_SETTEXT} 1 "STR:$CONFIGREAD2"
	GetDlgItem $0 $HWND 1204
	ShowWindow $0 0
	GetDlgItem $0 $HWND 1205
	ShowWindow $0 0
	GetDlgItem $0 $HWND 1211
	ShowWindow $0 0
	GetDlgItem $0 $HWND 1212
	ShowWindow $0 0
	GetDlgItem $0 $HWND 1213
	SendMessage $0 ${WM_SETTEXT} 1 "STR:InputFile"
	GetDlgItem $0 $HWND 1214
	SendMessage $0 ${WM_SETTEXT} 1 "STR:Entry"
	GetDlgItem $0 $HWND 1215
	SendMessage $0 ${WM_SETTEXT} 1 "STR:"
	GetDlgItem $0 $HWND 1216
	SendMessage $0 ${WM_SETTEXT} 1 "STR:"
	Abort

	ConfigWriteSend:
	StrCmp $R0 "8. ConfigWrite" 0 FileRecodeSend
	StrCpy $FUNCTION ConfigWrite
	GetDlgItem $0 $HWND 1201
	SendMessage $0 ${WM_SETTEXT} 1 "STR:$CONFIGWRITE1"
	GetDlgItem $0 $HWND 1203
	ShowWindow $0 1
	SendMessage $0 ${WM_SETTEXT} 1 "STR:$CONFIGWRITE2"
	GetDlgItem $0 $HWND 1204
	ShowWindow $0 0
	GetDlgItem $0 $HWND 1205
	ShowWindow $0 1
	SendMessage $0 ${WM_SETTEXT} 1 "STR:$CONFIGWRITE3"
	GetDlgItem $0 $HWND 1211
	ShowWindow $0 0
	GetDlgItem $0 $HWND 1212
	ShowWindow $0 0
	GetDlgItem $0 $HWND 1213
	SendMessage $0 ${WM_SETTEXT} 1 "STR:InputFile"
	GetDlgItem $0 $HWND 1214
	SendMessage $0 ${WM_SETTEXT} 1 "STR:Entry"
	GetDlgItem $0 $HWND 1215
	SendMessage $0 ${WM_SETTEXT} 1 "STR:Value"
	GetDlgItem $0 $HWND 1216
	SendMessage $0 ${WM_SETTEXT} 1 "STR:"
	Abort

	FileRecodeSend:
	StrCmp $R0 "9. FileRecode" 0 Abort
	StrCpy $FUNCTION FileRecode
	GetDlgItem $0 $HWND 1201
	SendMessage $0 ${WM_SETTEXT} 1 "STR:$FILERECODE1"
	GetDlgItem $0 $HWND 1203
	ShowWindow $0 1
	SendMessage $0 ${WM_SETTEXT} 1 "STR:$FILERECODE2"
	GetDlgItem $0 $HWND 1204
	ShowWindow $0 0
	GetDlgItem $0 $HWND 1205
	ShowWindow $0 0
	GetDlgItem $0 $HWND 1211
	ShowWindow $0 0
	GetDlgItem $0 $HWND 1212
	ShowWindow $0 0
	GetDlgItem $0 $HWND 1213
	SendMessage $0 ${WM_SETTEXT} 1 "STR:InputFile"
	GetDlgItem $0 $HWND 1214
	SendMessage $0 ${WM_SETTEXT} 1 "STR:Format"
	GetDlgItem $0 $HWND 1215
	SendMessage $0 ${WM_SETTEXT} 1 "STR:"
	GetDlgItem $0 $HWND 1216
	SendMessage $0 ${WM_SETTEXT} 1 "STR:"
	Abort

;=Enter=
	Enter:
	StrCmp $R1 '' 0 +3
	StrCpy $0 'Choose InputFile'
	goto send
	IfFileExists $R1 +3
	StrCpy $0 'InputFile is not exist'
	goto send

	StrCmp $R0 "1. LineFind" LineFindRead
	StrCmp $R0 "2. LineRead" LineRead
	StrCmp $R0 "3. FileReadFromEnd" FileReadFromEnd
	StrCmp $R0 "4. LineSum" LineSum
	StrCmp $R0 "5. FileJoin" FileJoin
	StrCmp $R0 "6. TextCompare" LineFind-TextCompare
	StrCmp $R0 "7. ConfigRead" ConfigRead
	StrCmp $R0 "8. ConfigWrite" ConfigWrite
	StrCmp $R0 "9. FileRecode" FileRecode
	Abort

	LineFindRead:
	StrCmp $R4 "Example7" 0 LineFind-TextCompare
	${LineFind} '$R1' '/NUL' '$R3' LineFindCallback
	IfErrors error
	StrCmp $R0 StopLineFind 0 done
	StrCpy $0 'stopped'
	goto send

	LineFind-TextCompare:
	GetLabelAddress $6 LineFindBack
	goto Edit
	LineFindBack:
	FileClose $0
	StrCmp $R0 "6. TextCompare" Compile
	StrCmp $CALLBACK '' Compile
	${FileJoin} "$PROJECT" "$CALLBACK" ""

	Compile:
	StrCmp $LOG '' 0 +4
	GetTempFileName $LOG $PLUGINSDIR
	GetDlgItem $0 $HWND 1212
	EnableWindow $0 1
	ReadRegStr $0 HKLM "SOFTWARE\NSIS" ""
	IfErrors 0 +2
	StrCpy $0 "${NSISDIR}"
	nsExec::Exec '"$0\makensis.exe" /O$LOG $PROJECT'
	Pop $0
	StrCmp $0 0 0 +6
	ExecWait '$PROJECT.exe' $0
	Delete $PROJECT
	Delete $PROJECT.exe
	StrCpy $PROJECT ''
	goto done
	MessageBox MB_YESNO|MB_ICONEXCLAMATION "Compile error. Open log?" IDNO +2
	Exec 'notepad.exe $LOG'
	StrCpy $0 "Compile Error"
	goto send

	LineRead:
	${LineRead} "$R1" "$R2" $0
	IfErrors error send

	FileReadFromEnd:
	${FileReadFromEnd} "$R1" "FileReadFromEndCallback"
	IfErrors error
	StrCmp $R0 StopFileReadFromEnd 0 done
	StrCpy $0 'stopped'
	goto send

	LineSum:
	${LineSum} "$R1" $0
	IfErrors error send

	FileJoin:
	${FileJoin} "$R1" "$R2" "$R3"
	IfErrors error
	MessageBox MB_YESNO "          Open output file?" IDNO done
	StrCmp $R3 '' 0 +3
	Exec '"notepad.exe" "$R1"'
	goto done
	Exec '"notepad.exe" "$R3"'
	goto done

	ConfigRead:
	${ConfigRead} "$R1" "$R2" $0
	IfErrors error send

	ConfigWrite:
	${ConfigWrite} "$R1" "$R2" "$R3" $0
	IfErrors error
	MessageBox MB_YESNO "          Open output file?" IDNO send
	Exec '"notepad.exe" "$R1"'
	goto send

	FileRecode:
	${FileRecode} "$R1" "$R2"
	IfErrors error
	MessageBox MB_YESNO "          Open output file?" IDNO done
	Exec '"notepad.exe" "$R1"'
	goto done

	error:
	StrCpy $0 'error'
	goto send

	done:
	StrCpy $0 'Done'

	send:
	GetDlgItem $1 $HWND 1210
	SendMessage $1 ${WM_SETTEXT} 1 "STR:$0"

	abort:
	Abort
FunctionEnd

Function LineFindCallback
	MessageBox MB_OKCANCEL '$$R9       "Line"=[$R9]$\n$$R8           "#"=[$R8]$\n$$R7          "-#"=[$R7]$\n$$R6   "Range"=[$R6]$\n$$R5     "Read"=[$R5]$\n$$R4     "Write"=[$R4]' IDOK +2
	StrCpy $R0 StopLineFind

	Push $R0
FunctionEnd

Function FileReadFromEndCallback
	MessageBox MB_OKCANCEL '$$9       "Line"=[$9]$\n$$8           "#"=[$8]$\n$$7          "-#"=[$7]' IDOK +2
	StrCpy $R0 StopFileReadFromEnd

	Push $R0
FunctionEnd

Function .onInit
	InitPluginsDir
	GetTempFileName $INI $PLUGINSDIR
	File /oname=$INI "TextFunc.ini"
FunctionEnd

Page instfiles

Section -Empty
SectionEnd
