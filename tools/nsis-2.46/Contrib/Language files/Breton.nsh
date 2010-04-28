;Language: Breton (1150)
;By KAD-Korvigelloù An Drouizig

!insertmacro LANGFILE "Breton" "Brezhoneg"

!ifdef MUI_WELCOMEPAGE
  ${LangFileString} MUI_TEXT_WELCOME_INFO_TITLE "Degemer mat e skoazeller staliañ $(^NameDA)"
  ${LangFileString} MUI_TEXT_WELCOME_INFO_TEXT "Sturiet e viot gant ar skoazeller-mañ evit staliañ $(^NameDA).$\r$\n$\r$\nGwelloc'h eo serriñ pep arload oberiant er reizhiad a-raok mont pelloc'h gant ar skoazeller-mañ. Evel-se e c'heller nevesaat ar restroù reizhiad hep rankout adloc'hañ hoc'h urzhiataer.$\r$\n$\r$\n$_CLICK"
!endif

!ifdef MUI_UNWELCOMEPAGE
  ${LangFileString} MUI_UNTEXT_WELCOME_INFO_TITLE "Degemer mat er skoazeller distaliañ $(^NameDA)"
  ${LangFileString} MUI_UNTEXT_WELCOME_INFO_TEXT "Sturiet e viot gant ar skoazeller-mañ a-benn distaliañ $(^NameDA).$\r$\n$\r$\nEn em asurit n'eo ket lañset $(^NameDA) a-raok mont pelloc'h gant an distaliañ.$\r$\n$\r$\n$_CLICK"
!endif

!ifdef MUI_LICENSEPAGE
  ${LangFileString} MUI_TEXT_LICENSE_TITLE "Lañvaz emglev"
  ${LangFileString} MUI_TEXT_LICENSE_SUBTITLE "Bezit aketus en ur lenn pep term eus al lañvaz a-raok staliañ $(^NameDA), mar plij."
  ${LangFileString} MUI_INNERTEXT_LICENSE_BOTTOM "Mar degemerit pep term eus al lañvaz, klikit war « War-lerc'h ». Ret eo deoc'h degemer al lañvaz evit staliañ $(^NameDA)."
  ${LangFileString} MUI_INNERTEXT_LICENSE_BOTTOM_CHECKBOX "Mar degemerit pep term eus al lañvaz, klikit war al log a-zindan. Ret eo deoc'h degemer al lañvaz a-benn staliañ $(^NameDA). $_CLICK"
  ${LangFileString} MUI_INNERTEXT_LICENSE_BOTTOM_RADIOBUTTONS "Mar degemerit pep term eus al lañvaz, diuzit an dibab kentañ a-zindan. Ret eo deoc'h degemer al lañvaz a-benn staliañ $(^NameDA). $_CLICK"
!endif

!ifdef MUI_UNLICENSEPAGE
  ${LangFileString} MUI_UNTEXT_LICENSE_TITLE "Lañvaz emglev"
  ${LangFileString} MUI_UNTEXT_LICENSE_SUBTITLE "Trugarez da lenn al lañvaz a-raok distaliañ $(^NameDA)."
  ${LangFileString} MUI_UNINNERTEXT_LICENSE_BOTTOM "Mar degemerit pep term eus al lañvaz, klikit war « A-du emaon » evit kenderc'hel. Ret eo deoc'h degemer al lañvaz evit distaliañ $(^NameDA)."
  ${LangFileString} MUI_UNINNERTEXT_LICENSE_BOTTOM_CHECKBOX "Mar degemerit pep term eus al lañvaz, klikit war al log a-zindan. Ret eo deoc'h degemer al lañvaz evit distaliañ $(^NameDA). $_CLICK"
  ${LangFileString} MUI_UNINNERTEXT_LICENSE_BOTTOM_RADIOBUTTONS "Mar degemerit pep term eus al lañvaz, dizuit an dibab kentañ a-zindan. Ret eo deoc'h degemer al lañvaz evit distaliañ $(^NameDA). $_CLICK"
