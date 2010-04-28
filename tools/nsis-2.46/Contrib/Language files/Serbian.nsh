;Language: Serbian (3098)
;Translation by Срђан Обућина <obucina@srpskijezik.edu.yu>

!insertmacro LANGFILE "Serbian" "Serbian Cyrillic"

!ifdef MUI_WELCOMEPAGE
  ${LangFileString} MUI_TEXT_WELCOME_INFO_TITLE "Добродошли у водич за инсталацију програма $(^NameDA)"
  ${LangFileString} MUI_TEXT_WELCOME_INFO_TEXT "Бићете вођени кроз процес инсталације програма $(^NameDA).$\r$\n$\r$\nПрепоручљиво је да искључите све друге програме пре почетка инсталације. Ово може омогућити ажурирање системских фајлова без потребе за поновним покретањем рачунара.$\r$\n$\r$\n$_CLICK"
!endif

!ifdef MUI_UNWELCOMEPAGE
  ${LangFileString} MUI_UNTEXT_WELCOME_INFO_TITLE "Добродошли у деинсталацију програма $(^NameDA)"
  ${LangFileString} MUI_UNTEXT_WELCOME_INFO_TEXT "Бићете вођени кроз процес деинсталације програма $(^NameDA).$\r$\n$\r$\nПре почетка деинсталације, уверите се да је програм $(^NameDA) искључен. $_CLICK"
!endif

!ifdef MUI_LICENSEPAGE
  ${LangFileString} MUI_TEXT_LICENSE_TITLE "Договор о праву коришћења"
  ${LangFileString} MUI_TEXT_LICENSE_SUBTITLE "Пажљиво прочитајте договор о праву коришћења пре инсталације програма $(^NameDA)."
  ${LangFileString} MUI_INNERTEXT_LICENSE_BOTTOM "Ако прихватате све услове договора, притисните дугме „Прихватам“ за наставак. Морате прихватити договор да бисте инсталирали програм $(^NameDA)."
  ${LangFileString} MUI_INNERTEXT_LICENSE_BOTTOM_CHECKBOX "Ако прихватате све услове договора, обележите квадратић испод. Морате прихватити договор да бисте инсталирали програм $(^NameDA). $_CLICK"
  ${LangFileString} MUI_INNERTEXT_LICENSE_BOTTOM_RADIOBUTTONS "Ако прихватате све услове договора, изаберите прву опцију испод. Морате прихватити договор да бисте инсталирали програм $(^NameDA). $_CLICK"
!endif

!ifdef MUI_UNLICENSEPAGE
  ${LangFileString} MUI_UNTEXT_LICENSE_TITLE "Договор о праву коришћења"
  ${LangFileString} MUI_UNTEXT_LICENSE_SUBTITLE "Пажљиво прочитајте договор о праву коришћења пре деинсталације програма $(^NameDA)."
  ${LangFileString} MUI_UNINNERTEXT_LICENSE_BOTTOM "Ако прихватате све услове договора, притисните дугме „Прихватам“ за наставак. Морате прихватити договор да бисте деинсталирали програм $(^NameDA)."
  ${LangFileString} MUI_UNINNERTEXT_LICENSE_BOTTOM_CHECKBOX "Ако прихватате све услове договора, обележите квадратић испод. Морате прихватити договор да бисте деинсталирали програм $(^NameDA). $_CLICK"
  ${LangFileString} MUI_UNINNERTEXT_LICENSE_BOTTOM_RADIOBUTTONS "Ако прихватате све услове договора, изаберите прву опцију испод. Морате прихватити договор да бисте деинсталирали програм $(^NameDA). $_CLICK"
!endif

!ifdef MUI_LICENSEPAGE | MUI_UNLICENSEPAGE
  ${LangFileString} MUI_INNERTEXT_LICENSE_TOP "Притисните Page Down да бисте видели остатак договора."
!endif

!ifdef MUI_COMPONENTSPAGE
  ${LangFileString} MUI_TEXT_COMPONENTS_TITLE "Избор компоненти за инсталацију"
  ${LangFileString} MUI_TEXT_COMPONENTS_SUBTITLE "Изаберите компоненте за инсталацију. Инсталирају се само означене компоненте."
!endif

!ifdef MUI_UNCOMPONENTSPAGE
  ${LangFileString} MUI_UNTEXT_COMPONENTS_TITLE "Избор компоненти за деинсталацију"
  ${LangFileString} MUI_UNTEXT_COMPONENTS_SUBTITLE "Изаберите компоненте за деинсталацију. Деинсталирају се само означене компоненте."
!endif

!ifdef MUI_COMPONENTSPAGE | MUI_UNCOMPONENTSPAGE
  ${LangFileString} MUI_INNERTEXT_COMPONENTS_DESCRIPTION_TITLE "Опис"
  !ifndef NSIS_CONFIG_COMPONENTPAGE_ALTERNATIVE
    ${LangFileString} MUI_INNERTEXT_COMPONENTS_DESCRIPTION_INFO "Пређите курсором миша преко имена компоненте да бисте видели њен опис."
  !else
    ${LangFileString} MUI_INNERTEXT_COMPONENTS_DESCRIPTION_INFO "Пређите курсором миша преко имена компоненте да бисте видели њен опис."
  !endif
!endif

