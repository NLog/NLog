/*

InstallOptions.nsh
Macros and conversion functions for InstallOptions

*/

!ifndef ___NSIS__INSTALL_OPTIONS__NSH___
!define ___NSIS__INSTALL_OPTIONS__NSH___

!include LogicLib.nsh

!macro INSTALLOPTIONS_FUNCTION_READ_CONVERT
  !insertmacro INSTALLOPTIONS_FUNCTION_IO2NSIS ""
!macroend

!macro INSTALLOPTIONS_UNFUNCTION_READ_CONVERT
  !insertmacro INSTALLOPTIONS_FUNCTION_IO2NSIS un.
!macroend

!macro INSTALLOPTIONS_FUNCTION_WRITE_CONVERT
  !insertmacro INSTALLOPTIONS_FUNCTION_NSIS2IO ""
!macroend

!macro INSTALLOPTIONS_UNFUNCTION_WRITE_CONVERT
  !insertmacro INSTALLOPTIONS_FUNCTION_NSIS2IO un.
!macroend

!macro INSTALLOPTIONS_FUNCTION_NSIS2IO UNINSTALLER_FUNCPREFIX

  ; Convert an NSIS string to a form suitable for use by InstallOptions
  ; Usage:
  ;   Push <NSIS-string>
  ;   Call Nsis2Io
  ;   Pop <IO-string>

  Function ${UNINSTALLER_FUNCPREFIX}Nsis2Io

    Exch $0 ; The source
    Push $1 ; The output
    Push $2 ; Temporary char
    Push $3 ; Length
    Push $4 ; Loop index
    StrCpy $1 "" ; Initialise the output

    StrLen $3 $0
    IntOp $3 $3 - 1

    ${For} $4 0 $3
      StrCpy $2 $0 1 $4
      ${If}     $2 == '\'
        StrCpy $2 '\\'
      ${ElseIf} $2 == '$\r'
        StrCpy $2 '\r'
      ${ElseIf} $2 == '$\n'
        StrCpy $2 '\n'
      ${ElseIf} $2 == '$\t'
        StrCpy $2 '\t'
      ${EndIf}
      StrCpy $1 $1$2
    ${Next}

    StrCpy $0 $1
    Pop $4
    Pop $3
    Pop $2
    Pop $1
    Exch $0

  FunctionEnd

!macroend

!macro INSTALLOPTIONS_FUNCTION_IO2NSIS UNINSTALLER_FUNCPREFIX

  ; Convert an InstallOptions string to a form suitable for use by NSIS
  ; Usage:
  ;   Push <IO-string>
  ;   Call Io2Nsis
  ;   Pop <NSIS-string>

  Function ${UNINSTALLER_FUNCPREFIX}Io2Nsis

    Exch $0 ; The source
    Push $1 ; The output
    Push $2 ; Temporary char
    Push $3 ; Length
    Push $4 ; Loop index
    StrCpy $1 "" ; Initialise the output

    StrLen $3 $0
    IntOp $3 $3 - 1

    ${For} $4 0 $3
      StrCpy $2 $0 2 $4
      ${If}     $2 == '\\'
        StrCpy $2 '\'
        IntOp $4 $4 + 1
      ${ElseIf} $2 == '\r'
        StrCpy $2 '$\r'
        IntOp $4 $4 + 1
      ${ElseIf} $2 == '\n'
        StrCpy $2 '$\n'
        IntOp $4 $4 + 1
      ${ElseIf} $2 == '\t'
        StrCpy $2 '$\t'
        IntOp $4 $4 + 1
      ${EndIf}
      StrCpy $2 $2 1
      StrCpy $1 $1$2
    ${Next}

    StrCpy $0 $1
    Pop $4
    Pop $3
    Pop $2
    Pop $1
    Exch $0

  FunctionEnd

!macroend

!macro INSTALLOPTIONS_EXTRACT FILE

  InitPluginsDir
  File "/oname=$PLUGINSDIR\${FILE}" "${FILE}"
  !insertmacro INSTALLOPTIONS_WRITE "${FILE}" "Settings" "RTL" "$(^RTL)"

  !verbose pop

!macroend

!macro INSTALLOPTIONS_EXTRACT_AS FILE FILENAME

  InitPluginsDir
  File "/oname=$PLUGINSDIR\${FILENAME}" "${FILE}"
  !insertmacro INSTALLOPTIONS_WRITE "${FILENAME}" "Settings" "RTL" "$(^RTL)"

!macroend

!macro INSTALLOPTIONS_DISPLAY FILE

  Push $0

  InstallOptions::dialog "$PLUGINSDIR\${FILE}"
  Pop $0

  Pop $0

!macroend

!macro INSTALLOPTIONS_DISPLAY_RETURN FILE

  InstallOptions::dialog "$PLUGINSDIR\${FILE}"

!macroend

!macro INSTALLOPTIONS_INITDIALOG FILE

  InstallOptions::initDialog "$PLUGINSDIR\${FILE}"

!macroend

!macro INSTALLOPTIONS_SHOW

  Push $0

  InstallOptions::show
  Pop $0

  Pop $0

!macroend

!macro INSTALLOPTIONS_SHOW_RETURN

  InstallOptions::show

!macroend

!macro INSTALLOPTIONS_READ VAR FILE SECTION KEY

  ReadIniStr ${VAR} "$PLUGINSDIR\${FILE}" "${SECTION}" "${KEY}"

!macroend

!macro INSTALLOPTIONS_WRITE FILE SECTION KEY VALUE

  WriteIniStr "$PLUGINSDIR\${FILE}" "${SECTION}" "${KEY}" "${VALUE}"

!macroend

!macro INSTALLOPTIONS_READ_CONVERT VAR FILE SECTION KEY

  ReadIniStr ${VAR} "$PLUGINSDIR\${FILE}" "${SECTION}" "${KEY}"
  Push ${VAR}
  Call Io2Nsis
  Pop ${VAR}

!macroend

!macro INSTALLOPTIONS_READ_UNCONVERT VAR FILE SECTION KEY

  ReadIniStr ${VAR} "$PLUGINSDIR\${FILE}" "${SECTION}" "${KEY}"
  Push ${VAR}
  Call un.Io2Nsis
  Pop ${VAR}

!macroend

!macro INSTALLOPTIONS_WRITE_CONVERT FILE SECTION KEY VALUE

  Push $0
  StrCpy $0 "${VALUE}"
  Push $0
  Call Nsis2Io
  Pop $0
  
  WriteIniStr "$PLUGINSDIR\${FILE}" "${SECTION}" "${KEY}" $0

  Pop $0

!macroend
  
!macro INSTALLOPTIONS_WRITE_UNCONVERT FILE SECTION KEY VALUE

  Push $0
  StrCpy $0 "${VALUE}"
  Push $0
  Call un.Nsis2Io
  Pop $0
  
  WriteIniStr "$PLUGINSDIR\${FILE}" "${SECTION}" "${KEY}" $0

  Pop $0

!macroend

!endif # ___NSIS__INSTALL_OPTIONS__NSH___
