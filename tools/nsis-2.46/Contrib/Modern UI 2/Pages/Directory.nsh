/*

NSIS Modern User Interface
Directory page

*/

;--------------------------------
;Page interface settings and variables

!macro MUI_DIRECTORYPAGE_INTERFACE

  !ifndef MUI_DIRECTORYPAGE_INTERFACE
    !define MUI_DIRECTORYPAGE_INTERFACE
    Var mui.DirectoryPage
    
    Var mui.DirectoryPage.Text
    
    Var mui.DirectoryPage.DirectoryBox        
    Var mui.DirectoryPage.Directory
    Var mui.DirectoryPage.BrowseButton
    
    Var mui.DirectoryPage.SpaceRequired
    Var mui.DirectoryPage.SpaceAvailable    
  !endif

!macroend


;--------------------------------
;Page declaration

!macro MUI_PAGEDECLARATION_DIRECTORY

  !insertmacro MUI_SET MUI_${MUI_PAGE_UNINSTALLER_PREFIX}DIRECTORYPAGE ""
  !insertmacro MUI_DIRECTORYPAGE_INTERFACE

  !insertmacro MUI_DEFAULT MUI_DIRECTORYPAGE_TEXT_TOP ""
  !insertmacro MUI_DEFAULT MUI_DIRECTORYPAGE_TEXT_DESTINATION ""

  PageEx ${MUI_PAGE_UNINSTALLER_FUNCPREFIX}directory

    PageCallbacks ${MUI_PAGE_UNINSTALLER_FUNCPREFIX}mui.DirectoryPre_${MUI_UNIQUEID} ${MUI_PAGE_UNINSTALLER_FUNCPREFIX}mui.DirectoryShow_${MUI_UNIQUEID} ${MUI_PAGE_UNINSTALLER_FUNCPREFIX}mui.DirectoryLeave_${MUI_UNIQUEID}

    Caption " "

    DirText "${MUI_DIRECTORYPAGE_TEXT_TOP}" "${MUI_DIRECTORYPAGE_TEXT_DESTINATION}"

    !ifdef MUI_DIRECTORYPAGE_VARIABLE
      DirVar "${MUI_DIRECTORYPAGE_VARIABLE}"
    !endif

    !ifdef MUI_DIRECTORYPAGE_VERIFYONLEAVE
      DirVerify leave
    !endif

  PageExEnd

  !insertmacro MUI_FUNCTION_DIRECTORYPAGE ${MUI_PAGE_UNINSTALLER_FUNCPREFIX}mui.DirectoryPre_${MUI_UNIQUEID} ${MUI_PAGE_UNINSTALLER_FUNCPREFIX}mui.DirectoryShow_${MUI_UNIQUEID} ${MUI_PAGE_UNINSTALLER_FUNCPREFIX}mui.DirectoryLeave_${MUI_UNIQUEID}

  !undef MUI_DIRECTORYPAGE_TEXT_TOP
  !undef MUI_DIRECTORYPAGE_TEXT_DESTINATION
  !insertmacro MUI_UNSET MUI_DIRECTORYPAGE_VARIABLE
  !insertmacro MUI_UNSET MUI_DIRECTORYPAGE_VERIFYONLEAVE

!macroend

!macro MUI_PAGE_DIRECTORY

  !verbose push
  !verbose ${MUI_VERBOSE}

  !insertmacro MUI_PAGE_INIT
  !insertmacro MUI_PAGEDECLARATION_DIRECTORY

  !verbose pop

!macroend

!macro MUI_UNPAGE_DIRECTORY

  !verbose push
  !verbose ${MUI_VERBOSE}

  !insertmacro MUI_UNPAGE_INIT
  !insertmacro MUI_PAGEDECLARATION_DIRECTORY

  !verbose pop

!macroend


;--------------------------------
;Page functions

!macro MUI_FUNCTION_DIRECTORYPAGE PRE SHOW LEAVE

  Function "${PRE}"
    !insertmacro MUI_PAGE_FUNCTION_CUSTOM PRE
    !insertmacro MUI_HEADER_TEXT_PAGE $(MUI_${MUI_PAGE_UNINSTALLER_PREFIX}TEXT_DIRECTORY_TITLE) $(MUI_${MUI_PAGE_UNINSTALLER_PREFIX}TEXT_DIRECTORY_SUBTITLE)
  FunctionEnd

  Function "${SHOW}"
  
    ;Get control handles
    FindWindow $mui.DirectoryPage "#32770" "" $HWNDPARENT
    GetDlgItem $mui.DirectoryPage.Text $mui.DirectoryPage 1006
    GetDlgItem $mui.DirectoryPage.DirectoryBox $mui.DirectoryPage 1020
    GetDlgItem $mui.DirectoryPage.Directory $mui.DirectoryPage 1019 
    GetDlgItem $mui.DirectoryPage.BrowseButton $mui.DirectoryPage 1001
    GetDlgItem $mui.DirectoryPage.SpaceRequired $mui.DirectoryPage 1023    
    GetDlgItem $mui.DirectoryPage.SpaceAvailable $mui.DirectoryPage 1024
  
    !ifdef MUI_DIRECTORYPAGE_BGCOLOR
      SetCtlColors $mui.DirectoryPage.Directory "" "${MUI_DIRECTORYPAGE_BGCOLOR}"
    !endif
    
    !insertmacro MUI_PAGE_FUNCTION_CUSTOM SHOW
  FunctionEnd

  Function "${LEAVE}"
    !insertmacro MUI_PAGE_FUNCTION_CUSTOM LEAVE
  FunctionEnd

!macroend
