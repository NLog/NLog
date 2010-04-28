/*

NSIS Modern User Interface
Deprecated code - display warnings

*/

;--------------------------------
;InstallOptions

!define INSTALLOPTIONS_ERROR "MUI_INSTALLOPTIONS_* macros are no longer a part of MUI2. Include InstallOptions.nsh and use INSTALLOPTIONS_* macros instead. It is also recommended to upgrade to nsDialogs."

!macro MUI_INSTALLOPTIONS_EXTRACT FILE

  !error "${INSTALLOPTIONS_ERROR}"

!macroend

!macro MUI_INSTALLOPTIONS_EXTRACT_AS FILE FILENAME

  !error "${INSTALLOPTIONS_ERROR}"

!macroend

!macro MUI_INSTALLOPTIONS_DISPLAY FILE

  !error "${INSTALLOPTIONS_ERROR}"

!macroend

!macro MUI_INSTALLOPTIONS_DISPLAY_RETURN FILE

  !error "${INSTALLOPTIONS_ERROR}"

!macroend

!macro MUI_INSTALLOPTIONS_INITDIALOG FILE

  !error "${INSTALLOPTIONS_ERROR}"

!macroend

!macro MUI_INSTALLOPTIONS_SHOW

  !error "${INSTALLOPTIONS_ERROR}"

!macroend

!macro MUI_INSTALLOPTIONS_SHOW_RETURN

  !error "${INSTALLOPTIONS_ERROR}"

!macroend

!macro MUI_INSTALLOPTIONS_READ VAR FILE SECTION KEY

  !error "${INSTALLOPTIONS_ERROR}"

!macroend

!macro MUI_INSTALLOPTIONS_WRITE FILE SECTION KEY VALUE

  !error "${INSTALLOPTIONS_ERROR}"

!macroend

!macro MUI_RESERVEFILE_INSTALLOPTIONS

  !define MUI_DOLLAR "$"
  !error "MUI_RESERVEFILE_INSTALLOPTIONS is no longer supported as InstallOptions is no longer used by MUI2. Instead, use ReserveFile '${MUI_DOLLAR}{NSISDIR}\Plugins\InstallOptions.dll'. It is also recommended to upgrade to nsDialogs."

!macroend
