;Language: Icelandic (15)
;By Gretar Orri Kristinsson

!insertmacro LANGFILE "Icelandic" "Icelandic"

!ifdef MUI_WELCOMEPAGE
  ${LangFileString} MUI_TEXT_WELCOME_INFO_TITLE "Velkominn til $(^NameDA) uppsetningarhjálparinnar"
  ${LangFileString} MUI_TEXT_WELCOME_INFO_TEXT "Þessi hjálp mun leiða þig í gegnum uppsetninguna á $(^NameDA).$\r$\n$\r$\nMælt er með því að þú lokir öllum öðrum forritum áður en uppsetningin hefst. Þetta mun gera uppsetningarforritinu kleyft að uppfæra kerfiskrár án þess að endurræsa tölvuna.$\r$\n$\r$\n$_CLICK"
!endif

!ifdef MUI_UNWELCOMEPAGE
  ${LangFileString} MUI_UNTEXT_WELCOME_INFO_TITLE "Velkomin(n) til $(^NameDA) fjarlægingarhjálparinnar"
  ${LangFileString} MUI_UNTEXT_WELCOME_INFO_TEXT "Þessi hjálp mun leiða þig í gegnum fjarlæginguna á $(^NameDA).$\r$\n$\r$\nÁður en fjarlæging hefst skal ganga úr skugga um að $(^NameDA) sé ekki opið.$\r$\n$\r$\n$_CLICK"
!endif

!ifdef MUI_LICENSEPAGE
  ${LangFileString} MUI_TEXT_LICENSE_TITLE "Notandaleyfissamningur"
  ${LangFileString} MUI_TEXT_LICENSE_SUBTITLE "Vinsamlegast skoðaðu Notandaleyfissamninginn vel áður en uppsetning á $(^NameDA) hefst."
  ${LangFileString} MUI_INNERTEXT_LICENSE_BOTTOM "Ef þú samþykkir skilmála samningsins, smelltu þá á 'Ég samþykki' til að halda áfram. Þú verður að samþykkja samninginn til þess að setja upp $(^NameDA)."
  ${LangFileString} MUI_INNERTEXT_LICENSE_BOTTOM_CHECKBOX "Ef þú samþykkir skilmála samningsins, hakaðu þá í kassann hér að neðan. Þú verður að samþykkja samninginn til þess að setja upp $(^NameDA). $_CLICK"
  ${LangFileString} MUI_INNERTEXT_LICENSE_BOTTOM_RADIOBUTTONS "Ef þú samþykkir skilmála samningsins, veldu þá fyrsta valmöguleikann hér að neðan. Þú verður að samþykkja samninginn til þess að setja upp $(^NameDA). $_CLICK"
!endif

!ifdef MUI_UNLICENSEPAGE
  ${LangFileString} MUI_UNTEXT_LICENSE_TITLE "Leyfissamningur"
  ${LangFileString} MUI_UNTEXT_LICENSE_SUBTITLE "Vinsamlegast skoðaðu leyfissamninginn vel áður en fjarlæging á $(^NameDA) hefst."
  ${LangFileString} MUI_UNINNERTEXT_LICENSE_BOTTOM "Ef þú samþykkir skilmála samningsins, smelltu þá á 'Ég samþykki' til að halda áfram. Þú verður að samþykkja samninginn til þess að fjarlægja $(^NameDA)."
  ${LangFileString} MUI_UNINNERTEXT_LICENSE_BOTTOM_CHECKBOX "Ef þú samþykkir skilmála samningsins, hakaðu þá í kassann hér að neðan. Þú verður að samþykkja samninginn til þess að fjarlægja $(^NameDA). $_CLICK"
  ${LangFileString} MUI_UNINNERTEXT_LICENSE_BOTTOM_RADIOBUTTONS "Ef þú samþykkir skilmála samningsins, veldu þá fyrsta valmöguleikann hér að neðan. Þú verður að samþykkja samninginn til þess að fjarlægja $(^NameDA). $_CLICK"
!endif

!ifdef MUI_LICENSEPAGE | MUI_UNLICENSEPAGE
  ${LangFileString} MUI_INNERTEXT_LICENSE_TOP "Smelltu á 'PageDown' takkann á lyklaborðinu til að sjá afganginn af samningnum."
!endif

!ifdef MUI_COMPONENTSPAGE
  ${LangFileString} MUI_TEXT_COMPONENTS_TITLE "Velja íhluti"
  ${LangFileString} MUI_TEXT_COMPONENTS_SUBTITLE "Veldu hvaða $(^NameDA) íhluti þú vilt setja upp."
!endif

!ifdef MUI_UNCOMPONENTSPAGE
  ${LangFileString} MUI_UNTEXT_COMPONENTS_TITLE "Velja íhluti"
  ${LangFileString} MUI_UNTEXT_COMPONENTS_SUBTITLE "Velja hvaða $(^NameDA) íhluti þú vilt fjarlægja."
!endif

!ifdef MUI_COMPONENTSPAGE | MUI_UNCOMPONENTSPAGE
  ${LangFileString} MUI_INNERTEXT_COMPONENTS_DESCRIPTION_TITLE "Lýsing"
  !ifndef NSIS_CONFIG_COMPONENTPAGE_ALTERNATIVE
    ${LangFileString} MUI_INNERTEXT_COMPONENTS_DESCRIPTION_INFO "Færðu músina yfir íhlut til að fá lýsinguna á honum."
  !else
    ${LangFileString} MUI_INNERTEXT_COMPONENTS_DESCRIPTION_INFO "Færðu músina yfir íhlut til að fá lýsinguna á honum."
  !endif
!endif

