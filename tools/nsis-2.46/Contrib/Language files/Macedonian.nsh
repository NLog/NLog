;Language: Macedonian (1071)
;By Sasko Zdravkin [wingman2083@yahoo.com]

!insertmacro LANGFILE "Macedonian" "Macedonian"

!ifdef MUI_WELCOMEPAGE
  ${LangFileString} MUI_TEXT_WELCOME_INFO_TITLE "Добро дојдовте во инсталацијата на $(^NameDA)"
  ${LangFileString} MUI_TEXT_WELCOME_INFO_TEXT "Овој програм ќе ве води низ инсталацијата на $(^NameDA).$\r$\n$\r$\nПрепорачано е да ги затворите сите програми пред да инсталирате. Ова ќе дозволи инсталациониот програм да обнови некои системски датотеки без да го рестартира компјутерот.$\r$\n$\r$\n$_CLICK"
!endif

!ifdef MUI_UNWELCOMEPAGE
  ${LangFileString} MUI_UNTEXT_WELCOME_INFO_TITLE "Добро дојдовте во деинсталацијата на $(^NameDA)"
  ${LangFileString} MUI_UNTEXT_WELCOME_INFO_TEXT "Овој програм ќе ве води низ деинсталацијата на $(^NameDA).$\r$\n$\r$\nПред да ја почнете деинсталацијата на $(^NameDA) проверете дали е исклучена програмата.$\r$\n$\r$\n$_CLICK"
!endif

!ifdef MUI_LICENSEPAGE
  ${LangFileString} MUI_TEXT_LICENSE_TITLE "Лиценцен Договор"
  ${LangFileString} MUI_TEXT_LICENSE_SUBTITLE "Ве молиме проверете ги лиценцните услови пред да го инсталирате $(^NameDA)."
  ${LangFileString} MUI_INNERTEXT_LICENSE_BOTTOM "Ако ги прифаќате условите од договорот, притиснете 'Да' за да продолжите. Мора да го прифатите договорот за да го инсталирате $(^NameDA)."
  ${LangFileString} MUI_INNERTEXT_LICENSE_BOTTOM_CHECKBOX "Ако ги прифаќате условите од договорот, чекирајте го check box-от подоле. Мора да го прифатите договорот за го инсталирате $(^NameDA). $_CLICK"
  ${LangFileString} MUI_INNERTEXT_LICENSE_BOTTOM_RADIOBUTTONS "Ако ги прифаќате условите од договорот, одберете ја првата опција подоле. Мора да го прифатите договорот за го инсталирате $(^NameDA). $_CLICK"
!endif

!ifdef MUI_UNLICENSEPAGE
  ${LangFileString} MUI_UNTEXT_LICENSE_TITLE "Лиценцен Договор"
  ${LangFileString} MUI_UNTEXT_LICENSE_SUBTITLE "Ве молиме проверете ги лиценцните услови пред да го деинсталирате $(^NameDA)."
  ${LangFileString} MUI_UNINNERTEXT_LICENSE_BOTTOM "Ако ги прифаќате условите од договорот, притиснете 'Да' за да продолжите. Мора да го прифатите договорот за да го деинсталирате $(^NameDA)."
  ${LangFileString} MUI_UNINNERTEXT_LICENSE_BOTTOM_CHECKBOX "Ако ги прифаќате условите од договорот, чекирајте го check box-от подоле. Мора да го прифатите договорот за го деинсталирате $(^NameDA). $_CLICK"
  ${LangFileString} MUI_UNINNERTEXT_LICENSE_BOTTOM_RADIOBUTTONS "Ако ги прифаќате условите од договорот, одберете ја првата опција подоле. Мора да го прифатите договорот за го деинсталирате $(^NameDA). $_CLICK"
!endif

!ifdef MUI_LICENSEPAGE | MUI_UNLICENSEPAGE
  ${LangFileString} MUI_INNERTEXT_LICENSE_TOP "Притиснете 'Page Down' за да го видете останатиот дел од договорот."
!endif

!ifdef MUI_COMPONENTSPAGE
  ${LangFileString} MUI_TEXT_COMPONENTS_TITLE "Одберете Компоненти"
  ${LangFileString} MUI_TEXT_COMPONENTS_SUBTITLE "Одберете кои работи од $(^NameDA) сакате да се инсталираат."
!endif

!ifdef MUI_UNCOMPONENTSPAGE
  ${LangFileString} MUI_UNTEXT_COMPONENTS_TITLE "Одберете Компоненти"
  ${LangFileString} MUI_UNTEXT_COMPONENTS_SUBTITLE "Одберете кои работи од $(^NameDA) сакате да се деинсталираат."
!endif

!ifdef MUI_COMPONENTSPAGE | MUI_UNCOMPONENTSPAGE
  ${LangFileString} MUI_INNERTEXT_COMPONENTS_DESCRIPTION_TITLE "Објаснение"
  !ifndef NSIS_CONFIG_COMPONENTPAGE_ALTERNATIVE
    ${LangFileString} MUI_INNERTEXT_COMPONENTS_DESCRIPTION_INFO "Однесете го курсорот до компонентата за да го видете нејзиното објаснение."
  !else
    ${LangFileString} MUI_INNERTEXT_COMPONENTS_DESCRIPTION_INFO "Однесете го курсорот до компонентата за да го видете нејзиното објаснение."
  !endif
!endif

