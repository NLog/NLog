/*

NSIS Modern User Interface
Uninstall confirmation page

*/

;--------------------------------
;Page interface settings and variables

!macro MUI_UNCONFIRMPAGE_INTERFACE

  !ifndef MUI_UNCONFIRMPAGE_INTERFACE
    !define MUI_UNCONFIRMPAGE_INTERFACE
    Var mui.UnConfirmPage
    
    Var mui.UnConfirmPage.Text
    Var mui.UnConfirmPage.DirectoryText
    Var mui.UnConfirmPage.Directory    
  !endif

!macroend


;--------------------------------
;Page declaration

!macro MUI_PAGEDECLARATION_CONFIRM

  !insertmacro MUI_SET MUI_UNCONFIRMPAGE ""
  !insertmacro MUI_UNCONFIRMPAGE_INTERFACE  

  !insertmacro MUI_DEFAULT MUI_UNCONFIRMPAGE_TEXT_TOP ""
  !insertmacro MUI_DEFAULT MUI_UNCONFIRMPAGE_TEXT_LOCATION ""

  PageEx un.uninstConfirm

    PageCallbacks un.mui.ConfirmPre_${MUI_UNIQUEID} un.mui.ConfirmShow_${MUI_UNIQUEID} un.mui.ConfirmLeave_${MUI_UNIQUEID}

    Caption " "

    UninstallText "${MUI_UNCONFIRMPAGE_TEXT_TOP}" "${MUI_UNCONFIRMPAGE_TEXT_LOCATION}"

  PageExEnd

  !insertmacro MUI_UNFUNCTION_CONFIRMPAGE un.mui.ConfirmPre_${MUI_UNIQUEID} un.mui.ConfirmShow_${MUI_UNIQUEID} un.mui.ConfirmLeave_${MUI_UNIQUEID}

  !insertmacro MUI_UNSET MUI_UNCONFIRMPAGE_TEXT_TOP
  !insertmacro MUI_UNSET MUI_UNCONFIRMPAGE_TEXT_LOCATION

!macroend

!macro MUI_UNPAGE_CONFIRM

  !verbose push
  !verbose ${MUI_VERBOSE}

  !insertmacro MUI_UNPAGE_INIT
  !insertmacro MUI_PAGEDECLARATION_CONFIRM
  
  !verbose pop

!macroend


;--------------------------------
;Page functions

!macro MUI_UNFUNCTION_CONFIRMPAGE PRE SHOW LEAVE

  Function "${PRE}"

   !insertmacro MUI_PAGE_FUNCTION_CUSTOM PRE
   !insertmacro MUI_HEADER_TEXT_PAGE $(MUI_UNTEXT_CONFIRM_TITLE) $(MUI_UNTEXT_CONFIRM_SUBTITLE)

  FunctionEnd

  Function "${SHOW}"

    ;Get controls handles
    FindWindow $mui.UnConfirmPage "#32770" "" $HWNDPARENT
    GetDlgItem $mui.UnConfirmPage.Text $mui.UnConfirmPage 1006
    GetDlgItem $mui.UnConfirmPage.DirectoryText $mui.UnConfirmPage 1029
    GetDlgItem $mui.UnConfirmPage.Directory $mui.UnConfirmPage 1000

    !insertmacro MUI_PAGE_FUNCTION_CUSTOM SHOW

  FunctionEnd

  Function "${LEAVE}"

    !insertmacro MUI_PAGE_FUNCTION_CUSTOM LEAVE

  FunctionEnd

!macroend
