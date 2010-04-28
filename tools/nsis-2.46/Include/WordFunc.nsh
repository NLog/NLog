/*
_____________________________________________________________________________

                       Word Functions Header v3.3
_____________________________________________________________________________

 2006 Shengalts Aleksander aka Instructor (Shengalts@mail.ru)

 See documentation for more information about the following functions.

 Usage in script:
 1. !include "WordFunc.nsh"
 2. [Section|Function]
      ${WordFunction} "Param1" "Param2" "..." $var
    [SectionEnd|FunctionEnd]


 WordFunction=[WordFind|WordFindS|WordFind2X|WordFind2XS|WordFind3X|WordFind3XS|
               WordReplace|WordReplaceS|WordAdd|WordAddS|WordInsert|WordInsertS|
               StrFilter|StrFilterS|VersionCompare|VersionConvert]

_____________________________________________________________________________

                       Thanks to:
_____________________________________________________________________________

WordFind3X
	Afrow UK (Based on his idea of Function "StrSortLR")
StrFilter
	sunjammer (Function "StrUpper")
VersionCompare
	Afrow UK (Based on his Function "VersionCheckNew2")
VersionConvert
	Afrow UK (Based on his idea of Function "CharIndexReplace")
*/


;_____________________________________________________________________________
;
;                         Macros
;_____________________________________________________________________________
;
; Change log window verbosity (default: 3=no script)
;
; Example:
; !include "WordFunc.nsh"
; !insertmacro WordFind
; ${WORDFUNC_VERBOSE} 4   # all verbosity
; !insertmacro WordReplace
; ${WORDFUNC_VERBOSE} 3   # no script

!ifndef WORDFUNC_INCLUDED
!define WORDFUNC_INCLUDED

!include Util.nsh

!verbose push
!verbose 3
!ifndef _WORDFUNC_VERBOSE
	!define _WORDFUNC_VERBOSE 3
!endif
!verbose ${_WORDFUNC_VERBOSE}
!define WORDFUNC_VERBOSE `!insertmacro WORDFUNC_VERBOSE`
!verbose pop

!macro WORDFUNC_VERBOSE _VERBOSE
	!verbose push
	!verbose 3
	!undef _WORDFUNC_VERBOSE
	!define _WORDFUNC_VERBOSE ${_VERBOSE}
	!verbose pop
!macroend


!macro WordFindCall _ART _STRING _DELIMITER _OPTION _RESULT
	!verbose push
	!verbose ${_WORDFUNC_VERBOSE}
	Push `${_STRING}`
	Push `${_DELIMITER}`
	Push `${_OPTION}`
	${CallArtificialFunction}${_ART} WordFind_
	Pop ${_RESULT}
	!verbose pop
!macroend

!macro WordFindSCall _ART _STRING _DELIMITER _OPTION _RESULT
	!verbose push
	!verbose ${_WORDFUNC_VERBOSE}
	Push `${_STRING}`
	Push `${_DELIMITER}`
	Push `${_OPTION}`
	${CallArtificialFunction}${_ART} WordFindS_
	Pop ${_RESULT}
	!verbose pop
!macroend

!macro WordFind2XCall _STRING _DELIMITER1 _DELIMITER2 _NUMBER _RESULT
	!verbose push
	!verbose ${_WORDFUNC_VERBOSE}
	Push `${_STRING}`
	Push `${_DELIMITER1}`
	Push `${_DELIMITER2}`
	Push `${_NUMBER}`
	${CallArtificialFunction} WordFind2X_
	Pop ${_RESULT}
	!verbose pop
!macroend

!macro WordFind2XSCall _STRING _DELIMITER1 _DELIMITER2 _NUMBER _RESULT
	!verbose push
	!verbose ${_WORDFUNC_VERBOSE}
	Push `${_STRING}`
	Push `${_DELIMITER1}`
	Push `${_DELIMITER2}`
	Push `${_NUMBER}`
	${CallArtificialFunction} WordFind2XS_
	Pop ${_RESULT}
	!verbose pop
!macroend

!macro WordFind3XCall _STRING _DELIMITER1 _CENTER _DELIMITER2 _NUMBER _RESULT
	!verbose push
	!verbose ${_WORDFUNC_VERBOSE}
	Push `${_STRING}`
	Push `${_DELIMITER1}`
	Push `${_CENTER}`
	Push `${_DELIMITER2}`
	Push `${_NUMBER}`
	${CallArtificialFunction} WordFind3X_
	Pop ${_RESULT}
	!verbose pop
!macroend

!macro WordFind3XSCall _STRING _DELIMITER1 _CENTER _DELIMITER2 _NUMBER _RESULT
	!verbose push
	!verbose ${_WORDFUNC_VERBOSE}
	Push `${_STRING}`
	Push `${_DELIMITER1}`
	Push `${_CENTER}`
	Push `${_DELIMITER2}`
	Push `${_NUMBER}`
	${CallArtificialFunction} WordFind3XS_
	Pop ${_RESULT}
	!verbose pop
!macroend

!macro WordReplaceCall _STRING _WORD1 _WORD2 _NUMBER _RESULT
	!verbose push
	!verbose ${_WORDFUNC_VERBOSE}
	Push `${_STRING}`
	Push `${_WORD1}`
	Push `${_WORD2}`
	Push `${_NUMBER}`
	${CallArtificialFunction} WordReplace_
	Pop ${_RESULT}
	!verbose pop
!macroend

!macro WordReplaceSCall _STRING _WORD1 _WORD2 _NUMBER _RESULT
	!verbose push
	!verbose ${_WORDFUNC_VERBOSE}
	Push `${_STRING}`
	Push `${_WORD1}`
	Push `${_WORD2}`
	Push `${_NUMBER}`
	${CallArtificialFunction} WordReplaceS_
	Pop ${_RESULT}
	!verbose pop
!macroend

!macro WordAddCall _STRING1 _DELIMITER _STRING2 _RESULT
	!verbose push
	!verbose ${_WORDFUNC_VERBOSE}
	Push `${_STRING1}`
	Push `${_DELIMITER}`
	Push `${_STRING2}`
	${CallArtificialFunction} WordAdd_
	Pop ${_RESULT}
	!verbose pop
!macroend

!macro WordAddSCall _STRING1 _DELIMITER _STRING2 _RESULT
	!verbose push
	!verbose ${_WORDFUNC_VERBOSE}
	Push `${_STRING1}`
	Push `${_DELIMITER}`
	Push `${_STRING2}`
	${CallArtificialFunction} WordAddS_
	Pop ${_RESULT}
	!verbose pop
!macroend

!macro WordInsertCall _STRING _DELIMITER _WORD _NUMBER _RESULT
	!verbose push
	!verbose ${_WORDFUNC_VERBOSE}
	Push `${_STRING}`
	Push `${_DELIMITER}`
	Push `${_WORD}`
	Push `${_NUMBER}`
	${CallArtificialFunction} WordInsert_
	Pop ${_RESULT}
	!verbose pop
!macroend

!macro WordInsertSCall _STRING _DELIMITER _WORD _NUMBER _RESULT
	!verbose push
	!verbose ${_WORDFUNC_VERBOSE}
	Push `${_STRING}`
	Push `${_DELIMITER}`
	Push `${_WORD}`
	Push `${_NUMBER}`
	${CallArtificialFunction} WordInsertS_
	Pop ${_RESULT}
	!verbose pop
!macroend

!macro StrFilterCall _STRING _FILTER _INCLUDE _EXCLUDE _RESULT
	!verbose push
	!verbose ${_WORDFUNC_VERBOSE}
	Push `${_STRING}`
	Push `${_FILTER}`
	Push `${_INCLUDE}`
	Push `${_EXCLUDE}`
	${CallArtificialFunction} StrFilter_
	Pop ${_RESULT}
	!verbose pop
!macroend

!macro StrFilterSCall _STRING _FILTER _INCLUDE _EXCLUDE _RESULT
	!verbose push
	!verbose ${_WORDFUNC_VERBOSE}
	Push `${_STRING}`
	Push `${_FILTER}`
	Push `${_INCLUDE}`
	Push `${_EXCLUDE}`
	${CallArtificialFunction} StrFilterS_
	Pop ${_RESULT}
	!verbose pop
!macroend

!macro VersionCompareCall _VER1 _VER2 _RESULT
	!verbose push
	!verbose ${_WORDFUNC_VERBOSE}
	Push `${_VER1}`
	Push `${_VER2}`
	${CallArtificialFunction} VersionCompare_
	Pop ${_RESULT}
	!verbose pop
!macroend

!macro VersionConvertCall _VERSION _CHARLIST _RESULT
	!verbose push
	!verbose ${_WORDFUNC_VERBOSE}
	Push `${_VERSION}`
	Push `${_CHARLIST}`
	${CallArtificialFunction} VersionConvert_
	Pop ${_RESULT}
	!verbose pop
!macroend

