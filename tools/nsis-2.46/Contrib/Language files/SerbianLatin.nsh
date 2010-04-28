;Language: Serbian Latin (2074)
;Translation by Srðan Obuæina <obucina@srpskijezik.edu.yu>

!insertmacro LANGFILE "SerbianLatin" "Serbian Latin"

!ifdef MUI_WELCOMEPAGE
  ${LangFileString} MUI_TEXT_WELCOME_INFO_TITLE "Dobrodošli u vodiè za instalaciju programa $(^NameDA)"
  ${LangFileString} MUI_TEXT_WELCOME_INFO_TEXT "Biæete voðeni kroz proces instalacije programa $(^NameDA).$\r$\n$\r$\nPreporuèljivo je da iskljuèite sve druge programe pre poèetka instalacije. Ovo može omoguæiti ažuriranje sistemskih fajlova bez potrebe za ponovnim pokretanjem raèunara.$\r$\n$\r$\n$_CLICK"
!endif

!ifdef MUI_UNWELCOMEPAGE
  ${LangFileString} MUI_UNTEXT_WELCOME_INFO_TITLE "Dobrodošli u deinstalaciju programa $(^NameDA)"
  ${LangFileString} MUI_UNTEXT_WELCOME_INFO_TEXT "Biæete voðeni kroz proces deinstalacije programa $(^NameDA).$\r$\n$\r$\nPre poèetka deinstalacije, uverite se da je program $(^NameDA) iskljuèen. $_CLICK"
!endif

!ifdef MUI_LICENSEPAGE
  ${LangFileString} MUI_TEXT_LICENSE_TITLE "Dogovor o pravu korišæenja"
  ${LangFileString} MUI_TEXT_LICENSE_SUBTITLE "Pažljivo proèitajte dogovor o pravu korišæenja pre instalacije programa $(^NameDA)."
  ${LangFileString} MUI_INNERTEXT_LICENSE_BOTTOM "Ako prihvatate sve uslove dogovora, pritisnite dugme „Prihvatam“ za nastavak. Morate prihvatiti dogovor da biste instalirali program $(^NameDA)."
  ${LangFileString} MUI_INNERTEXT_LICENSE_BOTTOM_CHECKBOX "Ako prihvatate sve uslove dogovora, obeležite kvadratiæ ispod. Morate prihvatiti dogovor da biste instalirali program $(^NameDA). $_CLICK"
  ${LangFileString} MUI_INNERTEXT_LICENSE_BOTTOM_RADIOBUTTONS "Ako prihvatate sve uslove dogovora, izaberite prvu opciju ispod. Morate prihvatiti dogovor da biste instalirali program $(^NameDA). $_CLICK"
!endif

!ifdef MUI_UNLICENSEPAGE
  ${LangFileString} MUI_UNTEXT_LICENSE_TITLE "Dogovor o pravu korišæenja"
  ${LangFileString} MUI_UNTEXT_LICENSE_SUBTITLE "Pažlivo proèitajte dogovor o pravu korišæenja pre deinstalacije programa $(^NameDA)."
  ${LangFileString} MUI_UNINNERTEXT_LICENSE_BOTTOM "Ako prihvatate sve uslove dogovora, pritisnite dugme „Prihvatam“ za nastavak. Morate prihvatiti dogovor da biste deinstalirali program $(^NameDA)."
  ${LangFileString} MUI_UNINNERTEXT_LICENSE_BOTTOM_CHECKBOX "Ako prihvatate sve uslove dogovora, obeležite kvadratiæ ispod. Morate prihvatiti dogovor da biste deinstalirali program $(^NameDA). $_CLICK"
  ${LangFileString} MUI_UNINNERTEXT_LICENSE_BOTTOM_RADIOBUTTONS "Ako prihvatate sve uslove dogovora, izaberite prvu opciju ispod. Morate prihvatiti dogovor da biste deinstalirali program $(^NameDA). $_CLICK"
!endif

!ifdef MUI_LICENSEPAGE | MUI_UNLICENSEPAGE
  ${LangFileString} MUI_INNERTEXT_LICENSE_TOP "Pritisnite Page Down da biste videli ostatak dogovora."
!endif

!ifdef MUI_COMPONENTSPAGE
  ${LangFileString} MUI_TEXT_COMPONENTS_TITLE "Izbor komponenti za instalaciju"
  ${LangFileString} MUI_TEXT_COMPONENTS_SUBTITLE "Izaberite komponente za instalaciju. Instaliraju se samo oznaèene komponente."
!endif

!ifdef MUI_UNCOMPONENTSPAGE
  ${LangFileString} MUI_UNTEXT_COMPONENTS_TITLE "Izbor komponenti za deinstalaciju"
  ${LangFileString} MUI_UNTEXT_COMPONENTS_SUBTITLE "Izaberite komponente za deinstalaciju. Deinstaliraju se samo oznaèene komponente."
!endif

!ifdef MUI_COMPONENTSPAGE | MUI_UNCOMPONENTSPAGE
  ${LangFileString} MUI_INNERTEXT_COMPONENTS_DESCRIPTION_TITLE "Opis"
  !ifndef NSIS_CONFIG_COMPONENTPAGE_ALTERNATIVE
    ${LangFileString} MUI_INNERTEXT_COMPONENTS_DESCRIPTION_INFO "Preðite kursorom miša preko imena komponente da biste videli njen opis."
  !else
    ${LangFileString} MUI_INNERTEXT_COMPONENTS_DESCRIPTION_INFO "Preðite kursorom miša preko imena komponente da biste videli njen opis."
  !endif
!endif

