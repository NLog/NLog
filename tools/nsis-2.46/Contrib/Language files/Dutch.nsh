;Language: Dutch (1043)
;By Joost Verburg

!insertmacro LANGFILE "Dutch" "Nederlands"

!ifdef MUI_WELCOMEPAGE
  ${LangFileString} MUI_TEXT_WELCOME_INFO_TITLE "Welkom bij de $(^NameDA)-installatiewizard"
  ${LangFileString} MUI_TEXT_WELCOME_INFO_TEXT "Deze wizard zal $(^NameDA) op uw systeem installeren.$\r$\n$\r$\nHet wordt aanbevolen alle overige toepassingen af te sluiten alvorens de installatie te starten. Dit maakt het mogelijk relevante systeembestanden bij te werken zonder uw systeem opnieuw op te moeten starten.$\r$\n$\r$\n$_CLICK"
!endif

!ifdef MUI_UNWELCOMEPAGE
  ${LangFileString} MUI_UNTEXT_WELCOME_INFO_TITLE "Welkom bij de $(^NameDA)-deïnstallatiewizard"
  ${LangFileString} MUI_UNTEXT_WELCOME_INFO_TEXT "Deze wizard zal $(^NameDA) van uw syteem verwijderen.$\r$\n$\r$\nControleer voordat u begint met verwijderen of $(^NameDA) is afgesloten.$\r$\n$\r$\n$_CLICK"
!endif

!ifdef MUI_LICENSEPAGE
  ${LangFileString} MUI_TEXT_LICENSE_TITLE "Licentieovereenkomst"
  ${LangFileString} MUI_TEXT_LICENSE_SUBTITLE "Lees de licentieovereenkomst voordat u $(^NameDA) installeert."
  ${LangFileString} MUI_INNERTEXT_LICENSE_BOTTOM "Klik op Akkoord om verder te gaan als u de overeenkomst accepteert. U moet de overeenkomst accepteren om $(^NameDA) te installeren."
  ${LangFileString} MUI_INNERTEXT_LICENSE_BOTTOM_CHECKBOX "Schakel het selectievakje hieronder in als u de overeenkomst accepteert. U moet de overeenkomst accepteren om $(^NameDA) te installeren."
  ${LangFileString} MUI_INNERTEXT_LICENSE_BOTTOM_RADIOBUTTONS "Selecteer de eerste optie hieronder als u de overeenkomst accepteert. U moet de overeenkomst accepteren om $(^NameDA) te installeren."
!endif

!ifdef MUI_UNLICENSEPAGE
  ${LangFileString} MUI_UNTEXT_LICENSE_TITLE "Licentieovereenkomst"
  ${LangFileString} MUI_UNTEXT_LICENSE_SUBTITLE "Lees de licentieovereenkomst voordat u $(^NameDA) verwijdert."
  ${LangFileString} MUI_UNINNERTEXT_LICENSE_BOTTOM "Klik op Akkoord op verder te gaan als u de overeenkomst accepteert. U moet de overeenkomst accepteren om $(^NameDA) te verwijderen."
  ${LangFileString} MUI_UNINNERTEXT_LICENSE_BOTTOM_CHECKBOX "Schakel het selectievakje hieronder in als u de overeenkomst accepteert. U moet de overeenkomst accepteren om $(^NameDA) te verwijderen."
  ${LangFileString} MUI_UNINNERTEXT_LICENSE_BOTTOM_RADIOBUTTONS "Selecteer de eerste optie hieronder als u de overeenkomst accepteert. U moet de overeenkomst accepteren om $(^NameDA) te verwijderen."
!endif

!ifdef MUI_LICENSEPAGE | MUI_UNLICENSEPAGE
  ${LangFileString} MUI_INNERTEXT_LICENSE_TOP "Druk op Page Down om de rest van de overeenkomst te zien."
!endif