!macro WordFindBody _WORDFUNC_S
	Exch $1
	Exch
	Exch $0
	Exch
	Exch 2
	Exch $R0
	Exch 2
	Push $2
	Push $3
	Push $4
	Push $5
	Push $6
	Push $7
	Push $8
	Push $9
	Push $R1
	Push $R2
	ClearErrors

	StrCpy $9 ''
	StrCpy $2 $1 1
	StrCpy $1 $1 '' 1
	StrCmp $2 'E' 0 +3
	StrCpy $9 E
	goto -4

	StrCpy $3 ''
	StrCmp${_WORDFUNC_S} $2 '+' +6
	StrCmp${_WORDFUNC_S} $2 '-' +5
	StrCmp${_WORDFUNC_S} $2 '/' WordFunc_WordFind${_WORDFUNC_S}_restart
	StrCmp${_WORDFUNC_S} $2 '#' WordFunc_WordFind${_WORDFUNC_S}_restart
	StrCmp${_WORDFUNC_S} $2 '*' WordFunc_WordFind${_WORDFUNC_S}_restart
	goto WordFunc_WordFind${_WORDFUNC_S}_error3

	StrCpy $4 $1 1 -1
	StrCmp${_WORDFUNC_S} $4 '*' +4
	StrCmp${_WORDFUNC_S} $4 '}' +3
	StrCmp${_WORDFUNC_S} $4 '{' +2
	goto +4
	StrCpy $1 $1 -1
	StrCpy $3 '$4$3'
	goto -7
	StrCmp${_WORDFUNC_S} $3 '*' WordFunc_WordFind${_WORDFUNC_S}_error3
	StrCmp${_WORDFUNC_S} $3 '**' WordFunc_WordFind${_WORDFUNC_S}_error3
	StrCmp${_WORDFUNC_S} $3 '}{' WordFunc_WordFind${_WORDFUNC_S}_error3
	IntOp $1 $1 + 0
	StrCmp${_WORDFUNC_S} $1 0 WordFunc_WordFind${_WORDFUNC_S}_error2

	WordFunc_WordFind${_WORDFUNC_S}_restart:
	StrCmp${_WORDFUNC_S} $R0 '' WordFunc_WordFind${_WORDFUNC_S}_error1
	StrCpy $4 0
	StrCpy $5 0
	StrCpy $6 0
	StrLen $7 $0
	goto WordFunc_WordFind${_WORDFUNC_S}_loop

	WordFunc_WordFind${_WORDFUNC_S}_preloop:
	IntOp $6 $6 + 1

	WordFunc_WordFind${_WORDFUNC_S}_loop:
	StrCpy $8 $R0 $7 $6
	StrCmp${_WORDFUNC_S} $8$5 0 WordFunc_WordFind${_WORDFUNC_S}_error1
	StrLen $R2 $8
	IntCmp $R2 0 +2
	StrCmp${_WORDFUNC_S} $8 $0 +5 WordFunc_WordFind${_WORDFUNC_S}_preloop
	StrCmp${_WORDFUNC_S} $3 '{' WordFunc_WordFind${_WORDFUNC_S}_minus
	StrCmp${_WORDFUNC_S} $3 '}' WordFunc_WordFind${_WORDFUNC_S}_minus
	StrCmp${_WORDFUNC_S} $2 '*' WordFunc_WordFind${_WORDFUNC_S}_minus
	StrCmp${_WORDFUNC_S} $5 $6 WordFunc_WordFind${_WORDFUNC_S}_minus +5
	StrCmp${_WORDFUNC_S} $3 '{' +4
	StrCmp${_WORDFUNC_S} $3 '}' +3
	StrCmp${_WORDFUNC_S} $2 '*' +2
	StrCmp${_WORDFUNC_S} $5 $6 WordFunc_WordFind${_WORDFUNC_S}_nextword
	IntOp $4 $4 + 1
	StrCmp${_WORDFUNC_S} $2$4 +$1 WordFunc_WordFind${_WORDFUNC_S}_plus
	StrCmp${_WORDFUNC_S} $2 '/' 0 WordFunc_WordFind${_WORDFUNC_S}_nextword
	IntOp $8 $6 - $5
	StrCpy $8 $R0 $8 $5
	StrCmp${_WORDFUNC_S} $1 $8 0 WordFunc_WordFind${_WORDFUNC_S}_nextword
	StrCpy $R1 $4
	goto WordFunc_WordFind${_WORDFUNC_S}_end
	WordFunc_WordFind${_WORDFUNC_S}_nextword:
	IntOp $6 $6 + $7
	StrCpy $5 $6
	goto WordFunc_WordFind${_WORDFUNC_S}_loop

	WordFunc_WordFind${_WORDFUNC_S}_minus:
	StrCmp${_WORDFUNC_S} $2 '-' 0 WordFunc_WordFind${_WORDFUNC_S}_sum
	StrCpy $2 '+'
	IntOp $1 $4 - $1
	IntOp $1 $1 + 1
	IntCmp $1 0 WordFunc_WordFind${_WORDFUNC_S}_error2 WordFunc_WordFind${_WORDFUNC_S}_error2 WordFunc_WordFind${_WORDFUNC_S}_restart
	WordFunc_WordFind${_WORDFUNC_S}_sum:
	StrCmp${_WORDFUNC_S} $2 '#' 0 WordFunc_WordFind${_WORDFUNC_S}_sumdelim
	StrCpy $R1 $4
	goto WordFunc_WordFind${_WORDFUNC_S}_end
	WordFunc_WordFind${_WORDFUNC_S}_sumdelim:
	StrCmp${_WORDFUNC_S} $2 '*' 0 WordFunc_WordFind${_WORDFUNC_S}_error2
	StrCpy $R1 $4
	goto WordFunc_WordFind${_WORDFUNC_S}_end

	WordFunc_WordFind${_WORDFUNC_S}_plus:
	StrCmp${_WORDFUNC_S} $3 '' 0 +4
	IntOp $6 $6 - $5
	StrCpy $R1 $R0 $6 $5
	goto WordFunc_WordFind${_WORDFUNC_S}_end
	StrCmp${_WORDFUNC_S} $3 '{' 0 +3
	StrCpy $R1 $R0 $6
	goto WordFunc_WordFind${_WORDFUNC_S}_end
	StrCmp${_WORDFUNC_S} $3 '}' 0 +4
	IntOp $6 $6 + $7
	StrCpy $R1 $R0 '' $6
	goto WordFunc_WordFind${_WORDFUNC_S}_end
	StrCmp${_WORDFUNC_S} $3 '{*' +2
	StrCmp${_WORDFUNC_S} $3 '*{' 0 +3
	StrCpy $R1 $R0 $6
	goto WordFunc_WordFind${_WORDFUNC_S}_end
	StrCmp${_WORDFUNC_S} $3 '*}' +2
	StrCmp${_WORDFUNC_S} $3 '}*' 0 +3
	StrCpy $R1 $R0 '' $5
	goto WordFunc_WordFind${_WORDFUNC_S}_end
	StrCmp${_WORDFUNC_S} $3 '}}' 0 +3
	StrCpy $R1 $R0 '' $6
	goto WordFunc_WordFind${_WORDFUNC_S}_end
	StrCmp${_WORDFUNC_S} $3 '{{' 0 +3
	StrCpy $R1 $R0 $5
	goto WordFunc_WordFind${_WORDFUNC_S}_end
	StrCmp${_WORDFUNC_S} $3 '{}' 0 WordFunc_WordFind${_WORDFUNC_S}_error3
	StrLen $3 $R0
	StrCmp${_WORDFUNC_S} $3 $6 0 +3
	StrCpy $0 ''
	goto +2
	IntOp $6 $6 + $7
	StrCpy $8 $R0 '' $6
	StrCmp${_WORDFUNC_S} $4$8 1 +6
	StrCmp${_WORDFUNC_S} $4 1 +2 +7
	IntOp $6 $6 + $7
	StrCpy $3 $R0 $7 $6
	StrCmp${_WORDFUNC_S} $3 '' +2
	StrCmp${_WORDFUNC_S} $3 $0 -3 +3
	StrCpy $R1 ''
	goto WordFunc_WordFind${_WORDFUNC_S}_end
	StrCmp${_WORDFUNC_S} $5 0 0 +3
	StrCpy $0 ''
	goto +2
	IntOp $5 $5 - $7
	StrCpy $3 $R0 $5
	StrCpy $R1 '$3$0$8'
	goto WordFunc_WordFind${_WORDFUNC_S}_end

	WordFunc_WordFind${_WORDFUNC_S}_error3:
	StrCpy $R1 3
	goto WordFunc_WordFind${_WORDFUNC_S}_error
	WordFunc_WordFind${_WORDFUNC_S}_error2:
	StrCpy $R1 2
	goto WordFunc_WordFind${_WORDFUNC_S}_error
	WordFunc_WordFind${_WORDFUNC_S}_error1:
	StrCpy $R1 1
	WordFunc_WordFind${_WORDFUNC_S}_error:
	StrCmp $9 'E' 0 +3
	SetErrors

	WordFunc_WordFind${_WORDFUNC_S}_end:
	StrCpy $R0 $R1

	Pop $R2
	Pop $R1
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
	Exch $R0
