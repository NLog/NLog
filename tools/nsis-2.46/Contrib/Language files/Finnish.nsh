;Compatible with Modern UI 1.86
;Language: Finnish (1035)
;By Eclipser (Jonne Lehtinen) <Eclipser at pilvikaupunki dot com>
;Updated by Puuhis (puuhis@puuhis.net)

!insertmacro LANGFILE "Finnish" "Suomi"

!ifdef MUI_WELCOMEPAGE
  ${LangFileString} MUI_TEXT_WELCOME_INFO_TITLE "Tervetuloa ohjelman $(^NameDA) asennukseen"
  ${LangFileString} MUI_TEXT_WELCOME_INFO_TEXT "T‰m‰ avustaja ohjaa sinut ohjelman $(^NameDA) asennuksen l‰pi.$\r$\n$\r$\nOn suositeltavaa sulkea kaikki muut ohjelmat ennen asennuksen aloittamista, jotta asennus voisi p‰ivitt‰‰ tiettyj‰ j‰rjestelm‰tiedostoja k‰ynnist‰m‰tt‰ konetta uudelleen.$\r$\n$\r$\n$_CLICK"
!endif

!ifdef MUI_UNWELCOMEPAGE
  ${LangFileString} MUI_UNTEXT_WELCOME_INFO_TITLE "Tervetuloa $(^NameDA) -ohjelmiston poisto-ohjelmaan"
  ${LangFileString} MUI_UNTEXT_WELCOME_INFO_TEXT "T‰m‰ velho auttaa sinut l‰pi $(^NameDA) -ohjelmiston poistamisen.$\r$\n$\r$\nEnnen poisto-ohjelman aloitusta, varmista ettei $(^NameDA) ole k‰ynniss‰.$\r$\n$\r$\n$_CLICK"
!endif

!ifdef MUI_LICENSEPAGE
  ${LangFileString} MUI_TEXT_LICENSE_TITLE "Lisenssisopimus"
  ${LangFileString} MUI_TEXT_LICENSE_SUBTITLE "Lue lisenssiehdot tarkasti ennen ohjelman $(^NameDA) asentamista."
  ${LangFileString} MUI_INNERTEXT_LICENSE_BOTTOM "Jos hyv‰ksyt ehdot, valitse Hyv‰ksyn jatkaaksesi. Sinun pit‰‰ hyv‰ksy‰ ehdot asentaaksesi ohjelman $(^NameDA)."
  ${LangFileString} MUI_INNERTEXT_LICENSE_BOTTOM_CHECKBOX "Jos hyv‰ksyt ehdot, laita rasti alla olevaan ruutuun. Sinun pit‰‰ hyv‰ksy‰ ehdot asentaaksesi ohjelman $(^NameDA). $_CLICK"
  ${LangFileString} MUI_INNERTEXT_LICENSE_BOTTOM_RADIOBUTTONS "Jos hyv‰ksyt ehdot, valitse ensimm‰inen vaihtoehto alapuolelta. Sinun pit‰‰ hyv‰ksy‰ ehdot asentaaksesi ohjelman $(^NameDA). $_CLICK"
!endif

!ifdef MUI_UNLICENSEPAGE
  ${LangFileString} MUI_UNTEXT_LICENSE_TITLE "Lisenssisopimus"
  ${LangFileString} MUI_UNTEXT_LICENSE_SUBTITLE "Lue huolellisesti lisenssiehdot ennen $(^NameDA) -ohjelmiston poistoa."
  ${LangFileString} MUI_UNINNERTEXT_LICENSE_BOTTOM "Jos hyv‰ksyt s‰‰nnˆt ja ehdot, paina Hyv‰ksyn -nappia jatkaakseni. Sinun t‰ytyy hyv‰ksy‰ ehdot poistaaksesi $(^NameDA) -ohjelmiston."
  ${LangFileString} MUI_UNINNERTEXT_LICENSE_BOTTOM_CHECKBOX "Jos hyv‰ksyt ehdot, klikkaa valintaruutua alhaalla. Sinun t‰ytyy hyv‰ksy‰ ehdot poistaaksesi $(^NameDA) -ohjelmiston. $_CLICK"
  ${LangFileString} MUI_UNINNERTEXT_LICENSE_BOTTOM_RADIOBUTTONS "Jos hyv‰ksyt ehdot, valitse ensimm‰inen vaihtoehto alhaalta. Sinun t‰ytyy hyv‰ksy‰ ehdot poistaaksesi $(^NameDA) -ohjelmiston. $_CLICK"
