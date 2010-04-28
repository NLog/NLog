/*
_____________________________________________________________________________

                       Text Functions Header v2.4
_____________________________________________________________________________

 2006 Shengalts Aleksander aka Instructor (Shengalts@mail.ru)

 See documentation for more information about the following functions.

 Usage in script:
 1. !include "TextFunc.nsh"
 2. [Section|Function]
      ${TextFunction} "File" "..."  $var
    [SectionEnd|FunctionEnd]


 TextFunction=[LineFind|LineRead|FileReadFromEnd|LineSum|FileJoin|
               TextCompare|TextCompareS|ConfigRead|ConfigReadS|
               ConfigWrite|ConfigWriteS|FileRecode|TrimNewLines]

_____________________________________________________________________________

                       Thanks to:
_____________________________________________________________________________

LineRead
	Afrow UK (Based on his idea of Function "ReadFileLine")
LineSum
	Afrow UK (Based on his idea of Function "LineCount")
FileJoin
	Afrow UK (Based on his idea of Function "JoinFiles")
ConfigRead
	vbgunz (His idea)
ConfigWrite
	vbgunz (His idea)
TrimNewLines
	sunjammer (Based on his Function "TrimNewLines")
*/


;_____________________________________________________________________________
;
;                                   Macros
;_____________________________________________________________________________
;
; Change log window verbosity (default: 3=no script)
;
; Example:
; !include "TextFunc.nsh"
; !insertmacro LineFind
; ${TEXTFUNC_VERBOSE} 4   # all verbosity
; !insertmacro LineSum
; ${TEXTFUNC_VERBOSE} 3   # no script

!ifndef TEXTFUNC_INCLUDED
!define TEXTFUNC_INCLUDED

!include FileFunc.nsh
!include Util.nsh

!verbose push
!verbose 3
!ifndef _TEXTFUNC_VERBOSE
	!define _TEXTFUNC_VERBOSE 3
!endif
!verbose ${_TEXTFUNC_VERBOSE}
!define TEXTFUNC_VERBOSE `!insertmacro TEXTFUNC_VERBOSE`
!verbose pop

!macro TEXTFUNC_VERBOSE _VERBOSE
	!verbose push
	!verbose 3
	!undef _TEXTFUNC_VERBOSE
	!define _TEXTFUNC_VERBOSE ${_VERBOSE}
	!verbose pop
!macroend

!macro LineFindCall _INPUT _OUTPUT _RANGE _FUNC
	!verbose push
	!verbose ${_TEXTFUNC_VERBOSE}
	Push $0
	Push `${_INPUT}`
	Push `${_OUTPUT}`
	Push `${_RANGE}`
	GetFunctionAddress $0 `${_FUNC}`
	Push `$0`
	${CallArtificialFunction} LineFind_
	Pop $0
	!verbose pop
!macroend

!macro LineReadCall _FILE _NUMBER _RESULT
	!verbose push
	!verbose ${_TEXTFUNC_VERBOSE}
	Push `${_FILE}`
	Push `${_NUMBER}`
	${CallArtificialFunction} LineRead_
	Pop ${_RESULT}
	!verbose pop
!macroend

!macro FileReadFromEndCall _FILE _FUNC
	!verbose push
	!verbose ${_TEXTFUNC_VERBOSE}
	Push $0
	Push `${_FILE}`
	GetFunctionAddress $0 `${_FUNC}`
	Push `$0`
	${CallArtificialFunction} FileReadFromEnd_
	Pop $0
	!verbose pop
!macroend

!macro LineSumCall _FILE _RESULT
	!verbose push
	!verbose ${_TEXTFUNC_VERBOSE}
	Push `${_FILE}`
	${CallArtificialFunction} LineSum_
	Pop ${_RESULT}
	!verbose pop
!macroend

!macro FileJoinCall _FILE1 _FILE2 _FILE3
	!verbose push
	!verbose ${_TEXTFUNC_VERBOSE}
	Push `${_FILE1}`
	Push `${_FILE2}`
	Push `${_FILE3}`
	${CallArtificialFunction} FileJoin_
	!verbose pop
!macroend

!macro TextCompareCall _FILE1 _FILE2 _OPTION _FUNC
	!verbose push
	!verbose ${_TEXTFUNC_VERBOSE}
	Push $0
	Push `${_FILE1}`
	Push `${_FILE2}`
	Push `${_OPTION}`
	GetFunctionAddress $0 `${_FUNC}`
	Push `$0`
	${CallArtificialFunction} TextCompare_
	Pop $0
	!verbose pop
!macroend

!macro TextCompareSCall _FILE1 _FILE2 _OPTION _FUNC
	!verbose push
	!verbose ${_TEXTFUNC_VERBOSE}
	Push $0
	Push `${_FILE1}`
	Push `${_FILE2}`
	Push `${_OPTION}`
	GetFunctionAddress $0 `${_FUNC}`
	Push `$0`
	${CallArtificialFunction} TextCompareS_
	Pop $0
	!verbose pop
