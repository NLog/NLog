/*
_____________________________________________________________________________

                       File Functions Header v3.4
_____________________________________________________________________________

 2006 Shengalts Aleksander aka Instructor (Shengalts@mail.ru)

 See documentation for more information about the following functions.

 Usage in script:
 1. !include "FileFunc.nsh"
 2. [Section|Function]
      ${FileFunction} "Param1" "Param2" "..." $var
    [SectionEnd|FunctionEnd]


 FileFunction=[Locate|GetSize|DriveSpace|GetDrives|GetTime|GetFileAttributes|
               GetFileVersion|GetExeName|GetExePath|GetParameters|GetOptions|
               GetOptionsS|GetRoot|GetParent|GetFileName|GetBaseName|GetFileExt|
               BannerTrimPath|DirState|RefreshShellIcons]

_____________________________________________________________________________

                       Thanks to:
_____________________________________________________________________________

GetSize
	KiCHiK (Function "FindFiles")
DriveSpace
	sunjammer (Function "CheckSpaceFree")
GetDrives
	deguix (Based on his idea of Function "DetectDrives")
GetTime
	Takhir (Script "StatTest") and deguix (Function "FileModifiedDate")
GetFileVersion
	KiCHiK (Based on his example for command "GetDLLVersion")
GetParameters
	sunjammer (Based on his Function "GetParameters")
GetRoot
	KiCHiK (Based on his Function "GetRoot")
GetParent
	sunjammer (Based on his Function "GetParent")
GetFileName
	KiCHiK (Based on his Function "GetFileName")
GetBaseName
	comperio (Based on his idea of Function "GetBaseName")
GetFileExt
	opher (author)
RefreshShellIcons
	jerome tremblay (author)
*/


;_____________________________________________________________________________
;
;                         Macros
;_____________________________________________________________________________
;
; Change log window verbosity (default: 3=no script)
;
; Example:
; !include "FileFunc.nsh"
; !insertmacro Locate
; ${FILEFUNC_VERBOSE} 4   # all verbosity
; !insertmacro VersionCompare
; ${FILEFUNC_VERBOSE} 3   # no script

!ifndef FILEFUNC_INCLUDED
!define FILEFUNC_INCLUDED

!include Util.nsh

!verbose push
!verbose 3
!ifndef _FILEFUNC_VERBOSE
	!define _FILEFUNC_VERBOSE 3
!endif
!verbose ${_FILEFUNC_VERBOSE}
!define FILEFUNC_VERBOSE `!insertmacro FILEFUNC_VERBOSE`
!verbose pop

!macro FILEFUNC_VERBOSE _VERBOSE
	!verbose push
	!verbose 3
	!undef _FILEFUNC_VERBOSE
	!define _FILEFUNC_VERBOSE ${_VERBOSE}
	!verbose pop
!macroend

!macro LocateCall _PATH _OPTIONS _FUNC
	!verbose push
	!verbose ${_FILEFUNC_VERBOSE}
	Push $0
	Push `${_PATH}`
	Push `${_OPTIONS}`
	GetFunctionAddress $0 `${_FUNC}`
	Push `$0`
	${CallArtificialFunction} Locate_
	Pop $0
	!verbose pop
!macroend

!macro GetSizeCall _PATH _OPTIONS _RESULT1 _RESULT2 _RESULT3
	!verbose push
	!verbose ${_FILEFUNC_VERBOSE}
	Push `${_PATH}`
	Push `${_OPTIONS}`
	${CallArtificialFunction} GetSize_
	Pop ${_RESULT1}
	Pop ${_RESULT2}
	Pop ${_RESULT3}
	!verbose pop
!macroend

!macro DriveSpaceCall _DRIVE _OPTIONS _RESULT
	!verbose push
	!verbose ${_FILEFUNC_VERBOSE}
	Push `${_DRIVE}`
	Push `${_OPTIONS}`
	${CallArtificialFunction} DriveSpace_
	Pop ${_RESULT}
	!verbose pop
!macroend

!macro GetDrivesCall _DRV _FUNC
	!verbose push
	!verbose ${_FILEFUNC_VERBOSE}
	Push $0
	Push `${_DRV}`
	GetFunctionAddress $0 `${_FUNC}`
	Push `$0`
	${CallArtificialFunction} GetDrives_
	Pop $0
	!verbose pop
!macroend

!macro GetTimeCall _FILE _OPTION _RESULT1 _RESULT2 _RESULT3 _RESULT4 _RESULT5 _RESULT6 _RESULT7
	!verbose push
	!verbose ${_FILEFUNC_VERBOSE}
	Push `${_FILE}`
	Push `${_OPTION}`
	${CallArtificialFunction} GetTime_
	Pop ${_RESULT1}
	Pop ${_RESULT2}
	Pop ${_RESULT3}
	Pop ${_RESULT4}
	Pop ${_RESULT5}
	Pop ${_RESULT6}
	Pop ${_RESULT7}
	!verbose pop
!macroend

!macro GetFileAttributesCall _PATH _ATTR _RESULT
	!verbose push
	!verbose ${_FILEFUNC_VERBOSE}
	Push `${_PATH}`
	Push `${_ATTR}`
	${CallArtificialFunction} GetFileAttributes_
	Pop ${_RESULT}
	!verbose pop
!macroend

!macro GetFileVersionCall _FILE _RESULT
	!verbose push
	!verbose ${_FILEFUNC_VERBOSE}
	Push `${_FILE}`
	${CallArtificialFunction} GetFileVersion_
	Pop ${_RESULT}
	!verbose pop
!macroend

!macro GetExeNameCall _RESULT
	!verbose push
	!verbose ${_FILEFUNC_VERBOSE}
	${CallArtificialFunction} GetExeName_
	Pop ${_RESULT}
	!verbose pop
!macroend

!macro GetExePathCall _RESULT
	!verbose push
	!verbose ${_FILEFUNC_VERBOSE}
	${CallArtificialFunction} GetExePath_
	Pop ${_RESULT}
	!verbose pop
!macroend

!macro GetParametersCall _RESULT
	!verbose push
	!verbose ${_FILEFUNC_VERBOSE}
	${CallArtificialFunction} GetParameters_
	Pop ${_RESULT}
	!verbose pop
!macroend

!macro GetOptionsCall _PARAMETERS _OPTION _RESULT
	!verbose push
	!verbose ${_FILEFUNC_VERBOSE}
	Push `${_PARAMETERS}`
	Push `${_OPTION}`
	${CallArtificialFunction} GetOptions_
	Pop ${_RESULT}
	!verbose pop
!macroend

!macro GetOptionsSCall _PARAMETERS _OPTION _RESULT
	!verbose push
	!verbose ${_FILEFUNC_VERBOSE}
	Push `${_PARAMETERS}`
	Push `${_OPTION}`
	${CallArtificialFunction} GetOptionsS_
	Pop ${_RESULT}
	!verbose pop
!macroend

!macro GetRootCall _FULLPATH _RESULT
	!verbose push
	!verbose ${_FILEFUNC_VERBOSE}
	Push `${_FULLPATH}`
	${CallArtificialFunction} GetRoot_
	Pop ${_RESULT}
	!verbose pop
!macroend

!macro GetParentCall _PATHSTRING _RESULT
	!verbose push
	!verbose ${_FILEFUNC_VERBOSE}
	Push `${_PATHSTRING}`
	${CallArtificialFunction} GetParent_
	Pop ${_RESULT}
	!verbose pop
!macroend

!macro GetFileNameCall _PATHSTRING _RESULT
	!verbose push
	!verbose ${_FILEFUNC_VERBOSE}
	Push `${_PATHSTRING}`
	${CallArtificialFunction} GetFileName_
	Pop ${_RESULT}
	!verbose pop
!macroend

