;Language: Bulgarian (1026)
;Translated by Asparouh Kalyandjiev [acnapyx@sbline.net]
;Review and update from v1.63 to v1.68 by Plamen Penkov [plamen71@hotmail.com]
;Updated by Кирил Кирилов (DumpeR) [dumper@data.bg]
;

!insertmacro LANGFILE "Bulgarian" "Bulgarian"

!ifdef MUI_WELCOMEPAGE
  ${LangFileString} MUI_TEXT_WELCOME_INFO_TITLE "Добре дошли в Съветника за инсталиране на $(^NameDA)!"
  ${LangFileString} MUI_TEXT_WELCOME_INFO_TEXT "Той ще инсталира $(^NameDA) на вашия компютър.$\r$\n$\r$\nПрепоръчва се да затворите всички други приложения, преди да продължите. Това ще позволи на програмата да обнови някои системни файлове, без да се рестартира компютъра.$\r$\n$\r$\n$_CLICK"
!endif

!ifdef MUI_UNWELCOMEPAGE
  ${LangFileString} MUI_UNTEXT_WELCOME_INFO_TITLE "Добре дошли в Съветника за изтриване на $(^NameDA)!"
  ${LangFileString} MUI_UNTEXT_WELCOME_INFO_TEXT "Той ще ви помогне да изтриете $(^NameDA) от вашия компютър.$\r$\n$\r$\nПреди да продължите, уверете се че $(^NameDA) не е стартирана в момента.$\r$\n$\r$\n$_CLICK"
!endif

!ifdef MUI_LICENSEPAGE
  ${LangFileString} MUI_TEXT_LICENSE_TITLE "Лицензионно споразумение"
  ${LangFileString} MUI_TEXT_LICENSE_SUBTITLE "Моля запознайте се Лицензионното споразумение преди да продължите."
  ${LangFileString} MUI_INNERTEXT_LICENSE_BOTTOM "Ако приемате условията на споразумението, натиснете $\"Съгласен$\", за да продължите. Трябва да приемете споразумението, за да инсталирате $(^NameDA)."
  ${LangFileString} MUI_INNERTEXT_LICENSE_BOTTOM_CHECKBOX "Ако приемате условията на споразумението, сложете отметка в полето по-долу. Трябва да приемете споразумението, за да инсталирате $(^NameDA). $_CLICK"
  ${LangFileString} MUI_INNERTEXT_LICENSE_BOTTOM_RADIOBUTTONS "Ако приемате условията на споразумението, изберете първата опция по-долу. Трябва да приемете споразумението, за да инсталирате $(^NameDA) $_CLICK"
!endif

!ifdef MUI_UNLICENSEPAGE
  ${LangFileString} MUI_UNTEXT_LICENSE_TITLE "Лицензионно споразумение"
  ${LangFileString} MUI_UNTEXT_LICENSE_SUBTITLE "Моля запознайте се лицензионните условия преди да изтриете $(^NameDA)."
  ${LangFileString} MUI_UNINNERTEXT_LICENSE_BOTTOM "Ако приемате условията на споразуменито, натиснете $\"Съгласен$\" за да продължите. Трябва да приемете споразумението, за да изтриете $(^NameDA). $_CLICK"
  ${LangFileString} MUI_UNINNERTEXT_LICENSE_BOTTOM_CHECKBOX "Ако приемате условията на споразумението, сложете отметка в полето по-долу. Трябва да приемете споразумението, за да изтриете $(^NameDA). $_CLICK"
  ${LangFileString} MUI_UNINNERTEXT_LICENSE_BOTTOM_RADIOBUTTONS "Ако приемате условията на споразуменито, изберете първата опция по-долу. Трябва да приемете споразумението, за да изтриете $(^NameDA). $_CLICK"
!endif

!ifdef MUI_LICENSEPAGE | MUI_UNLICENSEPAGE
  ${LangFileString} MUI_INNERTEXT_LICENSE_TOP "Натиснете клавиша $\"Page Down$\", за да видите останалата част от споразумението."
!endif

!ifdef MUI_COMPONENTSPAGE
  ${LangFileString} MUI_TEXT_COMPONENTS_TITLE "Избор на компоненти"
  ${LangFileString} MUI_TEXT_COMPONENTS_SUBTITLE "Изберете кои компоненти на $(^NameDA) искате да инсталирате."
!endif

!ifdef MUI_UNCOMPONENTSPAGE
  ${LangFileString} MUI_UNTEXT_COMPONENTS_TITLE "Избор на компоненти"
  ${LangFileString} MUI_UNTEXT_COMPONENTS_SUBTITLE "Изберете кои компоненти на $(^NameDA) искате да изтриете."
!endif

!ifdef MUI_COMPONENTSPAGE | MUI_UNCOMPONENTSPAGE
  ${LangFileString} MUI_INNERTEXT_COMPONENTS_DESCRIPTION_TITLE "Описание"
  !ifndef NSIS_CONFIG_COMPONENTPAGE_ALTERNATIVE
    ${LangFileString} MUI_INNERTEXT_COMPONENTS_DESCRIPTION_INFO "Преминете с мишката над определен компонент, за да видите описанието му."
  !else
    ${LangFileString} MUI_INNERTEXT_COMPONENTS_DESCRIPTION_INFO "Преминете с мишката над определен компонент, за да видите описанието му."
  !endif
!endif

