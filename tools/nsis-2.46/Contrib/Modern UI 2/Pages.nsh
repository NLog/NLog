/*

NSIS Modern User Interface
Support code for all pages

*/

;--------------------------------
;Page initialization

!macro MUI_PAGE_INIT

  ;Include interface settings in neccesary
  !insertmacro MUI_INTERFACE

  ;Define settings for installer page
  !insertmacro MUI_UNSET MUI_PAGE_UNINSTALLER
  !insertmacro MUI_UNSET MUI_PAGE_UNINSTALLER_PREFIX
  !insertmacro MUI_UNSET MUI_PAGE_UNINSTALLER_FUNCPREFIX
  
  !insertmacro MUI_SET MUI_PAGE_UNINSTALLER_PREFIX ""
  !insertmacro MUI_SET MUI_PAGE_UNINSTALLER_FUNCPREFIX ""

  ;Generate unique ID
  !insertmacro MUI_UNSET MUI_UNIQUEID
  !define MUI_UNIQUEID ${__LINE__}

!macroend

!macro MUI_UNPAGE_INIT

  ;Include interface settings
  !insertmacro MUI_INTERFACE

  ;Define prefixes for uninstaller page
  !insertmacro MUI_SET MUI_UNINSTALLER ""
  
  !insertmacro MUI_SET MUI_PAGE_UNINSTALLER ""
  !insertmacro MUI_SET MUI_PAGE_UNINSTALLER_PREFIX "UN"
  !insertmacro MUI_SET MUI_PAGE_UNINSTALLER_FUNCPREFIX "un."
  
  ;Generate unique ID
  !insertmacro MUI_UNSET MUI_UNIQUEID
  !define MUI_UNIQUEID ${__LINE__}  

!macroend


;--------------------------------
;Header text for standard MUI page

!macro MUI_HEADER_TEXT_PAGE TEXT SUBTEXT

  !ifdef MUI_PAGE_HEADER_TEXT & MUI_PAGE_HEADER_SUBTEXT
    !insertmacro MUI_HEADER_TEXT "${MUI_PAGE_HEADER_TEXT}" "${MUI_PAGE_HEADER_SUBTEXT}"
  !else ifdef MUI_PAGE_HEADER_TEXT
    !insertmacro MUI_HEADER_TEXT "${MUI_PAGE_HEADER_TEXT}" "${SUBTEXT}"
  !else ifdef MUI_PAGE_HEADER_SUBTEXT
    !insertmacro MUI_HEADER_TEXT "${TEXT}" "${MUI_PAGE_HEADER_SUBTEXT}"
  !else
    !insertmacro MUI_HEADER_TEXT "${TEXT}" "${SUBTEXT}"
  !endif

  !insertmacro MUI_UNSET MUI_PAGE_HEADER_TEXT
  !insertmacro MUI_UNSET MUI_PAGE_HEADER_SUBTEXT

!macroend


;--------------------------------
;Header text for custom page

!macro MUI_HEADER_TEXT TEXT SUBTEXT ;Called from script

  !verbose push
  !verbose ${MUI_VERBOSE}

  !ifdef MUI_HEADER_TRANSPARENT_TEXT
    LockWindow on
  !endif

  SendMessage $mui.Header.Text ${WM_SETTEXT} 0 "STR:${TEXT}"
  SendMessage $mui.Header.SubText ${WM_SETTEXT} 0 "STR:${SUBTEXT}"

  !ifdef MUI_HEADER_TRANSPARENT_TEXT
    LockWindow off
  !endif

  !verbose pop

!macroend


;--------------------------------
;Custom page functions

!macro MUI_PAGE_FUNCTION_CUSTOM TYPE

  !ifdef MUI_PAGE_CUSTOMFUNCTION_${TYPE}
    Call "${MUI_PAGE_CUSTOMFUNCTION_${TYPE}}"
    !undef MUI_PAGE_CUSTOMFUNCTION_${TYPE}
  !endif

!macroend


;--------------------------------
;Support for full window pages (like welcome/finish page)

!macro MUI_PAGE_FUNCTION_FULLWINDOW

  !ifndef MUI_${MUI_PAGE_UNINSTALLER_PREFIX}PAGE_FUNCTION_FULLWINDOW
    !define MUI_${MUI_PAGE_UNINSTALLER_PREFIX}PAGE_FUNCTION_FULLWINDOW

    Function ${MUI_PAGE_UNINSTALLER_FUNCPREFIX}muiPageLoadFullWindow
    
      LockWindow on
      
      ;The branding text needs to be hidden because the full windows page
      ;overlaps with it.
      ShowWindow $mui.Branding.Background ${SW_HIDE}
      ShowWindow $mui.Branding.Text ${SW_HIDE}      
      
      ;The texts need to be hidden because otherwise they may show through
      ;the page above when the Alt key is pressed.
      ShowWindow $mui.Header.Text ${SW_HIDE}
      ShowWindow $mui.Header.SubText ${SW_HIDE}
      ShowWindow $mui.Header.Image ${SW_HIDE}

      ;Show line below full width of page
      ShowWindow $mui.Line.Standard ${SW_HIDE}
      ShowWindow $mui.Line.FullWindow ${SW_NORMAL}
      
      LockWindow off
      
    FunctionEnd
    
    Function ${MUI_PAGE_UNINSTALLER_FUNCPREFIX}muiPageUnloadFullWindow
    
      ;Set everything back to normal again
    
      LockWindow on
      
      ShowWindow $mui.Branding.Background ${SW_NORMAL}
      ShowWindow $mui.Branding.Text ${SW_NORMAL}
      
      ShowWindow $mui.Header.Text ${SW_NORMAL}
      ShowWindow $mui.Header.SubText ${SW_NORMAL}
      ShowWindow $mui.Header.Image ${SW_NORMAL}
      
      ShowWindow $mui.Line.Standard ${SW_NORMAL}
      ShowWindow $mui.Line.FullWindow ${SW_HIDE}
      
      LockWindow off
      
    FunctionEnd    
    
  !endif

!macroend
