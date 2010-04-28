/*

NSIS Modern User Interface
InstallFiles page

*/

;--------------------------------
;Page interface settings and variables

!macro MUI_INSTFILESPAGE_INTERFACE

  !ifndef MUI_INSTFILESYPAGE_INTERFACE
    !define MUI_INSTFILESYPAGE_INTERFACE
    
    !insertmacro MUI_DEFAULT MUI_INSTFILESPAGE_COLORS "/windows"
    !insertmacro MUI_DEFAULT MUI_INSTFILESPAGE_PROGRESSBAR "smooth"    
    
    Var mui.InstFilesPage
    
    Var mui.InstFilesPage.Text
    Var mui.InstFilesPage.ProgressBar
    Var mui.InstFilesPage.ShowLogButton
    Var mui.InstFilesPage.Log
    
    ;Apply settings
    InstallColors ${MUI_INSTFILESPAGE_COLORS}
    InstProgressFlags ${MUI_INSTFILESPAGE_PROGRESSBAR}
    SubCaption 4 " "
    UninstallSubCaption 2 " "   
  !endif

!macroend


;--------------------------------
;Page declaration

!macro MUI_PAGEDECLARATION_INSTFILES

  !insertmacro MUI_SET MUI_${MUI_PAGE_UNINSTALLER_PREFIX}INSTFILESPAGE ""
  !insertmacro MUI_INSTFILESPAGE_INTERFACE
  
  PageEx ${MUI_PAGE_UNINSTALLER_FUNCPREFIX}instfiles

    PageCallbacks ${MUI_PAGE_UNINSTALLER_FUNCPREFIX}mui.InstFilesPre_${MUI_UNIQUEID} ${MUI_PAGE_UNINSTALLER_FUNCPREFIX}mui.InstFilesShow_${MUI_UNIQUEID} ${MUI_PAGE_UNINSTALLER_FUNCPREFIX}mui.InstFilesLeave_${MUI_UNIQUEID}

    Caption " "

  PageExEnd

  !insertmacro MUI_FUNCTION_INSTFILESPAGE ${MUI_PAGE_UNINSTALLER_FUNCPREFIX}mui.InstFilesPre_${MUI_UNIQUEID} ${MUI_PAGE_UNINSTALLER_FUNCPREFIX}mui.InstFilesShow_${MUI_UNIQUEID} ${MUI_PAGE_UNINSTALLER_FUNCPREFIX}mui.InstFilesLeave_${MUI_UNIQUEID}

  !insertmacro MUI_UNSET MUI_INSTFILESPAGE_FINISHHEADER_TEXT
  !insertmacro MUI_UNSET MUI_INSTFILESPAGE_FINISHHEADER_SUBTEXT
  !insertmacro MUI_UNSET MUI_INSTFILESPAGE_ABORTWARNING_TEXT
  !insertmacro MUI_UNSET MUI_INSTFILESPAGE_ABORTWARNING_SUBTEXT

!macroend

!macro MUI_PAGE_INSTFILES

  !verbose push
  !verbose ${MUI_VERBOSE}

  !insertmacro MUI_PAGE_INIT
  !insertmacro MUI_PAGEDECLARATION_INSTFILES

  !verbose pop

!macroend

!macro MUI_UNPAGE_INSTFILES

  !verbose push
  !verbose ${MUI_VERBOSE}

  !insertmacro MUI_UNPAGE_INIT
  !insertmacro MUI_PAGEDECLARATION_INSTFILES

  !verbose pop

!macroend


;--------------------------------
;Page functions

