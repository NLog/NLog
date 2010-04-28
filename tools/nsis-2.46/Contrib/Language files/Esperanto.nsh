;Language: Esperanto (0)
;By Felipe Castro <fefcas@gmail.com>

!insertmacro LANGFILE "Esperanto" "Esperanto"

!ifdef MUI_WELCOMEPAGE
  ${LangFileString} MUI_TEXT_WELCOME_INFO_TITLE "Bonvenon al la Gvidilo por Instalado de $(^NameDA)"
  ${LangFileString} MUI_TEXT_WELCOME_INFO_TEXT "Tiu cxi gvidilo helpos vin dum la instalado de $(^NameDA).$\r$\n$\r$\nOni rekomendas fermi cxiujn aliajn aplikajxojn antaux ol ekigi la Instaladon. Tio cxi ebligos al la Instalilo gxisdatigi la koncernajn dosierojn de la sistemo sen bezono restartigi la komputilon.$\r$\n$\r$\n$_CLICK"
!endif

!ifdef MUI_UNWELCOMEPAGE
  ${LangFileString} MUI_UNTEXT_WELCOME_INFO_TITLE "Bonvenon al la Gvidilo por Malinstalado de $(^NameDA)"
  ${LangFileString} MUI_UNTEXT_WELCOME_INFO_TEXT "Tiu cxi gvidilo helpos vin dum la malinstalado de $(^NameDA).$\r$\n$\r$\nAntaux ol ekigi la malinstalado, certigxu ke $(^NameDA) ne estas plenumata nun.$\r$\n$\r$\n$_CLICK"
!endif

!ifdef MUI_LICENSEPAGE
  ${LangFileString} MUI_TEXT_LICENSE_TITLE "Permes-Kontrakto"
  ${LangFileString} MUI_TEXT_LICENSE_SUBTITLE "Bonvole, kontrolu la kondicxojn de la permesilo antaux ol instali la programon $(^NameDA)."
  ${LangFileString} MUI_INNERTEXT_LICENSE_BOTTOM "Se vi akceptas la kondicxojn, musklaku en 'Akceptite' por dauxrigi. Vi devos akcepti la kontrakton por instali la programon $(^NameDA)."
  ${LangFileString} MUI_INNERTEXT_LICENSE_BOTTOM_CHECKBOX "Se vi akceptas la permes-kondicxojn, musklaku la suban elekt-skatolon. Vi devos akcepti la kontrakton por instali la programon $(^NameDA). $_CLICK"
  ${LangFileString} MUI_INNERTEXT_LICENSE_BOTTOM_RADIOBUTTONS "Se vi akceptas la permes-kondicxojn, elektu la unuan opcion sube. Vi devas akcepti la kontrakton por instali la programon $(^NameDA). $_CLICK"
!endif

!ifdef MUI_UNLICENSEPAGE
  ${LangFileString} MUI_UNTEXT_LICENSE_TITLE "Permes-Kontrakto"
  ${LangFileString} MUI_UNTEXT_LICENSE_SUBTITLE "Bonvole, kontrolu la kondicxojn de la permesilo antaux ol malinstali la programon $(^NameDA)."
  ${LangFileString} MUI_UNINNERTEXT_LICENSE_BOTTOM "Se vi akceptas la kondicxojn, musklaku en 'Akceptite' por dauxrigi. Vi devos akcepti la kontrakton por malinstali la programon $(^NameDA)."
  ${LangFileString} MUI_UNINNERTEXT_LICENSE_BOTTOM_CHECKBOX "Se vi akceptas la permes-kondicxojn, musklaku la suban elekt-skatolon. Vi devos akcepti la kontrakton por malinstali la programon $(^NameDA). $_CLICK"
  ${LangFileString} MUI_UNINNERTEXT_LICENSE_BOTTOM_RADIOBUTTONS "Se vi akceptas la permes-kondicxojn, elektu la unuan opcion sube. Vi devas akcepti la kontrakton por malinstali la programon $(^NameDA). $_CLICK"
!endif