!macroend

!macro ConfigReadCall _FILE _ENTRY _RESULT
	!verbose push
	!verbose ${_TEXTFUNC_VERBOSE}
	Push `${_FILE}`
	Push `${_ENTRY}`
	${CallArtificialFunction} ConfigRead_
	Pop ${_RESULT}
	!verbose pop
!macroend

!macro ConfigReadSCall _FILE _ENTRY _RESULT
	!verbose push
	!verbose ${_TEXTFUNC_VERBOSE}
	Push `${_FILE}`
	Push `${_ENTRY}`
	${CallArtificialFunction} ConfigReadS_
	Pop ${_RESULT}
	!verbose pop
!macroend

!macro ConfigWriteCall _FILE _ENTRY _VALUE _RESULT
	!verbose push
	!verbose ${_TEXTFUNC_VERBOSE}
	Push `${_FILE}`
	Push `${_ENTRY}`
	Push `${_VALUE}`
	${CallArtificialFunction} ConfigWrite_
	Pop ${_RESULT}
	!verbose pop
!macroend

!macro ConfigWriteSCall _FILE _ENTRY _VALUE _RESULT
	!verbose push
	!verbose ${_TEXTFUNC_VERBOSE}
	Push `${_FILE}`
	Push `${_ENTRY}`
	Push `${_VALUE}`
	${CallArtificialFunction} ConfigWriteS_
	Pop ${_RESULT}
	!verbose pop
!macroend

!macro FileRecodeCall _FILE _FORMAT
	!verbose push
	!verbose ${_TEXTFUNC_VERBOSE}
	Push `${_FILE}`
	Push `${_FORMAT}`
	${CallArtificialFunction} FileRecode_
	!verbose pop
!macroend

!macro TrimNewLinesCall _FILE _RESULT
	!verbose push
	!verbose ${_TEXTFUNC_VERBOSE}
	Push `${_FILE}`
	${CallArtificialFunction} TrimNewLines_
	Pop ${_RESULT}
	!verbose pop
!macroend

!macro _TextFunc_TempFileForFile _FILE _RESULT
	# XXX replace with GetParent
	Push `${_FILE}`
	Exch $0
	Push $1
	Push $2

	StrCpy $2 $0 1 -1
	StrCmp $2 '\' 0 +3
	StrCpy $0 $0 -1
	goto -3

	StrCpy $1 0
	IntOp $1 $1 - 1
	StrCpy $2 $0 1 $1
	StrCmp $2 '\' +2
	StrCmp $2 '' 0 -3
	StrCpy $0 $0 $1

	Pop $2
	Pop $1
	Exch $0
	Pop ${_RESULT}
	# XXX
	StrCmp ${_RESULT} "" 0 +2
		StrCpy ${_RESULT} $EXEDIR
	GetTempFileName ${_RESULT} ${_RESULT}
	StrCmp ${_RESULT} "" 0 +2
		GetTempFileName ${_RESULT}
	ClearErrors
!macroend

!define LineFind `!insertmacro LineFindCall`
!define un.LineFind `!insertmacro LineFindCall`

!macro LineFind
!macroend

!macro un.LineFind
!macroend

