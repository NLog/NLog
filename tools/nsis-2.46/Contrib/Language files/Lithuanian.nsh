;Language: Lithuanian (1063)
;By Vytautas Krivickas (Vytautas). Updated by Danielius Scepanskis (Daan daniel@takas.lt) 2004.01.09

!insertmacro LANGFILE "Lithuanian" "Lietuviu"

!ifdef MUI_WELCOMEPAGE
  ${LangFileString} MUI_TEXT_WELCOME_INFO_TITLE "Sveiki atvykæ á $(^NameDA) ádiegimo programà."
  ${LangFileString} MUI_TEXT_WELCOME_INFO_TEXT "Ði programa jums padës lengvai ádiegti $(^NameDA).$\r$\n$\r$\nRekomenduojama iðjungti visas programas, prieð pradedant ádiegimà. Tai leis atnaujinti sistemos failus neperkraunat kompiuterio.$\r$\n$\r$\n"
!endif

!ifdef MUI_UNWELCOMEPAGE
  ${LangFileString} MUI_UNTEXT_WELCOME_INFO_TITLE "Sveiki atvykæ á $(^NameDA) paðalinimo programà."
  ${LangFileString} MUI_UNTEXT_WELCOME_INFO_TEXT "Ði programa jums padës lengvai iðtrinti $(^NameDA).$\r$\n$\r$\nPrieð pradedant pasitikrinkite kad $(^NameDA) yra iðjungta.$\r$\n$\r$\n"
!endif

!ifdef MUI_LICENSEPAGE
  ${LangFileString} MUI_TEXT_LICENSE_TITLE "Naudojimo sutartis"
  ${LangFileString} MUI_TEXT_LICENSE_SUBTITLE "Praðome perskaityti sutartá prieð ádiegdami $(^NameDA)."
  ${LangFileString} MUI_INNERTEXT_LICENSE_BOTTOM "Jei jûs sutinkate su nurodytomis sàlygomis, spauskite Sutinku. Jûs privalote sutikti, jei norite ádiegti $(^NameDA)."
  ${LangFileString} MUI_INNERTEXT_LICENSE_BOTTOM_CHECKBOX "Jei jûs sutinkate su nurodytomis sàlygomis, padëkite varnelæ tam skirtame laukelyje. Jûs privalote sutikti, jei norite ádiegti $(^NameDA). "
  ${LangFileString} MUI_INNERTEXT_LICENSE_BOTTOM_RADIOBUTTONS "Jei jûs sutinkate su nurodytomis sàlygomis, pasirinkite pirmà pasirinkimà esantá þemiau. Jûs privalote sutikti, jei norite ádiegti $(^NameDA). "
!endif

!ifdef MUI_UNLICENSEPAGE
  ${LangFileString} MUI_UNTEXT_LICENSE_TITLE "Naudojimo sutartis"
  ${LangFileString} MUI_UNTEXT_LICENSE_SUBTITLE "Praðome perskaityti sutartá prieð $(^NameDA) paðalinimà."
  ${LangFileString} MUI_UNINNERTEXT_LICENSE_BOTTOM "Jei jûs sutinkate su nurodytomis sàlygomis, spauskite Sutinku. Jûs privalote sutikti, jei norite iðtrinti $(^NameDA)."
  ${LangFileString} MUI_UNINNERTEXT_LICENSE_BOTTOM_CHECKBOX "s, padëkite varnelæ tam skirtame laukelyje. Jûs privalote sutikti, jei norite iðtrinti $(^NameDA). "
  ${LangFileString} MUI_UNINNERTEXT_LICENSE_BOTTOM_RADIOBUTTONS "Jei jûs sutinkate su nurodytomis sàlygomis, pasirinkite pirmà pasirinkimà esantá þemiau. Jûs privalote sutikti, jei norite iðtrinti $(^NameDA)."
!endif

!ifdef MUI_LICENSEPAGE | MUI_UNLICENSEPAGE
  ${LangFileString} MUI_INNERTEXT_LICENSE_TOP "Paspauskite Page Down ir perskaitykite visà sutartá."
!endif

!ifdef MUI_COMPONENTSPAGE
  ${LangFileString} MUI_TEXT_COMPONENTS_TITLE "Pasirinkite"
  ${LangFileString} MUI_TEXT_COMPONENTS_SUBTITLE "Pasirinkite kokias $(^NameDA) galimybes jûs norite ádiegti."
!endif

!ifdef MUI_UNCOMPONENTSPAGE
  ${LangFileString} MUI_UNTEXT_COMPONENTS_TITLE "Pasirinkite"
  ${LangFileString} MUI_UNTEXT_COMPONENTS_SUBTITLE "Pasirinkite kokias $(^NameDA) galimybes jûs norite paðalinti."
!endif

!ifdef MUI_COMPONENTSPAGE | MUI_UNCOMPONENTSPAGE
  ${LangFileString} MUI_INNERTEXT_COMPONENTS_DESCRIPTION_TITLE "Paaiðkinimas"
  !ifndef NSIS_CONFIG_COMPONENTPAGE_ALTERNATIVE
    ${LangFileString} MUI_INNERTEXT_COMPONENTS_DESCRIPTION_INFO "Uþveskite pelës þymeklá ant komponento ir pamatysite jo apraðymà."
  !else
    ${LangFileString} MUI_INNERTEXT_COMPONENTS_DESCRIPTION_INFO "Uþveskite pelës þymeklá ant komponento ir pamatysite jo apraðymà."
  !endif
!endif