!macro GetBaseNameCall _FILESTRING _RESULT
	!verbose push
	!verbose ${_FILEFUNC_VERBOSE}
	Push `${_FILESTRING}`
	${CallArtificialFunction} GetBaseName_
	Pop ${_RESULT}
	!verbose pop
!macroend

!macro GetFileExtCall _FILESTRING _RESULT
	!verbose push
	!verbose ${_FILEFUNC_VERBOSE}
	Push `${_FILESTRING}`
	${CallArtificialFunction} GetFileExt_
	Pop ${_RESULT}
	!verbose pop
!macroend

!macro BannerTrimPathCall _PATH _LENGHT _RESULT
	!verbose push
	!verbose ${_FILEFUNC_VERBOSE}
	Push `${_PATH}`
	Push `${_LENGHT}`
	${CallArtificialFunction} BannerTrimPath_
	Pop ${_RESULT}
	!verbose pop
!macroend

!macro DirStateCall _PATH _RESULT
	!verbose push
	!verbose ${_FILEFUNC_VERBOSE}
	Push `${_PATH}`
	${CallArtificialFunction} DirState_
	Pop ${_RESULT}
	!verbose pop
!macroend

!macro RefreshShellIconsCall
	!verbose push
	!verbose ${_FILEFUNC_VERBOSE}
	${CallArtificialFunction} RefreshShellIcons_
	!verbose pop
!macroend

!define Locate `!insertmacro LocateCall`
!define un.Locate `!insertmacro LocateCall`

!macro Locate
!macroend

!macro un.Locate
!macroend

!macro Locate_
	!verbose push
	!verbose ${_FILEFUNC_VERBOSE}
		
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
	Push $7
	Push $8
	Push $9
	Push $R6
	Push $R7
	Push $R8
	Push $R9
	ClearErrors

	StrCpy $3 ''
	StrCpy $4 ''
	StrCpy $5 ''
	StrCpy $6 ''
	StrCpy $7 ''
	StrCpy $8 0
	StrCpy $R7 ''

	StrCpy $R9 $0 1 -1
	StrCmp $R9 '\' 0 +3
	StrCpy $0 $0 -1
	goto -3
	IfFileExists '$0\*.*' 0 FileFunc_Locate_error

	FileFunc_Locate_option:
	StrCpy $R9 $1 1
	StrCpy $1 $1 '' 1
	StrCmp $R9 ' ' -2
	StrCmp $R9 '' FileFunc_Locate_sizeset
	StrCmp $R9 '/' 0 -4
	StrCpy $9 -1
	IntOp $9 $9 + 1
	StrCpy $R9 $1 1 $9
	StrCmp $R9 '' +2
	StrCmp $R9 '/' 0 -3
	StrCpy $R8 $1 $9
	StrCpy $R8 $R8 '' 2
	StrCpy $R9 $R8 '' -1
	StrCmp $R9 ' ' 0 +3
	StrCpy $R8 $R8 -1
	goto -3
	StrCpy $R9 $1 2
	StrCpy $1 $1 '' $9

	StrCmp $R9 'L=' 0 FileFunc_Locate_mask
	StrCpy $3 $R8
	StrCmp $3 '' +6
	StrCmp $3 'FD' +5
	StrCmp $3 'F' +4
	StrCmp $3 'D' +3
	StrCmp $3 'DE' +2
	StrCmp $3 'FDE' 0 FileFunc_Locate_error
	goto FileFunc_Locate_option

	FileFunc_Locate_mask:
	StrCmp $R9 'M=' 0 FileFunc_Locate_size
	StrCpy $4 $R8
	goto FileFunc_Locate_option

	FileFunc_Locate_size:
	StrCmp $R9 'S=' 0 FileFunc_Locate_gotosubdir
	StrCpy $6 $R8
	goto FileFunc_Locate_option

	FileFunc_Locate_gotosubdir:
	StrCmp $R9 'G=' 0 FileFunc_Locate_banner
	StrCpy $7 $R8
	StrCmp $7 '' +3
	StrCmp $7 '1' +2
	StrCmp $7 '0' 0 FileFunc_Locate_error
	goto FileFunc_Locate_option

	FileFunc_Locate_banner:
	StrCmp $R9 'B=' 0 FileFunc_Locate_error
	StrCpy $R7 $R8
	StrCmp $R7 '' +3
	StrCmp $R7 '1' +2
	StrCmp $R7 '0' 0 FileFunc_Locate_error
	goto FileFunc_Locate_option

	FileFunc_Locate_sizeset:
	StrCmp $6 '' FileFunc_Locate_default
	StrCpy $9 0
	StrCpy $R9 $6 1 $9
	StrCmp $R9 '' +4
	StrCmp $R9 ':' +3
	IntOp $9 $9 + 1
	goto -4
	StrCpy $5 $6 $9
	IntOp $9 $9 + 1
	StrCpy $1 $6 1 -1
	StrCpy $6 $6 -1 $9
	StrCmp $5 '' +2
	IntOp $5 $5 + 0
	StrCmp $6 '' +2
	IntOp $6 $6 + 0

	StrCmp $1 'B' 0 +3
	StrCpy $1 1
	goto FileFunc_Locate_default
	StrCmp $1 'K' 0 +3
	StrCpy $1 1024
	goto FileFunc_Locate_default
	StrCmp $1 'M' 0 +3
	StrCpy $1 1048576
	goto FileFunc_Locate_default
	StrCmp $1 'G' 0 FileFunc_Locate_error
	StrCpy $1 1073741824

	FileFunc_Locate_default:
	StrCmp $3 '' 0 +2
	StrCpy $3 'FD'
	StrCmp $4 '' 0 +2
	StrCpy $4 '*.*'
	StrCmp $7 '' 0 +2
	StrCpy $7 '1'
	StrCmp $R7 '' 0 +2
	StrCpy $R7 '0'
	StrCpy $7 'G$7B$R7'

	StrCpy $8 1
	Push $0
	SetDetailsPrint textonly

	FileFunc_Locate_nextdir:
	IntOp $8 $8 - 1
	Pop $R8

	StrCpy $9 $7 2 2
	StrCmp $9 'B0' +3
	GetLabelAddress $9 FileFunc_Locate_findfirst
	goto call
	DetailPrint 'Search in: $R8'

	FileFunc_Locate_findfirst:
	FindFirst $0 $R7 '$R8\$4'
	IfErrors FileFunc_Locate_subdir
	StrCmp $R7 '.' 0 FileFunc_Locate_dir
	FindNext $0 $R7
	StrCmp $R7 '..' 0 FileFunc_Locate_dir
	FindNext $0 $R7
	IfErrors 0 FileFunc_Locate_dir
	FindClose $0
	goto FileFunc_Locate_subdir

	FileFunc_Locate_dir:
	IfFileExists '$R8\$R7\*.*' 0 FileFunc_Locate_file
	StrCpy $R6 ''
	StrCmp $3 'DE' +4
	StrCmp $3 'FDE' +3
	StrCmp $3 'FD' FileFunc_Locate_precall
	StrCmp $3 'F' FileFunc_Locate_findnext FileFunc_Locate_precall
	FindFirst $9 $R9 '$R8\$R7\*.*'
	StrCmp $R9 '.' 0 +4
	FindNext $9 $R9
	StrCmp $R9 '..' 0 +2
	FindNext $9 $R9
	FindClose $9
	IfErrors FileFunc_Locate_precall FileFunc_Locate_findnext

	FileFunc_Locate_file:
	StrCmp $3 'FDE' +3
	StrCmp $3 'FD' +2
	StrCmp $3 'F' 0 FileFunc_Locate_findnext
	StrCpy $R6 0
	StrCmp $5$6 '' FileFunc_Locate_precall
	FileOpen $9 '$R8\$R7' r
	IfErrors +3
	FileSeek $9 0 END $R6
	FileClose $9
	System::Int64Op $R6 / $1
	Pop $R6
	StrCmp $5 '' +2
	IntCmp $R6 $5 0 FileFunc_Locate_findnext
	StrCmp $6 '' +2
	IntCmp $R6 $6 0 0 FileFunc_Locate_findnext

	FileFunc_Locate_precall:
	StrCpy $9 0
	StrCpy $R9 '$R8\$R7'

	call:
	Push $0
	Push $1
	Push $2
	Push $3
	Push $4
	Push $5
	Push $6
	Push $7
	Push $8
	Push $9
	Push $R7
	Push $R8
	StrCmp $9 0 +4
	StrCpy $R6 ''
	StrCpy $R7 ''
	StrCpy $R9 ''
	Call $2
	Pop $R9
	Pop $R8
	Pop $R7
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

	IfErrors 0 +3
	FindClose $0
	goto FileFunc_Locate_error
	StrCmp $R9 'StopLocate' 0 +3
	FindClose $0
	goto FileFunc_Locate_clearstack
	goto $9

	FileFunc_Locate_findnext:
	FindNext $0 $R7
	IfErrors 0 FileFunc_Locate_dir
	FindClose $0

	FileFunc_Locate_subdir:
	StrCpy $9 $7 2
	StrCmp $9 'G0' FileFunc_Locate_end
	FindFirst $0 $R7 '$R8\*.*'
	StrCmp $R7 '.' 0 FileFunc_Locate_pushdir
	FindNext $0 $R7
	StrCmp $R7 '..' 0 FileFunc_Locate_pushdir
	FindNext $0 $R7
	IfErrors 0 FileFunc_Locate_pushdir
	FindClose $0
	StrCmp $8 0 FileFunc_Locate_end FileFunc_Locate_nextdir

	FileFunc_Locate_pushdir:
	IfFileExists '$R8\$R7\*.*' 0 +3
	Push '$R8\$R7'
	IntOp $8 $8 + 1
	FindNext $0 $R7
	IfErrors 0 FileFunc_Locate_pushdir
	FindClose $0
	StrCmp $8 0 FileFunc_Locate_end FileFunc_Locate_nextdir

	FileFunc_Locate_error:
	SetErrors

	FileFunc_Locate_clearstack:
	StrCmp $8 0 FileFunc_Locate_end
	IntOp $8 $8 - 1
	Pop $R8
	goto FileFunc_Locate_clearstack

	FileFunc_Locate_end:
	SetDetailsPrint both
	Pop $R9
	Pop $R8
	Pop $R7
	Pop $R6
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

