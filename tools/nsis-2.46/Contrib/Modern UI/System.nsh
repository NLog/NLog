/*

NSIS Modern User Interface - Version 1.8
Copyright 2002-2009 Joost Verburg

*/

!echo "NSIS Modern User Interface version 1.8 - Copyright 2002-2009 Joost Verburg"

;--------------------------------

!ifndef MUI_INCLUDED
!define MUI_INCLUDED

!define MUI_SYSVERSION "1.8"

!verbose push

!ifndef MUI_VERBOSE
  !define MUI_VERBOSE 3
!endif

!verbose ${MUI_VERBOSE}

;--------------------------------
;HEADER FILES, DECLARATIONS

!include InstallOptions.nsh
!include LangFile.nsh
!include WinMessages.nsh

Var MUI_TEMP1
Var MUI_TEMP2

;--------------------------------
;INSERT CODE

!macro MUI_INSERT

  !ifndef MUI_INSERT
    !define MUI_INSERT

    !ifdef MUI_PRODUCT | MUI_VERSION
      !warning "The MUI_PRODUCT and MUI_VERSION defines have been removed. Use a normal Name command now."
    !endif

    !insertmacro MUI_INTERFACE

    !insertmacro MUI_FUNCTION_GUIINIT
    !insertmacro MUI_FUNCTION_ABORTWARNING
  
    !ifdef MUI_IOCONVERT_USED
      !insertmacro INSTALLOPTIONS_FUNCTION_WRITE_CONVERT
    !endif

    !ifdef MUI_UNINSTALLER
      !insertmacro MUI_UNFUNCTION_GUIINIT
      !insertmacro MUI_FUNCTION_UNABORTWARNING
    
      !ifdef MUI_UNIOCONVERT_USED
        !insertmacro INSTALLOPTIONS_UNFUNCTION_WRITE_CONVERT
      !endif
    !endif

  !endif

!macroend

;--------------------------------
;GENERAL

!macro MUI_DEFAULT SYMBOL CONTENT

  !ifndef "${SYMBOL}"
    !define "${SYMBOL}" "${CONTENT}"
  !endif

!macroend

!macro MUI_DEFAULT_IOCONVERT SYMBOL CONTENT

  !ifndef "${SYMBOL}"
    !define "${SYMBOL}" "${CONTENT}"
    !insertmacro MUI_SET "${SYMBOL}_DEFAULTSET"
    !insertmacro MUI_SET "MUI_${MUI_PAGE_UNINSTALLER_PREFIX}IOCONVERT_USED"
  !else
    !insertmacro MUI_UNSET "${SYMBOL}_DEFAULTSET" 
  !endif

!macroend

!macro MUI_SET SYMBOL

  !ifndef "${SYMBOL}"
    !define "${SYMBOL}"
  !endif

!macroend

!macro MUI_UNSET SYMBOL

  !ifdef "${SYMBOL}"
    !undef "${SYMBOL}"
  !endif

!macroend

;--------------------------------
;INTERFACE - COMPILE TIME SETTINGS

!macro MUI_INTERFACE

  !ifndef MUI_INTERFACE
    !define MUI_INTERFACE

    !ifdef MUI_INSERT_NSISCONF
      !insertmacro MUI_NSISCONF
    !endif

    !insertmacro MUI_DEFAULT MUI_UI "${NSISDIR}\Contrib\UIs\modern.exe"
    !insertmacro MUI_DEFAULT MUI_UI_HEADERIMAGE "${NSISDIR}\Contrib\UIs\modern_headerbmp.exe"
    !insertmacro MUI_DEFAULT MUI_UI_HEADERIMAGE_RIGHT "${NSISDIR}\Contrib\UIs\modern_headerbmpr.exe"
    !insertmacro MUI_DEFAULT MUI_UI_COMPONENTSPAGE_SMALLDESC "${NSISDIR}\Contrib\UIs\modern_smalldesc.exe"
    !insertmacro MUI_DEFAULT MUI_UI_COMPONENTSPAGE_NODESC "${NSISDIR}\Contrib\UIs\modern_nodesc.exe"
    !insertmacro MUI_DEFAULT MUI_ICON "${NSISDIR}\Contrib\Graphics\Icons\modern-install.ico"
    !insertmacro MUI_DEFAULT MUI_UNICON "${NSISDIR}\Contrib\Graphics\Icons\modern-uninstall.ico"
    !insertmacro MUI_DEFAULT MUI_COMPONENTSPAGE_CHECKBITMAP "${NSISDIR}\Contrib\Graphics\Checks\modern.bmp"
    !insertmacro MUI_DEFAULT MUI_LICENSEPAGE_BGCOLOR "/windows"
    !insertmacro MUI_DEFAULT MUI_INSTFILESPAGE_COLORS "/windows"
    !insertmacro MUI_DEFAULT MUI_INSTFILESPAGE_PROGRESSBAR "smooth"
    !insertmacro MUI_DEFAULT MUI_BGCOLOR "FFFFFF"
    !insertmacro MUI_DEFAULT MUI_WELCOMEFINISHPAGE_INI "${NSISDIR}\Contrib\Modern UI\ioSpecial.ini"
    !insertmacro MUI_DEFAULT MUI_UNWELCOMEFINISHPAGE_INI "${NSISDIR}\Contrib\Modern UI\ioSpecial.ini"
    !insertmacro MUI_DEFAULT MUI_WELCOMEFINISHPAGE_BITMAP "${NSISDIR}\Contrib\Graphics\Wizard\win.bmp"
    !insertmacro MUI_DEFAULT MUI_UNWELCOMEFINISHPAGE_BITMAP "${NSISDIR}\Contrib\Graphics\Wizard\win.bmp"

    !ifdef MUI_HEADERIMAGE

      !insertmacro MUI_DEFAULT MUI_HEADERIMAGE_BITMAP "${NSISDIR}\Contrib\Graphics\Header\nsis.bmp"

      !ifndef MUI_HEADERIMAGE_UNBITMAP
        !define MUI_HEADERIMAGE_UNBITMAP "${MUI_HEADERIMAGE_BITMAP}"
        !ifdef MUI_HEADERIMAGE_BITMAP_NOSTRETCH
          !insertmacro MUI_SET MUI_HEADERIMAGE_UNBITMAP_NOSTRETCH
        !endif
      !endif

      !ifdef MUI_HEADERIMAGE_BITMAP_RTL
        !ifndef MUI_HEADERIMAGE_UNBITMAP_RTL
          !define MUI_HEADERIMAGE_UNBITMAP_RTL "${MUI_HEADERIMAGE_BITMAP_RTL}"
          !ifdef MUI_HEADERIMAGE_BITMAP_RTL_NOSTRETCH
            !insertmacro MUI_SET MUI_HEADERIMAGE_UNBITMAP_RTL_NOSTRETCH
          !endif
        !endif
      !endif

    !endif

    XPStyle On

    ChangeUI all "${MUI_UI}"
    !ifdef MUI_HEADERIMAGE
      !ifndef MUI_HEADERIMAGE_RIGHT
        ChangeUI IDD_INST "${MUI_UI_HEADERIMAGE}"
      !else
        ChangeUI IDD_INST "${MUI_UI_HEADERIMAGE_RIGHT}"
      !endif
    !endif
    !ifdef MUI_COMPONENTSPAGE_SMALLDESC
      ChangeUI IDD_SELCOM "${MUI_UI_COMPONENTSPAGE_SMALLDESC}"
    !else ifdef MUI_COMPONENTSPAGE_NODESC
       ChangeUI IDD_SELCOM "${MUI_UI_COMPONENTSPAGE_NODESC}"
    !endif

    Icon "${MUI_ICON}"
    UninstallIcon "${MUI_UNICON}"

    CheckBitmap "${MUI_COMPONENTSPAGE_CHECKBITMAP}"
    LicenseBkColor "${MUI_LICENSEPAGE_BGCOLOR}"
    InstallColors ${MUI_INSTFILESPAGE_COLORS}
    InstProgressFlags ${MUI_INSTFILESPAGE_PROGRESSBAR}

    SubCaption 4 " "
    UninstallSubCaption 2 " "

    !insertmacro MUI_DEFAULT MUI_ABORTWARNING_TEXT "$(MUI_TEXT_ABORTWARNING)"
    !insertmacro MUI_DEFAULT MUI_UNABORTWARNING_TEXT "$(MUI_UNTEXT_ABORTWARNING)"

  !endif

!macroend

;--------------------------------
;INTERFACE - RUN-TIME

!macro MUI_INNERDIALOG_TEXT CONTROL TEXT

  !verbose push
  !verbose ${MUI_VERBOSE}

  FindWindow $MUI_TEMP1 "#32770" "" $HWNDPARENT
  GetDlgItem $MUI_TEMP1 $MUI_TEMP1 ${CONTROL}
  SendMessage $MUI_TEMP1 ${WM_SETTEXT} 0 "STR:${TEXT}"

  !verbose pop

!macroend

!macro MUI_HEADER_TEXT_INTERNAL ID TEXT

  GetDlgItem $MUI_TEMP1 $HWNDPARENT "${ID}"

  !ifdef MUI_HEADER_TRANSPARENT_TEXT

    ShowWindow $MUI_TEMP1 ${SW_HIDE}

  !endif

  SendMessage $MUI_TEMP1 ${WM_SETTEXT} 0 "STR:${TEXT}"

  !ifdef MUI_HEADER_TRANSPARENT_TEXT

    ShowWindow $MUI_TEMP1 ${SW_SHOWNA}

  !endif

!macroend

!macro MUI_HEADER_TEXT TEXT SUBTEXT

  !verbose push
  !verbose ${MUI_VERBOSE}

  !ifdef MUI_HEADER_TRANSPARENT_TEXT

    LockWindow on

  !endif

  !insertmacro MUI_HEADER_TEXT_INTERNAL 1037 "${TEXT}"
  !insertmacro MUI_HEADER_TEXT_INTERNAL 1038 "${SUBTEXT}"

  !ifdef MUI_HEADER_TRANSPARENT_TEXT

    LockWindow off

  !endif

  !verbose pop

!macroend

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

!macro MUI_DESCRIPTION_BEGIN

  FindWindow $MUI_TEMP1 "#32770" "" $HWNDPARENT
  GetDlgItem $MUI_TEMP1 $MUI_TEMP1 1043

  StrCmp $0 -1 0 mui.description_begin_done
    SendMessage $MUI_TEMP1 ${WM_SETTEXT} 0 "STR:"
    EnableWindow $MUI_TEMP1 0
    SendMessage $MUI_TEMP1 ${WM_SETTEXT} 0 "STR:$MUI_TEXT"
    Goto mui.description_done
  mui.description_begin_done:

!macroend

