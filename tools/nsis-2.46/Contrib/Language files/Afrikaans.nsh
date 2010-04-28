;Language: Afrikaans (1078)
;By Friedel Wolff

!insertmacro LANGFILE "Afrikaans" "Afrikaans"

!ifdef MUI_WELCOMEPAGE
  ${LangFileString} MUI_TEXT_WELCOME_INFO_TITLE "Welkom by die $(^NameDA) Installasieslimmerd"
  ${LangFileString} MUI_TEXT_WELCOME_INFO_TEXT "Hierdie slimmerd lei mens deur die installasie van $(^NameDA).$\r$\n$\r$\nDit word aanbeveel dat u alle ander programme afsluit voor die begin van die installasie. Dit maak dit moontlik om die relevante stelsellêers op te dateer sonder om die rekenaar te herlaai.$\r$\n$\r$\n$_CLICK"
!endif

!ifdef MUI_UNWELCOMEPAGE
  ${LangFileString} MUI_UNTEXT_WELCOME_INFO_TITLE "Welkom by die $(^NameDA) Verwyderingslimmerd"
  ${LangFileString} MUI_UNTEXT_WELCOME_INFO_TEXT "Hierdie slimmerd lei mens deur die verwydering van $(^NameDA).$\r$\n$\r$\nVoor die verwydering begin word, maak seker dat $(^NameDA) nie loop nie.$\r$\n$\r$\n$_CLICK"
!endif

!ifdef MUI_LICENSEPAGE
  ${LangFileString} MUI_TEXT_LICENSE_TITLE "Lisensie-ooreenkoms"
  ${LangFileString} MUI_TEXT_LICENSE_SUBTITLE "Lees die lisensie-ooreenkoms voordat u $(^NameDA) installeer."
  ${LangFileString} MUI_INNERTEXT_LICENSE_BOTTOM "Klik op Regso om verder te gaan as u die ooreenkoms aanvaar. U moet die ooreenkoms aanvaar om $(^NameDA) te installeer."
  ${LangFileString} MUI_INNERTEXT_LICENSE_BOTTOM_CHECKBOX "Merk die blokkie hier onder as u die ooreenkoms aanvaar. U moet die ooreenkoms aanvaar om $(^NameDA) te installeer. $_CLICK"
  ${LangFileString} MUI_INNERTEXT_LICENSE_BOTTOM_RADIOBUTTONS "Kies die eerste keuse hieronder as u die ooreenkoms aanvaar. U moet die ooreenkoms aanvaar om $(^NameDA) te installeer. $_CLICK"
!endif

!ifdef MUI_UNLICENSEPAGE
  ${LangFileString} MUI_UNTEXT_LICENSE_TITLE "Lisensie-ooreenkoms"
  ${LangFileString} MUI_UNTEXT_LICENSE_SUBTITLE "Lees die lisensie-ooreenkoms voordat u $(^NameDA) verwyder."
  ${LangFileString} MUI_UNINNERTEXT_LICENSE_BOTTOM "Klik op Regso om verder te gaan as u die ooreenkoms aanvaar. U moet die ooreenkoms aanvaar om $(^NameDA) te verwyder."
  ${LangFileString} MUI_UNINNERTEXT_LICENSE_BOTTOM_CHECKBOX "Merk die kiesblokkie hieronder as u die ooreenkoms aanvaar. U moet die ooreenkoms aanvaar om $(^NameDA) te verwyder."
  ${LangFileString} MUI_UNINNERTEXT_LICENSE_BOTTOM_RADIOBUTTONS "Kies die eerste keuse hieronder as u die ooreenkoms aanvaar. U moet die ooreenkoms aanvaar om $(^NameDA) te verwyder."
!endif

!ifdef MUI_LICENSEPAGE | MUI_UNLICENSEPAGE
  ${LangFileString} MUI_INNERTEXT_LICENSE_TOP "Druk op Page Down om die res van die ooreenkoms te sien."
!endif

!ifdef MUI_COMPONENTSPAGE
  ${LangFileString} MUI_TEXT_COMPONENTS_TITLE "Kies komponente"
  ${LangFileString} MUI_TEXT_COMPONENTS_SUBTITLE "Kies watter komponente van $(^NameDA) geïnstalleer moet word."
!endif

!ifdef MUI_UNCOMPONENTSPAGE
  ${LangFileString} MUI_UNTEXT_COMPONENTS_TITLE "Kies komponente"
  ${LangFileString} MUI_UNTEXT_COMPONENTS_SUBTITLE "Kies watter komponente van $(^NameDA) verwyder moet word."
!endif

!ifdef MUI_COMPONENTSPAGE | MUI_UNCOMPONENTSPAGE
  ${LangFileString} MUI_INNERTEXT_COMPONENTS_DESCRIPTION_TITLE "Beskrywing"
  !ifndef NSIS_CONFIG_COMPONENTPAGE_ALTERNATIVE
    ${LangFileString} MUI_INNERTEXT_COMPONENTS_DESCRIPTION_INFO "Beweeg die muis oor 'n komponent om sy beskrywing te sien."
  !else
    ${LangFileString} MUI_INNERTEXT_COMPONENTS_DESCRIPTION_INFO "Beweeg die muis oor 'n komponent om sy beskrywing te sien."
  !endif
!endif