!define GetSize `!insertmacro GetSizeCall`
!define un.GetSize `!insertmacro GetSizeCall`

!macro GetSize
!macroend

!macro un.GetSize
!macroend

!macro GetSize_
	!verbose push
	!verbose ${_FILEFUNC_VERBOSE}
	
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
	Push $8
	Push $9
	Push $R3
	Push $R4
	Push $R5
	Push $R6
	Push $R7
	Push $R8
	Push $R9
	ClearErrors

	StrCpy $R9 $0 1 -1
	StrCmp $R9 '\' 0 +3
	StrCpy $0 $0 -1
	goto -3
	IfFileExists '$0\*.*' 0 FileFunc_GetSize_error

	StrCpy $3 ''
	StrCpy $4 ''
	StrCpy $5 ''
	StrCpy $6 ''
	StrCpy $8 0
	StrCpy $R3 ''
	StrCpy $R4 ''
	StrCpy $R5 ''

	FileFunc_GetSize_option:
	StrCpy $R9 $1 1
	StrCpy $1 $1 '' 1
	StrCmp $R9 ' ' -2
	StrCmp $R9 '' FileFunc_GetSize_sizeset
	StrCmp $R9 '/' 0 -4

	StrCpy $9 -1
	IntOp $9 $9 + 1
	StrCpy $R9 $1 1 $9
	StrCmp $R9 '' +2
	StrCmp $R9 '/' 0 -3
	StrCpy $8 $1 $9
	StrCpy $8 $8 '' 2
	StrCpy $R9 $8 '' -1
	StrCmp $R9 ' ' 0 +3
	StrCpy $8 $8 -1
	goto -3
	StrCpy $R9 $1 2
	StrCpy $1 $1 '' $9

	StrCmp $R9 'M=' 0 FileFunc_GetSize_size
	StrCpy $4 $8
	goto FileFunc_GetSize_option

	FileFunc_GetSize_size:
	StrCmp $R9 'S=' 0 FileFunc_GetSize_gotosubdir
	StrCpy $6 $8
	goto FileFunc_GetSize_option

	FileFunc_GetSize_gotosubdir:
	StrCmp $R9 'G=' 0 FileFunc_GetSize_error
	StrCpy $7 $8
	StrCmp $7 '' +3
	StrCmp $7 '1' +2
	StrCmp $7 '0' 0 FileFunc_GetSize_error
	goto FileFunc_GetSize_option

	FileFunc_GetSize_sizeset:
	StrCmp $6 '' FileFunc_GetSize_default
	StrCpy $9 0
	StrCpy $R9 $6 1 $9
	StrCmp $R9 '' +4
	StrCmp $R9 ':' +3
	IntOp $9 $9 + 1
	goto -4
	StrCpy $5 $6 $9
	IntOp $9 $9 + 1
	StrCpy $1 $6 1 -1
	StrCpy $6 $6 -1 $9
	StrCmp $5 '' +2
	IntOp $5 $5 + 0
	StrCmp $6 '' +2
	IntOp $6 $6 + 0

	StrCmp $1 'B' 0 +4
	StrCpy $1 1
	StrCpy $2 bytes
	goto FileFunc_GetSize_default
	StrCmp $1 'K' 0 +4
	StrCpy $1 1024
	StrCpy $2 Kb
	goto FileFunc_GetSize_default
	StrCmp $1 'M' 0 +4
	StrCpy $1 1048576
	StrCpy $2 Mb
	goto FileFunc_GetSize_default
	StrCmp $1 'G' 0 FileFunc_GetSize_error
	StrCpy $1 1073741824
	StrCpy $2 Gb

	FileFunc_GetSize_default:
	StrCmp $4 '' 0 +2
	StrCpy $4 '*.*'
	StrCmp $7 '' 0 +2
	StrCpy $7 '1'

	StrCpy $8 1
	Push $0
	SetDetailsPrint textonly

	FileFunc_GetSize_nextdir:
	IntOp $8 $8 - 1
	Pop $R8
	FindFirst $0 $R7 '$R8\$4'
	IfErrors FileFunc_GetSize_show
	StrCmp $R7 '.' 0 FileFunc_GetSize_dir
	FindNext $0 $R7
	StrCmp $R7 '..' 0 FileFunc_GetSize_dir
	FindNext $0 $R7
	IfErrors 0 FileFunc_GetSize_dir
	FindClose $0
	goto FileFunc_GetSize_show

	FileFunc_GetSize_dir:
	IfFileExists '$R8\$R7\*.*' 0 FileFunc_GetSize_file
	IntOp $R5 $R5 + 1
	goto FileFunc_GetSize_findnext

	FileFunc_GetSize_file:
	StrCpy $R6 0
	StrCmp $5$6 '' 0 +3
	IntOp $R4 $R4 + 1
	goto FileFunc_GetSize_findnext
	FileOpen $9 '$R8\$R7' r
	IfErrors +3
	FileSeek $9 0 END $R6
	FileClose $9
	StrCmp $5 '' +2
	IntCmp $R6 $5 0 FileFunc_GetSize_findnext
	StrCmp $6 '' +2
	IntCmp $R6 $6 0 0 FileFunc_GetSize_findnext
	IntOp $R4 $R4 + 1
	System::Int64Op $R3 + $R6
	Pop $R3

	FileFunc_GetSize_findnext:
	FindNext $0 $R7
	IfErrors 0 FileFunc_GetSize_dir
	FindClose $0

	FileFunc_GetSize_show:
	StrCmp $5$6 '' FileFunc_GetSize_nosize
	System::Int64Op $R3 / $1
	Pop $9
	DetailPrint 'Size:$9 $2  Files:$R4  Folders:$R5'
	goto FileFunc_GetSize_subdir
	FileFunc_GetSize_nosize:
	DetailPrint 'Files:$R4  Folders:$R5'

	FileFunc_GetSize_subdir:
	StrCmp $7 0 FileFunc_GetSize_preend
	FindFirst $0 $R7 '$R8\*.*'
	StrCmp $R7 '.' 0 FileFunc_GetSize_pushdir
	FindNext $0 $R7
	StrCmp $R7 '..' 0 FileFunc_GetSize_pushdir
	FindNext $0 $R7
	IfErrors 0 FileFunc_GetSize_pushdir
	FindClose $0
	StrCmp $8 0 FileFunc_GetSize_preend FileFunc_GetSize_nextdir

	FileFunc_GetSize_pushdir:
	IfFileExists '$R8\$R7\*.*' 0 +3
	Push '$R8\$R7'
	IntOp $8 $8 + 1
	FindNext $0 $R7
	IfErrors 0 FileFunc_GetSize_pushdir
	FindClose $0
	StrCmp $8 0 FileFunc_GetSize_preend FileFunc_GetSize_nextdir

	FileFunc_GetSize_preend:
	StrCmp $R3 '' FileFunc_GetSize_nosizeend
	System::Int64Op $R3 / $1
	Pop $R3
	FileFunc_GetSize_nosizeend:
	StrCpy $2 $R4
	StrCpy $1 $R5
	StrCpy $0 $R3
	goto FileFunc_GetSize_end

	FileFunc_GetSize_error:
	SetErrors
	StrCpy $0 ''
	StrCpy $1 ''
	StrCpy $2 ''

	FileFunc_GetSize_end:
	SetDetailsPrint both
	Pop $R9
	Pop $R8
	Pop $R7
	Pop $R6
	Pop $R5
	Pop $R4
	Pop $R3
	Pop $9
	Pop $8
	Pop $7
	Pop $6
	Pop $5
	Pop $4
	Pop $3
	Exch $2
	Exch
	Exch $1
	Exch 2
	Exch $0

	!verbose pop
