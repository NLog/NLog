;Language: Latvieðu [Latvian] - (1062)
;By Valdis Griíis
;Corrections by Kristaps Meòìelis / x-f (x-f 'AT' inbox.lv)

!insertmacro LANGFILE "Latvian" "Latvieðu"

!ifdef MUI_WELCOMEPAGE
  ${LangFileString} MUI_TEXT_WELCOME_INFO_TITLE "Esiet sveicinâti '$(^NameDA)' uzstâdîðanas vednî"
  ${LangFileString} MUI_TEXT_WELCOME_INFO_TEXT "Ðis uzstâdîðanas vednis jums palîdzçs veikt '$(^NameDA)' uzstâdîðanu.$\r$\n$\r$\nÏoti ieteicams aizvçrt citas programmas pirms ðîs programmas uzstâdîðanas veikðanas. Tas ïaus atjaunot svarîgus sistçmas failus bez datora pârstartçðanas.$\r$\n$\r$\n$_CLICK"
!endif

!ifdef MUI_UNWELCOMEPAGE
  ${LangFileString} MUI_UNTEXT_WELCOME_INFO_TITLE "Esiet sveicinâti '$(^NameDA)' atinstalçðanas vednî"
  ${LangFileString} MUI_UNTEXT_WELCOME_INFO_TEXT "Ðis vednis jums palîdzçs veikt '$(^NameDA)' atinstalçðanu.$\r$\n$\r$\nPirms sâkt atinstalçðanas procesu, pârliecinieties, vai '$(^NameDA)' paðlaik nedarbojas.$\r$\n$\r$\n$_CLICK"
!endif

!ifdef MUI_LICENSEPAGE
  ${LangFileString} MUI_TEXT_LICENSE_TITLE "Licences lîgums"
  ${LangFileString} MUI_TEXT_LICENSE_SUBTITLE "Lûdzu izlasiet licences lîgumu pirms '$(^NameDA)' uzstâdîðanas."
  ${LangFileString} MUI_INNERTEXT_LICENSE_BOTTOM "Ja piekrîtat licences lîguma noteikumiem, spiediet 'Piekrîtu', lai turpinâtu uzstâdîðanu. Jums ir jâpiekrît licences noteikumiem, lai uzstâdîtu '$(^NameDA)'."
  ${LangFileString} MUI_INNERTEXT_LICENSE_BOTTOM_CHECKBOX "Ja piekrîtat licences lîguma noteikumiem, tad atzîmçjiet izvçles rûtiòu. Jums ir jâpiekrît licences noteikumiem, lai uzstâdîtu '$(^NameDA)'. $_CLICK"
  ${LangFileString} MUI_INNERTEXT_LICENSE_BOTTOM_RADIOBUTTONS "Ja piekrîtat licences lîguma noteikumiem, tad izvçlieties pirmo zemâkesoðo opciju. Jums ir jâpiekrît licences noteikumiem, lai uzstâdîtu '$(^NameDA)'. $_CLICK"
!endif

!ifdef MUI_UNLICENSEPAGE
  ${LangFileString} MUI_UNTEXT_LICENSE_TITLE "Licences lîgums"
  ${LangFileString} MUI_UNTEXT_LICENSE_SUBTITLE "Lûdzu izlasiet licences lîgumu pirms '$(^NameDA)' atinstalçðanas."
  ${LangFileString} MUI_UNINNERTEXT_LICENSE_BOTTOM "Ja piekrîtat licences noteikumiem, spiediet 'Piekrîtu', lai turpinâtu. Jums ir jâpiekrît licences noteikumiem, lai atinstalçtu '$(^NameDA)'."
  ${LangFileString} MUI_UNINNERTEXT_LICENSE_BOTTOM_CHECKBOX "Ja piekrîtat licences lîguma noteikumiem, tad iezîmçjiet izvçles rûtiòu. Jums ir jâpiekrît licences noteikumiem, lai atinstalçtu '$(^NameDA)'. $_CLICK"
  ${LangFileString} MUI_UNINNERTEXT_LICENSE_BOTTOM_RADIOBUTTONS "Ja piekrîtat licences lîguma noteikumiem, tad izvçlieties pirmo zemâkesoðo opciju. Jums ir jâpiekrît licences noteikumiem, lai atinstalçtu '$(^NameDA)'. $_CLICK"
