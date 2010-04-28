;Compatible with Modern UI 1.72
;Language: Swedish (1053)
;By Magnus Bonnevier (magnus.bonnevier@telia.com), updated by Rickard Angbratt (r.angbratt@home.se), updated by Ulf Axelsson (ulf.axelsson@gmail.com)

!insertmacro LANGFILE "Swedish" "Svenska"

!ifdef MUI_WELCOMEPAGE
  ${LangFileString} MUI_TEXT_WELCOME_INFO_TITLE "Välkommen till installationsguiden för $(^NameDA)."
  ${LangFileString} MUI_TEXT_WELCOME_INFO_TEXT "Denna guide tar dig igenom installationen av $(^NameDA).$\r$\n$\r$\nDet rekommenderas att du avslutar alla andra program innan du fortsätter installationen. Detta tillåter att installationen uppdaterar nödvändiga systemfiler utan att behöva starta om din dator.$\r$\n$\r$\n$_CLICK"
!endif

!ifdef MUI_UNWELCOMEPAGE
  ${LangFileString} MUI_UNTEXT_WELCOME_INFO_TITLE "Välkommen till avinstallationsguiden för $(^NameDA)."
  ${LangFileString} MUI_UNTEXT_WELCOME_INFO_TEXT "Denna guide tar dig igenom avinstallationen av $(^NameDA).$\r$\n$\r$\nInnan du startar avinstallationen, försäkra dig om att $(^NameDA) inte körs.$\r$\n$\r$\n$_CLICK"
!endif

!ifdef MUI_LICENSEPAGE
  ${LangFileString} MUI_TEXT_LICENSE_TITLE "Licensavtal"
  ${LangFileString} MUI_TEXT_LICENSE_SUBTITLE "Var vänlig läs igenom licensvillkoren innan du installerar $(^NameDA)."
  ${LangFileString} MUI_INNERTEXT_LICENSE_BOTTOM "Om du accepterar villkoren i avtalet, klicka Jag Godkänner för att fortsätta. Du måste acceptera avtalet för att installera $(^NameDA)."
  ${LangFileString} MUI_INNERTEXT_LICENSE_BOTTOM_CHECKBOX "Om du accepterar villkoren i avtalet, klicka i checkrutan nedan. Du måste acceptera avtalet för att installera $(^NameDA). $_CLICK"
  ${LangFileString} MUI_INNERTEXT_LICENSE_BOTTOM_RADIOBUTTONS "Om du accepterar villkoren i avtalet, välj det första alternativet nedan. Du måste acceptera avtalet för att installera $(^NameDA). $_CLICK"
!endif

!ifdef MUI_UNLICENSEPAGE
  ${LangFileString} MUI_UNTEXT_LICENSE_TITLE "Licensavtal"
  ${LangFileString} MUI_UNTEXT_LICENSE_SUBTITLE "Var vänlig läs igenom licensvillkoren innan du avinstallerar $(^NameDA)."
  ${LangFileString} MUI_UNINNERTEXT_LICENSE_BOTTOM "Om du accepterar villkoren i avtalet, klicka Jag Godkänner för att fortsätta. Du måste acceptera avtalet för att avinstallera $(^NameDA)."
  ${LangFileString} MUI_UNINNERTEXT_LICENSE_BOTTOM_CHECKBOX "Om du accepterar villkoren i avtalet, klicka i checkrutan nedan. Du måste acceptera avtalet för att avinstallera $(^NameDA). $_CLICK"
  ${LangFileString} MUI_UNINNERTEXT_LICENSE_BOTTOM_RADIOBUTTONS "Om du accepterar villkoren i avtalet, välj det första alternativet nedan. Du måste acceptera avtalet för att avinstallera $(^NameDA). $_CLICK"
!endif

!ifdef MUI_LICENSEPAGE | MUI_UNLICENSEPAGE
  ${LangFileString} MUI_INNERTEXT_LICENSE_TOP "Tryck Page Down för att se resten av licensavtalet."
!endif

!ifdef MUI_COMPONENTSPAGE
  ${LangFileString} MUI_TEXT_COMPONENTS_TITLE "Välj komponenter"
  ${LangFileString} MUI_TEXT_COMPONENTS_SUBTITLE "Välj vilka alternativ av $(^NameDA) som du vill installera."
!endif

!ifdef MUI_UNCOMPONENTSPAGE
  ${LangFileString} MUI_UNTEXT_COMPONENTS_TITLE "Välj komponenter"
  ${LangFileString} MUI_UNTEXT_COMPONENTS_SUBTITLE "Välj vilka alternativ av $(^NameDA) som du vill avinstallera."
!endif

!ifdef MUI_COMPONENTSPAGE | MUI_UNCOMPONENTSPAGE
  ${LangFileString} MUI_INNERTEXT_COMPONENTS_DESCRIPTION_TITLE "Beskrivning"
  !ifndef NSIS_CONFIG_COMPONENTPAGE_ALTERNATIVE
    ${LangFileString} MUI_INNERTEXT_COMPONENTS_DESCRIPTION_INFO "Håll muspekaren över ett alternativ för att se dess beskrivning."
  !else
    ${LangFileString} MUI_INNERTEXT_COMPONENTS_DESCRIPTION_INFO "Håll muspekaren över ett alternativ för att se dess beskrivning."
  !endif
!endif

