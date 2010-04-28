;_____________________________________________________________________________
;
;                          Text Functions Test
;_____________________________________________________________________________
;
; 2006 Shengalts Aleksander aka Instructor (Shengalts@mail.ru)

Name "Text Functions Test"
OutFile "TextFuncTest.exe"
Caption "$(^Name)"
ShowInstDetails show
XPStyle on
RequestExecutionLevel user

Var FUNCTION
Var TEMPFILE1
Var TEMPFILE2
Var TEMPFILE3
Var HANDLE
Var OUT

!include "TextFunc.nsh"

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



Section CreateTestFile
	GetTempFileName $TEMPFILE1
	FileOpen $HANDLE $TEMPFILE1 w
	FileWrite $HANDLE '1A=a$\r$\n'
	FileWrite $HANDLE '2B=b$\r$\n'
	FileWrite $HANDLE '3C=c$\r$\n'
	FileWrite $HANDLE '4D=d$\r$\n'
	FileWrite $HANDLE '5E=e$\r$\n'
	FileClose $HANDLE
	GetTempFileName $TEMPFILE2
	GetTempFileName $TEMPFILE3
SectionEnd


Section LineFind
	${StackVerificationStart} LineFind

	${LineFind} '$TEMPFILE1' '/NUL' '1:-4 3 -1' 'LineFindCallback1'
	IfErrors error
	StrCmp $OUT '|1:2|-5|1|1A=a$\r$\n|1:2|-4|2|2B=b$\r$\n|3:3|-3|3|3C=c$\r$\n' 0 error

	StrCpy $OUT ''
	SetDetailsPrint none
	${LineFind} '$TEMPFILE1' '$TEMPFILE2' '1:-1' 'LineFindCallback2'
	SetDetailsPrint both
	IfErrors error
	StrCmp $OUT '|1:-1||1|1A=a$\r$\n|1:-1||2|4D=d$\r$\n|1:-1||3|3C=c$\r$\n|1:-1||4|2B=B$\r$\n|1:-1||5|5E=e$\r$\n' 0 error

	goto +2
	error:
	SetErrors

	${StackVerificationEnd}
SectionEnd

Function LineFindCallback1
	StrCpy $OUT '$OUT|$R6|$R7|$R8|$R9'
	StrCmp $R8 3 0 +2
	StrCpy $0 StopLineFind

	Push $0
FunctionEnd

Function LineFindCallback2
	StrCmp $R8 2 0 +2
	StrCpy $R9 '4D=d$\r$\n'
	StrCmp $R8 4 0 +2
	StrCpy $R9 '2B=B$\r$\n'

	StrCpy $OUT '$OUT|$R6|$R7|$R8|$R9'

	Push $0
FunctionEnd


Section LineRead
	${StackVerificationStart} LineRead

	${LineRead} '$TEMPFILE1' '-1' $OUT
	IfErrors error
	StrCmp $OUT '5E=e$\r$\n' 0 error

	goto +2
	error:
	SetErrors

	${StackVerificationEnd}
SectionEnd


Section FileReadFromEnd
	${StackVerificationStart} FileReadFromEnd

	StrCpy $OUT ''
	${FileReadFromEnd} '$TEMPFILE1' 'FileReadFromEndCallback'
	IfErrors error
	StrCmp $OUT '|-1|5|5E=e$\r$\n|-2|4|4D=d$\r$\n|-3|3|3C=c$\r$\n|-4|2|2B=b$\r$\n' 0 error

	goto +2
	error:
	SetErrors

	${StackVerificationEnd}
SectionEnd

Function FileReadFromEndCallback
	StrCpy $OUT '$OUT|$7|$8|$9'
	StrCmp $8 2 0 +2
	StrCpy $0 StopFileReadFromEnd

	Push $0
FunctionEnd


Section LineSum
	${StackVerificationStart} LineSum

	${LineSum} '$TEMPFILE1' $OUT
	IfErrors error
	StrCmp $OUT '5' 0 error

	goto +2
	error:
	SetErrors

	${StackVerificationEnd}
SectionEnd


Section FileJoin
	${StackVerificationStart} FileJoin

	SetDetailsPrint none
	${FileJoin} '$TEMPFILE1' '$TEMPFILE2' '$TEMPFILE3'
	SetDetailsPrint both

	${StackVerificationEnd}
SectionEnd


Section TextCompare
	${StackVerificationStart} TextCompare

	StrCpy $OUT ''
	${TextCompare} '$TEMPFILE1' '$TEMPFILE2' 'FastDiff' 'TextCompareCallback'
	StrCmp $OUT '|2|4D=d$\r$\n|2|2B=b$\r$\n|4|2B=B$\r$\n|4|4D=d$\r$\n' 0 error

	StrCpy $OUT ''
	${TextCompare} '$TEMPFILE1' '$TEMPFILE2' 'FastEqual' 'TextCompareCallback'
	StrCmp $OUT '|1|1A=a$\r$\n|1|1A=a$\r$\n|3|3C=c$\r$\n|3|3C=c$\r$\n|5|5E=e$\r$\n|5|5E=e$\r$\n' 0 error

	StrCpy $OUT ''
	${TextCompare} '$TEMPFILE1' '$TEMPFILE2' 'SlowDiff' 'TextCompareCallback'
	StrCmp $OUT '' 0 error

	StrCpy $OUT ''
	${TextCompare} '$TEMPFILE1' '$TEMPFILE2' 'SlowEqual' 'TextCompareCallback'
	StrCmp $OUT '|1|1A=a$\r$\n|1|1A=a$\r$\n|4|2B=B$\r$\n|2|2B=b$\r$\n|3|3C=c$\r$\n|3|3C=c$\r$\n|2|4D=d$\r$\n|4|4D=d$\r$\n|5|5E=e$\r$\n|5|5E=e$\r$\n' 0 error

	goto +2
	error:
	SetErrors

	${StackVerificationEnd}
