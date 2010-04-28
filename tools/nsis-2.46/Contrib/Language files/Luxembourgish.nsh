;Language: Luxembourgish (1031)
;By Snowloard, changes by Philo

!insertmacro LANGFILE "Luxembourgish" "Lëtzebuergesch"

!ifdef MUI_WELCOMEPAGE
  ${LangFileString} MUI_TEXT_WELCOME_INFO_TITLE "Wëllkomm beim Installatiouns-$\r$\nAssistent vun $(^NameDA)"
  ${LangFileString} MUI_TEXT_WELCOME_INFO_TEXT "Dësen Assistent wärt dech duech d'Installatioun vun $(^NameDA) begleeden.$\r$\n$\r$\nEt gëtt ugeroden alleguer d'Programmer di am Moment lafen zouzemaan, datt bestëmmt Systemdateien ouni Neistart ersat kënne ginn.$\r$\n$\r$\n$_CLICK"
!endif

!ifdef MUI_UNWELCOMEPAGE
  ${LangFileString} MUI_UNTEXT_WELCOME_INFO_TITLE "Wëllkomm am Desinstallatiouns-$\r$\n\Assistent fir $(^NameDA)"
  ${LangFileString} MUI_UNTEXT_WELCOME_INFO_TEXT "Desen Assistent begleet dech duech d'Desinstallatioun vun $(^NameDA).$\r$\n$\r$\nW.e.g. maach $(^NameDA) zu, ierts de mat der Desinstallatioun ufänks.$\r$\n$\r$\n$_CLICK"
!endif

!ifdef MUI_LICENSEPAGE
  ${LangFileString} MUI_TEXT_LICENSE_TITLE "Lizenzofkommes"
  ${LangFileString} MUI_TEXT_LICENSE_SUBTITLE "W.e.g. d'Lizenzoofkommes liesen, ierts de mat der Installatioun weiderfiers."
  ${LangFileString} MUI_INNERTEXT_LICENSE_BOTTOM "Wanns de alleguer d'Bedengungen vum Ofkommes akzeptéiers, klick op Unhuelen. Du muss alleguer d'Fuerderungen unerkennen, fir $(^NameDA) installéieren ze kënnen."
  ${LangFileString} MUI_INNERTEXT_LICENSE_BOTTOM_CHECKBOX "Wanns de alleguer d'Bedengungen vum Ofkommes akzeptéiers, aktivéier d'Këschtchen. Du muss alleguer d'Fuerderungen unerkennen, fir $(^NameDA) installéieren ze kënnen. $_CLICK"
  ${LangFileString} MUI_INNERTEXT_LICENSE_BOTTOM_RADIOBUTTONS "Wanns de alleguer d'Bedengungen vum Ofkommes akzeptéiers, wiel ënnen di entspriechend Äntwert aus. Du muss alleguer d'Fuerderungen unerkennen, fir $(^NameDA) installéieren ze kënnen. $_CLICK"
!endif

!ifdef MUI_UNLICENSEPAGE
  ${LangFileString} MUI_UNTEXT_LICENSE_TITLE "Lizenzofkommes"
  ${LangFileString} MUI_UNTEXT_LICENSE_SUBTITLE "W.e.g. lies d'Lizenzofkommes duech ierts de mat der Desinstallatioun vun $(^NameDA) weiderfiers."
  ${LangFileString} MUI_UNINNERTEXT_LICENSE_BOTTOM "Wanns de d'Fuerderungen vum Ofkommes akzeptéiers, klick op unhuelen. Du muss d'Ofkommes akzeptéieren, fir $(^NameDA) kënnen ze desinstalléieren."
  ${LangFileString} MUI_UNINNERTEXT_LICENSE_BOTTOM_CHECKBOX "Wanns de d'Fuerderungen vum Ofkommes akzeptéiers, aktivéier d'Këschtchen. Du muss d'Ofkommes akzeptéieren, fir $(^NameDA) kënnen ze desinstalléieren. $_CLICK"
  ${LangFileString} MUI_UNINNERTEXT_LICENSE_BOTTOM_RADIOBUTTONS "Wanns de d'Fuerderungen vum Ofkommes akzeptéiers, wiel ënnen di entspriechend Optioun. Du muss d'Oofkommes akzeptéieren, fir $(^NameDA) kennen ze desinstalléieren. $_CLICK"
