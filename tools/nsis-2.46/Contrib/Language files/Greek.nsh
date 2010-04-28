;Language: Greek (1032)
;By Makidis N. Michael - http://dias.aueb.gr/~p3010094/

!insertmacro LANGFILE "Greek" "Greek"

!ifdef MUI_WELCOMEPAGE
  ${LangFileString} MUI_TEXT_WELCOME_INFO_TITLE "Καλώς ήλθατε στην Εγκατάσταση του '$(^NameDA)'"
  ${LangFileString} MUI_TEXT_WELCOME_INFO_TEXT "Ο οδηγός αυτός θα σας καθοδηγήσει κατά τη διάρκεια της εγκατάστασης του '$(^NameDA)'.$\r$\n$\r$\nΣυνιστάται να κλείσετε όλες τις άλλες εφαρμογές πριν ξεκινήσετε την Εγκατάσταση. Αυτό θα επιτρέψει στην Εγκατάσταση να ενημερώσει τα σχετικά αρχεία συστήματος χωρίς την επανεκκίνηση του υπολογιστή σας.$\r$\n$\r$\n$_CLICK"
!endif

!ifdef MUI_UNWELCOMEPAGE
  ${LangFileString} MUI_UNTEXT_WELCOME_INFO_TITLE "Καλώς ήλθατε στον οδηγό απεγκατ. του '$(^NameDA)'"
  ${LangFileString} MUI_UNTEXT_WELCOME_INFO_TEXT "Ο οδηγός αυτός θα σας καθοδηγήσει κατά τη διάρκεια της απεγκατάστασης του '$(^NameDA)'.$\r$\n$\r$\nΠριν ξεκινήσετε την απεγκατάσταση, βεβαιωθείτε ότι το '$(^NameDA)' δεν τρέχει.$\r$\n$\r$\n$_CLICK"
!endif

!ifdef MUI_LICENSEPAGE
  ${LangFileString} MUI_TEXT_LICENSE_TITLE "Συμφωνία ’δειας Χρήσης"
  ${LangFileString} MUI_TEXT_LICENSE_SUBTITLE "Ελέγξτε την άδεια χρήσης πριν εγκαταστήσετε το '$(^NameDA)'."
  ${LangFileString} MUI_INNERTEXT_LICENSE_BOTTOM "Εάν αποδέχεστε τους όρους της άδειας χρήσης, κάντε κλικ στο Συμφωνώ για να συνεχίσετε. Πρέπει να αποδεχθείτε τη συμφωνία για να εγκαταστήσετε το '$(^NameDA)'."
  ${LangFileString} MUI_INNERTEXT_LICENSE_BOTTOM_CHECKBOX "Εάν αποδέχεστε τους όρους της άδειας χρήσης, κάντε κλικ στην επιλογή παρακάτω. Πρέπει να αποδεχθείτε τη συμφωνία για να εγκαταστήσετε το '$(^NameDA)'. $_CLICK"
  ${LangFileString} MUI_INNERTEXT_LICENSE_BOTTOM_RADIOBUTTONS "Εάν αποδέχεστε τους όρους της άδειας χρήσης, κάντε κλικ στην πρώτη επιλογή παρακάτω. Πρέπει να αποδεχθείτε τη συμφωνία για να εγκαταστήσετε το '$(^NameDA)'. $_CLICK"
!endif

!ifdef MUI_UNLICENSEPAGE
  ${LangFileString} MUI_UNTEXT_LICENSE_TITLE "Συμφωνία ’δειας Χρήσης"
  ${LangFileString} MUI_UNTEXT_LICENSE_SUBTITLE "Ελέγξτε την άδεια χρήσης πριν απεγκαταστήσετε το '$(^NameDA)'."
  ${LangFileString} MUI_UNINNERTEXT_LICENSE_BOTTOM "Εάν αποδέχεστε τους όρους της άδειας χρήσης, κάντε κλικ στο Συμφωνώ για να συνεχίσετε. Πρέπει να αποδεχθείτε τη συμφωνία για να απεγκαταστήσετε το '$(^NameDA)'."
  ${LangFileString} MUI_UNINNERTEXT_LICENSE_BOTTOM_CHECKBOX "Εάν αποδέχεστε τους όρους της άδειας χρήσης, κάντε κλικ στην επιλογή παρακάτω. Πρέπει να αποδεχθείτε τη συμφωνία για να απεγκαταστήσετε το '$(^NameDA)'. $_CLICK"
  ${LangFileString} MUI_UNINNERTEXT_LICENSE_BOTTOM_RADIOBUTTONS "Εάν αποδέχεστε τους όρους της άδειας χρήσης, κάντε κλικ στην πρώτη επιλογή παρακάτω. Πρέπει να αποδεχθείτε τη συμφωνία για να απεγκαταστήσετε το '$(^NameDA)'. $_CLICK"
