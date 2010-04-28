/*

NSIS Modern User Interface
License page

*/

;--------------------------------
;Page interface settings and variables

!macro MUI_LICENSEPAGE_INTERFACE

  !ifndef MUI_LICENSEPAGE_INTERFACE
    !define MUI_LICENSEPAGE_INTERFACE
    Var mui.LicensePage
    
    Var mui.Licensepage.TopText
    Var mui.Licensepage.Text
    Var mui.Licensepage.LicenseText
    
    !insertmacro MUI_DEFAULT MUI_LICENSEPAGE_BGCOLOR "/windows"
    
    ;Apply settings
    LicenseBkColor "${MUI_LICENSEPAGE_BGCOLOR}"
  !endif

!macroend


;--------------------------------
;Page declaration

!macro MUI_PAGEDECLARATION_LICENSE LICENSEDATA

  !insertmacro MUI_SET MUI_${MUI_PAGE_UNINSTALLER_PREFIX}LICENSEPAGE ""
  !insertmacro MUI_LICENSEPAGE_INTERFACE  

  !insertmacro MUI_DEFAULT MUI_LICENSEPAGE_TEXT_TOP "$(MUI_INNERTEXT_LICENSE_TOP)"
  !insertmacro MUI_DEFAULT MUI_LICENSEPAGE_BUTTON ""
  !insertmacro MUI_DEFAULT MUI_LICENSEPAGE_CHECKBOX_TEXT ""
  !insertmacro MUI_DEFAULT MUI_LICENSEPAGE_RADIOBUTTONS_TEXT_ACCEPT ""
  !insertmacro MUI_DEFAULT MUI_LICENSEPAGE_RADIOBUTTONS_TEXT_DECLINE ""

  PageEx ${MUI_PAGE_UNINSTALLER_FUNCPREFIX}license

    PageCallbacks ${MUI_PAGE_UNINSTALLER_FUNCPREFIX}mui.LicensePre_${MUI_UNIQUEID} ${MUI_PAGE_UNINSTALLER_FUNCPREFIX}mui.LicenseShow_${MUI_UNIQUEID} ${MUI_PAGE_UNINSTALLER_FUNCPREFIX}mui.LicenseLeave_${MUI_UNIQUEID}

    Caption " "

    LicenseData "${LICENSEDATA}"

    !ifndef MUI_LICENSEPAGE_TEXT_BOTTOM
      !ifndef MUI_LICENSEPAGE_CHECKBOX & MUI_LICENSEPAGE_RADIOBUTTONS
        LicenseText "$(MUI_${MUI_PAGE_UNINSTALLER_PREFIX}INNERTEXT_LICENSE_BOTTOM)" "${MUI_LICENSEPAGE_BUTTON}"
      !else ifdef MUI_LICENSEPAGE_CHECKBOX
        LicenseText "$(MUI_${MUI_PAGE_UNINSTALLER_PREFIX}INNERTEXT_LICENSE_BOTTOM_CHECKBOX)" "${MUI_LICENSEPAGE_BUTTON}"
      !else
        LicenseText "$(MUI_${MUI_PAGE_UNINSTALLER_PREFIX}INNERTEXT_LICENSE_BOTTOM_RADIOBUTTONS)" "${MUI_LICENSEPAGE_BUTTON}"
      !endif
    !else
      LicenseText "${MUI_LICENSEPAGE_TEXT_BOTTOM}" "${MUI_LICENSEPAGE_BUTTON}"
    !endif

    !ifdef MUI_LICENSEPAGE_CHECKBOX
      LicenseForceSelection checkbox "${MUI_LICENSEPAGE_CHECKBOX_TEXT}"
    !else ifdef MUI_LICENSEPAGE_RADIOBUTTONS
      LicenseForceSelection radiobuttons "${MUI_LICENSEPAGE_RADIOBUTTONS_TEXT_ACCEPT}" "${MUI_LICENSEPAGE_RADIOBUTTONS_TEXT_DECLINE}"
    !endif

  PageExEnd

  !insertmacro MUI_FUNCTION_LICENSEPAGE ${MUI_PAGE_UNINSTALLER_FUNCPREFIX}mui.LicensePre_${MUI_UNIQUEID} ${MUI_PAGE_UNINSTALLER_FUNCPREFIX}mui.LicenseShow_${MUI_UNIQUEID} ${MUI_PAGE_UNINSTALLER_FUNCPREFIX}mui.LicenseLeave_${MUI_UNIQUEID}

  !insertmacro MUI_UNSET MUI_LICENSEPAGE_TEXT_TOP
  !insertmacro MUI_UNSET MUI_LICENSEPAGE_TEXT_BOTTOM
  !insertmacro MUI_UNSET MUI_LICENSEPAGE_BUTTON
  !insertmacro MUI_UNSET MUI_LICENSEPAGE_CHECKBOX
    !insertmacro MUI_UNSET MUI_LICENSEPAGE_CHECKBOX_TEXT
  !insertmacro MUI_UNSET MUI_LICENSEPAGE_RADIOBUTTONS
    !insertmacro MUI_UNSET MUI_LICENSEPAGE_CHECKBOX_TEXT_ACCEPT
    !insertmacro MUI_UNSET MUI_LICENSEPAGE_CHECKBOX_TEXT_DECLINE

  !verbose pop

!macroend

!macro MUI_PAGE_LICENSE LICENSEDATA

  !verbose push
  !verbose ${MUI_VERBOSE}

  !insertmacro MUI_PAGE_INIT
  !insertmacro MUI_PAGEDECLARATION_LICENSE "${LICENSEDATA}"

  !verbose pop

!macroend

!macro MUI_UNPAGE_LICENSE LICENSEDATA

  !verbose push
  !verbose ${MUI_VERBOSE}

  !insertmacro MUI_UNPAGE_INIT
  !insertmacro MUI_PAGEDECLARATION_LICENSE "${LICENSEDATA}"

  !verbose pop

!macroend


;--------------------------------
;Page functions

!macro MUI_FUNCTION_LICENSEPAGE PRE SHOW LEAVE

  Function "${PRE}"

    !insertmacro MUI_PAGE_FUNCTION_CUSTOM PRE
    !insertmacro MUI_HEADER_TEXT_PAGE $(MUI_${MUI_PAGE_UNINSTALLER_PREFIX}TEXT_LICENSE_TITLE) $(MUI_${MUI_PAGE_UNINSTALLER_PREFIX}TEXT_LICENSE_SUBTITLE)

  FunctionEnd

  Function "${SHOW}"

    ;Get conrol handles
    FindWindow $mui.LicensePage "#32770" "" $HWNDPARENT
    GetDlgItem $mui.LicensePage.TopText $mui.LicensePage 1040
    GetDlgItem $mui.LicensePage.Text $mui.LicensePage 1006
    GetDlgItem $mui.LicensePage.LicenseText $mui.LicensePage 1000
    
    ;Top text
    SendMessage $mui.LicensePage.TopText ${WM_SETTEXT} 0 "STR:${MUI_LICENSEPAGE_TEXT_TOP}"

    !insertmacro MUI_PAGE_FUNCTION_CUSTOM SHOW

  FunctionEnd

  Function "${LEAVE}"

    !insertmacro MUI_PAGE_FUNCTION_CUSTOM LEAVE

  FunctionEnd

!macroend
