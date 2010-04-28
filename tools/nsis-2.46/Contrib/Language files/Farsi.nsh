;Language: Farsi (1065)
;By FzerorubigD - FzerorubigD@gmail.com - Thanx to all people help me in forum.persiantools.com

!insertmacro LANGFILE "Farsi" "Farsi"

!ifdef MUI_WELCOMEPAGE
  ${LangFileString} MUI_TEXT_WELCOME_INFO_TITLE "»е »—д«ге д’»  $(^NameDA) ќж‘ ¬гѕнѕ."
  ${LangFileString} MUI_TEXT_WELCOME_INFO_TEXT "«нд »—д«ге ‘г« —« ѕ— д’»  $(^NameDA) н«—н гнядѕ.$\r$\n$\r$\n ж’не гняднг ябне »—д«ге е«н ѕ— Ќ«б «ћ—« —« »»дѕнѕ. «нд »е »—д«ге д’» «ћ«“е гнѕеѕ яе Ё«нбе«н б«“г —« »ѕжд дн«“ »е —«е «дѕ«“н ѕж»«—е я«гБнж — ‘г« »е —ж“ ядѕ.$\r$\n$\r$\n$_CLICK"
!endif

!ifdef MUI_UNWELCOMEPAGE
  ${LangFileString} MUI_UNTEXT_WELCOME_INFO_TITLE "»е »—д«ге Ќ–Ё $(^NameDA) ќж‘ ¬гѕнѕ."
  ${LangFileString} MUI_UNTEXT_WELCOME_INFO_TEXT " «нд »—д«ге »—«н Ќ–Ё $(^NameDA) »е ‘г« ягя гнядѕ.$\r$\n$\r$\nё»б «“ Ќ–Ё  $(^NameDA) гЎг∆д ‘жнѕ «нд »—д«ге ѕ— Ќ«б «ћ—« д»«‘ѕ.$\r$\n$\r$\n$_CLICK"
!endif

!ifdef MUI_LICENSEPAGE
  ${LangFileString} MUI_TEXT_LICENSE_TITLE " ж«Ёёд«ге д’»"
  ${LangFileString} MUI_TEXT_LICENSE_SUBTITLE "бЎЁ« Бн‘ «“ д’» $(^NameDA) гЁ«ѕ  ж«Ёёд«ге —« г—ж— яднѕ."
  ${LangFileString} MUI_INNERTEXT_LICENSE_BOTTOM "«Р— ябне »дѕе«н  ж«Ёёд«ге —« ё»жб ѕ«—нѕ ѕяге гж«Ёёг —« »Ё‘«—нѕ. »—«н д’»  $(^NameDA) ‘г« »«н”  «нд  ж«Ёёд«ге —« ё»жб яднѕ."
  ${LangFileString} MUI_INNERTEXT_LICENSE_BOTTOM_CHECKBOX "«Р— ябне »дѕе«н  ж«Ёёд«ге —« ё»жб ѕ«—нѕ  ня “н— —« «д ќ«» яднѕ. »—«н д’»  $(^NameDA) ‘г« »«н”  «нд  ж«Ёёд«ге —« ё»жб яднѕ. $_CLICK"
  ${LangFileString} MUI_INNERTEXT_LICENSE_BOTTOM_RADIOBUTTONS "«Р— ябне »дѕе«н  ж«Ёёд«ге —« ё»жб ѕ«—нѕ Р“нде «жб —« «д ќ«» яднѕ. »—«н д’»  $(^NameDA) ‘г« »«н”  «нд  ж«Ёёд«ге —« ё»жб яднѕ. $_CLICK"
!endif

!ifdef MUI_UNLICENSEPAGE
  ${LangFileString} MUI_UNTEXT_LICENSE_TITLE " ж«Ёёд«ге Ќ–Ё"
  ${LangFileString} MUI_UNTEXT_LICENSE_SUBTITLE "бЎЁ« ябне »дѕе«н «нд  ж«Ёёд«ге —« ё»б «— Ќ–Ё $(^NameDA) г—ж— яднѕ."
  ${LangFileString} MUI_UNINNERTEXT_LICENSE_BOTTOM "«Р— ябне »дѕе«н  ж«Ёёд«ге —« ё»жб ѕ«—нѕ ѕяге гж«Ёёг —« »Ё‘«—нѕ. »—«н Ќ–Ё $(^NameDA) ‘г« »«н”  «нд  ж«Ёёд«ге —« ё»жб яднѕ."
  ${LangFileString} MUI_UNINNERTEXT_LICENSE_BOTTOM_CHECKBOX "«Р— ябне »дѕе«н  ж«Ёёд«ге —« ё»жб ѕ«—нѕ  ня “н— —« «д ќ«» яднѕ. »—«н Ќ–Ё $(^NameDA) ‘г« »«н”  «нд  ж«Ёёд«ге — ё»жб яднѕ. $_CLICK"
  ${LangFileString} MUI_UNINNERTEXT_LICENSE_BOTTOM_RADIOBUTTONS "«Р— ябне »дѕе«н  ж«Ёёд«ге —« ё»жб ѕ«—нѕ Р“нде «жб —« «д ќ«» яднѕ. »—«н Ќ–Ё $(^NameDA) ‘г« »«н”  «нд  ж«Ёёд«ге — ё»жб яднѕ. $_CLICK"