!endif

!ifdef MUI_LICENSEPAGE | MUI_UNLICENSEPAGE
  ${LangFileString} MUI_INNERTEXT_LICENSE_TOP "Dréck d'PageDown-Tast fir den Rescht vum Ofkommes ze liesen."
!endif

!ifdef MUI_COMPONENTSPAGE
  ${LangFileString} MUI_TEXT_COMPONENTS_TITLE "Komponenten auswielen"
  ${LangFileString} MUI_TEXT_COMPONENTS_SUBTITLE "Wiel d'Komponenten aus, déis de wëlls installéieren."
!endif

!ifdef MUI_UNCOMPONENTSPAGE
  ${LangFileString} MUI_UNTEXT_COMPONENTS_TITLE "Komponenten auswielen"
  ${LangFileString} MUI_UNTEXT_COMPONENTS_SUBTITLE "Wiel eng Komponent aus, déis de desinstalléieren wëlls."
!endif

!ifdef MUI_COMPONENTSPAGE | MUI_UNCOMPONENTSPAGE
  ${LangFileString} MUI_INNERTEXT_COMPONENTS_DESCRIPTION_TITLE "Beschreiwung"
  !ifndef NSIS_CONFIG_COMPONENTPAGE_ALTERNATIVE
    ${LangFileString} MUI_INNERTEXT_COMPONENTS_DESCRIPTION_INFO "Hal den Mausfeil iwwer eng Komponent, fir d'Beschreiwung dervun ze gesinn."
  !else
    ${LangFileString} MUI_INNERTEXT_COMPONENTS_DESCRIPTION_INFO "Hal den Mausfeil iwwer eng Komponent, fir d'Beschreiwung dervun ze gesinn."
  !endif
!endif

!ifdef MUI_DIRECTORYPAGE
  ${LangFileString} MUI_TEXT_DIRECTORY_TITLE "Zielverzeechnes auswielen"
  ${LangFileString} MUI_TEXT_DIRECTORY_SUBTITLE "Wiel den Dossier aus, an deen $(^NameDA) installéiert soll ginn."
!endif

!ifdef MUI_UNDIRECTORYPAGE
  ${LangFileString} MUI_UNTEXT_DIRECTORY_TITLE "Dossier fir d'Desinstallatioun wielen"
  ${LangFileString} MUI_UNTEXT_DIRECTORY_SUBTITLE "Wiel den Dossier aus, aus dem $(^NameDA) desinstalléiert soll ginn."
!endif

!ifdef MUI_INSTFILESPAGE
  ${LangFileString} MUI_TEXT_INSTALLING_TITLE "Installéieren..."
  ${LangFileString} MUI_TEXT_INSTALLING_SUBTITLE "Waard w.e.g während deem $(^NameDA) installéiert gëtt."
  ${LangFileString} MUI_TEXT_FINISH_TITLE "Installatioun färdeg"
  ${LangFileString} MUI_TEXT_FINISH_SUBTITLE "D'Installatioun ass feelerfräi oofgeschloss ginn."
  ${LangFileString} MUI_TEXT_ABORT_TITLE "Installatioun ofgebrach"
  ${LangFileString} MUI_TEXT_ABORT_SUBTITLE "D'Installatioun ass net komplett ofgeschloss ginn."
!endif