!ifdef MUI_LICENSEPAGE | MUI_UNLICENSEPAGE
  ${LangFileString} MUI_INNERTEXT_LICENSE_TOP "Premu 'Page Down' por rigardi la reston de la permeso."
!endif

!ifdef MUI_COMPONENTSPAGE
  ${LangFileString} MUI_TEXT_COMPONENTS_TITLE "Elekto de Konsisteroj"
  ${LangFileString} MUI_TEXT_COMPONENTS_SUBTITLE "Elektu kiujn funkciojn de $(^NameDA) vi deziras instali."
!endif

!ifdef MUI_UNCOMPONENTSPAGE
  ${LangFileString} MUI_UNTEXT_COMPONENTS_TITLE "Elekto de Konsisteroj"
  ${LangFileString} MUI_UNTEXT_COMPONENTS_SUBTITLE "Elektu kiujn funkciojn de $(^NameDA) vi deziras malinstali."
!endif

!ifdef MUI_COMPONENTSPAGE | MUI_UNCOMPONENTSPAGE
  ${LangFileString} MUI_INNERTEXT_COMPONENTS_DESCRIPTION_TITLE "Priskribo"
  !ifndef NSIS_CONFIG_COMPONENTPAGE_ALTERNATIVE
    ${LangFileString} MUI_INNERTEXT_COMPONENTS_DESCRIPTION_INFO "Poziciu la muson sur konsistero por rigardi ties priskribon."
  !else
    ${LangFileString} MUI_INNERTEXT_COMPONENTS_DESCRIPTION_INFO "Poziciu la muson sur konsistero por rigardi ties priskribon."
  !endif
!endif

!ifdef MUI_DIRECTORYPAGE
  ${LangFileString} MUI_TEXT_DIRECTORY_TITLE "Elekto de la Instalada Loko"
  ${LangFileString} MUI_TEXT_DIRECTORY_SUBTITLE "Elektu la dosierujon en kiun vi deziras instali la programon $(^NameDA)."
!endif

!ifdef MUI_UNDIRECTORYPAGE
  ${LangFileString} MUI_UNTEXT_DIRECTORY_TITLE "Elekto de la Malinstalada Loko"
  ${LangFileString} MUI_UNTEXT_DIRECTORY_SUBTITLE "Elektu la dosierujon el kiu vi deziras malinstali la programon $(^NameDA)."
!endif

!ifdef MUI_INSTFILESPAGE
  ${LangFileString} MUI_TEXT_INSTALLING_TITLE "Oni instalas"
  ${LangFileString} MUI_TEXT_INSTALLING_SUBTITLE "Bonvole, atendu dum $(^NameDA) estas instalata."
  ${LangFileString} MUI_TEXT_FINISH_TITLE "Instalado Plenumite"
  ${LangFileString} MUI_TEXT_FINISH_SUBTITLE "La instalado sukcese plenumigxis."
  ${LangFileString} MUI_TEXT_ABORT_TITLE "Instalado Cxesigite"
  ${LangFileString} MUI_TEXT_ABORT_SUBTITLE "La instalado ne plenumigxis sukcese."
!endif

!ifdef MUI_UNINSTFILESPAGE
  ${LangFileString} MUI_UNTEXT_UNINSTALLING_TITLE "Oni malinstalas"
  ${LangFileString} MUI_UNTEXT_UNINSTALLING_SUBTITLE "Bonvole, atendu dum $(^NameDA) estas malinstalata."
  ${LangFileString} MUI_UNTEXT_FINISH_TITLE "Malinstalado Plenumite"
  ${LangFileString} MUI_UNTEXT_FINISH_SUBTITLE "La malinstalado sukcese plenumigxis."
  ${LangFileString} MUI_UNTEXT_ABORT_TITLE "Malinstalado Cxesigxite"
  ${LangFileString} MUI_UNTEXT_ABORT_SUBTITLE "La malinstalado ne plenumigxis sukcese."
!endif