!macroend

!define WordFind `!insertmacro WordFindCall ''`
!define un.WordFind `!insertmacro WordFindCall ''`

!macro WordFind
!macroend

!macro un.WordFind
!macroend

!macro WordFind_
	!verbose push
	!verbose ${_WORDFUNC_VERBOSE}

	!insertmacro WordFindBody ''

	!verbose pop
!macroend

!define WordFindS `!insertmacro WordFindSCall ''`
!define un.WordFindS `!insertmacro WordFindSCall ''`

!macro WordFindS
!macroend

!macro un.WordFindS
!macroend

!macro WordFindS_
	!verbose push
	!verbose ${_WORDFUNC_VERBOSE}

	!insertmacro WordFindBody 'S'

	!verbose pop
!macroend

!macro WordFind2XBody _WORDFUNC_S
	Exch $2
	Exch
	Exch $1
	Exch
	Exch 2
	Exch $0
	Exch 2
	Exch 3
	Exch $R0
	Exch 3
	Push $3
	Push $4
	Push $5
	Push $6
	Push $7
	Push $8
	Push $9
	Push $R1
	Push $R2
	ClearErrors

	StrCpy $R2 ''
	StrCpy $3 $2 1
	StrCpy $2 $2 '' 1
	StrCmp $3 'E' 0 +3
	StrCpy $R2 E
	goto -4

	StrCmp${_WORDFUNC_S} $3 '+' +5
	StrCmp${_WORDFUNC_S} $3 '-' +4
	StrCmp${_WORDFUNC_S} $3 '#' WordFunc_WordFind2X${_WORDFUNC_S}_restart
	StrCmp${_WORDFUNC_S} $3 '/' WordFunc_WordFind2X${_WORDFUNC_S}_restart
	goto WordFunc_WordFind2X${_WORDFUNC_S}_error3

	StrCpy $4 $2 2 -2
	StrCmp${_WORDFUNC_S} $4 '{{' +9
	StrCmp${_WORDFUNC_S} $4 '}}' +8
	StrCmp${_WORDFUNC_S} $4 '{*' +7
	StrCmp${_WORDFUNC_S} $4 '*{' +6
	StrCmp${_WORDFUNC_S} $4 '*}' +5
	StrCmp${_WORDFUNC_S} $4 '}*' +4
	StrCmp${_WORDFUNC_S} $4 '{}' +3
	StrCpy $4 ''
	goto +2
	StrCpy $2 $2 -2
	IntOp $2 $2 + 0
	StrCmp${_WORDFUNC_S} $2 0 WordFunc_WordFind2X${_WORDFUNC_S}_error2

	WordFunc_WordFind2X${_WORDFUNC_S}_restart:
	StrCmp${_WORDFUNC_S} $R0 '' WordFunc_WordFind2X${_WORDFUNC_S}_error1
	StrCpy $5 -1
	StrCpy $6 0
	StrCpy $7 ''
	StrLen $8 $0
	StrLen $9 $1

	WordFunc_WordFind2X${_WORDFUNC_S}_loop:
	IntOp $5 $5 + 1

	WordFunc_WordFind2X${_WORDFUNC_S}_delim1:
	StrCpy $R1 $R0 $8 $5
	StrCmp${_WORDFUNC_S} $R1$6 0 WordFunc_WordFind2X${_WORDFUNC_S}_error1
	StrCmp${_WORDFUNC_S} $R1 '' WordFunc_WordFind2X${_WORDFUNC_S}_minus
	StrCmp${_WORDFUNC_S} $R1 $0 +2
	StrCmp${_WORDFUNC_S} $7 '' WordFunc_WordFind2X${_WORDFUNC_S}_loop WordFunc_WordFind2X${_WORDFUNC_S}_delim2
	StrCmp${_WORDFUNC_S} $0 $1 0 +2
	StrCmp${_WORDFUNC_S} $7 '' 0 WordFunc_WordFind2X${_WORDFUNC_S}_delim2
	IntOp $7 $5 + $8
	StrCpy $5 $7
	goto WordFunc_WordFind2X${_WORDFUNC_S}_delim1

	WordFunc_WordFind2X${_WORDFUNC_S}_delim2:
	StrCpy $R1 $R0 $9 $5
	StrCmp${_WORDFUNC_S} $R1 $1 0 WordFunc_WordFind2X${_WORDFUNC_S}_loop
	IntOp $6 $6 + 1
	StrCmp${_WORDFUNC_S} $3$6 '+$2' WordFunc_WordFind2X${_WORDFUNC_S}_plus
	StrCmp${_WORDFUNC_S} $3 '/' 0 WordFunc_WordFind2X${_WORDFUNC_S}_nextword
	IntOp $R1 $5 - $7
	StrCpy $R1 $R0 $R1 $7
	StrCmp${_WORDFUNC_S} $R1 $2 0 +3
	StrCpy $R1 $6
	goto WordFunc_WordFind2X${_WORDFUNC_S}_end
	WordFunc_WordFind2X${_WORDFUNC_S}_nextword:
	IntOp $5 $5 + $9
	StrCpy $7 ''
	goto WordFunc_WordFind2X${_WORDFUNC_S}_delim1

	WordFunc_WordFind2X${_WORDFUNC_S}_minus:
	StrCmp${_WORDFUNC_S} $3 '-' 0 WordFunc_WordFind2X${_WORDFUNC_S}_sum
	StrCpy $3 +
	IntOp $2 $6 - $2
	IntOp $2 $2 + 1
	IntCmp $2 0 WordFunc_WordFind2X${_WORDFUNC_S}_error2 WordFunc_WordFind2X${_WORDFUNC_S}_error2 WordFunc_WordFind2X${_WORDFUNC_S}_restart
	WordFunc_WordFind2X${_WORDFUNC_S}_sum:
	StrCmp${_WORDFUNC_S} $3 '#' 0 WordFunc_WordFind2X${_WORDFUNC_S}_error2
	StrCpy $R1 $6
	goto WordFunc_WordFind2X${_WORDFUNC_S}_end

	WordFunc_WordFind2X${_WORDFUNC_S}_plus:
	StrCmp${_WORDFUNC_S} $4 '' 0 +4
	IntOp $R1 $5 - $7
	StrCpy $R1 $R0 $R1 $7
	goto WordFunc_WordFind2X${_WORDFUNC_S}_end
	IntOp $5 $5 + $9
	IntOp $7 $7 - $8
	StrCmp${_WORDFUNC_S} $4 '{*' +2
	StrCmp${_WORDFUNC_S} $4 '*{' 0 +3
	StrCpy $R1 $R0 $5
	goto WordFunc_WordFind2X${_WORDFUNC_S}_end
	StrCmp${_WORDFUNC_S} $4 '*}' +2
	StrCmp${_WORDFUNC_S} $4 '}*' 0 +3
	StrCpy $R1 $R0 '' $7
	goto WordFunc_WordFind2X${_WORDFUNC_S}_end
	StrCmp${_WORDFUNC_S} $4 '}}' 0 +3
	StrCpy $R1 $R0 '' $5
	goto WordFunc_WordFind2X${_WORDFUNC_S}_end
	StrCmp${_WORDFUNC_S} $4 '{{' 0 +3
	StrCpy $R1 $R0 $7
	goto WordFunc_WordFind2X${_WORDFUNC_S}_end
	StrCmp${_WORDFUNC_S} $4 '{}' 0 WordFunc_WordFind2X${_WORDFUNC_S}_error3
	StrCpy $5 $R0 '' $5
	StrCpy $7 $R0 $7
	StrCpy $R1 '$7$5'
	goto WordFunc_WordFind2X${_WORDFUNC_S}_end

	WordFunc_WordFind2X${_WORDFUNC_S}_error3:
	StrCpy $R1 3
	goto WordFunc_WordFind2X${_WORDFUNC_S}_error
	WordFunc_WordFind2X${_WORDFUNC_S}_error2:
	StrCpy $R1 2
	goto WordFunc_WordFind2X${_WORDFUNC_S}_error
	WordFunc_WordFind2X${_WORDFUNC_S}_error1:
	StrCpy $R1 1
	WordFunc_WordFind2X${_WORDFUNC_S}_error:
	StrCmp $R2 'E' 0 +3
	SetErrors

	WordFunc_WordFind2X${_WORDFUNC_S}_end:
	StrCpy $R0 $R1

	Pop $R2
	Pop $R1
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
	Exch $R0
!macroend

!define WordFind2X `!insertmacro WordFind2XCall`
!define un.WordFind2X `!insertmacro WordFind2XCall`

!macro WordFind2X
!macroend

!macro un.WordFind2X
!macroend

!macro WordFind2X_
	!verbose push
	!verbose ${_WORDFUNC_VERBOSE}

	!insertmacro WordFind2XBody ''

	!verbose pop
!macroend

!define WordFind2XS `!insertmacro WordFind2XSCall`
!define un.WordFind2XS `!insertmacro WordFind2XSCall`

!macro WordFind2XS
!macroend

!macro un.WordFind2XS
!macroend