!endif

!ifdef MUI_LICENSEPAGE | MUI_UNLICENSEPAGE
  ${LangFileString} MUI_INNERTEXT_LICENSE_TOP "Pouezit war « Pajenn a-raok » evit lenn ar pajennoù eus al lañvaz da-heul."
!endif

!ifdef MUI_COMPONENTSPAGE
  ${LangFileString} MUI_TEXT_COMPONENTS_TITLE "Dibab elfennoù"
  ${LangFileString} MUI_TEXT_COMPONENTS_SUBTITLE "Dibabit pe elfenn(où) $(^NameDA) a fell deoc'h staliañ."
!endif

!ifdef MUI_UNCOMPONENTSPAGE
  ${LangFileString} MUI_UNTEXT_COMPONENTS_TITLE "Dibabit elfennoù"
  ${LangFileString} MUI_UNTEXT_COMPONENTS_SUBTITLE "Dibabit pe elfenn(où) $(^NameDA) a fell deoc'h distaliañ."
!endif

!ifdef MUI_COMPONENTSPAGE | MUI_UNCOMPONENTSPAGE
  ${LangFileString} MUI_INNERTEXT_COMPONENTS_DESCRIPTION_TITLE "Deskrivadenn"
  !ifndef NSIS_CONFIG_COMPONENTPAGE_ALTERNATIVE
    ${LangFileString} MUI_INNERTEXT_COMPONENTS_DESCRIPTION_INFO "Lakait ho logodenn a-zioc'h an elfenn evit gwelout he deskrivadenn."
  !else
    ${LangFileString} MUI_INNERTEXT_COMPONENTS_DESCRIPTION_INFO "Lakait ho logodenn a-zioc'h an elfenn evit gwelout he deskrivadenn."
  !endif
!endif

!ifdef MUI_DIRECTORYPAGE
  ${LangFileString} MUI_TEXT_DIRECTORY_TITLE "Dibabit al lec'hiadur staliañ"
  ${LangFileString} MUI_TEXT_DIRECTORY_SUBTITLE "Dibabit ar c'havlec'h ma vo lakaet $(^NameDA) ennañ."
!endif

!ifdef MUI_UNDIRECTORYPAGE
  ${LangFileString} MUI_UNTEXT_DIRECTORY_TITLE "Dibabit al lec'hiadur distaliañ"
  ${LangFileString} MUI_UNTEXT_DIRECTORY_SUBTITLE "Dibabit ar c'havlec'h e vo dilamet $(^NameDA) dioutañ."
!endif

!ifdef MUI_INSTFILESPAGE
  ${LangFileString} MUI_TEXT_INSTALLING_TITLE "O staliañ"
  ${LangFileString} MUI_TEXT_INSTALLING_SUBTITLE "Gortozit mar plij, emañ $(^NameDA) o vezañ staliet."
  ${LangFileString} MUI_TEXT_FINISH_TITLE "Echu eo gant ar staliañ"
  ${LangFileString} MUI_TEXT_FINISH_SUBTITLE "Kaset eo bet da benn mat ar staliañ."
  ${LangFileString} MUI_TEXT_ABORT_TITLE "Staliañ paouezet"
  ${LangFileString} MUI_TEXT_ABORT_SUBTITLE "N'eo ket bet kaset da benn mat ar staliañ."
!endif

!ifdef MUI_UNINSTFILESPAGE
  ${LangFileString} MUI_UNTEXT_UNINSTALLING_TITLE "O tistaliañ"
  ${LangFileString} MUI_UNTEXT_UNINSTALLING_SUBTITLE "Gortozit mar plij, emañ $(^NameDA) o vezañ distaliet."
  ${LangFileString} MUI_UNTEXT_FINISH_TITLE "Echu eo gant an distaliañ"
  ${LangFileString} MUI_UNTEXT_FINISH_SUBTITLE "Gant berzh eo bet kaset da benn an distaliañ."
  ${LangFileString} MUI_UNTEXT_ABORT_TITLE "Distaliañ paouezet"
  ${LangFileString} MUI_UNTEXT_ABORT_SUBTITLE "N'eo ket bet kaset da benn mat an distaliañ."
