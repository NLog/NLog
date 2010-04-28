/*

NSIS Modern User Interface
Welcome page (implemented using nsDialogs)

*/

;--------------------------------
;Page interface settings and variables

!macro MUI_WELCOMEPAGE_INTERFACE

  !ifndef MUI_WELCOMEPAGE_INTERFACE
    !define MUI_WELCOMEPAGE_INTERFACE
    Var mui.WelcomePage
        
    Var mui.WelcomePage.Image
    Var mui.WelcomePage.Image.Bitmap
    
    Var mui.WelcomePage.Title
    Var mui.WelcomePage.Title.Font
    
    Var mui.WelcomePage.Text    
  !endif
  
  !insertmacro MUI_DEFAULT MUI_${MUI_PAGE_UNINSTALLER_PREFIX}WELCOMEFINISHPAGE_BITMAP "${NSISDIR}\Contrib\Graphics\Wizard\win.bmp"  

!macroend


;--------------------------------
;Interface initialization

!macro MUI_WELCOMEPAGE_GUIINIT

  !ifndef MUI_${MUI_PAGE_UNINSTALLER_PREFIX}WELCOMEWELCOMEPAGE_GUINIT
    !define MUI_${MUI_PAGE_UNINSTALLER_PREFIX}WELCOMEWELCOMEPAGE_GUINIT

    Function ${MUI_PAGE_UNINSTALLER_FUNCPREFIX}mui.WelcomePage.GUIInit
      
      InitPluginsDir
      File "/oname=$PLUGINSDIR\modern-wizard.bmp" "${MUI_${MUI_PAGE_UNINSTALLER_PREFIX}WELCOMEFINISHPAGE_BITMAP}"
    
      !ifdef MUI_${MUI_PAGE_UNINSTALLER_PREFIX}PAGE_FUNCTION_GUIINIT
        Call "${MUI_${MUI_PAGE_UNINSTALLER_PREFIX}PAGE_FUNCTION_GUIINIT}"
      !endif   
    
    FunctionEnd
  
    !insertmacro MUI_SET MUI_${MUI_PAGE_UNINSTALLER_PREFIX}PAGE_FUNCTION_GUIINIT ${MUI_PAGE_UNINSTALLER_FUNCPREFIX}mui.WelcomePage.GUIInit

  !endif

!macroend


;--------------------------------
;Page declaration

!macro MUI_PAGEDECLARATION_WELCOME

  !insertmacro MUI_SET MUI_${MUI_PAGE_UNINSTALLER_PREFIX}WELCOMEPAGE ""
  !insertmacro MUI_WELCOMEPAGE_INTERFACE
  
  !insertmacro MUI_WELCOMEPAGE_GUIINIT

  !insertmacro MUI_DEFAULT MUI_WELCOMEPAGE_TITLE "$(MUI_${MUI_PAGE_UNINSTALLER_PREFIX}TEXT_WELCOME_INFO_TITLE)"
  !insertmacro MUI_DEFAULT MUI_WELCOMEPAGE_TEXT "$(MUI_${MUI_PAGE_UNINSTALLER_PREFIX}TEXT_WELCOME_INFO_TEXT)"
  
  !insertmacro MUI_PAGE_FUNCTION_FULLWINDOW

  PageEx ${MUI_PAGE_UNINSTALLER_FUNCPREFIX}custom

    PageCallbacks ${MUI_PAGE_UNINSTALLER_FUNCPREFIX}mui.WelcomePre_${MUI_UNIQUEID} ${MUI_PAGE_UNINSTALLER_FUNCPREFIX}mui.WelcomeLeave_${MUI_UNIQUEID}

  PageExEnd

  !insertmacro MUI_FUNCTION_WELCOMEPAGE ${MUI_PAGE_UNINSTALLER_FUNCPREFIX}mui.WelcomePre_${MUI_UNIQUEID} ${MUI_PAGE_UNINSTALLER_FUNCPREFIX}mui.WelcomeLeave_${MUI_UNIQUEID}

  !insertmacro MUI_UNSET MUI_WELCOMEPAGE_TITLE
  !insertmacro MUI_UNSET MUI_WELCOMEPAGE_TITLE_3LINES
  !insertmacro MUI_UNSET MUI_WELCOMEPAGE_TEXT