!macro WordFind2XS_
	!verbose push
	!verbose ${_WORDFUNC_VERBOSE}

	!insertmacro WordFind2XBody 'S'

	!verbose pop
!macroend

!macro WordFind3XBody _WORDFUNC_S
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
	Exch 4
	Exch $R0
	Exch 4
	Push $4
	Push $5
	Push $6
	Push $7
	Push $8
	Push $9
	Push $R1
	Push $R2
	Push $R3
	Push $R4
	Push $R5
	ClearErrors

	StrCpy $R5 ''
	StrCpy $4 $3 1
	StrCpy $3 $3 '' 1
	StrCmp $4 'E' 0 +3
	StrCpy $R5 E
	goto -4

	StrCmp${_WORDFUNC_S} $4 '+' +5
	StrCmp${_WORDFUNC_S} $4 '-' +4
	StrCmp${_WORDFUNC_S} $4 '#' WordFunc_WordFind3X${_WORDFUNC_S}_restart
	StrCmp${_WORDFUNC_S} $4 '/' WordFunc_WordFind3X${_WORDFUNC_S}_restart
	goto WordFunc_WordFind3X${_WORDFUNC_S}_error3

	StrCpy $5 $3 2 -2
	StrCmp${_WORDFUNC_S} $5 '{{' +9
	StrCmp${_WORDFUNC_S} $5 '}}' +8
	StrCmp${_WORDFUNC_S} $5 '{*' +7
	StrCmp${_WORDFUNC_S} $5 '*{' +6
	StrCmp${_WORDFUNC_S} $5 '*}' +5
	StrCmp${_WORDFUNC_S} $5 '}*' +4
	StrCmp${_WORDFUNC_S} $5 '{}' +3
	StrCpy $5 ''
	goto +2
	StrCpy $3 $3 -2
	IntOp $3 $3 + 0
	StrCmp${_WORDFUNC_S} $3 0 WordFunc_WordFind3X${_WORDFUNC_S}_error2

	WordFunc_WordFind3X${_WORDFUNC_S}_restart:
	StrCmp${_WORDFUNC_S} $R0 '' WordFunc_WordFind3X${_WORDFUNC_S}_error1
	StrCpy $6 -1
	StrCpy $7 0
	StrCpy $8 ''
	StrCpy $9 ''
	StrLen $R1 $0
	StrLen $R2 $1
	StrLen $R3 $2

	WordFunc_WordFind3X${_WORDFUNC_S}_loop:
	IntOp $6 $6 + 1

	WordFunc_WordFind3X${_WORDFUNC_S}_delim1:
	StrCpy $R4 $R0 $R1 $6
	StrCmp${_WORDFUNC_S} $R4$7 0 WordFunc_WordFind3X${_WORDFUNC_S}_error1
	StrCmp${_WORDFUNC_S} $R4 '' WordFunc_WordFind3X${_WORDFUNC_S}_minus
	StrCmp${_WORDFUNC_S} $R4 $0 +2
	StrCmp${_WORDFUNC_S} $8 '' WordFunc_WordFind3X${_WORDFUNC_S}_loop WordFunc_WordFind3X${_WORDFUNC_S}_center
	StrCmp${_WORDFUNC_S} $0 $1 +2
	StrCmp${_WORDFUNC_S} $0 $2 0 +2
	StrCmp${_WORDFUNC_S} $8 '' 0 WordFunc_WordFind3X${_WORDFUNC_S}_center
	IntOp $8 $6 + $R1
	StrCpy $6 $8
	goto WordFunc_WordFind3X${_WORDFUNC_S}_delim1

	WordFunc_WordFind3X${_WORDFUNC_S}_center:
	StrCmp${_WORDFUNC_S} $9 '' 0 WordFunc_WordFind3X${_WORDFUNC_S}_delim2
	StrCpy $R4 $R0 $R2 $6
	StrCmp${_WORDFUNC_S} $R4 $1 0 WordFunc_WordFind3X${_WORDFUNC_S}_loop
	IntOp $9 $6 + $R2
	StrCpy $6 $9
	goto WordFunc_WordFind3X${_WORDFUNC_S}_delim1

	WordFunc_WordFind3X${_WORDFUNC_S}_delim2:
	StrCpy $R4 $R0 $R3 $6
	StrCmp${_WORDFUNC_S} $R4 $2 0 WordFunc_WordFind3X${_WORDFUNC_S}_loop
	IntOp $7 $7 + 1
	StrCmp${_WORDFUNC_S} $4$7 '+$3' WordFunc_WordFind3X${_WORDFUNC_S}_plus
	StrCmp${_WORDFUNC_S} $4 '/' 0 WordFunc_WordFind3X${_WORDFUNC_S}_nextword
	IntOp $R4 $6 - $8
	StrCpy $R4 $R0 $R4 $8
	StrCmp${_WORDFUNC_S} $R4 $3 0 +3
	StrCpy $R4 $7
	goto WordFunc_WordFind3X${_WORDFUNC_S}_end
	WordFunc_WordFind3X${_WORDFUNC_S}_nextword:
	IntOp $6 $6 + $R3
	StrCpy $8 ''
	StrCpy $9 ''
	goto WordFunc_WordFind3X${_WORDFUNC_S}_delim1

	WordFunc_WordFind3X${_WORDFUNC_S}_minus:
	StrCmp${_WORDFUNC_S} $4 '-' 0 WordFunc_WordFind3X${_WORDFUNC_S}_sum
	StrCpy $4 +
	IntOp $3 $7 - $3
	IntOp $3 $3 + 1
	IntCmp $3 0 WordFunc_WordFind3X${_WORDFUNC_S}_error2 WordFunc_WordFind3X${_WORDFUNC_S}_error2 WordFunc_WordFind3X${_WORDFUNC_S}_restart
	WordFunc_WordFind3X${_WORDFUNC_S}_sum:
	StrCmp${_WORDFUNC_S} $4 '#' 0 WordFunc_WordFind3X${_WORDFUNC_S}_error2
	StrCpy $R4 $7
	goto WordFunc_WordFind3X${_WORDFUNC_S}_end

	WordFunc_WordFind3X${_WORDFUNC_S}_plus:
	StrCmp${_WORDFUNC_S} $5 '' 0 +4
	IntOp $R4 $6 - $8
	StrCpy $R4 $R0 $R4 $8
	goto WordFunc_WordFind3X${_WORDFUNC_S}_end
	IntOp $6 $6 + $R3
	IntOp $8 $8 - $R1
	StrCmp${_WORDFUNC_S} $5 '{*' +2
	StrCmp${_WORDFUNC_S} $5 '*{' 0 +3
	StrCpy $R4 $R0 $6
	goto WordFunc_WordFind3X${_WORDFUNC_S}_end
	StrCmp${_WORDFUNC_S} $5 '*}' +2
	StrCmp${_WORDFUNC_S} $5 '}*' 0 +3
	StrCpy $R4 $R0 '' $8
	goto WordFunc_WordFind3X${_WORDFUNC_S}_end
	StrCmp${_WORDFUNC_S} $5 '}}' 0 +3
	StrCpy $R4 $R0 '' $6
	goto WordFunc_WordFind3X${_WORDFUNC_S}_end
	StrCmp${_WORDFUNC_S} $5 '{{' 0 +3
	StrCpy $R4 $R0 $8
	goto WordFunc_WordFind3X${_WORDFUNC_S}_end
	StrCmp${_WORDFUNC_S} $5 '{}' 0 WordFunc_WordFind3X${_WORDFUNC_S}_error3
	StrCpy $6 $R0 '' $6
	StrCpy $8 $R0 $8
	StrCpy $R4 '$8$6'
	goto WordFunc_WordFind3X${_WORDFUNC_S}_end

	WordFunc_WordFind3X${_WORDFUNC_S}_error3:
	StrCpy $R4 3
	goto WordFunc_WordFind3X${_WORDFUNC_S}_error
	WordFunc_WordFind3X${_WORDFUNC_S}_error2:
	StrCpy $R4 2
	goto WordFunc_WordFind3X${_WORDFUNC_S}_error
	WordFunc_WordFind3X${_WORDFUNC_S}_error1:
	StrCpy $R4 1
	WordFunc_WordFind3X${_WORDFUNC_S}_error:
	StrCmp $R5 'E' 0 +3
	SetErrors

	WordFunc_WordFind3X${_WORDFUNC_S}_end:
	StrCpy $R0 $R4
	Pop $R5
	Pop $R4
	Pop $R3
	Pop $R2
	Pop $R1
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
	Exch $R0
!macroend

!define WordFind3X `!insertmacro WordFind3XCall`
!define un.WordFind3X `!insertmacro WordFind3XCall`

!macro WordFind3X
!macroend

!macro un.WordFind3X
!macroend

!macro WordFind3X_
	!verbose push
	!verbose ${_WORDFUNC_VERBOSE}

	!insertmacro WordFind3XBody ''

	!verbose pop
!macroend

!define WordFind3XS `!insertmacro WordFind3XSCall`
!define un.WordFind3XS `!insertmacro WordFind3XSCall`

