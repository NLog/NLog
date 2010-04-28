;Language: Irish (2108)
;By Kevin P. Scannell < scannell at slu dot edu >

!insertmacro LANGFILE "Irish" "Irish"

!ifdef MUI_WELCOMEPAGE
  ${LangFileString} MUI_TEXT_WELCOME_INFO_TITLE "Fáilte go dtí Draoi Suiteála $(^NameDA)"
  ${LangFileString} MUI_TEXT_WELCOME_INFO_TEXT "Déanfaidh an draoi seo treorú duit tríd an suiteáil de $(^NameDA).$\r$\n$\r$\nMoltar duit gach feidhmchlár eile a dhúnadh sula dtosaíonn tú an Suiteálaí. Cinnteoidh sé seo gur féidir na comhaid oiriúnacha a nuashonrú gan do ríomhaire a atosú.$\r$\n$\r$\n$_CLICK"
!endif

!ifdef MUI_UNWELCOMEPAGE
  ${LangFileString} MUI_UNTEXT_WELCOME_INFO_TITLE "Fáilte go dtí Draoi Díshuiteála $(^NameDA)"
  ${LangFileString} MUI_UNTEXT_WELCOME_INFO_TEXT "Déanfaidh an draoi seo treorú duit tríd an díshuiteáil de $(^NameDA).$\r$\n$\r$\nBí cinnte nach bhfuil $(^NameDA) ag rith sula dtosaíonn tú an díshuiteáil.$\r$\n$\r$\n$_CLICK"
!endif

!ifdef MUI_LICENSEPAGE
  ${LangFileString} MUI_TEXT_LICENSE_TITLE "Comhaontú um Cheadúnas"
  ${LangFileString} MUI_TEXT_LICENSE_SUBTITLE "Déan iniúchadh ar choinníollacha an cheadúnais sula suiteálann tú $(^NameDA)."
  ${LangFileString} MUI_INNERTEXT_LICENSE_BOTTOM "Má ghlacann tú le coinníollacha an chomhaontaithe, cliceáil $\"Glacaim Leis$\" chun leanúint ar aghaidh. Caithfidh tú glacadh leis an gcomhaontú chun $(^NameDA) a shuiteáil."
  ${LangFileString} MUI_INNERTEXT_LICENSE_BOTTOM_CHECKBOX "Má ghlacann tú le coinníollacha an chomhaontaithe, cliceáil an ticbhosca thíos. Caithfidh tú glacadh leis an gcomhaontú chun $(^NameDA) a shuiteáil. $_CLICK"
  ${LangFileString} MUI_INNERTEXT_LICENSE_BOTTOM_RADIOBUTTONS "Má ghlacann tú le coinníollacha an chomhaontaithe, roghnaigh an chéad rogha thíos. Caithfidh tú glacadh leis an gcomhaontú chun $(^NameDA) a dhíshuiteáil. $_CLICK"
!endif

!ifdef MUI_UNLICENSEPAGE
  ${LangFileString} MUI_UNTEXT_LICENSE_TITLE "Comhaontú um Cheadúnas"
  ${LangFileString} MUI_UNTEXT_LICENSE_SUBTITLE "Déan iniúchadh ar choinníollacha an cheadúnais sula ndíshuiteálann tú $(^NameDA)."
  ${LangFileString} MUI_UNINNERTEXT_LICENSE_BOTTOM "Má ghlacann tú le coinníollacha an chomhaontaithe, cliceáil $\"Glacaim Leis$\" chun leanúint ar aghaidh. Caithfidh tú glacadh leis an gcomhaontú chun $(^NameDA) a dhíshuiteáil."
  ${LangFileString} MUI_UNINNERTEXT_LICENSE_BOTTOM_CHECKBOX "Má ghlacann tú le coinníollacha an chomhaontaithe, cliceáil an ticbhosca thíos. Caithfidh tú glacadh leis an gcomhaontú chun $(^NameDA) a dhíshuiteáil. $_CLICK"
  ${LangFileString} MUI_UNINNERTEXT_LICENSE_BOTTOM_RADIOBUTTONS "Má ghlacann tú le coinníollacha an chomhaontaithe, roghnaigh an chéad rogha thíos. Caithfidh tú glacadh leis an gcomhaontú chun $(^NameDA) a dhíshuiteáil. $_CLICK"
