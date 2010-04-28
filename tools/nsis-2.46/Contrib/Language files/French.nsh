;Language: French (1036)
;By Sébastien Delahaye <seb@delahaye.net>

!insertmacro LANGFILE "French" "French"

!ifdef MUI_WELCOMEPAGE
  ${LangFileString} MUI_TEXT_WELCOME_INFO_TITLE "Bienvenue dans le programme d'installation de $(^NameDA)"
  ${LangFileString} MUI_TEXT_WELCOME_INFO_TEXT "Vous êtes sur le point d'installer $(^NameDA) sur votre ordinateur.$\r$\n$\r$\nAvant de démarrer l'installation, il est recommandé de fermer toutes les autres applications. Cela permettra la mise à jour de certains fichiers système sans redémarrer votre ordinateur.$\r$\n$\r$\n$_CLICK"
!endif

!ifdef MUI_UNWELCOMEPAGE
  ${LangFileString} MUI_UNTEXT_WELCOME_INFO_TITLE "Bienvenue dans le programme de désinstallation de $(^NameDA)"
  ${LangFileString} MUI_UNTEXT_WELCOME_INFO_TEXT "Vous êtes sur le point de désinstaller $(^NameDA) de votre ordinateur.$\r$\n$\r$\nAvant d'amorcer la désinstallation, assurez-vous que $(^NameDA) ne soit pas en cours d'exécution.$\r$\n$\r$\n$_CLICK"
!endif

!ifdef MUI_LICENSEPAGE
  ${LangFileString} MUI_TEXT_LICENSE_TITLE "Licence utilisateur"
  ${LangFileString} MUI_TEXT_LICENSE_SUBTITLE "Veuillez examiner les termes de la licence avant d'installer $(^NameDA)."
  ${LangFileString} MUI_INNERTEXT_LICENSE_BOTTOM "Si vous acceptez les conditions de la licence utilisateur, cliquez sur J'accepte pour continuer. Vous devez accepter la licence utilisateur afin d'installer $(^NameDA)."
  ${LangFileString} MUI_INNERTEXT_LICENSE_BOTTOM_CHECKBOX "Si vous acceptez les conditions de la licence utilisateur, cochez la case ci-dessous. Vous devez accepter la licence utilisateur afin d'installer $(^NameDA)."
  ${LangFileString} MUI_INNERTEXT_LICENSE_BOTTOM_RADIOBUTTONS "Si vous acceptez les conditions de la licence utilisateur, sélectionnez le premier choix ci-dessous. Vous devez accepter la licence utilisateur afin d'installer $(^NameDA)."
!endif

!ifdef MUI_UNLICENSEPAGE
  ${LangFileString} MUI_UNTEXT_LICENSE_TITLE "Licence utilisateur"
  ${LangFileString} MUI_UNTEXT_LICENSE_SUBTITLE "Veuillez examiner les conditions de la licence avant de désinstaller $(^NameDA)."
  ${LangFileString} MUI_UNINNERTEXT_LICENSE_BOTTOM "Si vous acceptez les conditions de la licence utilisateur, cliquez sur J'accepte pour continuer. Vous devez accepter la licence utilisateur afin de désinstaller $(^NameDA)."
  ${LangFileString} MUI_UNINNERTEXT_LICENSE_BOTTOM_CHECKBOX "Si vous acceptez les conditions de la licence utilisateur, cochez la case ci-dessous. Vous devez accepter la licence utilisateur afin de désintaller $(^NameDA)."
  ${LangFileString} MUI_UNINNERTEXT_LICENSE_BOTTOM_RADIOBUTTONS "Si vous acceptez les conditions de la licence utilisateur, sélectionnez le premier choix ci-dessous. Vous devez accepter la licence utilisateur afin de désinstaller $(^NameDA)."
!endif

!ifdef MUI_LICENSEPAGE | MUI_UNLICENSEPAGE
  ${LangFileString} MUI_INNERTEXT_LICENSE_TOP "Appuyez sur Page Suivante pour lire le reste de la licence utilisateur."