!macro LineFind_
	!verbose push
	!verbose ${_TEXTFUNC_VERBOSE}

	Exch $3
	Exch
	Exch $2
	Exch
	Exch 2
	Exch $1
	Exch 2
	Exch 3
	Exch $0
	Exch 3
	Push $4
	Push $5
	Push $6
	Push $7
	Push $8
	Push $9
	Push $R4
	Push $R5
	Push $R6
	Push $R7
	Push $R8
	Push $R9
	ClearErrors

	IfFileExists '$0' 0 TextFunc_LineFind_error
	StrCmp $1 '/NUL' TextFunc_LineFind_begin
	StrCpy $8 0
	IntOp $8 $8 - 1
	StrCpy $9 $1 1 $8
	StrCmp $9 \ +2
	StrCmp $9 '' +3 -3
	StrCpy $9 $1 $8
	IfFileExists '$9\*.*' 0 TextFunc_LineFind_error

	TextFunc_LineFind_begin:
	StrCpy $4 1
	StrCpy $5 -1
	StrCpy $6 0
	StrCpy $7 0
	StrCpy $R4 ''
	StrCpy $R6 ''
	StrCpy $R7 ''
	StrCpy $R8 0

	StrCpy $8 $2 1
	StrCmp $8 '{' 0 TextFunc_LineFind_delspaces
	StrCpy $2 $2 '' 1
	StrCpy $8 $2 1 -1
	StrCmp $8 '}' 0 TextFunc_LineFind_delspaces
	StrCpy $2 $2 -1
	StrCpy $R6 TextFunc_LineFind_cut

	TextFunc_LineFind_delspaces:
	StrCpy $8 $2 1
	StrCmp $8 ' ' 0 +3
	StrCpy $2 $2 '' 1
	goto -3
	StrCmp $2$7 '0' TextFunc_LineFind_file
	StrCpy $4 ''
	StrCpy $5 ''
	StrCmp $2 '' TextFunc_LineFind_writechk

	TextFunc_LineFind_range:
	StrCpy $8 0
	StrCpy $9 $2 1 $8
	StrCmp $9 '' +5
	StrCmp $9 ' ' +4
	StrCmp $9 ':' +3
	IntOp $8 $8 + 1
	goto -5
	StrCpy $5 $2 $8
	IntOp $5 $5 + 0
	IntOp $8 $8 + 1
	StrCpy $2 $2 '' $8
	StrCmp $4 '' 0 +2
	StrCpy $4 $5
	StrCmp $9 ':' TextFunc_LineFind_range

	IntCmp $4 0 0 +2
	IntCmp $5 -1 TextFunc_LineFind_goto 0 TextFunc_LineFind_growthcmp
	StrCmp $R7 '' 0 TextFunc_LineFind_minus2plus
	StrCpy $R7 0
	FileOpen $8 $0 r
	FileRead $8 $9
	IfErrors +3
	IntOp $R7 $R7 + 1
	Goto -3
	FileClose $8

	TextFunc_LineFind_minus2plus:
	IntCmp $4 0 +5 0 +5
	IntOp $4 $R7 + $4
	IntOp $4 $4 + 1
	IntCmp $4 0 +2 0 +2
	StrCpy $4 0
	IntCmp $5 -1 TextFunc_LineFind_goto 0 TextFunc_LineFind_growthcmp
	IntOp $5 $R7 + $5
	IntOp $5 $5 + 1
	TextFunc_LineFind_growthcmp:
	IntCmp $4 $5 TextFunc_LineFind_goto TextFunc_LineFind_goto
	StrCpy $5 $4
	TextFunc_LineFind_goto:
	goto $7

	TextFunc_LineFind_file:
	StrCmp $1 '/NUL' TextFunc_LineFind_notemp
	!insertmacro _TextFunc_TempFileForFile $1 $R4
	Push $R4
	FileOpen $R4 $R4 w
	TextFunc_LineFind_notemp:
	FileOpen $R5 $0 r
	IfErrors TextFunc_LineFind_preerror

	TextFunc_LineFind_loop:
	IntOp $R8 $R8 + 1
	FileRead $R5 $R9
	IfErrors TextFunc_LineFind_handleclose

	TextFunc_LineFind_cmp:
	StrCmp $2$4$5 '' TextFunc_LineFind_writechk
	IntCmp $4 $R8 TextFunc_LineFind_call 0 TextFunc_LineFind_writechk
	StrCmp $5 -1 TextFunc_LineFind_call
	IntCmp $5 $R8 TextFunc_LineFind_call 0 TextFunc_LineFind_call

	GetLabelAddress $7 TextFunc_LineFind_cmp
	goto TextFunc_LineFind_delspaces

	TextFunc_LineFind_call:
	StrCpy $7 $R9
	Push $0
	Push $1
	Push $2
	Push $3
	Push $4
	Push $5
	Push $6
	Push $7
	Push $R4
	Push $R5
	Push $R6
	Push $R7
	Push $R8
	StrCpy $R6 '$4:$5'
	StrCmp $R7 '' +3
	IntOp $R7 $R8 - $R7
	IntOp $R7 $R7 - 1
	Call $3
	Pop $9
	Pop $R8
	Pop $R7
	Pop $R6
	Pop $R5
	Pop $R4
	Pop $7
	Pop $6
	Pop $5
	Pop $4
	Pop $3
	Pop $2
	Pop $1
	Pop $0
	IfErrors TextFunc_LineFind_preerror
	StrCmp $9 'StopLineFind' 0 +3
	IntOp $6 $6 + 1
	goto TextFunc_LineFind_handleclose
	StrCmp $1 '/NUL' TextFunc_LineFind_loop
	StrCmp $9 'SkipWrite' 0 +3
	IntOp $6 $6 + 1
	goto TextFunc_LineFind_loop
	StrCmp $7 $R9 TextFunc_LineFind_write
	IntOp $6 $6 + 1
	goto TextFunc_LineFind_write

	TextFunc_LineFind_writechk:
	StrCmp $1 '/NUL' TextFunc_LineFind_loop
	StrCmp $R6 TextFunc_LineFind_cut 0 TextFunc_LineFind_write
	IntOp $6 $6 + 1
	goto TextFunc_LineFind_loop

	TextFunc_LineFind_write:
	FileWrite $R4 $R9
	goto TextFunc_LineFind_loop

	TextFunc_LineFind_preerror:
	SetErrors

	TextFunc_LineFind_handleclose:
	StrCmp $1 '/NUL' +3
	FileClose $R4
	Pop $R4
	FileClose $R5
	IfErrors TextFunc_LineFind_error

	StrCmp $1 '/NUL' TextFunc_LineFind_end
	StrCmp $1 '' 0 +2
	StrCpy $1 $0
	StrCmp $6 0 0 TextFunc_LineFind_rename
	FileOpen $7 $0 r
	FileSeek $7 0 END $8
	FileClose $7
	FileOpen $7 $R4 r
	FileSeek $7 0 END $9
	FileClose $7
	IntCmp $8 $9 0 TextFunc_LineFind_rename
	Delete $R4
	StrCmp $1 $0 TextFunc_LineFind_end
	CopyFiles /SILENT $0 $1
	goto TextFunc_LineFind_end

	TextFunc_LineFind_rename:
	Delete '$EXEDIR\$1'
	Rename $R4 '$EXEDIR\$1'
	IfErrors 0 TextFunc_LineFind_end
	Delete $1
	Rename $R4 $1
	IfErrors 0 TextFunc_LineFind_end

	TextFunc_LineFind_error:
	SetErrors

	TextFunc_LineFind_end:
	Pop $R9
	Pop $R8
	Pop $R7
	Pop $R6
	Pop $R5
	Pop $R4
	Pop $9
	Pop $8
	Pop $7
	Pop $6
	Pop $5
	Pop $4
	Pop $3
	Pop $2
	Pop $1
	Pop $0

	!verbose pop
