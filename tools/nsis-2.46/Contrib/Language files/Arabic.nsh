;Language: Arabic (1025)
;Translation by asdfuae@msn.com
;updated by Rami Kattan

!insertmacro LANGFILE "Arabic" "Arabic"

!ifdef MUI_WELCOMEPAGE
  ${LangFileString} MUI_TEXT_WELCOME_INFO_TITLE "„—Õ»« »ﬂ ›Ì „—‘œ ≈⁄œ«œ $(^NameDA)"
  ${LangFileString} MUI_TEXT_WELCOME_INFO_TEXT "”Ì”«⁄œﬂ Â–« «·„—‘œ ›Ì  ‰’Ì» $(^NameDA).$\r$\n$\r$\n„‰ «·„›÷· ≈€·«ﬁ Ã„Ì⁄ «·»—«„Ã ﬁ»· «· ‰’Ì». ”Ì”«⁄œ Â–« ›Ì  ÃœÌœ „·›«  «·‰Ÿ«„ œÊ‰ «·Õ«Ã… ·≈⁄«œ…  ‘€Ì· «·ÃÂ«“.$\r$\n$\r$\n$_CLICK"
!endif

!ifdef MUI_UNWELCOMEPAGE
  ${LangFileString} MUI_UNTEXT_WELCOME_INFO_TITLE "„—Õ»« »ﬂ ›Ì „—‘œ ≈“«·… $(^NameDA) "
  ${LangFileString} MUI_UNTEXT_WELCOME_INFO_TEXT "Â–« «·„—‘œ ”Ìœ·¯ﬂ √À‰«¡ ≈“«·… $(^NameDA).$\r$\n$\r$\n ﬁ»· «·»œ¡ »«·≈“«·…° Ì—ÃÏ «· √ﬂœ „‰ √‰ $(^NameDA) €Ì— ‘€¯«·.$\r$\n$\r$\n$_CLICK"
!endif

!ifdef MUI_LICENSEPAGE
  ${LangFileString} MUI_TEXT_LICENSE_TITLE "≈ ›«ﬁÌ…˛ «· —ŒÌ’"
  ${LangFileString} MUI_TEXT_LICENSE_SUBTITLE "«·—Ã«¡ „—«Ã⁄… ≈ ›«ﬁÌ…˛ «· —ŒÌ’ ﬁ»·  ‰’Ì» $(^NameDA)."
  ${LangFileString} MUI_INNERTEXT_LICENSE_BOTTOM "⁄‰œ «·„Ê«›ﬁ… ⁄·Ï ‘—Êÿ «·≈ ›«ﬁÌ…˛° ≈÷€ÿ √Ê«›ﬁ ··„ «»⁄…. ÌÃ» «·„Ê«›ﬁ… ⁄·Ï «·≈ ›«ﬁÌ…˛ · ‰’Ì» $(^NameDA)."
  ${LangFileString} MUI_INNERTEXT_LICENSE_BOTTOM_CHECKBOX "⁄‰œ «·„Ê«›ﬁ… ⁄·Ï ‘—Êÿ «·≈ ›«ﬁÌ…˛° ⁄·¯„ „—»⁄ «·⁄·«„… «· «·Ì. ÌÃ» «·„Ê«›ﬁ… ⁄·Ï «·≈ ›«ﬁÌ…˛ · ‰’Ì» $(^NameDA). $_CLICK"
  ${LangFileString} MUI_INNERTEXT_LICENSE_BOTTOM_RADIOBUTTONS "⁄‰œ «·„Ê«›ﬁ… ⁄·Ï ‘—Êÿ «·≈ ›«ﬁÌ…° ≈Œ — «·ŒÌ«— «·√Ê· „‰ «· «·Ì. ÌÃ» «·„Ê«›ﬁ… ⁄·Ï «·≈ ›«ﬁÌ… · ‰’Ì» $(^NameDA). $_CLICK"
!endif

!ifdef MUI_UNLICENSEPAGE
  ${LangFileString} MUI_UNTEXT_LICENSE_TITLE "≈ ›«ﬁÌ… «· —ŒÌ’"
  ${LangFileString} MUI_UNTEXT_LICENSE_SUBTITLE "«·—Ã«¡ „—«Ã⁄… ‘—Êÿ «· —ŒÌ’ ﬁ»· ≈“«·… $(^NameDA)."
  ${LangFileString} MUI_UNINNERTEXT_LICENSE_BOTTOM "⁄‰œ «·„Ê«›ﬁ… ⁄·Ï ‘—Êÿ «·≈ ›«ﬁÌ…° ≈÷€ÿ ⁄·Ï „Ê«›ﬁ. ÌÃ» «·„Ê«›ﬁ… ⁄·Ï «·≈ ›«ﬁÌ… ·≈“«·… $(^NameDA)."
  ${LangFileString} MUI_UNINNERTEXT_LICENSE_BOTTOM_CHECKBOX "⁄‰œ «·„Ê«›ﬁ… ⁄·Ï ‘—Êÿ «·≈ ›«ﬁÌ…° ⁄·¯„ «·„—»⁄ «·⁄·«„… «· «·Ì. ÌÃ» «·„Ê«›ﬁ… ⁄·Ï «·≈ ›«ﬁÌ… ·≈“«·… $(^NameDA). $_CLICK"
  ${LangFileString} MUI_UNINNERTEXT_LICENSE_BOTTOM_RADIOBUTTONS "⁄‰œ «·„Ê«›ﬁ… ⁄·Ï ‘—Êÿ «·≈ ›«ﬁÌ…° ≈Œ — «·ŒÌ«— «·√Ê· „‰ «· «·Ì. ÌÃ» «·„Ê«›ﬁ… ⁄·Ï «·≈ ›«ﬁÌ… ·≈“«·… $(^NameDA). $_CLICK"