!ifdef MUI_DIRECTORYPAGE
  ${LangFileString} MUI_TEXT_DIRECTORY_TITLE "Välj installationsväg"
  ${LangFileString} MUI_TEXT_DIRECTORY_SUBTITLE "Välj katalog att installera $(^NameDA) i."
!endif

!ifdef MUI_UNDIRECTORYPAGE
  ${LangFileString} MUI_UNTEXT_DIRECTORY_TITLE "Välj avinstallationsväg"
  ${LangFileString} MUI_UNTEXT_DIRECTORY_SUBTITLE "Välj katalog att avinstallera $(^NameDA) från."
!endif

!ifdef MUI_INSTFILESPAGE
  ${LangFileString} MUI_TEXT_INSTALLING_TITLE "Installerar"
  ${LangFileString} MUI_TEXT_INSTALLING_SUBTITLE "Var vänlig vänta medan $(^NameDA) installeras."
  ${LangFileString} MUI_TEXT_FINISH_TITLE "Installationen är klar"
  ${LangFileString} MUI_TEXT_FINISH_SUBTITLE "Guiden avslutades korrekt."
  ${LangFileString} MUI_TEXT_ABORT_TITLE "Installationen avbröts"
  ${LangFileString} MUI_TEXT_ABORT_SUBTITLE "Guiden genomfördes inte korrekt."
!endif

!ifdef MUI_UNINSTFILESPAGE
  ${LangFileString} MUI_UNTEXT_UNINSTALLING_TITLE "Avinstallerar"
  ${LangFileString} MUI_UNTEXT_UNINSTALLING_SUBTITLE "Var vänlig vänta medan $(^NameDA) avinstalleras."
  ${LangFileString} MUI_UNTEXT_FINISH_TITLE "Avinstallationen genomförd"
  ${LangFileString} MUI_UNTEXT_FINISH_SUBTITLE "Avinstallationen genomfördes korrekt."
  ${LangFileString} MUI_UNTEXT_ABORT_TITLE "Avinstallationen avbruten"
  ${LangFileString} MUI_UNTEXT_ABORT_SUBTITLE "Avinstallationen genomfördes inte korrekt."
!endif

!ifdef MUI_FINISHPAGE
  ${LangFileString} MUI_TEXT_FINISH_INFO_TITLE "Avslutar installationsguiden för $(^NameDA)."
  ${LangFileString} MUI_TEXT_FINISH_INFO_TEXT "$(^NameDA) har installerats på din dator.$\r$\n$\r$\nKlicka på Slutför för att avsluta guiden."
  ${LangFileString} MUI_TEXT_FINISH_INFO_REBOOT "Din dator måste startas om för att fullborda installationen av $(^NameDA). Vill du starta om nu?"
!endif

!ifdef MUI_UNFINISHPAGE
  ${LangFileString} MUI_UNTEXT_FINISH_INFO_TITLE "Avslutar avinstallationsguiden för $(^NameDA)."
  ${LangFileString} MUI_UNTEXT_FINISH_INFO_TEXT "$(^NameDA) komponenter har avinstallerats från din dator.$\r$\n$\r$\nKlicka på Slutför för att avsluta guiden."
  ${LangFileString} MUI_UNTEXT_FINISH_INFO_REBOOT "Din dator måste startas om för att fullborda avinstallationen av $(^NameDA). Vill du starta om nu?"
!endif

!ifdef MUI_FINISHPAGE | MUI_UNFINISHPAGE
  ${LangFileString} MUI_TEXT_FINISH_REBOOTNOW "Starta om nu"
  ${LangFileString} MUI_TEXT_FINISH_REBOOTLATER "Jag vill starta om själv senare"
  ${LangFileString} MUI_TEXT_FINISH_RUN "&Kör $(^NameDA)"
  ${LangFileString} MUI_TEXT_FINISH_SHOWREADME "&Visa Readme-filen"
  ${LangFileString} MUI_BUTTONTEXT_FINISH "&Slutför"  
!endif

!ifdef MUI_STARTMENUPAGE
  ${LangFileString} MUI_TEXT_STARTMENU_TITLE "Välj Startmenykatalog"
  ${LangFileString} MUI_TEXT_STARTMENU_SUBTITLE "Välj en Startmenykatalog för programmets genvägar."
  ${LangFileString} MUI_INNERTEXT_STARTMENU_TOP "Välj startmenykatalog i vilken du vill skapa programmets genvägar. Du kan ange ett eget namn för att skapa en ny katalog."
  ${LangFileString} MUI_INNERTEXT_STARTMENU_CHECKBOX "Skapa ej genvägar"
!endif

!ifdef MUI_UNCONFIRMPAGE
  ${LangFileString} MUI_UNTEXT_CONFIRM_TITLE "Avinstallera $(^NameDA)"
  ${LangFileString} MUI_UNTEXT_CONFIRM_SUBTITLE "Ta bort $(^NameDA) från din dator."
!endif

!ifdef MUI_ABORTWARNING
  ${LangFileString} MUI_TEXT_ABORTWARNING "Är du säker på att du vill avbryta installationen av $(^Name)?"
!endif

!ifdef MUI_UNABORTWARNING
  ${LangFileString} MUI_UNTEXT_ABORTWARNING "Är du säker på att du vill avbryta avinstallationen av $(^Name)?"
!endif