!macro MUI_DESCRIPTION_TEXT VAR TEXT

  !verbose push
  !verbose ${MUI_VERBOSE}

  StrCmp $0 ${VAR} 0 mui.description_${VAR}_done
    SendMessage $MUI_TEMP1 ${WM_SETTEXT} 0 "STR:"
    EnableWindow $MUI_TEMP1 1
    SendMessage $MUI_TEMP1 ${WM_SETTEXT} 0 "STR:${TEXT}"
    Goto mui.description_done
  mui.description_${VAR}_done:

  !verbose pop

!macroend

!macro MUI_DESCRIPTION_END

  !verbose push
  !verbose ${MUI_VERBOSE}

  mui.description_done:

  !verbose pop

!macroend

!macro MUI_ENDHEADER

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

!macroend

!macro MUI_ABORTWARNING

  !ifdef MUI_FINISHPAGE_ABORTWARNINGCHECK
    StrCmp $MUI_NOABORTWARNING "1" mui.quit
  !endif

  !ifdef MUI_ABORTWARNING_CANCEL_DEFAULT
    MessageBox MB_YESNO|MB_ICONEXCLAMATION|MB_DEFBUTTON2 "${MUI_ABORTWARNING_TEXT}" IDYES mui.quit
  !else
    MessageBox MB_YESNO|MB_ICONEXCLAMATION "${MUI_ABORTWARNING_TEXT}" IDYES mui.quit
  !endif

  Abort
  mui.quit:

!macroend

!macro MUI_UNABORTWARNING

  !ifdef MUI_UNABORTWARNING_CANCEL_DEFAULT
    MessageBox MB_YESNO|MB_ICONEXCLAMATION|MB_DEFBUTTON2 "${MUI_UNABORTWARNING_TEXT}" IDYES mui.quit
  !else
    MessageBox MB_YESNO|MB_ICONEXCLAMATION "${MUI_UNABORTWARNING_TEXT}" IDYES mui.quit
  !endif

  Abort
  mui.quit:

!macroend

!macro MUI_GUIINIT

  !insertmacro MUI_WELCOMEFINISHPAGE_INIT ""
  !insertmacro MUI_HEADERIMAGE_INIT ""

  !insertmacro MUI_GUIINIT_BASIC

!macroend

!macro MUI_UNGUIINIT

  !insertmacro MUI_WELCOMEFINISHPAGE_INIT "UN"
  !insertmacro MUI_HEADERIMAGE_INIT "UN"

  !insertmacro MUI_GUIINIT_BASIC

  !ifdef MUI_UNFINISHPAGE
    !ifndef MUI_UNFINISHPAGE_NOAUTOCLOSE
      SetAutoClose true
    !endif
  !endif

!macroend

!macro MUI_GUIINIT_BASIC

  GetDlgItem $MUI_TEMP1 $HWNDPARENT 1037
  CreateFont $MUI_TEMP2 "$(^Font)" "$(^FontSize)" "700"
  SendMessage $MUI_TEMP1 ${WM_SETFONT} $MUI_TEMP2 0

  !ifndef MUI_HEADER_TRANSPARENT_TEXT

    SetCtlColors $MUI_TEMP1 "" "${MUI_BGCOLOR}"

    GetDlgItem $MUI_TEMP1 $HWNDPARENT 1038
    SetCtlColors $MUI_TEMP1 "" "${MUI_BGCOLOR}"

  !else

    SetCtlColors $MUI_TEMP1 "" "transparent"

    GetDlgItem $MUI_TEMP1 $HWNDPARENT 1038
    SetCtlColors $MUI_TEMP1 "" "transparent"

  !endif

  GetDlgItem $MUI_TEMP1 $HWNDPARENT 1034
  SetCtlColors $MUI_TEMP1 "" "${MUI_BGCOLOR}"

  GetDlgItem $MUI_TEMP1 $HWNDPARENT 1039
  SetCtlColors $MUI_TEMP1 "" "${MUI_BGCOLOR}"

  GetDlgItem $MUI_TEMP1 $HWNDPARENT 1028
  SetCtlColors $MUI_TEMP1 /BRANDING
  GetDlgItem $MUI_TEMP1 $HWNDPARENT 1256
  SetCtlColors $MUI_TEMP1 /BRANDING
  SendMessage $MUI_TEMP1 ${WM_SETTEXT} 0 "STR:$(^Branding) "

!macroend

!macro MUI_WELCOMEFINISHPAGE_INIT UNINSTALLER

  !ifdef MUI_${UNINSTALLER}WELCOMEPAGE | MUI_${UNINSTALLER}FINISHPAGE

    !insertmacro INSTALLOPTIONS_EXTRACT_AS "${MUI_${UNINSTALLER}WELCOMEFINISHPAGE_INI}" "ioSpecial.ini"
    File "/oname=$PLUGINSDIR\modern-wizard.bmp" "${MUI_${UNINSTALLER}WELCOMEFINISHPAGE_BITMAP}"

    !insertmacro INSTALLOPTIONS_WRITE "ioSpecial.ini" "Field 1" "Text" "$PLUGINSDIR\modern-wizard.bmp"

    !ifdef MUI_${UNINSTALLER}WELCOMEFINISHPAGE_BITMAP_NOSTRETCH
      !insertmacro INSTALLOPTIONS_WRITE "ioSpecial.ini" "Field 1" "Flags" ""
    !endif

  !endif

!macroend

!macro MUI_HEADERIMAGE_INIT UNINSTALLER

  !ifdef MUI_HEADERIMAGE

    InitPluginsDir

    !ifdef MUI_HEADERIMAGE_${UNINSTALLER}BITMAP_RTL

    StrCmp $(^RTL) 0 mui.headerimageinit_nortl

        File "/oname=$PLUGINSDIR\modern-header.bmp" "${MUI_HEADERIMAGE_${UNINSTALLER}BITMAP_RTL}"

        !ifndef MUI_HEADERIMAGE_${UNINSTALLER}BITMAP_RTL_NOSTRETCH
          SetBrandingImage /IMGID=1046 /RESIZETOFIT "$PLUGINSDIR\modern-header.bmp"
        !else
          SetBrandingImage /IMGID=1046 "$PLUGINSDIR\modern-header.bmp"
        !endif

        Goto mui.headerimageinit_done

      mui.headerimageinit_nortl:

    !endif

        File "/oname=$PLUGINSDIR\modern-header.bmp" "${MUI_HEADERIMAGE_${UNINSTALLER}BITMAP}"

        !ifndef MUI_HEADERIMAGE_${UNINSTALLER}BITMAP_NOSTRETCH
          SetBrandingImage /IMGID=1046 /RESIZETOFIT "$PLUGINSDIR\modern-header.bmp"
        !else
          SetBrandingImage /IMGID=1046 "$PLUGINSDIR\modern-header.bmp"
        !endif

    !ifdef MUI_HEADERIMAGE_${UNINSTALLER}BITMAP_RTL

    mui.headerimageinit_done:

    !endif

  !endif

!macroend

;--------------------------------
;INTERFACE - FUNCTIONS

!macro MUI_FUNCTION_GUIINIT

  Function .onGUIInit

    !insertmacro MUI_GUIINIT

    !ifdef MUI_CUSTOMFUNCTION_GUIINIT
      Call "${MUI_CUSTOMFUNCTION_GUIINIT}"
    !endif

  FunctionEnd

!macroend

!macro MUI_FUNCTION_DESCRIPTION_BEGIN

  !verbose push
  !verbose ${MUI_VERBOSE}

  !ifndef MUI_VAR_TEXT
    Var MUI_TEXT
    !define MUI_VAR_TEXT
  !endif

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

!macro MUI_FUNCTION_ABORTWARNING

  Function .onUserAbort
    !ifdef MUI_ABORTWARNING
      !insertmacro MUI_ABORTWARNING
    !endif
    !ifdef MUI_CUSTOMFUNCTION_ABORT
      Call "${MUI_CUSTOMFUNCTION_ABORT}"
    !endif
  FunctionEnd

!macroend

!macro MUI_FUNCTION_UNABORTWARNING

  Function un.onUserAbort
    !ifdef MUI_UNABORTWARNING
      !insertmacro MUI_UNABORTWARNING
    !endif
    !ifdef MUI_CUSTOMFUNCTION_UNABORT
      Call "${MUI_CUSTOMFUNCTION_UNABORT}"
    !endif
  FunctionEnd

!macroend

!macro MUI_UNFUNCTION_GUIINIT

  Function un.onGUIInit

  !insertmacro MUI_UNGUIINIT

  !ifdef MUI_CUSTOMFUNCTION_UNGUIINIT
    Call "${MUI_CUSTOMFUNCTION_UNGUIINIT}"
  !endif

  FunctionEnd

!macroend

!macro MUI_FUNCTIONS_DESCRIPTION_BEGIN

  ;1.65 compatibility

  !warning "Modern UI macro name has changed. Please change MUI_FUNCTIONS_DESCRIPTION_BEGIN to MUI_FUNCTION_DESCRIPTION_BEGIN."

  !insertmacro MUI_FUNCTION_DESCRIPTION_BEGIN

!macroend

!macro MUI_FUNCTIONS_DESCRIPTION_END

  ;1.65 compatibility

  !warning "Modern UI macro name has changed. Please change MUI_FUNCTIONS_DESCRIPTION_END to MUI_FUNCTION_DESCRIPTION_END."

  !insertmacro MUI_FUNCTION_DESCRIPTION_END

!macroend

;--------------------------------
;START MENU FOLDER

!macro MUI_STARTMENU_GETFOLDER ID VAR

  !verbose push
  !verbose ${MUI_VERBOSE}

  !ifdef MUI_STARTMENUPAGE_${ID}_REGISTRY_ROOT & MUI_STARTMENUPAGE_${ID}_REGISTRY_KEY & MUI_STARTMENUPAGE_${ID}_REGISTRY_VALUENAME

    ReadRegStr $MUI_TEMP1 "${MUI_STARTMENUPAGE_${ID}_REGISTRY_ROOT}" "${MUI_STARTMENUPAGE_${ID}_REGISTRY_KEY}" "${MUI_STARTMENUPAGE_${ID}_REGISTRY_VALUENAME}"
      StrCmp $MUI_TEMP1 "" +3
        StrCpy "${VAR}" $MUI_TEMP1
        Goto +2

        StrCpy "${VAR}" "${MUI_STARTMENUPAGE_${ID}_DEFAULTFOLDER}"

   !else

     StrCpy "${VAR}" "${MUI_STARTMENUPAGE_${ID}_DEFAULTFOLDER}"

   !endif

  !verbose pop

!macroend

