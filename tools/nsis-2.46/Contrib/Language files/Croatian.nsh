;Language: Croatian (1050)
;By Igor Ostriz

!insertmacro LANGFILE "Croatian" "Hrvatski"

!ifdef MUI_WELCOMEPAGE
  ${LangFileString} MUI_TEXT_WELCOME_INFO_TITLE "Dobrodošli u instalaciju programa $(^NameDA)"
  ${LangFileString} MUI_TEXT_WELCOME_INFO_TEXT "Instalacija programa $(^NameDA) na Vaše raèunalo sastoji se od nekoliko jednostavnih koraka kroz koje æe Vas provesti ovaj èarobnjak.$\r$\n$\r$\nPreporuèamo zatvaranje svih ostalih aplikacija prije samog poèetka instalacije. To æe omoguæiti nadogradnju nekih sistemskih datoteka bez potrebe za ponovnim pokretanjem Vašeg raèunala. U svakom trenutku instalaciju možete prekinuti pritiskom na 'Odustani'.$\r$\n$\r$\n$_CLICK"
!endif

!ifdef MUI_UNWELCOMEPAGE
  ${LangFileString} MUI_UNTEXT_WELCOME_INFO_TITLE "Dobrodošli u postupak uklanjanja programa $(^NameDA)"
  ${LangFileString} MUI_UNTEXT_WELCOME_INFO_TEXT "Ovaj æe Vas èarobnjak provesti kroz postupak uklanjanja programa $(^NameDA).$\r$\n$\r$\nPrije samog poèetka, molim zatvorite program $(^NameDA) ukoliko je sluèajno otvoren.$\r$\n$\r$\n$_CLICK"
!endif

!ifdef MUI_LICENSEPAGE
  ${LangFileString} MUI_TEXT_LICENSE_TITLE "Licenèni ugovor"
  ${LangFileString} MUI_TEXT_LICENSE_SUBTITLE "Molim proèitajte licencu prije instalacije programa $(^NameDA)."
  ${LangFileString} MUI_INNERTEXT_LICENSE_BOTTOM "Ukoliko prihvaæate uvjete licence, odaberite 'Prihvaæam' za nastavak. Morate prihvatiti licencu za instalaciju programa $(^NameDA)."
  ${LangFileString} MUI_INNERTEXT_LICENSE_BOTTOM_CHECKBOX "Ukoliko prihvaæate uvjete licence, oznaèite donji kvadratiæ. Morate prihvatiti licencu za instalaciju programa $(^NameDA). $_CLICK"
  ${LangFileString} MUI_INNERTEXT_LICENSE_BOTTOM_RADIOBUTTONS "Ukoliko prihvaæate uvjete licence, odaberite prvu donju opciju. Morate prihvatiti licencu za instalaciju programa $(^NameDA). $_CLICK"
!endif

!ifdef MUI_UNLICENSEPAGE
  ${LangFileString} MUI_UNTEXT_LICENSE_TITLE "Licenèni ugovor"
  ${LangFileString} MUI_UNTEXT_LICENSE_SUBTITLE "Molim proèitajte licencu prije uklanjanja programa $(^NameDA)."
  ${LangFileString} MUI_UNINNERTEXT_LICENSE_BOTTOM "Ukoliko prihvaæate uvjete licence, odaberite 'Prihvaæam' za nastavak. Morate prihvatiti licencu za uklanjanje programa $(^NameDA)."
  ${LangFileString} MUI_UNINNERTEXT_LICENSE_BOTTOM_CHECKBOX "Ukoliko prihvaæate uvjete licence, oznaèite donji kvadratiæ. Morate prihvatiti licencu za uklanjanje programa $(^NameDA). $_CLICK"
  ${LangFileString} MUI_UNINNERTEXT_LICENSE_BOTTOM_RADIOBUTTONS "Ukoliko prihvaæate uvjete licence, odaberite prvu donju opciju. Morate prihvatiti licencu za uklanjanje programa $(^NameDA). $_CLICK"
!endif

!ifdef MUI_LICENSEPAGE | MUI_UNLICENSEPAGE
  ${LangFileString} MUI_INNERTEXT_LICENSE_TOP "'Page Down' za ostatak licence."
!endif

!ifdef MUI_COMPONENTSPAGE
  ${LangFileString} MUI_TEXT_COMPONENTS_TITLE "Izbor komponenti"
  ${LangFileString} MUI_TEXT_COMPONENTS_SUBTITLE "Odaberite komponente programa $(^NameDA) koje želite instalirati."
!endif

!ifdef MUI_UNCOMPONENTSPAGE
  ${LangFileString} MUI_UNTEXT_COMPONENTS_TITLE "Izbor komponenti"
  ${LangFileString} MUI_UNTEXT_COMPONENTS_SUBTITLE "Odaberite koje komponente programa $(^NameDA) želite ukloniti."
!endif

!ifdef MUI_COMPONENTSPAGE | MUI_UNCOMPONENTSPAGE
  ${LangFileString} MUI_INNERTEXT_COMPONENTS_DESCRIPTION_TITLE "Opis"
  !ifndef NSIS_CONFIG_COMPONENTPAGE_ALTERNATIVE
    ${LangFileString} MUI_INNERTEXT_COMPONENTS_DESCRIPTION_INFO "Postavite pokazivaè iznad komponente za njezin opis."
  !else
    ${LangFileString} MUI_INNERTEXT_COMPONENTS_DESCRIPTION_INFO "Postavite pokazivaè iznad komponente za njezin opis."
  !endif
!endif

