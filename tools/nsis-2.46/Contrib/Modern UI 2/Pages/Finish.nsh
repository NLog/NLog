 /*

NSIS Modern User Interface
Finish page (implemented using nsDialogs)

*/

;--------------------------------
;Page interface settings and variables

!macro MUI_FINISHPAGE_INTERFACE

  !ifndef MUI_FINISHPAGE_INTERFACE
    !define MUI_FINISHPAGE_INTERFACE
    Var mui.FinishPage
        
    Var mui.FinishPage.Image
    Var mui.FinishPage.Image.Bitmap
    
    Var mui.FinishPage.Title
    Var mui.FinishPage.Title.Font
    
    Var mui.FinishPage.Text
  !endif

  !ifndef MUI_FINISHPAGE_NOREBOOTSUPPORT
    !ifndef MUI_FINISHPAGE_RETURNVALUE_VARIABLES
      !define MUI_FINISHPAGE_RETURNVALUE_VARIABLES
      Var mui.FinishPage.ReturnValue
    !endif
  !else ifdef MUI_FINISHPAGE_RUN | MUI_FINISHPAGE_SHOWREADME
    !ifndef MUI_FINISHPAGE_RETURNVALUE_VARIABLES
      !define MUI_FINISHPAGE_RETURNVALUE_VARIABLES
      Var mui.FinishPage.ReturnValue
    !endif 
  !endif
    
  !ifdef MUI_FINISHPAGE_CANCEL_ENABLED
    !ifndef MUI_FINISHPAGE_CANCEL_ENABLED_VARIABLES
      !define MUI_FINISHPAGE_CANCEL_ENABLED_VARIABLES
      Var mui.FinishPage.DisableAbortWarning
    !endif  
  !endif
  
  !ifdef MUI_FINISHPAGE_RUN
    !ifndef MUI_FINISHPAGE_RUN_VARIABLES
      !define MUI_FINISHPAGE_RUN_VARIABLES
      Var mui.FinishPage.Run
    !endif
  !endif
  
  !ifdef MUI_FINISHPAGE_SHOWREADME
    !ifndef MUI_FINISHPAGE_SHOREADME_VARAIBLES
      !define MUI_FINISHPAGE_SHOREADME_VARAIBLES
      Var mui.FinishPage.ShowReadme
    !endif
  !endif
  
  !ifdef MUI_FINISHPAGE_LINK
    !ifndef MUI_FINISHPAGE_LINK_VARIABLES
      !define MUI_FINISHPAGE_LINK_VARIABLES
      Var mui.FinishPage.Link
    !endif
  !endif
  
  !ifndef MUI_FINISHPAGE_NOREBOOTSUPPORT
    !ifndef MUI_FINISHPAGE_REBOOT_VARIABLES
      !define MUI_FINISHPAGE_REBOOT_VARIABLES
      Var mui.FinishPage.RebootNow
      Var mui.FinishPage.RebootLater
    !endif
  !endif

  !insertmacro MUI_DEFAULT MUI_${MUI_PAGE_UNINSTALLER_PREFIX}WELCOMEFINISHPAGE_BITMAP "${NSISDIR}\Contrib\Graphics\Wizard\win.bmp"  

!macroend


;--------------------------------
;Interface initialization

!macro MUI_FINISHPAGE_GUIINIT

  !ifndef MUI_${MUI_PAGE_UNINSTALLER_PREFIX}WELCOMEFINISHPAGE_GUINIT
    !define MUI_${MUI_PAGE_UNINSTALLER_PREFIX}WELCOMEFINISHPAGE_GUINIT

    Function ${MUI_PAGE_UNINSTALLER_FUNCPREFIX}mui.FinishPage.GUIInit
  
      InitPluginsDir  
      File "/oname=$PLUGINSDIR\modern-wizard.bmp" "${MUI_${MUI_PAGE_UNINSTALLER_PREFIX}WELCOMEFINISHPAGE_BITMAP}"
    
      !ifdef MUI_${MUI_PAGE_UNINSTALLER_PREFIX}PAGE_FUNCTION_GUIINIT
        Call "${MUI_${MUI_PAGE_UNINSTALLER_PREFIX}PAGE_FUNCTION_GUIINIT}"
      !endif
      
      !ifndef MUI_${MUI_PAGE_UNINSTALLER_PREFIX}FINISHPAGE_NOAUTOCLOSE
        SetAutoClose true
      !endif
    
    FunctionEnd
  
    !insertmacro MUI_SET MUI_${MUI_PAGE_UNINSTALLER_PREFIX}PAGE_FUNCTION_GUIINIT ${MUI_PAGE_UNINSTALLER_FUNCPREFIX}mui.FinishPage.GUIInit
    
  !endif    