!endif

!ifdef MUI_LICENSEPAGE | MUI_UNLICENSEPAGE
  ${LangFileString} MUI_INNERTEXT_LICENSE_TOP "Πατήστε το Page Down για να δείτε το υπόλοιπο της άδειας χρήσης."
!endif

!ifdef MUI_COMPONENTSPAGE
  ${LangFileString} MUI_TEXT_COMPONENTS_TITLE "Επιλογή Στοιχείων"
  ${LangFileString} MUI_TEXT_COMPONENTS_SUBTITLE "Επιλέξτε τα στοιχεία του '$(^NameDA)' που θέλετε να εγκαταστήσετε."
!endif

!ifdef MUI_UNCOMPONENTSPAGE
  ${LangFileString} MUI_UNTEXT_COMPONENTS_TITLE "Επιλογή Στοιχείων"
  ${LangFileString} MUI_UNTEXT_COMPONENTS_SUBTITLE "Επιλέξτε τα στοιχεία του '$(^NameDA)' που θέλετε να απεγκαταστήσετε."
!endif

!ifdef MUI_COMPONENTSPAGE | MUI_UNCOMPONENTSPAGE
  ${LangFileString} MUI_INNERTEXT_COMPONENTS_DESCRIPTION_TITLE "Περιγραφή"
  !ifndef NSIS_CONFIG_COMPONENTPAGE_ALTERNATIVE
    ${LangFileString} MUI_INNERTEXT_COMPONENTS_DESCRIPTION_INFO "Περάστε το δείκτη του ποντικιού πάνω από ένα στοιχείο για να δείτε την περιγραφή του."
  !else
    ${LangFileString} MUI_INNERTEXT_COMPONENTS_DESCRIPTION_INFO "Περάστε το δείκτη του ποντικιού πάνω από ένα στοιχείο για να δείτε την περιγραφή του."
  !endif
!endif

!ifdef MUI_DIRECTORYPAGE
  ${LangFileString} MUI_TEXT_DIRECTORY_TITLE "Επιλογή Θέσης Εγκατάστασης"
  ${LangFileString} MUI_TEXT_DIRECTORY_SUBTITLE "Επιλέξτε το φάκελο μέσα στον οποίο θα εγκατασταθεί το '$(^NameDA)'."
!endif

!ifdef MUI_UNDIRECTORYPAGE
  ${LangFileString} MUI_UNTEXT_DIRECTORY_TITLE "Επιλογή Θέσης Απεγκατάστασης"
  ${LangFileString} MUI_UNTEXT_DIRECTORY_SUBTITLE "Επιλέξτε το φάκελο από τον οποίο θα απεγκατασταθεί το '$(^NameDA)'."
!endif

!ifdef MUI_INSTFILESPAGE
  ${LangFileString} MUI_TEXT_INSTALLING_TITLE "Εγκατάσταση Σε Εξέλιξη"
  ${LangFileString} MUI_TEXT_INSTALLING_SUBTITLE "Παρακαλώ περιμένετε όσο το '$(^NameDA)' εγκαθίσταται."
  ${LangFileString} MUI_TEXT_FINISH_TITLE "Η Εγκατάσταση Ολοκληρώθηκε"
  ${LangFileString} MUI_TEXT_FINISH_SUBTITLE "Η εγκατάσταση ολοκληρώθηκε επιτυχώς."
  ${LangFileString} MUI_TEXT_ABORT_TITLE "Η Εγκατάσταση Διακόπηκε"
  ${LangFileString} MUI_TEXT_ABORT_SUBTITLE "Η εγκατάσταση δεν ολοκληρώθηκε επιτυχώς."
!endif

!ifdef MUI_UNINSTFILESPAGE
  ${LangFileString} MUI_UNTEXT_UNINSTALLING_TITLE "Απεγκατάσταση Σε Εξέλιξη"
  ${LangFileString} MUI_UNTEXT_UNINSTALLING_SUBTITLE "Παρακαλώ περιμένετε όσο το '$(^NameDA)' απεγκαθίσταται."
  ${LangFileString} MUI_UNTEXT_FINISH_TITLE "Η Απεγκατάσταση Ολοκληρώθηκε"
  ${LangFileString} MUI_UNTEXT_FINISH_SUBTITLE "Η απεγκατάσταση ολοκληρώθηκε επιτυχώς."
  ${LangFileString} MUI_UNTEXT_ABORT_TITLE "Η Απεγκατάσταση Διακόπηκε"
  ${LangFileString} MUI_UNTEXT_ABORT_SUBTITLE "Η απεγκατάσταση δεν ολοκληρώθηκε επιτυχώς."