!macroend

!define DriveSpace `!insertmacro DriveSpaceCall`
!define un.DriveSpace `!insertmacro DriveSpaceCall`

!macro DriveSpace
!macroend

!macro un.DriveSpace
!macroend

!macro DriveSpace_
	!verbose push
	!verbose ${_FILEFUNC_VERBOSE}
	
	Exch $1
	Exch
	Exch $0
	Exch
	Push $2
	Push $3
	Push $4
	Push $5
	Push $6
	ClearErrors

	StrCpy $2 $0 1 -1
	StrCmp $2 '\' 0 +3
	StrCpy $0 $0 -1
	goto -3
	IfFileExists '$0\NUL' 0 FileFunc_DriveSpace_error

	StrCpy $5 ''
	StrCpy $6 ''

	FileFunc_DriveSpace_option:
	StrCpy $2 $1 1
	StrCpy $1 $1 '' 1
	StrCmp $2 ' ' -2
	StrCmp $2 '' FileFunc_DriveSpace_default
	StrCmp $2 '/' 0 -4
	StrCpy $3 -1
	IntOp $3 $3 + 1
	StrCpy $2 $1 1 $3
	StrCmp $2 '' +2
	StrCmp $2 '/' 0 -3
	StrCpy $4 $1 $3
	StrCpy $4 $4 '' 2
	StrCpy $2 $4 1 -1
	StrCmp $2 ' ' 0 +3
	StrCpy $4 $4 -1
	goto -3
	StrCpy $2 $1 2
	StrCpy $1 $1 '' $3

	StrCmp $2 'D=' 0 FileFunc_DriveSpace_unit
	StrCpy $5 $4
	StrCmp $5 '' +4
	StrCmp $5 'T' +3
	StrCmp $5 'O' +2
	StrCmp $5 'F' 0 FileFunc_DriveSpace_error
	goto FileFunc_DriveSpace_option

	FileFunc_DriveSpace_unit:
	StrCmp $2 'S=' 0 FileFunc_DriveSpace_error
	StrCpy $6 $4
	goto FileFunc_DriveSpace_option

	FileFunc_DriveSpace_default:
	StrCmp $5 '' 0 +2
	StrCpy $5 'T'
	StrCmp $6 '' 0 +3
	StrCpy $6 '1'
	goto FileFunc_DriveSpace_getspace

	StrCmp $6 'B' 0 +3
	StrCpy $6 1
	goto FileFunc_DriveSpace_getspace
	StrCmp $6 'K' 0 +3
	StrCpy $6 1024
	goto FileFunc_DriveSpace_getspace
	StrCmp $6 'M' 0 +3
	StrCpy $6 1048576
	goto FileFunc_DriveSpace_getspace
	StrCmp $6 'G' 0 FileFunc_DriveSpace_error
	StrCpy $6 1073741824

	FileFunc_DriveSpace_getspace:
	System::Call 'kernel32::GetDiskFreeSpaceExA(t, *l, *l, *l)i(r0,.r2,.r3,.)'

	StrCmp $5 T 0 +3
	StrCpy $0 $3
	goto FileFunc_DriveSpace_getsize
	StrCmp $5 O 0 +4
	System::Int64Op $3 - $2
	Pop $0
	goto FileFunc_DriveSpace_getsize
	StrCmp $5 F 0 +2
	StrCpy $0 $2

	FileFunc_DriveSpace_getsize:
	System::Int64Op $0 / $6
	Pop $0
	goto FileFunc_DriveSpace_end

	FileFunc_DriveSpace_error:
	SetErrors
	StrCpy $0 ''

	FileFunc_DriveSpace_end:
	Pop $6
	Pop $5
	Pop $4
	Pop $3
	Pop $2
	Pop $1
	Exch $0

	!verbose pop
!macroend

!define GetDrives `!insertmacro GetDrivesCall`
!define un.GetDrives `!insertmacro GetDrivesCall`

!macro GetDrives
!macroend

!macro un.GetDrives
!macroend

