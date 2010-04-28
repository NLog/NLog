;Language: Italian (1040)
;By SANFACE Software <sanface@sanface.com> v1.67 accents
;Review and update from v1.65 to v1.67 by Alessandro Staltari < staltari (a) geocities.com >
;Review and update from v1.67 to v1.68 by Lorenzo Bevilacqua < meow811@libero.it >

!insertmacro LANGFILE "Italian" "Italiano"

!ifdef MUI_WELCOMEPAGE
  ${LangFileString} MUI_TEXT_WELCOME_INFO_TITLE "Benvenuti nel programma di installazione di $(^NameDA)"
  ${LangFileString} MUI_TEXT_WELCOME_INFO_TEXT "Questo programma installerà $(^NameDA) nel vostro computer.$\r$\n$\r$\nSi raccomanda di chiudere tutte le altre applicazioni prima di iniziare l'installazione. Questo permetterà al programma di installazione di aggiornare i file di sistema senza dover riavviare il computer.$\r$\n$\r$\n$_CLICK"
!endif

!ifdef MUI_UNWELCOMEPAGE
  ${LangFileString} MUI_UNTEXT_WELCOME_INFO_TITLE "Benvenuti nella procedura guidata di disinstallazione di $(^NameDA)"
  ${LangFileString} MUI_UNTEXT_WELCOME_INFO_TEXT "Questa procedura vi guiderà nella disinstallazione di $(^NameDA).$\r$\n$\r$\nPrima di iniziare la disinstallazione, assicuratevi che $(^Name) non sia in esecuzione.$\r$\n$\r$\n$_CLICK"
!endif

!ifdef MUI_LICENSEPAGE
  ${LangFileString} MUI_TEXT_LICENSE_TITLE "Licenza d'uso"
  ${LangFileString} MUI_TEXT_LICENSE_SUBTITLE "Prego leggere le condizioni della licenza d'uso prima di installare $(^NameDA)."
  ${LangFileString} MUI_INNERTEXT_LICENSE_BOTTOM "Se si accettano i termini della licenza d'uso scegliere Accetto per continuare. È necessario accettare i termini della licenza d'uso per installare $(^NameDA)."
  ${LangFileString} MUI_INNERTEXT_LICENSE_BOTTOM_CHECKBOX "Se si accettano i termini della licenza d'uso, selezionare la casella sottostante. È necessario accettare i termini della licenza d'uso per installare $(^NameDA). $_CLICK"
  ${LangFileString} MUI_INNERTEXT_LICENSE_BOTTOM_RADIOBUTTONS "Se si accettano i termini della licenza d'uso, selezionare la prima opzione sottostante. È necessario accettare i termini della licenza d'uso per installare $(^NameDA). $_CLICK"
!endif

!ifdef MUI_UNLICENSEPAGE
  ${LangFileString} MUI_UNTEXT_LICENSE_TITLE "Licenza d'uso"
  ${LangFileString} MUI_UNTEXT_LICENSE_SUBTITLE "Prego leggere le condizioni della licenza d'uso prima di disinstallare $(^NameDA)."
  ${LangFileString} MUI_UNINNERTEXT_LICENSE_BOTTOM "Se si accettano i termini della licenza d'uso scegliere Accetto per continuare. È necessario accettare i termini della licenza d'uso per disinstallare $(^NameDA)."
  ${LangFileString} MUI_UNINNERTEXT_LICENSE_BOTTOM_CHECKBOX "Se si accettano i termini della licenza d'uso, selezionare la casella sottostante. È necessario accettare i termini della licenza d'uso per disinstallare $(^NameDA). $_CLICK"
  ${LangFileString} MUI_UNINNERTEXT_LICENSE_BOTTOM_RADIOBUTTONS "Se si accettano i termini della licenza d'uso, selezionare la prima opzione sottostante. È necessario accettare i termini della licenza d'uso per disinstallare $(^NameDA). $_CLICK"
!endif