!endif

!ifdef MUI_COMPONENTSPAGE
  ${LangFileString} MUI_TEXT_COMPONENTS_TITLE "Choisissez les composants"
  ${LangFileString} MUI_TEXT_COMPONENTS_SUBTITLE "Choisissez les composants de $(^NameDA) que vous souhaitez installer."
!endif

!ifdef MUI_UNCOMPONENTSPAGE
  ${LangFileString} MUI_UNTEXT_COMPONENTS_TITLE "Choisissez les composants"
  ${LangFileString} MUI_UNTEXT_COMPONENTS_SUBTITLE "Cochez les composants de $(^NameDA) que vous souhaitez désinstaller."
!endif

!ifdef MUI_COMPONENTSPAGE | MUI_UNCOMPONENTSPAGE
  ${LangFileString} MUI_INNERTEXT_COMPONENTS_DESCRIPTION_TITLE "Description"
  !ifndef NSIS_CONFIG_COMPONENTPAGE_ALTERNATIVE
    ${LangFileString} MUI_INNERTEXT_COMPONENTS_DESCRIPTION_INFO "Passez le curseur de votre souris sur un composant pour en voir la description."
  !else
    ${LangFileString} MUI_INNERTEXT_COMPONENTS_DESCRIPTION_INFO "Passez le curseur de votre souris sur un composant pour en voir la description."
  !endif
!endif

!ifdef MUI_DIRECTORYPAGE
  ${LangFileString} MUI_TEXT_DIRECTORY_TITLE "Choisissez le dossier d'installation"
  ${LangFileString} MUI_TEXT_DIRECTORY_SUBTITLE "Choisissez le dossier dans lequel installer $(^NameDA)."
!endif

!ifdef MUI_UNDIRECTORYPAGE
  ${LangFileString} MUI_UNTEXT_DIRECTORY_TITLE "Choisissez le dossier de désinstallation"
  ${LangFileString} MUI_UNTEXT_DIRECTORY_SUBTITLE "Choisissez le dossier à partir duquel vous voulez désinstaller $(^NameDA)."
!endif

!ifdef MUI_INSTFILESPAGE
  ${LangFileString} MUI_TEXT_INSTALLING_TITLE "Installation en cours"
  ${LangFileString} MUI_TEXT_INSTALLING_SUBTITLE "Veuillez patienter pendant que $(^NameDA) est en train d'être installé."
  ${LangFileString} MUI_TEXT_FINISH_TITLE "Installation terminée"
  ${LangFileString} MUI_TEXT_FINISH_SUBTITLE "L'installation s'est terminée avec succès."
  ${LangFileString} MUI_TEXT_ABORT_TITLE "Installation interrompue"
  ${LangFileString} MUI_TEXT_ABORT_SUBTITLE "L'installation n'a pas été terminée."
!endif

!ifdef MUI_UNINSTFILESPAGE
  ${LangFileString} MUI_UNTEXT_UNINSTALLING_TITLE "Désinstallation en cours"
  ${LangFileString} MUI_UNTEXT_UNINSTALLING_SUBTITLE "Veuillez patienter pendant que $(^NameDA) est en train d'être supprimé de votre ordinateur."
  ${LangFileString} MUI_UNTEXT_FINISH_TITLE "Désinstallation terminée"
  ${LangFileString} MUI_UNTEXT_FINISH_SUBTITLE "La désinstallation s'est terminée avec succès."
  ${LangFileString} MUI_UNTEXT_ABORT_TITLE "Désinstallation interrompue"
  ${LangFileString} MUI_UNTEXT_ABORT_SUBTITLE "La désinstallation n'a pas été terminée."
!endif

!ifdef MUI_FINISHPAGE
  ${LangFileString} MUI_TEXT_FINISH_INFO_TITLE "Fin de l'installation de $(^NameDA)"
  ${LangFileString} MUI_TEXT_FINISH_INFO_TEXT "$(^NameDA) a été installé sur votre ordinateur.$\r$\n$\r$\nCliquez sur Fermer pour quitter le programme d'installation."
  ${LangFileString} MUI_TEXT_FINISH_INFO_REBOOT "Votre ordinateur doit être redémarré afin de compléter l'installation de $(^NameDA). Souhaitez-vous redémarrer maintenant ?"
