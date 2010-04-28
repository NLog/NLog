;Language: Basque (1069)
;By Iñaki San Vicente

!insertmacro LANGFILE "Basque" "Euskera"

!ifdef MUI_WELCOMEPAGE
  ${LangFileString} MUI_TEXT_WELCOME_INFO_TITLE "Ongi etorri $(^NameDA) -ren instalazio programara"
  ${LangFileString} MUI_TEXT_WELCOME_INFO_TEXT "Programa honek $(^NameDA) zure ordenagailuan instalatuko du.$\r$\n$\r$\nAholkatzen da instalazioarekin hasi aurretik beste aplikazio guztiak ixtea. Honek sistemarekin erlazionatuta dauden fitxategien eguneratzea ahalbidetuko du, ordenagailua berrabiarazi beharrik izan gabe.$\r$\n$\r$\n$_CLICK"
!endif

!ifdef MUI_UNWELCOMEPAGE
  ${LangFileString} MUI_UNTEXT_WELCOME_INFO_TITLE "Ongi etorri $(^NameDA) -ren ezabaketa programara"
  ${LangFileString} MUI_UNTEXT_WELCOME_INFO_TEXT "Laguntzaile honek $(^NameDA)-ren ezabaketa prozesuan zehar gidatuko zaitu.$\r$\n$\r$\nEzabaketa hasi aurretik, ziurtatu $(^NameDA) martxan ez dagoela .$\r$\n$\r$\n$_CLICK"
!endif

!ifdef MUI_LICENSEPAGE
  ${LangFileString} MUI_TEXT_LICENSE_TITLE "Lizentzia hitzarmena"
  ${LangFileString} MUI_TEXT_LICENSE_SUBTITLE "Mesedez aztertu lizentziaren baldintzak $(^NameDA) instalatu aurretik."
  ${LangFileString} MUI_INNERTEXT_LICENSE_BOTTOM "Baldintzak onartzen badituzu, sakatu Onartu aurrera egiteko. Hitzarmena onartzea ezinbestekoa da $(^NameDA) instalatzeko."
  ${LangFileString} MUI_INNERTEXT_LICENSE_BOTTOM_CHECKBOX "Baldintzak onartzen badituzu, nabarmendu azpiko laukitxoa. Hitzarmena onartzea ezinbestekoa da $(^NameDA) instalatzeko. $_CLICK"
  ${LangFileString} MUI_INNERTEXT_LICENSE_BOTTOM_RADIOBUTTONS "Baldintzak onartzen badituzu, hautatu azpian lehen aukera. Hitzarmena onartzea ezinbestekoa da $(^NameDA) instalatzeko. $_CLICK"
!endif

!ifdef MUI_UNLICENSEPAGE
  ${LangFileString} MUI_UNTEXT_LICENSE_TITLE "Lizentzia hitzarmena"
  ${LangFileString} MUI_UNTEXT_LICENSE_SUBTITLE "Mesedez aztertu lizentziaren baldintzak $(^NameDA) ezabatu aurretik."
  ${LangFileString} MUI_UNINNERTEXT_LICENSE_BOTTOM "Baldintzak onartzen badituzu, sakatu Onartu aurrera egiteko. Hitzarmena onartzea ezinbestekoa da $(^NameDA) ezabatzeko."
  ${LangFileString} MUI_UNINNERTEXT_LICENSE_BOTTOM_CHECKBOX "Baldintzak onartzen badituzu, nabarmendu azpiko laukitxoa. Hitzarmena onartzea ezinbestekoa da $(^NameDA) ezabatzeko. $_CLICK"
  ${LangFileString} MUI_UNINNERTEXT_LICENSE_BOTTOM_RADIOBUTTONS "Baldintzak onartzen badituzu, hautatu azpian lehen aukera. Hitzarmena onartzea ezinbestekoa da $(^NameDA) ezabatzeko. $_CLICK"
!endif

!ifdef MUI_LICENSEPAGE | MUI_UNLICENSEPAGE
  ${LangFileString} MUI_INNERTEXT_LICENSE_TOP "Sakatu Av Pág hitzarmenaren gainontzeko atalak ikusteko."
!endif

!ifdef MUI_COMPONENTSPAGE
  ${LangFileString} MUI_TEXT_COMPONENTS_TITLE "Osagaien hautatzea"
  ${LangFileString} MUI_TEXT_COMPONENTS_SUBTITLE "Hautatu $(^NameDA)-ren zein ezaugarri instalatu nahi duzun."
!endif

!ifdef MUI_UNCOMPONENTSPAGE
  ${LangFileString} MUI_UNTEXT_COMPONENTS_TITLE "Osagaien hautatzea"
  ${LangFileString} MUI_UNTEXT_COMPONENTS_SUBTITLE "Hautatu $(^NameDA)-ren zein ezaugarri ezabatu nahi duzun."
!endif

!ifdef MUI_COMPONENTSPAGE | MUI_UNCOMPONENTSPAGE
  ${LangFileString} MUI_INNERTEXT_COMPONENTS_DESCRIPTION_TITLE "Azalpena"
  !ifndef NSIS_CONFIG_COMPONENTPAGE_ALTERNATIVE
    ${LangFileString} MUI_INNERTEXT_COMPONENTS_DESCRIPTION_INFO "Jarri sagua osagai baten gainean dagokion azalpena ikusteko."
  !else
    ${LangFileString} MUI_INNERTEXT_COMPONENTS_DESCRIPTION_INFO "Jarri sagua osagai baten gainean dagokion azalpena ikusteko."
  !endif
!endif