!macro WordFind3XS
!macroend

!macro un.WordFind3XS
!macroend

!macro WordFind3XS_
	!verbose push
	!verbose ${_WORDFUNC_VERBOSE}

	!insertmacro WordFind3XBody 'S'

	!verbose pop
!macroend

!macro WordReplaceBody _WORDFUNC_S
	Exch $2
	Exch
	Exch $1
	Exch
	Exch 2
	Exch $0
	Exch 2
	Exch 3
	Exch $R0
	Exch 3
	Push $3
	Push $4
	Push $5
	Push $6
	Push $7
	Push $8
	Push $9
	Push $R1
	ClearErrors

	StrCpy $R1 $R0
	StrCpy $9 ''
	StrCpy $3 $2 1
	StrCpy $2 $2 '' 1
	StrCmp $3 'E' 0 +3
	StrCpy $9 E
	goto -4

	StrCpy $4 $2 1 -1
	StrCpy $5 ''
	StrCpy $6 ''
	StrLen $7 $0

	StrCmp${_WORDFUNC_S} $7 0 WordFunc_WordReplace${_WORDFUNC_S}_error1
	StrCmp${_WORDFUNC_S} $R0 '' WordFunc_WordReplace${_WORDFUNC_S}_error1
	StrCmp${_WORDFUNC_S} $3 '{' WordFunc_WordReplace${_WORDFUNC_S}_beginning
	StrCmp${_WORDFUNC_S} $3 '}' WordFunc_WordReplace${_WORDFUNC_S}_ending WordFunc_WordReplace${_WORDFUNC_S}_errorchk

	WordFunc_WordReplace${_WORDFUNC_S}_beginning:
	StrCpy $8 $R0 $7
	StrCmp${_WORDFUNC_S} $8 $0 0 +4
	StrCpy $R0 $R0 '' $7
	StrCpy $5 '$5$1'
	goto -4
	StrCpy $3 $2 1
	StrCmp${_WORDFUNC_S} $3 '}' 0 WordFunc_WordReplace${_WORDFUNC_S}_merge

	WordFunc_WordReplace${_WORDFUNC_S}_ending:
	StrCpy $8 $R0 '' -$7
	StrCmp${_WORDFUNC_S} $8 $0 0 +4
	StrCpy $R0 $R0 -$7
	StrCpy $6 '$6$1'
	goto -4

	WordFunc_WordReplace${_WORDFUNC_S}_merge:
	StrCmp${_WORDFUNC_S} $4 '*' 0 +5
	StrCmp${_WORDFUNC_S} $5 '' +2
	StrCpy $5 $1
	StrCmp${_WORDFUNC_S} $6 '' +2
	StrCpy $6 $1
	StrCpy $R0 '$5$R0$6'
	goto WordFunc_WordReplace${_WORDFUNC_S}_end

	WordFunc_WordReplace${_WORDFUNC_S}_errorchk:
	StrCmp${_WORDFUNC_S} $3 '+' +2
	StrCmp${_WORDFUNC_S} $3 '-' 0 WordFunc_WordReplace${_WORDFUNC_S}_error3

	StrCpy $5 $2 1
	IntOp $2 $2 + 0
	StrCmp${_WORDFUNC_S} $2 0 0 WordFunc_WordReplace${_WORDFUNC_S}_one
	StrCmp${_WORDFUNC_S} $5 0 WordFunc_WordReplace${_WORDFUNC_S}_error2
	StrCpy $3 ''

	WordFunc_WordReplace${_WORDFUNC_S}_all:
	StrCpy $5 0
	StrCpy $2 $R0 $7 $5
	StrCmp${_WORDFUNC_S} $2 '' +4
	StrCmp${_WORDFUNC_S} $2 $0 +6
	IntOp $5 $5 + 1
	goto -4
	StrCmp${_WORDFUNC_S} $R0 $R1 WordFunc_WordReplace${_WORDFUNC_S}_error1
	StrCpy $R0 '$3$R0'
	goto WordFunc_WordReplace${_WORDFUNC_S}_end
	StrCpy $2 $R0 $5
	IntOp $5 $5 + $7
	StrCmp${_WORDFUNC_S} $4 '*' 0 +3
	StrCpy $6 $R0 $7 $5
	StrCmp${_WORDFUNC_S} $6 $0 -3
	StrCpy $R0 $R0 '' $5
	StrCpy $3 '$3$2$1'
	goto WordFunc_WordReplace${_WORDFUNC_S}_all

	WordFunc_WordReplace${_WORDFUNC_S}_one:
	StrCpy $5 0
	StrCpy $8 0
	goto WordFunc_WordReplace${_WORDFUNC_S}_loop

	WordFunc_WordReplace${_WORDFUNC_S}_preloop:
	IntOp $5 $5 + 1

	WordFunc_WordReplace${_WORDFUNC_S}_loop:
	StrCpy $6 $R0 $7 $5
	StrCmp${_WORDFUNC_S} $6$8 0 WordFunc_WordReplace${_WORDFUNC_S}_error1
	StrCmp${_WORDFUNC_S} $6 '' WordFunc_WordReplace${_WORDFUNC_S}_minus
	StrCmp${_WORDFUNC_S} $6 $0 0 WordFunc_WordReplace${_WORDFUNC_S}_preloop
	IntOp $8 $8 + 1
	StrCmp${_WORDFUNC_S} $3$8 +$2 WordFunc_WordReplace${_WORDFUNC_S}_found
	IntOp $5 $5 + $7
	goto WordFunc_WordReplace${_WORDFUNC_S}_loop

	WordFunc_WordReplace${_WORDFUNC_S}_minus:
	StrCmp${_WORDFUNC_S} $3 '-' 0 WordFunc_WordReplace${_WORDFUNC_S}_error2
	StrCpy $3 +
	IntOp $2 $8 - $2
	IntOp $2 $2 + 1
	IntCmp $2 0 WordFunc_WordReplace${_WORDFUNC_S}_error2 WordFunc_WordReplace${_WORDFUNC_S}_error2 WordFunc_WordReplace${_WORDFUNC_S}_one

	WordFunc_WordReplace${_WORDFUNC_S}_found:
	StrCpy $3 $R0 $5
	StrCmp${_WORDFUNC_S} $4 '*' 0 +5
	StrCpy $6 $3 '' -$7
	StrCmp${_WORDFUNC_S} $6 $0 0 +3
	StrCpy $3 $3 -$7
	goto -3
	IntOp $5 $5 + $7
	StrCmp${_WORDFUNC_S} $4 '*' 0 +3
	StrCpy $6 $R0 $7 $5
	StrCmp${_WORDFUNC_S} $6 $0 -3
	StrCpy $R0 $R0 '' $5
	StrCpy $R0 '$3$1$R0'
	goto WordFunc_WordReplace${_WORDFUNC_S}_end

	WordFunc_WordReplace${_WORDFUNC_S}_error3:
	StrCpy $R0 3
	goto WordFunc_WordReplace${_WORDFUNC_S}_error
	WordFunc_WordReplace${_WORDFUNC_S}_error2:
	StrCpy $R0 2
	goto WordFunc_WordReplace${_WORDFUNC_S}_error
	WordFunc_WordReplace${_WORDFUNC_S}_error1:
	StrCpy $R0 1
	WordFunc_WordReplace${_WORDFUNC_S}_error:
	StrCmp $9 'E' +3
	StrCpy $R0 $R1
	goto +2
	SetErrors

	WordFunc_WordReplace${_WORDFUNC_S}_end:
	Pop $R1
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
	Exch $R0
!macroend

!define WordReplace `!insertmacro WordReplaceCall`
!define un.WordReplace `!insertmacro WordReplaceCall`

!macro WordReplace
!macroend

!macro un.WordReplace
!macroend

!macro WordReplace_
	!verbose push
	!verbose ${_WORDFUNC_VERBOSE}

	!insertmacro WordReplaceBody ''

	!verbose pop
!macroend

!define WordReplaceS `!insertmacro WordReplaceSCall`
!define un.WordReplaceS `!insertmacro WordReplaceSCall`

!macro WordReplaceS
!macroend

!macro un.WordReplaceS
!macroend

!macro WordReplaceS_
	!verbose push
	!verbose ${_WORDFUNC_VERBOSE}

	!insertmacro WordReplaceBody 'S'

	!verbose pop
!macroend