!macro MUI_STARTMENU_WRITE_BEGIN ID

  !verbose push
  !verbose ${MUI_VERBOSE}

  !define MUI_STARTMENUPAGE_CURRENT_ID "${ID}"

  StrCpy $MUI_TEMP1 "${MUI_STARTMENUPAGE_${MUI_STARTMENUPAGE_CURRENT_ID}_VARIABLE}" 1
  StrCmp $MUI_TEMP1 ">" mui.startmenu_write_${MUI_STARTMENUPAGE_CURRENT_ID}_done

  StrCmp "${MUI_STARTMENUPAGE_${MUI_STARTMENUPAGE_CURRENT_ID}_VARIABLE}" "" 0 mui.startmenu_writebegin_${MUI_STARTMENUPAGE_CURRENT_ID}_notempty

    !insertmacro MUI_STARTMENU_GETFOLDER "${MUI_STARTMENUPAGE_CURRENT_ID}" "${MUI_STARTMENUPAGE_${MUI_STARTMENUPAGE_CURRENT_ID}_VARIABLE}"

  mui.startmenu_writebegin_${MUI_STARTMENUPAGE_CURRENT_ID}_notempty:

  !verbose pop

!macroend

!macro MUI_STARTMENU_WRITE_END

  !verbose push
  !verbose ${MUI_VERBOSE}

  !ifdef MUI_STARTMENUPAGE_${MUI_STARTMENUPAGE_CURRENT_ID}_REGISTRY_ROOT & MUI_STARTMENUPAGE_${MUI_STARTMENUPAGE_CURRENT_ID}_REGISTRY_KEY & MUI_STARTMENUPAGE_${MUI_STARTMENUPAGE_CURRENT_ID}_REGISTRY_VALUENAME
    WriteRegStr "${MUI_STARTMENUPAGE_${MUI_STARTMENUPAGE_CURRENT_ID}_REGISTRY_ROOT}" "${MUI_STARTMENUPAGE_${MUI_STARTMENUPAGE_CURRENT_ID}_REGISTRY_KEY}" "${MUI_STARTMENUPAGE_${MUI_STARTMENUPAGE_CURRENT_ID}_REGISTRY_VALUENAME}" "${MUI_STARTMENUPAGE_${MUI_STARTMENUPAGE_CURRENT_ID}_VARIABLE}"
  !endif

  mui.startmenu_write_${MUI_STARTMENUPAGE_CURRENT_ID}_done:

  !undef MUI_STARTMENUPAGE_CURRENT_ID

  !verbose pop

!macroend

;--------------------------------
;PAGES

!macro MUI_PAGE_INIT

  !insertmacro MUI_INTERFACE

  !insertmacro MUI_DEFAULT MUI_PAGE_UNINSTALLER_PREFIX ""
  !insertmacro MUI_DEFAULT MUI_PAGE_UNINSTALLER_FUNCPREFIX ""

  !insertmacro MUI_UNSET MUI_UNIQUEID

  !define MUI_UNIQUEID ${__LINE__}

!macroend

!macro MUI_UNPAGE_INIT

  !ifndef MUI_UNINSTALLER
    !define MUI_UNINSTALLER
  !endif

  !define MUI_PAGE_UNINSTALLER

  !insertmacro MUI_UNSET MUI_PAGE_UNINSTALLER_PREFIX
  !insertmacro MUI_UNSET MUI_PAGE_UNINSTALLER_FUNCPREFIX

  !define MUI_PAGE_UNINSTALLER_PREFIX "UN"
  !define MUI_PAGE_UNINSTALLER_FUNCPREFIX "un."

!macroend

!macro MUI_UNPAGE_END

  !undef MUI_PAGE_UNINSTALLER
  !undef MUI_PAGE_UNINSTALLER_PREFIX
  !undef MUI_PAGE_UNINSTALLER_FUNCPREFIX

!macroend

!macro MUI_PAGE_WELCOME

  !verbose push
  !verbose ${MUI_VERBOSE}

  !insertmacro MUI_PAGE_INIT

  !insertmacro MUI_SET MUI_${MUI_PAGE_UNINSTALLER_PREFIX}WELCOMEPAGE

  !insertmacro MUI_DEFAULT_IOCONVERT MUI_WELCOMEPAGE_TITLE "$(MUI_${MUI_PAGE_UNINSTALLER_PREFIX}TEXT_WELCOME_INFO_TITLE)"
  !insertmacro MUI_DEFAULT_IOCONVERT MUI_WELCOMEPAGE_TEXT "$(MUI_${MUI_PAGE_UNINSTALLER_PREFIX}TEXT_WELCOME_INFO_TEXT)"

  !ifndef MUI_VAR_HWND
    Var MUI_HWND
    !define MUI_VAR_HWND
  !endif

  PageEx ${MUI_PAGE_UNINSTALLER_FUNCPREFIX}custom

    PageCallbacks ${MUI_PAGE_UNINSTALLER_FUNCPREFIX}mui.WelcomePre_${MUI_UNIQUEID} ${MUI_PAGE_UNINSTALLER_FUNCPREFIX}mui.WelcomeLeave_${MUI_UNIQUEID}

  PageExEnd

  !insertmacro MUI_FUNCTION_WELCOMEPAGE ${MUI_PAGE_UNINSTALLER_FUNCPREFIX}mui.WelcomePre_${MUI_UNIQUEID} ${MUI_PAGE_UNINSTALLER_FUNCPREFIX}mui.WelcomeLeave_${MUI_UNIQUEID}

  !insertmacro MUI_UNSET MUI_WELCOMEPAGE_TITLE
  !insertmacro MUI_UNSET MUI_WELCOMEPAGE_TITLE_3LINES
  !insertmacro MUI_UNSET MUI_WELCOMEPAGE_TEXT

  !verbose pop

!macroend

!macro MUI_PAGE_LICENSE LICENSEDATA

  !verbose push
  !verbose ${MUI_VERBOSE}

  !insertmacro MUI_PAGE_INIT

  !insertmacro MUI_SET MUI_${MUI_PAGE_UNINSTALLER_PREFIX}LICENSEPAGE

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

!macro MUI_PAGE_COMPONENTS

  !verbose push
  !verbose ${MUI_VERBOSE}

  !insertmacro MUI_PAGE_INIT

  !insertmacro MUI_SET MUI_${MUI_PAGE_UNINSTALLER_PREFIX}COMPONENTSPAGE

  !insertmacro MUI_DEFAULT MUI_COMPONENTSPAGE_TEXT_TOP ""
  !insertmacro MUI_DEFAULT MUI_COMPONENTSPAGE_TEXT_COMPLIST ""
  !insertmacro MUI_DEFAULT MUI_COMPONENTSPAGE_TEXT_INSTTYPE ""
  !insertmacro MUI_DEFAULT MUI_COMPONENTSPAGE_TEXT_DESCRIPTION_TITLE "$(MUI_INNERTEXT_COMPONENTS_DESCRIPTION_TITLE)"
  !insertmacro MUI_DEFAULT MUI_COMPONENTSPAGE_TEXT_DESCRIPTION_INFO "$(MUI_INNERTEXT_COMPONENTS_DESCRIPTION_INFO)"

  !ifndef MUI_VAR_TEXT
    Var MUI_TEXT
    !define MUI_VAR_TEXT
  !endif

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

  !verbose pop

!macroend

!macro MUI_PAGE_DIRECTORY

  !verbose push
  !verbose ${MUI_VERBOSE}

  !insertmacro MUI_PAGE_INIT

  !insertmacro MUI_SET MUI_${MUI_PAGE_UNINSTALLER_PREFIX}DIRECTORYPAGE

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
  !insertmacro MUI_UNSET MUI_DIRECTORYPAGE_BGCOLOR
  !insertmacro MUI_UNSET MUI_DIRECTORYPAGE_VARIABLE
  !insertmacro MUI_UNSET MUI_DIRECTORYPAGE_VERIFYONLEAVE

  !verbose pop

!macroend

!macro MUI_PAGE_STARTMENU ID VAR

  !verbose push
  !verbose ${MUI_VERBOSE}

  !insertmacro MUI_PAGE_INIT

  !insertmacro MUI_SET MUI_${MUI_PAGE_UNINSTALLER_PREFIX}STARTMENUPAGE

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

  !ifndef MUI_VAR_HWND
    Var MUI_HWND
    !define MUI_VAR_HWND
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
  !insertmacro MUI_UNSET MUI_STARTMENUPAGE_BGCOLOR

  !verbose pop

!macroend

!macro MUI_PAGE_INSTFILES

  !verbose push
  !verbose ${MUI_VERBOSE}

  !insertmacro MUI_PAGE_INIT

  !insertmacro MUI_SET MUI_${MUI_PAGE_UNINSTALLER_PREFIX}INSTFILESPAGE

  PageEx ${MUI_PAGE_UNINSTALLER_FUNCPREFIX}instfiles

    PageCallbacks ${MUI_PAGE_UNINSTALLER_FUNCPREFIX}mui.InstFilesPre_${MUI_UNIQUEID} ${MUI_PAGE_UNINSTALLER_FUNCPREFIX}mui.InstFilesShow_${MUI_UNIQUEID} ${MUI_PAGE_UNINSTALLER_FUNCPREFIX}mui.InstFilesLeave_${MUI_UNIQUEID}

    Caption " "

  PageExEnd

  !insertmacro MUI_FUNCTION_INSTFILESPAGE ${MUI_PAGE_UNINSTALLER_FUNCPREFIX}mui.InstFilesPre_${MUI_UNIQUEID} ${MUI_PAGE_UNINSTALLER_FUNCPREFIX}mui.InstFilesShow_${MUI_UNIQUEID} ${MUI_PAGE_UNINSTALLER_FUNCPREFIX}mui.InstFilesLeave_${MUI_UNIQUEID}

  !insertmacro MUI_UNSET MUI_INSTFILESPAGE_FINISHHEADER_TEXT
  !insertmacro MUI_UNSET MUI_INSTFILESPAGE_FINISHHEADER_SUBTEXT
  !insertmacro MUI_UNSET MUI_INSTFILESPAGE_ABORTWARNING_TEXT
  !insertmacro MUI_UNSET MUI_INSTFILESPAGE_ABORTWARNING_SUBTEXT

  !verbose pop

!macroend