!ifdef MUI_DIRECTORYPAGE
  ${LangFileString} MUI_TEXT_DIRECTORY_TITLE "Избор на папка за инсталиране"
  ${LangFileString} MUI_TEXT_DIRECTORY_SUBTITLE "Изберете папката, в която да се инсталира $(^NameDA)."
!endif

!ifdef MUI_UNDIRECTORYPAGE
  ${LangFileString} MUI_UNTEXT_DIRECTORY_TITLE "Избор на папка за изтриване"
  ${LangFileString} MUI_UNTEXT_DIRECTORY_SUBTITLE "Изберете папката, от която да се изтрие $(^NameDA)."
!endif

!ifdef MUI_INSTFILESPAGE
  ${LangFileString} MUI_TEXT_INSTALLING_TITLE "Инсталиране"
  ${LangFileString} MUI_TEXT_INSTALLING_SUBTITLE "Изчакайте, инсталират се файловете на $(^NameDA)..."
  ${LangFileString} MUI_TEXT_FINISH_TITLE "Инсталирането завърши."
  ${LangFileString} MUI_TEXT_FINISH_SUBTITLE "Инсталирането завърши успешно."
  ${LangFileString} MUI_TEXT_ABORT_TITLE "Инсталирането прекратено."
  ${LangFileString} MUI_TEXT_ABORT_SUBTITLE "Инсталирането не завърши успешно."
!endif

!ifdef MUI_UNINSTFILESPAGE
  ${LangFileString} MUI_UNTEXT_UNINSTALLING_TITLE "Изтриване"
  ${LangFileString} MUI_UNTEXT_UNINSTALLING_SUBTITLE "Изчакайте, изтриват се файловете на $(^NameDA)..."
  ${LangFileString} MUI_UNTEXT_FINISH_TITLE "Край"
  ${LangFileString} MUI_UNTEXT_FINISH_SUBTITLE "Изтриването завърши успешно."
  ${LangFileString} MUI_UNTEXT_ABORT_TITLE "Изтриването прекратено."
  ${LangFileString} MUI_UNTEXT_ABORT_SUBTITLE "Изтриването не завърши напълно."
!endif

!ifdef MUI_FINISHPAGE
  ${LangFileString} MUI_TEXT_FINISH_INFO_TITLE "Приключване на Съветника за инсталиране на $(^NameDA)"
  ${LangFileString} MUI_TEXT_FINISH_INFO_TEXT "Инсталирането на $(^NameDA) е завършено.$\r$\n$\r$\nНатиснете бутона $\"Край$\", за да затворите Съветника."
  ${LangFileString} MUI_TEXT_FINISH_INFO_REBOOT "Компютърът трябва да бъде рестартиран, за да завърши инсталирането на $(^NameDA). Искате ли да рестартирате сега?"
!endif

!ifdef MUI_UNFINISHPAGE
  ${LangFileString} MUI_UNTEXT_FINISH_INFO_TITLE "Приключване на Съветника за изтриване на $(^NameDA)"
  ${LangFileString} MUI_UNTEXT_FINISH_INFO_TEXT "Програмата $(^NameDA) беше изтрита от вашия компютър.$\r$\n$\r$\nНатиснете $\"Край$\" за да затворите този Съветник."
  ${LangFileString} MUI_UNTEXT_FINISH_INFO_REBOOT "Компютърът ви трябва да се рестартира, за да приключи успешно изтриването на $(^NameDA). Искате ли да рестартирате сега?"
!endif

!ifdef MUI_FINISHPAGE | MUI_UNFINISHPAGE
  ${LangFileString} MUI_TEXT_FINISH_REBOOTNOW "Да, рестартирай сега"
  ${LangFileString} MUI_TEXT_FINISH_REBOOTLATER "Не, ще рестартирам по-късно"
  ${LangFileString} MUI_TEXT_FINISH_RUN "Стартирай $(^NameDA)"
  ${LangFileString} MUI_TEXT_FINISH_SHOWREADME "Покажи файла $\"ReadMe$\""
  ${LangFileString} MUI_BUTTONTEXT_FINISH "&Край"  
!endif

!ifdef MUI_STARTMENUPAGE
  ${LangFileString} MUI_TEXT_STARTMENU_TITLE "Избор на папка в менюто $\"Старт$\""
  ${LangFileString} MUI_TEXT_STARTMENU_SUBTITLE "Изберете папка в менюто $\"Старт$\" за преки пътища към програмата."
  ${LangFileString} MUI_INNERTEXT_STARTMENU_TOP "Изберете папка в менюто $\"Старт$\", в която искате да създадете преки пътища към програмата. Можете също така да въведете име, за да създадете нова папка."
  ${LangFileString} MUI_INNERTEXT_STARTMENU_CHECKBOX "Не създавай преки пътища"
!endif

!ifdef MUI_UNCONFIRMPAGE
  ${LangFileString} MUI_UNTEXT_CONFIRM_TITLE "Изтриване на $(^NameDA)"
  ${LangFileString} MUI_UNTEXT_CONFIRM_SUBTITLE "Изтриване на $(^NameDA) от вашия компютър."
!endif

!ifdef MUI_ABORTWARNING
  ${LangFileString} MUI_TEXT_ABORTWARNING "Сигурни ли сте, че искате да прекратите инсталирането на $(^Name)?"
!endif

!ifdef MUI_UNABORTWARNING
  ${LangFileString} MUI_UNTEXT_ABORTWARNING "Сигурни ли сте, че искате да прекратите изтриването на $(^Name)?"
!endif
