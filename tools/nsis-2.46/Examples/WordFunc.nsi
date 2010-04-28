;_____________________________________________________________________________
;
;                          Word Functions
;_____________________________________________________________________________
;
; 2005 Shengalts Aleksander aka Instructor (Shengalts@mail.ru)

Name "Word Functions"
OutFile "WordFunc.exe"
Caption "$(^Name)"
XPStyle on
RequestExecutionLevel user

Var INI
Var HWND
Var STATE

!include "WinMessages.nsh"
!include "WordFunc.nsh"

Page Custom ShowCustom LeaveCustom

Function ShowCustom
	InstallOptions::initDialog "$INI"
	Pop $hwnd
	InstallOptions::show
	Pop $0
FunctionEnd

Function LeaveCustom
	ReadINIStr $0 $INI "Settings" "State"
	StrCmp $0 0 Enter

	GetDlgItem $1 $HWND 1202
	EnableWindow $1 1
	GetDlgItem $1 $HWND 1203
	ShowWindow $1 1
	GetDlgItem $1 $HWND 1204
	ShowWindow $1 1
	GetDlgItem $1 $HWND 1206
	EnableWindow $1 1
	GetDlgItem $1 $HWND 1205
	SendMessage $1 ${WM_SETTEXT} 1 "STR:"
	GetDlgItem $1 $HWND 1206
	SendMessage $1 ${WM_SETTEXT} 1 "STR:"

	ReadINIStr $0 $INI "Field 1" "State"
	StrCmp $0 "1. WordFind        (Find word by number)" 0 WordFind2Send
	GetDlgItem $1 $HWND 1202
	SendMessage $1 ${WM_SETTEXT} 1 "STR:|C:\"
	GetDlgItem $1 $HWND 1204
	SendMessage $1 ${WM_SETTEXT} 1 "STR:-4"
	GetDlgItem $1 $HWND 1210
	SendMessage $1 ${WM_SETTEXT} 1 "STR:Word #"
	GetDlgItem $1 $HWND 1211
	SendMessage $1 ${WM_SETTEXT} 1 "STR:Result (Word):"
	goto WordFindSend

	WordFind2Send:
	StrCmp $0 "                           (Delimiter exclude)" 0 WordFind3Send
	GetDlgItem $1 $HWND 1202
	SendMessage $1 ${WM_SETTEXT} 1 "STR:|C:\"
	GetDlgItem $1 $HWND 1204
	SendMessage $1 ${WM_SETTEXT} 1 "STR:E-2{"
	GetDlgItem $1 $HWND 1210
	SendMessage $1 ${WM_SETTEXT} 1 "STR:Word #"
	GetDlgItem $1 $HWND 1211
	SendMessage $1 ${WM_SETTEXT} 1 "STR:Result (Before{ or }after delimiter):"
	goto WordFindSend

	WordFind3Send:
	StrCmp $0 "                           (Sum of words)" 0 WordFind4Send
	GetDlgItem $1 $HWND 1202
	SendMessage $1 ${WM_SETTEXT} 1 "STR:|C:\"
	GetDlgItem $1 $HWND 1204
	SendMessage $1 ${WM_SETTEXT} 1 "STR:#"
	GetDlgItem $1 $HWND 1210
	SendMessage $1 ${WM_SETTEXT} 1 "STR:Option"
	GetDlgItem $1 $HWND 1211
	SendMessage $1 ${WM_SETTEXT} 1 "STR:Result (Sum of words):"
	goto WordFindSend

	WordFind4Send:
	StrCmp $0 "                           (Sum of delimiters)" 0 WordFind5Send
	GetDlgItem $1 $HWND 1202
	SendMessage $1 ${WM_SETTEXT} 1 "STR:|"
	GetDlgItem $1 $HWND 1204
	SendMessage $1 ${WM_SETTEXT} 1 "STR:E*"
	GetDlgItem $1 $HWND 1210
	SendMessage $1 ${WM_SETTEXT} 1 "STR:Option"
	GetDlgItem $1 $HWND 1211
	SendMessage $1 ${WM_SETTEXT} 1 "STR:Result (Sum of delimiters):"
	goto WordFindSend

	WordFind5Send:
	StrCmp $0 "                           (Find word number)" 0 WordFind6Send
	GetDlgItem $1 $HWND 1202
	SendMessage $1 ${WM_SETTEXT} 1 "STR:|C:\"
	GetDlgItem $1 $HWND 1204
	SendMessage $1 ${WM_SETTEXT} 1 "STR:/Program Files"
	GetDlgItem $1 $HWND 1210
	SendMessage $1 ${WM_SETTEXT} 1 "STR:/Word"
	GetDlgItem $1 $HWND 1211
	SendMessage $1 ${WM_SETTEXT} 1 "STR:Result (Word #):"
	goto WordFindSend

	WordFind6Send:
	StrCmp $0 "                           ( }} )" 0 WordFind7Send
	GetDlgItem $1 $HWND 1202
	SendMessage $1 ${WM_SETTEXT} 1 "STR:|C:\"
	GetDlgItem $1 $HWND 1204
	SendMessage $1 ${WM_SETTEXT} 1 "STR:E+2}}"
	GetDlgItem $1 $HWND 1210
	SendMessage $1 ${WM_SETTEXT} 1 "STR:Word #"
	GetDlgItem $1 $HWND 1211
	SendMessage $1 ${WM_SETTEXT} 1 "STR:Result (Before{{ or }}after word):"
	goto WordFindSend

	WordFind7Send:
	StrCmp $0 "                           ( {} )" 0 WordFind8Send
	GetDlgItem $1 $HWND 1202
	SendMessage $1 ${WM_SETTEXT} 1 "STR:|C:\"
	GetDlgItem $1 $HWND 1204
	SendMessage $1 ${WM_SETTEXT} 1 "STR:+2{}"
	GetDlgItem $1 $HWND 1210
	SendMessage $1 ${WM_SETTEXT} 1 "STR:Word #"
	GetDlgItem $1 $HWND 1211
	SendMessage $1 ${WM_SETTEXT} 1 "STR:Result (Without word):"
	goto WordFindSend

	WordFind8Send:
	StrCmp $0 "                           ( *} )" 0 WordFind2XSend
	GetDlgItem $1 $HWND 1202
	SendMessage $1 ${WM_SETTEXT} 1 "STR:|C:\"
	GetDlgItem $1 $HWND 1204
	SendMessage $1 ${WM_SETTEXT} 1 "STR:E+2*}"
	GetDlgItem $1 $HWND 1210
	SendMessage $1 ${WM_SETTEXT} 1 "STR:Word #"
	GetDlgItem $1 $HWND 1211
	SendMessage $1 ${WM_SETTEXT} 1 "STR:Result (Before{* or *}after word with word):"
	goto WordFindSend

	WordFind2XSend:
	StrCmp $0 "2. WordFind2X" 0 WordReplace1Send
	GetDlgItem $1 $HWND 1201
	SendMessage $1 ${WM_SETTEXT} 1 "STR:[C:\io.sys];[C:\logo.sys];[C:\WINDOWS]"
	GetDlgItem $1 $HWND 1202
	SendMessage $1 ${WM_SETTEXT} 1 "STR:[C:\"
	GetDlgItem $1 $HWND 1203
	EnableWindow $1 1
	SendMessage $1 ${WM_SETTEXT} 1 "STR:];"
	GetDlgItem $1 $HWND 1204
	SendMessage $1 ${WM_SETTEXT} 1 "STR:E+2"
	GetDlgItem $1 $HWND 1207
	SendMessage $1 ${WM_SETTEXT} 1 "STR:String"
	GetDlgItem $1 $HWND 1208
	SendMessage $1 ${WM_SETTEXT} 1 "STR:Delimiter1"
	GetDlgItem $1 $HWND 1209
	SendMessage $1 ${WM_SETTEXT} 1 "STR:Delimiter2"
	GetDlgItem $1 $HWND 1210
	SendMessage $1 ${WM_SETTEXT} 1 "STR:Word #"
	GetDlgItem $1 $HWND 1211
	SendMessage $1 ${WM_SETTEXT} 1 "STR:Result (Word):"
	abort

	WordReplace1Send:
	StrCmp $0 "3. WordReplace (Replace)" 0 WordReplace2Send
	GetDlgItem $1 $HWND 1201
	SendMessage $1 ${WM_SETTEXT} 1 "STR:C:\io.sys|C:\logo.sys|C:\WINDOWS"
	GetDlgItem $1 $HWND 1202
	SendMessage $1 ${WM_SETTEXT} 1 "STR:SYS"
	GetDlgItem $1 $HWND 1203
	SendMessage $1 ${WM_SETTEXT} 1 "STR:bmp"
	GetDlgItem $1 $HWND 1204
	SendMessage $1 ${WM_SETTEXT} 1 "STR:+2"
	goto WordReplaceSend

	WordReplace2Send:
	StrCmp $0 "                           (Delete)" 0 WordReplace3Send
	GetDlgItem $1 $HWND 1201
	SendMessage $1 ${WM_SETTEXT} 1 "STR:C:\io.sys|C:\logo.sys|C:\WINDOWS"
	GetDlgItem $1 $HWND 1202
	SendMessage $1 ${WM_SETTEXT} 1 "STR:SYS"
	GetDlgItem $1 $HWND 1203
	SendMessage $1 ${WM_SETTEXT} 1 "STR:"
	GetDlgItem $1 $HWND 1204
	SendMessage $1 ${WM_SETTEXT} 1 "STR:E+"
	goto WordReplaceSend

	WordReplace3Send:
	StrCmp $0 "                           (Multiple-replace)" 0 WordAdd1Send
	GetDlgItem $1 $HWND 1201
	SendMessage $1 ${WM_SETTEXT} 1 "STR:C:\io.sys||||||C:\logo.sys|||C:\WINDOWS"
	GetDlgItem $1 $HWND 1202
	SendMessage $1 ${WM_SETTEXT} 1 "STR:|"
	GetDlgItem $1 $HWND 1203
	SendMessage $1 ${WM_SETTEXT} 1 "STR:|"
	GetDlgItem $1 $HWND 1204
	SendMessage $1 ${WM_SETTEXT} 1 "STR:+1*"
	goto WordReplaceSend

	WordAdd1Send:
	StrCmp $0 "4. WordAdd        (Add)" 0 WordAdd2Send
	GetDlgItem $1 $HWND 1204
	SendMessage $1 ${WM_SETTEXT} 1 "STR:+C:\WINDOWS|C:\config.sys|C:\IO.SYS"
	GetDlgItem $1 $HWND 1211
	SendMessage $1 ${WM_SETTEXT} 1 "STR:Result (String1 + String2):"
	goto WordAddSend

	WordAdd2Send:
	StrCmp $0 "                           (Delete) " 0 WordInsertSend
	GetDlgItem $1 $HWND 1204
	SendMessage $1 ${WM_SETTEXT} 1 "STR:E-C:\WINDOWS|C:\config.sys|C:\IO.SYS"
	GetDlgItem $1 $HWND 1211
	SendMessage $1 ${WM_SETTEXT} 1 "STR:Result (String1 - String2):"
	goto WordAddSend

	WordInsertSend:
	StrCmp $0 "5. WordInsert" 0 StrFilter1Send
	GetDlgItem $1 $HWND 1201
	SendMessage $1 ${WM_SETTEXT} 1 "STR:C:\io.sys|C:\WINDOWS"
	GetDlgItem $1 $HWND 1202
	SendMessage $1 ${WM_SETTEXT} 1 "STR:|"
	GetDlgItem $1 $HWND 1203
	EnableWindow $1 1
	SendMessage $1 ${WM_SETTEXT} 1 "STR:C:\logo.sys"
	GetDlgItem $1 $HWND 1204
	SendMessage $1 ${WM_SETTEXT} 1 "STR:E+2"
	GetDlgItem $1 $HWND 1207
	SendMessage $1 ${WM_SETTEXT} 1 "STR:String"
	GetDlgItem $1 $HWND 1208
	SendMessage $1 ${WM_SETTEXT} 1 "STR:Delimiter"
	GetDlgItem $1 $HWND 1209
	SendMessage $1 ${WM_SETTEXT} 1 "STR:Word"
	GetDlgItem $1 $HWND 1210
	SendMessage $1 ${WM_SETTEXT} 1 "STR:Word #"
	GetDlgItem $1 $HWND 1211
	SendMessage $1 ${WM_SETTEXT} 1 "STR:Result:"
	abort

	StrFilter1Send:
	StrCmp $0 "6. StrFilter           (UpperCase)" 0 StrFilter2Send
	GetDlgItem $1 $HWND 1201
	SendMessage $1 ${WM_SETTEXT} 1 "STR:123abc 456DEF 7890|%#"
	GetDlgItem $1 $HWND 1202
	SendMessage $1 ${WM_SETTEXT} 1 "STR:+"
	GetDlgItem $1 $HWND 1203
	SendMessage $1 ${WM_SETTEXT} 1 "STR:"
	GetDlgItem $1 $HWND 1204
	SendMessage $1 ${WM_SETTEXT} 1 "STR:"
	GetDlgItem $1 $HWND 1211
	SendMessage $1 ${WM_SETTEXT} 1 "STR:Result (String in uppercase):"
	goto StrFilterSend

	StrFilter2Send:
	StrCmp $0 "                           (LowerCase)" 0 StrFilter3Send
	GetDlgItem $1 $HWND 1201
	SendMessage $1 ${WM_SETTEXT} 1 "STR:123abc 456DEF 7890|%#"
	GetDlgItem $1 $HWND 1202
	SendMessage $1 ${WM_SETTEXT} 1 "STR:-"
	GetDlgItem $1 $HWND 1203
	SendMessage $1 ${WM_SETTEXT} 1 "STR:ef"
	GetDlgItem $1 $HWND 1204
	SendMessage $1 ${WM_SETTEXT} 1 "STR:"
	GetDlgItem $1 $HWND 1211
	SendMessage $1 ${WM_SETTEXT} 1 "STR:Result (String in lowercase except EF):"
	goto StrFilterSend

	StrFilter3Send:
	StrCmp $0 "                           (Filter)" 0 VersionCompareSend
	GetDlgItem $1 $HWND 1201
	SendMessage $1 ${WM_SETTEXT} 1 "STR:123abc 456DEF 7890|%#"
	GetDlgItem $1 $HWND 1202
	SendMessage $1 ${WM_SETTEXT} 1 "STR:+12"
	GetDlgItem $1 $HWND 1203
	SendMessage $1 ${WM_SETTEXT} 1 "STR:b"
	GetDlgItem $1 $HWND 1204
	SendMessage $1 ${WM_SETTEXT} 1 "STR:def"
	GetDlgItem $1 $HWND 1211
	SendMessage $1 ${WM_SETTEXT} 1 "STR:Result (String Digits + Letters + b - def):"
	goto StrFilterSend

	VersionCompareSend:
	StrCmp $0 "7. VersionCompare" 0 VersionConvertSend
	GetDlgItem $1 $HWND 1201
	SendMessage $1 ${WM_SETTEXT} 1 "STR:1.1.1.9"
	GetDlgItem $1 $HWND 1202
	SendMessage $1 ${WM_SETTEXT} 1 "STR:1.1.1.01"
	GetDlgItem $1 $HWND 1203
	ShowWindow $1 0
	GetDlgItem $1 $HWND 1204
	ShowWindow $1 0
	GetDlgItem $1 $HWND 1206
	EnableWindow $1 0
	GetDlgItem $1 $HWND 1207
	SendMessage $1 ${WM_SETTEXT} 1 "STR:Version1"
	GetDlgItem $1 $HWND 1208
	SendMessage $1 ${WM_SETTEXT} 1 "STR:Version2"
	GetDlgItem $1 $HWND 1209
	SendMessage $1 ${WM_SETTEXT} 1 "STR:"
	GetDlgItem $1 $HWND 1210
	SendMessage $1 ${WM_SETTEXT} 1 "STR:"
	GetDlgItem $1 $HWND 1211
	SendMessage $1 ${WM_SETTEXT} 1 "STR:Result (0-equal 1-newer 2-older):"
	abort

	VersionConvertSend:
	StrCmp $0 "8. VersionConvert" 0 Abort
	GetDlgItem $1 $HWND 1201
	SendMessage $1 ${WM_SETTEXT} 1 "STR:9.0c"
	GetDlgItem $1 $HWND 1202
	SendMessage $1 ${WM_SETTEXT} 1 "STR:"
	GetDlgItem $1 $HWND 1203
	ShowWindow $1 0
	GetDlgItem $1 $HWND 1204
	ShowWindow $1 0
	SendMessage $1 ${WM_SETTEXT} 1 "STR:"
	GetDlgItem $1 $HWND 1206
	EnableWindow $1 0
	GetDlgItem $1 $HWND 1207
	SendMessage $1 ${WM_SETTEXT} 1 "STR:Version"
	GetDlgItem $1 $HWND 1208
	SendMessage $1 ${WM_SETTEXT} 1 "STR:CharList"
	GetDlgItem $1 $HWND 1209
	SendMessage $1 ${WM_SETTEXT} 1 "STR:"
	GetDlgItem $1 $HWND 1210
	SendMessage $1 ${WM_SETTEXT} 1 "STR:"
	GetDlgItem $1 $HWND 1211
	SendMessage $1 ${WM_SETTEXT} 1 "STR:Result (numerical version format):"
	abort

	Abort:
	Abort

	WordFindSend:
	GetDlgItem $1 $HWND 1203
	EnableWindow $1 0
	SendMessage $1 ${WM_SETTEXT} 1 "STR:"
	GetDlgItem $1 $HWND 1201
	SendMessage $1 ${WM_SETTEXT} 1 "STR:C:\io.sys|C:\logo.sys|C:\Program Files|C:\WINDOWS"
	GetDlgItem $1 $HWND 1207
	SendMessage $1 ${WM_SETTEXT} 1 "STR:String"
	GetDlgItem $1 $HWND 1208
	SendMessage $1 ${WM_SETTEXT} 1 "STR:Delimiter"
	GetDlgItem $1 $HWND 1209
	SendMessage $1 ${WM_SETTEXT} 1 "STR:"
	Abort

	WordReplaceSend:
	GetDlgItem $1 $HWND 1203
	EnableWindow $1 1
	GetDlgItem $1 $HWND 1207
	SendMessage $1 ${WM_SETTEXT} 1 "STR:String"
	GetDlgItem $1 $HWND 1208
	SendMessage $1 ${WM_SETTEXT} 1 "STR:Replace it"
	GetDlgItem $1 $HWND 1209
	SendMessage $1 ${WM_SETTEXT} 1 "STR:         with"
	GetDlgItem $1 $HWND 1210
	SendMessage $1 ${WM_SETTEXT} 1 "STR:Word #"
	GetDlgItem $1 $HWND 1211
	SendMessage $1 ${WM_SETTEXT} 1 "STR:Result:"
	Abort

	WordAddSend:
	GetDlgItem $1 $HWND 1203
	EnableWindow $1 0
	SendMessage $1 ${WM_SETTEXT} 1 "STR:"
	GetDlgItem $1 $HWND 1201
	SendMessage $1 ${WM_SETTEXT} 1 "STR:C:\io.sys|C:\logo.sys|C:\WINDOWS"
	GetDlgItem $1 $HWND 1202
	SendMessage $1 ${WM_SETTEXT} 1 "STR:|"
	GetDlgItem $1 $HWND 1207
	SendMessage $1 ${WM_SETTEXT} 1 "STR:String1"
	GetDlgItem $1 $HWND 1208
	SendMessage $1 ${WM_SETTEXT} 1 "STR:Delimiter"
	GetDlgItem $1 $HWND 1209
	SendMessage $1 ${WM_SETTEXT} 1 "STR:"
	GetDlgItem $1 $HWND 1210
	SendMessage $1 ${WM_SETTEXT} 1 "STR:String2"
	Abort

	StrFilterSend:
	GetDlgItem $1 $HWND 1203
	EnableWindow $1 1
	GetDlgItem $1 $HWND 1206
	EnableWindow $1 0
	GetDlgItem $1 $HWND 1207
	SendMessage $1 ${WM_SETTEXT} 1 "STR:String"
	GetDlgItem $1 $HWND 1208
	SendMessage $1 ${WM_SETTEXT} 1 "STR:Filter"
	GetDlgItem $1 $HWND 1209
	SendMessage $1 ${WM_SETTEXT} 1 "STR:Include"
	GetDlgItem $1 $HWND 1210
	SendMessage $1 ${WM_SETTEXT} 1 "STR:Exclude"
	Abort

