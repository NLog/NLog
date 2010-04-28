;Language: Slovak (1051)
;Translated by:
;  Kypec (peter.dzugas@mahe.sk)
;edited by:
;  Marián Hikaník (podnety@mojepreklady.net)
;  Ivan Masár <helix84@centrum.sk>, 2008.

!insertmacro LANGFILE "Slovak" "Slovensky"

!ifdef MUI_WELCOMEPAGE
  ${LangFileString} MUI_TEXT_WELCOME_INFO_TITLE "Vitajte v sprievodcovi inštaláciou programu $(^NameDA)"
  ${LangFileString} MUI_TEXT_WELCOME_INFO_TEXT "Tento sprievodca vás prevedie inštaláciou $(^NameDA).$\r$\n$\r$\nPred zaèiatkom inštalácie sa odporúèa ukonèi všetky ostatné programy. Tım umoníte aktualizovanie systémovıch súborov bez potreby reštartovania vášho poèítaèa.$\r$\n$\r$\n$_CLICK"
!endif

!ifdef MUI_UNWELCOMEPAGE
  ${LangFileString} MUI_UNTEXT_WELCOME_INFO_TITLE "Vitajte v sprievodcovi odinštalovaním programu $(^NameDA)"
  ${LangFileString} MUI_UNTEXT_WELCOME_INFO_TEXT "Tento sprievodca vás prevedie procesom odinštalovania programu $(^NameDA).$\r$\n$\r$\nPred spustením procesu odinštalovania sa uistite, e program $(^NameDA) nie je práve aktívny.$\r$\n$\r$\n$_CLICK"
!endif

!ifdef MUI_LICENSEPAGE
  ${LangFileString} MUI_TEXT_LICENSE_TITLE "Licenèná zmluva"
  ${LangFileString} MUI_TEXT_LICENSE_SUBTITLE "Pred inštaláciou $(^NameDA) si prosím preštudujte licenèné podmienky."
  ${LangFileString} MUI_INNERTEXT_LICENSE_BOTTOM "Ak súhlasíte s podmienkami zmluvy, kliknite na tlaèidlo Súhlasím a môete pokraèova v inštalácii. Ak chcete v inštalácii pokraèova, musíte odsúhlasi podmienky licenènej zmluvy $(^NameDA)."
  ${LangFileString} MUI_INNERTEXT_LICENSE_BOTTOM_CHECKBOX "Ak súhlasíte s podmienkami zmluvy, zaškrtnite nišie uvedené políèko. Ak chcete v inštalácii pokraèova, musíte odsúhlasi podmienky licenènej zmluvy $(^NameDA)."
  ${LangFileString} MUI_INNERTEXT_LICENSE_BOTTOM_RADIOBUTTONS "Ak súhlasíte s podmienkami zmluvy, oznaète prvú z nišie uvedenıch moností. Ak chcete v inštalácii pokraèova, musíte odsúhlasi podmienky licenènej zmluvy $(^NameDA)."
!endif

!ifdef MUI_UNLICENSEPAGE
  ${LangFileString} MUI_UNTEXT_LICENSE_TITLE "Licenèná zmluva"
  ${LangFileString} MUI_UNTEXT_LICENSE_SUBTITLE "Pred odinštalovaním programu $(^NameDA) si prosím preèítajte licenèné podmienky."
  ${LangFileString} MUI_UNINNERTEXT_LICENSE_BOTTOM "Ak súhlasíte s podmienkami zmluvy, zvo¾te Súhlasím. Licenènú zmluvu musíte odsúhlasi, ak chcete v odinštalovaní programu $(^NameDA) pokraèova."
  ${LangFileString} MUI_UNINNERTEXT_LICENSE_BOTTOM_CHECKBOX "Ak súhlasíte s podmienkami zmluvy, zaškrtnite nišie uvedené políèko. Licenènú zmluvu musíte odsúhlasi, ak chcete pokraèova v odinštalovaní programu $(^NameDA). $_CLICK"
  ${LangFileString} MUI_UNINNERTEXT_LICENSE_BOTTOM_RADIOBUTTONS "Ak súhlasíte s podmienkami licenènej zmluvy, oznaète prvú z nišie uvedenıch moností. Licenènú zmluvu musíte odsúhlasi, ak chcete pokraèova v odinštalovaní programu $(^NameDA). $_CLICK"
