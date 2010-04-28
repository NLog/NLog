;Language: Slovenian (1060)
;By Janez Dolinar, edited by Martin Srebotnjak - Lugos.si

!insertmacro LANGFILE "Slovenian" "Slovenski jezik"

!ifdef MUI_WELCOMEPAGE
  ${LangFileString} MUI_TEXT_WELCOME_INFO_TITLE "Dobrodošli v èarovniku namestitve $(^NameDA)"
  ${LangFileString} MUI_TEXT_WELCOME_INFO_TEXT "Ta èarovnik vas vodi skozi namestitev programa $(^NameDA).$\r$\n$\r$\nPred namestitvijo je priporoèeno zapreti vsa ostala okna in programe. S tem omogoèite nemoteno namestitev programa in potrebnih sistemskih datotek brez ponovnega zagona raèunalnika.$\r$\n$\r$\n$_CLICK"
!endif

!ifdef MUI_UNWELCOMEPAGE
  ${LangFileString} MUI_UNTEXT_WELCOME_INFO_TITLE "Dobrodošli v èarovniku za odstranitev $(^NameDA)"
  ${LangFileString} MUI_UNTEXT_WELCOME_INFO_TEXT "Ta èarovnik vas bo vodil skozi odstranitev $(^NameDA).$\r$\n$\r$\nPreden priènete z odstranitvijo, se preprièajte, da program $(^NameDA) ni zagnan.$\r$\n$\r$\n$_CLICK"
!endif

!ifdef MUI_LICENSEPAGE
  ${LangFileString} MUI_TEXT_LICENSE_TITLE "Licenèna pogodba"
  ${LangFileString} MUI_TEXT_LICENSE_SUBTITLE "Prosimo, da si ogledate pogoje licenène pogodbe pred namestitvijo $(^NameDA)."
  ${LangFileString} MUI_INNERTEXT_LICENSE_BOTTOM "Èe se strinjate s pogoji, pritisnite Se strinjam. Da bi lahko namestili $(^NameDA), se morate s pogodbo strinjati."
  ${LangFileString} MUI_INNERTEXT_LICENSE_BOTTOM_CHECKBOX "Èe se strinjate z licenènimi pogoji pogodbe, spodaj izberite ustrezno okence. Za namestitev $(^NameDA) se morate strinjati s pogoji pogodbe. $_CLICK"
  ${LangFileString} MUI_INNERTEXT_LICENSE_BOTTOM_RADIOBUTTONS "Èe se strinjate z licenènimi pogoji pogodbe, spodaj izberite prvo možnost. Za namestitev $(^NameDA) se morate strinjati s pogoji pogodbe. $_CLICK"
!endif

!ifdef MUI_UNLICENSEPAGE
  ${LangFileString} MUI_UNTEXT_LICENSE_TITLE "Licenèna pogodba"
  ${LangFileString} MUI_UNTEXT_LICENSE_SUBTITLE "Prosimo, da pred odstranitvijo $(^NameDA) pregledate pogoje licenène pogodbe."
  ${LangFileString} MUI_UNINNERTEXT_LICENSE_BOTTOM "Èe se strinjate s pogoji licenène pogodbe, izberite Se strinjam. Za odstranitev $(^NameDA) se morate strinjati s pogoji."
  ${LangFileString} MUI_UNINNERTEXT_LICENSE_BOTTOM_CHECKBOX "Èe se strinjate s pogoji licenène pogodbe, kliknite na okence spodaj. Za odstranitev $(^NameDA) se morate strinjati s pogoji. $_CLICK"
  ${LangFileString} MUI_UNINNERTEXT_LICENSE_BOTTOM_RADIOBUTTONS "Èe se strinjate s pogoji licenène pogodbe, spodaj izberite prvo podano možnost. Za odstranitev $(^NameDA) se morate strinjati s pogoji. $_CLICK"
!endif

!ifdef MUI_LICENSEPAGE | MUI_UNLICENSEPAGE
  ${LangFileString} MUI_INNERTEXT_LICENSE_TOP "Za preostali del pogodbe pritisnite tipko 'Page Down'."