!macroend

!define LineRead `!insertmacro LineReadCall`
!define un.LineRead `!insertmacro LineReadCall`

!macro LineRead
!macroend

!macro un.LineRead
!macroend

!macro LineRead_
	!verbose push
	!verbose ${_TEXTFUNC_VERBOSE}

	Exch $1
	Exch
	Exch $0
	Exch
	Push $2
	Push $3
	Push $4
	ClearErrors

	IfFileExists $0 0 TextFunc_LineRead_error
	IntOp $1 $1 + 0
	IntCmp $1 0 TextFunc_LineRead_error 0 TextFunc_LineRead_plus
	StrCpy $4 0
	FileOpen $2 $0 r
	IfErrors TextFunc_LineRead_error
	FileRead $2 $3
	IfErrors +3
	IntOp $4 $4 + 1
	Goto -3
	FileClose $2
	IntOp $1 $4 + $1
	IntOp $1 $1 + 1
	IntCmp $1 0 TextFunc_LineRead_error TextFunc_LineRead_error

	TextFunc_LineRead_plus:
	FileOpen $2 $0 r
	IfErrors TextFunc_LineRead_error
	StrCpy $3 0
	IntOp $3 $3 + 1
	FileRead $2 $0
	IfErrors +4
	StrCmp $3 $1 0 -3
	FileClose $2
	goto TextFunc_LineRead_end
	FileClose $2

	TextFunc_LineRead_error:
	SetErrors
	StrCpy $0 ''

	TextFunc_LineRead_end:
	Pop $4
	Pop $3
	Pop $2
	Pop $1
	Exch $0

	!verbose pop
!macroend

!define FileReadFromEnd `!insertmacro FileReadFromEndCall`
!define un.FileReadFromEnd `!insertmacro FileReadFromEndCall`

!macro FileReadFromEnd
!macroend

!macro un.FileReadFromEnd
!macroend

!macro FileReadFromEnd_
	!verbose push
	!verbose ${_TEXTFUNC_VERBOSE}

	Exch $1
	Exch
	Exch $0
	Exch
	Push $7
	Push $8
	Push $9
	ClearErrors

	StrCpy $7 -1
	StrCpy $8 0
	IfFileExists $0 0 TextFunc_FileReadFromEnd_error
	FileOpen $0 $0 r
	IfErrors TextFunc_FileReadFromEnd_error
	FileRead $0 $9
	IfErrors +4
	Push $9
	IntOp $8 $8 + 1
	goto -4
	FileClose $0

	TextFunc_FileReadFromEnd_nextline:
	StrCmp $8 0 TextFunc_FileReadFromEnd_end
	Pop $9
	Push $1
	Push $7
	Push $8
	Call $1
	Pop $0
	Pop $8
	Pop $7
	Pop $1
	IntOp $7 $7 - 1
	IntOp $8 $8 - 1
	IfErrors TextFunc_FileReadFromEnd_error
	StrCmp $0 'StopFileReadFromEnd' TextFunc_FileReadFromEnd_clearstack TextFunc_FileReadFromEnd_nextline

	TextFunc_FileReadFromEnd_error:
	SetErrors

	TextFunc_FileReadFromEnd_clearstack:
	StrCmp $8 0 TextFunc_FileReadFromEnd_end
	Pop $9
	IntOp $8 $8 - 1
	goto TextFunc_FileReadFromEnd_clearstack

	TextFunc_FileReadFromEnd_end:
	Pop $9
	Pop $8
	Pop $7
	Pop $1
	Pop $0

	!verbose pop
