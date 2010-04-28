;Language: Russian (1049)
;Translation updated by Dmitry Yerokhin [erodim@mail.ru] (050424)

!insertmacro LANGFILE "Russian" "Russian"

!ifdef MUI_WELCOMEPAGE
  ${LangFileString} MUI_TEXT_WELCOME_INFO_TITLE "Вас приветствует мастер установки $(^NameDA)"
  ${LangFileString} MUI_TEXT_WELCOME_INFO_TEXT "Эта программа установит $(^NameDA) на ваш компьютер.$\r$\n$\r$\nПеред началом установки рекомендуется закрыть все работающие приложения. Это позволит программе установки обновить системные файлы без перезагрузки компьютера.$\r$\n$\r$\n$_CLICK"
!endif

!ifdef MUI_UNWELCOMEPAGE
  ${LangFileString} MUI_UNTEXT_WELCOME_INFO_TITLE "Вас приветствует мастер удаления $(^NameDA)"
  ${LangFileString} MUI_UNTEXT_WELCOME_INFO_TEXT "Эта программа удалит $(^NameDA) из вашего компьютера.$\r$\n$\r$\nПеред началом удаления убедитесь, что программа $(^NameDA) не запущена.$\r$\n$\r$\n$_CLICK"
!endif

!ifdef MUI_LICENSEPAGE
  ${LangFileString} MUI_TEXT_LICENSE_TITLE "Лицензионное соглашение"
  ${LangFileString} MUI_TEXT_LICENSE_SUBTITLE "Перед установкой $(^NameDA) ознакомьтесь с лицензионным соглашением."
  ${LangFileString} MUI_INNERTEXT_LICENSE_BOTTOM "Если вы принимаете условия соглашения, нажмите кнопку $\"Принимаю$\". Чтобы установить программу, необходимо принять соглашение."
  ${LangFileString} MUI_INNERTEXT_LICENSE_BOTTOM_CHECKBOX "Если вы принимаете условия соглашения, установите флажок ниже. Чтобы установить программу, необходимо принять соглашение. $_CLICK"
  ${LangFileString} MUI_INNERTEXT_LICENSE_BOTTOM_RADIOBUTTONS "Если вы принимаете условия соглашения, выберите первый вариант из предложенных ниже. Чтобы установить программу, необходимо принять соглашение. $_CLICK"
!endif

!ifdef MUI_UNLICENSEPAGE
  ${LangFileString} MUI_UNTEXT_LICENSE_TITLE "Лицензионное соглашение"
  ${LangFileString} MUI_UNTEXT_LICENSE_SUBTITLE "Перед удалением $(^NameDA) ознакомьтесь с лицензионным соглашением."
  ${LangFileString} MUI_UNINNERTEXT_LICENSE_BOTTOM "Если вы принимаете условия соглашения, нажмите кнопку $\"Принимаю$\". Для удаления необходимо принять соглашение. $_CLICK"
  ${LangFileString} MUI_UNINNERTEXT_LICENSE_BOTTOM_CHECKBOX "Если вы принимаете условия соглашения, установите флажок ниже. Для удаления необходимо принять соглашение. $_CLICK"
  ${LangFileString} MUI_UNINNERTEXT_LICENSE_BOTTOM_RADIOBUTTONS "Если вы принимаете условия соглашения, выберите первый вариант из предложенных ниже. Для удаления необходимо принять соглашение. $_CLICK"
!endif

!ifdef MUI_LICENSEPAGE | MUI_UNLICENSEPAGE
  ${LangFileString} MUI_INNERTEXT_LICENSE_TOP "Для перемещения по тексту используйте клавиши $\"PageUp$\" и $\"PageDown$\"."
!endif

!ifdef MUI_COMPONENTSPAGE
  ${LangFileString} MUI_TEXT_COMPONENTS_TITLE "Компоненты устанавливаемой программы"
  ${LangFileString} MUI_TEXT_COMPONENTS_SUBTITLE "Выберите компоненты $(^NameDA), которые вы хотите установить."
!endif

!ifdef MUI_UNCOMPONENTSPAGE
  ${LangFileString} MUI_UNTEXT_COMPONENTS_TITLE "Компоненты программы"
  ${LangFileString} MUI_UNTEXT_COMPONENTS_SUBTITLE "Выберите компоненты $(^NameDA), которые вы хотите удалить."
!endif

!ifdef MUI_COMPONENTSPAGE | MUI_UNCOMPONENTSPAGE
  ${LangFileString} MUI_INNERTEXT_COMPONENTS_DESCRIPTION_TITLE "Описание"
  !ifndef NSIS_CONFIG_COMPONENTPAGE_ALTERNATIVE
    ${LangFileString} MUI_INNERTEXT_COMPONENTS_DESCRIPTION_INFO "Наведите курсор мыши на название компонента, чтобы прочесть его описание."
  !else
    ${LangFileString} MUI_INNERTEXT_COMPONENTS_DESCRIPTION_INFO "Наведите курсор мыши на название компонента, чтобы прочесть его описание."
  !endif
!endif