!endif

!ifdef MUI_LICENSEPAGE | MUI_UNLICENSEPAGE
  ${LangFileString} MUI_INNERTEXT_LICENSE_TOP "Stlaèením klávesu Page Down posuniete text licenènej zmluvy."
!endif

!ifdef MUI_COMPONENTSPAGE
  ${LangFileString} MUI_TEXT_COMPONENTS_TITLE "Vo¾ba súèastí programu"
  ${LangFileString} MUI_TEXT_COMPONENTS_SUBTITLE "Zvo¾te si tie súèasti programu $(^NameDA), ktoré chcete nainštalova."
!endif

!ifdef MUI_UNCOMPONENTSPAGE
  ${LangFileString} MUI_UNTEXT_COMPONENTS_TITLE "Vo¾ba súèastí"
  ${LangFileString} MUI_UNTEXT_COMPONENTS_SUBTITLE "Zvo¾te súèasti programu $(^NameDA), ktoré chcete odinštalova."
!endif

!ifdef MUI_COMPONENTSPAGE | MUI_UNCOMPONENTSPAGE
  ${LangFileString} MUI_INNERTEXT_COMPONENTS_DESCRIPTION_TITLE "Popis"
  !ifndef NSIS_CONFIG_COMPONENTPAGE_ALTERNATIVE
    ${LangFileString} MUI_INNERTEXT_COMPONENTS_DESCRIPTION_INFO "Pri prejdení kurzorom myši nad názvom súèasti sa zobrazí jej popis."
  !else
    ${LangFileString} MUI_INNERTEXT_COMPONENTS_DESCRIPTION_INFO "Pri prejdení kurzorom myši nad názvom súèasti sa zobrazí jej popis."
  !endif
!endif

!ifdef MUI_DIRECTORYPAGE
  ${LangFileString} MUI_TEXT_DIRECTORY_TITLE "Vo¾ba umiestnenia programu"
  ${LangFileString} MUI_TEXT_DIRECTORY_SUBTITLE "Vyberte si prieèinok, do ktorého chcete nainštalova program $(^NameDA)."
!endif

!ifdef MUI_UNDIRECTORYPAGE
  ${LangFileString} MUI_UNTEXT_DIRECTORY_TITLE "Umiestenie programu pre odinštalovanie"
  ${LangFileString} MUI_UNTEXT_DIRECTORY_SUBTITLE "Vyberte si prieèinok, z ktorého chcete odinštalova program $(^NameDA)."
!endif

!ifdef MUI_INSTFILESPAGE
  ${LangFileString} MUI_TEXT_INSTALLING_TITLE "Inštalácia"
  ${LangFileString} MUI_TEXT_INSTALLING_SUBTITLE "Poèkajte prosím, kım prebehne inštalácia programu $(^NameDA)."
  ${LangFileString} MUI_TEXT_FINISH_TITLE "Ukonèenie inštalácie"
  ${LangFileString} MUI_TEXT_FINISH_SUBTITLE "Inštalácia bola dokonèená úspešne."
  ${LangFileString} MUI_TEXT_ABORT_TITLE "Prerušenie inštalácie"
  ${LangFileString} MUI_TEXT_ABORT_SUBTITLE "Inštaláciu sa nepodarilo dokonèi."
!endif

!ifdef MUI_UNINSTFILESPAGE
  ${LangFileString} MUI_UNTEXT_UNINSTALLING_TITLE "Odinštalovanie"
  ${LangFileString} MUI_UNTEXT_UNINSTALLING_SUBTITLE "Èakajte prosím, kım prebehne odinštalovanie programu $(^NameDA)."
  ${LangFileString} MUI_UNTEXT_FINISH_TITLE "Ukonèenie odinštalovania"
  ${LangFileString} MUI_UNTEXT_FINISH_SUBTITLE "Odinštalovanie bolo úspešne dokonèené."
  ${LangFileString} MUI_UNTEXT_ABORT_TITLE "Prerušenie odinštalovania"
  ${LangFileString} MUI_UNTEXT_ABORT_SUBTITLE "Odinštalovanie sa neukonèilo úspešne."
!endif

