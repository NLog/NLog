;Language: Romanian (1048)
;Translated by Cristian Pirvu (pcristip@yahoo.com)
;Updates by Sorin Sbarnea - INTERSOL SRL (sbarneasorin@intersol.ro) - ROBO Design (www.robodesign.ro)
;New revision by George Radu (georadu@hotmail.com) http://mediatae.3x.ro
;New revision by Vlad Rusu (vlad@bitattack.ro)
;	- Use Romanian letters ãâîºþ
;	- ".. produsului" removed as unnecessary
;	- "Eliminã" related terms replaced with more appropiate "Dezinstaleazã"
;	- Misc language tweaks
!insertmacro LANGFILE "Romanian" "Romana"

!ifdef MUI_WELCOMEPAGE
  ${LangFileString} MUI_TEXT_WELCOME_INFO_TITLE "Bine aþi venit la instalarea $(^NameDA)"
  ${LangFileString} MUI_TEXT_WELCOME_INFO_TEXT "Aceastã aplicaþie va instala $(^NameDA).$\r$\n$\r$\nEste recomandat sã închideþi toate aplicaþiile înainte de începerea procesului de instalare. Acest lucru vã poate asigura un proces de instalare fãrã erori sau situaþii neprevãzute.$\r$\n$\r$\n$_CLICK"
!endif

!ifdef MUI_UNWELCOMEPAGE
  ${LangFileString} MUI_UNTEXT_WELCOME_INFO_TITLE "Bine aþi venit la dezinstalarea $(^NameDA)"
  ${LangFileString} MUI_UNTEXT_WELCOME_INFO_TEXT "Aceastã aplicaþie va dezinstala $(^NameDA).$\r$\n$\r$\nEste recomandat sã închideþi toate aplicaþiile înainte de începerea procesului de dezinstalare. Acest lucru vã poate asigura un proces de dezinstalare fãrã erori sau situaþii neprevãzute.$\r$\n$\r$\n$_CLICK"
!endif

!ifdef MUI_WELCOMEPAGE | MUI_UNWELCOMEPAGE
  ${LangFileString} MUI_BUTTONTEXT_FINISH "&Terminare"
!endif

!ifdef MUI_LICENSEPAGE
  ${LangFileString} MUI_TEXT_LICENSE_TITLE "Contract de licenþã"
  ${LangFileString} MUI_TEXT_LICENSE_SUBTITLE "Citiþi cu atenþie termenii contractului de licenþã înainte de a instala $(^NameDA)."
  ${LangFileString} MUI_INNERTEXT_LICENSE_BOTTOM "Dacã acceptaþi termenii contractului de licenþã, apãsati De Acord. Pentru a instala $(^NameDA) trebuie sã acceptaþi termenii din contractul de licenþã."
  ${LangFileString} MUI_INNERTEXT_LICENSE_BOTTOM_CHECKBOX "Dacã acceptaþi termenii contractului de licenþã, bifaþi cãsuþa de mai jos. Pentru a instala $(^NameDA) trebuie sã acceptaþi termenii din contractul de licenþã. $_CLICK"
  ${LangFileString} MUI_INNERTEXT_LICENSE_BOTTOM_RADIOBUTTONS "Dacã acceptaþi termenii contractului de licenþã, selectaþi prima opþiune de mai jos. Pentru a instala $(^NameDA) trebuie sã acceptaþi termenii din contractul de licenþã. $_CLICK"
!endif

