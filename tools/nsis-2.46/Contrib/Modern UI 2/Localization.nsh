/*

NSIS Modern User Interface
Localization

*/

;--------------------------------
;Variables

!macro MUI_LANGDLL_VARIABLES

  !ifdef MUI_LANGDLL_REGISTRY_ROOT & MUI_LANGDLL_REGISTRY_KEY & MUI_LANGDLL_REGISTRY_VALUENAME
    !ifndef MUI_LANGDLL_REGISTRY_VARAIBLES
      !define MUI_LANGDLL_REGISTRY_VARAIBLES

      ;/GLOBAL because the macros are included in a function
      Var /GLOBAL mui.LangDLL.RegistryLanguage

    !endif
  !endif

!macroend


;--------------------------------
;Include langauge files

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
;Language selection

!macro MUI_LANGDLL_DISPLAY

  !verbose push
  !verbose ${MUI_VERBOSE}

  !insertmacro MUI_LANGDLL_VARIABLES

  !insertmacro MUI_DEFAULT MUI_LANGDLL_WINDOWTITLE "Installer Language"
  !insertmacro MUI_DEFAULT MUI_LANGDLL_INFO "Please select a language."

  !ifdef MUI_LANGDLL_REGISTRY_VARAIBLES

    ReadRegStr $mui.LangDLL.RegistryLanguage "${MUI_LANGDLL_REGISTRY_ROOT}" "${MUI_LANGDLL_REGISTRY_KEY}" "${MUI_LANGDLL_REGISTRY_VALUENAME}"
    
    ${if} $mui.LangDLL.RegistryLanguage != ""
      ;Set default langauge to registry language
      StrCpy $LANGUAGE $mui.LangDLL.RegistryLanguage
    ${endif}

  !endif

  !ifdef NSIS_CONFIG_SILENT_SUPPORT
    ${unless} ${Silent}
  !endif

  !ifndef MUI_LANGDLL_ALWAYSSHOW
  !ifdef MUI_LANGDLL_REGISTRY_VARAIBLES
    ${if} $mui.LangDLL.RegistryLanguage == ""
  !endif
  !endif
  
  ;Show langauge selection dialog
  !ifdef MUI_LANGDLL_ALLLANGUAGES
    LangDLL::LangDialog "${MUI_LANGDLL_WINDOWTITLE}" "${MUI_LANGDLL_INFO}" A ${MUI_LANGDLL_LANGUAGES} ""
  !else
    LangDLL::LangDialog "${MUI_LANGDLL_WINDOWTITLE}" "${MUI_LANGDLL_INFO}" AC ${MUI_LANGDLL_LANGUAGES_CP} ""
  !endif
  
    Pop $LANGUAGE
    ${if} $LANGUAGE == "cancel"
      Abort
    ${endif}
  
  !ifndef MUI_LANGDLL_ALWAYSSHOW
  !ifdef MUI_LANGDLL_REGISTRY_VARAIBLES
    ${endif}
  !endif
  !endif


  !ifdef NSIS_CONFIG_SILENT_SUPPORT
    ${endif}
  !endif

  !verbose pop

!macroend

!macro MUI_LANGDLL_SAVELANGUAGE

  ;Save language in registry

  !ifndef MUI_PAGE_UNINSTALLER

    IfAbort mui.langdllsavelanguage_abort

    !ifdef MUI_LANGDLL_REGISTRY_ROOT & MUI_LANGDLL_REGISTRY_KEY & MUI_LANGDLL_REGISTRY_VALUENAME
      WriteRegStr "${MUI_LANGDLL_REGISTRY_ROOT}" "${MUI_LANGDLL_REGISTRY_KEY}" "${MUI_LANGDLL_REGISTRY_VALUENAME}" $LANGUAGE
    !endif

    mui.langdllsavelanguage_abort:

  !endif

!macroend

!macro MUI_UNGETLANGUAGE

  ;Get language from registry in uninstaller

  !verbose push
  !verbose ${MUI_VERBOSE}

  !insertmacro MUI_LANGDLL_VARIABLES

  !ifdef MUI_LANGDLL_REGISTRY_ROOT & MUI_LANGDLL_REGISTRY_KEY & MUI_LANGDLL_REGISTRY_VALUENAME

    ReadRegStr $mui.LangDLL.RegistryLanguage "${MUI_LANGDLL_REGISTRY_ROOT}" "${MUI_LANGDLL_REGISTRY_KEY}" "${MUI_LANGDLL_REGISTRY_VALUENAME}"
    
    ${if} $mui.LangDLL.RegistryLanguage = ""

  !endif

  !insertmacro MUI_LANGDLL_DISPLAY

  !ifdef MUI_LANGDLL_REGISTRY_ROOT & MUI_LANGDLL_REGISTRY_KEY & MUI_LANGDLL_REGISTRY_VALUENAME

    ${else}
      StrCpy $LANGUAGE $mui.LangDLL.RegistryLanguage
    ${endif}

  !endif

  !verbose pop

!macroend


;--------------------------------
;Rerserve LangDLL file

!macro MUI_RESERVEFILE_LANGDLL

  !verbose push
  !verbose ${MUI_VERBOSE}

  ReserveFile "${NSISDIR}\Plugins\LangDLL.dll"

  !verbose pop

!macroend
