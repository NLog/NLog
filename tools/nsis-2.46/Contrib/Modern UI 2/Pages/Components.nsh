/*

NSIS Modern User Interface
Components page

*/

;--------------------------------
;Page interface settings and variables

!macro MUI_COMPONENTSPAGE_INTERFACE

  !ifndef MUI_COMPONENTSPAGE_INTERFACE
    !define MUI_COMPONENTSPAGE_INTERFACE
    Var mui.ComponentsPage
    
    Var mui.ComponentsPage.Text
    Var mui.ComponentsPage.InstTypesText
    Var mui.ComponentsPage.ComponentsText 

    Var mui.ComponentsPage.InstTypes
    Var mui.ComponentsPage.Components    
    
    Var mui.ComponentsPage.DescriptionTitle
    Var mui.ComponentsPage.DescriptionText.Info
    Var mui.ComponentsPage.DescriptionText
    
    Var mui.ComponentsPage.SpaceRequired
    
    !insertmacro MUI_DEFAULT MUI_COMPONENTSPAGE_CHECKBITMAP "${NSISDIR}\Contrib\Graphics\Checks\modern.bmp"
    
    !insertmacro MUI_DEFAULT MUI_UI_COMPONENTSPAGE_SMALLDESC "${NSISDIR}\Contrib\UIs\modern_smalldesc.exe"
    !insertmacro MUI_DEFAULT MUI_UI_COMPONENTSPAGE_NODESC "${NSISDIR}\Contrib\UIs\modern_nodesc.exe"
    
    ;Apply settings
    
    !ifdef MUI_COMPONENTSPAGE_SMALLDESC
      ChangeUI IDD_SELCOM "${MUI_UI_COMPONENTSPAGE_SMALLDESC}"
    !else ifdef MUI_COMPONENTSPAGE_NODESC
      ChangeUI IDD_SELCOM "${MUI_UI_COMPONENTSPAGE_NODESC}"
    !endif

    CheckBitmap "${MUI_COMPONENTSPAGE_CHECKBITMAP}"    
       
  !endif

!macroend


;--------------------------------
;Page declaration

!macro MUI_PAGEDECLARATION_COMPONENTS

  !insertmacro MUI_SET MUI_${MUI_PAGE_UNINSTALLER_PREFIX}COMPONENTSPAGE ""
  !insertmacro MUI_COMPONENTSPAGE_INTERFACE

  !insertmacro MUI_DEFAULT MUI_COMPONENTSPAGE_TEXT_TOP ""
  !insertmacro MUI_DEFAULT MUI_COMPONENTSPAGE_TEXT_COMPLIST ""
  !insertmacro MUI_DEFAULT MUI_COMPONENTSPAGE_TEXT_INSTTYPE ""
  !insertmacro MUI_DEFAULT MUI_COMPONENTSPAGE_TEXT_DESCRIPTION_TITLE "$(MUI_INNERTEXT_COMPONENTS_DESCRIPTION_TITLE)"
  !insertmacro MUI_DEFAULT MUI_COMPONENTSPAGE_TEXT_DESCRIPTION_INFO "$(MUI_INNERTEXT_COMPONENTS_DESCRIPTION_INFO)"
  
  PageEx ${MUI_PAGE_UNINSTALLER_FUNCPREFIX}components

    PageCallbacks ${MUI_PAGE_UNINSTALLER_FUNCPREFIX}mui.ComponentsPre_${MUI_UNIQUEID} ${MUI_PAGE_UNINSTALLER_FUNCPREFIX}mui.ComponentsShow_${MUI_UNIQUEID} ${MUI_PAGE_UNINSTALLER_FUNCPREFIX}mui.ComponentsLeave_${MUI_UNIQUEID}

    Caption " "

    ComponentText "${MUI_COMPONENTSPAGE_TEXT_TOP}" "${MUI_COMPONENTSPAGE_TEXT_INSTTYPE}" "${MUI_COMPONENTSPAGE_TEXT_COMPLIST}"

  PageExEnd

  !insertmacro MUI_FUNCTION_COMPONENTSPAGE ${MUI_PAGE_UNINSTALLER_FUNCPREFIX}mui.ComponentsPre_${MUI_UNIQUEID} ${MUI_PAGE_UNINSTALLER_FUNCPREFIX}mui.ComponentsShow_${MUI_UNIQUEID} ${MUI_PAGE_UNINSTALLER_FUNCPREFIX}mui.ComponentsLeave_${MUI_UNIQUEID}

  !undef MUI_COMPONENTSPAGE_TEXT_TOP
  !undef MUI_COMPONENTSPAGE_TEXT_COMPLIST
  !undef MUI_COMPONENTSPAGE_TEXT_INSTTYPE
  !insertmacro MUI_UNSET MUI_COMPONENTSPAGE_TEXT_DESCRIPTION_TITLE
  !insertmacro MUI_UNSET MUI_COMPONENTSPAGE_TEXT_DESCRIPTION_INFO

!macroend

!macro MUI_PAGE_COMPONENTS

  !verbose push
  !verbose ${MUI_VERBOSE}

  !insertmacro MUI_PAGE_INIT
  !insertmacro MUI_PAGEDECLARATION_COMPONENTS

  !verbose pop

!macroend