!ifdef MUI_DIRECTORYPAGE
  ${LangFileString} MUI_TEXT_DIRECTORY_TITLE "Pasirinkite ádiegimo vietà"
  ${LangFileString} MUI_TEXT_DIRECTORY_SUBTITLE "Pasirinkite katalogà á kûri ádiegsite $(^NameDA)."
!endif

!ifdef MUI_UNDIRECTORYPAGE
  ${LangFileString} MUI_UNTEXT_DIRECTORY_TITLE "Pasirinkite iðtrinimo vietà"
  ${LangFileString} MUI_UNTEXT_DIRECTORY_SUBTITLE "Pasirinkite katalogà ið kurio iðtrinsite $(^NameDA)."
!endif

!ifdef MUI_INSTFILESPAGE
  ${LangFileString} MUI_TEXT_INSTALLING_TITLE "Diegiama"
  ${LangFileString} MUI_TEXT_INSTALLING_SUBTITLE "Praðome palaukti, kol $(^NameDA) bus ádiegtas."
  ${LangFileString} MUI_TEXT_FINISH_TITLE "Ádiegimas baigtas"
  ${LangFileString} MUI_TEXT_FINISH_SUBTITLE "Ádiegimas baigtas sekmingai."
  ${LangFileString} MUI_TEXT_ABORT_TITLE "Ádiegimas nutrauktas"
  ${LangFileString} MUI_TEXT_ABORT_SUBTITLE "Ádiegimas nebuvo baigtas sekmingai."
!endif

!ifdef MUI_UNINSTFILESPAGE
  ${LangFileString} MUI_UNTEXT_UNINSTALLING_TITLE "Ðalinama"
  ${LangFileString} MUI_UNTEXT_UNINSTALLING_SUBTITLE "Praðome palaukti, kol $(^NameDA) bus paðalinta."
  ${LangFileString} MUI_UNTEXT_FINISH_TITLE "Programos paðalinimas baigtas"
  ${LangFileString} MUI_UNTEXT_FINISH_SUBTITLE "Iðtrynimas baigtas sekmingai."
  ${LangFileString} MUI_UNTEXT_ABORT_TITLE "Iðtrynimas nutrauktas"
  ${LangFileString} MUI_UNTEXT_ABORT_SUBTITLE "Iðtrynimas nebuvo baigtas sekmingai."
!endif

!ifdef MUI_FINISHPAGE
  ${LangFileString} MUI_TEXT_FINISH_INFO_TITLE "Baigiu $(^NameDA) ádiegimo procesà"
  ${LangFileString} MUI_TEXT_FINISH_INFO_TEXT "$(^NameDA) buvo ádiegtas á jûsø kompiuterá.$\r$\n$\r$\nPaspauskite Baigti."
  ${LangFileString} MUI_TEXT_FINISH_INFO_REBOOT "Jûsø kompiuteris turi bûti perkrautas, kad bûtø baigtas $(^NameDA) ádiegimas. Ar jûs norite perkrauti dabar?"
!endif

!ifdef MUI_UNFINISHPAGE
  ${LangFileString} MUI_UNTEXT_FINISH_INFO_TITLE "Baigiu $(^NameDA) paðalinimo programà."
  ${LangFileString} MUI_UNTEXT_FINISH_INFO_TEXT "$(^NameDA) buvo iðtrinta ið jûsø kompiuterio.$\r$\n$\r$\nPaspauskite Baigti."
  ${LangFileString} MUI_UNTEXT_FINISH_INFO_REBOOT "Jûsø kompiuteris turi bûti perkrautas, kad bûtø baigtas $(^NameDA) paðalinimas. Ar jûs norite perkrauti dabar?"
!endif

!ifdef MUI_FINISHPAGE | MUI_UNFINISHPAGE
  ${LangFileString} MUI_TEXT_FINISH_REBOOTNOW "Perkrauti dabar"
  ${LangFileString} MUI_TEXT_FINISH_REBOOTLATER "Að noriu perkrauti veliau pats"
  ${LangFileString} MUI_TEXT_FINISH_RUN "&Leisti $(^NameDA)"
  ${LangFileString} MUI_TEXT_FINISH_SHOWREADME "&Parodyti dokumentacijà"
  ${LangFileString} MUI_BUTTONTEXT_FINISH "&Baigti"  
!endif

!ifdef MUI_STARTMENUPAGE
  ${LangFileString} MUI_TEXT_STARTMENU_TITLE "Pasirinkite Start Menu katalogà"
  ${LangFileString} MUI_TEXT_STARTMENU_SUBTITLE "Pasirinkite Start Menu katalogà, kuriame bus sukurtos programos nuorodos."
  ${LangFileString} MUI_INNERTEXT_STARTMENU_TOP "Pasirinkite Start Menu katalogà, kuriame bus sukurtos programos nuorodos. Jûs taip pat galite sukurti naujà katalogà."
  ${LangFileString} MUI_INNERTEXT_STARTMENU_CHECKBOX "Nekurti nuorodø"
!endif

!ifdef MUI_UNCONFIRMPAGE
  ${LangFileString} MUI_UNTEXT_CONFIRM_TITLE "Panaikinti $(^NameDA)"
  ${LangFileString} MUI_UNTEXT_CONFIRM_SUBTITLE "Iðtrinti $(^NameDA) ið jûsø kompiuterio."
!endif

!ifdef MUI_ABORTWARNING
  ${LangFileString} MUI_TEXT_ABORTWARNING "Ar jûs tikrai norite iðjungti $(^Name) ádiegimo programà?"
!endif

!ifdef MUI_UNABORTWARNING
  ${LangFileString} MUI_UNTEXT_ABORTWARNING "Ar jûs tikrai norite iðjungti $(^Name) paðalinimo programà?"
!endif
