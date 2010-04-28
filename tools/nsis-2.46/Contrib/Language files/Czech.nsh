;Language: Czech (1029)
;By SELiCE (ls@selice.cz - http://ls.selice.cz)
;Corrected by Ondøej Vaniš - http://www.vanis.cz/ondra

!insertmacro LANGFILE "Czech" "Cesky"

!ifdef MUI_WELCOMEPAGE
  ${LangFileString} MUI_TEXT_WELCOME_INFO_TITLE "Vítejte v prùvodci instalace programu $(^NameDA)"
  ${LangFileString} MUI_TEXT_WELCOME_INFO_TEXT "Tento prùvodce Vás provede instalací $(^NameDA).$\r$\n$\r$\nPøed zaèátkem instalace je doporuèeno zavøít všechny ostatní aplikace. Toto umožní aktualizovat dùležité systémové soubory bez restartování Vašeho poèítaèe.$\r$\n$\r$\n$_CLICK"
!endif

!ifdef MUI_UNWELCOMEPAGE
  ${LangFileString} MUI_UNTEXT_WELCOME_INFO_TITLE "Vítejte v $(^NameDA) odinstalaèním prùvodci"
  ${LangFileString} MUI_UNTEXT_WELCOME_INFO_TEXT "Tento prùvodce Vás provede odinstalací $(^NameDA).$\r$\n$\r$\nPøed zaèátkem odinstalace, se pøesvìdète, že $(^NameDA) není spuštìn.$\r$\n$\r$\n$_CLICK"
!endif

!ifdef MUI_LICENSEPAGE
  ${LangFileString} MUI_TEXT_LICENSE_TITLE "Licenèní ujednání"
  ${LangFileString} MUI_TEXT_LICENSE_SUBTITLE "Pøed instalací programu $(^NameDA) si prosím prostudujte licenèní podmínky."
  ${LangFileString} MUI_INNERTEXT_LICENSE_BOTTOM "Jestliže souhlasíte se všemi podmínkami ujednání, zvolte 'Souhlasím' pro pokraèování. Pro instalaci programu $(^NameDA) je nutné souhlasit s licenèním ujednáním."
  ${LangFileString} MUI_INNERTEXT_LICENSE_BOTTOM_CHECKBOX "Jestliže souhlasíte se všemi podmínkami ujednání, zaškrtnìte níže uvedenou volbu. Pro instalaci programu $(^NameDA) je nutné souhlasit s licenèním ujednáním. $_CLICK"
  ${LangFileString} MUI_INNERTEXT_LICENSE_BOTTOM_RADIOBUTTONS "Jestliže souhlasíte se všemi podmínkami ujednání, zvolte první z možností uvedených níže. Pro instalaci programu $(^NameDA) je nutné souhlasit s licenèním ujednáním. $_CLICK"
!endif

!ifdef MUI_UNLICENSEPAGE
  ${LangFileString} MUI_UNTEXT_LICENSE_TITLE "Licenèní ujednání"
  ${LangFileString} MUI_UNTEXT_LICENSE_SUBTITLE "Pøed odinstalováním programu $(^NameDA) si prosím prostudujte licenèní podmínky."
  ${LangFileString} MUI_UNINNERTEXT_LICENSE_BOTTOM "Jestliže souhlasíte se všemi podmínkami ujednání, zvolte 'Souhlasím' pro pokraèování. Pro odinstalování programu $(^NameDA) je nutné souhlasit s licenèním ujednáním."
  ${LangFileString} MUI_UNINNERTEXT_LICENSE_BOTTOM_CHECKBOX "Jestliže souhlasíte se všemi podmínkami ujednání, zaškrtnìte níže uvedenou volbu. Pro odinstalování programu $(^NameDA) je nutné souhlasit s licenèním ujednáním. $_CLICK"
  ${LangFileString} MUI_UNINNERTEXT_LICENSE_BOTTOM_RADIOBUTTONS "Jestliže souhlasíte se všemi podmínkami ujednání, zvolte první z níže uvedených možností. Pro odinstalování programu $(^NameDA) je nutné souhlasit s licenèním ujednáním. $_CLICK"
!endif

!ifdef MUI_LICENSEPAGE | MUI_UNLICENSEPAGE
  ${LangFileString} MUI_INNERTEXT_LICENSE_TOP "Stisknutím klávesy Page Down posunete text licenèního ujednání."
!endif

!ifdef MUI_COMPONENTSPAGE
  ${LangFileString} MUI_TEXT_COMPONENTS_TITLE "Volba souèástí"
  ${LangFileString} MUI_TEXT_COMPONENTS_SUBTITLE "Zvolte souèásti programu $(^NameDA), které chcete nainstalovat."
!endif

!ifdef MUI_UNCOMPONENTSPAGE
  ${LangFileString} MUI_UNTEXT_COMPONENTS_TITLE "Volba souèástí"
  ${LangFileString} MUI_UNTEXT_COMPONENTS_SUBTITLE "Zvolte souèásti programu $(^NameDA), které chcete odinstalovat."
!endif

!ifdef MUI_COMPONENTSPAGE | MUI_UNCOMPONENTSPAGE
  ${LangFileString} MUI_INNERTEXT_COMPONENTS_DESCRIPTION_TITLE "Popis"
  !ifndef NSIS_CONFIG_COMPONENTPAGE_ALTERNATIVE
    ${LangFileString} MUI_INNERTEXT_COMPONENTS_DESCRIPTION_INFO "Pøi pohybu myší nad instalátorem programu se zobrazí její popis."
  !else
    ${LangFileString} MUI_INNERTEXT_COMPONENTS_DESCRIPTION_INFO "Pøi pohybu myší nad instalátorem programu se zobrazí její popis."
  !endif
!endif

!ifdef MUI_DIRECTORYPAGE
  ${LangFileString} MUI_TEXT_DIRECTORY_TITLE "Zvolte umístìní instalace"
  ${LangFileString} MUI_TEXT_DIRECTORY_SUBTITLE "Zvolte složku, do které bude program $(^NameDA) nainstalován."
!endif

!ifdef MUI_UNDIRECTORYPAGE
  ${LangFileString} MUI_UNTEXT_DIRECTORY_TITLE "Zvolte umístìní odinstalace"
  ${LangFileString} MUI_UNTEXT_DIRECTORY_SUBTITLE "Zvolte složku, ze které bude program $(^NameDA) odinstalován."
!endif

!ifdef MUI_INSTFILESPAGE
  ${LangFileString} MUI_TEXT_INSTALLING_TITLE "Instalace"
  ${LangFileString} MUI_TEXT_INSTALLING_SUBTITLE "Vyèkejte, prosím, na dokonèení instalace programu $(^NameDA)."
  ${LangFileString} MUI_TEXT_FINISH_TITLE "Instalace dokonèena"
  ${LangFileString} MUI_TEXT_FINISH_SUBTITLE "Instalace probìhla v poøádku."
  ${LangFileString} MUI_TEXT_ABORT_TITLE "Instalace pøerušena"
  ${LangFileString} MUI_TEXT_ABORT_SUBTITLE "Instalace nebyla dokonèena."
!endif

!ifdef MUI_UNINSTFILESPAGE
  ${LangFileString} MUI_UNTEXT_UNINSTALLING_TITLE "Odinstalace"
  ${LangFileString} MUI_UNTEXT_UNINSTALLING_SUBTITLE "Vyèkejte, prosím, na dokonèení odinstalace programu $(^NameDA)."
  ${LangFileString} MUI_UNTEXT_FINISH_TITLE "Odinstalace dokonèena"
  ${LangFileString} MUI_UNTEXT_FINISH_SUBTITLE "Odinstalace probìhla v poøádku."
  ${LangFileString} MUI_UNTEXT_ABORT_TITLE "Odinstalace pøerušena"
  ${LangFileString} MUI_UNTEXT_ABORT_SUBTITLE "Odinstalace nebyla dokonèena."
!endif

!ifdef MUI_FINISHPAGE
  ${LangFileString} MUI_TEXT_FINISH_INFO_TITLE "Dokonèení prùvodce programu $(^NameDA)"
  ${LangFileString} MUI_TEXT_FINISH_INFO_TEXT "Program $(^NameDA) byl nainstalován na Váš poèítaè.$\r$\n$\r$\nKliknìte 'Dokonèit' pro ukonèení prùvodce."
  ${LangFileString} MUI_TEXT_FINISH_INFO_REBOOT "Pro dokonèení instalace programu $(^NameDA) je nutno restartovat poèítaè. Chcete restatovat nyní?"
!endif

!ifdef MUI_UNFINISHPAGE
  ${LangFileString} MUI_UNTEXT_FINISH_INFO_TITLE "Dokonèuji odinstalaèního prùvodce $(^NameDA)"
  ${LangFileString} MUI_UNTEXT_FINISH_INFO_TEXT "$(^NameDA) byl odinstalován z Vašeho poèítaèe.$\r$\n$\r$\nKliknìte na 'Dokonèit' pro ukonèení tohoto prùvodce."
  ${LangFileString} MUI_UNTEXT_FINISH_INFO_REBOOT "Pro dokonèení odinstalace $(^NameDA) musí být Váš poèítaè restartován. Chcete restartovat nyní?"
!endif

!ifdef MUI_FINISHPAGE | MUI_UNFINISHPAGE
  ${LangFileString} MUI_TEXT_FINISH_REBOOTNOW "Restartovat nyní"
  ${LangFileString} MUI_TEXT_FINISH_REBOOTLATER "Restartovat ruènì pozdìji"
  ${LangFileString} MUI_TEXT_FINISH_RUN "&Spustit program $(^NameDA)"
  ${LangFileString} MUI_TEXT_FINISH_SHOWREADME "&Zobrazit Èti-mne"
  ${LangFileString} MUI_BUTTONTEXT_FINISH "&Dokonèit"  
!endif

!ifdef MUI_STARTMENUPAGE
  ${LangFileString} MUI_TEXT_STARTMENU_TITLE "Zvolte složku v Nabídce Start"
  ${LangFileString} MUI_TEXT_STARTMENU_SUBTITLE "Zvolte složku v Nabídce Start pro zástupce programu $(^NameDA)."
  ${LangFileString} MUI_INNERTEXT_STARTMENU_TOP "Zvolte složku v Nabídce Start, ve které chcete vytvoøit zástupce programu. Mùžete také zadat nové jméno pro vytvoøení nové složky."
  ${LangFileString} MUI_INNERTEXT_STARTMENU_CHECKBOX "Nevytváøet zástupce"
!endif

!ifdef MUI_UNCONFIRMPAGE
  ${LangFileString} MUI_UNTEXT_CONFIRM_TITLE "Odinstalovat program $(^NameDA)"
  ${LangFileString} MUI_UNTEXT_CONFIRM_SUBTITLE "Odebrat program $(^NameDA) z Vašeho poèítaèe."
!endif

!ifdef MUI_ABORTWARNING
  ${LangFileString} MUI_TEXT_ABORTWARNING "Opravdu chcete ukonèit instalaci programu $(^Name)?"
!endif

!ifdef MUI_UNABORTWARNING
  ${LangFileString} MUI_UNTEXT_ABORTWARNING "Skuteènì chcete ukonèit odinstalaci $(^Name)?"
!endif