!ifdef MUI_DIRECTORYPAGE
  ${LangFileString} MUI_TEXT_DIRECTORY_TITLE "Выбор папки установки"
  ${LangFileString} MUI_TEXT_DIRECTORY_SUBTITLE "Выберите папку для установки $(^NameDA)."
!endif

!ifdef MUI_UNDIRECTORYPAGE
  ${LangFileString} MUI_UNTEXT_DIRECTORY_TITLE "Выбор папки для удаления"
  ${LangFileString} MUI_UNTEXT_DIRECTORY_SUBTITLE "Укажите папку, из которой нужно удалить $(^NameDA)."
!endif

!ifdef MUI_INSTFILESPAGE
  ${LangFileString} MUI_TEXT_INSTALLING_TITLE "Копирование файлов"
  ${LangFileString} MUI_TEXT_INSTALLING_SUBTITLE "Подождите, идет копирование файлов $(^NameDA)..."
  ${LangFileString} MUI_TEXT_FINISH_TITLE "Установка завершена"
  ${LangFileString} MUI_TEXT_FINISH_SUBTITLE "Установка успешно завершена."
  ${LangFileString} MUI_TEXT_ABORT_TITLE "Установка прервана"
  ${LangFileString} MUI_TEXT_ABORT_SUBTITLE "Установка не завершена."
!endif

!ifdef MUI_UNINSTFILESPAGE
  ${LangFileString} MUI_UNTEXT_UNINSTALLING_TITLE "Удаление"
  ${LangFileString} MUI_UNTEXT_UNINSTALLING_SUBTITLE "Подождите, идет удаление файлов $(^NameDA)..."
  ${LangFileString} MUI_UNTEXT_FINISH_TITLE "Удаление завершено"
  ${LangFileString} MUI_UNTEXT_FINISH_SUBTITLE "Удаление программы успешно завершено."
  ${LangFileString} MUI_UNTEXT_ABORT_TITLE "Удаление прервано"
  ${LangFileString} MUI_UNTEXT_ABORT_SUBTITLE "Удаление произведено не полностью."
!endif

!ifdef MUI_FINISHPAGE
  ${LangFileString} MUI_TEXT_FINISH_INFO_TITLE "Завершение работы мастера установки $(^NameDA)"
  ${LangFileString} MUI_TEXT_FINISH_INFO_TEXT "Установка $(^NameDA) выполнена.$\r$\n$\r$\nНажмите кнопку $\"Готово$\" для выхода из программы установки."
  ${LangFileString} MUI_TEXT_FINISH_INFO_REBOOT "Для завершения установки $(^NameDA) необходимо перезагрузить компьютер. Хотите сделать это сейчас?"
!endif

!ifdef MUI_UNFINISHPAGE
  ${LangFileString} MUI_UNTEXT_FINISH_INFO_TITLE "Завершение работы мастера удаления $(^NameDA)"
  ${LangFileString} MUI_UNTEXT_FINISH_INFO_TEXT "Программа $(^NameDA) удалена из вашего компьютера.$\r$\n$\r$\nНажмите кнопку $\"Готово$\"для выхода из программы удаления."
  ${LangFileString} MUI_UNTEXT_FINISH_INFO_REBOOT "Для завершения удаления $(^NameDA) нужно перезагрузить компьютер. Хотите сделать это сейчас?"
!endif

!ifdef MUI_FINISHPAGE | MUI_UNFINISHPAGE
  ${LangFileString} MUI_TEXT_FINISH_REBOOTNOW "Да, перезагрузить ПК сейчас"
  ${LangFileString} MUI_TEXT_FINISH_REBOOTLATER "Нет, я перезагружу ПК позже"
  ${LangFileString} MUI_TEXT_FINISH_RUN "&Запустить $(^NameDA)"
  ${LangFileString} MUI_TEXT_FINISH_SHOWREADME "&Показать файл ReadMe"
  ${LangFileString} MUI_BUTTONTEXT_FINISH "&Готово"  
!endif

!ifdef MUI_STARTMENUPAGE
  ${LangFileString} MUI_TEXT_STARTMENU_TITLE "Папка в меню $\"Пуск$\""
  ${LangFileString} MUI_TEXT_STARTMENU_SUBTITLE "Выберите папку в меню $\"Пуск$\" для размещения ярлыков программы."
  ${LangFileString} MUI_INNERTEXT_STARTMENU_TOP "Выберите папку в меню $\"Пуск$\", куда будут помещены ярлыки программы. Вы также можете ввести другое имя папки."
  ${LangFileString} MUI_INNERTEXT_STARTMENU_CHECKBOX "Не создавать ярлыки"
!endif

!ifdef MUI_UNCONFIRMPAGE
  ${LangFileString} MUI_UNTEXT_CONFIRM_TITLE "Удаление $(^NameDA)"
  ${LangFileString} MUI_UNTEXT_CONFIRM_SUBTITLE "Удаление $(^NameDA) из компьютера."
!endif

!ifdef MUI_ABORTWARNING
  ${LangFileString} MUI_TEXT_ABORTWARNING "Вы действительно хотите отменить установку $(^Name)?"
!endif

!ifdef MUI_UNABORTWARNING
  ${LangFileString} MUI_UNTEXT_ABORTWARNING "Вы действительно хотите отменить удаление $(^Name)?"
!endif
