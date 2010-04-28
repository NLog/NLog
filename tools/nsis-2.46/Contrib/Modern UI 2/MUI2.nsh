/*

NSIS Modern User Interface - Version 2.0
Copyright 2002-2009 Joost Verburg

*/

!echo "NSIS Modern User Interface version 2.0 - Copyright 2002-2009 Joost Verburg"

;--------------------------------

!ifndef MUI_INCLUDED
!define MUI_INCLUDED

!define MUI_SYSVERSION "2.0"

!verbose push

!ifndef MUI_VERBOSE
  !define MUI_VERBOSE 3
!endif

!verbose ${MUI_VERBOSE}

!addincludedir "${NSISDIR}\Contrib\Modern UI 2"

;--------------------------------
;Header files required by MUI

!include WinMessages.nsh
!include LogicLib.nsh
!include nsDialogs.nsh
!include LangFile.nsh


;--------------------------------
;Macros for compile-time defines

!macro MUI_DEFAULT SYMBOL CONTENT

  ;Define symbol if not yet defined
  ;For setting default values

  !ifndef "${SYMBOL}"
    !define "${SYMBOL}" "${CONTENT}"
  !endif

!macroend

!macro MUI_SET SYMBOL CONTENT

  ;Define symbol and undefine if neccesary
  
  !insertmacro MUI_UNSET "${SYMBOL}"
  !define "${SYMBOL}" "${CONTENT}"

!macroend

!macro MUI_UNSET SYMBOL

  ;Undefine symbol if defined

  !ifdef "${SYMBOL}"
    !undef "${SYMBOL}"
  !endif

!macroend


;--------------------------------
;MUI interface

!include "Deprecated.nsh"
!include "Interface.nsh"
!include "Localization.nsh"
!include "Pages.nsh"


;--------------------------------
;Pages

!include "Pages\Components.nsh"
!include "Pages\Directory.nsh"
!include "Pages\Finish.nsh"
!include "Pages\InstallFiles.nsh"
!include "Pages\License.nsh"
!include "Pages\StartMenu.nsh"
!include "Pages\UninstallConfirm.nsh"
!include "Pages\Welcome.nsh"


;--------------------------------
;Insert MUI code in script

!macro MUI_INSERT

  !ifndef MUI_INSERT
    !define MUI_INSERT

    ;This macro is included when the first language file is included,
    ;after the pages.

    ;Interface settings
    !insertmacro MUI_INTERFACE

    ;Interface functions - Installer
    !insertmacro MUI_FUNCTION_GUIINIT
    !insertmacro MUI_FUNCTION_ABORTWARNING

    ;Interface functions - Uninstaller
    !ifdef MUI_UNINSTALLER
      !insertmacro MUI_UNFUNCTION_GUIINIT
      !insertmacro MUI_FUNCTION_UNABORTWARNING
    !endif

  !endif

!macroend

!endif

!verbose pop