!macroend


;--------------------------------
;Abort warning

!macro MUI_FINISHPAGE_ABORTWARNING

  !ifdef MUI_FINISHPAGE_CANCEL_ENABLED
  
    !ifndef MUI_${MUI_PAGE_UNINSTALLER_PREFIX}FINISHPAGE_ABORTWARNING
      !define MUI_${MUI_PAGE_UNINSTALLER_PREFIX}FINISHPAGE_ABORTWARNING

      Function ${MUI_PAGE_UNINSTALLER_FUNCPREFIX}mui.FinishPage.AbortWarning

        ${if} $mui.FinishPage.DisableAbortWarning == "1"
          Quit
        ${endif}
      
        !ifdef MUI_${MUI_PAGE_UNINSTALLER_PREFIX}PAGE_FUNCTION_ABORTWARNING
          Call ${MUI_${MUI_PAGE_UNINSTALLER_PREFIX}PAGE_FUNCTION_ABORTWARNING}
        !endif
  
      FunctionEnd
    
      !insertmacro MUI_SET MUI_${MUI_PAGE_UNINSTALLER_PREFIX}PAGE_FUNCTION_ABORTWARNING ${MUI_PAGE_UNINSTALLER_FUNCPREFIX}mui.FinishPage.AbortWarning
    
    !endif
  
  !endif

!macroend


;--------------------------------
;Page declaration