!endif

!ifdef MUI_LICENSEPAGE | MUI_UNLICENSEPAGE
  ${LangFileString} MUI_INNERTEXT_LICENSE_TOP "»—«н ѕнѕд г д »е ’ж—  я«гб «“ ябнѕ Page Down «” Ё«ѕе яднѕ."
!endif

!ifdef MUI_COMPONENTSPAGE
  ${LangFileString} MUI_TEXT_COMPONENTS_TITLE "«д ќ«» «ћ“«н »—д«ге "
  ${LangFileString} MUI_TEXT_COMPONENTS_SUBTITLE "«ћ“«нн «“  $(^NameDA) яе гнќж«енѕ д’» ‘ждѕ —« «д ќ«» яднѕ."
!endif

!ifdef MUI_UNCOMPONENTSPAGE
  ${LangFileString} MUI_UNTEXT_COMPONENTS_TITLE "«д ќ«» «ћ“«н »—д«ге"
  ${LangFileString} MUI_UNTEXT_COMPONENTS_SUBTITLE "«ћ“«нн «“ $(^NameDA) —« яе гнќж«енѕ Ќ–Ё яднѕ «д ќ«» яднѕ."
!endif

!ifdef MUI_COMPONENTSPAGE | MUI_UNCOMPONENTSPAGE
  ${LangFileString} MUI_INNERTEXT_COMPONENTS_DESCRIPTION_TITLE " ж÷нЌ« "
  !ifndef NSIS_CONFIG_COMPONENTPAGE_ALTERNATIVE
    ${LangFileString} MUI_INNERTEXT_COMPONENTS_DESCRIPTION_INFO "д‘«дР— г«ж” —« »— —жн  «ћ“«нн яе гнќж«енѕ »»—нѕ  «  ж÷нЌ«  ¬д —« »»нднѕ."
  !else
    ${LangFileString} MUI_INNERTEXT_COMPONENTS_DESCRIPTION_INFO "д‘«дР— г«ж” —« »— —жн  «ћ“«нн яе гнќж«енѕ »»—нѕ  «  ж÷нЌ«  ¬д —« »»нднѕ."
  !endif
!endif

!ifdef MUI_DIRECTORYPAGE
  ${LangFileString} MUI_TEXT_DIRECTORY_TITLE "«д ќ«» Бж‘е д’»"
  ${LangFileString} MUI_TEXT_DIRECTORY_SUBTITLE "Бж‘е «н яе гнќж«енѕ  $(^NameDA) ѕ— ¬д д’» ‘жѕ —« «д ќ«» яднѕ."
!endif

!ifdef MUI_UNDIRECTORYPAGE
  ${LangFileString} MUI_UNTEXT_DIRECTORY_TITLE "Бж‘е Ќ–Ё —« «д ќ«» яднѕ"
  ${LangFileString} MUI_UNTEXT_DIRECTORY_SUBTITLE "Бж‘е «н яе гнќж«енѕ $(^NameDA) —« «“ ¬д Ќ–Ё яднѕ «д ќ«» яднѕ."
!endif

!ifdef MUI_INSTFILESPAGE
  ${LangFileString} MUI_TEXT_INSTALLING_TITLE "д’» »—д«ге"
  ${LangFileString} MUI_TEXT_INSTALLING_SUBTITLE "бЎЁ« гѕ  “г«дн яе  $(^NameDA) ѕ— Ќ«б д’» «”  —« ’»— яднѕ."
  ${LangFileString} MUI_TEXT_FINISH_TITLE "д’» Б«н«д н«Ё "
  ${LangFileString} MUI_TEXT_FINISH_SUBTITLE "»—д«ге д’» »« гжЁён  Б«н«д н«Ё ."
  ${LangFileString} MUI_TEXT_ABORT_TITLE "»—д«ге д’» бџж ‘ѕ."
  ${LangFileString} MUI_TEXT_ABORT_SUBTITLE "»—д«ге д’» »е ’ж—  днге  г«г Б«н«д н«Ё ."
!endif

!ifdef MUI_UNINSTFILESPAGE
  ${LangFileString} MUI_UNTEXT_UNINSTALLING_TITLE "Ќ–Ё »—д«ге"
  ${LangFileString} MUI_UNTEXT_UNINSTALLING_SUBTITLE "бЎЁ« гѕ  “г«дн яе  $(^NameDA) ѕ— Ќ«б Ќ–Ё «”  —« ’»— яднѕ."
  ${LangFileString} MUI_UNTEXT_FINISH_TITLE "Ќ–Ё Б«н«д н«Ё "
  ${LangFileString} MUI_UNTEXT_FINISH_SUBTITLE "»—д«ге Ќ–Ё »« гжЁён  Б«н«д н«Ё ."
  ${LangFileString} MUI_UNTEXT_ABORT_TITLE "»—д«ге Ќ–Ё бџж ‘ѕ"
  ${LangFileString} MUI_UNTEXT_ABORT_SUBTITLE "»—д«ге Ќ–Ё »е ’ж—  днге  г«г Б«н«д н«Ё "