!macro WordAddBody _WORDFUNC_S
	Exch $1
	Exch
	Exch $0
	Exch
	Exch 2
	Exch $R0
	Exch 2
	Push $2
	Push $3
	Push $4
	Push $5
	Push $6
	Push $7
	Push $R1
	ClearErrors

	StrCpy $7 ''
	StrCpy $2 $1 1
	StrCmp $2 'E' 0 +4
	StrCpy $7 E
	StrCpy $1 $1 '' 1
	goto -4

	StrCpy $5 0
	StrCpy $R1 $R0
	StrCpy $2 $1 '' 1
	StrCpy $1 $1 1
	StrCmp${_WORDFUNC_S} $1 '+' +2
	StrCmp${_WORDFUNC_S} $1 '-' 0 WordFunc_WordAdd${_WORDFUNC_S}_error3

	StrCmp${_WORDFUNC_S} $0 '' WordFunc_WordAdd${_WORDFUNC_S}_error1
	StrCmp${_WORDFUNC_S} $2 '' WordFunc_WordAdd${_WORDFUNC_S}_end
	StrCmp${_WORDFUNC_S} $R0 '' 0 +5
	StrCmp${_WORDFUNC_S} $1 '-' WordFunc_WordAdd${_WORDFUNC_S}_end
	StrCmp${_WORDFUNC_S} $1 '+' 0 +3
	StrCpy $R0 $2
	goto WordFunc_WordAdd${_WORDFUNC_S}_end

	WordFunc_WordAdd${_WORDFUNC_S}_loop:
	IntOp $5 $5 + 1
	!insertmacro WordFind${_WORDFUNC_S}Call 2 $2 $0 E+$5 $3
	IfErrors 0 WordFunc_WordAdd${_WORDFUNC_S}_/word
	StrCmp${_WORDFUNC_S} $3 2 +4
	StrCmp${_WORDFUNC_S} $3$5 11 0 +3
	StrCpy $3 $2
	goto WordFunc_WordAdd${_WORDFUNC_S}_/word
	StrCmp${_WORDFUNC_S} $1 '-' WordFunc_WordAdd${_WORDFUNC_S}_end WordFunc_WordAdd${_WORDFUNC_S}_preend

	WordFunc_WordAdd${_WORDFUNC_S}_/word:
	!insertmacro WordFind${_WORDFUNC_S}Call 2 $R0 $0 E/$3 $4
	IfErrors +2
	StrCmp${_WORDFUNC_S} $1 '-' WordFunc_WordAdd${_WORDFUNC_S}_delete WordFunc_WordAdd${_WORDFUNC_S}_loop
	StrCmp${_WORDFUNC_S} $1$4 '-1' +2
	StrCmp${_WORDFUNC_S} $1 '-' WordFunc_WordAdd${_WORDFUNC_S}_loop +4
	StrCmp${_WORDFUNC_S} $R0 $3 0 WordFunc_WordAdd${_WORDFUNC_S}_loop
	StrCpy $R0 ''
	goto WordFunc_WordAdd${_WORDFUNC_S}_end
	StrCmp${_WORDFUNC_S} $1$4 '+1' 0 +2
	StrCmp${_WORDFUNC_S} $R0 $3 WordFunc_WordAdd${_WORDFUNC_S}_loop
	StrCmp${_WORDFUNC_S} $R0 $R1 +3
	StrCpy $R1 '$R1$0$3'
	goto WordFunc_WordAdd${_WORDFUNC_S}_loop
	StrLen $6 $0
	StrCpy $6 $R0 '' -$6
	StrCmp${_WORDFUNC_S} $6 $0 0 -4
	StrCpy $R1 '$R1$3'
	goto WordFunc_WordAdd${_WORDFUNC_S}_loop

	WordFunc_WordAdd${_WORDFUNC_S}_delete:
	!insertmacro WordFind${_WORDFUNC_S}Call 2 $R0 $0 E+$4{} $R0
	goto WordFunc_WordAdd${_WORDFUNC_S}_/word

	WordFunc_WordAdd${_WORDFUNC_S}_error3:
	StrCpy $R1 3
	goto WordFunc_WordAdd${_WORDFUNC_S}_error
	WordFunc_WordAdd${_WORDFUNC_S}_error1:
	StrCpy $R1 1
	WordFunc_WordAdd${_WORDFUNC_S}_error:
	StrCmp $7 'E' 0 WordFunc_WordAdd${_WORDFUNC_S}_end
	SetErrors

	WordFunc_WordAdd${_WORDFUNC_S}_preend:
	StrCpy $R0 $R1

	WordFunc_WordAdd${_WORDFUNC_S}_end:
	Pop $R1
	Pop $7
	Pop $6
	Pop $5
	Pop $4
	Pop $3
	Pop $2
	Pop $1
	Pop $0
	Exch $R0
!macroend

!define WordAdd `!insertmacro WordAddCall`
!define un.WordAdd `!insertmacro WordAddCall`

!macro WordAdd
!macroend

!macro un.WordAdd
!macroend

!macro WordAdd_
	!verbose push
	!verbose ${_WORDFUNC_VERBOSE}

	!insertmacro WordAddBody ''

	!verbose pop
!macroend

!define WordAddS `!insertmacro WordAddSCall`
!define un.WordAddS `!insertmacro WordAddSCall`

!macro WordAddS
!macroend

!macro un.WordAddS
!macroend

!macro WordAddS_
	!verbose push
	!verbose ${_WORDFUNC_VERBOSE}

	!insertmacro WordAddBody 'S'

	!verbose pop
!macroend