;=Enter=
	Enter:
	StrCpy $0 ''
	ReadINIStr $STATE $INI "Field 1" "State"
	ReadINIStr $R1 $INI "Field 2" "State"
	ReadINIStr $R2 $INI "Field 3" "State"
	ReadINIStr $R3 $INI "Field 4" "State"
	ReadINIStr $R4 $INI "Field 5" "State"

	StrCmp $STATE "1. WordFind        (Find word by number)" WordFind
	StrCmp $STATE "                           (Delimiter exclude)" WordFind
	StrCmp $STATE "                           (Find in string)" WordFind
	StrCmp $STATE "                           (Sum of words)" WordFind
	StrCmp $STATE "                           (Sum of delimiters)" WordFind
	StrCmp $STATE "                           (Find word number)" WordFind
	StrCmp $STATE "                           ( }} )" WordFind
	StrCmp $STATE "                           ( {} )" WordFind
	StrCmp $STATE "                           ( *} )" WordFind
	StrCmp $STATE "2. WordFind2X" WordFind2X
	StrCmp $STATE "3. WordReplace (Replace)" WordReplace
	StrCmp $STATE "                           (Delete)" WordReplace
	StrCmp $STATE "                           (Multiple-replace)" WordReplace
	StrCmp $STATE "4. WordAdd        (Add)" WordAdd
	StrCmp $STATE "                           (Delete) " WordAdd
	StrCmp $STATE "5. WordInsert" WordInsert
	StrCmp $STATE "6. StrFilter           (UpperCase)" StrFilter
	StrCmp $STATE "                           (LowerCase)" StrFilter
	StrCmp $STATE "                           (Filter)" StrFilter
	StrCmp $STATE "7. VersionCompare" VersionCompare
	StrCmp $STATE "8. VersionConvert" VersionConvert
	Abort

	WordFind:
	${WordFind} "$R1" "$R2" "$R4" $R0
	IfErrors 0 Send
	StrCpy $0 $R0
	StrCmp $R0 3 0 +3
	StrCpy $3 '"+1" "-1" "+1}" "+1{" "#" "/word"'
	goto error3
	StrCmp $R0 2 0 error1
	StrCpy $R4 $R4 '' 1
	StrCpy $1 $R4 1
	StrCmp $1 / 0 error2
	StrCpy $R4 $R4 '' 1
	StrCpy $R0 '"$R4" no such word.'
	goto Send

	WordFind2X:
	${WordFind2X} "$R1" "$R2" "$R3" "$R4" $R0
	IfErrors 0 Send
	StrCpy $0 $R0
	StrCmp $R0 3 0 +3
	StrCpy $3 '"+1" "-1"'
	goto error3
	StrCmp $R0 2 +3
	StrCpy $R0 '"$R2...$R3" no words found.'
	goto Send
	StrCpy $R4 $R4 '' 1
	StrCpy $1 $R4 1
	StrCmp $1 / 0 +2
	StrCpy $R4 $R4 '' 1
	StrCpy $R0 '"$R4" no such word.'
	goto Send

	WordReplace:
	${WordReplace} "$R1" "$R2" "$R3" "$R4" $R0
	IfErrors 0 Send
	StrCpy $0 $R0
	StrCmp $R0 3 0 +3
	StrCpy $3 '"+1" "+1*" "+" "+*" "{}"'
	goto error3
	StrCmp $R0 2 0 error1
	StrCpy $R4 $R4 '' 1
	goto error2

	WordAdd:
	${WordAdd} "$R1" "$R2" "$R4" $R0
	IfErrors 0 Send
	StrCpy $0 $R0
	StrCmp $R0 3 0 error1empty
	StrCpy $3 '"+text" "-text"'
	goto error3

	WordInsert:
	${WordInsert} "$R1" "$R2" "$R3" "$R4" $R0
	IfErrors 0 Send
	StrCpy $0 $R0
	StrCmp $R0 3 0 +3
	StrCpy $3 '"+1" "-1"'
	goto error3
	StrCmp $R0 2 0 error1empty
	StrCpy $R4 $R4 '' 1
	goto error2

	StrFilter:
	${StrFilter} "$R1" "$R2" "$R3" "$R4" $R0
	IfErrors 0 Send
	StrCpy $R0 'Syntax error'
	goto Send

	VersionCompare:
	${VersionCompare} "$R1" "$R2" $R0
	goto Send

	VersionConvert:
	${VersionConvert} "$R1" "$R2" $R0
	goto Send

	error3:
	StrCpy $R0 '"$R4" syntax error ($3)'
	goto Send
	error2:
	StrCpy $R0 '"$R4" no such word number'
	goto Send
	error1empty:
	StrCpy $R0 '"$R2" delimiter is empty'
	goto Send
	error1:
	StrCpy $R0 '"$R2" delimiter not found in string'
	goto Send

	Send:
	GetDlgItem $1 $HWND 1205
	SendMessage $1 ${WM_SETTEXT} 1 "STR:$R0"
	GetDlgItem $1 $HWND 1206
	SendMessage $1 ${WM_SETTEXT} 1 "STR:$0"
	abort
FunctionEnd

Function .onInit
	InitPluginsDir
	GetTempFileName $INI $PLUGINSDIR
	File /oname=$INI "WordFunc.ini"
FunctionEnd

Page instfiles

Section "Empty"
SectionEnd