!macro MUI_PAGE_FINISH

  !verbose push
  !verbose ${MUI_VERBOSE}

  !insertmacro MUI_PAGE_INIT

  !insertmacro MUI_SET MUI_${MUI_PAGE_UNINSTALLER_PREFIX}FINISHPAGE

  !insertmacro MUI_DEFAULT_IOCONVERT MUI_FINISHPAGE_TITLE "$(MUI_${MUI_PAGE_UNINSTALLER_PREFIX}TEXT_FINISH_INFO_TITLE)"
  !insertmacro MUI_DEFAULT_IOCONVERT MUI_FINISHPAGE_TEXT "$(MUI_${MUI_PAGE_UNINSTALLER_PREFIX}TEXT_FINISH_INFO_TEXT)"
  !insertmacro MUI_DEFAULT_IOCONVERT MUI_FINISHPAGE_BUTTON "$(MUI_BUTTONTEXT_FINISH)"
  !insertmacro MUI_DEFAULT_IOCONVERT MUI_FINISHPAGE_TEXT_REBOOT "$(MUI_${MUI_PAGE_UNINSTALLER_PREFIX}TEXT_FINISH_INFO_REBOOT)"
  !insertmacro MUI_DEFAULT_IOCONVERT MUI_FINISHPAGE_TEXT_REBOOTNOW "$(MUI_TEXT_FINISH_REBOOTNOW)"
  !insertmacro MUI_DEFAULT_IOCONVERT MUI_FINISHPAGE_TEXT_REBOOTLATER "$(MUI_TEXT_FINISH_REBOOTLATER)"
  !insertmacro MUI_DEFAULT_IOCONVERT MUI_FINISHPAGE_RUN_TEXT "$(MUI_TEXT_FINISH_RUN)"
  !insertmacro MUI_DEFAULT_IOCONVERT MUI_FINISHPAGE_SHOWREADME_TEXT "$(MUI_TEXT_FINISH_SHOWREADME)"
  !insertmacro MUI_DEFAULT MUI_FINISHPAGE_LINK_COLOR "000080"

  !ifndef MUI_VAR_HWND
    Var MUI_HWND
    !define MUI_VAR_HWND
  !endif

  !ifndef MUI_PAGE_UNINSTALLER
    !ifndef MUI_FINISHPAGE_NOAUTOCLOSE
      AutoCloseWindow true
    !endif
  !endif

  !ifdef MUI_FINISHPAGE_CANCEL_ENABLED
    !ifndef MUI_VAR_NOABORTWARNING
      !define MUI_VAR_NOABORTWARNING
      Var MUI_NOABORTWARNING
    !endif
  !endif

  PageEx ${MUI_PAGE_UNINSTALLER_FUNCPREFIX}custom

    PageCallbacks ${MUI_PAGE_UNINSTALLER_FUNCPREFIX}mui.FinishPre_${MUI_UNIQUEID} ${MUI_PAGE_UNINSTALLER_FUNCPREFIX}mui.FinishLeave_${MUI_UNIQUEID}

    Caption " "

  PageExEnd

  !insertmacro MUI_FUNCTION_FINISHPAGE ${MUI_PAGE_UNINSTALLER_FUNCPREFIX}mui.FinishPre_${MUI_UNIQUEID} ${MUI_PAGE_UNINSTALLER_FUNCPREFIX}mui.FinishLeave_${MUI_UNIQUEID}

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

  !insertmacro MUI_UNSET MUI_FINISHPAGE_CURFIELD_TOP
  !insertmacro MUI_UNSET MUI_FINISHPAGE_CURFIELD_BOTTOM

  !verbose pop

!macroend

!macro MUI_UNPAGE_WELCOME

  !verbose push
  !verbose ${MUI_VERBOSE}

  !insertmacro MUI_UNPAGE_INIT

    !insertmacro MUI_PAGE_WELCOME

  !insertmacro MUI_UNPAGE_END

  !verbose pop

!macroend

!macro MUI_UNPAGE_CONFIRM

  !verbose push
  !verbose ${MUI_VERBOSE}

  !ifndef MUI_UNINSTALLER
    !define MUI_UNINSTALLER
  !endif

  !insertmacro MUI_PAGE_INIT

  !insertmacro MUI_SET MUI_UNCONFIRMPAGE

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

  !verbose pop

!macroend

!macro MUI_UNPAGE_LICENSE LICENSEDATA

  !verbose push
  !verbose ${MUI_VERBOSE}

  !insertmacro MUI_UNPAGE_INIT

    !insertmacro MUI_PAGE_LICENSE "${LICENSEDATA}"

  !insertmacro MUI_UNPAGE_END

  !verbose pop

!macroend

!macro MUI_UNPAGE_COMPONENTS

  !verbose push
  !verbose ${MUI_VERBOSE}

  !insertmacro MUI_UNPAGE_INIT

    !insertmacro MUI_PAGE_COMPONENTS

  !insertmacro MUI_UNPAGE_END

  !verbose pop

!macroend

!macro MUI_UNPAGE_DIRECTORY

  !verbose push
  !verbose ${MUI_VERBOSE}

  !insertmacro MUI_UNPAGE_INIT

    !insertmacro MUI_PAGE_DIRECTORY

  !insertmacro MUI_UNPAGE_END

  !verbose pop

!macroend

!macro MUI_UNPAGE_INSTFILES

  !verbose push
  !verbose ${MUI_VERBOSE}

  !insertmacro MUI_UNPAGE_INIT

    !insertmacro MUI_PAGE_INSTFILES

  !insertmacro MUI_UNPAGE_END

  !verbose pop

!macroend

!macro MUI_UNPAGE_FINISH

  !verbose push
  !verbose ${MUI_VERBOSE}

  !insertmacro MUI_UNPAGE_INIT

    !insertmacro MUI_PAGE_FINISH

  !insertmacro MUI_UNPAGE_END

  !verbose pop

!macroend

;--------------------------------
;PAGE FUNCTIONS

!macro MUI_PAGE_FUNCTION_CUSTOM TYPE

  !ifdef MUI_PAGE_CUSTOMFUNCTION_${TYPE}
    Call "${MUI_PAGE_CUSTOMFUNCTION_${TYPE}}"
    !undef MUI_PAGE_CUSTOMFUNCTION_${TYPE}
  !endif

!macroend

!macro MUI_WELCOMEFINISHPAGE_FUNCTION_CUSTOM

  !ifdef MUI_WELCOMEFINISHPAGE_CUSTOMFUNCTION_INIT
    Call "${MUI_WELCOMEFINISHPAGE_CUSTOMFUNCTION_INIT}"
    !undef MUI_WELCOMEFINISHPAGE_CUSTOMFUNCTION_INIT
  !endif

!macroend

!macro MUI_FUNCTION_WELCOMEPAGE PRE LEAVE

  Function "${PRE}"

    !insertmacro MUI_WELCOMEFINISHPAGE_FUNCTION_CUSTOM

    !insertmacro INSTALLOPTIONS_WRITE "ioSpecial.ini" "Settings" "NumFields" "3"
    !insertmacro INSTALLOPTIONS_WRITE "ioSpecial.ini" "Settings" "NextButtonText" ""
    !insertmacro INSTALLOPTIONS_WRITE "ioSpecial.ini" "Settings" "CancelEnabled" ""

    !insertmacro MUI_INSTALLOPTIONS_WRITE_DEFAULTCONVERT "ioSpecial.ini" "Field 2" "Text" MUI_WELCOMEPAGE_TITLE

    !ifndef MUI_WELCOMEPAGE_TITLE_3LINES
      !insertmacro INSTALLOPTIONS_WRITE "ioSpecial.ini" "Field 2" "Bottom" "38"
      !insertmacro INSTALLOPTIONS_WRITE "ioSpecial.ini" "Field 3" "Top" "45"
    !else
      !insertmacro INSTALLOPTIONS_WRITE "ioSpecial.ini" "Field 2" "Bottom" "48"
      !insertmacro INSTALLOPTIONS_WRITE "ioSpecial.ini" "Field 3" "Top" "55"
    !endif

    !insertmacro INSTALLOPTIONS_WRITE "ioSpecial.ini" "Field 3" "Bottom" "185"
    !insertmacro MUI_INSTALLOPTIONS_WRITE_DEFAULTCONVERT "ioSpecial.ini" "Field 3" "Text" MUI_WELCOMEPAGE_TEXT

    !insertmacro MUI_PAGE_FUNCTION_CUSTOM PRE

    LockWindow on
    GetDlgItem $MUI_TEMP1 $HWNDPARENT 1028
    ShowWindow $MUI_TEMP1 ${SW_HIDE}

    GetDlgItem $MUI_TEMP1 $HWNDPARENT 1256
    ShowWindow $MUI_TEMP1 ${SW_HIDE}

    GetDlgItem $MUI_TEMP1 $HWNDPARENT 1035
    ShowWindow $MUI_TEMP1 ${SW_HIDE}

    GetDlgItem $MUI_TEMP1 $HWNDPARENT 1037
    ShowWindow $MUI_TEMP1 ${SW_HIDE}

    GetDlgItem $MUI_TEMP1 $HWNDPARENT 1038
    ShowWindow $MUI_TEMP1 ${SW_HIDE}

    GetDlgItem $MUI_TEMP1 $HWNDPARENT 1039
    ShowWindow $MUI_TEMP1 ${SW_HIDE}

    GetDlgItem $MUI_TEMP1 $HWNDPARENT 1045
    ShowWindow $MUI_TEMP1 ${SW_NORMAL}
    LockWindow off

    !insertmacro INSTALLOPTIONS_INITDIALOG "ioSpecial.ini"
    Pop $MUI_HWND
    SetCtlColors $MUI_HWND "" "${MUI_BGCOLOR}"

    GetDlgItem $MUI_TEMP1 $MUI_HWND 1201
    SetCtlColors $MUI_TEMP1 "" "${MUI_BGCOLOR}"

    CreateFont $MUI_TEMP2 "$(^Font)" "12" "700"
    SendMessage $MUI_TEMP1 ${WM_SETFONT} $MUI_TEMP2 0

    GetDlgItem $MUI_TEMP1 $MUI_HWND 1202
    SetCtlColors $MUI_TEMP1 "" "${MUI_BGCOLOR}"

    !insertmacro MUI_PAGE_FUNCTION_CUSTOM SHOW

    !insertmacro INSTALLOPTIONS_SHOW

    LockWindow on
    GetDlgItem $MUI_TEMP1 $HWNDPARENT 1028
    ShowWindow $MUI_TEMP1 ${SW_NORMAL}

    GetDlgItem $MUI_TEMP1 $HWNDPARENT 1256
    ShowWindow $MUI_TEMP1 ${SW_NORMAL}

    GetDlgItem $MUI_TEMP1 $HWNDPARENT 1035
    ShowWindow $MUI_TEMP1 ${SW_NORMAL}

    GetDlgItem $MUI_TEMP1 $HWNDPARENT 1037
    ShowWindow $MUI_TEMP1 ${SW_NORMAL}

    GetDlgItem $MUI_TEMP1 $HWNDPARENT 1038
    ShowWindow $MUI_TEMP1 ${SW_NORMAL}

    GetDlgItem $MUI_TEMP1 $HWNDPARENT 1039
    ShowWindow $MUI_TEMP1 ${SW_NORMAL}

    GetDlgItem $MUI_TEMP1 $HWNDPARENT 1045
    ShowWindow $MUI_TEMP1 ${SW_HIDE}
    LockWindow off

  FunctionEnd

  Function "${LEAVE}"

    !insertmacro MUI_PAGE_FUNCTION_CUSTOM LEAVE

  FunctionEnd

!macroend

