/*

NSIS Modern User Interface
Start Menu folder page

*/

;--------------------------------
;Page interface settings and variables

!macro MUI_STARTMENUPAGE_INTERFACE

  !ifndef MUI_STARTMENUPAGE_INTERFACE
    !define MUI_STARTMENUPAGE_INTERFACE
    Var mui.StartMenuPage
    Var mui.StartMenuPage.Location
    Var mui.StartMenuPage.FolderList

    Var mui.StartMenuPage.Temp
  !endif

  !ifdef MUI_STARTMENUPAGE_REGISTRY_ROOT & MUI_STARTMENUPAGE_REGISTRY_KEY & MUI_STARTMENUPAGE_REGISTRY_VALUENAME
    !ifndef MUI_STARTMENUPAGE_REGISTRY_VARIABLES
      !define MUI_STARTMENUPAGE_REGISTRY_VARIABLES
        Var mui.StartMenuPage.RegistryLocation
    !endif
  !endif

!macroend


;--------------------------------
;Page declaration

!macro MUI_PAGEDECLARATION_STARTMENU ID VAR

  !insertmacro MUI_SET MUI_${MUI_PAGE_UNINSTALLER_PREFIX}STARTMENUPAGE ""
  !insertmacro MUI_STARTMENUPAGE_INTERFACE  

  !insertmacro MUI_DEFAULT MUI_STARTMENUPAGE_DEFAULTFOLDER "$(^Name)"
  !insertmacro MUI_DEFAULT MUI_STARTMENUPAGE_TEXT_TOP "$(MUI_${MUI_PAGE_UNINSTALLER_PREFIX}INNERTEXT_STARTMENU_TOP)"
  !insertmacro MUI_DEFAULT MUI_STARTMENUPAGE_TEXT_CHECKBOX "$(MUI_${MUI_PAGE_UNINSTALLER_PREFIX}INNERTEXT_STARTMENU_CHECKBOX)"

  !define MUI_STARTMENUPAGE_VARIABLE "${VAR}"
  !define "MUI_STARTMENUPAGE_${ID}_VARIABLE" "${MUI_STARTMENUPAGE_VARIABLE}"
  !define "MUI_STARTMENUPAGE_${ID}_DEFAULTFOLDER" "${MUI_STARTMENUPAGE_DEFAULTFOLDER}"
  !ifdef MUI_STARTMENUPAGE_REGISTRY_ROOT
    !define "MUI_STARTMENUPAGE_${ID}_REGISTRY_ROOT" "${MUI_STARTMENUPAGE_REGISTRY_ROOT}"
  !endif
  !ifdef MUI_STARTMENUPAGE_REGISTRY_KEY
    !define "MUI_STARTMENUPAGE_${ID}_REGISTRY_KEY" "${MUI_STARTMENUPAGE_REGISTRY_KEY}"
  !endif
  !ifdef MUI_STARTMENUPAGE_REGISTRY_VALUENAME
    !define "MUI_STARTMENUPAGE_${ID}_REGISTRY_VALUENAME" "${MUI_STARTMENUPAGE_REGISTRY_VALUENAME}"
  !endif

  PageEx ${MUI_PAGE_UNINSTALLER_FUNCPREFIX}custom

    PageCallbacks ${MUI_PAGE_UNINSTALLER_FUNCPREFIX}mui.StartmenuPre_${MUI_UNIQUEID} ${MUI_PAGE_UNINSTALLER_FUNCPREFIX}mui.StartmenuLeave_${MUI_UNIQUEID}

    Caption " "

  PageExEnd

  !insertmacro MUI_FUNCTION_STARTMENUPAGE ${MUI_PAGE_UNINSTALLER_FUNCPREFIX}mui.StartmenuPre_${MUI_UNIQUEID} ${MUI_PAGE_UNINSTALLER_FUNCPREFIX}mui.StartmenuLeave_${MUI_UNIQUEID}

  !undef MUI_STARTMENUPAGE_VARIABLE
  !undef MUI_STARTMENUPAGE_TEXT_TOP
  !undef MUI_STARTMENUPAGE_TEXT_CHECKBOX
  !undef MUI_STARTMENUPAGE_DEFAULTFOLDER
  !insertmacro MUI_UNSET MUI_STARTMENUPAGE_NODISABLE
  !insertmacro MUI_UNSET MUI_STARTMENUPAGE_REGISTRY_ROOT
  !insertmacro MUI_UNSET MUI_STARTMENUPAGE_REGISTRY_KEY
  !insertmacro MUI_UNSET MUI_STARTMENUPAGE_REGISTRY_VALUENAME