!macroend

!macro MUI_PAGE_WELCOME

  !verbose push
  !verbose ${MUI_VERBOSE}

  !insertmacro MUI_PAGE_INIT
  !insertmacro MUI_PAGEDECLARATION_WELCOME

  !verbose pop

!macroend

!macro MUI_UNPAGE_WELCOME

  !verbose push
  !verbose ${MUI_VERBOSE}

  !insertmacro MUI_UNPAGE_INIT
  !insertmacro MUI_PAGEDECLARATION_WELCOME

  !verbose pop

!macroend


;--------------------------------
;Page functions

!macro MUI_FUNCTION_WELCOMEPAGE PRE LEAVE

  Function "${PRE}"
  
    !insertmacro MUI_PAGE_FUNCTION_CUSTOM PRE  

    ;Create dialog
    nsDialogs::Create 1044
    Pop $mui.WelcomePage
    nsDialogs::SetRTL $(^RTL)
    SetCtlColors $mui.WelcomePage "" "${MUI_BGCOLOR}"    

    ;Image control
    ${NSD_CreateBitmap} 0u 0u 109u 193u ""
    Pop $mui.WelcomePage.Image
    !ifndef MUI_${MUI_PAGE_UNINSTALLER_PREFIX}WELCOMEFINISHPAGE_BITMAP_NOSTRETCH
      ${NSD_SetStretchedImage} $mui.WelcomePage.Image $PLUGINSDIR\modern-wizard.bmp $mui.WelcomePage.Image.Bitmap
    !else
      ${NSD_SetImage} $mui.WelcomePage.Image $PLUGINSDIR\modern-wizard.bmp $mui.WelcomePage.Image.Bitmap
    !endif

    ;Positiong of controls

    ;Title    
    !ifndef MUI_WELCOMEPAGE_TITLE_3LINES
      !define MUI_WELCOMEPAGE_TITLE_HEIGHT 28
    !else
      !define MUI_WELCOMEPAGE_TITLE_HEIGHT 38
    !endif
    
    ;Text
    ;17 = 10 (top margin) + 7 (distance between texts)
    !define /math MUI_WELCOMEPAGE_TEXT_TOP 17 + ${MUI_WELCOMEPAGE_TITLE_HEIGHT}

    ;Title
    ${NSD_CreateLabel} 120u 10u 195u ${MUI_WELCOMEPAGE_TITLE_HEIGHT}u "${MUI_WELCOMEPAGE_TITLE}"
    Pop $mui.WelcomePage.Title
    SetCtlColors $mui.WelcomePage.Title "" "${MUI_BGCOLOR}"    
    CreateFont $mui.WelcomePage.Title.Font "$(^Font)" "12" "700"
    SendMessage $mui.WelcomePage.Title ${WM_SETFONT} $mui.WelcomePage.Title.Font 0

    ;Welcome text
    ${NSD_CreateLabel} 120u ${MUI_WELCOMEPAGE_TEXT_TOP}u 195u 130u "${MUI_WELCOMEPAGE_TEXT}"
    Pop $mui.WelcomePage.Text
    SetCtlColors $mui.WelcomePage.Text "" "${MUI_BGCOLOR}"

    ;Show page
    Call ${MUI_PAGE_UNINSTALLER_FUNCPREFIX}muiPageLoadFullWindow
    !insertmacro MUI_PAGE_FUNCTION_CUSTOM SHOW
    nsDialogs::Show
    Call ${MUI_PAGE_UNINSTALLER_FUNCPREFIX}muiPageUnloadFullWindow    

    ;Delete image from memory
    ${NSD_FreeImage} $mui.WelcomePage.Image.Bitmap

    !insertmacro MUI_UNSET MUI_WELCOMEPAGE_TITLE_HEIGHT
    !insertmacro MUI_UNSET MUI_WELCOMEPAGE_TEXT_TOP
    
  FunctionEnd

  Function "${LEAVE}"

    !insertmacro MUI_PAGE_FUNCTION_CUSTOM LEAVE

  FunctionEnd

!macroend