!macro MUI_FUNCTION_LICENSEPAGE PRE SHOW LEAVE

  Function "${PRE}"

    !insertmacro MUI_PAGE_FUNCTION_CUSTOM PRE
    !insertmacro MUI_HEADER_TEXT_PAGE $(MUI_${MUI_PAGE_UNINSTALLER_PREFIX}TEXT_LICENSE_TITLE) $(MUI_${MUI_PAGE_UNINSTALLER_PREFIX}TEXT_LICENSE_SUBTITLE)

  FunctionEnd

  Function "${SHOW}"

    !insertmacro MUI_INNERDIALOG_TEXT 1040 "${MUI_LICENSEPAGE_TEXT_TOP}"
    !insertmacro MUI_PAGE_FUNCTION_CUSTOM SHOW

  FunctionEnd

  Function "${LEAVE}"

    !insertmacro MUI_PAGE_FUNCTION_CUSTOM LEAVE

  FunctionEnd

!macroend

!macro MUI_FUNCTION_COMPONENTSPAGE PRE SHOW LEAVE

  Function "${PRE}"
    !insertmacro MUI_PAGE_FUNCTION_CUSTOM PRE
    !insertmacro MUI_HEADER_TEXT_PAGE $(MUI_${MUI_PAGE_UNINSTALLER_PREFIX}TEXT_COMPONENTS_TITLE) $(MUI_${MUI_PAGE_UNINSTALLER_PREFIX}TEXT_COMPONENTS_SUBTITLE)
  FunctionEnd

  Function "${SHOW}"

    !insertmacro MUI_INNERDIALOG_TEXT 1042 "${MUI_COMPONENTSPAGE_TEXT_DESCRIPTION_TITLE}"

    FindWindow $MUI_TEMP1 "#32770" "" $HWNDPARENT
    GetDlgItem $MUI_TEMP1 $MUI_TEMP1 1043
    EnableWindow $MUI_TEMP1 0

    !insertmacro MUI_INNERDIALOG_TEXT 1043 "${MUI_COMPONENTSPAGE_TEXT_DESCRIPTION_INFO}"
    StrCpy $MUI_TEXT "${MUI_COMPONENTSPAGE_TEXT_DESCRIPTION_INFO}"
    !insertmacro MUI_PAGE_FUNCTION_CUSTOM SHOW

  FunctionEnd

  Function "${LEAVE}"

    !insertmacro MUI_PAGE_FUNCTION_CUSTOM LEAVE

  FunctionEnd

!macroend

!macro MUI_FUNCTION_DIRECTORYPAGE PRE SHOW LEAVE

  Function "${PRE}"
    !insertmacro MUI_PAGE_FUNCTION_CUSTOM PRE
    !insertmacro MUI_HEADER_TEXT_PAGE $(MUI_${MUI_PAGE_UNINSTALLER_PREFIX}TEXT_DIRECTORY_TITLE) $(MUI_${MUI_PAGE_UNINSTALLER_PREFIX}TEXT_DIRECTORY_SUBTITLE)
  FunctionEnd

  Function "${SHOW}"
    !ifdef MUI_DIRECTORYPAGE_BGCOLOR
      FindWindow $MUI_TEMP1 "#32770" "" $HWNDPARENT
      GetDlgItem $MUI_TEMP1 $MUI_TEMP1 1019
      SetCtlColors $MUI_TEMP1 "" "${MUI_DIRECTORYPAGE_BGCOLOR}"
    !endif
    
    !insertmacro MUI_PAGE_FUNCTION_CUSTOM SHOW
  FunctionEnd

  Function "${LEAVE}"
    !insertmacro MUI_PAGE_FUNCTION_CUSTOM LEAVE
  FunctionEnd

!macroend

!macro MUI_FUNCTION_STARTMENUPAGE PRE LEAVE

  Function "${PRE}"

    !insertmacro MUI_PAGE_FUNCTION_CUSTOM PRE

     !ifdef MUI_STARTMENUPAGE_REGISTRY_ROOT & MUI_STARTMENUPAGE_REGISTRY_KEY & MUI_STARTMENUPAGE_REGISTRY_VALUENAME

      StrCmp "${MUI_STARTMENUPAGE_VARIABLE}" "" 0 +4

      ReadRegStr $MUI_TEMP1 "${MUI_STARTMENUPAGE_REGISTRY_ROOT}" "${MUI_STARTMENUPAGE_REGISTRY_KEY}" "${MUI_STARTMENUPAGE_REGISTRY_VALUENAME}"
        StrCmp $MUI_TEMP1 "" +2
          StrCpy "${MUI_STARTMENUPAGE_VARIABLE}" $MUI_TEMP1

    !endif

    !insertmacro MUI_HEADER_TEXT_PAGE $(MUI_TEXT_STARTMENU_TITLE) $(MUI_TEXT_STARTMENU_SUBTITLE)

    StrCmp $(^RTL) 0 mui.startmenu_nortl
      !ifndef MUI_STARTMENUPAGE_NODISABLE
        StartMenu::Init /rtl /noicon /autoadd /text "${MUI_STARTMENUPAGE_TEXT_TOP}" /lastused "${MUI_STARTMENUPAGE_VARIABLE}" /checknoshortcuts "${MUI_STARTMENUPAGE_TEXT_CHECKBOX}" "${MUI_STARTMENUPAGE_DEFAULTFOLDER}"
      !else
        StartMenu::Init /rtl /noicon /autoadd /text "${MUI_STARTMENUPAGE_TEXT_TOP}" /lastused "${MUI_STARTMENUPAGE_VARIABLE}" "${MUI_STARTMENUPAGE_DEFAULTFOLDER}"
      !endif
      Goto mui.startmenu_initdone
    mui.startmenu_nortl:
      !ifndef MUI_STARTMENUPAGE_NODISABLE
        StartMenu::Init /noicon /autoadd /text "${MUI_STARTMENUPAGE_TEXT_TOP}" /lastused "${MUI_STARTMENUPAGE_VARIABLE}" /checknoshortcuts "${MUI_STARTMENUPAGE_TEXT_CHECKBOX}" "${MUI_STARTMENUPAGE_DEFAULTFOLDER}"
      !else
        StartMenu::Init /noicon /autoadd /text "${MUI_STARTMENUPAGE_TEXT_TOP}" /lastused "${MUI_STARTMENUPAGE_VARIABLE}" "${MUI_STARTMENUPAGE_DEFAULTFOLDER}"
      !endif
    mui.startmenu_initdone:

  Pop $MUI_HWND

  !ifdef MUI_STARTMENUPAGE_BGCOLOR
    GetDlgItem $MUI_TEMP1 $MUI_HWND 1002
    SetCtlColors $MUI_TEMP1 "" "${MUI_STARTMENUPAGE_BGCOLOR}"
    GetDlgItem $MUI_TEMP1 $MUI_HWND 1004
    SetCtlColors $MUI_TEMP1 "" "${MUI_STARTMENUPAGE_BGCOLOR}"
  !endif

  !insertmacro MUI_PAGE_FUNCTION_CUSTOM SHOW

  StartMenu::Show

    Pop $MUI_TEMP1
    StrCmp $MUI_TEMP1 "success" 0 +2
      Pop "${MUI_STARTMENUPAGE_VARIABLE}"

  FunctionEnd

  Function "${LEAVE}"

    !insertmacro MUI_PAGE_FUNCTION_CUSTOM LEAVE

  FunctionEnd

!macroend

!macro MUI_FUNCTION_INSTFILESPAGE PRE SHOW LEAVE

  Function "${PRE}"

    !insertmacro MUI_PAGE_FUNCTION_CUSTOM PRE
    !insertmacro MUI_HEADER_TEXT_PAGE $(MUI_${MUI_PAGE_UNINSTALLER_PREFIX}TEXT_${MUI_PAGE_UNINSTALLER_PREFIX}INSTALLING_TITLE) $(MUI_${MUI_PAGE_UNINSTALLER_PREFIX}TEXT_${MUI_PAGE_UNINSTALLER_PREFIX}INSTALLING_SUBTITLE)

  FunctionEnd

  Function "${SHOW}"

    !insertmacro MUI_PAGE_FUNCTION_CUSTOM SHOW

  FunctionEnd

  Function "${LEAVE}"

    !insertmacro MUI_PAGE_FUNCTION_CUSTOM LEAVE

    !insertmacro MUI_ENDHEADER
    !insertmacro MUI_LANGDLL_SAVELANGUAGE

  FunctionEnd

!macroend