!endif

!ifdef MUI_FINISHPAGE
  ${LangFileString} MUI_TEXT_FINISH_INFO_TITLE "Oc'h echuiñ staliañ $(^NameDA) gant ar skoazeller"
  ${LangFileString} MUI_TEXT_FINISH_INFO_TEXT "Staliet eo bet $(^NameDA) war hoc'h urzhiataer.$\r$\n$\r$\nKlikit war « Echuiñ » evit serriñ ar skoazeller-mañ."
  ${LangFileString} MUI_TEXT_FINISH_INFO_REBOOT "Ret eo hoc'h urzhiataer bezañ adloc'het evit ma vez kaset da benn staliañ $(^NameDA). Ha fellout a ra deoc'h adloc'hañ diouzhtu ?"
!endif

!ifdef MUI_UNFINISHPAGE
  ${LangFileString} MUI_UNTEXT_FINISH_INFO_TITLE "Oc'h echuiñ distaliañ $(^NameDA) gant ar skoazeller"
  ${LangFileString} MUI_UNTEXT_FINISH_INFO_TEXT "Distaliet eo bet $(^NameDA) diouzh hoc'h urzhiataer.$\r$\n$\r$\nKlikit war « Echuiñ » evit serriñ ar skoazeller-mañ."
  ${LangFileString} MUI_UNTEXT_FINISH_INFO_REBOOT "Ret eo hoc'h urzhiataer bezañ adloc'het evit ma vez kaset da benn distaliañ $(^NameDA). Ha fellout a ra deoc'h adloc'hañ diouzhtu ?"
!endif

!ifdef MUI_FINISHPAGE | MUI_UNFINISHPAGE
  ${LangFileString} MUI_TEXT_FINISH_REBOOTNOW "Adloc'hañ diouzhtu"
  ${LangFileString} MUI_TEXT_FINISH_REBOOTLATER "Fellout a ra din adloc'hañ diwezatoc'h dre zorn"
  ${LangFileString} MUI_TEXT_FINISH_RUN "&Lañsañ $(^NameDA)"
  ${LangFileString} MUI_TEXT_FINISH_SHOWREADME "&Diskouez ar restr Malennit"
  ${LangFileString} MUI_BUTTONTEXT_FINISH "&Echuiñ"  
!endif

!ifdef MUI_STARTMENUPAGE
  ${LangFileString} MUI_TEXT_STARTMENU_TITLE "Diskouez kavlec'h al Lañser loc'hañ"
  ${LangFileString} MUI_TEXT_STARTMENU_SUBTITLE "Dibabit ur c'havlec'h Lañser loc'hañ evit berradennoù $(^NameDA)."
  ${LangFileString} MUI_INNERTEXT_STARTMENU_TOP "Diuzit ar c'havlec'h Lañser loc'hañ e vo savet ennañ berradennoù ar goulevioù. Gallout a rit ingal reiñ un anv evit sevel ur c'havlec'h nevez."
  ${LangFileString} MUI_INNERTEXT_STARTMENU_CHECKBOX "Chom hep sevel berradennoù"
!endif

!ifdef MUI_UNCONFIRMPAGE
  ${LangFileString} MUI_UNTEXT_CONFIRM_TITLE "Distaliañ $(^NameDA)"
  ${LangFileString} MUI_UNTEXT_CONFIRM_SUBTITLE "Dilemel $(^NameDA) adalek hoc'h urzhiataer."
!endif

!ifdef MUI_ABORTWARNING
  ${LangFileString} MUI_TEXT_ABORTWARNING "Ha sur oc'h e fell deoc'h kuitaat staliañ $(^Name) ?"
!endif

!ifdef MUI_UNABORTWARNING
  ${LangFileString} MUI_UNTEXT_ABORTWARNING "Ha sur oc'h e fell deoc'h kuitaat distaliañ $(^Name) ?"
!endif
