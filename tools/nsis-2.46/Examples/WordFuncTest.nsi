;_____________________________________________________________________________
;
;                          Word Functions Test
;_____________________________________________________________________________
;
; 2006 Shengalts Aleksander aka Instructor (Shengalts@mail.ru)

Name "Word Functions Test"
OutFile "WordFuncTest.exe"
Caption "$(^Name)"
ShowInstDetails show
XPStyle on
RequestExecutionLevel user

Var FUNCTION
Var OUT

!include "WordFunc.nsh"

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



Section WordFind
	${StackVerificationStart} WordFind

	${WordFind} '||io.sys|||Program Files|||WINDOWS' '||' '-02' $OUT
	StrCmp $OUT '|Program Files' 0 error

	${WordFind} '||io.sys||||Program Files||||WINDOWS' '||' '-2' $OUT
	StrCmp $OUT 'Program Files' 0 error

	${WordFind} 'C:\io.sys|||logo.sys|||WINDOWS' '||' '-2}' $OUT
	StrCmp $OUT '|logo.sys|||WINDOWS' 0 error

	${WordFind} 'C:\io.sys|||logo.sys|||WINDOWS' '||' '#' $OUT
	StrCmp $OUT '3' 0 error

	${WordFind} 'C:\io.sys|||logo.sys|||WINDOWS' '||' '*' $OUT
	StrCmp $OUT '2' 0 error

	${WordFind} 'C:\io.sys|||Program Files|||WINDOWS' '||' '/|Program Files' $OUT
	StrCmp $OUT '2' 0 error

	${WordFind} 'C:\io.sys|||logo.sys|||WINDOWS' '||' '+2}}' $OUT
	StrCmp $OUT '|||WINDOWS' 0 error

	${WordFind} 'C:\io.sys|||logo.sys|||WINDOWS' '||' '+2{}' $OUT
	StrCmp $OUT 'C:\io.sys|||WINDOWS' 0 error

	${WordFind} 'C:\io.sys|||logo.sys|||WINDOWS' '||' '+2*}' $OUT
	StrCmp $OUT '|logo.sys|||WINDOWS' 0 error

	${WordFind} 'C:\\Program Files\\NSIS\\NSIS.chm' '\' '-2{*' $OUT
	StrCmp $OUT 'C:\\Program Files\\NSIS' 0 error

	${WordFind} 'C:\io.sys|||Program Files|||WINDOWS|||' '||' '-1' $OUT
	StrCmp $OUT '|' 0 error

	${WordFind} '||C:\io.sys|||logo.sys|||WINDOWS||' '||' '-1}' $OUT
	StrCmp $OUT '' 0 error

	${WordFind} '||C:\io.sys|||logo.sys|||WINDOWS||' '||' '+1{' $OUT
	StrCmp $OUT '' 0 error

	${WordFind} 'C:\io.sys|||logo.sys' '_' 'E+1' $OUT
	IfErrors 0 error
	StrCmp $OUT 1 0 error

	${WordFind} 'C:\io.sys|||logo.sys|||' '\' 'E+3' $OUT
	IfErrors 0 error
	StrCmp $OUT 2 0 error

	${WordFind} 'C:\io.sys|||logo.sys' '\' 'E1' $OUT
	IfErrors 0 error
	StrCmp $OUT 3 0 error

	goto +2
	error:
	SetErrors

	${StackVerificationEnd}
SectionEnd


Section WordFindS
	${StackVerificationStart} WordFindS

	${WordFindS} 'C:\io.sys|||Program Files|||WINDOWS' '||' '/|PROGRAM FILES' $OUT
	StrCmp $OUT 'C:\io.sys|||Program Files|||WINDOWS' 0 error

	${WordFindS} 'C:\io.sys|||Program Files|||WINDOWS' '||' '/|Program Files' $OUT
	StrCmp $OUT '2' 0 error

	goto +2
	error:
	SetErrors

	${StackVerificationEnd}
SectionEnd


Section WordFind2X
	${StackVerificationStart} WordFind2X

	${WordFind2X} '[C:\io.sys];[C:\logo.sys];[C:\WINDOWS]' '[C:\' '];' '+2' $OUT
	StrCmp $OUT 'logo.sys' 0 error

	${WordFind2X} 'C:\WINDOWS C:\io.sys C:\logo.sys' '\' '.' '-1' $OUT
	StrCmp $OUT 'logo' 0 error

	${WordFind2X} 'C:\WINDOWS C:\io.sys C:\logo.sys' '\' '.' '-1{{' $OUT
	StrCmp $OUT 'C:\WINDOWS C:\io.sys C:' 0 error

	${WordFind2X} 'C:\WINDOWS C:\io.sys C:\logo.sys' '\' '.' '-1{}' $OUT
	StrCmp $OUT 'C:\WINDOWS C:\io.sys C:sys' 0 error

	${WordFind2X} 'C:\WINDOWS C:\io.sys C:\logo.sys' '\' '.' '-1{*' $OUT
	StrCmp $OUT 'C:\WINDOWS C:\io.sys C:\logo.' 0 error

	${WordFind2X} 'C:\WINDOWS C:\io.sys C:\logo.sys' '\' '.' '/logo' $OUT
	StrCmp $OUT '2' 0 error

	${WordFind2X} '||a||b||c' '||' '||' 'E+1' $OUT
	StrCmp $OUT 'a' 0 error

	${WordFind2X} '[io.sys];[C:\logo.sys]' '\' '];' 'E+1' $OUT
	IfErrors 0 error
	StrCmp $OUT 1 0 error

	${WordFind2X} '[io.sys];[C:\logo.sys]' '[' '];' 'E+2' $OUT
	IfErrors 0 error
	StrCmp $OUT 2 0 error

	${WordFind2X} '[io.sys];[C:\logo.sys]' '\' '];' 'E2' $OUT
	IfErrors 0 error
	StrCmp $OUT 3 0 error

	goto +2
	error:
	SetErrors

	${StackVerificationEnd}
SectionEnd


Section WordFind2XS
	${StackVerificationStart} WordFind2XS

	${WordFind2XS} 'C:\WINDOWS C:\io.sys C:\logo.sys' '\' '.' '/LOGO' $OUT
	StrCmp $OUT 'C:\WINDOWS C:\io.sys C:\logo.sys' 0 error

	${WordFind2XS} 'C:\WINDOWS C:\io.sys C:\logo.sys' '\' '.' '/logo' $OUT
	StrCmp $OUT '2' 0 error

	goto +2
	error:
	SetErrors

	${StackVerificationEnd}
SectionEnd


Section WordFind3X
	${StackVerificationStart} WordFind3X

	${WordFind3X} '[1.AAB];[2.BAA];[3.BBB];' '[' 'AA' '];' '+1' $OUT
	StrCmp $OUT '1.AAB' 0 error

	${WordFind3X} '[1.AAB];[2.BAA];[3.BBB];' '[' 'AA' '];' '-1' $OUT
	StrCmp $OUT '2.BAA' 0 error

	${WordFind3X} '[1.AAB];[2.BAA];[3.BBB];' '[' 'AA' '];' '-1{{' $OUT
	StrCmp $OUT '[1.AAB];' 0 error

	${WordFind3X} '[1.AAB];[2.BAA];[3.BBB];' '[' 'AA' '];' '-1{}' $OUT
	StrCmp $OUT '[1.AAB];[3.BBB];' 0 error

	${WordFind3X} '[1.AAB];[2.BAA];[3.BBB];' '[' 'AA' '];' '-1{*' $OUT
	StrCmp $OUT '[1.AAB];[2.BAA];' 0 error

	${WordFind3X} '[1.AAB];[2.BAA];[3.BBB];' '[' 'AA' '];' '/2.BAA' $OUT
	StrCmp $OUT '2' 0 error

	${WordFind3X} '[1.AAB];[2.BAA];[3.BBB];' '[' 'XX' '];' 'E+1' $OUT
	IfErrors 0 error
	StrCmp $OUT '1' 0 error

	${WordFind3X} '[1.AAB];[2.BAA];[3.BBB];' '[' 'AA' '];' 'E+3' $OUT
	IfErrors 0 error
	StrCmp $OUT '2' 0 error

	${WordFind3X} '[1.AAB];[2.BAA];[3.BBB];' '[' 'AA' '];' 'E3' $OUT
	IfErrors 0 error
	StrCmp $OUT '3' 0 error

	goto +2
	error:
	SetErrors

	${StackVerificationEnd}
SectionEnd


Section WordFind3XS
	${StackVerificationStart} WordFind3XS

	${WordFind3XS} '[1.AAB];[2.BAA];[3.BBB];' '[' 'AA' '];' '/2.baa' $OUT
	StrCmp $OUT '[1.AAB];[2.BAA];[3.BBB];' 0 error

	${WordFind3XS} '[1.AAB];[2.BAA];[3.BBB];' '[' 'AA' '];' '/2.BAA' $OUT
	StrCmp $OUT '2' 0 error

	goto +2
	error:
	SetErrors

	${StackVerificationEnd}
SectionEnd


Section WordReplace
	${StackVerificationStart} WordReplace

	${WordReplace} 'C:\io.sys C:\logo.sys C:\WINDOWS' 'SYS' 'bmp' '+2' $OUT
	StrCmp $OUT 'C:\io.sys C:\logo.bmp C:\WINDOWS' 0 error

	${WordReplace} 'C:\io.sys C:\logo.sys C:\WINDOWS' 'SYS' '' '+' $OUT
	StrCmp $OUT 'C:\io. C:\logo. C:\WINDOWS' 0 error

	${WordReplace} 'C:\io.sys C:\logo.sys C:\WINDOWS' 'C:\io.sys' '' '+' $OUT
	StrCmp $OUT ' C:\logo.sys C:\WINDOWS' 0 error

	${WordReplace} 'C:\io.sys      C:\logo.sys   C:\WINDOWS' ' ' ' ' '+1*' $OUT
	StrCmp $OUT 'C:\io.sys C:\logo.sys   C:\WINDOWS' 0 error

	${WordReplace} 'C:\io.sys C:\logo.sysSYSsys C:\WINDOWS' 'sys' 'bmp' '+*' $OUT
	StrCmp $OUT 'C:\io.bmp C:\logo.bmp C:\WINDOWS' 0 error

	${WordReplace} 'SYSsysC:\io.sys C:\logo.sys C:\WINDOWSsysSYSsys' 'sys' '|' '{' $OUT
	StrCmp $OUT '||C:\io.sys C:\logo.sys C:\WINDOWSsysSYSsys' 0 error

	${WordReplace} 'SYSsysC:\io.sys C:\logo.sys C:\WINDOWSsysSYSsys' 'sys' '|' '}' $OUT
	StrCmp $OUT 'SYSsysC:\io.sys C:\logo.sys C:\WINDOWS|||' 0 error

	${WordReplace} 'SYSsysC:\io.sys C:\logo.sys C:\WINDOWSsysSYSsys' 'sys' '|' '{}' $OUT
	StrCmp $OUT '||C:\io.sys C:\logo.sys C:\WINDOWS|||' 0 error

	${WordReplace} 'SYSsysC:\io.sys C:\logo.sys C:\WINDOWSsysSYSsys' 'sys' '|' '{*' $OUT
	StrCmp $OUT '|C:\io.sys C:\logo.sys C:\WINDOWSsysSYSsys' 0 error

	${WordReplace} 'SYSsysC:\io.sys C:\logo.sys C:\WINDOWSsysSYSsys' 'sys' '|' '}*' $OUT
	StrCmp $OUT 'SYSsysC:\io.sys C:\logo.sys C:\WINDOWS|' 0 error

	${WordReplace} 'SYSsysC:\io.sys C:\logo.sys C:\WINDOWSsysSYSsys' 'sys' '|' '{}*' $OUT
	StrCmp $OUT '|C:\io.sys C:\logo.sys C:\WINDOWS|' 0 error

	${WordReplace} 'sysSYSsysC:\io.sys C:\logo.sys C:\WINDOWSsysSYSsys' 'sys' '|' '{}*' $OUT
	StrCmp $OUT '|C:\io.sys C:\logo.sys C:\WINDOWS|' 0 error

	${WordReplace} 'C:\io.sys C:\logo.sys' '#sys' '|sys|' 'E+1' $OUT
	IfErrors 0 error
	StrCmp $OUT '1' 0 error

	${WordReplace} 'C:\io.sys C:\logo.sys' '.sys' '|sys|' 'E+3' $OUT
	IfErrors 0 error
	StrCmp $OUT '2' 0 error

	${WordReplace} 'C:\io.sys C:\logo.sys' '.sys' '|sys|' 'E3' $OUT
	IfErrors 0 error
	StrCmp $OUT '3' 0 error

	goto +2
	error:
	SetErrors

	${StackVerificationEnd}
SectionEnd


Section WordReplaceS
	${StackVerificationStart} WordReplaceS

	${WordReplaceS} 'C:\io.sys C:\logo.sys C:\WINDOWS' 'SYS' 'bmp' '+2' $OUT
	StrCmp $OUT 'C:\io.sys C:\logo.sys C:\WINDOWS' 0 error

	${WordReplaceS} 'C:\io.sys C:\logo.sys C:\WINDOWS' 'sys' 'bmp' '+2' $OUT
	StrCmp $OUT 'C:\io.sys C:\logo.bmp C:\WINDOWS' 0 error

	goto +2
	error:
	SetErrors

	${StackVerificationEnd}
SectionEnd


Section WordAdd
	${StackVerificationStart} WordAdd

	${WordAdd} 'C:\io.sys C:\WINDOWS' ' ' '+C:\WINDOWS C:\config.sys' $OUT
	StrCmp $OUT 'C:\io.sys C:\WINDOWS C:\config.sys' 0 error

	${WordAdd} 'C:\io.sys C:\logo.sys C:\WINDOWS' ' ' '-C:\WINDOWS C:\config.sys C:\IO.SYS' $OUT
	StrCmp $OUT 'C:\logo.sys' 0 error

	${WordAdd} 'C:\io.sys' ' ' '+C:\WINDOWS C:\config.sys C:\IO.SYS' $OUT
	StrCmp $OUT 'C:\io.sys C:\WINDOWS C:\config.sys' 0 error

	${WordAdd} 'C:\io.sys C:\logo.sys C:\WINDOWS' ' ' '-C:\WINDOWS' $OUT
	StrCmp $OUT 'C:\io.sys C:\logo.sys' 0 error

	${WordAdd} 'C:\io.sys C:\logo.sys' ' ' '+C:\logo.sys' $OUT
	StrCmp $OUT 'C:\io.sys C:\logo.sys' 0 error

	${WordAdd} 'C:\io.sys C:\logo.sys' ' ' 'E-' $OUT
	StrCmp $OUT 'C:\io.sys C:\logo.sys' 0 error
	IfErrors error

	${WordAdd} 'C:\io.sys C:\logo.sys' '' 'E-C:\logo.sys' $OUT
	IfErrors 0 error
	StrCmp $OUT '1' 0 error

	${WordAdd} 'C:\io.sys C:\logo.sys' '' 'EC:\logo.sys' $OUT
	IfErrors 0 error
	StrCmp $OUT '3' 0 error

	goto +2
	error:
	SetErrors

	${StackVerificationEnd}
SectionEnd


Section WordAddS
	${StackVerificationStart} WordAddS

	${WordAddS} 'C:\io.sys C:\WINDOWS' ' ' '+C:\windows C:\config.sys' $OUT
	StrCmp $OUT 'C:\io.sys C:\WINDOWS C:\windows C:\config.sys' 0 error

	${WordAddS} 'C:\io.sys C:\WINDOWS' ' ' '+C:\WINDOWS C:\config.sys' $OUT
	StrCmp $OUT 'C:\io.sys C:\WINDOWS C:\config.sys' 0 error

	goto +2
	error:
	SetErrors

	${StackVerificationEnd}
SectionEnd


Section WordInsert
	${StackVerificationStart} WordInsert

	${WordInsert} 'C:\io.sys C:\WINDOWS' ' ' 'C:\logo.sys' '-2' $OUT
	StrCmp $OUT 'C:\io.sys C:\logo.sys C:\WINDOWS' 0 error

	${WordInsert} 'C:\io.sys' ' ' 'C:\WINDOWS' '+2' $OUT
	StrCmp $OUT 'C:\io.sys C:\WINDOWS' 0 error

	${WordInsert} '' ' ' 'C:\WINDOWS' '+1' $OUT
	StrCmp $OUT 'C:\WINDOWS ' 0 error

	${WordInsert} 'C:\io.sys C:\logo.sys' '' 'C:\logo.sys' 'E+1' $OUT
	IfErrors 0 error
	StrCmp $OUT '1' 0 error

	${WordInsert} 'C:\io.sys C:\logo.sys' ' ' 'C:\logo.sys' 'E+4' $OUT
	IfErrors 0 error
	StrCmp $OUT '2' 0 error

	${WordInsert} 'C:\io.sys C:\logo.sys' '' 'C:\logo.sys' 'E1' $OUT
	IfErrors 0 error
	StrCmp $OUT '3' 0 error

	goto +2
	error:
	SetErrors

	${StackVerificationEnd}
SectionEnd


Section WordInsertS
	${StackVerificationStart} WordInsertS

	${WordInsertS} 'C:\io.sys x C:\logo.sys' ' X ' 'C:\NTLDR' '+2' $OUT
	StrCmp $OUT 'C:\io.sys x C:\logo.sys X C:\NTLDR' 0 error

	${WordInsertS} 'C:\io.sys x C:\logo.sys' ' x ' 'C:\NTLDR' '+2' $OUT
	StrCmp $OUT 'C:\io.sys x C:\NTLDR x C:\logo.sys' 0 error

	goto +2
	error:
	SetErrors

	${StackVerificationEnd}
SectionEnd


Section StrFilter
	${StackVerificationStart} StrFilter

	${StrFilter} '123abc 456DEF 7890|%#' '+' '' '' $OUT
	IfErrors error
	StrCmp $OUT '123ABC 456DEF 7890|%#' 0 error

	${StrFilter} '123abc 456DEF 7890|%#' '-' 'ef' '' $OUT
	IfErrors error
	StrCmp $OUT '123abc 456dEF 7890|%#' 0 error

	${StrFilter} '123abc 456DEF 7890|%#' '2' '|%' '' $OUT
	IfErrors error
	StrCmp $OUT 'abcDEF|%' 0 error

	${StrFilter} '123abc 456DEF 7890|%#' '13' 'af' '4590' $OUT
	IfErrors error
	StrCmp $OUT '123a 6F 78|%#' 0 error

	${StrFilter} '123abc 456DEF 7890|%#' '+12' 'b' 'def' $OUT
	IfErrors error
	StrCmp $OUT '123AbC4567890' 0 error

	${StrFilter} '123abc 456DEF 7890|%#' '+12' 'b' 'def' $OUT
	IfErrors error
	StrCmp $OUT '123AbC4567890' 0 error

	${StrFilter} '123abc 456DEF 7890|%#' '123' 'b' 'def' $OUT
	IfErrors 0 error

	goto +2
	error:
	SetErrors

	${StackVerificationEnd}
SectionEnd


Section StrFilterS
	${StackVerificationStart} StrFilterS

	${StrFilterS} '123abc 456DEF 7890|%#' '13' 'af' '4590' $OUT
	IfErrors error
	StrCmp $OUT '123a 6 78|%#' 0 error

	goto +2
	error:
	SetErrors

	${StackVerificationEnd}
SectionEnd


Section VersionCompare
	${StackVerificationStart} VersionCompare

	${VersionCompare} '1.1.1.9' '1.1.1.01' $OUT
	StrCmp $OUT '1' 0 error

	${VersionCompare} '1.1.1.1' '1.1.1.10' $OUT
	StrCmp $OUT '2' 0 error

	${VersionCompare} '91.1.1.1' '101.1.1.9' $OUT
	StrCmp $OUT '2' 0 error

	${VersionCompare} '1.1.1.1' '1.1.1.1' $OUT
	StrCmp $OUT '0' 0 error

	${VersionCompare} '1.1.1.9' '1.1.1.10' $OUT
	StrCmp $OUT '2' 0 error

	${VersionCompare} '1.1.1.0' '1.1.1' $OUT
	StrCmp $OUT '0' 0 error

	${VersionCompare} '1.1.0.0' '1.1' $OUT
	StrCmp $OUT '0' 0 error

	goto +2
	error:
	SetErrors

	${StackVerificationEnd}
SectionEnd


Section VersionConvert
	${StackVerificationStart} VersionConvert

	${VersionConvert} '9.0a' '' $OUT
	StrCmp $OUT '9.0.01' 0 error

	${VersionConvert} '9.0c' '' $OUT
	StrCmp $OUT '9.0.03' 0 error

	${VersionConvert} '0.15c-9m' '' $OUT
	StrCmp $OUT '0.15.03.9.13' 0 error

	${VersionConvert} '0.15c+' 'abcdefghijklmnopqrstuvwxyz+' $OUT
	StrCmp $OUT '0.15.0327' 0 error

	${VersionConvert} '0.0xa12.x.ax|.|.|x|a|.3|a.4.||5.|' '' $OUT
	StrCmp $OUT '0.0.2401.12.24.0124.24.01.3.01.4.5' 0 error

	goto +2
	error:
	SetErrors

	${StackVerificationEnd}
SectionEnd


Section WriteUninstaller
	goto +2
	WriteUninstaller '$EXEDIR\un.WordFuncTest.exe'
SectionEnd



;############### UNINSTALL ###############

Section un.Uninstall
	${WordFind} 'C:\io.sys C:\Program Files C:\WINDOWS' ' C:\' '-02' $OUT
	${WordFindS} 'C:\io.sys C:\Program Files C:\WINDOWS' ' C:\' '-02' $OUT
	${WordFind2X} '[C:\io.sys];[C:\logo.sys];[C:\WINDOWS]' '[C:\' '];' '+2' $OUT
	${WordFind2XS} '[C:\io.sys];[C:\logo.sys];[C:\WINDOWS]' '[C:\' '];' '+2' $OUT
	${WordFind3X} '[1.AAB];[2.BAA];[3.BBB];' '[' 'AA' '];' '+1' $OUT
	${WordFind3XS} '[1.AAB];[2.BAA];[3.BBB];' '[' 'AA' '];' '+1' $OUT
	${WordReplace} 'C:\io.sys C:\logo.sys C:\WINDOWS' 'SYS' 'bmp' '+2' $OUT
	${WordReplaceS} 'C:\io.sys C:\logo.sys C:\WINDOWS' 'SYS' 'bmp' '+2' $OUT
	${WordAdd} 'C:\io.sys C:\WINDOWS' ' ' '+C:\WINDOWS C:\config.sys' $OUT
	${WordAddS} 'C:\io.sys C:\WINDOWS' ' ' '+C:\WINDOWS C:\config.sys' $OUT
	${WordInsert} 'C:\io.sys C:\WINDOWS' ' ' 'C:\logo.sys' '-2' $OUT
	${WordInsertS} 'C:\io.sys C:\WINDOWS' ' ' 'C:\logo.sys' '-2' $OUT
	${StrFilter} '123abc 456DEF 7890|%#' '+' '' '' $OUT
	${StrFilterS} '123abc 456DEF 7890|%#' '+' '' '' $OUT
	${VersionCompare} '1.1.1.9' '1.1.1.01' $OUT
	${VersionConvert} '9.0a' '' $OUT
SectionEnd
