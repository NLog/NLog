;Language: Estonian (1061)
;Translated by johnny izzo (izzo@hot.ee)

!insertmacro LANGFILE "Estonian" "Eesti keel"

!ifdef MUI_WELCOMEPAGE
  ${LangFileString} MUI_TEXT_WELCOME_INFO_TITLE "$(^NameDA) paigaldamine!"
  ${LangFileString} MUI_TEXT_WELCOME_INFO_TEXT "See abiline aitab paigaldada $(^NameDA).$\r$\n$\r$\nEnne paigaldamise alustamist on soovitatav kõik teised programmid sulgeda, see võimaldab teatud süsteemifaile uuendada ilma arvutit taaskäivitamata.$\r$\n$\r$\n$_CLICK"
!endif

!ifdef MUI_UNWELCOMEPAGE
  ${LangFileString} MUI_UNTEXT_WELCOME_INFO_TITLE "$(^NameDA) eemaldamine!"
  ${LangFileString} MUI_UNTEXT_WELCOME_INFO_TEXT "See abiline aitab eemaldada $(^NameDA).$\r$\n$\r$\nEnne eemaldamist vaata, et $(^NameDA) oleks suletud.$\r$\n$\r$\n$_CLICK"
!endif

!ifdef MUI_LICENSEPAGE
  ${LangFileString} MUI_TEXT_LICENSE_TITLE "Litsentsileping"
  ${LangFileString} MUI_TEXT_LICENSE_SUBTITLE "Enne $(^NameDA) paigaldamist vaata palun litsentsileping üle."
  ${LangFileString} MUI_INNERTEXT_LICENSE_BOTTOM "Kui sa oled lepingu tingimustega nõus, vali jätkamiseks Nõustun. $(^NameDA) paigaldamiseks pead sa lepinguga nõustuma."
  ${LangFileString} MUI_INNERTEXT_LICENSE_BOTTOM_CHECKBOX "Kui nõustud lepingu tingimustega, vali allolev märkeruut. $(^NameDA) paigaldamiseks pead lepinguga nõustuma. $_CLICK"
  ${LangFileString} MUI_INNERTEXT_LICENSE_BOTTOM_RADIOBUTTONS "Kui nõustud lepingu tingimustega, märgi allpool esimene valik. $(^NameDA) paigaldamiseks pead lepinguga nõustuma. $_CLICK"
!endif

!ifdef MUI_UNLICENSEPAGE
  ${LangFileString} MUI_UNTEXT_LICENSE_TITLE "Litsentsileping"
  ${LangFileString} MUI_UNTEXT_LICENSE_SUBTITLE "Enne $(^NameDA) eemaldamist vaata palun litsentsileping üle."
  ${LangFileString} MUI_UNINNERTEXT_LICENSE_BOTTOM "Kui sa oled lepingu tingimustega nõus, vali jätkamiseks Nõustun. $(^NameDA) eemaldamiseks pead sa lepinguga nõustuma."
  ${LangFileString} MUI_UNINNERTEXT_LICENSE_BOTTOM_CHECKBOX "Kui nõustud lepingu tingimustega, vali allolev märkeruut. $(^NameDA) eemaldamiseks pead lepinguga nõustuma. $_CLICK"
  ${LangFileString} MUI_UNINNERTEXT_LICENSE_BOTTOM_RADIOBUTTONS "Kui nõustud lepingu tingimustega, märgi allpool esimene valik. $(^NameDA) eemaldamiseks pead lepinguga nõustuma. $_CLICK"
!endif

!ifdef MUI_LICENSEPAGE | MUI_UNLICENSEPAGE
  ${LangFileString} MUI_INNERTEXT_LICENSE_TOP "Vajuta Page Down, et näha ülejäänud teksti."
!endif

!ifdef MUI_COMPONENTSPAGE
  ${LangFileString} MUI_TEXT_COMPONENTS_TITLE "Vali komponendid"
  ${LangFileString} MUI_TEXT_COMPONENTS_SUBTITLE "Vali millised $(^NameDA) osad sa soovid paigaldada."
!endif

!ifdef MUI_UNCOMPONENTSPAGE
  ${LangFileString} MUI_UNTEXT_COMPONENTS_TITLE "Vali komponendid"
  ${LangFileString} MUI_UNTEXT_COMPONENTS_SUBTITLE "Vali millised $(^NameDA) osad sa soovid eemaldada."
!endif

!ifdef MUI_COMPONENTSPAGE | MUI_UNCOMPONENTSPAGE
  ${LangFileString} MUI_INNERTEXT_COMPONENTS_DESCRIPTION_TITLE "Kirjeldus"
  !ifndef NSIS_CONFIG_COMPONENTPAGE_ALTERNATIVE
    ${LangFileString} MUI_INNERTEXT_COMPONENTS_DESCRIPTION_INFO "Nihuta hiir komponendile, et näha selle kirjeldust."
  !else
    ${LangFileString} MUI_INNERTEXT_COMPONENTS_DESCRIPTION_INFO "Nihuta hiir komponendile, et näha selle kirjeldust."
  !endif
!endif

!ifdef MUI_DIRECTORYPAGE
  ${LangFileString} MUI_TEXT_DIRECTORY_TITLE "Vali asukoht"
  ${LangFileString} MUI_TEXT_DIRECTORY_SUBTITLE "Vali kaust kuhu paigaldada $(^NameDA)."