!ifdef MUI_COMPONENTSPAGE
  ${LangFileString} MUI_TEXT_COMPONENTS_TITLE "Onderdelen kiezen"
  ${LangFileString} MUI_TEXT_COMPONENTS_SUBTITLE "Kies de onderdelen die u wilt installeren."
!endif

!ifdef MUI_UNCOMPONENTSPAGE
  ${LangFileString} MUI_UNTEXT_COMPONENTS_TITLE "Onderdelen kiezen"
  ${LangFileString} MUI_UNTEXT_COMPONENTS_SUBTITLE "Kies de onderdelen die u wilt verwijderen."
!endif

!ifdef MUI_COMPONENTSPAGE | MUI_UNCOMPONENTSPAGE
  ${LangFileString} MUI_INNERTEXT_COMPONENTS_DESCRIPTION_TITLE "Beschrijving"
  !ifndef NSIS_CONFIG_COMPONENTPAGE_ALTERNATIVE
    ${LangFileString} MUI_INNERTEXT_COMPONENTS_DESCRIPTION_INFO "Beweeg uw muis over een onderdeel om de beschrijving te zien."
  !else
    ${LangFileString} MUI_INNERTEXT_COMPONENTS_DESCRIPTION_INFO "Beweeg uw muis over een onderdeel om de beschrijving te zien."
  !endif
!endif

!ifdef MUI_DIRECTORYPAGE
  ${LangFileString} MUI_TEXT_DIRECTORY_TITLE "Installatielocatie kiezen"
  ${LangFileString} MUI_TEXT_DIRECTORY_SUBTITLE "Kies de map waarin u $(^NameDA) wilt installeren."
!endif

!ifdef MUI_UNDIRECTORYPAGE
  ${LangFileString} MUI_UNTEXT_DIRECTORY_TITLE "Locatie kiezen"
  ${LangFileString} MUI_UNTEXT_DIRECTORY_SUBTITLE "Kies de map waaruit u $(^NameDA) wilt verwijderen."
!endif

!ifdef MUI_INSTFILESPAGE
  ${LangFileString} MUI_TEXT_INSTALLING_TITLE "Bezig met installeren"
  ${LangFileString} MUI_TEXT_INSTALLING_SUBTITLE "Een ogenblik geduld terwijl $(^NameDA) wordt geïnstalleerd."
  ${LangFileString} MUI_TEXT_FINISH_TITLE "Installatie voltooid"
  ${LangFileString} MUI_TEXT_FINISH_SUBTITLE "De installatie is succesvol voltooid."
  ${LangFileString} MUI_TEXT_ABORT_TITLE "Installatie afgebroken"
  ${LangFileString} MUI_TEXT_ABORT_SUBTITLE "De installatie is niet voltooid."
!endif

!ifdef MUI_UNINSTFILESPAGE
  ${LangFileString} MUI_UNTEXT_UNINSTALLING_TITLE "Bezig met verwijderen"
  ${LangFileString} MUI_UNTEXT_UNINSTALLING_SUBTITLE "Een ogenblik geduld terwijl $(^NameDA) van uw systeem wordt verwijderd."
  ${LangFileString} MUI_UNTEXT_FINISH_TITLE "Verwijderen gereed"
  ${LangFileString} MUI_UNTEXT_FINISH_SUBTITLE "$(^NameDA) is van uw systeem verwijderd."
  ${LangFileString} MUI_UNTEXT_ABORT_TITLE "Verwijderen afgebroken"
  ${LangFileString} MUI_UNTEXT_ABORT_SUBTITLE "$(^NameDA) is niet volledig van uw systeem verwijderd."
!endif

!ifdef MUI_FINISHPAGE
  ${LangFileString} MUI_TEXT_FINISH_INFO_TITLE "Voltooien van de $(^NameDA)-installatiewizard"
  ${LangFileString} MUI_TEXT_FINISH_INFO_TEXT "$(^NameDA) is geïnstalleerd op uw systeem.$\r$\n$\r$\nKlik op Voltooien om deze wizard te sluiten."
  ${LangFileString} MUI_TEXT_FINISH_INFO_REBOOT "Uw systeem moet opnieuw worden opgestart om de installatie van $(^NameDA) te voltooien. Wilt u nu herstarten?"
