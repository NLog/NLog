;Language: Norwegian (2068)
;By Jonas Lindsrøm (jonasc_88@hotmail.com) Reviewed and fixed by Jan Ivar Beddari, d0der at online.no

!insertmacro LANGFILE "Norwegian" "Norwegian"

!ifdef MUI_WELCOMEPAGE
  ${LangFileString} MUI_TEXT_WELCOME_INFO_TITLE "Velkommen til veiviseren for installasjon av $(^NameDA) "
  ${LangFileString} MUI_TEXT_WELCOME_INFO_TEXT "Denne veiviseren vil lede deg gjennom installasjonen av $(^NameDA).$\r$\n$\r$\nDet anbefales at du avslutter alle andre programmer før du fortsetter. Dette vil la installasjonsprogrammet forandre på systemfiler uten at du må starte datamaskinen på nytt.$\r$\n$\r$\n$_CLICK"
!endif

!ifdef MUI_UNWELCOMEPAGE
  ${LangFileString} MUI_UNTEXT_WELCOME_INFO_TITLE "Velkommen til veiviseren for avinstallasjon av $(^NameDA)"
  ${LangFileString} MUI_UNTEXT_WELCOME_INFO_TEXT "Denne veiviseren vil lede deg gjennom avinstallasjonen av $(^NameDA).$\r$\n$\r$\nFør du fortsetter må du forsikre deg om at $(^NameDA) ikke kjører.$\r$\n$\r$\n$_CLICK"
!endif

!ifdef MUI_LICENSEPAGE
  ${LangFileString} MUI_TEXT_LICENSE_TITLE "Lisensavtale"
  ${LangFileString} MUI_TEXT_LICENSE_SUBTITLE "Vennligst les gjennom lisensavtalen før du starter installasjonen av $(^NameDA)."
  ${LangFileString} MUI_INNERTEXT_LICENSE_BOTTOM "Hvis du godtar lisensavtalen trykk Godta for å fortsette. Du må godta lisensavtalen for å installere $(^NameDA)."
  ${LangFileString} MUI_INNERTEXT_LICENSE_BOTTOM_CHECKBOX "Hvis du godtar lisensavtalen, kryss av på merket under. Du må godta lisensavtalen for å installere $(^NameDA). $_CLICK"
  ${LangFileString} MUI_INNERTEXT_LICENSE_BOTTOM_RADIOBUTTONS "Hvis du godtar lisensavtalen, velg det første alternativet ovenfor. Du må godta lisensavtalen for å installere $(^NameDA). $_CLICK"
!endif

!ifdef MUI_UNLICENSEPAGE
  ${LangFileString} MUI_UNTEXT_LICENSE_TITLE "Lisensavtale"
  ${LangFileString} MUI_UNTEXT_LICENSE_SUBTITLE "Vennligst les gjennom lisensavtalen før du avinstallerer $(^NameDA)."
  ${LangFileString} MUI_UNINNERTEXT_LICENSE_BOTTOM "Hvis du godtar lisensavtalen trykk Godta for å fortsette.  Du må godta lisensavtalen for å avintallere $(^NameDA)."
  ${LangFileString} MUI_UNINNERTEXT_LICENSE_BOTTOM_CHECKBOX "Hvis du godtar lisensavtalen, kryss av på merket under. Du må godta lisensavtalen for å avinstallere $(^NameDA). $_CLICK"
  ${LangFileString} MUI_UNINNERTEXT_LICENSE_BOTTOM_RADIOBUTTONS "Hvis du godtar lisensavtalen, velg det første alternativet ovenfor. Du må godta lisensavtalen for å avinstallere $(^NameDA). $_CLICK"
!endif

!ifdef MUI_LICENSEPAGE | MUI_UNLICENSEPAGE
  ${LangFileString} MUI_INNERTEXT_LICENSE_TOP "Trykk Page Down knappen for å se resten av lisensavtalen."
!endif

!ifdef MUI_COMPONENTSPAGE
  ${LangFileString} MUI_TEXT_COMPONENTS_TITLE "Velg komponenter"
  ${LangFileString} MUI_TEXT_COMPONENTS_SUBTITLE "Velg hvilke deler av $(^NameDA) du ønsker å installere."
!endif

!ifdef MUI_UNCOMPONENTSPAGE
  ${LangFileString} MUI_UNTEXT_COMPONENTS_TITLE "Velg komponenter"
  ${LangFileString} MUI_UNTEXT_COMPONENTS_SUBTITLE "Velg hvilke deler av $(^NameDA) du ønsker å avinstallere."
!endif

!ifdef MUI_COMPONENTSPAGE | MUI_UNCOMPONENTSPAGE
  ${LangFileString} MUI_INNERTEXT_COMPONENTS_DESCRIPTION_TITLE "Beskrivelse"
  !ifndef NSIS_CONFIG_COMPONENTPAGE_ALTERNATIVE
    ${LangFileString} MUI_INNERTEXT_COMPONENTS_DESCRIPTION_INFO "Beveg musen over komponentene for å se beskrivelsen."
  !else
    ${LangFileString} MUI_INNERTEXT_COMPONENTS_DESCRIPTION_INFO "Beveg musen over komponentene for å se beskrivelsen."
  !endif
!endif

!ifdef MUI_DIRECTORYPAGE
  ${LangFileString} MUI_TEXT_DIRECTORY_TITLE "Velg installasjonsmappe"
  ${LangFileString} MUI_TEXT_DIRECTORY_SUBTITLE "Velg hvilken mappe du vil installere $(^NameDA) i."