!endif

!ifdef MUI_COMPONENTSPAGE
  ${LangFileString} MUI_TEXT_COMPONENTS_TITLE "Izbor komponent"
  ${LangFileString} MUI_TEXT_COMPONENTS_SUBTITLE "Izberite, katere komponente izdelka $(^NameDA) želite namestiti."
!endif

!ifdef MUI_UNCOMPONENTSPAGE
  ${LangFileString} MUI_UNTEXT_COMPONENTS_TITLE "Izbor komponent"
  ${LangFileString} MUI_UNTEXT_COMPONENTS_SUBTITLE "Izberite komponente $(^NameDA), ki jih želite odstraniti."
!endif

!ifdef MUI_COMPONENTSPAGE | MUI_UNCOMPONENTSPAGE
  ${LangFileString} MUI_INNERTEXT_COMPONENTS_DESCRIPTION_TITLE "Opis"
  !ifndef NSIS_CONFIG_COMPONENTPAGE_ALTERNATIVE
    ${LangFileString} MUI_INNERTEXT_COMPONENTS_DESCRIPTION_INFO "Povlecite miško nad komponento, da vidite njen opis."
  !else
    ${LangFileString} MUI_INNERTEXT_COMPONENTS_DESCRIPTION_INFO "Povlecite miško nad komponento, da vidite njen opis."
  !endif
!endif

!ifdef MUI_DIRECTORYPAGE
  ${LangFileString} MUI_TEXT_DIRECTORY_TITLE "Izberite pot namestive"
  ${LangFileString} MUI_TEXT_DIRECTORY_SUBTITLE "Izberite mapo, v katero želite namestiti $(^NameDA)."
!endif

!ifdef MUI_UNDIRECTORYPAGE
  ${LangFileString} MUI_UNTEXT_DIRECTORY_TITLE "Izbor mape"
  ${LangFileString} MUI_UNTEXT_DIRECTORY_SUBTITLE "Izberite mapo, iz katere želite odstraniti $(^NameDA)."
!endif

!ifdef MUI_INSTFILESPAGE
  ${LangFileString} MUI_TEXT_INSTALLING_TITLE "Namešèanje poteka"
  ${LangFileString} MUI_TEXT_INSTALLING_SUBTITLE "Prosimo, poèakajte, $(^NameDA) se namešèa."
  ${LangFileString} MUI_TEXT_FINISH_TITLE "Dokonèana namestitev"
  ${LangFileString} MUI_TEXT_FINISH_SUBTITLE "Namestitev je uspešno zakljuèena."
  ${LangFileString} MUI_TEXT_ABORT_TITLE "Prekinjena namestitev"
  ${LangFileString} MUI_TEXT_ABORT_SUBTITLE "Namestitev ni bila uspešno zakljuèena."
!endif

!ifdef MUI_UNINSTFILESPAGE
  ${LangFileString} MUI_UNTEXT_UNINSTALLING_TITLE "Odstranjevanje poteka"
  ${LangFileString} MUI_UNTEXT_UNINSTALLING_SUBTITLE "Prosimo, poèakajte, dokler se program $(^NameDA) odstranjuje."
  ${LangFileString} MUI_UNTEXT_FINISH_TITLE "Odstranitev konèana"
  ${LangFileString} MUI_UNTEXT_FINISH_SUBTITLE "Odstranitev je uspešno konèana."
  ${LangFileString} MUI_UNTEXT_ABORT_TITLE "Odstranitev prekinjena"
  ${LangFileString} MUI_UNTEXT_ABORT_SUBTITLE "Odstranitev ni bila konèana uspešno."
!endif