!macro GetDrives_
	!verbose push
	!verbose ${_FILEFUNC_VERBOSE}
	
	Exch $1
	Exch
	Exch $0
	Exch
	Push $2
	Push $3
	Push $4
	Push $5
	Push $6
	Push $8
	Push $9

	System::Alloc 1024
	Pop $2
	System::Call 'kernel32::GetLogicalDriveStringsA(i,i) i(1024, r2)'

	StrCmp $0 ALL FileFunc_GetDrives_drivestring
	StrCmp $0 '' 0 FileFunc_GetDrives_typeset
	StrCpy $0 ALL
	goto FileFunc_GetDrives_drivestring

	FileFunc_GetDrives_typeset:
	StrCpy $6 -1
	IntOp $6 $6 + 1
	StrCpy $8 $0 1 $6
	StrCmp $8$0 '' FileFunc_GetDrives_enumex
	StrCmp $8 '' +2
	StrCmp $8 '+' 0 -4
	StrCpy $8 $0 $6
	IntOp $6 $6 + 1
	StrCpy $0 $0 '' $6

	StrCmp $8 'FDD' 0 +3
	StrCpy $6 2
	goto FileFunc_GetDrives_drivestring
	StrCmp $8 'HDD' 0 +3
	StrCpy $6 3
	goto FileFunc_GetDrives_drivestring
	StrCmp $8 'NET' 0 +3
	StrCpy $6 4
	goto FileFunc_GetDrives_drivestring
	StrCmp $8 'CDROM' 0 +3
	StrCpy $6 5
	goto FileFunc_GetDrives_drivestring
	StrCmp $8 'RAM' 0 FileFunc_GetDrives_typeset
	StrCpy $6 6

	FileFunc_GetDrives_drivestring:
	StrCpy $3 $2

	FileFunc_GetDrives_enumok:
	System::Call 'kernel32::lstrlenA(t) i(i r3) .r4'
	StrCmp $4$0 '0ALL' FileFunc_GetDrives_enumex
	StrCmp $4 0 FileFunc_GetDrives_typeset
	System::Call 'kernel32::GetDriveTypeA(t) i(i r3) .r5'

	StrCmp $0 ALL +2
	StrCmp $5 $6 FileFunc_GetDrives_letter FileFunc_GetDrives_enumnext
	StrCmp $5 2 0 +3
	StrCpy $8 FDD
	goto FileFunc_GetDrives_letter
	StrCmp $5 3 0 +3
	StrCpy $8 HDD
	goto FileFunc_GetDrives_letter
	StrCmp $5 4 0 +3
	StrCpy $8 NET
	goto FileFunc_GetDrives_letter
	StrCmp $5 5 0 +3
	StrCpy $8 CDROM
	goto FileFunc_GetDrives_letter
	StrCmp $5 6 0 FileFunc_GetDrives_enumex
	StrCpy $8 RAM

	FileFunc_GetDrives_letter:
	System::Call '*$3(&t1024 .r9)'

	Push $0
	Push $1
	Push $2
	Push $3
	Push $4
	Push $5
	Push $6
	Push $8
	Call $1
	Pop $9
	Pop $8
	Pop $6
	Pop $5
	Pop $4
	Pop $3
	Pop $2
	Pop $1
	Pop $0
	StrCmp $9 'StopGetDrives' FileFunc_GetDrives_enumex

	FileFunc_GetDrives_enumnext:
	IntOp $3 $3 + $4
	IntOp $3 $3 + 1
	goto FileFunc_GetDrives_enumok

	FileFunc_GetDrives_enumex:
	System::Free $2

	Pop $9
	Pop $8
	Pop $6
	Pop $5
	Pop $4
	Pop $3
	Pop $2
	Pop $1
	Pop $0
	
	!verbose pop
!macroend

!define GetTime `!insertmacro GetTimeCall`
!define un.GetTime `!insertmacro GetTimeCall`

!macro GetTime
!macroend

!macro un.GetTime
!macroend

!macro GetTime_
	!verbose push
	!verbose ${_FILEFUNC_VERBOSE}
	
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
	ClearErrors

	StrCmp $1 'L' FileFunc_GetTime_gettime
	StrCmp $1 'A' FileFunc_GetTime_getfile
	StrCmp $1 'C' FileFunc_GetTime_getfile
	StrCmp $1 'M' FileFunc_GetTime_getfile
	StrCmp $1 'LS' FileFunc_GetTime_gettime
	StrCmp $1 'AS' FileFunc_GetTime_getfile
	StrCmp $1 'CS' FileFunc_GetTime_getfile
	StrCmp $1 'MS' FileFunc_GetTime_getfile
	goto FileFunc_GetTime_error

	FileFunc_GetTime_getfile:
	IfFileExists $0 0 FileFunc_GetTime_error
	System::Call '*(i,l,l,l,i,i,i,i,&t260,&t14) i .r6'
	System::Call 'kernel32::FindFirstFileA(t,i)i(r0,r6) .r2'
	System::Call 'kernel32::FindClose(i)i(r2)'

	FileFunc_GetTime_gettime:
	System::Call '*(&i2,&i2,&i2,&i2,&i2,&i2,&i2,&i2) i .r7'
	StrCmp $1 'L' 0 FileFunc_GetTime_systemtime
	System::Call 'kernel32::GetLocalTime(i)i(r7)'
	goto FileFunc_GetTime_convert
	FileFunc_GetTime_systemtime:
	StrCmp $1 'LS' 0 FileFunc_GetTime_filetime
	System::Call 'kernel32::GetSystemTime(i)i(r7)'
	goto FileFunc_GetTime_convert

	FileFunc_GetTime_filetime:
	System::Call '*$6(i,l,l,l,i,i,i,i,&t260,&t14)i(,.r4,.r3,.r2)'
	System::Free $6
	StrCmp $1 'A' 0 +3
	StrCpy $2 $3
	goto FileFunc_GetTime_tolocal
	StrCmp $1 'C' 0 +3
	StrCpy $2 $4
	goto FileFunc_GetTime_tolocal
	StrCmp $1 'M' FileFunc_GetTime_tolocal

	StrCmp $1 'AS' FileFunc_GetTime_tosystem
	StrCmp $1 'CS' 0 +3
	StrCpy $3 $4
	goto FileFunc_GetTime_tosystem
	StrCmp $1 'MS' 0 +3
	StrCpy $3 $2
	goto FileFunc_GetTime_tosystem

	FileFunc_GetTime_tolocal:
	System::Call 'kernel32::FileTimeToLocalFileTime(*l,*l)i(r2,.r3)'
	FileFunc_GetTime_tosystem:
	System::Call 'kernel32::FileTimeToSystemTime(*l,i)i(r3,r7)'

	FileFunc_GetTime_convert:
	System::Call '*$7(&i2,&i2,&i2,&i2,&i2,&i2,&i2,&i2)i(.r5,.r6,.r4,.r0,.r3,.r2,.r1,)'
	System::Free $7

	IntCmp $0 9 0 0 +2
	StrCpy $0 '0$0'
	IntCmp $1 9 0 0 +2
	StrCpy $1 '0$1'
	IntCmp $2 9 0 0 +2
	StrCpy $2 '0$2'
	IntCmp $6 9 0 0 +2
	StrCpy $6 '0$6'

	StrCmp $4 0 0 +3
	StrCpy $4 Sunday
	goto FileFunc_GetTime_end
	StrCmp $4 1 0 +3
	StrCpy $4 Monday
	goto FileFunc_GetTime_end
	StrCmp $4 2 0 +3
	StrCpy $4 Tuesday
	goto FileFunc_GetTime_end
	StrCmp $4 3 0 +3
	StrCpy $4 Wednesday
	goto FileFunc_GetTime_end
	StrCmp $4 4 0 +3
	StrCpy $4 Thursday
	goto FileFunc_GetTime_end
	StrCmp $4 5 0 +3
	StrCpy $4 Friday
	goto FileFunc_GetTime_end
	StrCmp $4 6 0 FileFunc_GetTime_error
	StrCpy $4 Saturday
	goto FileFunc_GetTime_end

	FileFunc_GetTime_error:
	SetErrors
	StrCpy $0 ''
	StrCpy $1 ''
	StrCpy $2 ''
	StrCpy $3 ''
	StrCpy $4 ''
	StrCpy $5 ''
	StrCpy $6 ''

	FileFunc_GetTime_end:
	Pop $7
	Exch $6
	Exch
	Exch $5
	Exch 2
	Exch $4
	Exch 3
	Exch $3
	Exch 4
	Exch $2
	Exch 5
	Exch $1
	Exch 6
	Exch $0

	!verbose pop
!macroend

!define GetFileAttributes `!insertmacro GetFileAttributesCall`
!define un.GetFileAttributes `!insertmacro GetFileAttributesCall`

!macro GetFileAttributes
!macroend

!macro un.GetFileAttributes
!macroend