!macro MUI_FUNCTION_FINISHPAGE PRE LEAVE

  Function "${PRE}"

    !insertmacro MUI_WELCOMEFINISHPAGE_FUNCTION_CUSTOM

    !insertmacro MUI_INSTALLOPTIONS_WRITE_DEFAULTCONVERT "ioSpecial.ini" "Settings" "NextButtonText" MUI_FINISHPAGE_BUTTON

    !ifdef MUI_FINISHPAGE_CANCEL_ENABLED
      !insertmacro INSTALLOPTIONS_WRITE "ioSpecial.ini" "Settings" "CancelEnabled" "1"
    !endif

    !ifndef MUI_FINISHPAGE_TITLE_3LINES
      !insertmacro INSTALLOPTIONS_WRITE "ioSpecial.ini" "Field 2" "Bottom" "38"
      !insertmacro INSTALLOPTIONS_WRITE "ioSpecial.ini" "Field 3" "Top" "45"
    !else
      !insertmacro INSTALLOPTIONS_WRITE "ioSpecial.ini" "Field 2" "Bottom" "48"
      !insertmacro INSTALLOPTIONS_WRITE "ioSpecial.ini" "Field 3" "Top" "55"
    !endif

    !insertmacro MUI_INSTALLOPTIONS_WRITE_DEFAULTCONVERT "ioSpecial.ini" "Field 2" "Text" MUI_FINISHPAGE_TITLE

    !ifdef MUI_FINISHPAGE_RUN | MUI_FINISHPAGE_SHOWREADME
      !ifndef MUI_FINISHPAGE_TITLE_3LINES
        !ifndef MUI_FINISHPAGE_TEXT_LARGE
          !insertmacro INSTALLOPTIONS_WRITE "ioSpecial.ini" "Field 3" "Bottom" "85"
        !else
          !insertmacro INSTALLOPTIONS_WRITE "ioSpecial.ini" "Field 3" "Bottom" "115"
        !endif
      !else
        !ifndef MUI_FINISHPAGE_TEXT_LARGE
          !insertmacro INSTALLOPTIONS_WRITE "ioSpecial.ini" "Field 3" "Bottom" "95"
        !else
          !insertmacro INSTALLOPTIONS_WRITE "ioSpecial.ini" "Field 3" "Bottom" "125"
        !endif
      !endif
    !else
      !ifndef MUI_FINISHPAGE_LINK
        !insertmacro INSTALLOPTIONS_WRITE "ioSpecial.ini" "Field 3" "Bottom" "185"
      !else
        !insertmacro INSTALLOPTIONS_WRITE "ioSpecial.ini" "Field 3" "Bottom" "175"
      !endif
    !endif

    !ifndef MUI_FINISHPAGE_NOREBOOTSUPPORT

      IfRebootFlag 0 mui.finish_noreboot_init

        !ifndef MUI_FINISHPAGE_TITLE_3LINES
          !ifndef MUI_FINISHPAGE_TEXT_LARGE
            !insertmacro INSTALLOPTIONS_WRITE "ioSpecial.ini" "Field 3" "Bottom" "85"
          !else
            !insertmacro INSTALLOPTIONS_WRITE "ioSpecial.ini" "Field 3" "Bottom" "115"
          !endif
        !else
          !ifndef MUI_FINISHPAGE_TEXT_LARGE
            !insertmacro INSTALLOPTIONS_WRITE "ioSpecial.ini" "Field 3" "Bottom" "95"
          !else
            !insertmacro INSTALLOPTIONS_WRITE "ioSpecial.ini" "Field 3" "Bottom" "125"
          !endif
        !endif

        !insertmacro MUI_INSTALLOPTIONS_WRITE_DEFAULTCONVERT "ioSpecial.ini" "Field 3" "Text" MUI_FINISHPAGE_TEXT_REBOOT

        !insertmacro INSTALLOPTIONS_WRITE "ioSpecial.ini" "Settings" "Numfields" "5"

        !insertmacro INSTALLOPTIONS_WRITE "ioSpecial.ini" "Field 4" "Type" "RadioButton"
        !insertmacro MUI_INSTALLOPTIONS_WRITE_DEFAULTCONVERT "ioSpecial.ini" "Field 4" "Text" MUI_FINISHPAGE_TEXT_REBOOTNOW
        !insertmacro INSTALLOPTIONS_WRITE "ioSpecial.ini" "Field 4" "Left" "120"
        !insertmacro INSTALLOPTIONS_WRITE "ioSpecial.ini" "Field 4" "Right" "321"
        !ifndef MUI_FINISHPAGE_TITLE_3LINES
          !ifndef MUI_FINISHPAGE_TEXT_LARGE
            !insertmacro INSTALLOPTIONS_WRITE "ioSpecial.ini" "Field 4" "Top" "90"
            !insertmacro INSTALLOPTIONS_WRITE "ioSpecial.ini" "Field 4" "Bottom" "100"
          !else
            !insertmacro INSTALLOPTIONS_WRITE "ioSpecial.ini" "Field 4" "Top" "120"
            !insertmacro INSTALLOPTIONS_WRITE "ioSpecial.ini" "Field 4" "Bottom" "130"
          !endif
        !else
          !ifndef MUI_FINISHPAGE_TEXT_LARGE
            !insertmacro INSTALLOPTIONS_WRITE "ioSpecial.ini" "Field 4" "Top" "100"
            !insertmacro INSTALLOPTIONS_WRITE "ioSpecial.ini" "Field 4" "Bottom" "110"
          !else
            !insertmacro INSTALLOPTIONS_WRITE "ioSpecial.ini" "Field 4" "Top" "130"
            !insertmacro INSTALLOPTIONS_WRITE "ioSpecial.ini" "Field 4" "Bottom" "140"
          !endif
        !endif
        !insertmacro INSTALLOPTIONS_WRITE "ioSpecial.ini" "Field 5" "Type" "RadioButton"
        !insertmacro MUI_INSTALLOPTIONS_WRITE_DEFAULTCONVERT "ioSpecial.ini" "Field 5" "Text" MUI_FINISHPAGE_TEXT_REBOOTLATER
        !insertmacro INSTALLOPTIONS_WRITE "ioSpecial.ini" "Field 5" "Left" "120"
        !insertmacro INSTALLOPTIONS_WRITE "ioSpecial.ini" "Field 5" "Right" "321"
        !ifndef MUI_FINISHPAGE_TITLE_3LINES
          !insertmacro INSTALLOPTIONS_WRITE "ioSpecial.ini" "Field 5" "Top" "110"
          !insertmacro INSTALLOPTIONS_WRITE "ioSpecial.ini" "Field 5" "Bottom" "120"
        !else
          !insertmacro INSTALLOPTIONS_WRITE "ioSpecial.ini" "Field 5" "Top" "110"
          !insertmacro INSTALLOPTIONS_WRITE "ioSpecial.ini" "Field 5" "Bottom" "120"
        !endif
        !ifdef MUI_FINISHPAGE_REBOOTLATER_DEFAULT
		  !insertmacro INSTALLOPTIONS_WRITE "ioSpecial.ini" "Field 4" "State" "0"
          !insertmacro INSTALLOPTIONS_WRITE "ioSpecial.ini" "Field 5" "State" "1"
        !else
          !insertmacro INSTALLOPTIONS_WRITE "ioSpecial.ini" "Field 4" "State" "1"
		  !insertmacro INSTALLOPTIONS_WRITE "ioSpecial.ini" "Field 5" "State" "0"
        !endif

        Goto mui.finish_load

      mui.finish_noreboot_init:

    !endif

    !insertmacro MUI_INSTALLOPTIONS_WRITE_DEFAULTCONVERT "ioSpecial.ini" "Field 3" "Text" MUI_FINISHPAGE_TEXT

    !ifdef MUI_FINISHPAGE_RUN

      !insertmacro INSTALLOPTIONS_WRITE "ioSpecial.ini" "Field 4" "Type" "CheckBox"
      !insertmacro MUI_INSTALLOPTIONS_WRITE_DEFAULTCONVERT "ioSpecial.ini" "Field 4" "Text" MUI_FINISHPAGE_RUN_TEXT
      !insertmacro INSTALLOPTIONS_WRITE "ioSpecial.ini" "Field 4" "Left" "120"
      !insertmacro INSTALLOPTIONS_WRITE "ioSpecial.ini" "Field 4" "Right" "315"
      !ifndef MUI_FINISHPAGE_TITLE_3LINES
        !ifndef MUI_FINISHPAGE_TEXT_LARGE
          !insertmacro INSTALLOPTIONS_WRITE "ioSpecial.ini" "Field 4" "Top" "90"
          !insertmacro INSTALLOPTIONS_WRITE "ioSpecial.ini" "Field 4" "Bottom" "100"
        !else
          !insertmacro INSTALLOPTIONS_WRITE "ioSpecial.ini" "Field 4" "Top" "120"
          !insertmacro INSTALLOPTIONS_WRITE "ioSpecial.ini" "Field 4" "Bottom" "130"
        !endif
      !else
        !ifndef MUI_FINISHPAGE_TEXT_LARGE
          !insertmacro INSTALLOPTIONS_WRITE "ioSpecial.ini" "Field 4" "Top" "100"
          !insertmacro INSTALLOPTIONS_WRITE "ioSpecial.ini" "Field 4" "Bottom" "110"
        !else
          !insertmacro INSTALLOPTIONS_WRITE "ioSpecial.ini" "Field 4" "Top" "130"
          !insertmacro INSTALLOPTIONS_WRITE "ioSpecial.ini" "Field 4" "Bottom" "140"
        !endif
      !endif
      !ifndef MUI_FINISHPAGE_RUN_NOTCHECKED
        !insertmacro INSTALLOPTIONS_WRITE "ioSpecial.ini" "Field 4" "State" "1"
      !endif

    !endif

    !ifdef MUI_FINISHPAGE_SHOWREADME

      !ifdef MUI_FINISHPAGE_CURFIELD_NO
        !undef MUI_FINISHPAGE_CURFIELD_NO
      !endif

      !ifndef MUI_FINISHPAGE_RUN
        !define MUI_FINISHPAGE_CURFIELD_NO 4
        !ifndef MUI_FINISHPAGE_TITLE_3LINES
          !ifndef MUI_FINISHPAGE_TEXT_LARGE
            !define MUI_FINISHPAGE_CURFIELD_TOP 90
            !define MUI_FINISHPAGE_CURFIELD_BOTTOM 100
          !else
            !define MUI_FINISHPAGE_CURFIELD_TOP 120
            !define MUI_FINISHPAGE_CURFIELD_BOTTOM 130
          !endif
        !else
          !ifndef MUI_FINISHPAGE_TEXT_LARGE
            !define MUI_FINISHPAGE_CURFIELD_TOP 100
            !define MUI_FINISHPAGE_CURFIELD_BOTTOM 110
          !else
            !define MUI_FINISHPAGE_CURFIELD_TOP 130
            !define MUI_FINISHPAGE_CURFIELD_BOTTOM 140
          !endif
        !endif
      !else
        !define MUI_FINISHPAGE_CURFIELD_NO 5
        !ifndef MUI_FINISHPAGE_TITLE_3LINES
          !ifndef MUI_FINISHPAGE_TEXT_LARGE
            !define MUI_FINISHPAGE_CURFIELD_TOP 110
            !define MUI_FINISHPAGE_CURFIELD_BOTTOM 120
          !else
            !define MUI_FINISHPAGE_CURFIELD_TOP 140
            !define MUI_FINISHPAGE_CURFIELD_BOTTOM 150
          !endif
        !else
          !ifndef MUI_FINISHPAGE_TEXT_LARGE
            !define MUI_FINISHPAGE_CURFIELD_TOP 120
            !define MUI_FINISHPAGE_CURFIELD_BOTTOM 130
          !else
            !define MUI_FINISHPAGE_CURFIELD_TOP 150
            !define MUI_FINISHPAGE_CURFIELD_BOTTOM 160
          !endif
        !endif
      !endif

      !insertmacro INSTALLOPTIONS_WRITE "ioSpecial.ini" "Field ${MUI_FINISHPAGE_CURFIELD_NO}" "Type" "CheckBox"
      !insertmacro MUI_INSTALLOPTIONS_WRITE_DEFAULTCONVERT "ioSpecial.ini" "Field ${MUI_FINISHPAGE_CURFIELD_NO}" "Text" MUI_FINISHPAGE_SHOWREADME_TEXT
      !insertmacro INSTALLOPTIONS_WRITE "ioSpecial.ini" "Field ${MUI_FINISHPAGE_CURFIELD_NO}" "Left" "120"
      !insertmacro INSTALLOPTIONS_WRITE "ioSpecial.ini" "Field ${MUI_FINISHPAGE_CURFIELD_NO}" "Right" "315"
      !insertmacro INSTALLOPTIONS_WRITE "ioSpecial.ini" "Field ${MUI_FINISHPAGE_CURFIELD_NO}" "Top" ${MUI_FINISHPAGE_CURFIELD_TOP}
      !insertmacro INSTALLOPTIONS_WRITE "ioSpecial.ini" "Field ${MUI_FINISHPAGE_CURFIELD_NO}" "Bottom" ${MUI_FINISHPAGE_CURFIELD_BOTTOM}
      !ifndef MUI_FINISHPAGE_SHOWREADME_NOTCHECKED
         !insertmacro INSTALLOPTIONS_WRITE "ioSpecial.ini" "Field ${MUI_FINISHPAGE_CURFIELD_NO}" "State" "1"
      !endif

    !endif

    !ifdef MUI_FINISHPAGE_LINK

      !ifdef MUI_FINISHPAGE_CURFIELD_NO
        !undef MUI_FINISHPAGE_CURFIELD_NO
      !endif

      !ifdef MUI_FINISHPAGE_RUN & MUI_FINISHPAGE_SHOWREADME
        !define MUI_FINISHPAGE_CURFIELD_NO 6
      !else ifdef MUI_FINISHPAGE_RUN | MUI_FINISHPAGE_SHOWREADME
        !define MUI_FINISHPAGE_CURFIELD_NO 5
      !else
        !define MUI_FINISHPAGE_CURFIELD_NO 4
      !endif

      !insertmacro INSTALLOPTIONS_WRITE "ioSpecial.ini" "Field ${MUI_FINISHPAGE_CURFIELD_NO}" "Type" "Link"
      !insertmacro MUI_INSTALLOPTIONS_WRITE_DEFAULTCONVERT "ioSpecial.ini" "Field ${MUI_FINISHPAGE_CURFIELD_NO}" "Text" MUI_FINISHPAGE_LINK
      !insertmacro INSTALLOPTIONS_WRITE "ioSpecial.ini" "Field ${MUI_FINISHPAGE_CURFIELD_NO}" "Left" "120"
      !insertmacro INSTALLOPTIONS_WRITE "ioSpecial.ini" "Field ${MUI_FINISHPAGE_CURFIELD_NO}" "Right" "315"
      !insertmacro INSTALLOPTIONS_WRITE "ioSpecial.ini" "Field ${MUI_FINISHPAGE_CURFIELD_NO}" "Top" "175"
      !insertmacro INSTALLOPTIONS_WRITE "ioSpecial.ini" "Field ${MUI_FINISHPAGE_CURFIELD_NO}" "Bottom" "185"
      !insertmacro MUI_INSTALLOPTIONS_WRITE_DEFAULTCONVERT "ioSpecial.ini" "Field ${MUI_FINISHPAGE_CURFIELD_NO}" "State" MUI_FINISHPAGE_LINK_LOCATION

    !endif

    !ifdef MUI_FINISHPAGE_RUN & MUI_FINISHPAGE_SHOWREADME & MUI_FINISHPAGE_LINK
      !insertmacro INSTALLOPTIONS_WRITE "ioSpecial.ini" "Settings" "Numfields" "6"
    !else ifdef MUI_FINISHPAGE_RUN & MUI_FINISHPAGE_SHOWREADME
      !insertmacro INSTALLOPTIONS_WRITE "ioSpecial.ini" "Settings" "Numfields" "5"
    !else ifdef MUI_FINISHPAGE_RUN & MUI_FINISHPAGE_LINK
      !insertmacro INSTALLOPTIONS_WRITE "ioSpecial.ini" "Settings" "Numfields" "5"
    !else ifdef MUI_FINISHPAGE_SHOWREADME & MUI_FINISHPAGE_LINK
      !insertmacro INSTALLOPTIONS_WRITE "ioSpecial.ini" "Settings" "Numfields" "5"
    !else ifdef MUI_FINISHPAGE_RUN | MUI_FINISHPAGE_SHOWREADME | MUI_FINISHPAGE_LINK
      !insertmacro INSTALLOPTIONS_WRITE "ioSpecial.ini" "Settings" "Numfields" "4"
    !endif

    !ifndef MUI_FINISHPAGE_NOREBOOTSUPPORT
       mui.finish_load:
    !endif

    !insertmacro MUI_PAGE_FUNCTION_CUSTOM PRE

    LockWindow on
    GetDlgItem $MUI_TEMP1 $HWNDPARENT 1028
    ShowWindow $MUI_TEMP1 ${SW_HIDE}

    GetDlgItem $MUI_TEMP1 $HWNDPARENT 1256
    ShowWindow $MUI_TEMP1 ${SW_HIDE}

    GetDlgItem $MUI_TEMP1 $HWNDPARENT 1035
    ShowWindow $MUI_TEMP1 ${SW_HIDE}

    GetDlgItem $MUI_TEMP1 $HWNDPARENT 1037
    ShowWindow $MUI_TEMP1 ${SW_HIDE}

    GetDlgItem $MUI_TEMP1 $HWNDPARENT 1038
    ShowWindow $MUI_TEMP1 ${SW_HIDE}

    GetDlgItem $MUI_TEMP1 $HWNDPARENT 1039
    ShowWindow $MUI_TEMP1 ${SW_HIDE}

    GetDlgItem $MUI_TEMP1 $HWNDPARENT 1045
    ShowWindow $MUI_TEMP1 ${SW_NORMAL}
    LockWindow off

    !insertmacro INSTALLOPTIONS_INITDIALOG "ioSpecial.ini"
    Pop $MUI_HWND
    SetCtlColors $MUI_HWND "" "${MUI_BGCOLOR}"

    GetDlgItem $MUI_TEMP1 $MUI_HWND 1201
    SetCtlColors $MUI_TEMP1 "" "${MUI_BGCOLOR}"

    CreateFont $MUI_TEMP2 "$(^Font)" "12" "700"
    SendMessage $MUI_TEMP1 ${WM_SETFONT} $MUI_TEMP2 0

    GetDlgItem $MUI_TEMP1 $MUI_HWND 1202
    SetCtlColors $MUI_TEMP1 "" "${MUI_BGCOLOR}"

    !ifndef MUI_FINISHPAGE_NOREBOOTSUPPORT

      IfRebootFlag 0 mui.finish_noreboot_show

        GetDlgItem $MUI_TEMP1 $MUI_HWND 1203
        SetCtlColors $MUI_TEMP1 "" "${MUI_BGCOLOR}"

        GetDlgItem $MUI_TEMP1 $MUI_HWND 1204
        SetCtlColors $MUI_TEMP1 "" "${MUI_BGCOLOR}"

        Goto mui.finish_show

      mui.finish_noreboot_show:

    !endif

    !ifdef MUI_FINISHPAGE_RUN
      GetDlgItem $MUI_TEMP1 $MUI_HWND 1203
      SetCtlColors $MUI_TEMP1 "" "${MUI_BGCOLOR}"
    !endif

    !ifdef MUI_FINISHPAGE_SHOWREADME
      !ifndef MUI_FINISHPAGE_RUN
        GetDlgItem $MUI_TEMP1 $MUI_HWND 1203
      !else
        GetDlgItem $MUI_TEMP1 $MUI_HWND 1204
      !endif
      SetCtlColors $MUI_TEMP1 "" "${MUI_BGCOLOR}"
    !endif

    !ifdef MUI_FINISHPAGE_LINK
      !ifdef MUI_FINISHPAGE_RUN & MUI_FINISHPAGE_SHOWREADME
        GetDlgItem $MUI_TEMP1 $MUI_HWND 1205
      !else ifdef MUI_FINISHPAGE_RUN | MUI_FINISHPAGE_SHOWREADME
        GetDlgItem $MUI_TEMP1 $MUI_HWND 1204
      !else
        GetDlgItem $MUI_TEMP1 $MUI_HWND 1203
      !endif
      SetCtlColors $MUI_TEMP1 "${MUI_FINISHPAGE_LINK_COLOR}" "${MUI_BGCOLOR}"
    !endif

    !ifndef MUI_FINISHPAGE_NOREBOOTSUPPORT
      mui.finish_show:
    !endif

    !insertmacro MUI_PAGE_FUNCTION_CUSTOM SHOW

    !ifdef MUI_FINISHPAGE_CANCEL_ENABLED
      StrCpy $MUI_NOABORTWARNING "1"
    !endif

    !insertmacro INSTALLOPTIONS_SHOW

    !ifdef MUI_FINISHPAGE_CANCEL_ENABLED
      StrCpy $MUI_NOABORTWARNING ""
    !endif

    LockWindow on
    GetDlgItem $MUI_TEMP1 $HWNDPARENT 1028
    ShowWindow $MUI_TEMP1 ${SW_NORMAL}

    GetDlgItem $MUI_TEMP1 $HWNDPARENT 1256
    ShowWindow $MUI_TEMP1 ${SW_NORMAL}

    GetDlgItem $MUI_TEMP1 $HWNDPARENT 1035
    ShowWindow $MUI_TEMP1 ${SW_NORMAL}

    GetDlgItem $MUI_TEMP1 $HWNDPARENT 1037
    ShowWindow $MUI_TEMP1 ${SW_NORMAL}

    GetDlgItem $MUI_TEMP1 $HWNDPARENT 1038
    ShowWindow $MUI_TEMP1 ${SW_NORMAL}

    GetDlgItem $MUI_TEMP1 $HWNDPARENT 1039
    ShowWindow $MUI_TEMP1 ${SW_NORMAL}

    GetDlgItem $MUI_TEMP1 $HWNDPARENT 1045
    ShowWindow $MUI_TEMP1 ${SW_HIDE}
    LockWindow off

  FunctionEnd

  Function "${LEAVE}"

    !insertmacro MUI_PAGE_FUNCTION_CUSTOM LEAVE

    !ifndef MUI_FINISHPAGE_NOREBOOTSUPPORT

      IfRebootFlag "" mui.finish_noreboot_end

        !insertmacro INSTALLOPTIONS_READ $MUI_TEMP1 "ioSpecial.ini" "Field 4" "State"

          StrCmp $MUI_TEMP1 "1" 0 +2
            Reboot

          Return

      mui.finish_noreboot_end:

    !endif

    !ifdef MUI_FINISHPAGE_RUN

      !insertmacro INSTALLOPTIONS_READ $MUI_TEMP1 "ioSpecial.ini" "Field 4" "State"

      StrCmp $MUI_TEMP1 "1" 0 mui.finish_norun
        !ifndef MUI_FINISHPAGE_RUN_FUNCTION
          !ifndef MUI_FINISHPAGE_RUN_PARAMETERS
            StrCpy $MUI_TEMP1 "$\"${MUI_FINISHPAGE_RUN}$\""
          !else
            StrCpy $MUI_TEMP1 "$\"${MUI_FINISHPAGE_RUN}$\" ${MUI_FINISHPAGE_RUN_PARAMETERS}"
          !endif
          Exec "$MUI_TEMP1"
        !else
          Call "${MUI_FINISHPAGE_RUN_FUNCTION}"
        !endif

        mui.finish_norun:

    !endif

    !ifdef MUI_FINISHPAGE_SHOWREADME

      !ifndef MUI_FINISHPAGE_RUN
        !insertmacro INSTALLOPTIONS_READ $MUI_TEMP1 "ioSpecial.ini" "Field 4" "State"
      !else
        !insertmacro INSTALLOPTIONS_READ $MUI_TEMP1 "ioSpecial.ini" "Field 5" "State"
      !endif

      StrCmp $MUI_TEMP1 "1" 0 mui.finish_noshowreadme
        !ifndef MUI_FINISHPAGE_SHOWREADME_FUNCTION
           ExecShell "open" "${MUI_FINISHPAGE_SHOWREADME}"
        !else
          Call "${MUI_FINISHPAGE_SHOWREADME_FUNCTION}"
        !endif

        mui.finish_noshowreadme:

    !endif

  FunctionEnd