!ifdef MUI_LICENSEPAGE | MUI_UNLICENSEPAGE
  ${LangFileString} MUI_INNERTEXT_LICENSE_TOP "Premere Page Down per vedere il resto della licenza d'uso."
!endif

!ifdef MUI_COMPONENTSPAGE
  ${LangFileString} MUI_TEXT_COMPONENTS_TITLE "Selezione dei componenti"
  ${LangFileString} MUI_TEXT_COMPONENTS_SUBTITLE "Selezionare i componenti di $(^NameDA) che si desidera installare."
!endif

!ifdef MUI_UNCOMPONENTSPAGE
  ${LangFileString} MUI_UNTEXT_COMPONENTS_TITLE "Selezione componenti"
  ${LangFileString} MUI_UNTEXT_COMPONENTS_SUBTITLE "Scegliere i componenti di $(^NameDA) che si desidera disinstallare."
!endif

!ifdef MUI_COMPONENTSPAGE | MUI_UNCOMPONENTSPAGE
  ${LangFileString} MUI_INNERTEXT_COMPONENTS_DESCRIPTION_TITLE "Descrizione"
  !ifndef NSIS_CONFIG_COMPONENTPAGE_ALTERNATIVE
    ${LangFileString} MUI_INNERTEXT_COMPONENTS_DESCRIPTION_INFO "Posizionare il puntatore del mouse sul componente per vederne la descrizione."
  !else
    ${LangFileString} MUI_INNERTEXT_COMPONENTS_DESCRIPTION_INFO "Posizionare il puntatore del mouse sul componente per vederne la descrizione."
  !endif
!endif

!ifdef MUI_DIRECTORYPAGE
  ${LangFileString} MUI_TEXT_DIRECTORY_TITLE "Scelta della cartella di installazione"
  ${LangFileString} MUI_TEXT_DIRECTORY_SUBTITLE "Scegliere la cartella nella quale installare $(^NameDA)."
!endif

!ifdef MUI_UNDIRECTORYPAGE
  ${LangFileString} MUI_UNTEXT_DIRECTORY_TITLE "Scelta della cartella da cui disinstallare"
  ${LangFileString} MUI_UNTEXT_DIRECTORY_SUBTITLE "Scegliere la cartella dalla quale disinstallare $(^NameDA)."
!endif

!ifdef MUI_INSTFILESPAGE
  ${LangFileString} MUI_TEXT_INSTALLING_TITLE "Installazione in corso"
  ${LangFileString} MUI_TEXT_INSTALLING_SUBTITLE "Prego attendere mentre $(^NameDA) viene installato."
  ${LangFileString} MUI_TEXT_FINISH_TITLE "Installazione completata"
  ${LangFileString} MUI_TEXT_FINISH_SUBTITLE "L'installazione è stata completata con successo."
  ${LangFileString} MUI_TEXT_ABORT_TITLE "Installazione interrotta"
  ${LangFileString} MUI_TEXT_ABORT_SUBTITLE "L'installazione non è stata completata correttamente."
!endif

!ifdef MUI_UNINSTFILESPAGE
  ${LangFileString} MUI_UNTEXT_UNINSTALLING_TITLE "Disinstallazione in corso"
  ${LangFileString} MUI_UNTEXT_UNINSTALLING_SUBTITLE "Prego attendere mentre $(^NameDA) viene disinstallato."
  ${LangFileString} MUI_UNTEXT_FINISH_TITLE "Disinstallazione completata"
  ${LangFileString} MUI_UNTEXT_FINISH_SUBTITLE "La disinstallazione è stata completata con successo."
  ${LangFileString} MUI_UNTEXT_ABORT_TITLE "Disinstallazione interrotta"
  ${LangFileString} MUI_UNTEXT_ABORT_SUBTITLE "La disintallazione non è stata completata correttamente."
!endif