!ifdef MUI_FINISHPAGE
  ${LangFileString} MUI_TEXT_FINISH_INFO_TITLE "Oni finigas la Gvidilon por Instalado de $(^NameDA)"
  ${LangFileString} MUI_TEXT_FINISH_INFO_TEXT "$(^NameDA) estas instalita en via komputilo.$\r$\n$\r$\nMusklaku en Finigi por fermi tiun cxi gvidilon."
  ${LangFileString} MUI_TEXT_FINISH_INFO_REBOOT "Via komputilo devas esti restartigita por kompletigi la instaladon de $(^NameDA). Cxu restartigi nun?"
!endif

!ifdef MUI_UNFINISHPAGE
  ${LangFileString} MUI_UNTEXT_FINISH_INFO_TITLE "Oni finigas la Gvidilon por Malinstalado de $(^NameDA)"
  ${LangFileString} MUI_UNTEXT_FINISH_INFO_TEXT "$(^NameDA) estis forigita el via komputilo.$\r$\n$\r$\nMusklaku en Finigi por fermi tiun cxi gvidilon."
  ${LangFileString} MUI_UNTEXT_FINISH_INFO_REBOOT "Via komputilo devas esti restartigita por kompletigi la malinstaladon de $(^NameDA). Cxu restartigi nun?"
!endif

!ifdef MUI_FINISHPAGE | MUI_UNFINISHPAGE
  ${LangFileString} MUI_TEXT_FINISH_REBOOTNOW "Restartigi Nun"
  ${LangFileString} MUI_TEXT_FINISH_REBOOTLATER "Mi volas restartigi permane poste"
  ${LangFileString} MUI_TEXT_FINISH_RUN "&Lancxi $(^NameDA)"
  ${LangFileString} MUI_TEXT_FINISH_SHOWREADME "&Montri Legumin"
  ${LangFileString} MUI_BUTTONTEXT_FINISH "&Finigi"  
!endif

!ifdef MUI_STARTMENUPAGE
  ${LangFileString} MUI_TEXT_STARTMENU_TITLE "Elektu Dosierujon de la Ek-Menuo"
  ${LangFileString} MUI_TEXT_STARTMENU_SUBTITLE "Elektu dosierujon de la Ek-Menuo por la lancxiloj de la programo."
  ${LangFileString} MUI_INNERTEXT_STARTMENU_TOP "Elektu dosierujon de la Ek-Menuo en kiu vi deziras krei la lancxilojn de la programo. Vi povas ankaux tajpi nomon por krei novan ujon."
  ${LangFileString} MUI_INNERTEXT_STARTMENU_CHECKBOX "Ne krei lancxilojn"
!endif

!ifdef MUI_UNCONFIRMPAGE
  ${LangFileString} MUI_UNTEXT_CONFIRM_TITLE "Malinstali $(^NameDA)"
  ${LangFileString} MUI_UNTEXT_CONFIRM_SUBTITLE "Forigi $(^NameDA) el via komputilo."
!endif

!ifdef MUI_ABORTWARNING
  ${LangFileString} MUI_TEXT_ABORTWARNING "Cxu vi certe deziras nuligi la instaladon de $(^Name)?"
!endif

!ifdef MUI_UNABORTWARNING
  ${LangFileString} MUI_UNTEXT_ABORTWARNING "Cxu vi certe deziras nuligi la malinstaladon de $(^Name)?"
!endif

!ifdef MULTIUSER_INSTALLMODEPAGE
  ${LangFileString} MULTIUSER_TEXT_INSTALLMODE_TITLE "Elekti Uzantojn"
  ${LangFileString} MULTIUSER_TEXT_INSTALLMODE_SUBTITLE "Elekti por kiuj uzantoj vi deziras instali $(^NameDA)."
  ${LangFileString} MULTIUSER_INNERTEXT_INSTALLMODE_TOP "Elektu cxu vi volas instali $(^NameDA) por vi mem aux por cxiuj uzantoj de tiu cxi komputilo. $(^ClickNext)"
  ${LangFileString} MULTIUSER_INNERTEXT_INSTALLMODE_ALLUSERS "Instali por iu ajn uzanto de tiu cxi komputilo"
  ${LangFileString} MULTIUSER_INNERTEXT_INSTALLMODE_CURRENTUSER "Instali nur por mi"
!endif