!macro MUI_PAGEDECLARATION_FINISH

  !insertmacro MUI_SET MUI_${MUI_PAGE_UNINSTALLER_PREFIX}FINISHPAGE ""
  !insertmacro MUI_FINISHPAGE_INTERFACE
  
  !insertmacro MUI_FINISHPAGE_GUIINIT
  !insertmacro MUI_FINISHPAGE_ABORTWARNING

  !insertmacro MUI_DEFAULT MUI_FINISHPAGE_TITLE "$(MUI_${MUI_PAGE_UNINSTALLER_PREFIX}TEXT_FINISH_INFO_TITLE)"
  !insertmacro MUI_DEFAULT MUI_FINISHPAGE_TEXT "$(MUI_${MUI_PAGE_UNINSTALLER_PREFIX}TEXT_FINISH_INFO_TEXT)"
  !insertmacro MUI_DEFAULT MUI_FINISHPAGE_BUTTON "$(MUI_BUTTONTEXT_FINISH)"
  !insertmacro MUI_DEFAULT MUI_FINISHPAGE_TEXT_REBOOT "$(MUI_${MUI_PAGE_UNINSTALLER_PREFIX}TEXT_FINISH_INFO_REBOOT)"
  !insertmacro MUI_DEFAULT MUI_FINISHPAGE_TEXT_REBOOTNOW "$(MUI_TEXT_FINISH_REBOOTNOW)"
  !insertmacro MUI_DEFAULT MUI_FINISHPAGE_TEXT_REBOOTLATER "$(MUI_TEXT_FINISH_REBOOTLATER)"
  !insertmacro MUI_DEFAULT MUI_FINISHPAGE_RUN_TEXT "$(MUI_TEXT_FINISH_RUN)"
  !insertmacro MUI_DEFAULT MUI_FINISHPAGE_SHOWREADME_TEXT "$(MUI_TEXT_FINISH_SHOWREADME)"
  !insertmacro MUI_DEFAULT MUI_FINISHPAGE_LINK_COLOR "000080"

  !insertmacro MUI_PAGE_FUNCTION_FULLWINDOW

  PageEx ${MUI_PAGE_UNINSTALLER_FUNCPREFIX}custom

    PageCallbacks ${MUI_PAGE_UNINSTALLER_FUNCPREFIX}mui.FinishPage.Pre_${MUI_UNIQUEID} \
      ${MUI_PAGE_UNINSTALLER_FUNCPREFIX}mui.FinishPage.Leave_${MUI_UNIQUEID}

    Caption " "

  PageExEnd

  !insertmacro MUI_FUNCTION_FINISHPAGE ${MUI_PAGE_UNINSTALLER_FUNCPREFIX}mui.FinishPage.Pre_${MUI_UNIQUEID} \
    ${MUI_PAGE_UNINSTALLER_FUNCPREFIX}mui.FinishPage.Leave_${MUI_UNIQUEID} \
    ${MUI_PAGE_UNINSTALLER_FUNCPREFIX}mui.FinishPage.Link_${MUI_UNIQUEID}

  !insertmacro MUI_UNSET MUI_FINISHPAGE_TITLE
  !insertmacro MUI_UNSET MUI_FINISHPAGE_TITLE_3LINES
  !insertmacro MUI_UNSET MUI_FINISHPAGE_TEXT
  !insertmacro MUI_UNSET MUI_FINISHPAGE_TEXT_LARGE
  !insertmacro MUI_UNSET MUI_FINISHPAGE_BUTTON
  !insertmacro MUI_UNSET MUI_FINISHPAGE_CANCEL_ENABLED
  !insertmacro MUI_UNSET MUI_FINISHPAGE_TEXT_REBOOT
  !insertmacro MUI_UNSET MUI_FINISHPAGE_TEXT_REBOOTNOW
  !insertmacro MUI_UNSET MUI_FINISHPAGE_TEXT_REBOOTLATER
  !insertmacro MUI_UNSET MUI_FINISHPAGE_REBOOTLATER_DEFAULT
  !insertmacro MUI_UNSET MUI_FINISHPAGE_RUN
    !insertmacro MUI_UNSET MUI_FINISHPAGE_RUN_TEXT
    !insertmacro MUI_UNSET MUI_FINISHPAGE_RUN_PARAMETERS
    !insertmacro MUI_UNSET MUI_FINISHPAGE_RUN_NOTCHECKED
    !insertmacro MUI_UNSET MUI_FINISHPAGE_RUN_FUNCTION
  !insertmacro MUI_UNSET MUI_FINISHPAGE_SHOWREADME
    !insertmacro MUI_UNSET MUI_FINISHPAGE_SHOWREADME_TEXT
    !insertmacro MUI_UNSET MUI_FINISHPAGE_SHOWREADME_NOTCHECKED
    !insertmacro MUI_UNSET MUI_FINISHPAGE_SHOWREADME_FUNCTION
  !insertmacro MUI_UNSET MUI_FINISHPAGE_LINK
    !insertmacro MUI_UNSET MUI_FINISHPAGE_LINK_LOCATION
    !insertmacro MUI_UNSET MUI_FINISHPAGE_LINK_COLOR
  !insertmacro MUI_UNSET MUI_FINISHPAGE_NOREBOOTSUPPORT

  !insertmacro MUI_UNSET MUI_FINISHPAGE_ABORTWARNINGCHECK
  !insertmacro MUI_UNSET MUI_FINISHPAGE_CURFIELD_TOP
  !insertmacro MUI_UNSET MUI_FINISHPAGE_CURFIELD_BOTTOM

!macroend

!macro MUI_PAGE_FINISH

  !verbose push
  !verbose ${MUI_VERBOSE}

  !insertmacro MUI_PAGE_INIT
  !insertmacro MUI_PAGEDECLARATION_FINISH

  !verbose pop

!macroend

!macro MUI_UNPAGE_FINISH

  !verbose push
  !verbose ${MUI_VERBOSE}

  !insertmacro MUI_UNPAGE_INIT
  !insertmacro MUI_PAGEDECLARATION_FINISH

  !verbose pop

!macroend


;--------------------------------
;Page functions

