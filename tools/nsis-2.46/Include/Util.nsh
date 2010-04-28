; ---------------------
;       Util.nsh
; ---------------------
;
; Voodoo macros to make end-user usage easier. This may be documented someday.

!verbose push
!verbose 3

!ifndef ___UTIL__NSH___
!define ___UTIL__NSH___

# see WinVer.nsh and *Func.nsh for usage examples
!macro CallArtificialFunction NAME
  !ifndef __UNINSTALL__
    !define CallArtificialFunction_TYPE inst
  !else
    !define CallArtificialFunction_TYPE uninst
  !endif
  Call :.${NAME}${CallArtificialFunction_TYPE}
  !ifndef ${NAME}${CallArtificialFunction_TYPE}_DEFINED
    Goto ${NAME}${CallArtificialFunction_TYPE}_DONE
    !define ${NAME}${CallArtificialFunction_TYPE}_DEFINED
    .${NAME}${CallArtificialFunction_TYPE}:
      !insertmacro ${NAME}
    Return
    ${NAME}${CallArtificialFunction_TYPE}_DONE:
  !endif
  !undef CallArtificialFunction_TYPE
!macroend
!define CallArtificialFunction `!insertmacro CallArtificialFunction`

# for usage of artificial functions inside artificial functions
# macro recursion is prohibited
!macro CallArtificialFunction2 NAME
  !ifndef __UNINSTALL__
    !define CallArtificialFunction2_TYPE inst
  !else
    !define CallArtificialFunction2_TYPE uninst
  !endif
  Call :.${NAME}${CallArtificialFunction2_TYPE}
  !ifndef ${NAME}${CallArtificialFunction2_TYPE}_DEFINED
    Goto ${NAME}${CallArtificialFunction2_TYPE}_DONE
    !define ${NAME}${CallArtificialFunction2_TYPE}_DEFINED
    .${NAME}${CallArtificialFunction2_TYPE}:
      !insertmacro ${NAME}
    Return
    ${NAME}${CallArtificialFunction2_TYPE}_DONE:
  !endif
  !undef CallArtificialFunction2_TYPE
!macroend
!define CallArtificialFunction2 `!insertmacro CallArtificialFunction2`

!endif # !___UTIL__NSH___

!verbose pop