!ifdef MUI_UNINSTFILESPAGE
  ${LangFileString} MUI_UNTEXT_UNINSTALLING_TITLE "Desinstalléieren..."
  ${LangFileString} MUI_UNTEXT_UNINSTALLING_SUBTITLE "W.e.g. waard, während deems $(^NameDA) desinstalléiert gëtt."
  ${LangFileString} MUI_UNTEXT_FINISH_TITLE "Desinstallatioun ofgeschloss"
  ${LangFileString} MUI_UNTEXT_FINISH_SUBTITLE "D'Desinstallatioun ass erfollegräich ofgeschloss ginn."
  ${LangFileString} MUI_UNTEXT_ABORT_TITLE "Desinstallatioun oofbriechen"
  ${LangFileString} MUI_UNTEXT_ABORT_SUBTITLE "Desinstallatioun ass net erfollegräich ofgeschloss ginn."
!endif

!ifdef MUI_FINISHPAGE
  ${LangFileString} MUI_TEXT_FINISH_INFO_TITLE "D'Installatioun vun $(^NameDA) gëtt ofgeschloss."
  ${LangFileString} MUI_TEXT_FINISH_INFO_TEXT "$(^NameDA) ass um Computer installéiert ginn.$\r$\n$\r$\nKlick op färdeg maan, fir den Installatiouns-Assistent zou ze maan.."
  ${LangFileString} MUI_TEXT_FINISH_INFO_REBOOT "Den Windows muss nei gestart ginn, fir d'Installatioun vun $(^NameDA) ofzeschléissen. Wëlls de Windows lo néi starten?"
!endif

!ifdef MUI_UNFINISHPAGE
  ${LangFileString} MUI_UNTEXT_FINISH_INFO_TITLE "Desinstallatioun vun $(^NameDA) gëtt ofgeschloss"
  ${LangFileString} MUI_UNTEXT_FINISH_INFO_TEXT "$(^NameDA) ass vum Computer desinstalléiert ginn.$\r$\n$\r$\nKlick op Ofschléissen fir den Assistent zou ze maan."
  ${LangFileString} MUI_UNTEXT_FINISH_INFO_REBOOT "Windows muss néi gestart gin, fir d'Desinstallatioun vun $(^NameDA) ze vervollstännegen. Wëlls de Windows lo néi starten?"
!endif

!ifdef MUI_FINISHPAGE | MUI_UNFINISHPAGE
  ${LangFileString} MUI_TEXT_FINISH_REBOOTNOW "Lo néi starten"
  ${LangFileString} MUI_TEXT_FINISH_REBOOTLATER "Spéider manuell néi starten"
  ${LangFileString} MUI_TEXT_FINISH_RUN "$(^NameDA) op maan"
  ${LangFileString} MUI_TEXT_FINISH_SHOWREADME "Liesmech op maan"
  ${LangFileString} MUI_BUTTONTEXT_FINISH "&Färdeg man"  
!endif

!ifdef MUI_STARTMENUPAGE
  ${LangFileString} MUI_TEXT_STARTMENU_TITLE "Startmenü-Dossier bestëmmen"
  ${LangFileString} MUI_TEXT_STARTMENU_SUBTITLE "Bestëmm een Startmanü-Dossier an deen d'Programmofkierzungen kommen."
  ${LangFileString} MUI_INNERTEXT_STARTMENU_TOP "Bestëmm een Startmanü-Dossier an deen d'Programmofkierzungen kommen. Wanns de een néien Dossier man wells, gëff deem säin zukünftegen Numm an."
  ${LangFileString} MUI_INNERTEXT_STARTMENU_CHECKBOX "Keng Ofkierzungen man"
!endif

!ifdef MUI_UNCONFIRMPAGE
  ${LangFileString} MUI_UNTEXT_CONFIRM_TITLE "Desinstallatioun vun $(^NameDA)"
  ${LangFileString} MUI_UNTEXT_CONFIRM_SUBTITLE "$(^NameDA) gett vum Computer desinstalléiert."
!endif

!ifdef MUI_ABORTWARNING
  ${LangFileString} MUI_TEXT_ABORTWARNING "Bass de sécher, dass de d'Installatioun vun $(^Name) ofbriechen wëlls?"
!endif

!ifdef MUI_UNABORTWARNING
  ${LangFileString} MUI_UNTEXT_ABORTWARNING "Bass de sécher, dass de d'Desinstallatioun vun $(^Name) ofbriechen wëlls?"
!endif