!ifdef MUI_FINISHPAGE
  ${LangFileString} MUI_TEXT_FINISH_INFO_TITLE "Zakljuèevanje namestitve $(^NameDA)"
  ${LangFileString} MUI_TEXT_FINISH_INFO_TEXT "Program $(^NameDA) je bil namešèen na vaš raèunalnik.$\r$\n$\r$\nPritisnite Dokonèaj za zaprtje èarovnika."
  ${LangFileString} MUI_TEXT_FINISH_INFO_REBOOT "Za dokonèanje namestitve $(^NameDA) morate ponovno zagnati raèunalnik. Želite zdaj ponovno zagnati raèunalnik?"
!endif

!ifdef MUI_UNFINISHPAGE
  ${LangFileString} MUI_UNTEXT_FINISH_INFO_TITLE "Èarovnik za odstranitev $(^NameDA) se zakljuèuje"
  ${LangFileString} MUI_UNTEXT_FINISH_INFO_TEXT "Program $(^NameDA) je odstranjen z vašega raèunalnika.$\r$\n$\r$\nKliknite Dokonèaj, da zaprete èarovnika."
  ${LangFileString} MUI_UNTEXT_FINISH_INFO_REBOOT "Da bi se namestitev $(^NameDA) dokonèala, morate ponovno zagnati raèunalnik. Želite zdaj znova zagnati raèunalnik?"
!endif

!ifdef MUI_FINISHPAGE | MUI_UNFINISHPAGE
  ${LangFileString} MUI_TEXT_FINISH_REBOOTNOW "Ponovni zagon"
  ${LangFileString} MUI_TEXT_FINISH_REBOOTLATER "Raèunalnik želim znova zagnati kasneje"
  ${LangFileString} MUI_TEXT_FINISH_RUN "&Zaženi $(^NameDA)"
  ${LangFileString} MUI_TEXT_FINISH_SHOWREADME "&Pokaži BeriMe"
  ${LangFileString} MUI_BUTTONTEXT_FINISH "Do&konèaj"  
!endif

!ifdef MUI_STARTMENUPAGE
  ${LangFileString} MUI_TEXT_STARTMENU_TITLE "Izberite mapo menija Start"
  ${LangFileString} MUI_TEXT_STARTMENU_SUBTITLE "Izberite mapo menija Start za bližnjice do $(^NameDA)."
  ${LangFileString} MUI_INNERTEXT_STARTMENU_TOP "Izberite mapo menija Start, kjer želite ustvariti bližnjico do programa. Èe vpišete novo ime, boste ustvarili istoimensko mapo."
  ${LangFileString} MUI_INNERTEXT_STARTMENU_CHECKBOX "Ne ustvari bližnjic"
!endif

!ifdef MUI_UNCONFIRMPAGE
  ${LangFileString} MUI_UNTEXT_CONFIRM_TITLE "Odstranitev $(^NameDA)"
  ${LangFileString} MUI_UNTEXT_CONFIRM_SUBTITLE "Odstrani $(^NameDA) z vašega raèunalnika."
!endif

!ifdef MUI_ABORTWARNING
  ${LangFileString} MUI_TEXT_ABORTWARNING "Ste preprièani, da želite prekiniti namestitev $(^Name)?"
!endif

!ifdef MUI_UNABORTWARNING
  ${LangFileString} MUI_UNTEXT_ABORTWARNING "Ste preprièani, da želite zapustiti odstranitev $(^Name)?"
!endif

!ifdef MULTIUSER_INSTALLMODEPAGE
  ${LangFileString} MULTIUSER_TEXT_INSTALLMODE_TITLE "Izberite uporabnike"
  ${LangFileString} MULTIUSER_TEXT_INSTALLMODE_SUBTITLE "Izberite uporabnike, za katere želite namestiti $(^NameDA)."
  ${LangFileString} MULTIUSER_INNERTEXT_INSTALLMODE_TOP "Izberite, ali želite namestiti $(^NameDA) le zase ali za vse uporabnike tega raèunalnika. $(^ClickNext)"
  ${LangFileString} MULTIUSER_INNERTEXT_INSTALLMODE_ALLUSERS "Namesti za vse uporabnike tega raèunalnika"
  ${LangFileString} MULTIUSER_INNERTEXT_INSTALLMODE_CURRENTUSER "Namesti le zame"
!endif