!macro GetFileAttributes_
	!verbose push
	!verbose ${_FILEFUNC_VERBOSE}
	
	Exch $1
	Exch
	Exch $0
	Exch
	Push $2
	Push $3
	Push $4
	Push $5

	System::Call 'kernel32::GetFileAttributes(t r0)i .r2'
	StrCmp $2 -1 FileFunc_GetFileAttributes_error
	StrCpy $3 ''

	IntOp $0 $2 & 0x4000
	IntCmp $0 0 +2
	StrCpy $3 'ENCRYPTED|'

	IntOp $0 $2 & 0x2000
	IntCmp $0 0 +2
	StrCpy $3 'NOT_CONTENT_INDEXED|$3'

	IntOp $0 $2 & 0x1000
	IntCmp $0 0 +2
	StrCpy $3 'OFFLINE|$3'

	IntOp $0 $2 & 0x0800
	IntCmp $0 0 +2
	StrCpy $3 'COMPRESSED|$3'

	IntOp $0 $2 & 0x0400
	IntCmp $0 0 +2
	StrCpy $3 'REPARSE_POINT|$3'

	IntOp $0 $2 & 0x0200
	IntCmp $0 0 +2
	StrCpy $3 'SPARSE_FILE|$3'

	IntOp $0 $2 & 0x0100
	IntCmp $0 0 +2
	StrCpy $3 'TEMPORARY|$3'

	IntOp $0 $2 & 0x0080
	IntCmp $0 0 +2
	StrCpy $3 'NORMAL|$3'

	IntOp $0 $2 & 0x0040
	IntCmp $0 0 +2
	StrCpy $3 'DEVICE|$3'

	IntOp $0 $2 & 0x0020
	IntCmp $0 0 +2
	StrCpy $3 'ARCHIVE|$3'

	IntOp $0 $2 & 0x0010
	IntCmp $0 0 +2
	StrCpy $3 'DIRECTORY|$3'

	IntOp $0 $2 & 0x0004
	IntCmp $0 0 +2
	StrCpy $3 'SYSTEM|$3'

	IntOp $0 $2 & 0x0002
	IntCmp $0 0 +2
	StrCpy $3 'HIDDEN|$3'

	IntOp $0 $2 & 0x0001
	IntCmp $0 0 +2
	StrCpy $3 'READONLY|$3'

	StrCpy $0 $3 -1
	StrCmp $1 '' FileFunc_GetFileAttributes_end
	StrCmp $1 'ALL' FileFunc_GetFileAttributes_end

	FileFunc_GetFileAttributes_attrcmp:
	StrCpy $5 0
	IntOp $5 $5 + 1
	StrCpy $4 $1 1 $5
	StrCmp $4 '' +2
	StrCmp $4 '|'  0 -3
	StrCpy $2 $1 $5
	IntOp $5 $5 + 1
	StrCpy $1 $1 '' $5
	StrLen $3 $2
	StrCpy $5 -1
	IntOp $5 $5 + 1
	StrCpy $4 $0 $3 $5
	StrCmp $4 '' FileFunc_GetFileAttributes_notfound
	StrCmp $4 $2 0 -3
	StrCmp $1 '' 0 FileFunc_GetFileAttributes_attrcmp
	StrCpy $0 1
	goto FileFunc_GetFileAttributes_end

	FileFunc_GetFileAttributes_notfound:
	StrCpy $0 0
	goto FileFunc_GetFileAttributes_end

	FileFunc_GetFileAttributes_error:
	SetErrors
	StrCpy $0 ''

	FileFunc_GetFileAttributes_end:
	Pop $5
	Pop $4
	Pop $3
	Pop $2
	Pop $1
	Exch $0
		
	!verbose pop
!macroend

!define GetFileVersion `!insertmacro GetFileVersionCall`
!define un.GetFileVersion `!insertmacro GetFileVersionCall`

!macro GetFileVersion
!macroend

!macro un.GetFileVersion
!macroend

!macro GetFileVersion_
	!verbose push
	!verbose ${_FILEFUNC_VERBOSE}
	
	Exch $0
	Push $1
	Push $2
	Push $3
	Push $4
	Push $5
	Push $6
	ClearErrors

	GetDllVersion '$0' $1 $2
	IfErrors FileFunc_GetFileVersion_error
	IntOp $3 $1 >> 16
	IntOp $3 $3 & 0x0000FFFF
	IntOp $4 $1 & 0x0000FFFF
	IntOp $5 $2 >> 16
	IntOp $5 $5 & 0x0000FFFF
	IntOp $6 $2 & 0x0000FFFF
	StrCpy $0 '$3.$4.$5.$6'
	goto FileFunc_GetFileVersion_end

	FileFunc_GetFileVersion_error:
	SetErrors
	StrCpy $0 ''

	FileFunc_GetFileVersion_end:
	Pop $6
	Pop $5
	Pop $4
	Pop $3
	Pop $2
	Pop $1
	Exch $0

	!verbose pop
!macroend

!define GetExeName `!insertmacro GetExeNameCall`
!define un.GetExeName `!insertmacro GetExeNameCall`

!macro GetExeName
!macroend

!macro un.GetExeName
!macroend

!macro GetExeName_
	!verbose push
	!verbose ${_FILEFUNC_VERBOSE}
	
	Push $0
	Push $1
	Push $2
	System::Call 'kernel32::GetModuleFileNameA(i 0, t .r0, i 1024)'
	System::Call 'kernel32::GetLongPathNameA(t r0, t .r1, i 1024)i .r2'
	StrCmp $2 error +2
	StrCpy $0 $1
	Pop $2
	Pop $1
	Exch $0

	!verbose pop
!macroend

!define GetExePath `!insertmacro GetExePathCall`
!define un.GetExePath `!insertmacro GetExePathCall`

!macro GetExePath
!macroend

!macro un.GetExePath
!macroend

!macro GetExePath_
	!verbose push
	!verbose ${_FILEFUNC_VERBOSE}
	
	Push $0
	Push $1
	Push $2
	StrCpy $0 $EXEDIR
	System::Call 'kernel32::GetLongPathNameA(t r0, t .r1, i 1024)i .r2'
	StrCmp $2 error +2
	StrCpy $0 $1
	Pop $2
	Pop $1
	Exch $0
	
	!verbose pop
!macroend

!define GetParameters `!insertmacro GetParametersCall`
!define un.GetParameters `!insertmacro GetParametersCall`

!macro GetParameters
!macroend

!macro un.GetParameters
!macroend

!macro GetParameters_
	!verbose push
	!verbose ${_FILEFUNC_VERBOSE}
	
	;cmdline-check
	StrCmp $CMDLINE "" 0 +3
	Push ""
	Return

	;vars
	Push $0  ;tmp
	Push $1  ;length
	Push $2  ;parameter offset
	Push $3  ;separator

	;length/offset
	StrLen $1 $CMDLINE
	StrCpy $2 2  ;start with third character

	;separator
	StrCpy $3 $CMDLINE 1 ;first character
	StrCmp $3 '"' +2
	StrCpy $3 ' '

	FileFunc_GetParameters_token:  ;finding second separator
	IntCmp $2 $1 FileFunc_GetParameters_strip 0 FileFunc_GetParameters_strip
	StrCpy $0 $CMDLINE 1 $2
	IntOp $2 $2 + 1
	StrCmp $3 $0 0 FileFunc_GetParameters_token

	FileFunc_GetParameters_strip:  ;strip white space
	IntCmp $2 $1 FileFunc_GetParameters_copy 0 FileFunc_GetParameters_copy
	StrCpy $0 $CMDLINE 1 $2
	StrCmp $0 ' ' 0 FileFunc_GetParameters_copy
	IntOp $2 $2 + 1
	Goto FileFunc_GetParameters_strip

	FileFunc_GetParameters_copy:
	StrCpy $0 $CMDLINE "" $2

	;strip white spaces from end
	FileFunc_GetParameters_rstrip:
	StrCpy $1 $0 1 -1
	StrCmp $1 ' ' 0 FileFunc_GetParameters_done
	StrCpy $0 $0 -1
	Goto FileFunc_GetParameters_rstrip

	FileFunc_GetParameters_done:
	Pop $3
	Pop $2
	Pop $1
	Exch $0

	!verbose pop
!macroend