!endif

!ifdef MUI_LICENSEPAGE | MUI_UNLICENSEPAGE
  ${LangFileString} MUI_INNERTEXT_LICENSE_TOP "Paina Page Down n‰hd‰ksesi loput sopimuksesta."
!endif

!ifdef MUI_COMPONENTSPAGE
  ${LangFileString} MUI_TEXT_COMPONENTS_TITLE "Valitse komponentit"
  ${LangFileString} MUI_TEXT_COMPONENTS_SUBTITLE "Valitse toiminnot, jotka haluat asentaa ohjelmaan $(^NameDA)."
!endif

!ifdef MUI_UNCOMPONENTSPAGE
  ${LangFileString} MUI_UNTEXT_COMPONENTS_TITLE "Valitse komponentit"
  ${LangFileString} MUI_UNTEXT_COMPONENTS_SUBTITLE "Valitse $(^NameDA) toiminnot, jotka haluat poistaa."
!endif

!ifdef MUI_COMPONENTSPAGE | MUI_UNCOMPONENTSPAGE
  ${LangFileString} MUI_INNERTEXT_COMPONENTS_DESCRIPTION_TITLE "Selitys"
  !ifndef NSIS_CONFIG_COMPONENTPAGE_ALTERNATIVE
    ${LangFileString} MUI_INNERTEXT_COMPONENTS_DESCRIPTION_INFO "Siirr‰ hiiri komponentin nimen p‰‰lle saadaksesi sen selityksen."
  !else
    ${LangFileString} MUI_INNERTEXT_COMPONENTS_DESCRIPTION_INFO "Siirr‰ hiiri komponentin nimen p‰‰lle saadaksesi sen selityksen."
  !endif
!endif

!ifdef MUI_DIRECTORYPAGE
  ${LangFileString} MUI_TEXT_DIRECTORY_TITLE "Valitse asennuskohde"
  ${LangFileString} MUI_TEXT_DIRECTORY_SUBTITLE "Valitse hakemisto, johon haluat asentaa ohjelman $(^NameDA)."
!endif

!ifdef MUI_UNDIRECTORYPAGE
  ${LangFileString} MUI_UNTEXT_DIRECTORY_TITLE "Valitse paikka mist‰ poistetaan"
  ${LangFileString} MUI_UNTEXT_DIRECTORY_SUBTITLE "Valitse kansio mist‰ $(^NameDA) poistetaan."
!endif

!ifdef MUI_INSTFILESPAGE
  ${LangFileString} MUI_TEXT_INSTALLING_TITLE "Asennetaan"
  ${LangFileString} MUI_TEXT_INSTALLING_SUBTITLE "Odota... $(^NameDA) asennetaan..."
  ${LangFileString} MUI_TEXT_FINISH_TITLE "Asennus valmis"
  ${LangFileString} MUI_TEXT_FINISH_SUBTITLE "Asennus valmistui onnistuneesti."
  ${LangFileString} MUI_TEXT_ABORT_TITLE "Asennus keskeytettiin"
  ${LangFileString} MUI_TEXT_ABORT_SUBTITLE "Asennus ei onnistunut."
!endif

!ifdef MUI_UNINSTFILESPAGE
  ${LangFileString} MUI_UNTEXT_UNINSTALLING_TITLE "Poistetaan"
  ${LangFileString} MUI_UNTEXT_UNINSTALLING_SUBTITLE "Odota... Ohjelmaa $(^NameDA) poistetaan."
  ${LangFileString} MUI_UNTEXT_FINISH_TITLE "Poisto valmis"
  ${LangFileString} MUI_UNTEXT_FINISH_SUBTITLE "Ohjelma poistettiin onnistuneesti."
  ${LangFileString} MUI_UNTEXT_ABORT_TITLE "Poisto lopetettu"
  ${LangFileString} MUI_UNTEXT_ABORT_SUBTITLE "Ohjelmaa poisto ep‰onnistuneesti."