!endif

!ifdef MUI_LICENSEPAGE | MUI_UNLICENSEPAGE
  ${LangFileString} MUI_INNERTEXT_LICENSE_TOP "Brúigh $\"Page Down$\" chun an chuid eile den cheadúnas a léamh."
!endif

!ifdef MUI_COMPONENTSPAGE
  ${LangFileString} MUI_TEXT_COMPONENTS_TITLE "Roghnaigh Comhpháirteanna"
  ${LangFileString} MUI_TEXT_COMPONENTS_SUBTITLE "Roghnaigh na gnéithe $(^NameDA) ba mhaith leat suiteáil."
!endif

!ifdef MUI_UNCOMPONENTSPAGE
  ${LangFileString} MUI_UNTEXT_COMPONENTS_TITLE "Roghnaigh Comhpháirteanna"
  ${LangFileString} MUI_UNTEXT_COMPONENTS_SUBTITLE "Roghnaigh na gnéithe $(^NameDA) ba mhaith leat díshuiteáil."
!endif

!ifdef MUI_COMPONENTSPAGE | MUI_UNCOMPONENTSPAGE
  ${LangFileString} MUI_INNERTEXT_COMPONENTS_DESCRIPTION_TITLE "Cur Síos"
  !ifndef NSIS_CONFIG_COMPONENTPAGE_ALTERNATIVE
    ${LangFileString} MUI_INNERTEXT_COMPONENTS_DESCRIPTION_INFO "Cuir do luch os cionn comhpháirte chun cur síos a fheiceáil."
  !else
    ${LangFileString} MUI_INNERTEXT_COMPONENTS_DESCRIPTION_INFO "Cuir do luch os cionn comhpháirte chun cur síos a fheiceáil."
  !endif
!endif

!ifdef MUI_DIRECTORYPAGE
  ${LangFileString} MUI_TEXT_DIRECTORY_TITLE "Roghnaigh Suíomh na Suiteála"
  ${LangFileString} MUI_TEXT_DIRECTORY_SUBTITLE "Roghnaigh an fillteán inar mian leat $(^NameDA) a shuiteáil."
!endif

!ifdef MUI_UNDIRECTORYPAGE
  ${LangFileString} MUI_UNTEXT_DIRECTORY_TITLE "Roghnaigh Suíomh na Díshuiteála"
  ${LangFileString} MUI_UNTEXT_DIRECTORY_SUBTITLE "Roghnaigh an fillteán ar mian leat $(^NameDA) a dhíshuiteáil as."
!endif

!ifdef MUI_INSTFILESPAGE
  ${LangFileString} MUI_TEXT_INSTALLING_TITLE "Á Shuiteáil"
  ${LangFileString} MUI_TEXT_INSTALLING_SUBTITLE "Fan go fóill; $(^NameDA) á shuiteáil."
  ${LangFileString} MUI_TEXT_FINISH_TITLE "Suiteáil Críochnaithe"
  ${LangFileString} MUI_TEXT_FINISH_SUBTITLE "D'éirigh leis an tsuiteáil."
  ${LangFileString} MUI_TEXT_ABORT_TITLE "Suiteáil Tobscortha"
  ${LangFileString} MUI_TEXT_ABORT_SUBTITLE "Níor éirigh leis an tsuiteáil."
!endif

!ifdef MUI_UNINSTFILESPAGE
  ${LangFileString} MUI_UNTEXT_UNINSTALLING_TITLE "Á Dhíshuiteáil"
  ${LangFileString} MUI_UNTEXT_UNINSTALLING_SUBTITLE "Fan go fóill; $(^NameDA) á dhíshuiteáil."
  ${LangFileString} MUI_UNTEXT_FINISH_TITLE "Díshuiteáil Críochnaithe"
  ${LangFileString} MUI_UNTEXT_FINISH_SUBTITLE "D'éirigh leis an díshuiteáil."
  ${LangFileString} MUI_UNTEXT_ABORT_TITLE "Díshuiteáil Tobscortha"
  ${LangFileString} MUI_UNTEXT_ABORT_SUBTITLE "Níor éirigh leis an díshuiteáil."