!macro GetOptionsBody _FILEFUNC_S

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
	ClearErrors

	StrCpy $2 $1 '' 1
	StrCpy $1 $1 1
	StrLen $3 $2
	StrCpy $7 0

	FileFunc_GetOptions${_FILEFUNC_S}_begin:
	StrCpy $4 -1
	StrCpy $6 ''

	FileFunc_GetOptions${_FILEFUNC_S}_quote:
	IntOp $4 $4 + 1
	StrCpy $5 $0 1 $4
	StrCmp${_FILEFUNC_S} $5$7 '0' FileFunc_GetOptions${_FILEFUNC_S}_notfound
	StrCmp${_FILEFUNC_S} $5 '' FileFunc_GetOptions${_FILEFUNC_S}_trimright
	StrCmp${_FILEFUNC_S} $5 '"' 0 +7
	StrCmp${_FILEFUNC_S} $6 '' 0 +3
	StrCpy $6 '"'
	goto FileFunc_GetOptions${_FILEFUNC_S}_quote
	StrCmp${_FILEFUNC_S} $6 '"' 0 +3
	StrCpy $6 ''
	goto FileFunc_GetOptions${_FILEFUNC_S}_quote
	StrCmp${_FILEFUNC_S} $5 `'` 0 +7
	StrCmp${_FILEFUNC_S} $6 `` 0 +3
	StrCpy $6 `'`
	goto FileFunc_GetOptions${_FILEFUNC_S}_quote
	StrCmp${_FILEFUNC_S} $6 `'` 0 +3
	StrCpy $6 ``
	goto FileFunc_GetOptions${_FILEFUNC_S}_quote
	StrCmp${_FILEFUNC_S} $5 '`' 0 +7
	StrCmp${_FILEFUNC_S} $6 '' 0 +3
	StrCpy $6 '`'
	goto FileFunc_GetOptions${_FILEFUNC_S}_quote
	StrCmp${_FILEFUNC_S} $6 '`' 0 +3
	StrCpy $6 ''
	goto FileFunc_GetOptions${_FILEFUNC_S}_quote
	StrCmp${_FILEFUNC_S} $6 '"' FileFunc_GetOptions${_FILEFUNC_S}_quote
	StrCmp${_FILEFUNC_S} $6 `'` FileFunc_GetOptions${_FILEFUNC_S}_quote
	StrCmp${_FILEFUNC_S} $6 '`' FileFunc_GetOptions${_FILEFUNC_S}_quote
	StrCmp${_FILEFUNC_S} $5 $1 0 FileFunc_GetOptions${_FILEFUNC_S}_quote
	StrCmp${_FILEFUNC_S} $7 0 FileFunc_GetOptions${_FILEFUNC_S}_trimleft FileFunc_GetOptions${_FILEFUNC_S}_trimright

	FileFunc_GetOptions${_FILEFUNC_S}_trimleft:
	IntOp $4 $4 + 1
	StrCpy $5 $0 $3 $4
	StrCmp${_FILEFUNC_S} $5 '' FileFunc_GetOptions${_FILEFUNC_S}_notfound
	StrCmp${_FILEFUNC_S} $5 $2 0 FileFunc_GetOptions${_FILEFUNC_S}_quote
	IntOp $4 $4 + $3
	StrCpy $0 $0 '' $4
	StrCpy $4 $0 1
	StrCmp${_FILEFUNC_S} $4 ' ' 0 +3
	StrCpy $0 $0 '' 1
	goto -3
	StrCpy $7 1
	goto FileFunc_GetOptions${_FILEFUNC_S}_begin

	FileFunc_GetOptions${_FILEFUNC_S}_trimright:
	StrCpy $0 $0 $4
	StrCpy $4 $0 1 -1
	StrCmp${_FILEFUNC_S} $4 ' ' 0 +3
	StrCpy $0 $0 -1
	goto -3
	StrCpy $3 $0 1
	StrCpy $4 $0 1 -1
	StrCmp${_FILEFUNC_S} $3 $4 0 FileFunc_GetOptions${_FILEFUNC_S}_end
	StrCmp${_FILEFUNC_S} $3 '"' +3
	StrCmp${_FILEFUNC_S} $3 `'` +2
	StrCmp${_FILEFUNC_S} $3 '`' 0 FileFunc_GetOptions${_FILEFUNC_S}_end
	StrCpy $0 $0 -1 1
	goto FileFunc_GetOptions${_FILEFUNC_S}_end

	FileFunc_GetOptions${_FILEFUNC_S}_notfound:
	SetErrors
	StrCpy $0 ''

	FileFunc_GetOptions${_FILEFUNC_S}_end:
	Pop $7
	Pop $6
	Pop $5
	Pop $4
	Pop $3
	Pop $2
	Pop $1
	Exch $0

!macroend

!define GetOptions `!insertmacro GetOptionsCall`
!define un.GetOptions `!insertmacro GetOptionsCall`

!macro GetOptions
!macroend

!macro un.GetOptions
!macroend

!macro GetOptions_
	!verbose push
	!verbose ${_FILEFUNC_VERBOSE}

	!insertmacro GetOptionsBody ''

	!verbose pop
!macroend

!define GetOptionsS `!insertmacro GetOptionsSCall`
!define un.GetOptionsS `!insertmacro GetOptionsSCall`

!macro GetOptionsS
!macroend

!macro un.GetOptionsS
!macroend

!macro GetOptionsS_
	!verbose push
	!verbose ${_FILEFUNC_VERBOSE}

	!insertmacro GetOptionsBody 'S'

	!verbose pop
!macroend

!define GetRoot `!insertmacro GetRootCall`
!define un.GetRoot `!insertmacro GetRootCall`

!macro GetRoot
!macroend

!macro un.GetRoot
!macroend

!macro GetRoot_
	!verbose push
	!verbose ${_FILEFUNC_VERBOSE}
	
	Exch $0
	Push $1
	Push $2
	Push $3

	StrCpy $1 $0 2
	StrCmp $1 '\\' FileFunc_GetRoot_UNC
	StrCpy $2 $1 1 1
	StrCmp $2 ':' 0 FileFunc_GetRoot_empty
	StrCpy $0 $1
	goto FileFunc_GetRoot_end

	FileFunc_GetRoot_UNC:
	StrCpy $2 1
	StrCpy $3 ''

	FileFunc_GetRoot_loop:
	IntOp $2 $2 + 1
	StrCpy $1 $0 1 $2
	StrCmp $1$3 '' FileFunc_GetRoot_empty
	StrCmp $1 '' +5
	StrCmp $1 '\' 0 FileFunc_GetRoot_loop
	StrCmp $3 '1' +3
	StrCpy $3 '1'
	goto FileFunc_GetRoot_loop
	StrCpy $0 $0 $2
	StrCpy $2 $0 1 -1
	StrCmp $2 '\' 0 FileFunc_GetRoot_end

	FileFunc_GetRoot_empty:
	StrCpy $0 ''

	FileFunc_GetRoot_end:
	Pop $3
	Pop $2
	Pop $1
	Exch $0

	!verbose pop
!macroend

!define GetParent `!insertmacro GetParentCall`
!define un.GetParent `!insertmacro GetParentCall`

!macro GetParent
!macroend

!macro un.GetParent
!macroend

!macro GetParent_
	!verbose push
	!verbose ${_FILEFUNC_VERBOSE}
	
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

	!verbose pop
!macroend

!define GetFileName `!insertmacro GetFileNameCall`
!define un.GetFileName `!insertmacro GetFileNameCall`

!macro GetFileName
!macroend

!macro un.GetFileName
!macroend

!macro GetFileName_
	!verbose push
	!verbose ${_FILEFUNC_VERBOSE}
	
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
	StrCmp $2 '' FileFunc_GetFileName_end
	StrCmp $2 '\' 0 -3
	IntOp $1 $1 + 1
	StrCpy $0 $0 '' $1

	FileFunc_GetFileName_end:
	Pop $2
	Pop $1
	Exch $0

	!verbose pop