!macro WordInsertBody _WORDFUNC_S
	Exch $2
	Exch
	Exch $1
	Exch
	Exch 2
	Exch $0
	Exch 2
	Exch 3
	Exch $R0
	Exch 3
	Push $3
	Push $4
	Push $5
	Push $6
	Push $7
	Push $8
	Push $9
	Push $R1
	ClearErrors

	StrCpy $5 ''
	StrCpy $6 $0
	StrCpy $7 }

	StrCpy $9 ''
	StrCpy $R1 $R0
	StrCpy $3 $2 1
	StrCpy $2 $2 '' 1
	StrCmp $3 'E' 0 +3
	StrCpy $9 'E'
	goto -4

	StrCmp${_WORDFUNC_S} $3 '+' +2
	StrCmp${_WORDFUNC_S} $3 '-' 0 WordFunc_WordInsert${_WORDFUNC_S}_error3
	IntOp $2 $2 + 0
	StrCmp${_WORDFUNC_S} $2 0 WordFunc_WordInsert${_WORDFUNC_S}_error2
	StrCmp${_WORDFUNC_S} $0 '' WordFunc_WordInsert${_WORDFUNC_S}_error1

	StrCmp${_WORDFUNC_S} $2 1 0 WordFunc_WordInsert${_WORDFUNC_S}_two
	GetLabelAddress $8 WordFunc_WordInsert${_WORDFUNC_S}_oneback
	StrCmp${_WORDFUNC_S} $3 '+' WordFunc_WordInsert${_WORDFUNC_S}_call
	StrCpy $7 {
	goto WordFunc_WordInsert${_WORDFUNC_S}_call
	WordFunc_WordInsert${_WORDFUNC_S}_oneback:
	IfErrors 0 +2
	StrCpy $4 $R0
	StrCmp${_WORDFUNC_S} $3 '+' 0 +3
	StrCpy $R0 '$1$0$4'
	goto WordFunc_WordInsert${_WORDFUNC_S}_end
	StrCpy $R0 '$4$0$1'
	goto WordFunc_WordInsert${_WORDFUNC_S}_end

	WordFunc_WordInsert${_WORDFUNC_S}_two:
	IntOp $2 $2 - 1
	GetLabelAddress $8 WordFunc_WordInsert${_WORDFUNC_S}_twoback
	StrCmp${_WORDFUNC_S} $3 '+' 0 WordFunc_WordInsert${_WORDFUNC_S}_call
	StrCpy $7 {
	goto WordFunc_WordInsert${_WORDFUNC_S}_call
	WordFunc_WordInsert${_WORDFUNC_S}_twoback:
	IfErrors 0 WordFunc_WordInsert${_WORDFUNC_S}_tree
	StrCmp${_WORDFUNC_S} $2$4 11 0 WordFunc_WordInsert${_WORDFUNC_S}_error2
	StrCmp${_WORDFUNC_S} $3 '+' 0 +3
	StrCpy $R0 '$R0$0$1'
	goto WordFunc_WordInsert${_WORDFUNC_S}_end
	StrCpy $R0 '$1$0$R0'
	goto WordFunc_WordInsert${_WORDFUNC_S}_end

	WordFunc_WordInsert${_WORDFUNC_S}_tree:
	StrCpy $7 }
	StrCpy $5 $4
	IntOp $2 $2 + 1
	GetLabelAddress $8 WordFunc_WordInsert${_WORDFUNC_S}_treeback
	StrCmp${_WORDFUNC_S} $3 '+' WordFunc_WordInsert${_WORDFUNC_S}_call
	StrCpy $7 {
	goto WordFunc_WordInsert${_WORDFUNC_S}_call
	WordFunc_WordInsert${_WORDFUNC_S}_treeback:
	IfErrors 0 +3
	StrCpy $4 ''
	StrCpy $6 ''
	StrCmp${_WORDFUNC_S} $3 '+' 0 +3
	StrCpy $R0 '$5$0$1$6$4'
	goto WordFunc_WordInsert${_WORDFUNC_S}_end
	StrCpy $R0 '$4$6$1$0$5'
	goto WordFunc_WordInsert${_WORDFUNC_S}_end

	WordFunc_WordInsert${_WORDFUNC_S}_call:
	!insertmacro WordFind${_WORDFUNC_S}Call 2 $R0 $0 E$3$2*$7 $4
	goto $8

	WordFunc_WordInsert${_WORDFUNC_S}_error3:
	StrCpy $R0 3
	goto WordFunc_WordInsert${_WORDFUNC_S}_error
	WordFunc_WordInsert${_WORDFUNC_S}_error2:
	StrCpy $R0 2
	goto WordFunc_WordInsert${_WORDFUNC_S}_error
	WordFunc_WordInsert${_WORDFUNC_S}_error1:
	StrCpy $R0 1
	WordFunc_WordInsert${_WORDFUNC_S}_error:
	StrCmp $9 'E' +3
	StrCpy $R0 $R1
	goto +2
	SetErrors

	WordFunc_WordInsert${_WORDFUNC_S}_end:
	Pop $R1
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
	Exch $R0
!macroend

!define WordInsert `!insertmacro WordInsertCall`
!define un.WordInsert `!insertmacro WordInsertCall`

!macro WordInsert
!macroend

!macro un.WordInsert
!macroend

!macro WordInsert_
	!verbose push
	!verbose ${_WORDFUNC_VERBOSE}

	!insertmacro WordInsertBody ''

	!verbose pop
!macroend


!define WordInsertS `!insertmacro WordInsertSCall`
!define un.WordInsertS `!insertmacro WordInsertSCall`

!macro WordInsertS
!macroend

!macro un.WordInsertS
!macroend

!macro WordInsertS_
	!verbose push
	!verbose ${_WORDFUNC_VERBOSE}

	!insertmacro WordInsertBody 'S'

	!verbose pop
!macroend

!macro StrFilterBody _WORDFUNC_S
	Exch $2
	Exch
	Exch $1
	Exch
	Exch 2
	Exch $0
	Exch 2
	Exch 3
	Exch $R0
	Exch 3
	Push $3
	Push $4
	Push $5
	Push $6
	Push $7
	Push $R1
	Push $R2
	Push $R3
	Push $R4
	Push $R5
	Push $R6
	Push $R7
	Push $R8
	ClearErrors

	StrCpy $R2 $0 '' -3
	StrCmp $R2 "eng" WordFunc_StrFilter${_WORDFUNC_S}_eng
	StrCmp $R2 "rus" WordFunc_StrFilter${_WORDFUNC_S}_rus
	WordFunc_StrFilter${_WORDFUNC_S}_eng:
	StrCpy $4 65
	StrCpy $5 90
	StrCpy $6 97
	StrCpy $7 122
	goto WordFunc_StrFilter${_WORDFUNC_S}_langend
	WordFunc_StrFilter${_WORDFUNC_S}_rus:
	StrCpy $4 192
	StrCpy $5 223
	StrCpy $6 224
	StrCpy $7 255
	goto WordFunc_StrFilter${_WORDFUNC_S}_langend
	;...

	WordFunc_StrFilter${_WORDFUNC_S}_langend:
	StrCpy $R7 ''
	StrCpy $R8 ''

	StrCmp${_WORDFUNC_S} $2 '' 0 WordFunc_StrFilter${_WORDFUNC_S}_begin

	WordFunc_StrFilter${_WORDFUNC_S}_restart1:
	StrCpy $2 ''
	StrCpy $3 $0 1
	StrCmp${_WORDFUNC_S} $3 '+' +2
	StrCmp${_WORDFUNC_S} $3 '-' 0 +3
	StrCpy $0 $0 '' 1
	goto +2
	StrCpy $3 ''

	IntOp $0 $0 + 0
	StrCmp${_WORDFUNC_S} $0 0 +5
	StrCpy $R7 $0 1 0
	StrCpy $R8 $0 1 1
	StrCpy $R2 $0 1 2
	StrCmp${_WORDFUNC_S} $R2 '' WordFunc_StrFilter${_WORDFUNC_S}_filter WordFunc_StrFilter${_WORDFUNC_S}_error

	WordFunc_StrFilter${_WORDFUNC_S}_restart2:
	StrCmp${_WORDFUNC_S} $3 '' WordFunc_StrFilter${_WORDFUNC_S}_end
	StrCpy $R7 ''
	StrCpy $R8 '+-'
	goto WordFunc_StrFilter${_WORDFUNC_S}_begin

	WordFunc_StrFilter${_WORDFUNC_S}_filter:
	StrCmp${_WORDFUNC_S} $R7 '1' +3
	StrCmp${_WORDFUNC_S} $R7 '2' +2
	StrCmp${_WORDFUNC_S} $R7 '3' 0 WordFunc_StrFilter${_WORDFUNC_S}_error

	StrCmp${_WORDFUNC_S} $R8 '' WordFunc_StrFilter${_WORDFUNC_S}_begin
	StrCmp${_WORDFUNC_S} $R7$R8 '23' +2
	StrCmp${_WORDFUNC_S} $R7$R8 '32' 0 +3
	StrCpy $R7 -1
	goto WordFunc_StrFilter${_WORDFUNC_S}_begin
	StrCmp${_WORDFUNC_S} $R7$R8 '13' +2
	StrCmp${_WORDFUNC_S} $R7$R8 '31' 0 +3
	StrCpy $R7 -2
	goto WordFunc_StrFilter${_WORDFUNC_S}_begin
	StrCmp${_WORDFUNC_S} $R7$R8 '12' +2
	StrCmp${_WORDFUNC_S} $R7$R8 '21' 0 WordFunc_StrFilter${_WORDFUNC_S}_error
	StrCpy $R7 -3

	WordFunc_StrFilter${_WORDFUNC_S}_begin:
	StrCpy $R6 0
	StrCpy $R1 ''

	WordFunc_StrFilter${_WORDFUNC_S}_loop:
	StrCpy $R2 $R0 1 $R6
	StrCmp${_WORDFUNC_S} $R2 '' WordFunc_StrFilter${_WORDFUNC_S}_restartchk

	StrCmp${_WORDFUNC_S} $2 '' +7
	StrCpy $R4 0
	StrCpy $R5 $2 1 $R4
	StrCmp${_WORDFUNC_S} $R5 '' WordFunc_StrFilter${_WORDFUNC_S}_addsymbol
	StrCmp${_WORDFUNC_S} $R5 $R2 WordFunc_StrFilter${_WORDFUNC_S}_skipsymbol
	IntOp $R4 $R4 + 1
	goto -4

	StrCmp${_WORDFUNC_S} $1 '' +7
	StrCpy $R4 0
	StrCpy $R5 $1 1 $R4
	StrCmp${_WORDFUNC_S} $R5 '' +4
	StrCmp${_WORDFUNC_S} $R5 $R2 WordFunc_StrFilter${_WORDFUNC_S}_addsymbol
	IntOp $R4 $R4 + 1
	goto -4

	StrCmp${_WORDFUNC_S} $R7 '1' +2
	StrCmp${_WORDFUNC_S} $R7 '-1' 0 +4
	StrCpy $R4 48
	StrCpy $R5 57
	goto WordFunc_StrFilter${_WORDFUNC_S}_loop2
	StrCmp${_WORDFUNC_S} $R8 '+-' 0 +2
	StrCmp${_WORDFUNC_S} $3 '+' 0 +4
	StrCpy $R4 $4
	StrCpy $R5 $5
	goto WordFunc_StrFilter${_WORDFUNC_S}_loop2
	StrCpy $R4 $6
	StrCpy $R5 $7

	WordFunc_StrFilter${_WORDFUNC_S}_loop2:
	IntFmt $R3 '%c' $R4
	StrCmp $R2 $R3 WordFunc_StrFilter${_WORDFUNC_S}_found
	StrCmp $R4 $R5 WordFunc_StrFilter${_WORDFUNC_S}_notfound
	IntOp $R4 $R4 + 1
	goto WordFunc_StrFilter${_WORDFUNC_S}_loop2

	WordFunc_StrFilter${_WORDFUNC_S}_found:
	StrCmp${_WORDFUNC_S} $R8 '+-' WordFunc_StrFilter${_WORDFUNC_S}_setcase
	StrCmp${_WORDFUNC_S} $R7 '3' WordFunc_StrFilter${_WORDFUNC_S}_skipsymbol
	StrCmp${_WORDFUNC_S} $R7 '-3' WordFunc_StrFilter${_WORDFUNC_S}_addsymbol
	StrCmp${_WORDFUNC_S} $R8 '' WordFunc_StrFilter${_WORDFUNC_S}_addsymbol WordFunc_StrFilter${_WORDFUNC_S}_skipsymbol

	WordFunc_StrFilter${_WORDFUNC_S}_notfound:
	StrCmp${_WORDFUNC_S} $R8 '+-' WordFunc_StrFilter${_WORDFUNC_S}_addsymbol
	StrCmp${_WORDFUNC_S} $R7 '3' 0 +2
	StrCmp${_WORDFUNC_S} $R5 57 WordFunc_StrFilter${_WORDFUNC_S}_addsymbol +3
	StrCmp${_WORDFUNC_S} $R7 '-3' 0 +5
	StrCmp${_WORDFUNC_S} $R5 57 WordFunc_StrFilter${_WORDFUNC_S}_skipsymbol
	StrCpy $R4 48
	StrCpy $R5 57
	goto WordFunc_StrFilter${_WORDFUNC_S}_loop2
	StrCmp${_WORDFUNC_S} $R8 '' WordFunc_StrFilter${_WORDFUNC_S}_skipsymbol WordFunc_StrFilter${_WORDFUNC_S}_addsymbol

	WordFunc_StrFilter${_WORDFUNC_S}_setcase:
	StrCpy $R2 $R3
	WordFunc_StrFilter${_WORDFUNC_S}_addsymbol:
	StrCpy $R1 $R1$R2
	WordFunc_StrFilter${_WORDFUNC_S}_skipsymbol:
	IntOp $R6 $R6 + 1
	goto WordFunc_StrFilter${_WORDFUNC_S}_loop

	WordFunc_StrFilter${_WORDFUNC_S}_error:
	SetErrors
	StrCpy $R0 ''
	goto WordFunc_StrFilter${_WORDFUNC_S}_end

	WordFunc_StrFilter${_WORDFUNC_S}_restartchk:
	StrCpy $R0 $R1
	StrCmp${_WORDFUNC_S} $2 '' 0 WordFunc_StrFilter${_WORDFUNC_S}_restart1
	StrCmp${_WORDFUNC_S} $R8 '+-' 0 WordFunc_StrFilter${_WORDFUNC_S}_restart2

	WordFunc_StrFilter${_WORDFUNC_S}_end:
	Pop $R8
	Pop $R7
	Pop $R6
	Pop $R5
	Pop $R4
	Pop $R3
	Pop $R2
	Pop $R1
	Pop $7
	Pop $6
	Pop $5
	Pop $4
	Pop $3
	Pop $2
	Pop $1
	Pop $0
	Exch $R0
!macroend

!define StrFilter `!insertmacro StrFilterCall`
!define un.StrFilter `!insertmacro StrFilterCall`

!macro StrFilter
!macroend

!macro un.StrFilter
!macroend

!macro StrFilter_
	!verbose push
	!verbose ${_WORDFUNC_VERBOSE}

	!insertmacro StrFilterBody ''

	!verbose pop
!macroend


!define StrFilterS `!insertmacro StrFilterSCall`
!define un.StrFilterS `!insertmacro StrFilterSCall`

!macro StrFilterS
!macroend

!macro un.StrFilterS
!macroend

!macro StrFilterS_
	!verbose push
	!verbose ${_WORDFUNC_VERBOSE}

	!insertmacro StrFilterBody 'S'

	!verbose pop
!macroend

!define VersionCompare `!insertmacro VersionCompareCall`
!define un.VersionCompare `!insertmacro VersionCompareCall`

!macro VersionCompare
!macroend

!macro un.VersionCompare
!macroend

!macro VersionCompare_
	!verbose push
	!verbose ${_WORDFUNC_VERBOSE}

	Exch $1
	Exch
	Exch $0
	Exch
	Push $2
	Push $3
	Push $4
	Push $5
	Push $6
	Push $7

	WordFunc_VersionCompare_begin:
	StrCpy $2 -1
	IntOp $2 $2 + 1
	StrCpy $3 $0 1 $2
	StrCmp $3 '' +2
	StrCmp $3 '.' 0 -3
	StrCpy $4 $0 $2
	IntOp $2 $2 + 1
	StrCpy $0 $0 '' $2

	StrCpy $2 -1
	IntOp $2 $2 + 1
	StrCpy $3 $1 1 $2
	StrCmp $3 '' +2
	StrCmp $3 '.' 0 -3
	StrCpy $5 $1 $2
	IntOp $2 $2 + 1
	StrCpy $1 $1 '' $2

	StrCmp $4$5 '' WordFunc_VersionCompare_equal

	StrCpy $6 -1
	IntOp $6 $6 + 1
	StrCpy $3 $4 1 $6
	StrCmp $3 '0' -2
	StrCmp $3 '' 0 +2
	StrCpy $4 0

	StrCpy $7 -1
	IntOp $7 $7 + 1
	StrCpy $3 $5 1 $7
	StrCmp $3 '0' -2
	StrCmp $3 '' 0 +2
	StrCpy $5 0

	StrCmp $4 0 0 +2
	StrCmp $5 0 WordFunc_VersionCompare_begin WordFunc_VersionCompare_newer2
	StrCmp $5 0 WordFunc_VersionCompare_newer1
	IntCmp $6 $7 0 WordFunc_VersionCompare_newer1 WordFunc_VersionCompare_newer2

	StrCpy $4 '1$4'
	StrCpy $5 '1$5'
	IntCmp $4 $5 WordFunc_VersionCompare_begin WordFunc_VersionCompare_newer2 WordFunc_VersionCompare_newer1

	WordFunc_VersionCompare_equal:
	StrCpy $0 0
	goto WordFunc_VersionCompare_end
	WordFunc_VersionCompare_newer1:
	StrCpy $0 1
	goto WordFunc_VersionCompare_end
	WordFunc_VersionCompare_newer2:
	StrCpy $0 2

	WordFunc_VersionCompare_end:
	Pop $7
	Pop $6
	Pop $5
	Pop $4
	Pop $3
	Pop $2
	Pop $1
	Exch $0

	!verbose pop
!macroend

!define VersionConvert `!insertmacro VersionConvertCall`
!define un.VersionConvert `!insertmacro VersionConvertCall`

!macro VersionConvert
!macroend

!macro un.VersionConvert
!macroend

!macro VersionConvert_
	!verbose push
	!verbose ${_WORDFUNC_VERBOSE}

	Exch $1
	Exch
	Exch $0
	Exch
	Push $2
	Push $3
	Push $4
	Push $5
	Push $6
	Push $7

	StrCmp $1 '' 0 +2
	StrCpy $1 'abcdefghijklmnopqrstuvwxyz'
	StrCpy $1 $1 99

	StrCpy $2 0
	StrCpy $7 'dot'
	goto WordFunc_VersionConvert_loop

	WordFunc_VersionConvert_preloop:
	IntOp $2 $2 + 1

	WordFunc_VersionConvert_loop:
	StrCpy $3 $0 1 $2
	StrCmp $3 '' WordFunc_VersionConvert_endcheck
	StrCmp $3 '.' WordFunc_VersionConvert_dot
	StrCmp $3 '0' WordFunc_VersionConvert_digit
	IntCmp $3 '0' WordFunc_VersionConvert_letter WordFunc_VersionConvert_letter WordFunc_VersionConvert_digit

	WordFunc_VersionConvert_dot:
	StrCmp $7 'dot' WordFunc_VersionConvert_replacespecial
	StrCpy $7 'dot'
	goto WordFunc_VersionConvert_preloop

	WordFunc_VersionConvert_digit:
	StrCmp $7 'letter' WordFunc_VersionConvert_insertdot
	StrCpy $7 'digit'
	goto WordFunc_VersionConvert_preloop

	WordFunc_VersionConvert_letter:
	StrCpy $5 0
	StrCpy $4 $1 1 $5
	IntOp $5 $5 + 1
	StrCmp $4 '' WordFunc_VersionConvert_replacespecial
	StrCmp $4 $3 0 -3
	IntCmp $5 9 0 0 +2
	StrCpy $5 '0$5'

	StrCmp $7 'letter' +2
	StrCmp $7 'dot' 0 +3
	StrCpy $6 ''
	goto +2
	StrCpy $6 '.'

	StrCpy $4 $0 $2
	IntOp $2 $2 + 1
	StrCpy $0 $0 '' $2
	StrCpy $0 '$4$6$5$0'
	StrLen $4 '$6$5'
	IntOp $2 $2 + $4
	IntOp $2 $2 - 1
	StrCpy $7 'letter'
	goto WordFunc_VersionConvert_loop

	WordFunc_VersionConvert_replacespecial:
	StrCmp $7 'dot' 0 +3
	StrCpy $6 ''
	goto +2
	StrCpy $6 '.'

	StrCpy $4 $0 $2
	IntOp $2 $2 + 1
	StrCpy $0 $0 '' $2
	StrCpy $0 '$4$6$0'
	StrLen $4 $6
	IntOp $2 $2 + $4
	IntOp $2 $2 - 1
	StrCpy $7 'dot'
	goto WordFunc_VersionConvert_loop

	WordFunc_VersionConvert_insertdot:
	StrCpy $4 $0 $2
	StrCpy $0 $0 '' $2
	StrCpy $0 '$4.$0'
	StrCpy $7 'dot'
	goto WordFunc_VersionConvert_preloop

	WordFunc_VersionConvert_endcheck:
	StrCpy $4 $0 1 -1
	StrCmp $4 '.' 0 WordFunc_VersionConvert_end
	StrCpy $0 $0 -1
	goto -3

	WordFunc_VersionConvert_end:
	Pop $7
	Pop $6
	Pop $5
	Pop $4
	Pop $3
	Pop $2
	Pop $1
	Exch $0

	!verbose pop
!macroend

!endif