!endif

!ifdef MUI_UNFINISHPAGE
  ${LangFileString} MUI_UNTEXT_FINISH_INFO_TITLE "Fin de la désinstallation de $(^NameDA)"
  ${LangFileString} MUI_UNTEXT_FINISH_INFO_TEXT "$(^NameDA) a été supprimé de votre ordinateur.$\r$\n$\r$\nCliquez sur Fermer pour quitter le programme d'installation."
  ${LangFileString} MUI_UNTEXT_FINISH_INFO_REBOOT "Votre ordinateur doit être redémarré pour terminer la désinstallation de $(^NameDA). Souhaitez-vous redémarrer maintenant ?"
!endif

!ifdef MUI_FINISHPAGE | MUI_UNFINISHPAGE
  ${LangFileString} MUI_TEXT_FINISH_REBOOTNOW "Redémarrer maintenant"
  ${LangFileString} MUI_TEXT_FINISH_REBOOTLATER "Je souhaite redémarrer moi-même plus tard"
  ${LangFileString} MUI_TEXT_FINISH_RUN "Lancer $(^NameDA)"
  ${LangFileString} MUI_TEXT_FINISH_SHOWREADME "Afficher le fichier Readme"
  ${LangFileString} MUI_BUTTONTEXT_FINISH "&Fermer"  
!endif

!ifdef MUI_STARTMENUPAGE
  ${LangFileString} MUI_TEXT_STARTMENU_TITLE "Choisissez un dossier dans le menu Démarrer"
  ${LangFileString} MUI_TEXT_STARTMENU_SUBTITLE "Choisissez un dossier dans le menu Démarrer pour les raccourcis de l'application."
  ${LangFileString} MUI_INNERTEXT_STARTMENU_TOP "Choisissez le dossier du menu Démarrer dans lequel vous voulez placer les raccourcis du programme. Vous pouvez également entrer un nouveau nom pour créer un nouveau dossier."
  ${LangFileString} MUI_INNERTEXT_STARTMENU_CHECKBOX "Ne pas créer de raccourcis"
!endif

!ifdef MUI_UNCONFIRMPAGE
  ${LangFileString} MUI_UNTEXT_CONFIRM_TITLE "Désinstaller $(^NameDA)"
  ${LangFileString} MUI_UNTEXT_CONFIRM_SUBTITLE "Supprimer $(^NameDA) de votre ordinateur."
!endif

!ifdef MUI_ABORTWARNING
  ${LangFileString} MUI_TEXT_ABORTWARNING "Êtes-vous sûr de vouloir quitter l'installation de $(^Name) ?"
!endif

!ifdef MUI_UNABORTWARNING
  ${LangFileString} MUI_UNTEXT_ABORTWARNING "Êtes-vous sûr de vouloir quitter la désinstallation de $(^Name) ?"
!endif

!ifdef MULTIUSER_INSTALLMODEPAGE
  ${LangFileString} MULTIUSER_TEXT_INSTALLMODE_TITLE "Choix des utilisateurs"
  ${LangFileString} MULTIUSER_TEXT_INSTALLMODE_SUBTITLE "Sélection des utilisateurs désirant utiliser $(^NameDA)."
  ${LangFileString} MULTIUSER_INNERTEXT_INSTALLMODE_TOP "Choix entre installer $(^NameDA) seulement pour vous-même  ou bien pour tous les utilisateurs du système. $(^ClickNext)"
  ${LangFileString} MULTIUSER_INNERTEXT_INSTALLMODE_ALLUSERS "Installer pour tous les utilisateurs"
  ${LangFileString} MULTIUSER_INNERTEXT_INSTALLMODE_CURRENTUSER "Installer seulement pour moi"
!endif