!endif

!ifdef MUI_UNDIRECTORYPAGE
  ${LangFileString} MUI_UNTEXT_DIRECTORY_TITLE "Velg mappe for avinstallasjon"
  ${LangFileString} MUI_UNTEXT_DIRECTORY_SUBTITLE "Velg mappen du vil avinstallere $(^NameDA) fra."
!endif

!ifdef MUI_INSTFILESPAGE
  ${LangFileString} MUI_TEXT_INSTALLING_TITLE "Installasjonen pågår"
  ${LangFileString} MUI_TEXT_INSTALLING_SUBTITLE "Vennligst vent mens $(^NameDA) blir installert."
  ${LangFileString} MUI_TEXT_FINISH_TITLE "Installasjonen er ferdig"
  ${LangFileString} MUI_TEXT_FINISH_SUBTITLE "Installasjonen ble fullført uten feil."
  ${LangFileString} MUI_TEXT_ABORT_TITLE "Installasjonen er avbrutt"
  ${LangFileString} MUI_TEXT_ABORT_SUBTITLE "Installasjonen ble ikke fullført riktig."
!endif

!ifdef MUI_UNINSTFILESPAGE
  ${LangFileString} MUI_UNTEXT_UNINSTALLING_TITLE "Avinstallasjon pågår"
  ${LangFileString} MUI_UNTEXT_UNINSTALLING_SUBTITLE "Vennligst vent mens $(^NameDA) blir avinstallert."
  ${LangFileString} MUI_UNTEXT_FINISH_TITLE "Avinstallasjon ferdig"
  ${LangFileString} MUI_UNTEXT_FINISH_SUBTITLE "Avinstallasjonen ble utført uten feil."
  ${LangFileString} MUI_UNTEXT_ABORT_TITLE "Avinstallasjon avbrutt"
  ${LangFileString} MUI_UNTEXT_ABORT_SUBTITLE "Avinstallasjonen ble ikke utført riktig."
!endif

!ifdef MUI_FINISHPAGE
  ${LangFileString} MUI_TEXT_FINISH_INFO_TITLE "Avslutter $(^NameDA) installasjonsveiviser"
  ${LangFileString} MUI_TEXT_FINISH_INFO_TEXT "$(^NameDA) er klart til bruk på din datamskin.$\r$\n$\r$\nTrykk Ferdig for å avslutte installasjonsprogrammet."
  ${LangFileString} MUI_TEXT_FINISH_INFO_REBOOT "Du må starte datamaskinen på nytt for å fullføre installasjonen av $(^NameDA). Vil du starte datamaskinen på nytt nå?"
!endif

!ifdef MUI_UNFINISHPAGE
  ${LangFileString} MUI_UNTEXT_FINISH_INFO_TITLE "Fullfører avinstallasjonen av $(^NameDA)"
  ${LangFileString} MUI_UNTEXT_FINISH_INFO_TEXT "$(^NameDA) har blitt avinstallert fra din datamaskin.$\r$\n$\r$\nTrykk på ferdig for å avslutte denne veiviseren."
  ${LangFileString} MUI_UNTEXT_FINISH_INFO_REBOOT "Datamaskinen må starte på nytt for å fullføre avinstallasjonen av $(^NameDA). Vil du starte datamaskinen på nytt nå?"
!endif

!ifdef MUI_FINISHPAGE | MUI_UNFINISHPAGE
  ${LangFileString} MUI_TEXT_FINISH_REBOOTNOW "Ja. Start datamaskinen på nytt nå"
  ${LangFileString} MUI_TEXT_FINISH_REBOOTLATER "Nei. Jeg vil starte datamaskinen på nytt senere"
  ${LangFileString} MUI_TEXT_FINISH_RUN "&Kjør $(^NameDA)"
  ${LangFileString} MUI_TEXT_FINISH_SHOWREADME "&Vis Readme filen"
  ${LangFileString} MUI_BUTTONTEXT_FINISH "&Ferdig"  
!endif

!ifdef MUI_STARTMENUPAGE
  ${LangFileString} MUI_TEXT_STARTMENU_TITLE "Velg plassering på startmenyen"
  ${LangFileString} MUI_TEXT_STARTMENU_SUBTITLE "Velg hvilken mappe snarveiene til $(^NameDA) skal ligge i."
  ${LangFileString} MUI_INNERTEXT_STARTMENU_TOP "Velg mappe for snarveiene til programmet. Du kan også skrive inn et nytt navn for å lage en ny mappe."
  ${LangFileString} MUI_INNERTEXT_STARTMENU_CHECKBOX "Ikke lag snarveier"
!endif

!ifdef MUI_UNCONFIRMPAGE
  ${LangFileString} MUI_UNTEXT_CONFIRM_TITLE "Avinstaller $(^NameDA)"
  ${LangFileString} MUI_UNTEXT_CONFIRM_SUBTITLE "Fjern $(^NameDA) fra din datamaskin."
!endif

!ifdef MUI_ABORTWARNING
  ${LangFileString} MUI_TEXT_ABORTWARNING "Er du sikker på at du vil avslutte installasjonen av $(^Name)?"
!endif

!ifdef MUI_UNABORTWARNING
  ${LangFileString} MUI_UNTEXT_ABORTWARNING "Er du sikker på at du vil avbryte avinstallasjonen av $(^Name)?"
!endif