!endif

!ifdef MUI_UNFINISHPAGE
  ${LangFileString} MUI_UNTEXT_FINISH_INFO_TITLE "Voltooien van de $(^NameDA)-deïnstallatiewizard"
  ${LangFileString} MUI_UNTEXT_FINISH_INFO_TEXT "$(^NameDA) is van uw systeem verwijderd.$\r$\n$\r$\nKlik op Voltooien om deze wizard te sluiten."
  ${LangFileString} MUI_UNTEXT_FINISH_INFO_REBOOT "Uw systeem moet opnieuw worden opgestart om het verwijderen van $(^NameDA) te voltooien. Wilt u nu herstarten?"
!endif

!ifdef MUI_FINISHPAGE | MUI_UNFINISHPAGE
  ${LangFileString} MUI_TEXT_FINISH_REBOOTNOW "Nu herstarten"
  ${LangFileString} MUI_TEXT_FINISH_REBOOTLATER "Ik wil later handmatig herstarten"
  ${LangFileString} MUI_TEXT_FINISH_RUN "$(^NameDA) &starten"
  ${LangFileString} MUI_TEXT_FINISH_SHOWREADME "&Leesmij weergeven"
  ${LangFileString} MUI_BUTTONTEXT_FINISH "&Voltooien"  
!endif

!ifdef MUI_STARTMENUPAGE
  ${LangFileString} MUI_TEXT_STARTMENU_TITLE "Startmenumap kiezen"
  ${LangFileString} MUI_TEXT_STARTMENU_SUBTITLE "Kies een map in het menu Start voor de snelkoppelingen van $(^NameDA)."
  ${LangFileString} MUI_INNERTEXT_STARTMENU_TOP "Kies een map in het menu Start waarin de snelkoppelingen moeten worden aangemaakt. U kunt ook een naam invoeren om een nieuwe map te maken."
  ${LangFileString} MUI_INNERTEXT_STARTMENU_CHECKBOX "Geen snelkoppelingen aanmaken"
!endif

!ifdef MUI_UNCONFIRMPAGE
  ${LangFileString} MUI_UNTEXT_CONFIRM_TITLE "$(^NameDA) verwijderen"
  ${LangFileString} MUI_UNTEXT_CONFIRM_SUBTITLE "$(^NameDA) van uw systeem verwijderen."
!endif

!ifdef MUI_ABORTWARNING
  ${LangFileString} MUI_TEXT_ABORTWARNING "Weet u zeker dat u de $(^Name)-installatie wilt afsluiten?"
!endif

!ifdef MUI_UNABORTWARNING
  ${LangFileString} MUI_UNTEXT_ABORTWARNING "Weet u zeker dat u de $(^Name)-deïnstallatie wilt afsluiten?"
!endif

!ifdef MULTIUSER_INSTALLMODEPAGE
  ${LangFileString} MULTIUSER_TEXT_INSTALLMODE_TITLE "Gebruikers kiezen"
  ${LangFileString} MULTIUSER_TEXT_INSTALLMODE_SUBTITLE "Kies voor welke gebruikers u $(^NameDA) wilt installeren."
  ${LangFileString} MULTIUSER_INNERTEXT_INSTALLMODE_TOP "Selecteer of u $(^NameDA) alleen voor uzelf of voor alle gebruikers van deze computer wilt installeren. $(^ClickNext)"
  ${LangFileString} MULTIUSER_INNERTEXT_INSTALLMODE_ALLUSERS "Installeer voor alle gebruikers van deze computer"
  ${LangFileString} MULTIUSER_INNERTEXT_INSTALLMODE_CURRENTUSER "Installeer alleen voor mijzelf"
!endif