!macroend

!macro MUI_UNFUNCTION_CONFIRMPAGE PRE SHOW LEAVE

  Function "${PRE}"

   !insertmacro MUI_PAGE_FUNCTION_CUSTOM PRE
   !insertmacro MUI_HEADER_TEXT_PAGE $(MUI_UNTEXT_CONFIRM_TITLE) $(MUI_UNTEXT_CONFIRM_SUBTITLE)

  FunctionEnd

  Function "${SHOW}"

    !insertmacro MUI_PAGE_FUNCTION_CUSTOM SHOW

  FunctionEnd

  Function "${LEAVE}"

    !insertmacro MUI_PAGE_FUNCTION_CUSTOM LEAVE

  FunctionEnd

!macroend

;--------------------------------
;INSTALL OPTIONS (CUSTOM PAGES)

!macro MUI_INSTALLOPTIONS_EXTRACT FILE

  !verbose push
  !verbose ${MUI_VERBOSE}

  !insertmacro INSTALLOPTIONS_EXTRACT "${FILE}"

  !verbose pop

!macroend

!macro MUI_INSTALLOPTIONS_EXTRACT_AS FILE FILENAME

  !verbose push
  !verbose ${MUI_VERBOSE}

  !insertmacro INSTALLOPTIONS_EXTRACT_AS "${FILE}" "${FILENAME}"

  !verbose pop