!macroend

!define LineSum `!insertmacro LineSumCall`
!define un.LineSum `!insertmacro LineSumCall`

!macro LineSum
!macroend

!macro un.LineSum
!macroend

!macro LineSum_
	!verbose push
	!verbose ${_TEXTFUNC_VERBOSE}

	Exch $0
	Push $1
	Push $2
	ClearErrors

	IfFileExists $0 0 TextFunc_LineSum_error
	StrCpy $2 0
	FileOpen $0 $0 r
	IfErrors TextFunc_LineSum_error
	FileRead $0 $1
	IfErrors +3
	IntOp $2 $2 + 1
	Goto -3
	FileClose $0
	StrCpy $0 $2
	goto TextFunc_LineSum_end

	TextFunc_LineSum_error:
	SetErrors
	StrCpy $0 ''

	TextFunc_LineSum_end:
	Pop $2
	Pop $1
	Exch $0

	!verbose pop
!macroend

!define FileJoin `!insertmacro FileJoinCall`
!define un.FileJoin `!insertmacro FileJoinCall`

!macro FileJoin
!macroend

!macro un.FileJoin
!macroend

!macro FileJoin_
	!verbose push
	!verbose ${_TEXTFUNC_VERBOSE}

	Exch $2
	Exch
	Exch $1
	Exch
	Exch 2
	Exch $0
	Exch 2
	Push $3
	Push $4
	Push $5
	ClearErrors

	IfFileExists $0 0 TextFunc_FileJoin_error
	IfFileExists $1 0 TextFunc_FileJoin_error
	StrCpy $3 0
	IntOp $3 $3 - 1
	StrCpy $4 $2 1 $3
	StrCmp $4 \ +2
	StrCmp $4 '' +3 -3
	StrCpy $4 $2 $3
	IfFileExists '$4\*.*' 0 TextFunc_FileJoin_error

	StrCmp $2 $0 0 +2
	StrCpy $2 ''
	StrCmp $2 '' 0 +3
	StrCpy $4 $0
	Goto TextFunc_FileJoin_notemp
	!insertmacro _TextFunc_TempFileForFile $2 $4
	CopyFiles /SILENT $0 $4
	TextFunc_FileJoin_notemp:
	FileOpen $3 $4 a
	IfErrors TextFunc_FileJoin_error
	FileSeek $3 -1 END
	FileRead $3 $5
	StrCmp $5 '$\r' +3
	StrCmp $5 '$\n' +2
	FileWrite $3 '$\r$\n'

	;FileWrite $3 '$\r$\n--Divider--$\r$\n'

	FileOpen $0 $1 r
	IfErrors TextFunc_FileJoin_error
	FileRead $0 $5
	IfErrors +3
	FileWrite $3 $5
	goto -3
	FileClose $0
	FileClose $3
	StrCmp $2 '' TextFunc_FileJoin_end
	Delete '$EXEDIR\$2'
	Rename $4 '$EXEDIR\$2'
	IfErrors 0 TextFunc_FileJoin_end
	Delete $2
	Rename $4 $2
	IfErrors 0 TextFunc_FileJoin_end

	TextFunc_FileJoin_error:
	SetErrors

	TextFunc_FileJoin_end:
	Pop $5
	Pop $4
	Pop $3
	Pop $2
	Pop $1
	Pop $0

	!verbose pop
!macroend