!ifdef MUI_FINISHPAGE
  ${LangFileString} MUI_TEXT_FINISH_INFO_TITLE "Dokonèenie inštalácie programu $(^NameDA)"
  ${LangFileString} MUI_TEXT_FINISH_INFO_TEXT "Program $(^NameDA) bol nainštalovanı do vášho poèítaèa.$\r$\nKliknite na tlaèidlo Dokonèi a tento sprievodca sa ukonèí."
  ${LangFileString} MUI_TEXT_FINISH_INFO_REBOOT "Pre úplné dokonèenie inštalácie programu $(^NameDA) je potrebné reštartova váš poèítaè. Chcete ho reštartova ihneï?"
!endif

!ifdef MUI_UNFINISHPAGE
  ${LangFileString} MUI_UNTEXT_FINISH_INFO_TITLE "Dokonèenie sprievodcu odinštalovaním"
  ${LangFileString} MUI_UNTEXT_FINISH_INFO_TEXT "Program $(^NameDA) bol odinštalovanı z vášho poèítaèa.$\r$\n$\r$\nKliknite na tlaèidlo Dokonèi a tento sprievodca sa ukonèí."
  ${LangFileString} MUI_UNTEXT_FINISH_INFO_REBOOT "Pre úplné dokonèenie odinštalovania programu $(^NameDA) je nutné reštartova váš poèítaè. Chcete ho reštartova ihneï?"
!endif

!ifdef MUI_FINISHPAGE | MUI_UNFINISHPAGE
  ${LangFileString} MUI_TEXT_FINISH_REBOOTNOW "Reštartova teraz"
  ${LangFileString} MUI_TEXT_FINISH_REBOOTLATER "Reštartova neskôr (manuálne)"
  ${LangFileString} MUI_TEXT_FINISH_RUN "&Spusti program $(^NameDA)"
  ${LangFileString} MUI_TEXT_FINISH_SHOWREADME "&Zobrazi súbor s informáciami"
  ${LangFileString} MUI_BUTTONTEXT_FINISH "&Dokonèi"  
!endif

!ifdef MUI_STARTMENUPAGE
  ${LangFileString} MUI_TEXT_STARTMENU_TITLE "Vo¾ba umiestnenia v ponuke Štart"
  ${LangFileString} MUI_TEXT_STARTMENU_SUBTITLE "Vyberte si prieèinok v ponuke Štart, kam sa umiestnia odkazy na program $(^NameDA)."
  ${LangFileString} MUI_INNERTEXT_STARTMENU_TOP "Vyberte si prieèinok v ponuke Štart, v ktorom chcete vytvori odkazy na program. Takisto môete napísa názov nového prieèinka."
  ${LangFileString} MUI_INNERTEXT_STARTMENU_CHECKBOX "Nevytvára odkazy"
!endif

!ifdef MUI_UNCONFIRMPAGE
  ${LangFileString} MUI_UNTEXT_CONFIRM_TITLE "Odinštalovanie programu $(^NameDA)"
  ${LangFileString} MUI_UNTEXT_CONFIRM_SUBTITLE "Odstránenie programu $(^NameDA) z vášho poèítaèa."
!endif

!ifdef MUI_ABORTWARNING
  ${LangFileString} MUI_TEXT_ABORTWARNING "Naozaj chcete ukonèi inštaláciu programu $(^Name)?"
!endif

!ifdef MUI_UNABORTWARNING
  ${LangFileString} MUI_UNTEXT_ABORTWARNING "Naozaj chcete ukonèi proces odinštalovania programu $(^Name)?"
!endif

!ifdef MULTIUSER_INSTALLMODEPAGE
  ${LangFileString} MULTIUSER_TEXT_INSTALLMODE_TITLE "Vybra pouívate¾ov"
  ${LangFileString} MULTIUSER_TEXT_INSTALLMODE_SUBTITLE "Vyberte pre ktorıch pouívate¾ov chcete nainštalova $(^NameDA)."
  ${LangFileString} MULTIUSER_INNERTEXT_INSTALLMODE_TOP "Vyberte, èi chcete nainštalova program $(^NameDA) iba pre seba alebo pre všetkıch pouívate¾ov tohto poèítaèa. $(^ClickNext)"
  ${LangFileString} MULTIUSER_INNERTEXT_INSTALLMODE_ALLUSERS "Nainštalova pre všetkıch pouívate¾ov tohto poèítaèa"
  ${LangFileString} MULTIUSER_INNERTEXT_INSTALLMODE_CURRENTUSER "Nainštalova iba pre mòa"
!endif