!endif

!ifdef MUI_LICENSEPAGE | MUI_UNLICENSEPAGE
  ${LangFileString} MUI_INNERTEXT_LICENSE_TOP "≈÷⁄ÿ „› «Õ ’›Õ… ··√”›· ·—ƒÌ… »«ﬁÌ «·≈ ›«ﬁÌ…"
!endif

!ifdef MUI_COMPONENTSPAGE
  ${LangFileString} MUI_TEXT_COMPONENTS_TITLE "≈Œ — «·„ﬂÊ‰« "
  ${LangFileString} MUI_TEXT_COMPONENTS_SUBTITLE "≈Œ — „Ì“«  $(^NameDA) «·„—«œ  ‰’Ì»Â«."
!endif

!ifdef MUI_UNCOMPONENTSPAGE
  ${LangFileString} MUI_UNTEXT_COMPONENTS_TITLE "≈Œ — «·„ﬂÊ‰« "
  ${LangFileString} MUI_UNTEXT_COMPONENTS_SUBTITLE "≈Œ — „Ì“«  $(^NameDA) «·„—«œ ≈“«· Â«."
!endif

!ifdef MUI_COMPONENTSPAGE | MUI_UNCOMPONENTSPAGE
  ${LangFileString} MUI_INNERTEXT_COMPONENTS_DESCRIPTION_TITLE "«·Ê’›"
  !ifndef NSIS_CONFIG_COMPONENTPAGE_ALTERNATIVE
    ${LangFileString} MUI_INNERTEXT_COMPONENTS_DESCRIPTION_INFO "√‘— »«·›√—… ›Êﬁ √Õœ «·„ﬂÊ‰«  ·—ƒÌ… «·Ê’›"
  !else
    ${LangFileString} MUI_INNERTEXT_COMPONENTS_DESCRIPTION_INFO "√‘— »«·›√—… ›Êﬁ √Õœ «·„ﬂÊ‰«  ·—ƒÌ… «·Ê’›"
  !endif
!endif

!ifdef MUI_DIRECTORYPAGE
  ${LangFileString} MUI_TEXT_DIRECTORY_TITLE "≈Œ — „Êﬁ⁄ «· ‰’Ì»"
  ${LangFileString} MUI_TEXT_DIRECTORY_SUBTITLE "≈Œ — «·„Ã·œ «·„—«œ  ‰’Ì» $(^NameDA) ›ÌÂ."
!endif

!ifdef MUI_UNDIRECTORYPAGE
  ${LangFileString} MUI_UNTEXT_DIRECTORY_TITLE "≈Œ — „Êﬁ⁄ «·„“Ì·"
  ${LangFileString} MUI_UNTEXT_DIRECTORY_SUBTITLE "≈Œ — «·„Ã·œ «·–Ì ”Ì“«· „‰Â $(^NameDA)."
!endif

!ifdef MUI_INSTFILESPAGE
  ${LangFileString} MUI_TEXT_INSTALLING_TITLE " ‰’Ì»"
  ${LangFileString} MUI_TEXT_INSTALLING_SUBTITLE "«·—Ã«¡ «·≈‰ Ÿ«— √À‰«¡  ‰’Ì» $(^NameDA)."
  ${LangFileString} MUI_TEXT_FINISH_TITLE "≈‰ ÂÏ «· ‰’Ì»"
  ${LangFileString} MUI_TEXT_FINISH_SUBTITLE "≈‰ Â  ⁄„·Ì… «· ‰’Ì» »‰Ã«Õ."
  ${LangFileString} MUI_TEXT_ABORT_TITLE "≈·€«¡ «· ‰’Ì»"
  ${LangFileString} MUI_TEXT_ABORT_SUBTITLE "·„ Ì‰ ÂÌ «· ‰’Ì» »‰Ã«Õ."
!endif