!ifdef MUI_DIRECTORYPAGE
  ${LangFileString} MUI_TEXT_DIRECTORY_TITLE "Veldu uppsetningarskáarsafn"
  ${LangFileString} MUI_TEXT_DIRECTORY_SUBTITLE "Veldu það skráarsafn sem þú vilt setja $(^NameDA) upp í."
!endif

!ifdef MUI_UNDIRECTORYPAGE
  ${LangFileString} MUI_UNTEXT_DIRECTORY_TITLE "Velja fjarlægingarskáarsafn"
  ${LangFileString} MUI_UNTEXT_DIRECTORY_SUBTITLE "Veldu það skráarsafn sem þú vilt fjarlægja $(^NameDA) úr."
!endif

!ifdef MUI_INSTFILESPAGE
  ${LangFileString} MUI_TEXT_INSTALLING_TITLE "Set upp"
  ${LangFileString} MUI_TEXT_INSTALLING_SUBTITLE "Vinsamlegast dokaðu við meðan $(^NameDA) er sett upp."
  ${LangFileString} MUI_TEXT_FINISH_TITLE "Uppsetningu lokið"
  ${LangFileString} MUI_TEXT_FINISH_SUBTITLE "Uppsetning tókst."
  ${LangFileString} MUI_TEXT_ABORT_TITLE "Hætt við uppsetningu"
  ${LangFileString} MUI_TEXT_ABORT_SUBTITLE "Uppsetningu lauk ekki sem skildi."
!endif

!ifdef MUI_UNINSTFILESPAGE
  ${LangFileString} MUI_UNTEXT_UNINSTALLING_TITLE "Fjarlægi"
  ${LangFileString} MUI_UNTEXT_UNINSTALLING_SUBTITLE "Vinsamlegast dokaðu við á meðan $(^NameDA) er fjarlægt."
  ${LangFileString} MUI_UNTEXT_FINISH_TITLE "Fjarlægingu lokið"
  ${LangFileString} MUI_UNTEXT_FINISH_SUBTITLE "Fjarlæging tókst."
  ${LangFileString} MUI_UNTEXT_ABORT_TITLE "Hætt við fjarlægingu"
  ${LangFileString} MUI_UNTEXT_ABORT_SUBTITLE "Fjarlægingu lauk ekki sem skildi."
!endif

!ifdef MUI_FINISHPAGE
  ${LangFileString} MUI_TEXT_FINISH_INFO_TITLE "Ljúka $(^NameDA) uppsetningarhjálpinni"
  ${LangFileString} MUI_TEXT_FINISH_INFO_TEXT "$(^NameDA) er nú upp sett á tölvunni þinni.$\r$\n$\r$\nSmelltu á 'Ljúka' til að loka þessari hjálp."
  ${LangFileString} MUI_TEXT_FINISH_INFO_REBOOT "Til að ljúka uppsetningunni á $(^NameDA) verður að endurræsa tölvuna. Viltu endurræsa núna?"
!endif

!ifdef MUI_UNFINISHPAGE
  ${LangFileString} MUI_UNTEXT_FINISH_INFO_TITLE "Ljúka $(^NameDA) fjarlægingarhjálpinni"
  ${LangFileString} MUI_UNTEXT_FINISH_INFO_TEXT "$(^NameDA) hefur nú verið fjarlægt úr tölvunni.$\r$\n$\r$\nSmelltu á 'Ljúka' til að loka þessari hjálp."
  ${LangFileString} MUI_UNTEXT_FINISH_INFO_REBOOT "Til að ljúka fjarlægingunni á $(^NameDA) verður að endurræsa tölvuna. Viltu endurræsa núna?"
!endif

!ifdef MUI_FINISHPAGE | MUI_UNFINISHPAGE
  ${LangFileString} MUI_TEXT_FINISH_REBOOTNOW "Endurræsa núna"
  ${LangFileString} MUI_TEXT_FINISH_REBOOTLATER "Ég vil endurræsa seinna"
  ${LangFileString} MUI_TEXT_FINISH_RUN "&Keyra $(^NameDA)"
  ${LangFileString} MUI_TEXT_FINISH_SHOWREADME "&Skoða LestuMig"
  ${LangFileString} MUI_BUTTONTEXT_FINISH "&Ljúka"  
!endif

!ifdef MUI_STARTMENUPAGE
  ${LangFileString} MUI_TEXT_STARTMENU_TITLE "Velja skráarsafn 'Start' valmyndar"
  ${LangFileString} MUI_TEXT_STARTMENU_SUBTITLE "Veldu skráarsafn $(^NameDA) flýtileiða fyrir 'Start' valmyndina."
  ${LangFileString} MUI_INNERTEXT_STARTMENU_TOP "Veldu skráarsafn flýtileiða forritsins fyrir 'Start' valmyndina. Þú getur einnig búið til nýtt skráarsafn með því að setja inn nýtt nafn."
  ${LangFileString} MUI_INNERTEXT_STARTMENU_CHECKBOX "Ekki búa til flýtileiðir í 'Start' valmyndinni"
!endif

!ifdef MUI_UNCONFIRMPAGE
  ${LangFileString} MUI_UNTEXT_CONFIRM_TITLE "Fjarlægja $(^NameDA)"
  ${LangFileString} MUI_UNTEXT_CONFIRM_SUBTITLE "Fjarlægja $(^NameDA) úr tölvunni."
!endif

!ifdef MUI_ABORTWARNING
  ${LangFileString} MUI_TEXT_ABORTWARNING "Ertu viss um að þú viljir loka $(^Name) uppsetningarhjálpinni?"
!endif

!ifdef MUI_UNABORTWARNING
  ${LangFileString} MUI_UNTEXT_ABORTWARNING "Ertu viss um að þú viljir loka $(^Name) fjarlægingarhjálpinni?"
!endif