!macroend

!macro MUI_INSTALLOPTIONS_DISPLAY FILE

  !verbose push
  !verbose ${MUI_VERBOSE}

  !insertmacro INSTALLOPTIONS_DISPLAY "${FILE}"

  !verbose pop

!macroend

!macro MUI_INSTALLOPTIONS_DISPLAY_RETURN FILE

  !verbose push
  !verbose ${MUI_VERBOSE}

  !insertmacro INSTALLOPTIONS_DISPLAY_RETURN "${FILE}"
  
  !verbose pop

!macroend

!macro MUI_INSTALLOPTIONS_INITDIALOG FILE

  !verbose push
  !verbose ${MUI_VERBOSE}

  !insertmacro INSTALLOPTIONS_INITDIALOG "${FILE}"

  !verbose pop

!macroend

!macro MUI_INSTALLOPTIONS_SHOW

  !verbose push
  !verbose ${MUI_VERBOSE}

  !insertmacro INSTALLOPTIONS_SHOW

  !verbose pop

!macroend

!macro MUI_INSTALLOPTIONS_SHOW_RETURN

  !verbose push
  !verbose ${MUI_VERBOSE}

  !insertmacro INSTALLOPTIONS_SHOW_RETURN

  !verbose pop

!macroend

!macro MUI_INSTALLOPTIONS_READ VAR FILE SECTION KEY

  !verbose push
  !verbose ${MUI_VERBOSE}

  !insertmacro INSTALLOPTIONS_READ "${VAR}" "${FILE}" "${SECTION}" "${KEY}"

  !verbose pop

!macroend

!macro MUI_INSTALLOPTIONS_WRITE FILE SECTION KEY VALUE

  !verbose push
  !verbose ${MUI_VERBOSE}

  !insertmacro INSTALLOPTIONS_WRITE "${FILE}" "${SECTION}" "${KEY}" "${VALUE}"

  !verbose pop

!macroend

!macro MUI_INSTALLOPTIONS_WRITE_DEFAULTCONVERT FILE SECTION KEY SYMBOL

  ;Converts default strings from language files to InstallOptions format
  ;Only for use inside MUI

  !verbose push
  !verbose ${MUI_VERBOSE}

  !ifndef "${SYMBOL}_DEFAULTSET"
    !insertmacro INSTALLOPTIONS_WRITE "${FILE}" "${SECTION}" "${KEY}" "${${SYMBOL}}"
  !else
    Push "${${SYMBOL}}"
    Call ${MUI_PAGE_UNINSTALLER_FUNCPREFIX}Nsis2Io
    Pop $MUI_TEMP1
    !insertmacro INSTALLOPTIONS_WRITE "${FILE}" "${SECTION}" "${KEY}" $MUI_TEMP1
  !endif

  !verbose pop

!macroend

;--------------------------------
;RESERVE FILES

!macro MUI_RESERVEFILE_INSTALLOPTIONS

  !verbose push
  !verbose ${MUI_VERBOSE}

  ReserveFile "${NSISDIR}\Plugins\InstallOptions.dll"

  !verbose pop

!macroend

!macro MUI_RESERVEFILE_LANGDLL

  !verbose push
  !verbose ${MUI_VERBOSE}

  ReserveFile "${NSISDIR}\Plugins\LangDLL.dll"

  !verbose pop

!macroend

;--------------------------------
;LANGUAGES

!macro MUI_LANGUAGE LANGUAGE

  ;Include a language

  !verbose push
  !verbose ${MUI_VERBOSE}

  !insertmacro MUI_INSERT

  LoadLanguageFile "${NSISDIR}\Contrib\Language files\${LANGUAGE}.nlf"

  ;Include language file
  !insertmacro LANGFILE_INCLUDE_WITHDEFAULT "${NSISDIR}\Contrib\Language files\${LANGUAGE}.nsh" "${NSISDIR}\Contrib\Language files\English.nsh"

  ;Add language to list of languages for selection dialog  
  !ifndef MUI_LANGDLL_LANGUAGES
    !define MUI_LANGDLL_LANGUAGES "'${LANGFILE_${LANGUAGE}_NAME}' '${LANG_${LANGUAGE}}' "
    !define MUI_LANGDLL_LANGUAGES_CP "'${LANGFILE_${LANGUAGE}_NAME}' '${LANG_${LANGUAGE}}' '${LANG_${LANGUAGE}_CP}' "
  !else
    !ifdef MUI_LANGDLL_LANGUAGES_TEMP
      !undef MUI_LANGDLL_LANGUAGES_TEMP
    !endif
    !define MUI_LANGDLL_LANGUAGES_TEMP "${MUI_LANGDLL_LANGUAGES}"
    !undef MUI_LANGDLL_LANGUAGES

	!ifdef MUI_LANGDLL_LANGUAGES_CP_TEMP
      !undef MUI_LANGDLL_LANGUAGES_CP_TEMP
    !endif
    !define MUI_LANGDLL_LANGUAGES_CP_TEMP "${MUI_LANGDLL_LANGUAGES_CP}"
    !undef MUI_LANGDLL_LANGUAGES_CP

    !define MUI_LANGDLL_LANGUAGES "'${LANGFILE_${LANGUAGE}_NAME}' '${LANG_${LANGUAGE}}' ${MUI_LANGDLL_LANGUAGES_TEMP}"
    !define MUI_LANGDLL_LANGUAGES_CP "'${LANGFILE_${LANGUAGE}_NAME}' '${LANG_${LANGUAGE}}' '${LANG_${LANGUAGE}_CP}' ${MUI_LANGDLL_LANGUAGES_CP_TEMP}"
  !endif
  
  !verbose pop

!macroend

;--------------------------------
;LANGUAGE SELECTION DIALOG

!macro MUI_LANGDLL_DISPLAY

  !verbose push
  !verbose ${MUI_VERBOSE}

  !insertmacro MUI_DEFAULT MUI_LANGDLL_WINDOWTITLE "Installer Language"
  !insertmacro MUI_DEFAULT MUI_LANGDLL_INFO "Please select a language."

  !ifdef MUI_LANGDLL_REGISTRY_ROOT & MUI_LANGDLL_REGISTRY_KEY & MUI_LANGDLL_REGISTRY_VALUENAME

    ReadRegStr $MUI_TEMP1 "${MUI_LANGDLL_REGISTRY_ROOT}" "${MUI_LANGDLL_REGISTRY_KEY}" "${MUI_LANGDLL_REGISTRY_VALUENAME}"
    StrCmp $MUI_TEMP1 "" mui.langdll_show
      StrCpy $LANGUAGE $MUI_TEMP1
      !ifndef MUI_LANGDLL_ALWAYSSHOW
        Goto mui.langdll_done
      !endif
    mui.langdll_show:

  !endif
  
  !ifdef NSIS_CONFIG_SILENT_SUPPORT
    IfSilent mui.langdll_done
  !endif  

  !ifdef MUI_LANGDLL_ALLLANGUAGES
    LangDLL::LangDialog "${MUI_LANGDLL_WINDOWTITLE}" "${MUI_LANGDLL_INFO}" A ${MUI_LANGDLL_LANGUAGES} ""
  !else
    LangDLL::LangDialog "${MUI_LANGDLL_WINDOWTITLE}" "${MUI_LANGDLL_INFO}" AC ${MUI_LANGDLL_LANGUAGES_CP} ""
  !endif

  Pop $LANGUAGE
  StrCmp $LANGUAGE "cancel" 0 +2
    Abort

  !ifdef NSIS_CONFIG_SILENT_SUPPORT
    mui.langdll_done:
  !else ifdef MUI_LANGDLL_REGISTRY_ROOT & MUI_LANGDLL_REGISTRY_KEY & MUI_LANGDLL_REGISTRY_VALUENAME
    mui.langdll_done:
  !endif

  !verbose pop

!macroend

!macro MUI_LANGDLL_SAVELANGUAGE

  !ifndef MUI_PAGE_UNINSTALLER

    IfAbort mui.langdllsavelanguage_abort

    !ifdef MUI_LANGDLL_REGISTRY_ROOT & MUI_LANGDLL_REGISTRY_KEY & MUI_LANGDLL_REGISTRY_VALUENAME
      WriteRegStr "${MUI_LANGDLL_REGISTRY_ROOT}" "${MUI_LANGDLL_REGISTRY_KEY}" "${MUI_LANGDLL_REGISTRY_VALUENAME}" $LANGUAGE
    !endif

    mui.langdllsavelanguage_abort:

  !endif

!macroend

!macro MUI_UNGETLANGUAGE

  !verbose pop

  !ifdef MUI_LANGDLL_REGISTRY_ROOT & MUI_LANGDLL_REGISTRY_KEY & MUI_LANGDLL_REGISTRY_VALUENAME

    ReadRegStr $MUI_TEMP1 "${MUI_LANGDLL_REGISTRY_ROOT}" "${MUI_LANGDLL_REGISTRY_KEY}" "${MUI_LANGDLL_REGISTRY_VALUENAME}"
    StrCmp $MUI_TEMP1 "" 0 mui.ungetlanguage_setlang

  !endif

  !insertmacro MUI_LANGDLL_DISPLAY

  !ifdef MUI_LANGDLL_REGISTRY_ROOT & MUI_LANGDLL_REGISTRY_KEY & MUI_LANGDLL_REGISTRY_VALUENAME

    Goto mui.ungetlanguage_done

    mui.ungetlanguage_setlang:
      StrCpy $LANGUAGE $MUI_TEMP1

    mui.ungetlanguage_done:

  !endif

  !verbose pop

!macroend

;--------------------------------
;END

!endif

!verbose pop