SectionEnd

Section TextCompareS
	${StackVerificationStart} TextCompareS

	StrCpy $OUT ''
	${TextCompareS} '$TEMPFILE1' '$TEMPFILE2' 'SlowDiff' 'TextCompareCallback'
	StrCmp $OUT '|||2|2B=b$\r$\n' 0 error

	StrCpy $OUT ''
	${TextCompareS} '$TEMPFILE1' '$TEMPFILE2' 'SlowEqual' 'TextCompareCallback'
	StrCmp $OUT '|1|1A=a$\r$\n|1|1A=a$\r$\n|3|3C=c$\r$\n|3|3C=c$\r$\n|2|4D=d$\r$\n|4|4D=d$\r$\n|5|5E=e$\r$\n|5|5E=e$\r$\n' 0 error

	goto +2
	error:
	SetErrors

	${StackVerificationEnd}
SectionEnd

Function TextCompareCallback
	StrCpy $OUT '$OUT|$6|$7|$8|$9'

	Push $0
FunctionEnd


Section ConfigRead
	${StackVerificationStart} ConfigRead

	${ConfigRead} '$TEMPFILE1' '3c=' $OUT
	StrCmp $OUT 'c' 0 error

	${ConfigRead} '$TEMPFILE1' '6F=' $OUT
	StrCmp $OUT '' 0 error

	${ConfigRead} '$TEMPFILE1' 'FF=' $OUT
	IfErrors 0 error

	goto +2
	error:
	SetErrors

	${StackVerificationEnd}
SectionEnd


Section ConfigReadS
	${StackVerificationStart} ConfigReadS

	${ConfigReadS} '$TEMPFILE1' '3C=' $OUT
	StrCmp $OUT 'c' 0 error

	${ConfigReadS} '$TEMPFILE1' '3c=' $OUT
	IfErrors 0 error
	StrCmp $OUT '' 0 error

	goto +2
	error:
	SetErrors

	${StackVerificationEnd}
SectionEnd


Section ConfigWrite
	${StackVerificationStart} ConfigWrite

	${ConfigWrite} '$TEMPFILE1' '5E=' 'e**' $OUT
	StrCmp $OUT 'CHANGED' 0 error

	${ConfigWrite} '$TEMPFILE1' '2B=' '' $OUT
	StrCmp $OUT 'DELETED' 0 error

	${ConfigWrite} '$TEMPFILE1' '3c=' 'c' $OUT
	StrCmp $OUT 'SAME' 0 error

	${ConfigWrite} '$TEMPFILE1' '6F=' '*' $OUT
	StrCmp $OUT 'ADDED' 0 error

	goto +2
	error:
	SetErrors

	${StackVerificationEnd}
SectionEnd


Section ConfigWriteS
	${StackVerificationStart} ConfigWriteS

	${ConfigWriteS} '$TEMPFILE1' '5e=' 'e**' $OUT
	StrCmp $OUT 'ADDED' 0 error

	goto +2
	error:
	SetErrors

	${StackVerificationEnd}
SectionEnd


Section FileRecode
	${StackVerificationStart} FileRecode

	${FileRecode} '$TEMPFILE1' 'CharToOem'

	${StackVerificationEnd}
SectionEnd


Section TrimNewLines
	${StackVerificationStart} TrimNewLines

	${TrimNewLines} 'Text Line$\r$\n' $OUT
	StrCmp $OUT 'Text Line' 0 error

	${TrimNewLines} 'Text Line' $OUT
	StrCmp $OUT 'Text Line' 0 error

	${TrimNewLines} 'Text Line$\n' $OUT
	StrCmp $OUT 'Text Line' 0 error

	goto +2
	error:
	SetErrors

	${StackVerificationEnd}
SectionEnd


Section WriteUninstaller
	SetDetailsPrint none
	Delete $TEMPFILE1
	Delete $TEMPFILE2
	Delete $TEMPFILE3
	SetDetailsPrint both
	goto +2
	WriteUninstaller '$EXEDIR\un.TextFuncTest.exe'
SectionEnd



;############### UNINSTALL ###############

Section un.Uninstall
	${LineFind} '$TEMPFILE1' '/NUL' '1:-1' 'un.LineFindCallback'
	${LineRead} '$TEMPFILE1' '-1' $OUT
	${FileReadFromEnd} '$TEMPFILE1' 'un.FileReadFromEndCallback'
	${LineSum} '$TEMPFILE1' $OUT
	${FileJoin} '$TEMPFILE1' '$TEMPFILE2' '$TEMPFILE3'
	${TextCompare} '$TEMPFILE1' '$TEMPFILE2' 'FastDiff' 'un.TextCompareCallback'
	${TextCompareS} '$TEMPFILE1' '$TEMPFILE2' 'FastDiff' 'un.TextCompareCallback'
	${ConfigRead} '$TEMPFILE1' '3c=' $OUT
	${ConfigReadS} '$TEMPFILE1' '3c=' $OUT
	${ConfigWrite} '$TEMPFILE1' '5E=' 'e**' $OUT
	${ConfigWriteS} '$TEMPFILE1' '5E=' 'e**' $OUT
	${FileRecode} '$TEMPFILE1' 'CharToOem'
	${TrimNewLines} 'Text Line$\r$\n' $OUT
SectionEnd

Function un.LineFindCallback
	Push $0
FunctionEnd

Function un.FileReadFromEndCallback
	Push $0
FunctionEnd

Function un.TextCompareCallback
	Push $0
FunctionEnd