!macro MUI_UNPAGE_COMPONENTS

  !verbose push
  !verbose ${MUI_VERBOSE}

  !insertmacro MUI_UNPAGE_INIT
  !insertmacro MUI_PAGEDECLARATION_COMPONENTS

  !verbose pop

!macroend


;--------------------------------
;Page functions

!macro MUI_FUNCTION_COMPONENTSPAGE PRE SHOW LEAVE

  Function "${PRE}"
    !insertmacro MUI_PAGE_FUNCTION_CUSTOM PRE
    !insertmacro MUI_HEADER_TEXT_PAGE $(MUI_${MUI_PAGE_UNINSTALLER_PREFIX}TEXT_COMPONENTS_TITLE) $(MUI_${MUI_PAGE_UNINSTALLER_PREFIX}TEXT_COMPONENTS_SUBTITLE)
  FunctionEnd

  Function "${SHOW}"
  
    ;Get control hanldes
    FindWindow $mui.ComponentsPage "#32770" "" $HWNDPARENT
    GetDlgItem $mui.ComponentsPage.Text             $mui.ComponentsPage 1006
    GetDlgItem $mui.ComponentsPage.InstTypesText    $mui.ComponentsPage 1021
    GetDlgItem $mui.ComponentsPage.ComponentsText   $mui.ComponentsPage 1022
    GetDlgItem $mui.ComponentsPage.InstTypes        $mui.ComponentsPage 1017
    GetDlgItem $mui.ComponentsPage.Components       $mui.ComponentsPage 1032
    GetDlgItem $mui.ComponentsPage.DescriptionTitle $mui.ComponentsPage 1042
    GetDlgItem $mui.ComponentsPage.DescriptionText  $mui.ComponentsPage 1043
    GetDlgItem $mui.ComponentsPage.SpaceRequired    $mui.ComponentsPage 1023    

    ;Default text in description textbox
    SendMessage $mui.ComponentsPage.DescriptionTitle ${WM_SETTEXT} 0 "STR:${MUI_COMPONENTSPAGE_TEXT_DESCRIPTION_TITLE}"
    EnableWindow $mui.ComponentsPage.DescriptionText 0
    SendMessage $mui.ComponentsPage.DescriptionText ${WM_SETTEXT} 0 "STR:${MUI_COMPONENTSPAGE_TEXT_DESCRIPTION_INFO}"

    StrCpy $mui.ComponentsPage.DescriptionText.Info "${MUI_COMPONENTSPAGE_TEXT_DESCRIPTION_INFO}" ;Text for current components page

    !insertmacro MUI_PAGE_FUNCTION_CUSTOM SHOW

  FunctionEnd

  Function "${LEAVE}"

    !insertmacro MUI_PAGE_FUNCTION_CUSTOM LEAVE

  FunctionEnd

!macroend


;--------------------------------
;Script functions for components descriptions

!macro MUI_DESCRIPTION_BEGIN

  ${if} $0 == -1
    ;No mouse hover over component in list
    SendMessage $mui.ComponentsPage.DescriptionText ${WM_SETTEXT} 0 "STR:"
    EnableWindow $mui.ComponentsPage.DescriptionText 0
    SendMessage $mui.ComponentsPage.DescriptionText ${WM_SETTEXT} 0 "STR:$mui.ComponentsPage.DescriptionText.Info"

!macroend

!macro MUI_DESCRIPTION_TEXT VAR TEXT

  !verbose push
  !verbose ${MUI_VERBOSE}

  ${elseif} $0 == ${VAR}
    SendMessage $mui.ComponentsPage.DescriptionText ${WM_SETTEXT} 0 "STR:"
    EnableWindow $mui.ComponentsPage.DescriptionText 1
    SendMessage $mui.ComponentsPage.DescriptionText ${WM_SETTEXT} 0 "STR:${TEXT}"

  !verbose pop

!macroend

!macro MUI_DESCRIPTION_END

  !verbose push
  !verbose ${MUI_VERBOSE}

  ${endif}

  !verbose pop

!macroend

!macro MUI_FUNCTION_DESCRIPTION_BEGIN

  !verbose push
  !verbose ${MUI_VERBOSE}

  Function .onMouseOverSection
    !insertmacro MUI_DESCRIPTION_BEGIN

  !verbose pop

!macroend

!macro MUI_FUNCTION_DESCRIPTION_END

  !verbose push
  !verbose ${MUI_VERBOSE}

    !insertmacro MUI_DESCRIPTION_END
    !ifdef MUI_CUSTOMFUNCTION_ONMOUSEOVERSECTION
      Call "${MUI_CUSTOMFUNCTION_ONMOUSEOVERSECTION}"
    !endif
  FunctionEnd

  !verbose pop

!macroend

!macro MUI_UNFUNCTION_DESCRIPTION_BEGIN

  !verbose push
  !verbose ${MUI_VERBOSE}

  Function un.onMouseOverSection
    !insertmacro MUI_DESCRIPTION_BEGIN

  !verbose pop

!macroend

!macro MUI_UNFUNCTION_DESCRIPTION_END

  !verbose push
  !verbose ${MUI_VERBOSE}

    !insertmacro MUI_DESCRIPTION_END
    !ifdef MUI_CUSTOMFUNCTION_UNONMOUSEOVERSECTION
      Call "${MUI_CUSTOMFUNCTION_UNONMOUSEOVERSECTION}"
    !endif
  FunctionEnd

  !verbose pop

!macroend