!macro TextCompareBody _TEXTFUNC_S
	Exch $3
	Exch
	Exch $2
	Exch
	Exch 2
	Exch $1
	Exch 2
	Exch 3
	Exch $0
	Exch 3
	Push $4
	Push $5
	Push $6
	Push $7
	Push $8
	Push $9
	ClearErrors

	IfFileExists $0 0 TextFunc_TextCompare${_TEXTFUNC_S}_error
	IfFileExists $1 0 TextFunc_TextCompare${_TEXTFUNC_S}_error
	StrCmp $2 'FastDiff' +5
	StrCmp $2 'FastEqual' +4
	StrCmp $2 'SlowDiff' +3
	StrCmp $2 'SlowEqual' +2
	goto TextFunc_TextCompare${_TEXTFUNC_S}_error

	FileOpen $4 $0 r
	IfErrors TextFunc_TextCompare${_TEXTFUNC_S}_error
	FileOpen $5 $1 r
	IfErrors TextFunc_TextCompare${_TEXTFUNC_S}_error
	SetDetailsPrint textonly

	StrCpy $6 0
	StrCpy $8 0

	TextFunc_TextCompare${_TEXTFUNC_S}_nextline:
	StrCmp${_TEXTFUNC_S} $4 '' TextFunc_TextCompare${_TEXTFUNC_S}_fast
	IntOp $8 $8 + 1
	FileRead $4 $9
	IfErrors 0 +4
	FileClose $4
	StrCpy $4 ''
	StrCmp${_TEXTFUNC_S} $5 '' TextFunc_TextCompare${_TEXTFUNC_S}_end
	StrCmp $2 'FastDiff' TextFunc_TextCompare${_TEXTFUNC_S}_fast
	StrCmp $2 'FastEqual' TextFunc_TextCompare${_TEXTFUNC_S}_fast TextFunc_TextCompare${_TEXTFUNC_S}_slow

	TextFunc_TextCompare${_TEXTFUNC_S}_fast:
	StrCmp${_TEXTFUNC_S} $5 '' TextFunc_TextCompare${_TEXTFUNC_S}_call
	IntOp $6 $6 + 1
	FileRead $5 $7
	IfErrors 0 +5
	FileClose $5
	StrCpy $5 ''
	StrCmp${_TEXTFUNC_S} $4 '' TextFunc_TextCompare${_TEXTFUNC_S}_end
	StrCmp $2 'FastDiff' TextFunc_TextCompare${_TEXTFUNC_S}_call TextFunc_TextCompare${_TEXTFUNC_S}_close
	StrCmp $2 'FastDiff' 0 +2
	StrCmp${_TEXTFUNC_S} $7 $9 TextFunc_TextCompare${_TEXTFUNC_S}_nextline TextFunc_TextCompare${_TEXTFUNC_S}_call
	StrCmp${_TEXTFUNC_S} $7 $9 TextFunc_TextCompare${_TEXTFUNC_S}_call TextFunc_TextCompare${_TEXTFUNC_S}_nextline

	TextFunc_TextCompare${_TEXTFUNC_S}_slow:
	StrCmp${_TEXTFUNC_S} $4 '' TextFunc_TextCompare${_TEXTFUNC_S}_close
	StrCpy $6 ''
	DetailPrint '$8. $9'
	FileSeek $5 0

	TextFunc_TextCompare${_TEXTFUNC_S}_slownext:
	FileRead $5 $7
	IfErrors 0 +2
	StrCmp $2 'SlowDiff' TextFunc_TextCompare${_TEXTFUNC_S}_call TextFunc_TextCompare${_TEXTFUNC_S}_nextline
	StrCmp $2 'SlowDiff' 0 +2
	StrCmp${_TEXTFUNC_S} $7 $9 TextFunc_TextCompare${_TEXTFUNC_S}_nextline TextFunc_TextCompare${_TEXTFUNC_S}_slownext
	IntOp $6 $6 + 1
	StrCmp${_TEXTFUNC_S} $7 $9 0 TextFunc_TextCompare${_TEXTFUNC_S}_slownext

	TextFunc_TextCompare${_TEXTFUNC_S}_call:
	Push $2
	Push $3
	Push $4
	Push $5
	Push $6
	Push $7
	Push $8
	Push $9
	Call $3
	Pop $0
	Pop $9
	Pop $8
	Pop $7
	Pop $6
	Pop $5
	Pop $4
	Pop $3
	Pop $2
	StrCmp $0 'StopTextCompare' 0 TextFunc_TextCompare${_TEXTFUNC_S}_nextline

	TextFunc_TextCompare${_TEXTFUNC_S}_close:
	FileClose $4
	FileClose $5
	goto TextFunc_TextCompare${_TEXTFUNC_S}_end

	TextFunc_TextCompare${_TEXTFUNC_S}_error:
	SetErrors

	TextFunc_TextCompare${_TEXTFUNC_S}_end:
	SetDetailsPrint both
	Pop $9
	Pop $8
	Pop $7
	Pop $6
	Pop $5
	Pop $4
	Pop $3
	Pop $2
	Pop $1
	Pop $0
!macroend

!define TextCompare `!insertmacro TextCompareCall`
!define un.TextCompare `!insertmacro TextCompareCall`

!macro TextCompare
!macroend

!macro un.TextCompare
!macroend

!macro TextCompare_
	!verbose push
	!verbose ${_TEXTFUNC_VERBOSE}

	!insertmacro TextCompareBody ''

	!verbose pop
!macroend

!define TextCompareS `!insertmacro TextCompareSCall`
!define un.TextCompareS `!insertmacro TextCompareSCall`

!macro TextCompareS
!macroend

!macro un.TextCompareS
!macroend

!macro TextCompareS_
	!verbose push
	!verbose ${_TEXTFUNC_VERBOSE}

	!insertmacro TextCompareBody 'S'

	!verbose pop