!ifdef MUI_UNINSTFILESPAGE
  ${LangFileString} MUI_UNTEXT_UNINSTALLING_TITLE "≈“«·…"
  ${LangFileString} MUI_UNTEXT_UNINSTALLING_SUBTITLE "«·—Ã«¡ «·≈‰ Ÿ«— √À‰«¡ ≈“«·… $(^NameDA)."
  ${LangFileString} MUI_UNTEXT_FINISH_TITLE "≈‰ ÂÏ"
  ${LangFileString} MUI_UNTEXT_FINISH_SUBTITLE "≈‰ Â  ⁄„·Ì… «·≈“«·… »‰Ã«Õ."
  ${LangFileString} MUI_UNTEXT_ABORT_TITLE "≈·€«¡ «·≈“«·…"
  ${LangFileString} MUI_UNTEXT_ABORT_SUBTITLE "·„  ‰ ÂÌ «·≈“«·… »‰Ã«Õ."
!endif

!ifdef MUI_FINISHPAGE
  ${LangFileString} MUI_TEXT_FINISH_INFO_TITLE "≈‰Â«¡ „—‘œ ≈⁄œ«œ $(^NameDA)"
  ${LangFileString} MUI_TEXT_FINISH_INFO_TEXT "·ﬁœ  „  ‰’Ì» $(^NameDA) ⁄·Ï «·ÃÂ«“$\r$\n$\r$\n≈÷€ÿ ≈‰Â«¡ ·≈€·«ﬁ „—‘œ «·≈⁄œ«œ."
  ${LangFileString} MUI_TEXT_FINISH_INFO_REBOOT "ÌÃ» ≈⁄«œ…  ‘€Ì· «·ÃÂ«“ ·≈‰Â«¡  ‰’Ì» $(^NameDA). Â·  —Ìœ ≈⁄«œ… «· ‘€Ì· «·¬‰ø"
!endif

!ifdef MUI_UNFINISHPAGE
  ${LangFileString} MUI_UNTEXT_FINISH_INFO_TITLE "≈‰Â«¡ „—‘œ ≈“«·… $(^NameDA)"
  ${LangFileString} MUI_UNTEXT_FINISH_INFO_TEXT "·ﬁœ  „ ≈“«·… $(^NameDA) „‰ «·ÃÂ«“.$\r$\n$\r$\n ≈÷€ÿ ≈‰Â«¡ ·≈€·«ﬁ «·„—‘œ."
  ${LangFileString} MUI_UNTEXT_FINISH_INFO_REBOOT "ÌÃ» ≈⁄«œ…  ‘€Ì· «·ÃÂ«“ ·≈‰Â«¡ ≈“«·… $(^NameDA). Â·  —Ìœ ≈⁄«œ… «· ‘€Ì· «·¬‰ø"
!endif

!ifdef MUI_FINISHPAGE | MUI_UNFINISHPAGE
  ${LangFileString} MUI_TEXT_FINISH_REBOOTNOW "√⁄œ «· ‘€Ì· «·¬‰"
  ${LangFileString} MUI_TEXT_FINISH_REBOOTLATER "√—€» ›Ì ≈⁄«œ…  ‘€Ì· «·ÃÂ«“ ›Ì Êﬁ  ·«Õﬁ"
  ${LangFileString} MUI_TEXT_FINISH_RUN "&‘€· $(^NameDA)"
  ${LangFileString} MUI_TEXT_FINISH_SHOWREADME "«⁄—÷& √ﬁ—√‰Ì"
  ${LangFileString} MUI_BUTTONTEXT_FINISH "&≈‰Â«¡"  
!endif

!ifdef MUI_STARTMENUPAGE
  ${LangFileString} MUI_TEXT_STARTMENU_TITLE "≈Œ — „Ã·œ ﬁ«∆„… «»œ√"
  ${LangFileString} MUI_TEXT_STARTMENU_SUBTITLE "≈Œ — „Ã·œ ﬁ«∆„… «»œ√ ·≈Œ ’«—«  $(^NameDA)."
  ${LangFileString} MUI_INNERTEXT_STARTMENU_TOP "≈Œ — «·„Ã·œ ›Ì ﬁ«∆„… «»œ√ «·–Ì ” ‰‘√ ›ÌÂ ≈Œ ’«—«  «·»—‰«„Ã. Ì„ﬂ‰ √Ì÷« ﬂ «»… ≈”„ ·≈‰‘«¡ „Ã·œ ÃœÌœ."
  ${LangFileString} MUI_INNERTEXT_STARTMENU_CHECKBOX "·«  ‰‘∆ ≈Œ ’«—« "
!endif

!ifdef MUI_UNCONFIRMPAGE
  ${LangFileString} MUI_UNTEXT_CONFIRM_TITLE "≈“«·… $(^NameDA)"
  ${LangFileString} MUI_UNTEXT_CONFIRM_SUBTITLE "≈“«·… $(^NameDA) „‰ «·ÃÂ«“."
!endif

!ifdef MUI_ABORTWARNING
  ${LangFileString} MUI_TEXT_ABORTWARNING "Â· √‰  „ √ﬂœ „‰ ≈€·«ﬁ „‰’¯» $(^Name)ø"
!endif

!ifdef MUI_UNABORTWARNING
  ${LangFileString} MUI_UNTEXT_ABORTWARNING "Â· √‰  „ √ﬂœ „‰ √‰ﬂ «·Œ—ÊÃ „‰ „“Ì· $(^Name)ø"
!endif