!ifdef MUI_DIRECTORYPAGE
  ${LangFileString} MUI_TEXT_DIRECTORY_TITLE "Izbor foldera za instalaciju"
  ${LangFileString} MUI_TEXT_DIRECTORY_SUBTITLE "Izaberite folder u koji æete instalirati program $(^NameDA)."
!endif

!ifdef MUI_UNDIRECTORYPAGE
  ${LangFileString} MUI_UNTEXT_DIRECTORY_TITLE "Izbor foldera za deinstalaciju"
  ${LangFileString} MUI_UNTEXT_DIRECTORY_SUBTITLE "Izaberite folder iz koga æete deinstalirati program $(^NameDA)."
!endif

!ifdef MUI_INSTFILESPAGE
  ${LangFileString} MUI_TEXT_INSTALLING_TITLE "Instalacija"
  ${LangFileString} MUI_TEXT_INSTALLING_SUBTITLE "Saèekajte dok se program $(^NameDA) instalira."
  ${LangFileString} MUI_TEXT_FINISH_TITLE "Završena instalacija"
  ${LangFileString} MUI_TEXT_FINISH_SUBTITLE "Instalacija je uspešno završena."
  ${LangFileString} MUI_TEXT_ABORT_TITLE "Prekinuta instalacija"
  ${LangFileString} MUI_TEXT_ABORT_SUBTITLE "Instalacija je prekinuta i nije uspešno završena."
!endif

!ifdef MUI_UNINSTFILESPAGE
  ${LangFileString} MUI_UNTEXT_UNINSTALLING_TITLE "Deinstalacija"
  ${LangFileString} MUI_UNTEXT_UNINSTALLING_SUBTITLE "Saèekajte dok se program $(^NameDA) deinstalira."
  ${LangFileString} MUI_UNTEXT_FINISH_TITLE "Završena deinstalacija"
  ${LangFileString} MUI_UNTEXT_FINISH_SUBTITLE "Deinstalacija je uspešno završena."
  ${LangFileString} MUI_UNTEXT_ABORT_TITLE "Prekinuta deinstalacija"
  ${LangFileString} MUI_UNTEXT_ABORT_SUBTITLE "Deinstalacija je prekinuta i nije uspešno završena."
!endif

!ifdef MUI_FINISHPAGE
  ${LangFileString} MUI_TEXT_FINISH_INFO_TITLE "Završena instalacija programa $(^NameDA)"
  ${LangFileString} MUI_TEXT_FINISH_INFO_TEXT "Program $(^NameDA) je instaliran na raèunar.$\r$\n$\r$\nPritisnite dugme „Kraj“ za zatvaranje ovog prozora."
  ${LangFileString} MUI_TEXT_FINISH_INFO_REBOOT "Raèunar mora biti ponovo pokrenut da bi se proces instalacije programa $(^NameDA) uspešno završio. Želite li to odmah da uradite?"
!endif

!ifdef MUI_UNFINISHPAGE
  ${LangFileString} MUI_UNTEXT_FINISH_INFO_TITLE "Završena deinstalacija programa $(^NameDA)"
  ${LangFileString} MUI_UNTEXT_FINISH_INFO_TEXT "Program $(^NameDA) je deinstaliran sa raèunara.$\r$\n$\r$\nPritisnite dugme „Kraj“ za zatvaranje ovog prozora."
  ${LangFileString} MUI_UNTEXT_FINISH_INFO_REBOOT "Raèunar mora biti ponovo pokrenut da bi se proces deinstalacije programa $(^NameDA) uspešno završio. Želite li to da uradite odmah?"
!endif

!ifdef MUI_FINISHPAGE | MUI_UNFINISHPAGE
  ${LangFileString} MUI_TEXT_FINISH_REBOOTNOW "Odmah ponovo pokreni raèunar"
  ${LangFileString} MUI_TEXT_FINISH_REBOOTLATER "Bez ponovnog pokretanja"
  ${LangFileString} MUI_TEXT_FINISH_RUN "Pokreni program $(^NameDA)"
  ${LangFileString} MUI_TEXT_FINISH_SHOWREADME "Prikaži ProèitajMe fajl"
  ${LangFileString} MUI_BUTTONTEXT_FINISH "Kraj"  
!endif

!ifdef MUI_STARTMENUPAGE
  ${LangFileString} MUI_TEXT_STARTMENU_TITLE "Izbor foldera u Start meniju"
  ${LangFileString} MUI_TEXT_STARTMENU_SUBTITLE "Izaberite folder u Start meniju u kome æete kreirati preèice."
  ${LangFileString} MUI_INNERTEXT_STARTMENU_TOP "Izaberite folder u Start meniju u kome želite da budu kreirane preèice programa. Možete upisati i ime za kreiranje novog foldera."
  ${LangFileString} MUI_INNERTEXT_STARTMENU_CHECKBOX "Bez kreiranja preèica"
!endif

!ifdef MUI_UNCONFIRMPAGE
  ${LangFileString} MUI_UNTEXT_CONFIRM_TITLE "Deinstalacija programa $(^NameDA)"
  ${LangFileString} MUI_UNTEXT_CONFIRM_SUBTITLE "Deinstalacija programa $(^NameDA) sa raèunara."
!endif

!ifdef MUI_ABORTWARNING
  ${LangFileString} MUI_TEXT_ABORTWARNING "Sigurno želite da prekinete instalaciju programa $(^Name)?"
!endif

!ifdef MUI_UNABORTWARNING
  ${LangFileString} MUI_UNTEXT_ABORTWARNING "Sigurno želite da prekinete deinstalaciju programa $(^Name)?"
!endif