!macroend

!macro ConfigReadBody _TEXTFUNC_S
	Exch $1
	Exch
	Exch $0
	Exch
	Push $2
	Push $3
	Push $4
	ClearErrors

	FileOpen $2 $0 r
	IfErrors TextFunc_ConfigRead${_TEXTFUNC_S}_error
	StrLen $0 $1
	StrCmp${_TEXTFUNC_S} $0 0 TextFunc_ConfigRead${_TEXTFUNC_S}_error

	TextFunc_ConfigRead${_TEXTFUNC_S}_readnext:
	FileRead $2 $3
	IfErrors TextFunc_ConfigRead${_TEXTFUNC_S}_error
	StrCpy $4 $3 $0
	StrCmp${_TEXTFUNC_S} $4 $1 0 TextFunc_ConfigRead${_TEXTFUNC_S}_readnext
	StrCpy $0 $3 '' $0
	StrCpy $4 $0 1 -1
	StrCmp${_TEXTFUNC_S} $4 '$\r' +2
	StrCmp${_TEXTFUNC_S} $4 '$\n' 0 TextFunc_ConfigRead${_TEXTFUNC_S}_close
	StrCpy $0 $0 -1
	goto -4

	TextFunc_ConfigRead${_TEXTFUNC_S}_error:
	SetErrors
	StrCpy $0 ''

	TextFunc_ConfigRead${_TEXTFUNC_S}_close:
	FileClose $2

	Pop $4
	Pop $3
	Pop $2
	Pop $1
	Exch $0
!macroend

!define ConfigRead `!insertmacro ConfigReadCall`
!define un.ConfigRead `!insertmacro ConfigReadCall`

!macro ConfigRead
!macroend

!macro un.ConfigRead
!macroend

!macro ConfigRead_
	!verbose push
	!verbose ${_TEXTFUNC_VERBOSE}

	!insertmacro ConfigReadBody ''

	!verbose pop
!macroend

!define ConfigReadS `!insertmacro ConfigReadSCall`
!define un.ConfigReadS `!insertmacro ConfigReadSCall`

!macro ConfigReadS
!macroend

!macro un.ConfigReadS
!macroend

!macro ConfigReadS_
	!verbose push
	!verbose ${_TEXTFUNC_VERBOSE}

	!insertmacro ConfigReadBody 'S'

	!verbose pop
!macroend

!macro ConfigWriteBody _TEXTFUNC_S
	Exch $2
	Exch
	Exch $1
	Exch
	Exch 2
	Exch $0
	Exch 2
	Push $3
	Push $4
	Push $5
	Push $6
	ClearErrors

	IfFileExists $0 0 TextFunc_ConfigWrite${_TEXTFUNC_S}_error
	FileOpen $3 $0 a
	IfErrors TextFunc_ConfigWrite${_TEXTFUNC_S}_error

	StrLen $0 $1
	StrCmp${_TEXTFUNC_S} $0 0 0 TextFunc_ConfigWrite${_TEXTFUNC_S}_readnext
	StrCpy $0 ''
	goto TextFunc_ConfigWrite${_TEXTFUNC_S}_close

	TextFunc_ConfigWrite${_TEXTFUNC_S}_readnext:
	FileRead $3 $4
	IfErrors TextFunc_ConfigWrite${_TEXTFUNC_S}_add
	StrCpy $5 $4 $0
	StrCmp${_TEXTFUNC_S} $5 $1 0 TextFunc_ConfigWrite${_TEXTFUNC_S}_readnext

	StrCpy $5 0
	IntOp $5 $5 - 1
	StrCpy $6 $4 1 $5
	StrCmp${_TEXTFUNC_S} $6 '$\r' -2
	StrCmp${_TEXTFUNC_S} $6 '$\n' -3
	StrCpy $6 $4
	StrCmp${_TEXTFUNC_S} $5 -1 +3
	IntOp $5 $5 + 1
	StrCpy $6 $4 $5

	StrCmp${_TEXTFUNC_S} $2 '' TextFunc_ConfigWrite${_TEXTFUNC_S}_change
	StrCmp${_TEXTFUNC_S} $6 '$1$2' 0 TextFunc_ConfigWrite${_TEXTFUNC_S}_change
	StrCpy $0 SAME
	goto TextFunc_ConfigWrite${_TEXTFUNC_S}_close

	TextFunc_ConfigWrite${_TEXTFUNC_S}_change:
	FileSeek $3 0 CUR $5
	StrLen $4 $4
	IntOp $4 $5 - $4
	FileSeek $3 0 END $6
	IntOp $6 $6 - $5

	System::Alloc $6
	Pop $0
	FileSeek $3 $5 SET
	System::Call 'kernel32::ReadFile(i r3, i r0, i $6, t.,)'
	FileSeek $3 $4 SET
	StrCmp${_TEXTFUNC_S} $2 '' +2
	FileWrite $3 '$1$2$\r$\n'
	System::Call 'kernel32::WriteFile(i r3, i r0, i $6, t.,)'
	System::Call 'kernel32::SetEndOfFile(i r3)'
	System::Free $0
	StrCmp${_TEXTFUNC_S} $2 '' +3
	StrCpy $0 CHANGED
	goto TextFunc_ConfigWrite${_TEXTFUNC_S}_close
	StrCpy $0 DELETED
	goto TextFunc_ConfigWrite${_TEXTFUNC_S}_close

	TextFunc_ConfigWrite${_TEXTFUNC_S}_add:
	StrCmp${_TEXTFUNC_S} $2 '' 0 +3
	StrCpy $0 SAME
	goto TextFunc_ConfigWrite${_TEXTFUNC_S}_close
	FileSeek $3 -1 END
	FileRead $3 $4
	IfErrors +4
	StrCmp${_TEXTFUNC_S} $4 '$\r' +3
	StrCmp${_TEXTFUNC_S} $4 '$\n' +2
	FileWrite $3 '$\r$\n'
	FileWrite $3 '$1$2$\r$\n'
	StrCpy $0 ADDED

	TextFunc_ConfigWrite${_TEXTFUNC_S}_close:
	FileClose $3
	goto TextFunc_ConfigWrite${_TEXTFUNC_S}_end

	TextFunc_ConfigWrite${_TEXTFUNC_S}_error:
	SetErrors
	StrCpy $0 ''

	TextFunc_ConfigWrite${_TEXTFUNC_S}_end:
	Pop $6
	Pop $5
	Pop $4
	Pop $3
	Pop $2
	Pop $1
	Exch $0