!endif

!ifdef MUI_FINISHPAGE
  ${LangFileString} MUI_TEXT_FINISH_INFO_TITLE "»—д«ге д’»  $(^NameDA) Б«н«д н«Ё "
  ${LangFileString} MUI_TEXT_FINISH_INFO_TEXT "$(^NameDA) »— —жн я«гБнж — ‘г« д’» ‘ѕ.$\r$\n$\r$\n»— —жн ѕяге Б«н«д »—«н ќ—жћ «“ «нд »—д«ге ябня яднѕ."
  ${LangFileString} MUI_TEXT_FINISH_INFO_REBOOT "я«гБнж — ‘г« »—«н  ягнб д’» $(^NameDA) »«н” н ѕж»«—е —«е «дѕ«“н ‘жѕ. ¬н« гнќж«енѕ «нд я«— —« «б«д «дћ«г ѕенѕњ"
!endif

!ifdef MUI_UNFINISHPAGE
  ${LangFileString} MUI_UNTEXT_FINISH_INFO_TITLE "»—д«ге Ќ–Ё $(^NameDA) Б«н«д н«Ё "
  ${LangFileString} MUI_UNTEXT_FINISH_INFO_TEXT "$(^NameDA) «“ —жн я«гБнж — ‘г« Ќ–Ё ‘ѕ.$\r$\n$\r$\n»— —жн ѕяге Б«н«д »—«н ќ—жћ «“ «нд »—д«ге ябня яднѕ."
  ${LangFileString} MUI_UNTEXT_FINISH_INFO_REBOOT "я«гБнж — ‘г« »—«н  ягнб Ќ–Ё$(^NameDA) »«н”  ѕж»«—е —«е «дѕ«“н ‘жѕ.¬н« гнќж«енѕ «нд я«— —« «б«д «дћ«г ѕенѕњ"
!endif

!ifdef MUI_FINISHPAGE | MUI_UNFINISHPAGE
  ${LangFileString} MUI_TEXT_FINISH_REBOOTNOW "—«е «дѕ«“н гћѕѕ."
  ${LangFileString} MUI_TEXT_FINISH_REBOOTLATER "гд ќжѕг «нд я«— —« «дћ«г ќж«ег ѕ«ѕ."
  ${LangFileString} MUI_TEXT_FINISH_RUN "&«ћ—«н  $(^NameDA)"
  ${LangFileString} MUI_TEXT_FINISH_SHOWREADME "&дг«н‘ Ё«нб  ж÷нЌ« "
  ${LangFileString} MUI_BUTTONTEXT_FINISH "&Б«н«д"  
!endif

!ifdef MUI_STARTMENUPAGE
  ${LangFileString} MUI_TEXT_STARTMENU_TITLE "«д ќ«» Бж‘е ѕ— гджн »—д«ге е«"
  ${LangFileString} MUI_TEXT_STARTMENU_SUBTITLE "Бж‘е «н яе гнќж«енѕ гн«д»—е«н  $(^NameDA) ѕ— ¬д ё—«— »Рн—дѕ —« «д ќ«» яднѕ."
  ${LangFileString} MUI_INNERTEXT_STARTMENU_TOP "Бж‘е «н ѕ— гджн »—д«ге е« яе гнќж«енѕ гн«д»—е«н »—д«ге ѕ— ¬дћ« «нћ«ѕ ‘ждѕ —« «д ќ«» яднѕ. »—«н «нћ«ѕ ня Бж‘е ћѕнѕ гн ж«днѕ ня д«г  «нБ яднѕ."
  ${LangFileString} MUI_INNERTEXT_STARTMENU_CHECKBOX "гн«д»—н д”«“"
!endif

!ifdef MUI_UNCONFIRMPAGE
  ${LangFileString} MUI_UNTEXT_CONFIRM_TITLE "Ќ–Ё $(^NameDA)"
  ${LangFileString} MUI_UNTEXT_CONFIRM_SUBTITLE "Б«я я—ѕд $(^NameDA) «“ —жн я«гБнж — ‘г«."
!endif

!ifdef MUI_ABORTWARNING
  ${LangFileString} MUI_TEXT_ABORTWARNING "¬н« гЎг∆днѕ яе гнќж«енѕ «“ »—д«ге д’» $(^Name) ќ«—ћ ‘жнѕњ"
!endif

!ifdef MUI_UNABORTWARNING
  ${LangFileString} MUI_UNTEXT_ABORTWARNING "¬н« гЎг∆днѕ яе гнќж«енѕ «“ »—д«ге Ќ–Ё  $(^Name) ќ«—ћ ‘жнѕњ"
!endif