!ifdef MUI_DIRECTORYPAGE
  ${LangFileString} MUI_TEXT_DIRECTORY_TITLE "Kies installasieplek"
  ${LangFileString} MUI_TEXT_DIRECTORY_SUBTITLE "Kies die gids waarin u $(^NameDA) wil installeer."
!endif

!ifdef MUI_UNDIRECTORYPAGE
  ${LangFileString} MUI_UNTEXT_DIRECTORY_TITLE "Kies verwyderinggids"
  ${LangFileString} MUI_UNTEXT_DIRECTORY_SUBTITLE "Kies die gids waaruit u $(^NameDA) wil verwyder."
!endif

!ifdef MUI_INSTFILESPAGE
  ${LangFileString} MUI_TEXT_INSTALLING_TITLE "Installeer tans"
  ${LangFileString} MUI_TEXT_INSTALLING_SUBTITLE "Wag asb. terwyl $(^NameDA) geïnstalleer word."
  ${LangFileString} MUI_TEXT_FINISH_TITLE "Installasie voltooid"
  ${LangFileString} MUI_TEXT_FINISH_SUBTITLE "Die installasie is suksesvol voltooi."
  ${LangFileString} MUI_TEXT_ABORT_TITLE "Installasie gestaak"
  ${LangFileString} MUI_TEXT_ABORT_SUBTITLE "Die installasie is nie suksesvol voltooi nie."
!endif

!ifdef MUI_UNINSTFILESPAGE
  ${LangFileString} MUI_UNTEXT_UNINSTALLING_TITLE "Verwyder tans"
  ${LangFileString} MUI_UNTEXT_UNINSTALLING_SUBTITLE "Wag asb. terwyl $(^NameDA) van u rekenaar verwyder word."
  ${LangFileString} MUI_UNTEXT_FINISH_TITLE "Verwydering voltooi"
  ${LangFileString} MUI_UNTEXT_FINISH_SUBTITLE "Verwydering is suksesvol voltooi."
  ${LangFileString} MUI_UNTEXT_ABORT_TITLE "Verwydering gestaak"
  ${LangFileString} MUI_UNTEXT_ABORT_SUBTITLE "Verwydering is nie suksesvol voltooi nie."
!endif

!ifdef MUI_FINISHPAGE
  ${LangFileString} MUI_TEXT_FINISH_INFO_TITLE "Voltooi van die $(^NameDA) Installasieslimmerd"
  ${LangFileString} MUI_TEXT_FINISH_INFO_TEXT "$(^NameDA) is geïnstalleer op uw rekenaar.$\r$\n$\r$\nKlik op Voltooi om hierdie slimmerd af te sluit."
  ${LangFileString} MUI_TEXT_FINISH_INFO_REBOOT "Die rekenaar moet oorbegin word om die installasie van $(^NameDA) te voltooi. Wil u nou oorbegin?"
!endif

!ifdef MUI_UNFINISHPAGE
  ${LangFileString} MUI_UNTEXT_FINISH_INFO_TITLE "Voltooi van die $(^NameDA) Verwyderingslimmerd"
  ${LangFileString} MUI_UNTEXT_FINISH_INFO_TEXT "$(^NameDA) is van u rekenaar verwyder.$\r$\n$\r$\nKlik op Voltooi om hierdie slimmerd af te sluit."
  ${LangFileString} MUI_UNTEXT_FINISH_INFO_REBOOT "Die rekenaar moet oorbegin word om die verwydering van $(^NameDA) te voltooi. Wil u nou oorbegin?"
!endif

!ifdef MUI_FINISHPAGE | MUI_UNFINISHPAGE
  ${LangFileString} MUI_TEXT_FINISH_REBOOTNOW "Begin nou oor"
  ${LangFileString} MUI_TEXT_FINISH_REBOOTLATER "Ek wil later self oorbegin"
  ${LangFileString} MUI_TEXT_FINISH_RUN "&Laat loop $(^NameDA)"
  ${LangFileString} MUI_TEXT_FINISH_SHOWREADME "&Wys Leesmy-lêer"
  ${LangFileString} MUI_BUTTONTEXT_FINISH "&Voltooi"  
!endif

!ifdef MUI_STARTMENUPAGE
  ${LangFileString} MUI_TEXT_STARTMENU_TITLE "Kies gids in Begin-kieslys"
  ${LangFileString} MUI_TEXT_STARTMENU_SUBTITLE "Kies 'n gids in die Begin-kieslys vir $(^NameDA) se kortpaaie."
  ${LangFileString} MUI_INNERTEXT_STARTMENU_TOP "Kies die gids in die Begin-kieslys waarin die program se kortpaaie geskep moet word. U kan ook 'n nuwe naam gee om 'n nuwe gids te skep."
  ${LangFileString} MUI_INNERTEXT_STARTMENU_CHECKBOX "Moenie kortpaaie maak nie"
!endif

!ifdef MUI_UNCONFIRMPAGE
  ${LangFileString} MUI_UNTEXT_CONFIRM_TITLE "Verwyder $(^NameDA)"
  ${LangFileString} MUI_UNTEXT_CONFIRM_SUBTITLE "Verwyder $(^NameDA) van u rekenaar."
!endif

!ifdef MUI_ABORTWARNING
  ${LangFileString} MUI_TEXT_ABORTWARNING "Wil u definitief die installasie van $(^Name) afsluit?"
!endif

!ifdef MUI_UNABORTWARNING
  ${LangFileString} MUI_UNTEXT_ABORTWARNING "Wil u definitief die verwydering van $(^Name) afsluit?"
!endif