!macroend

!define ConfigWrite `!insertmacro ConfigWriteCall`
!define un.ConfigWrite `!insertmacro ConfigWriteCall`

!macro ConfigWrite
!macroend

!macro un.ConfigWrite
!macroend

!macro ConfigWrite_
	!verbose push
	!verbose ${_TEXTFUNC_VERBOSE}

	!insertmacro ConfigWriteBody ''

	!verbose pop
!macroend

!define ConfigWriteS `!insertmacro ConfigWriteSCall`
!define un.ConfigWriteS `!insertmacro ConfigWriteSCall`

!macro ConfigWriteS
!macroend

!macro un.ConfigWriteS
!macroend

!macro ConfigWriteS_
	!verbose push
	!verbose ${_TEXTFUNC_VERBOSE}

	!insertmacro ConfigWriteBody 'S'

	!verbose pop
!macroend

!define FileRecode `!insertmacro FileRecodeCall`
!define un.FileRecode `!insertmacro FileRecodeCall`

!macro FileRecode
!macroend

!macro un.FileRecode
!macroend

!macro FileRecode_
	!verbose push
	!verbose ${_TEXTFUNC_VERBOSE}

	Exch $1
	Exch
	Exch $0
	Exch
	Push $2
	Push $3
	Push $4

	IfFileExists $0 0 TextFunc_FileRecode_error
	StrCmp $1 OemToChar +2
	StrCmp $1 CharToOem 0 TextFunc_FileRecode_error

	FileOpen $2 $0 a
	FileSeek $2 0 END $3
	System::Alloc $3
	Pop $4
	FileSeek $2 0 SET
	System::Call 'kernel32::ReadFile(i r2, i r4, i $3, t.,)'
	System::Call 'user32::$1Buff(i r4, i r4, i $3)'
	FileSeek $2 0 SET
	System::Call 'kernel32::WriteFile(i r2, i r4, i $3, t.,)'
	System::Free $4
	FileClose $2
	goto TextFunc_FileRecode_end

	TextFunc_FileRecode_error:
	SetErrors

	TextFunc_FileRecode_end:
	Pop $4
	Pop $3
	Pop $2
	Pop $1
	Pop $0

	!verbose pop
!macroend

!define TrimNewLines `!insertmacro TrimNewLinesCall`
!define un.TrimNewLines `!insertmacro TrimNewLinesCall`

!macro TrimNewLines
!macroend

!macro un.TrimNewLines
!macroend

!macro TrimNewLines_
	!verbose push
	!verbose ${_TEXTFUNC_VERBOSE}

	Exch $0
	Push $1
	Push $2

	StrCpy $1 0
	IntOp $1 $1 - 1
	StrCpy $2 $0 1 $1
	StrCmp $2 '$\r' -2
	StrCmp $2 '$\n' -3
	StrCmp $1 -1 +3
	IntOp $1 $1 + 1
	StrCpy $0 $0 $1

	Pop $2
	Pop $1
	Exch $0

	!verbose pop
!macroend

!endif