!ifdef MUI_DIRECTORYPAGE
  ${LangFileString} MUI_TEXT_DIRECTORY_TITLE "Hautatu instalazioaren lekua"
  ${LangFileString} MUI_TEXT_DIRECTORY_SUBTITLE "Aukeratu $(^NameDA) instalatzeko karpeta."
!endif

!ifdef MUI_UNDIRECTORYPAGE
  ${LangFileString} MUI_UNTEXT_DIRECTORY_TITLE "Aukeratu ezabatuko den karpeta"
  ${LangFileString} MUI_UNTEXT_DIRECTORY_SUBTITLE "Aukeratu $(^NameDA) zein karpetatik ezabatuko den."
!endif

!ifdef MUI_INSTFILESPAGE
  ${LangFileString} MUI_TEXT_INSTALLING_TITLE "Instalatzen"
  ${LangFileString} MUI_TEXT_INSTALLING_SUBTITLE "Mesedez itxoin $(^NameDA) instalatzen den bitartean."
  ${LangFileString} MUI_TEXT_FINISH_TITLE "Instalazioa burututa"
  ${LangFileString} MUI_TEXT_FINISH_SUBTITLE "Instalazioa zuzen burutu da."
  ${LangFileString} MUI_TEXT_ABORT_TITLE "Instalazioa ezeztatua"
  ${LangFileString} MUI_TEXT_ABORT_SUBTITLE "Instalazioa ez da zuzen burutu."
!endif

!ifdef MUI_UNINSTFILESPAGE
  ${LangFileString} MUI_UNTEXT_UNINSTALLING_TITLE "Ezabatzen"
  ${LangFileString} MUI_UNTEXT_UNINSTALLING_SUBTITLE "Mesedez itxoin $(^NameDA) ezabatzen den bitartean."
  ${LangFileString} MUI_UNTEXT_FINISH_TITLE "Ezabatzea burututa"
  ${LangFileString} MUI_UNTEXT_FINISH_SUBTITLE "Ezabatzea zuzen burutu da."
  ${LangFileString} MUI_UNTEXT_ABORT_TITLE "Ezabatzea ezeztatuta"
  ${LangFileString} MUI_UNTEXT_ABORT_SUBTITLE "Ezabatzea ez da zuzen burutu."
!endif

!ifdef MUI_FINISHPAGE
  ${LangFileString} MUI_TEXT_FINISH_INFO_TITLE "$(^NameDA)-ren instalazio laguntzailea osatzen"
  ${LangFileString} MUI_TEXT_FINISH_INFO_TEXT "$(^NameDA) zure sisteman instalatu da.$\r$\n$\r$\nSakatu Amaitu laguntzaile hau ixteko."
  ${LangFileString} MUI_TEXT_FINISH_INFO_REBOOT "Zure sistema berrabiarazi behar duzu $(^NameDA)-ren instalazioa osatzeko. Orain Berrabiarazi nahi duzu?"
!endif

!ifdef MUI_UNFINISHPAGE
  ${LangFileString} MUI_UNTEXT_FINISH_INFO_TITLE "$(^NameDA)-ren ezabaketa laguntzailea osatzen"
  ${LangFileString} MUI_UNTEXT_FINISH_INFO_TEXT "$(^NameDA) zure sistematik ezabatu da.$\r$\n$\r$\nSakatu Amaitu laguntzaile hau ixteko."
  ${LangFileString} MUI_UNTEXT_FINISH_INFO_REBOOT "Zure ordenagailuak berrabiarazia izan behar du $(^NameDA)-ren ezabaketa osatzeko. Orain Berrabiarazi nahi duzu?"
!endif

!ifdef MUI_FINISHPAGE | MUI_UNFINISHPAGE
  ${LangFileString} MUI_TEXT_FINISH_REBOOTNOW "Berrabiarazi orain"
  ${LangFileString} MUI_TEXT_FINISH_REBOOTLATER "Neuk berrabiarazi geroago"
  ${LangFileString} MUI_TEXT_FINISH_RUN "&Exekutatu $(^NameDA)"
  ${LangFileString} MUI_TEXT_FINISH_SHOWREADME "&Ikusi Readme.txt"
  ${LangFileString} MUI_BUTTONTEXT_FINISH "&Amaitu"  
!endif

!ifdef MUI_STARTMENUPAGE
  ${LangFileString} MUI_TEXT_STARTMENU_TITLE "Aukeratu Hasiera Menuko karpeta"
  ${LangFileString} MUI_TEXT_STARTMENU_SUBTITLE "Aukeratu Hasiera Menuko karpeta bat $(^NameDA)-ren lasterbideentzako."
  ${LangFileString} MUI_INNERTEXT_STARTMENU_TOP "Aukeratu Hasiera Menuko karpeta bat, non programaren lasterbideak instalatu nahi dituzun. Karpeta berri bat sortzeko izen bat ere adierazi dezakezu."
  ${LangFileString} MUI_INNERTEXT_STARTMENU_CHECKBOX "Ez sortu lasterbiderik"
!endif

!ifdef MUI_UNCONFIRMPAGE
  ${LangFileString} MUI_UNTEXT_CONFIRM_TITLE "Ezabatu $(^NameDA)"
  ${LangFileString} MUI_UNTEXT_CONFIRM_SUBTITLE "$(^NameDA) zure sistematik ezabatzen du."
!endif

!ifdef MUI_ABORTWARNING
  ${LangFileString} MUI_TEXT_ABORTWARNING "Ziur zaude $(^Name)-ren instalaziotik irten nahi duzula?"
!endif

!ifdef MUI_UNABORTWARNING
  ${LangFileString} MUI_UNTEXT_ABORTWARNING "Ziur zaude $(^Name)-ren ezabaketa laguntzailetik irten nahi duzula?"
!endif