!macro MUI_FUNCTION_INSTFILESPAGE PRE SHOW LEAVE

  Function "${PRE}"

    !insertmacro MUI_PAGE_FUNCTION_CUSTOM PRE
    !insertmacro MUI_HEADER_TEXT_PAGE $(MUI_${MUI_PAGE_UNINSTALLER_PREFIX}TEXT_${MUI_PAGE_UNINSTALLER_PREFIX}INSTALLING_TITLE) $(MUI_${MUI_PAGE_UNINSTALLER_PREFIX}TEXT_${MUI_PAGE_UNINSTALLER_PREFIX}INSTALLING_SUBTITLE)

  FunctionEnd

  Function "${SHOW}"

    ;Get controls handles
    FindWindow $mui.InstFilesPage "#32770" "" $HWNDPARENT
    GetDlgItem $mui.InstFilesPage.Text $mui.InstFilesPage 1006
    GetDlgItem $mui.InstFilesPage.ProgressBar $mui.InstFilesPage 1004
    GetDlgItem $mui.InstFilesPage.ShowLogButton $mui.InstFilesPage 1027    
    GetDlgItem $mui.InstFilesPage.Log $mui.InstFilesPage 1016

    !insertmacro MUI_PAGE_FUNCTION_CUSTOM SHOW

  FunctionEnd

  Function "${LEAVE}"

    !insertmacro MUI_PAGE_FUNCTION_CUSTOM LEAVE

    ;Set text on completed page header

    IfAbort mui.endheader_abort

      !ifdef MUI_INSTFILESPAGE_FINISHHEADER_TEXT & MUI_INSTFILESPAGE_FINISHHEADER_SUBTEXT
        !insertmacro MUI_HEADER_TEXT "${MUI_INSTFILESPAGE_FINISHHEADER_TEXT}" "${MUI_INSTFILESPAGE_FINISHHEADER_SUBTEXT}"
      !else ifdef MUI_INSTFILESPAGE_FINISHHEADER_TEXT
        !insertmacro MUI_HEADER_TEXT "${MUI_INSTFILESPAGE_FINISHHEADER_TEXT}" "$(MUI_${MUI_PAGE_UNINSTALLER_PREFIX}TEXT_FINISH_SUBTITLE)"
      !else ifdef MUI_INSTFILESPAGE_FINISHHEADER_SUBTEXT
        !insertmacro MUI_HEADER_TEXT "$(MUI_${MUI_PAGE_UNINSTALLER_PREFIX}TEXT_FINISH_TITLE)" "${MUI_INSTFILESPAGE_FINISHHEADER_SUBTEXT}"
      !else
        !insertmacro MUI_HEADER_TEXT "$(MUI_${MUI_PAGE_UNINSTALLER_PREFIX}TEXT_FINISH_TITLE)" "$(MUI_${MUI_PAGE_UNINSTALLER_PREFIX}TEXT_FINISH_SUBTITLE)"
      !endif

    Goto mui.endheader_done

    mui.endheader_abort:

      !ifdef MUI_INSTFILESPAGE_ABORTHEADER_TEXT & MUI_INSTFILESPAGE_ABORTHEADER_SUBTEXT
        !insertmacro MUI_HEADER_TEXT "${MUI_INSTFILESPAGE_ABORTHEADER_TEXT}" "${MUI_INSTFILESPAGE_ABORTHEADER_SUBTEXT}"
      !else ifdef MUI_INSTFILESPAGE_ABORTHEADER_TEXT
        !insertmacro MUI_HEADER_TEXT "${MUI_INSTFILESPAGE_ABORTHEADER_TEXT}" "$(MUI_${MUI_PAGE_UNINSTALLER_PREFIX}TEXT_ABORT_SUBTITLE)"
      !else ifdef MUI_INSTFILESPAGE_ABORTHEADER_SUBTEXT
        !insertmacro MUI_HEADER_TEXT "$(MUI_${MUI_PAGE_UNINSTALLER_PREFIX}TEXT_ABORT_TITLE)" "${MUI_INSTFILESPAGE_ABORTHEADER_SUBTEXT}"
      !else
        !insertmacro MUI_HEADER_TEXT "$(MUI_${MUI_PAGE_UNINSTALLER_PREFIX}TEXT_ABORT_TITLE)" "$(MUI_${MUI_PAGE_UNINSTALLER_PREFIX}TEXT_ABORT_SUBTITLE)"
      !endif

    mui.endheader_done:

      !insertmacro MUI_LANGDLL_SAVELANGUAGE

  FunctionEnd

!macroend