!ifdef MUI_UNLICENSEPAGE
  ${LangFileString} MUI_UNTEXT_LICENSE_TITLE "Contract de licenþã"
  ${LangFileString} MUI_UNTEXT_LICENSE_SUBTITLE "Citiþi cu atenþie termenii contractului de licenþã înainte de a dezinstala $(^NameDA)."
  ${LangFileString} MUI_UNINNERTEXT_LICENSE_BOTTOM "Dacã acceptaþi termenii contractului de licenþã, apãsati De Acord. Pentru a dezinstala $(^NameDA) trebuie sã acceptaþi termenii din contractul de licenþã."
  ${LangFileString} MUI_UNINNERTEXT_LICENSE_BOTTOM_CHECKBOX "Dacã acceptaþi termenii contractului de licenþã, bifaþi cãsuþa de mai jos. Pentru a dezinstala $(^NameDA) trebuie sã acceptaþi termenii din contractul de licenþã. $_CLICK"
  ${LangFileString} MUI_UNINNERTEXT_LICENSE_BOTTOM_RADIOBUTTONS "Dacã acceptaþi termenii contractului de licenþã, selectaþi prima opþiune de mai jos. Pentru a dezinstala $(^NameDA) trebuie sã acceptaþi termenii din contractul de licenþã. $_CLICK"
!endif

!ifdef MUI_LICENSEPAGE | MUI_UNLICENSEPAGE
  ${LangFileString} MUI_INNERTEXT_LICENSE_TOP "Apãsaþi Page Down pentru a vizualiza restul contractului de licenþã."
!endif

!ifdef MUI_COMPONENTSPAGE
  ${LangFileString} MUI_TEXT_COMPONENTS_TITLE "Selectare componente"
  ${LangFileString} MUI_TEXT_COMPONENTS_SUBTITLE "Selectaþi componentele $(^NameDA) pe care doriþi sã le instalaþi."
!endif

!ifdef MUI_UNCOMPONENTSPAGE
  ${LangFileString} MUI_UNTEXT_COMPONENTS_TITLE "Selectare componente"
  ${LangFileString} MUI_UNTEXT_COMPONENTS_SUBTITLE "Selectaþi componentele $(^NameDA) pe care doriþi sã le dezinstalaþi."
!endif

!ifdef MUI_COMPONENTSPAGE | MUI_UNCOMPONENTSPAGE
  ${LangFileString} MUI_INNERTEXT_COMPONENTS_DESCRIPTION_TITLE "Descriere"
  !ifndef NSIS_CONFIG_COMPONENTPAGE_ALTERNATIVE
    ${LangFileString} MUI_INNERTEXT_COMPONENTS_DESCRIPTION_INFO "Aºezaþi mouse-ul deasupra fiecãrei componente pentru a vizualiza descrierea acesteia."
  !else
    ${LangFileString} MUI_INNERTEXT_COMPONENTS_DESCRIPTION_INFO "Aºezaþi mouse-ul deasupra fiecãrei componente pentru a vizualiza descrierea acesteia."
  !endif
!endif

!ifdef MUI_DIRECTORYPAGE
  ${LangFileString} MUI_TEXT_DIRECTORY_TITLE "Selectare director destinaþie"
  ${LangFileString} MUI_TEXT_DIRECTORY_SUBTITLE "Selectaþi directorul în care doriþi sã instalaþi $(^NameDA)."
!endif

!ifdef MUI_UNDIRECTORYPAGE
  ${LangFileString} MUI_UNTEXT_DIRECTORY_TITLE "Selectare director de dezinstalat"
  ${LangFileString} MUI_UNTEXT_DIRECTORY_SUBTITLE "Selectaþi directorul din care doriþi sã dezinstalaþi $(^NameDA)."
!endif

!ifdef MUI_INSTFILESPAGE
  ${LangFileString} MUI_TEXT_INSTALLING_TITLE "În curs de instalare"
  ${LangFileString} MUI_TEXT_INSTALLING_SUBTITLE "Vã rugãm sã aºteptaþi, $(^NameDA) se instaleazã."
  ${LangFileString} MUI_TEXT_FINISH_TITLE "Instalare terminatã"
  ${LangFileString} MUI_TEXT_FINISH_SUBTITLE "Instalarea s-a terminat cu succes."
  ${LangFileString} MUI_TEXT_ABORT_TITLE "Instalare anulatã"
  ${LangFileString} MUI_TEXT_ABORT_SUBTITLE "Instalarea a fost anulatã de utilizator."
!endif