!endif

!ifdef MUI_UNDIRECTORYPAGE
  ${LangFileString} MUI_UNTEXT_DIRECTORY_TITLE "Vali asukoht"
  ${LangFileString} MUI_UNTEXT_DIRECTORY_SUBTITLE "Vali kaust kust $(^NameDA) eemaldada."
!endif

!ifdef MUI_INSTFILESPAGE
  ${LangFileString} MUI_TEXT_INSTALLING_TITLE "Paigaldan..."
  ${LangFileString} MUI_TEXT_INSTALLING_SUBTITLE "Palun oota kuni $(^NameDA) on paigaldatud."
  ${LangFileString} MUI_TEXT_FINISH_TITLE "Programm paigaldatud"
  ${LangFileString} MUI_TEXT_FINISH_SUBTITLE "Paigaldus edukalt sooritatud."
  ${LangFileString} MUI_TEXT_ABORT_TITLE "Paigaldus katkestatud"
  ${LangFileString} MUI_TEXT_ABORT_SUBTITLE "Paigaldamine ebaõnnestus."
!endif

!ifdef MUI_UNINSTFILESPAGE
  ${LangFileString} MUI_UNTEXT_UNINSTALLING_TITLE "Eemaldan..."
  ${LangFileString} MUI_UNTEXT_UNINSTALLING_SUBTITLE "Palun oota kuni $(^NameDA) on eemaldatud."
  ${LangFileString} MUI_UNTEXT_FINISH_TITLE "Eemaldamine lõpetatud"
  ${LangFileString} MUI_UNTEXT_FINISH_SUBTITLE "Eemaldamine edukalt lõpule viidud."
  ${LangFileString} MUI_UNTEXT_ABORT_TITLE "Eemaldamine katkestatud"
  ${LangFileString} MUI_UNTEXT_ABORT_SUBTITLE "Eemaldamine ebaõnestus."
!endif

!ifdef MUI_FINISHPAGE
  ${LangFileString} MUI_TEXT_FINISH_INFO_TITLE "$(^NameDA) paigalduse lõpule viimine."
  ${LangFileString} MUI_TEXT_FINISH_INFO_TEXT "$(^NameDA) on sinu arvutisse paigaldatud.$\r$\n$\r$\nAbilise sulgemiseks vajuta Lõpeta."
  ${LangFileString} MUI_TEXT_FINISH_INFO_REBOOT "$(^NameDA) täielikuks paigaldamiseks tuleb arvuti taaskäivitada. Kas soovid arvuti kohe taaskäivitada ?"
!endif

!ifdef MUI_UNFINISHPAGE
  ${LangFileString} MUI_UNTEXT_FINISH_INFO_TITLE "$(^NameDA) eemaldamise lõpule viimine."
  ${LangFileString} MUI_UNTEXT_FINISH_INFO_TEXT "$(^NameDA) on sinu arvutist eemaldatud.$\r$\n$\r$\nAbilise sulgemiseks vajuta Lõpeta."
  ${LangFileString} MUI_UNTEXT_FINISH_INFO_REBOOT "$(^NameDA) täielikuks eemaldamiseks tuleb arvuti taaskäivitada. Kas soovid arvuti kohe taaskäivitada ?"
!endif

!ifdef MUI_FINISHPAGE | MUI_UNFINISHPAGE
  ${LangFileString} MUI_TEXT_FINISH_REBOOTNOW "Taaskäivita kohe"
  ${LangFileString} MUI_TEXT_FINISH_REBOOTLATER "Taaskäivitan hiljem käsitsi"
  ${LangFileString} MUI_TEXT_FINISH_RUN "Käivita $(^NameDA)"
  ${LangFileString} MUI_TEXT_FINISH_SHOWREADME "Kuva Loemind"
  ${LangFileString} MUI_BUTTONTEXT_FINISH "Lõpeta"  
!endif

!ifdef MUI_STARTMENUPAGE
  ${LangFileString} MUI_TEXT_STARTMENU_TITLE "Vali Start-menüü kaust"
  ${LangFileString} MUI_TEXT_STARTMENU_SUBTITLE "Vali $(^NameDA) otseteede jaoks Start-menüü kaust."
  ${LangFileString} MUI_INNERTEXT_STARTMENU_TOP "Vali Start-menüü kaust, kuhu soovid paigutada programmi otseteed. Võid ka sisestada nime, et luua uus kaust."
  ${LangFileString} MUI_INNERTEXT_STARTMENU_CHECKBOX "Ära loo otseteid"
!endif

!ifdef MUI_UNCONFIRMPAGE
  ${LangFileString} MUI_UNTEXT_CONFIRM_TITLE "Eemalda $(^NameDA)"
  ${LangFileString} MUI_UNTEXT_CONFIRM_SUBTITLE "Eemalda $(^NameDA) oma arvutist."
!endif

!ifdef MUI_ABORTWARNING
  ${LangFileString} MUI_TEXT_ABORTWARNING "Oled sa kindel et soovid $(^Name) paigaldamise katkestada?"
!endif

!ifdef MUI_UNABORTWARNING
  ${LangFileString} MUI_UNTEXT_ABORTWARNING "Oled sa kindel et soovid $(^Name) eemaldamise katkestada?"
!endif