!macro MUI_FUNCTION_FINISHPAGE PRE LEAVE LINK

  !ifdef MUI_FINISHPAGE_LINK
  
  Function "${LINK}"
  
    ExecShell open "${MUI_FINISHPAGE_LINK_LOCATION}"
  
  FunctionEnd
  
  !endif
  
  Function "${PRE}"

    !insertmacro MUI_PAGE_FUNCTION_CUSTOM PRE

    ;Set text on Next button
    SendMessage $mui.Button.Next ${WM_SETTEXT} 0 "STR:${MUI_FINISHPAGE_BUTTON}"
    
    ;Enable cancel button if set in script
    !ifdef MUI_FINISHPAGE_CANCEL_ENABLED
      EnableWindow $mui.Button.Cancel 1
    !endif
    
    ;Create dialog
    nsDialogs::Create 1044
    Pop $mui.FinishPage
    nsDialogs::SetRTL $(^RTL)
    SetCtlColors $mui.FinishPage "" "${MUI_BGCOLOR}"

    ;Image control
    ${NSD_CreateBitmap} 0u 0u 109u 193u ""
    Pop $mui.FinishPage.Image
    !ifndef MUI_${MUI_PAGE_UNINSTALLER_PREFIX}WELCOMEFINISHPAGE_BITMAP_NOSTRETCH
      ${NSD_SetStretchedImage} $mui.FinishPage.Image $PLUGINSDIR\modern-wizard.bmp $mui.FinishPage.Image.Bitmap
    !else
      ${NSD_SetImage} $mui.FinishPage.Image $PLUGINSDIR\modern-wizard.bmp $mui.FinishPage.Image.Bitmap
    !endif
    
    ;Positiong of controls

    ;Title    
    !ifndef MUI_FINISHPAGE_TITLE_3LINES
      !define MUI_FINISHPAGE_TITLE_HEIGHT 28
    !else
      !define MUI_FINISHPAGE_TITLE_HEIGHT 38
    !endif
    
    ;Text
    ;17 = 10 (top margin) + 7 (distance between texts)
    !define /math MUI_FINISHPAGE_TEXT_TOP 17 + ${MUI_FINISHPAGE_TITLE_HEIGHT}
    
    ;Height if space required for radio buttons or check boxes
    !ifndef MUI_FINISHPAGE_TEXT_LARGE
      !define MUI_FINISHPAGE_TEXT_HEIGHT_BUTTONS 40
    !else
      !define MUI_FINISHPAGE_TEXT_HEIGHT_BUTTONS 60
    !endif
    
    !define /math MUI_FINISHPAGE_TEXT_BOTTOM_BUTTONS ${MUI_FINISHPAGE_TEXT_TOP} + ${MUI_FINISHPAGE_TEXT_HEIGHT_BUTTONS}
    
    ;Positioning of radio buttons to ask for a reboot
    !ifndef MUI_FINISHPAGE_NOREBOOTSUPPORT
      !define /math MUI_FINISHPAGE_REBOOTNOW_TOP ${MUI_FINISHPAGE_TEXT_BOTTOM_BUTTONS} + 5 ;Distance between text and options
      ;25 = 10 (height of first radio button) + 15 (distance between buttons)
      !define /math MUI_FINISHPAGE_REBOOTLATER_TOP ${MUI_FINISHPAGE_REBOOTNOW_TOP} + 25
    !endif
    
    ;Positioning of checkboxes
    !ifdef MUI_FINISHPAGE_RUN
      !define /math MUI_FINISHPAGE_RUN_TOP ${MUI_FINISHPAGE_TEXT_BOTTOM_BUTTONS} + 5 ;Distance between text and options 
    !endif
    !ifdef MUI_FINISHPAGE_SHOWREADME
      !ifdef MUI_FINISHPAGE_RUN
        ;25 = 10 (height of run checkbox) + 10 (distance between checkboxes)
        !define /math MUI_FINISHPAGE_SHOWREADME_TOP ${MUI_FINISHPAGE_RUN_TOP} + 20
      !else
        !define /math MUI_FINISHPAGE_SHOWREADME_TOP ${MUI_FINISHPAGE_TEXT_BOTTOM_BUTTONS} + 5 ;Distance between text and options    
      !endif
    !endif

    !ifndef MUI_FINISHPAGE_RUN & MUI_FINISHPAGE_SHOWREADME
      ;Height if full space is available for text and link
      !ifndef MUI_FINISHPAGE_LINK
        !define MUI_FINISHPAGE_TEXT_HEIGHT 130
      !else
        !define MUI_FINISHPAGE_TEXT_HEIGHT 120
      !endif
    !endif 
    
    !ifndef MUI_FINISHPAGE_NOREBOOTSUPPORT

      ${if} ${RebootFlag}

        ;Title text
        ${NSD_CreateLabel} 120u 10u 195u ${MUI_FINISHPAGE_TITLE_HEIGHT}u "${MUI_FINISHPAGE_TITLE}"
        Pop $mui.FinishPage.Title
        SetCtlColors $mui.FinishPage.Title "" "${MUI_BGCOLOR}"
        CreateFont $mui.FinishPage.Title.Font "$(^Font)" "12" "700"
        SendMessage $mui.FinishPage.Title ${WM_SETFONT} $mui.FinishPage.Title.Font 0

        ;Finish text
        ${NSD_CreateLabel} 120u 45u 195u ${MUI_FINISHPAGE_TEXT_HEIGHT_BUTTONS}u "${MUI_FINISHPAGE_TEXT_REBOOT}"
        Pop $mui.FinishPage.Text
        SetCtlColors $mui.FinishPage.Text "" "${MUI_BGCOLOR}"
      
        ;Radio buttons for reboot page
        ${NSD_CreateRadioButton} 120u ${MUI_FINISHPAGE_REBOOTNOW_TOP}u 195u 10u "${MUI_FINISHPAGE_TEXT_REBOOTNOW}"
        Pop $mui.FinishPage.RebootNow
        SetCtlColors $mui.FinishPage.RebootNow "" "${MUI_BGCOLOR}"        
        ${NSD_CreateRadioButton} 120u ${MUI_FINISHPAGE_REBOOTLATER_TOP}u 195u 10u "${MUI_FINISHPAGE_TEXT_REBOOTLATER}"
        Pop $mui.FinishPage.RebootLater
        SetCtlColors $mui.FinishPage.RebootLater "" "${MUI_BGCOLOR}"
        !ifndef MUI_FINISHPAGE_REBOOTLATER_DEFAULT
          SendMessage $mui.FinishPage.RebootNow ${BM_SETCHECK} ${BST_CHECKED} 0
        !else
          SendMessage $mui.FinishPage.RebootLater ${BM_SETCHECK} ${BST_CHECKED} 0
        !endif
        ${NSD_SetFocus} $mui.FinishPage.RebootNow

      ${else}

    !endif
        
        ;Title text
        ${NSD_CreateLabel} 120u 10u 195u ${MUI_FINISHPAGE_TITLE_HEIGHT}u "${MUI_FINISHPAGE_TITLE}"
        Pop $mui.FinishPage.Title
        SetCtlColors $mui.FinishPage.Title "" "${MUI_BGCOLOR}"
        CreateFont $mui.FinishPage.Title.Font "$(^Font)" "12" "700"
        SendMessage $mui.FinishPage.Title ${WM_SETFONT} $mui.FinishPage.Title.Font 0

        ;Finish text
        !ifndef MUI_FINISHPAGE_RUN & MUI_FINISHPAGE_SHOWREADME
          ${NSD_CreateLabel} 120u ${MUI_FINISHPAGE_TEXT_TOP}u 195u ${MUI_FINISHPAGE_TEXT_HEIGHT}u "${MUI_FINISHPAGE_TEXT}"
        !else
          ${NSD_CreateLabel} 120u ${MUI_FINISHPAGE_TEXT_TOP}u 195u ${MUI_FINISHPAGE_TEXT_HEIGHT_BUTTONS}u "${MUI_FINISHPAGE_TEXT}"
        !endif
        Pop $mui.FinishPage.Text
        SetCtlColors $mui.FinishPage.Text "" "${MUI_BGCOLOR}"
    
        ;Checkboxes
        !ifdef MUI_FINISHPAGE_RUN
          ${NSD_CreateCheckbox} 120u ${MUI_FINISHPAGE_RUN_TOP}u 195u 10u "${MUI_FINISHPAGE_RUN_TEXT}"
          Pop $mui.FinishPage.Run
          SetCtlColors $mui.FinishPage.Run "" "${MUI_BGCOLOR}"
          !ifndef MUI_FINISHPAGE_RUN_NOTCHECKED
            SendMessage $mui.FinishPage.Run ${BM_SETCHECK} ${BST_CHECKED} 0
          !endif
          ${NSD_SetFocus} $mui.FinishPage.Run
        !endif
        !ifdef MUI_FINISHPAGE_SHOWREADME
          ${NSD_CreateCheckbox} 120u ${MUI_FINISHPAGE_SHOWREADME_TOP}u 195u 10u "${MUI_FINISHPAGE_SHOWREADME_TEXT}"
          Pop $mui.FinishPage.ShowReadme
          SetCtlColors $mui.FinishPage.ShowReadme "" "${MUI_BGCOLOR}"
          !ifndef MUI_FINISHPAGE_SHOWREADME_NOTCHECKED
            SendMessage $mui.FinishPage.ShowReadme ${BM_SETCHECK} ${BST_CHECKED} 0
          !endif
          !ifndef MUI_FINISHPAGE_RUN
            ${NSD_SetFocus} $mui.FinishPage.ShowReadme
          !endif
        !endif
    
        ;Link
        !ifdef MUI_FINISHPAGE_LINK
          ${NSD_CreateLink} 120u 175u 195u 10u "${MUI_FINISHPAGE_LINK}"
          Pop $mui.FinishPage.Link
          SetCtlColors $mui.FinishPage.Link "${MUI_FINISHPAGE_LINK_COLOR}" "${MUI_BGCOLOR}"
          ${NSD_OnClick} $mui.FinishPage.Link "${LINK}"
        !endif
        
    !ifndef MUI_FINISHPAGE_NOREBOOTSUPPORT        
      ${endif}
    !endif

    !ifdef MUI_FINISHPAGE_CANCEL_ENABLED
      StrCpy $mui.FinishPage.DisableAbortWarning "1"
    !endif

    ;Show page
    Call ${MUI_PAGE_UNINSTALLER_FUNCPREFIX}muiPageLoadFullWindow
    !insertmacro MUI_PAGE_FUNCTION_CUSTOM SHOW
    nsDialogs::Show
    Call ${MUI_PAGE_UNINSTALLER_FUNCPREFIX}muiPageUnloadFullWindow 
    
    !ifdef MUI_FINISHPAGE_CANCEL_ENABLED
      StrCpy $mui.FinishPage.DisableAbortWarning ""
    !endif
    
    ;Delete image from memory
    ${NSD_FreeImage} $mui.FinishPage.Image.Bitmap
    
    !insertmacro MUI_UNSET MUI_FINISHPAGE_TITLE_HEIGHT
    !insertmacro MUI_UNSET MUI_FINISHPAGE_TEXT_TOP
    !insertmacro MUI_UNSET MUI_FINISHPAGE_TEXT_HEIGHT
    !insertmacro MUI_UNSET MUI_FINISHPAGE_TEXT_HEIGHT_BUTTONS
    !insertmacro MUI_UNSET MUI_FINISHPAGE_TEXT_BOTTOM_BUTTONS
    !insertmacro MUI_UNSET MUI_FINISHPAGE_REBOOTNOW_TOP
    !insertmacro MUI_UNSET MUI_FINISHPAGE_REBOOTLATER_TOP
    !insertmacro MUI_UNSET MUI_FINISHPAGE_RUN_TOP
    !insertmacro MUI_UNSET MUI_FINISHPAGE_SHOWREADME_TOP

  FunctionEnd

  Function "${LEAVE}"

    !insertmacro MUI_PAGE_FUNCTION_CUSTOM LEAVE

    !ifndef MUI_FINISHPAGE_NOREBOOTSUPPORT

      ;Check whether the user has chosen to reboot the computer
      ${if} ${RebootFlag}
        SendMessage $mui.FinishPage.RebootNow ${BM_GETCHECK} 0 0 $mui.FinishPage.ReturnValue
        ${if} $mui.FinishPage.ReturnValue = ${BST_CHECKED}
          Reboot
        ${else}
          Return
        ${endif}
      ${endif}

    !endif

    ;Run functions depending on checkbox state 

    !ifdef MUI_FINISHPAGE_RUN
    
      SendMessage $mui.FinishPage.Run ${BM_GETCHECK} 0 0 $mui.FinishPage.ReturnValue

      ${if} $mui.FinishPage.ReturnValue = ${BST_CHECKED}
        !ifndef MUI_FINISHPAGE_RUN_FUNCTION
          !ifndef MUI_FINISHPAGE_RUN_PARAMETERS
            Exec "$\"${MUI_FINISHPAGE_RUN}$\""
          !else
            Exec "$\"${MUI_FINISHPAGE_RUN}$\" ${MUI_FINISHPAGE_RUN_PARAMETERS}"
          !endif
        !else
          Call "${MUI_FINISHPAGE_RUN_FUNCTION}"
        !endif
      ${endif}

    !endif

    !ifdef MUI_FINISHPAGE_SHOWREADME

      SendMessage $mui.FinishPage.ShowReadme ${BM_GETCHECK} 0 0 $mui.FinishPage.ReturnValue

      ${if} $mui.FinishPage.ReturnValue = ${BST_CHECKED}
        !ifndef MUI_FINISHPAGE_SHOWREADME_FUNCTION
          ExecShell open "${MUI_FINISHPAGE_SHOWREADME}"
        !else
          Call "${MUI_FINISHPAGE_SHOWREADME_FUNCTION}"
        !endif
      ${endif}

    !endif

  FunctionEnd

!macroend