!endif

!ifdef MUI_FINISHPAGE
  ${LangFileString} MUI_TEXT_FINISH_INFO_TITLE "Draoi Suiteála $(^NameDA) á Chríochnú"
  ${LangFileString} MUI_TEXT_FINISH_INFO_TEXT "Suiteáladh $(^NameDA) ar do ríomhaire.$\r$\n$\r$\nCliceáil $\"Críochnaigh$\" chun an draoi seo a dhúnadh."
  ${LangFileString} MUI_TEXT_FINISH_INFO_REBOOT "Ní mór duit do ríomhaire a atosú chun suiteáil $(^NameDA) a chur i gcrích. Ar mhaith leat atosú anois?"
!endif

!ifdef MUI_UNFINISHPAGE
  ${LangFileString} MUI_UNTEXT_FINISH_INFO_TITLE "Draoi Díshuiteála $(^NameDA) á Chríochnú"
  ${LangFileString} MUI_UNTEXT_FINISH_INFO_TEXT "Díshuiteáladh $(^NameDA) ó do ríomhaire.$\r$\n$\r$\nCliceáil $\"Críochnaigh$\" chun an draoi seo a dhúnadh."
  ${LangFileString} MUI_UNTEXT_FINISH_INFO_REBOOT "Ní mór duit do ríomhaire a atosú chun díshuiteáil $(^NameDA) a chur i gcrích. Ar mhaith leat atosú anois?"
!endif

!ifdef MUI_FINISHPAGE | MUI_UNFINISHPAGE
  ${LangFileString} MUI_TEXT_FINISH_REBOOTNOW "Atosaigh anois"
  ${LangFileString} MUI_TEXT_FINISH_REBOOTLATER "Atosóidh mé de láimh níos déanaí"
  ${LangFileString} MUI_TEXT_FINISH_RUN "&Rith $(^NameDA)"
  ${LangFileString} MUI_TEXT_FINISH_SHOWREADME "Tai&speáin comhad README"
  ${LangFileString} MUI_BUTTONTEXT_FINISH "&Críochnaigh"  
!endif

!ifdef MUI_STARTMENUPAGE
  ${LangFileString} MUI_TEXT_STARTMENU_TITLE "Roghnaigh Fillteán sa Roghchlár Tosaigh"
  ${LangFileString} MUI_TEXT_STARTMENU_SUBTITLE "Roghnaigh fillteán sa Roghchlár Tosaigh a gcuirfear aicearraí $(^NameDA) ann."
  ${LangFileString} MUI_INNERTEXT_STARTMENU_TOP "Roghnaigh an fillteán sa Roghchlár Tosaigh inar mian leat aicearraí an chláir a chruthú. Is féidir freisin fillteán nua a chruthú trí ainm nua a iontráil."
  ${LangFileString} MUI_INNERTEXT_STARTMENU_CHECKBOX "Ná cruthaigh aicearraí"
!endif

!ifdef MUI_UNCONFIRMPAGE
  ${LangFileString} MUI_UNTEXT_CONFIRM_TITLE "Díshuiteáil $(^NameDA)"
  ${LangFileString} MUI_UNTEXT_CONFIRM_SUBTITLE "Bain $(^NameDA) ó do ríomhaire."
!endif

!ifdef MUI_ABORTWARNING
  ${LangFileString} MUI_TEXT_ABORTWARNING "An bhfuil tú cinnte gur mian leat Suiteálaí $(^Name) a scor?"
!endif

!ifdef MUI_UNABORTWARNING
  ${LangFileString} MUI_UNTEXT_ABORTWARNING "An bhfuil tú cinnte gur mian leat Díshuiteálaí $(^Name) a scor?"
!endif