!ifdef MUI_DIRECTORYPAGE
  ${LangFileString} MUI_TEXT_DIRECTORY_TITLE "Одберете ја локацијата за инсталирање"
  ${LangFileString} MUI_TEXT_DIRECTORY_SUBTITLE "Одберете го директориумот каде што сакате да се инсталира $(^NameDA)."
!endif

!ifdef MUI_UNDIRECTORYPAGE
  ${LangFileString} MUI_UNTEXT_DIRECTORY_TITLE "Одберете ја локацијата за деинсталирање"
  ${LangFileString} MUI_UNTEXT_DIRECTORY_SUBTITLE "Одберете го директориумот од кој сакате да се деинсталира $(^NameDA)."
!endif

!ifdef MUI_INSTFILESPAGE
  ${LangFileString} MUI_TEXT_INSTALLING_TITLE "Инсталира"
  ${LangFileString} MUI_TEXT_INSTALLING_SUBTITLE "Ве молиме почекајте додека $(^NameDA) се инсталира."
  ${LangFileString} MUI_TEXT_FINISH_TITLE "Инсталацијата е завршена"
  ${LangFileString} MUI_TEXT_FINISH_SUBTITLE "Инсталирањето беше успешно."
  ${LangFileString} MUI_TEXT_ABORT_TITLE "Инсталацијата е откажана"
  ${LangFileString} MUI_TEXT_ABORT_SUBTITLE "Инсталирањето не беше успешно завршено."
!endif

!ifdef MUI_UNINSTFILESPAGE
  ${LangFileString} MUI_UNTEXT_UNINSTALLING_TITLE "Деинсталира"
  ${LangFileString} MUI_UNTEXT_UNINSTALLING_SUBTITLE "Ве молиме почекајте додека $(^NameDA) се деинсталира."
  ${LangFileString} MUI_UNTEXT_FINISH_TITLE "Деинсталацијата е завршена"
  ${LangFileString} MUI_UNTEXT_FINISH_SUBTITLE "Деинсталирањето беше успешно."
  ${LangFileString} MUI_UNTEXT_ABORT_TITLE "Деинсталацијата е откажана"
  ${LangFileString} MUI_UNTEXT_ABORT_SUBTITLE "Деинсталирањето не беше успешно завршено."
!endif

!ifdef MUI_FINISHPAGE
  ${LangFileString} MUI_TEXT_FINISH_INFO_TITLE "Завршува инсталирањето на $(^NameDA)"
  ${LangFileString} MUI_TEXT_FINISH_INFO_TEXT "$(^NameDA) е инсталиран на вашиот компјутер.$\r$\n$\r$\nПритиснете 'Крај' за да го затворите инсталациониот програм."
  ${LangFileString} MUI_TEXT_FINISH_INFO_REBOOT "Вашиот компјутер мора да се рестартира за да заврши инсталацијата на $(^NameDA). Дали сакате да се рестартира сега?"
!endif

!ifdef MUI_UNFINISHPAGE
  ${LangFileString} MUI_UNTEXT_FINISH_INFO_TITLE "Завршува деинсталирањето на $(^NameDA)"
  ${LangFileString} MUI_UNTEXT_FINISH_INFO_TEXT "$(^NameDA) е деинсталиран од вашиот компјутер.$\r$\n$\r$\nПритиснете 'Крај' за да го затворите деинсталациониот програм."
  ${LangFileString} MUI_UNTEXT_FINISH_INFO_REBOOT "Вашиот компјутер мора да се рестартира за да заврши деинсталацијата на $(^NameDA). Дали сакате да се рестартира сега?"
!endif

!ifdef MUI_FINISHPAGE | MUI_UNFINISHPAGE
  ${LangFileString} MUI_TEXT_FINISH_REBOOTNOW "Рестартирај сега"
  ${LangFileString} MUI_TEXT_FINISH_REBOOTLATER "Ако сакате да го рестартирате подоцна"
  ${LangFileString} MUI_TEXT_FINISH_RUN "Пок&рени го $(^NameDA)"
  ${LangFileString} MUI_TEXT_FINISH_SHOWREADME "Отвор&и 'Прочитај Ме'"
  ${LangFileString} MUI_BUTTONTEXT_FINISH "&Крај"  
!endif

!ifdef MUI_STARTMENUPAGE
  ${LangFileString} MUI_TEXT_STARTMENU_TITLE "Одберете директориум за Старт Менито"
  ${LangFileString} MUI_TEXT_STARTMENU_SUBTITLE "Одберете директориум во Старт Менито за креирање скратеница на $(^NameDA)."
  ${LangFileString} MUI_INNERTEXT_STARTMENU_TOP "Одберете го директориумот во Старт Менито во кој сакате да се креира скратеница за програмата. Исто така можете да внесете друго име за да се креира нов директориум."
  ${LangFileString} MUI_INNERTEXT_STARTMENU_CHECKBOX "Не креирај скратеница"
!endif

!ifdef MUI_UNCONFIRMPAGE
  ${LangFileString} MUI_UNTEXT_CONFIRM_TITLE "Деинсталирај го $(^NameDA)"
  ${LangFileString} MUI_UNTEXT_CONFIRM_SUBTITLE "Одстранете го $(^NameDA) од вашиот компјутер."
!endif

!ifdef MUI_ABORTWARNING
  ${LangFileString} MUI_TEXT_ABORTWARNING "Сигурни ли сте дека сакате да се откажете од инсталацијата на $(^Name)?"
!endif

!ifdef MUI_UNABORTWARNING
  ${LangFileString} MUI_UNTEXT_ABORTWARNING "Сигурни ли сте дека сакате да се откажете од деинсталацијата на $(^Name)?"
!endif