!ifdef MUI_DIRECTORYPAGE
  ${LangFileString} MUI_TEXT_DIRECTORY_TITLE "Odaberite odredište za instalaciju"
  ${LangFileString} MUI_TEXT_DIRECTORY_SUBTITLE "Odaberite mapu u koju želite instalirati program $(^NameDA)."
!endif

!ifdef MUI_UNDIRECTORYPAGE
  ${LangFileString} MUI_UNTEXT_DIRECTORY_TITLE "Odaberite polazište za uklanjanje"
  ${LangFileString} MUI_UNTEXT_DIRECTORY_SUBTITLE "Odaberite mapu iz koje želite ukloniti program $(^NameDA)."
!endif

!ifdef MUI_INSTFILESPAGE
  ${LangFileString} MUI_TEXT_INSTALLING_TITLE "Instaliranje"
  ${LangFileString} MUI_TEXT_INSTALLING_SUBTITLE "Molim prièekajte na završetak instalacije programa $(^NameDA)."
  ${LangFileString} MUI_TEXT_FINISH_TITLE "Kraj instalacije"
  ${LangFileString} MUI_TEXT_FINISH_SUBTITLE "Instalacija je u potpunosti završila uspješno."
  ${LangFileString} MUI_TEXT_ABORT_TITLE "Instalacija je prekinuta"
  ${LangFileString} MUI_TEXT_ABORT_SUBTITLE "Instalacija nije završila uspješno."
!endif

!ifdef MUI_UNINSTFILESPAGE
  ${LangFileString} MUI_UNTEXT_UNINSTALLING_TITLE "Uklanjanje"
  ${LangFileString} MUI_UNTEXT_UNINSTALLING_SUBTITLE "Molim prièekajte na završetak uklanjanja programa $(^NameDA)."
  ${LangFileString} MUI_UNTEXT_FINISH_TITLE "Uklanjanje završeno"
  ${LangFileString} MUI_UNTEXT_FINISH_SUBTITLE "Uklanjanje je u potpunosti završilo uspješno."
  ${LangFileString} MUI_UNTEXT_ABORT_TITLE "Uklanjanje je prekinuto"
  ${LangFileString} MUI_UNTEXT_ABORT_SUBTITLE "Uklanjanje nije završilo uspješno."
!endif

!ifdef MUI_FINISHPAGE
  ${LangFileString} MUI_TEXT_FINISH_INFO_TITLE "Dovršenje instalacije programa $(^NameDA)"
  ${LangFileString} MUI_TEXT_FINISH_INFO_TEXT "Program $(^NameDA) je instaliran na Vaše raèunalo.$\r$\n$\r$\nOdaberite 'Kraj' za završetak."
  ${LangFileString} MUI_TEXT_FINISH_INFO_REBOOT "Vaše raèunalo treba ponovno pokrenuti za dovršenje instalacije programa $(^NameDA). Želite li to uèiniti sada?"
!endif

!ifdef MUI_UNFINISHPAGE
  ${LangFileString} MUI_UNTEXT_FINISH_INFO_TITLE "Završetak uklanjanja programa $(^NameDA)"
  ${LangFileString} MUI_UNTEXT_FINISH_INFO_TEXT "Program $(^NameDA) je uklonjen s Vašeg raèunala.$\r$\n$\r$\nOdaberite 'Kraj' za zatvaranje ovog èarobnjaka."
  ${LangFileString} MUI_UNTEXT_FINISH_INFO_REBOOT "Vaše raèunalo treba ponovno pokrenuti za dovršenje postupka uklanjanja programa $(^NameDA). Želite li to uèiniti sada?"
!endif

!ifdef MUI_FINISHPAGE | MUI_UNFINISHPAGE
  ${LangFileString} MUI_TEXT_FINISH_REBOOTNOW "Ponovno pokreni raèunalo sada"
  ${LangFileString} MUI_TEXT_FINISH_REBOOTLATER "Ponovno æu pokrenuti raèunalo kasnije"
  ${LangFileString} MUI_TEXT_FINISH_RUN "&Pokreni program $(^NameDA)"
  ${LangFileString} MUI_TEXT_FINISH_SHOWREADME "Prikaži &Readme"
  ${LangFileString} MUI_BUTTONTEXT_FINISH "&Kraj"  
!endif

!ifdef MUI_STARTMENUPAGE
  ${LangFileString} MUI_TEXT_STARTMENU_TITLE "Izbor mape u Start meniju"
  ${LangFileString} MUI_TEXT_STARTMENU_SUBTITLE "Odaberite ime za programsku mapu unutar Start menija."
  ${LangFileString} MUI_INNERTEXT_STARTMENU_TOP "Program æe pripadati odabranoj programskoj mapi u Start meniju. Možete odrediti novo ime za mapu ili odabrati veæ postojeæu."
  ${LangFileString} MUI_INNERTEXT_STARTMENU_CHECKBOX "Nemoj napraviti preèace"
!endif

!ifdef MUI_UNCONFIRMPAGE
  ${LangFileString} MUI_UNTEXT_CONFIRM_TITLE "Uklanjanje programa $(^NameDA)"
  ${LangFileString} MUI_UNTEXT_CONFIRM_SUBTITLE "Program $(^NameDA) æe biti uklonjen s Vašeg raèunala."
!endif

!ifdef MUI_ABORTWARNING
  ${LangFileString} MUI_TEXT_ABORTWARNING "Jeste li sigurni da želite prekinuti instalaciju programa $(^Name)?"
!endif

!ifdef MUI_UNABORTWARNING
  ${LangFileString} MUI_UNTEXT_ABORTWARNING "Jeste li sigurni da želite prekinuti uklanjanje programa $(^Name)?"
!endif