!endif

!ifdef MUI_FINISHPAGE
  ${LangFileString} MUI_TEXT_FINISH_INFO_TITLE "Ολοκλήρωση της Εγκατάστασης του '$(^NameDA)'"
  ${LangFileString} MUI_TEXT_FINISH_INFO_TEXT "Το '$(^NameDA)' εγκαταστάθηκε στον υπολογιστή σας.$\r$\n$\r$\nΚάντε κλικ στο Τέλος για να κλείσετε αυτόν τον οδηγό."
  ${LangFileString} MUI_TEXT_FINISH_INFO_REBOOT "Πρέπει να γίνει επανεκκίνηση του υπολογιστή σας για να ολοκληρωθεί η εγκατάσταση του '$(^NameDA)'. Θέλετε να επανεκκινήσετε τον υπολογιστή σας τώρα;"
!endif

!ifdef MUI_UNFINISHPAGE
  ${LangFileString} MUI_UNTEXT_FINISH_INFO_TITLE "Ολοκλήρωση της Απεγκατάστασης του '$(^NameDA)'"
  ${LangFileString} MUI_UNTEXT_FINISH_INFO_TEXT "Το '$(^NameDA)' απεγκαταστάθηκε από τον υπολογιστή σας.$\r$\n$\r$\nΚάντε κλικ στο Τέλος για να κλείσετε αυτόν τον οδηγό."
  ${LangFileString} MUI_UNTEXT_FINISH_INFO_REBOOT "Πρέπει να γίνει επανεκκίνηση του υπολογιστή σας για να ολοκληρωθεί η απεγκατάσταση του '$(^NameDA)'. Θέλετε να επανεκκινήσετε τον υπολογιστή σας τώρα;"
!endif

!ifdef MUI_FINISHPAGE | MUI_UNFINISHPAGE
  ${LangFileString} MUI_TEXT_FINISH_REBOOTNOW "Να γίνει επανεκκίνηση τώρα"
  ${LangFileString} MUI_TEXT_FINISH_REBOOTLATER "Θα επανεκκινήσω τον υπολογιστή μου αργότερα"
  ${LangFileString} MUI_TEXT_FINISH_RUN "&Εκτέλεση του '$(^NameDA)'"
  ${LangFileString} MUI_TEXT_FINISH_SHOWREADME "Εμφάνιση του &αρχείου Readme"
  ${LangFileString} MUI_BUTTONTEXT_FINISH "&Τέλος"  
!endif

!ifdef MUI_STARTMENUPAGE
  ${LangFileString} MUI_TEXT_STARTMENU_TITLE "Επιλογή Φακέλου για το Μενού Έναρξη"
  ${LangFileString} MUI_TEXT_STARTMENU_SUBTITLE "Επιλέξτε ένα φάκελο του μενού Έναρξη για τις συντομεύσεις του '$(^NameDA)'."
  ${LangFileString} MUI_INNERTEXT_STARTMENU_TOP "Επιλέξτε ένα φάκελο του μενού Έναρξη για τις συντομεύσεις του προγράμματος. Μπορείτε επίσης να εισάγετε ένα όνομα για να δημιουργήσετε ένα νέο φάκελο."
  ${LangFileString} MUI_INNERTEXT_STARTMENU_CHECKBOX "Να μη δημιουργηθούν συντομεύσεις"
!endif

!ifdef MUI_UNCONFIRMPAGE
  ${LangFileString} MUI_UNTEXT_CONFIRM_TITLE "Απεγκατάσταση του '$(^NameDA)'"
  ${LangFileString} MUI_UNTEXT_CONFIRM_SUBTITLE "Αφαίρεση του '$(^NameDA)' από τον υπολογιστή σας."
!endif

!ifdef MUI_ABORTWARNING
  ${LangFileString} MUI_TEXT_ABORTWARNING "Είστε σίγουροι πως θέλετε να τερματίσετε την εγκατάσταση του '$(^Name)';"
!endif

!ifdef MUI_UNABORTWARNING
  ${LangFileString} MUI_UNTEXT_ABORTWARNING "Είστε σίγουροι πως θέλετε να τερματίσετε την απεγκατάσταση του '$(^Name)';"
!endif