!endif

!ifdef MUI_LICENSEPAGE | MUI_UNLICENSEPAGE
  ${LangFileString} MUI_INNERTEXT_LICENSE_TOP "Spiediet 'Page Down', lai aplûkotu visu lîgumu."
!endif

!ifdef MUI_COMPONENTSPAGE
  ${LangFileString} MUI_TEXT_COMPONENTS_TITLE "Izvçlieties komponentus"
  ${LangFileString} MUI_TEXT_COMPONENTS_SUBTITLE "Izvçlieties nepiecieðamâs '$(^NameDA)' sastâvdaïas, kuras uzstâdît."
!endif

!ifdef MUI_UNCOMPONENTSPAGE
  ${LangFileString} MUI_UNTEXT_COMPONENTS_TITLE "Izvçlieties komponentus"
  ${LangFileString} MUI_UNTEXT_COMPONENTS_SUBTITLE "Izvçlieties nepiecieðamâs '$(^NameDA)' sastâvdaïas, kuras atinstalçt."
!endif

!ifdef MUI_COMPONENTSPAGE | MUI_UNCOMPONENTSPAGE
  ${LangFileString} MUI_INNERTEXT_COMPONENTS_DESCRIPTION_TITLE "Apraksts"
  !ifndef NSIS_CONFIG_COMPONENTPAGE_ALTERNATIVE
    ${LangFileString} MUI_INNERTEXT_COMPONENTS_DESCRIPTION_INFO "Novietojiet peles kursoru uz komponenta, lai tiktu parâdîts tâ apraksts."
  !else
    ${LangFileString} MUI_INNERTEXT_COMPONENTS_DESCRIPTION_INFO "Novietojiet peles kursoru uz komponenta, lai tiktu parâdîts tâ apraksts."
  !endif
!endif

!ifdef MUI_DIRECTORYPAGE
  ${LangFileString} MUI_TEXT_DIRECTORY_TITLE "Izvçlieties uzstâdîðanas mapi"
  ${LangFileString} MUI_TEXT_DIRECTORY_SUBTITLE "Izvçlieties mapi, kurâ uzstâdît '$(^NameDA)'."
!endif

!ifdef MUI_UNDIRECTORYPAGE
  ${LangFileString} MUI_UNTEXT_DIRECTORY_TITLE "Izvçlieties atinstalçðanas mapi"
  ${LangFileString} MUI_UNTEXT_DIRECTORY_SUBTITLE "Izvçlieties mapi, no kuras notiks '$(^NameDA)' atinstalçðana."
!endif

!ifdef MUI_INSTFILESPAGE
  ${LangFileString} MUI_TEXT_INSTALLING_TITLE "Notiek uzstâdîðana"
  ${LangFileString} MUI_TEXT_INSTALLING_SUBTITLE "Lûdzu uzgaidiet, kamçr notiek '$(^NameDA)' uzstâdîðana."
  ${LangFileString} MUI_TEXT_FINISH_TITLE "Uzstâdîðana pabeigta"
  ${LangFileString} MUI_TEXT_FINISH_SUBTITLE "Uzstâdîðana noritçja veiksmîgi."
  ${LangFileString} MUI_TEXT_ABORT_TITLE "Uzstâdîðana atcelta"
  ${LangFileString} MUI_TEXT_ABORT_SUBTITLE "Uzstâdîðana nenoritçja veiksmîgi."
!endif

!ifdef MUI_UNINSTFILESPAGE
  ${LangFileString} MUI_UNTEXT_UNINSTALLING_TITLE "Notiek atinstalçðana"
  ${LangFileString} MUI_UNTEXT_UNINSTALLING_SUBTITLE "Lûdzu uzgaidiet, kamçr '$(^NameDA)' tiek atinstalçta."
  ${LangFileString} MUI_UNTEXT_FINISH_TITLE "Atinstalçðana pabeigta"
  ${LangFileString} MUI_UNTEXT_FINISH_SUBTITLE "Atinstalçðana noritçja veiksmîgi."
  ${LangFileString} MUI_UNTEXT_ABORT_TITLE "Atinstalçðana atcelta"
  ${LangFileString} MUI_UNTEXT_ABORT_SUBTITLE "Atinstalçðana nenoritçja veiksmîgi."