!macroend

!define GetBaseName `!insertmacro GetBaseNameCall`
!define un.GetBaseName `!insertmacro GetBaseNameCall`

!macro GetBaseName
!macroend

!macro un.GetBaseName
!macroend

!macro GetBaseName_
	!verbose push
	!verbose ${_FILEFUNC_VERBOSE}
	
	Exch $0
	Push $1
	Push $2
	Push $3

	StrCpy $1 0
	StrCpy $3 ''

	FileFunc_GetBaseName_loop:
	IntOp $1 $1 - 1
	StrCpy $2 $0 1 $1
	StrCmp $2 '' FileFunc_GetBaseName_trimpath
	StrCmp $2 '\' FileFunc_GetBaseName_trimpath
	StrCmp $3 'noext' FileFunc_GetBaseName_loop
	StrCmp $2 '.' 0 FileFunc_GetBaseName_loop
	StrCpy $0 $0 $1
	StrCpy $3 'noext'
	StrCpy $1 0
	goto FileFunc_GetBaseName_loop

	FileFunc_GetBaseName_trimpath:
	StrCmp $1 -1 FileFunc_GetBaseName_empty
	IntOp $1 $1 + 1
	StrCpy $0 $0 '' $1
	goto FileFunc_GetBaseName_end

	FileFunc_GetBaseName_empty:
	StrCpy $0 ''

	FileFunc_GetBaseName_end:
	Pop $3
	Pop $2
	Pop $1
	Exch $0

	!verbose pop
!macroend

!define GetFileExt `!insertmacro GetFileExtCall`
!define un.GetFileExt `!insertmacro GetFileExtCall`

!macro GetFileExt
!macroend

!macro un.GetFileExt
!macroend

!macro GetFileExt_
	!verbose push
	!verbose ${_FILEFUNC_VERBOSE}
	
	Exch $0
	Push $1
	Push $2

	StrCpy $1 0

	FileFunc_GetFileExt_loop:
	IntOp $1 $1 - 1
	StrCpy $2 $0 1 $1
	StrCmp $2 '' FileFunc_GetFileExt_empty
	StrCmp $2 '\' FileFunc_GetFileExt_empty
	StrCmp $2 '.' 0 FileFunc_GetFileExt_loop

	StrCmp $1 -1 FileFunc_GetFileExt_empty
	IntOp $1 $1 + 1
	StrCpy $0 $0 '' $1
	goto FileFunc_GetFileExt_end

	FileFunc_GetFileExt_empty:
	StrCpy $0 ''

	FileFunc_GetFileExt_end:
	Pop $2
	Pop $1
	Exch $0

	!verbose pop
!macroend

!define BannerTrimPath `!insertmacro BannerTrimPathCall`
!define un.BannerTrimPath `!insertmacro BannerTrimPathCall`

!macro BannerTrimPath
!macroend

!macro un.BannerTrimPath
!macroend

!macro BannerTrimPath_
	!verbose push
	!verbose ${_FILEFUNC_VERBOSE}
	
	Exch $1
	Exch
	Exch $0
	Exch
	Push $2
	Push $3
	Push $4

	StrCpy $3 $1 1 -1
	IntOp $1 $1 + 0
	StrLen $2 $0
	IntCmp $2 $1 FileFunc_BannerTrimPath_end FileFunc_BannerTrimPath_end
	IntOp $1 $1 - 3
	IntCmp $1 0 FileFunc_BannerTrimPath_empty FileFunc_BannerTrimPath_empty
	StrCmp $3 'A' FileFunc_BannerTrimPath_A-trim
	StrCmp $3 'B' FileFunc_BannerTrimPath_B-trim
	StrCmp $3 'C' FileFunc_BannerTrimPath_C-trim
	StrCmp $3 'D' FileFunc_BannerTrimPath_D-trim

	FileFunc_BannerTrimPath_A-trim:
	StrCpy $3 $0 1 1
	StrCpy $2 0
	StrCmp $3 ':' 0 +2
	IntOp $2 $2 + 2

	FileFunc_BannerTrimPath_loopleft:
	IntOp $2 $2 + 1
	StrCpy $3 $0 1 $2
	StrCmp $2 $1 FileFunc_BannerTrimPath_C-trim
	StrCmp $3 '\' 0 FileFunc_BannerTrimPath_loopleft
	StrCpy $3 $0 $2
	IntOp $2 $2 - $1
	IntCmp $2 0 FileFunc_BannerTrimPath_B-trim 0 FileFunc_BannerTrimPath_B-trim

	FileFunc_BannerTrimPath_loopright:
	IntOp $2 $2 + 1
	StrCpy $4 $0 1 $2
	StrCmp $2 0 FileFunc_BannerTrimPath_B-trim
	StrCmp $4 '\' 0 FileFunc_BannerTrimPath_loopright
	StrCpy $4 $0 '' $2
	StrCpy $0 '$3\...$4'
	goto FileFunc_BannerTrimPath_end

	FileFunc_BannerTrimPath_B-trim:
	StrCpy $2 $1
	IntOp $2 $2 - 1
	StrCmp $2 -1 FileFunc_BannerTrimPath_C-trim
	StrCpy $3 $0 1 $2
	StrCmp $3 '\' 0 -3
	StrCpy $0 $0 $2
	StrCpy $0 '$0\...'
	goto FileFunc_BannerTrimPath_end

	FileFunc_BannerTrimPath_C-trim:
	StrCpy $0 $0 $1
	StrCpy $0 '$0...'
	goto FileFunc_BannerTrimPath_end

	FileFunc_BannerTrimPath_D-trim:
	StrCpy $3 -1
	IntOp $3 $3 - 1
	StrCmp $3 -$2 FileFunc_BannerTrimPath_C-trim
	StrCpy $4 $0 1 $3
	StrCmp $4 '\' 0 -3
	StrCpy $4 $0 '' $3
	IntOp $3 $1 + $3
	IntCmp $3 2 FileFunc_BannerTrimPath_C-trim FileFunc_BannerTrimPath_C-trim
	StrCpy $0 $0 $3
	StrCpy $0 '$0...$4'
	goto FileFunc_BannerTrimPath_end

	FileFunc_BannerTrimPath_empty:
	StrCpy $0 ''

	FileFunc_BannerTrimPath_end:
	Pop $4
	Pop $3
	Pop $2
	Pop $1
	Exch $0

	!verbose pop
!macroend

!define DirState `!insertmacro DirStateCall`
!define un.DirState `!insertmacro DirStateCall`

!macro DirState
!macroend

!macro un.DirState
!macroend

!macro DirState_
	!verbose push
	!verbose ${_FILEFUNC_VERBOSE}
	
	Exch $0
	Push $1
	ClearErrors

	FindFirst $1 $0 '$0\*.*'
	IfErrors 0 +3
	StrCpy $0 -1
	goto FileFunc_DirState_end
	StrCmp $0 '.' 0 +4
	FindNext $1 $0
	StrCmp $0 '..' 0 +2
	FindNext $1 $0
	FindClose $1
	IfErrors 0 +3
	StrCpy $0 0
	goto FileFunc_DirState_end
	StrCpy $0 1

	FileFunc_DirState_end:
	Pop $1
	Exch $0

	!verbose pop
!macroend

!define RefreshShellIcons `!insertmacro RefreshShellIconsCall`
!define un.RefreshShellIcons `!insertmacro RefreshShellIconsCall`

!macro RefreshShellIcons
!macroend

!macro un.RefreshShellIcons
!macroend

!macro RefreshShellIcons_
	!verbose push
	!verbose ${_FILEFUNC_VERBOSE}
	
	System::Call 'shell32::SHChangeNotify(i 0x08000000, i 0, i 0, i 0)'

	!verbose pop
!macroend

!endif