!endif

!ifdef MUI_FINISHPAGE
  ${LangFileString} MUI_TEXT_FINISH_INFO_TITLE "Viimeistell‰‰n ohjelman $(^NameDA) asennusta"
  ${LangFileString} MUI_TEXT_FINISH_INFO_TEXT "$(^NameDA) on asennettu koneellesi.$\r$\n$\r$\nValitse Valmis sulkeaksesi avustajan."
  ${LangFileString} MUI_TEXT_FINISH_INFO_REBOOT "Tietokoneesi pit‰‰ k‰ynnist‰‰ uudelleen jotta ohjelman $(^NameDA) asennus saataisiin valmiiksi. Haluatko k‰ynnist‰‰ koneen uudelleen nyt?"
!endif

!ifdef MUI_UNFINISHPAGE
  ${LangFileString} MUI_UNTEXT_FINISH_INFO_TITLE "Viimeistell‰‰n $(^NameDA) -ohjelmiston poistamista"
  ${LangFileString} MUI_UNTEXT_FINISH_INFO_TEXT "$(^NameDA) on poistettu koneeltasi.$\r$\n$\r$\nPaina Lopeta -nappia sulkeaksesi t‰m‰n velhon."
  ${LangFileString} MUI_UNTEXT_FINISH_INFO_REBOOT "Jotta $(^NameDA) -ohjelmiston poistaminen olisi valmis, tulee tietokone k‰ynnist‰‰ uudelleen. Haluatko uudelleenk‰ynnist‰‰ nyt?"
!endif

!ifdef MUI_FINISHPAGE | MUI_UNFINISHPAGE
  ${LangFileString} MUI_TEXT_FINISH_REBOOTNOW "K‰ynnist‰ uudelleen nyt"
  ${LangFileString} MUI_TEXT_FINISH_REBOOTLATER "K‰ynnist‰n koneen myˆhemmin uudelleen"
  ${LangFileString} MUI_TEXT_FINISH_RUN "K‰ynnist‰ $(^NameDA)"
  ${LangFileString} MUI_TEXT_FINISH_SHOWREADME "N‰yt‰ LueMinut"
  ${LangFileString} MUI_BUTTONTEXT_FINISH "&Valmis"  
!endif

!ifdef MUI_STARTMENUPAGE
  ${LangFileString} MUI_TEXT_STARTMENU_TITLE "Valitse K‰ynnist‰-valikon hakemisto"
  ${LangFileString} MUI_TEXT_STARTMENU_SUBTITLE "Valitse K‰ynnist‰-valikon hakemisto ohjelman pikakuvakkeille."
  ${LangFileString} MUI_INNERTEXT_STARTMENU_TOP "Valitse K‰ynnist‰-valikon hakemisto, johon haluaisit luoda ohjelman pikakuvakkeet. Voit myˆs kirjoittaa uuden nimen."
  ${LangFileString} MUI_INNERTEXT_STARTMENU_CHECKBOX "ƒl‰ luo pikakuvakkeita"
!endif

!ifdef MUI_UNCONFIRMPAGE
  ${LangFileString} MUI_UNTEXT_CONFIRM_TITLE "Poista $(^NameDA)"
  ${LangFileString} MUI_UNTEXT_CONFIRM_SUBTITLE "Poista $(^NameDA) tietokoneestasi."
!endif

!ifdef MUI_ABORTWARNING
  ${LangFileString} MUI_TEXT_ABORTWARNING "Haluatko varmasti lopettaa $(^Name) Asennuksen?"
!endif

!ifdef MUI_UNABORTWARNING
  ${LangFileString} MUI_UNTEXT_ABORTWARNING "Oletko varma ett‰ haluat poistua $(^Name) poisto-ohjelmasta?"
!endif