!macroend

!macro MUI_PAGE_STARTMENU ID VAR

  !verbose push
  !verbose ${MUI_VERBOSE}

  !insertmacro MUI_PAGE_INIT
  !insertmacro MUI_PAGEDECLARATION_STARTMENU "${ID}" "${VAR}"

  !verbose pop

!macroend

;--------------------------------
;Page functions

!macro MUI_FUNCTION_STARTMENUPAGE PRE LEAVE

  Function "${PRE}"

    !insertmacro MUI_PAGE_FUNCTION_CUSTOM PRE

    !ifdef MUI_STARTMENUPAGE_REGISTRY_ROOT & MUI_STARTMENUPAGE_REGISTRY_KEY & MUI_STARTMENUPAGE_REGISTRY_VALUENAME
      
      ;Get Start Menu location from registry

      ${if} "${MUI_STARTMENUPAGE_VARIABLE}" == ""

        ReadRegStr $mui.StartMenuPage.RegistryLocation  "${MUI_STARTMENUPAGE_REGISTRY_ROOT}" "${MUI_STARTMENUPAGE_REGISTRY_KEY}" "${MUI_STARTMENUPAGE_REGISTRY_VALUENAME}"
        ${if} $mui.StartMenuPage.RegistryLocation != ""
          StrCpy "${MUI_STARTMENUPAGE_VARIABLE}" $mui.StartMenuPage.RegistryLocation
        ${endif}

        ClearErrors

      ${endif}

    !endif

    !insertmacro MUI_HEADER_TEXT_PAGE $(MUI_TEXT_STARTMENU_TITLE) $(MUI_TEXT_STARTMENU_SUBTITLE)

    ${if} $(^RTL) == "0"
       !ifndef MUI_STARTMENUPAGE_NODISABLE
        StartMenu::Init /noicon /autoadd /text "${MUI_STARTMENUPAGE_TEXT_TOP}" /lastused "${MUI_STARTMENUPAGE_VARIABLE}" /checknoshortcuts "${MUI_STARTMENUPAGE_TEXT_CHECKBOX}" "${MUI_STARTMENUPAGE_DEFAULTFOLDER}"
      !else
        StartMenu::Init /noicon /autoadd /text "${MUI_STARTMENUPAGE_TEXT_TOP}" /lastused "${MUI_STARTMENUPAGE_VARIABLE}" "${MUI_STARTMENUPAGE_DEFAULTFOLDER}"
      !endif
    ${else}
      !ifndef MUI_STARTMENUPAGE_NODISABLE
        StartMenu::Init /rtl /noicon /autoadd /text "${MUI_STARTMENUPAGE_TEXT_TOP}" /lastused "${MUI_STARTMENUPAGE_VARIABLE}" /checknoshortcuts "${MUI_STARTMENUPAGE_TEXT_CHECKBOX}" "${MUI_STARTMENUPAGE_DEFAULTFOLDER}"
      !else
        StartMenu::Init /rtl /noicon /autoadd /text "${MUI_STARTMENUPAGE_TEXT_TOP}" /lastused "${MUI_STARTMENUPAGE_VARIABLE}" "${MUI_STARTMENUPAGE_DEFAULTFOLDER}"
      !endif
    ${endif}

    Pop $mui.StartMenuPage

    ;Get control handles
    GetDlgItem $mui.StartMenuPage.Location $mui.StartMenuPage 1002
    GetDlgItem $mui.StartMenuPage.FolderList $mui.StartMenuPage 1004

    !ifdef MUI_STARTMENUPAGE_BGCOLOR
      SetCtlColors $mui.StartMenuPage.Location "" "${MUI_STARTMENUPAGE_BGCOLOR}"
      SetCtlColors $mui.StartMenuMenu.FolderList "" "${MUI_STARTMENUPAGE_BGCOLOR}"
    !endif

    !insertmacro MUI_PAGE_FUNCTION_CUSTOM SHOW

    StartMenu::Show

    Pop $mui.StartMenuPage.Temp
    ${if} $mui.StartMenuPage.Temp ==  "success"
      Pop "${MUI_STARTMENUPAGE_VARIABLE}"
    ${endif}

  FunctionEnd

  Function "${LEAVE}"

    !insertmacro MUI_PAGE_FUNCTION_CUSTOM LEAVE

  FunctionEnd

