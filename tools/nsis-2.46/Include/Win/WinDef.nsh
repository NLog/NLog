!ifndef __WIN_WINDEF__INC
!define __WIN_WINDEF__INC
!verbose push
!verbose 3
!ifndef __WIN_NOINC_WINDEF


!ifndef MAX_PATH
!define MAX_PATH 260
!endif
#define NULL 0


!macro _Win_MINMAX _intcmp _j1 _j2 _outvar _a _b
${_intcmp} "${_a}" "${_b}" ${_j1} ${_j1} ${_j2}
StrCpy ${_outvar} "${_a}"
goto +2
StrCpy ${_outvar} "${_b}"
!macroend
!ifndef __WIN_MS_NOMINMAX & min & max & min_u & max_u
!define min "!insertmacro _Win_MINMAX IntCmp +1 +3 "
!define max "!insertmacro _Win_MINMAX IntCmp +3 +1 "
!define min_u "!insertmacro _Win_MINMAX IntCmpU +1 +3 "
!define max_u "!insertmacro _Win_MINMAX IntCmpU +3 +1 "
!endif

!macro _Win_LOBYTE _outvar _in
IntOp ${_outvar} "${_in}" & 0xFF
!macroend
!define LOBYTE "!insertmacro _Win_LOBYTE "

!macro _Win_HIBYTE _outvar _in
IntOp ${_outvar} "${_in}" >> 8
${LOBYTE} ${_outvar} ${_outvar}
!macroend
!define HIBYTE "!insertmacro _Win_HIBYTE "

!macro _Win_LOWORD _outvar _in
IntOp ${_outvar} "${_in}" & 0xFFFF
!macroend
!define LOWORD "!insertmacro _Win_LOWORD "

!macro _Win_HIWORD _outvar _in
IntOp ${outvar} "${_in}" >> 16 ;sign extended :(
${LOWORD} ${_outvar} ${outvar} ;make sure we strip off the upper word
!macroend
!define HIWORD "!insertmacro _Win_HIWORD "

!macro _Win_MAKEWORD _outvar _tmpvar _lo _hi
${LOBYTE} ${_outvar} "${_hi}"
${LOBYTE} ${_tmpvar} "${_lo}"
IntOp ${_outvar} ${_outvar} << 8
IntOp ${_outvar} ${_outvar} | ${_tmpvar}
!macroend
!define MAKEWORD "!insertmacro _Win_MAKEWORD "

!macro _Win_MAKELONG32 _outvar _tmpvar _wlo _whi
${LOWORD} ${_outvar} "${_wlo}"
IntOp ${_tmpvar} "${_whi}" << 16
IntOp ${_outvar} ${_outvar} | ${_tmpvar}
!macroend
!define MAKELONG "!insertmacro _Win_MAKELONG32 "
!if "${__WIN_PTRSIZE}" <= 4
!define MAKEWPARAM "${MAKELONG}"
!define MAKELPARAM "${MAKELONG}"
!define MAKELRESULT "${MAKELONG}"
!else
!error "Missing 64bit imp!"
!endif


!endif /* __WIN_NOINC_WINDEF */
!verbose pop
!endif /* __WIN_WINDEF__INC */