!ifdef MUI_UNINSTFILESPAGE
  ${LangFileString} MUI_UNTEXT_UNINSTALLING_TITLE "În curs de dezinstalare"
  ${LangFileString} MUI_UNTEXT_UNINSTALLING_SUBTITLE "Vã rugãm sã aºteptaþi, $(^NameDA) se dezinstaleazã."
  ${LangFileString} MUI_UNTEXT_FINISH_TITLE "Dezinstalare terminatã"
  ${LangFileString} MUI_UNTEXT_FINISH_SUBTITLE "Dezinstalarea s-a terminat cu succes."
  ${LangFileString} MUI_UNTEXT_ABORT_TITLE "Dezinstalare anulatã"
  ${LangFileString} MUI_UNTEXT_ABORT_SUBTITLE "Dezinstalarea fost anulatã de utilizator."
!endif

!ifdef MUI_FINISHPAGE
  ${LangFileString} MUI_TEXT_FINISH_INFO_TITLE "Terminare instalare $(^NameDA)"
  ${LangFileString} MUI_TEXT_FINISH_INFO_TEXT "$(^NameDA) a fost instalat.$\r$\n$\r$\nApãsaþi Terminare pentru a încheia instalarea."
  ${LangFileString} MUI_TEXT_FINISH_INFO_REBOOT "Trebuie sã reporniþi calculatorul pentru a termina instalarea. Doriþi sã-l reporniþi acum?"
!endif

!ifdef MUI_UNFINISHPAGE
  ${LangFileString} MUI_UNTEXT_FINISH_INFO_TITLE "Terminare dezinstalare $(^NameDA)"
  ${LangFileString} MUI_UNTEXT_FINISH_INFO_TEXT "$(^NameDA) a fost dezinstalat.$\r$\n$\r$\nApãsaþi Terminare pentru a încheia dezinstalarea."
  ${LangFileString} MUI_UNTEXT_FINISH_INFO_REBOOT "Trebuie sã reporniþi calculatorul pentru a termina dezinstalarea. Doriþi sã-l reporniþi acum?"
!endif

!ifdef MUI_FINISHPAGE | MUI_UNFINISHPAGE
  ${LangFileString} MUI_TEXT_FINISH_REBOOTNOW "Reporneºte acum"
  ${LangFileString} MUI_TEXT_FINISH_REBOOTLATER "Repornesc eu mai târziu"
  ${LangFileString} MUI_TEXT_FINISH_RUN "Executare $(^NameDA)"
  ${LangFileString} MUI_TEXT_FINISH_SHOWREADME "Afiºare fiºier readme (citeºte-mã)."
!endif

!ifdef MUI_STARTMENUPAGE
  ${LangFileString} MUI_TEXT_STARTMENU_TITLE "Selectare grup Meniul Start"
  ${LangFileString} MUI_TEXT_STARTMENU_SUBTITLE "Selectaþi un grup in Meniul Start pentru a crea comenzi rapide pentru produs."
  ${LangFileString} MUI_INNERTEXT_STARTMENU_TOP "Selectaþi grupul din Meniul Start în care vor fi create comenzi rapide pentru produs. Puteþi de asemenea sã creaþi un grup nou."
  ${LangFileString} MUI_INNERTEXT_STARTMENU_CHECKBOX "Nu doresc comenzi rapide"
!endif

!ifdef MUI_UNCONFIRMPAGE
  ${LangFileString} MUI_UNTEXT_CONFIRM_TITLE "Dezinstalare $(^NameDA)"
  ${LangFileString} MUI_UNTEXT_CONFIRM_SUBTITLE "Dezinstalare $(^NameDA) din calculatorul dumneavoastrã."
!endif

!ifdef MUI_ABORTWARNING
  ${LangFileString} MUI_TEXT_ABORTWARNING "Sunteþi sigur(ã) cã doriþi sã anulaþi instalarea $(^Name)?"
!endif

!ifdef MUI_UNABORTWARNING
  ${LangFileString} MUI_UNTEXT_ABORTWARNING "Sunteþi sigur(ã) cã doriþi sã anulaþi dezinstalarea $(^Name)?"
!endif