!endif

!ifdef MUI_FINISHPAGE
  ${LangFileString} MUI_TEXT_FINISH_INFO_TITLE "Tiek pabeigta '$(^NameDA)' uzstâdîðana"
  ${LangFileString} MUI_TEXT_FINISH_INFO_TEXT "'$(^NameDA)' tika veiksmîgi uzstâdîta jûsu datorâ.$\r$\n$\r$\nNospiediet 'Pabeigt', lai aizvçrtu vedni."
  ${LangFileString} MUI_TEXT_FINISH_INFO_REBOOT "Jûsu datoru ir nepiecieðams pârstartçt, lai pabeigtu '$(^NameDA)' uzstâdîðanu. Vai vçlaties pârstartçt datoru tûlît?"
!endif

!ifdef MUI_UNFINISHPAGE
  ${LangFileString} MUI_UNTEXT_FINISH_INFO_TITLE "Tiek pabeigta '$(^NameDA)' atinstalâcija"
  ${LangFileString} MUI_UNTEXT_FINISH_INFO_TEXT "'$(^NameDA)' tika veiksmîgi izdzçsta no jûsu datora.$\r$\n$\r$\nNospiediet 'Pabeigt', lai aizvçrtu vedni."
  ${LangFileString} MUI_UNTEXT_FINISH_INFO_REBOOT "Jûsu datoru nepiecieðams pârstartçt, lai pabeigtu '$(^NameDA)' atinstalçðanu. Vai vçlaties pârstartçt datoru tûlît?"
!endif

!ifdef MUI_FINISHPAGE | MUI_UNFINISHPAGE
  ${LangFileString} MUI_TEXT_FINISH_REBOOTNOW "Pârstartçt tûlît"
  ${LangFileString} MUI_TEXT_FINISH_REBOOTLATER "Es vçlos pârstartçt pats vçlâk"
  ${LangFileString} MUI_TEXT_FINISH_RUN "P&alaist '$(^NameDA)'"
  ${LangFileString} MUI_TEXT_FINISH_SHOWREADME "Pa&râdît LasiMani failu"
  ${LangFileString} MUI_BUTTONTEXT_FINISH "&Pabeigt"  
!endif

!ifdef MUI_STARTMENUPAGE
  ${LangFileString} MUI_TEXT_STARTMENU_TITLE "Izvçlieties 'Start Menu' folderi"
  ${LangFileString} MUI_TEXT_STARTMENU_SUBTITLE "Izvçlieties 'Start Menu' mapi '$(^NameDA)' saîsnçm."
  ${LangFileString} MUI_INNERTEXT_STARTMENU_TOP "Izvçlieties 'Start Menu' mapi, kurâ tiks izveidotas programmas saîsnes. Varat arî pats izveidot jaunu mapi."
  ${LangFileString} MUI_INNERTEXT_STARTMENU_CHECKBOX "Neveidot saîsnes"
!endif

!ifdef MUI_UNCONFIRMPAGE
  ${LangFileString} MUI_UNTEXT_CONFIRM_TITLE "'$(^NameDA)' atinstalçðana"
  ${LangFileString} MUI_UNTEXT_CONFIRM_SUBTITLE "Dzçst '$(^NameDA)' no jûsu datora."
!endif

!ifdef MUI_ABORTWARNING
  ${LangFileString} MUI_TEXT_ABORTWARNING "Vai tieðâm vçlaties pârtraukt '$(^Name)' uzstâdîðanu?"
!endif

!ifdef MUI_UNABORTWARNING
  ${LangFileString} MUI_UNTEXT_ABORTWARNING "Vai tieðâm vçlaties pârtraukt '$(^Name)' atinstalçðanu?"
!endif