!macroend


;--------------------------------
;Script macros to get Start Menu folder

!macro MUI_STARTMENU_GETFOLDER ID VAR

  !verbose push
  !verbose ${MUI_VERBOSE}

  ;Get Start Menu folder from registry
  ;Can be called from the script in the uninstaller

  !ifdef MUI_STARTMENUPAGE_${ID}_REGISTRY_ROOT & MUI_STARTMENUPAGE_${ID}_REGISTRY_KEY & MUI_STARTMENUPAGE_${ID}_REGISTRY_VALUENAME

    ReadRegStr $mui.StartMenuPage.RegistryLocation "${MUI_STARTMENUPAGE_${ID}_REGISTRY_ROOT}" "${MUI_STARTMENUPAGE_${ID}_REGISTRY_KEY}" "${MUI_STARTMENUPAGE_${ID}_REGISTRY_VALUENAME}"
    
    ${if} $mui.StartMenuPage.RegistryLocation != ""
      StrCpy "${VAR}" $mui.StartMenuPage.RegistryLocation
    ${else}
      StrCpy "${VAR}" "${MUI_STARTMENUPAGE_${ID}_DEFAULTFOLDER}"
    ${endif}

  !else

    StrCpy "${VAR}" "${MUI_STARTMENUPAGE_${ID}_DEFAULTFOLDER}"

  !endif
   
  !verbose pop   

!macroend

!macro MUI_STARTMENU_WRITE_BEGIN ID

  ;The code in the script to write the shortcuts should be put between the
  ;MUI_STARTMENU_WRITE_BEGIN and MUI_STARTMENU_WRITE_END macros

  !verbose push
  !verbose ${MUI_VERBOSE}

  !define MUI_STARTMENUPAGE_CURRENT_ID "${ID}"

  StrCpy $mui.StartMenuPage.Temp "${MUI_STARTMENUPAGE_${MUI_STARTMENUPAGE_CURRENT_ID}_VARIABLE}" 1
  
  ;If the folder start with >, the user has chosen not to create a shortcut
  ${if} $mui.StartMenuPage.Temp != ">"

    ${if} "${MUI_STARTMENUPAGE_${MUI_STARTMENUPAGE_CURRENT_ID}_VARIABLE}" == ""
      ;Get folder from registry if the variable doesn't contain anything
      !insertmacro MUI_STARTMENU_GETFOLDER "${MUI_STARTMENUPAGE_CURRENT_ID}" "${MUI_STARTMENUPAGE_${MUI_STARTMENUPAGE_CURRENT_ID}_VARIABLE}"
    ${endif}

  !verbose pop

!macroend

!macro MUI_STARTMENU_WRITE_END

  !verbose push
  !verbose ${MUI_VERBOSE}

    !ifdef MUI_STARTMENUPAGE_${MUI_STARTMENUPAGE_CURRENT_ID}_REGISTRY_ROOT & MUI_STARTMENUPAGE_${MUI_STARTMENUPAGE_CURRENT_ID}_REGISTRY_KEY & MUI_STARTMENUPAGE_${MUI_STARTMENUPAGE_CURRENT_ID}_REGISTRY_VALUENAME
      ;Write folder to registry
      WriteRegStr "${MUI_STARTMENUPAGE_${MUI_STARTMENUPAGE_CURRENT_ID}_REGISTRY_ROOT}" "${MUI_STARTMENUPAGE_${MUI_STARTMENUPAGE_CURRENT_ID}_REGISTRY_KEY}" "${MUI_STARTMENUPAGE_${MUI_STARTMENUPAGE_CURRENT_ID}_REGISTRY_VALUENAME}" "${MUI_STARTMENUPAGE_${MUI_STARTMENUPAGE_CURRENT_ID}_VARIABLE}"
    !endif

  ${endif}

  !undef MUI_STARTMENUPAGE_CURRENT_ID

  !verbose pop

!macroend