!ifdef MUI_DIRECTORYPAGE
  ${LangFileString} MUI_TEXT_DIRECTORY_TITLE "Избор фолдера за инсталацију"
  ${LangFileString} MUI_TEXT_DIRECTORY_SUBTITLE "Изаберите фолдер у који ћете инсталирати програм $(^NameDA)."
!endif

!ifdef MUI_UNDIRECTORYPAGE
  ${LangFileString} MUI_UNTEXT_DIRECTORY_TITLE "Избор фолдера за деинсталaцију"
  ${LangFileString} MUI_UNTEXT_DIRECTORY_SUBTITLE "Изаберите фолдер из кога ћете деинсталирати програм $(^NameDA)."
!endif

!ifdef MUI_INSTFILESPAGE
  ${LangFileString} MUI_TEXT_INSTALLING_TITLE "Инсталација"
  ${LangFileString} MUI_TEXT_INSTALLING_SUBTITLE "Сачекајте док се програм $(^NameDA) инсталира."
  ${LangFileString} MUI_TEXT_FINISH_TITLE "Завршена инсталација"
  ${LangFileString} MUI_TEXT_FINISH_SUBTITLE "Инсталација је успешно завршена."
  ${LangFileString} MUI_TEXT_ABORT_TITLE "Прекинута инсталација"
  ${LangFileString} MUI_TEXT_ABORT_SUBTITLE "Инсталација је прекинута и није успешно завршена."
!endif

!ifdef MUI_UNINSTFILESPAGE
  ${LangFileString} MUI_UNTEXT_UNINSTALLING_TITLE "Деинсталација"
  ${LangFileString} MUI_UNTEXT_UNINSTALLING_SUBTITLE "Сачекајте док се програм $(^NameDA) деинсталира."
  ${LangFileString} MUI_UNTEXT_FINISH_TITLE "Завршена деинсталација"
  ${LangFileString} MUI_UNTEXT_FINISH_SUBTITLE "Деинсталација је успешно завршена."
  ${LangFileString} MUI_UNTEXT_ABORT_TITLE "Прекинута деинсталација"
  ${LangFileString} MUI_UNTEXT_ABORT_SUBTITLE "Деинсталација је прекинута и није успешно завршена."
!endif

!ifdef MUI_FINISHPAGE
  ${LangFileString} MUI_TEXT_FINISH_INFO_TITLE "Завршена инсталација програма $(^NameDA)"
  ${LangFileString} MUI_TEXT_FINISH_INFO_TEXT "Програм $(^NameDA) је инсталиран на рачунар.$\r$\n$\r$\nПритисните дугме „Крај“ за затварање овог прозора."
  ${LangFileString} MUI_TEXT_FINISH_INFO_REBOOT "Рачунар мора бити поново покренут да би се процес инсталације програма $(^NameDA) успешно завршио. Желите ли то одмах да урадите?"
!endif

!ifdef MUI_UNFINISHPAGE
  ${LangFileString} MUI_UNTEXT_FINISH_INFO_TITLE "Завршена деинсталација програма $(^NameDA)"
  ${LangFileString} MUI_UNTEXT_FINISH_INFO_TEXT "Програм $(^NameDA) је деинсталиран са рачунара.$\r$\n$\r$\nПритисните дугме „Крај“ за затварање овог прозора."
  ${LangFileString} MUI_UNTEXT_FINISH_INFO_REBOOT "Рачунар мора бити поново покренут да би се процес деинсталације програма $(^NameDA) успешно завршио. Желите ли то да урадите одмах?"
!endif

!ifdef MUI_FINISHPAGE | MUI_UNFINISHPAGE
  ${LangFileString} MUI_TEXT_FINISH_REBOOTNOW "Одмах поново покрени рачунар"
  ${LangFileString} MUI_TEXT_FINISH_REBOOTLATER "Без поновног покретања"
  ${LangFileString} MUI_TEXT_FINISH_RUN "Покрени програм $(^NameDA)"
  ${LangFileString} MUI_TEXT_FINISH_SHOWREADME "Прикажи ПрочитајМе фајл"
  ${LangFileString} MUI_BUTTONTEXT_FINISH "Крај"  
!endif

!ifdef MUI_STARTMENUPAGE
  ${LangFileString} MUI_TEXT_STARTMENU_TITLE "Избор фолдера у Старт менију"
  ${LangFileString} MUI_TEXT_STARTMENU_SUBTITLE "Изаберите фолдер у Старт менију у коме ћете креирати пречице."
  ${LangFileString} MUI_INNERTEXT_STARTMENU_TOP "Изаберите фолдер у Старт менију у коме желите да буду креиране пречице програма. Можете уписати и име за креирање новог фолдера."
  ${LangFileString} MUI_INNERTEXT_STARTMENU_CHECKBOX "Без креирања пречица"
!endif

!ifdef MUI_UNCONFIRMPAGE
  ${LangFileString} MUI_UNTEXT_CONFIRM_TITLE "Деинсталација програма $(^NameDA)"
  ${LangFileString} MUI_UNTEXT_CONFIRM_SUBTITLE "Деинсталација програма $(^NameDA) са рачунара."
!endif

!ifdef MUI_ABORTWARNING
  ${LangFileString} MUI_TEXT_ABORTWARNING "Сигурно желите да прекинете инсталацију програма $(^Name)?"
!endif

!ifdef MUI_UNABORTWARNING
  ${LangFileString} MUI_UNTEXT_ABORTWARNING "Сигурно желите да прекинете деинсталацију програма $(^Name)?"
!endif