!ifdef MUI_FINISHPAGE
  ${LangFileString} MUI_TEXT_FINISH_INFO_TITLE "Completamento dell'installazione di $(^NameDA)."
  ${LangFileString} MUI_TEXT_FINISH_INFO_TEXT "$(^NameDA) è stato installato sul vostro computer.$\r$\n$\r$\nScegliere Fine per chiudere il programma di installazione."
  ${LangFileString} MUI_TEXT_FINISH_INFO_REBOOT "Il computer deve essere riavviato per completare l'installazione di $(^NameDA). Vuoi riavviarlo ora?"
!endif

!ifdef MUI_UNFINISHPAGE
  ${LangFileString} MUI_UNTEXT_FINISH_INFO_TITLE "Completamento della disinstallazione di $(^NameDA)"
  ${LangFileString} MUI_UNTEXT_FINISH_INFO_TEXT "$(^NameDA) è stato disinstallato dal computer.$\r$\n$\r$\nSelezionare Fine per terminare questa procedura."
  ${LangFileString} MUI_UNTEXT_FINISH_INFO_REBOOT "Il computer deve essere riavviato per completare l'installazione di $(^NameDA). Vuoi riavviarlo ora?"
!endif

!ifdef MUI_FINISHPAGE | MUI_UNFINISHPAGE
  ${LangFileString} MUI_TEXT_FINISH_REBOOTNOW "Riavvia adesso"
  ${LangFileString} MUI_TEXT_FINISH_REBOOTLATER "Preferisco riavviarlo manualmente più tardi"
  ${LangFileString} MUI_TEXT_FINISH_RUN "Esegui $(^NameDA)"
  ${LangFileString} MUI_TEXT_FINISH_SHOWREADME "Mostra il file Readme"
  ${LangFileString} MUI_BUTTONTEXT_FINISH "&Fine"
!endif

!ifdef MUI_STARTMENUPAGE
  ${LangFileString} MUI_TEXT_STARTMENU_TITLE "Scelta della cartella del menu Start"
  ${LangFileString} MUI_TEXT_STARTMENU_SUBTITLE "Scegliere una cartella del menu Start per i collegamenti del programma."
  ${LangFileString} MUI_INNERTEXT_STARTMENU_TOP "Scegliere la cartella del menu Start in cui verranno creati i collegamenti del programma. È possibile inserire un nome per creare una nuova cartella."
  ${LangFileString} MUI_INNERTEXT_STARTMENU_CHECKBOX "Non creare i collegamenti al programma."
!endif

!ifdef MUI_UNCONFIRMPAGE
  ${LangFileString} MUI_UNTEXT_CONFIRM_TITLE "Disinstalla $(^NameDA)"
  ${LangFileString} MUI_UNTEXT_CONFIRM_SUBTITLE "Rimuove $(^NameDA) dal computer."
!endif

!ifdef MUI_ABORTWARNING
  ${LangFileString} MUI_TEXT_ABORTWARNING "Sei sicuro di voler interrompere l'installazione di $(^Name) ?"
!endif

!ifdef MUI_UNABORTWARNING
  ${LangFileString} MUI_UNTEXT_ABORTWARNING "Sei sicuro di voler interrompere la disinstallazione di $(^Name)?"
!endif

!ifdef MULTIUSER_INSTALLMODEPAGE
  ${LangFileString} MULTIUSER_TEXT_INSTALLMODE_TITLE "Scelta degli Utenti"
  ${LangFileString} MULTIUSER_TEXT_INSTALLMODE_SUBTITLE "Scegliete per quali utenti volete installare $(^NameDA)."
  ${LangFileString} MULTIUSER_INNERTEXT_INSTALLMODE_TOP "Scegliete se volete installare $(^NameDA) solo per voi o per tutti gli utenti di questo sistema. $(^ClickNext)"
  ${LangFileString} MULTIUSER_INNERTEXT_INSTALLMODE_ALLUSERS "Installazione per tutti gli utenti"
  ${LangFileString} MULTIUSER_INNERTEXT_INSTALLMODE_CURRENTUSER "Installazione personale"
!endif
