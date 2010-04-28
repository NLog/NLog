;Language: Kurdish
;By Rêzan Tovjîn
;Updated by Erdal Ronahî (erdal.ronahi@gmail.com)

!insertmacro LANGFILE "Kurdish" "Kurdî"

!ifdef MUI_WELCOMEPAGE
  ${LangFileString} MUI_TEXT_WELCOME_INFO_TITLE "$(^NameDA) Tu bi xêr hatî sêrbaziya sazkirinê"
  ${LangFileString} MUI_TEXT_WELCOME_INFO_TEXT "Ev sêrbaz dê di dema sazkirina $(^NameDA) de rêberiya te bike.$\r$\n$\r$\nBerî tu dest bi sazkirinê bikî, em pêþniyar dikin tu hemû bernameyên vekirî bigirî. Bi vî rengî beyî tu komputera ji nû ve vekî dê hinek dosiyên pergalê bêpirsgirêk werin sazkirin.$\r$\n$\r$\n$_CLICK"
!endif

!ifdef MUI_UNWELCOMEPAGE
  ${LangFileString} MUI_UNTEXT_WELCOME_INFO_TITLE "Tu bi xêr hatî sêrbaziya rakirina bernameya $(^NameDA)"
  ${LangFileString} MUI_UNTEXT_WELCOME_INFO_TEXT "Ev sêrbaz ji bo rakirina bernameya $(^NameDA) dê alîkariya te bike.$\r$\n$\r$\nBerî tu dest bi rakirina bernameyê bikî, bernameyên vekirî hemûyan bigire. Bi vî rengî dû re tu mecbûr namînî ku komputera xwe bigirî û ji nû ve veki.$\r$\n$\r$\n$_CLICK"
!endif

!ifdef MUI_LICENSEPAGE
  ${LangFileString} MUI_TEXT_LICENSE_TITLE "Peymana Lîsansê"
  ${LangFileString} MUI_TEXT_LICENSE_SUBTITLE "Ji kerema xwe re berî tu bernameya $(^NameDA) saz bikî, peymana lîsansê bixwîne."
  ${LangFileString} MUI_INNERTEXT_LICENSE_BOTTOM "Heke tu þertên peymanê dipejirînî, 'Ez Dipejirînim'ê bitikîne. Ji bo sazkirina bernameya $(^NameDA) divê tu þertên peymanê bipejirînî."
  ${LangFileString} MUI_INNERTEXT_LICENSE_BOTTOM_CHECKBOX "Heke tu þertên peymanê dipejirînî, zeviya piþtrastkirinê ya jêrîn dagire. Ji bo tu bikarî bernameya $(^NameDA) saz bikî divê tu þertên peymanê bipejirînî. $_CLICK"
  ${LangFileString} MUI_INNERTEXT_LICENSE_BOTTOM_RADIOBUTTONS "Heke tu þertên peymanê dipejirînî, biþkojka erêkirinê ya jêrîn bitikîne. Ji bo sazkirina bernameya $(^NameDA) divê tu þertên peymanê bipejirînî. $_CLICK"
!endif

!ifdef MUI_UNLICENSEPAGE
  ${LangFileString} MUI_UNTEXT_LICENSE_TITLE "Peymana Lîsansê"
  ${LangFileString} MUI_UNTEXT_LICENSE_SUBTITLE "Berî tu bernameya $(^NameDA) ji pergala xwe rakî peymanê bixwîne."
  ${LangFileString} MUI_UNINNERTEXT_LICENSE_BOTTOM "Heke tu þertên peymanê dipejirînî, 'Dipejirînim'ê bitikîne. Ji bo rakirina bernameya  $(^NameDA) divê tu þertên peymanê bipejirînî."
  ${LangFileString} MUI_UNINNERTEXT_LICENSE_BOTTOM_CHECKBOX "Heke tu þertên peymanê dipejirînî, zeviya erêkirinê ya jêrîn dagire. Ji bo tu bernameya $(^NameDA) ji pergala xwe rakî divê tu peymanê bipejirînî. $_CLICK"
  ${LangFileString} MUI_UNINNERTEXT_LICENSE_BOTTOM_RADIOBUTTONS "Heke tu þertên peymanê dipejirînî, biþkojka erêkirinê ya jêrîn hilbijêre. Ji bo tu bernameya  $(^NameDA) ji pergala xwe rakî divê tu þertên peymanê bipejirînî. $_CLICK"
!endif

!ifdef MUI_LICENSEPAGE | MUI_UNLICENSEPAGE
  ${LangFileString} MUI_INNERTEXT_LICENSE_TOP "Ji bo dûmahîka peymanê biþkojka 'page down' bitikîne."
!endif

!ifdef MUI_COMPONENTSPAGE
  ${LangFileString} MUI_TEXT_COMPONENTS_TITLE "Hilbijartina pareyan"
  ${LangFileString} MUI_TEXT_COMPONENTS_SUBTITLE "Ji bo sazkirina $(^NameDA) pareyên tu dixwazî hilbijêre."
!endif

!ifdef MUI_UNCOMPONENTSPAGE
  ${LangFileString} MUI_UNTEXT_COMPONENTS_TITLE "Hilbijartina Pareyan"
  ${LangFileString} MUI_UNTEXT_COMPONENTS_SUBTITLE "Pareya bernameya $(^NameDA) ku tu dixwazî rakî hilbijêre."
!endif

!ifdef MUI_COMPONENTSPAGE | MUI_UNCOMPONENTSPAGE
  ${LangFileString} MUI_INNERTEXT_COMPONENTS_DESCRIPTION_TITLE "Dazanîn"
  !ifndef NSIS_CONFIG_COMPONENTPAGE_ALTERNATIVE
    ${LangFileString} MUI_INNERTEXT_COMPONENTS_DESCRIPTION_INFO "Ji bo tu der barê pareyan de agahiyan bistînî nîþanekê bibe ser pareyekê."
  !else
    ${LangFileString} MUI_INNERTEXT_COMPONENTS_DESCRIPTION_INFO "Ji bo tu der barê pareyan de agahiyan bistînî nîþanekê bibe ser pareyekê."
  !endif
!endif

!ifdef MUI_DIRECTORYPAGE
  ${LangFileString} MUI_TEXT_DIRECTORY_TITLE "Hilbijartina peldanka armanckirî"
  ${LangFileString} MUI_TEXT_DIRECTORY_SUBTITLE "Peldanka tu dixwazî bernameya $(^NameDA) tê de were sazkirin hilbijêre."
!endif

!ifdef MUI_UNDIRECTORYPAGE
  ${LangFileString} MUI_UNTEXT_DIRECTORY_TITLE "Hilbijartina Peldanka Dê Were Rakirin"
  ${LangFileString} MUI_UNTEXT_DIRECTORY_SUBTITLE "Peldanka bernameya $(^NameDA) ku tudixwazî rakî hilbijêre."
!endif

!ifdef MUI_INSTFILESPAGE
  ${LangFileString} MUI_TEXT_INSTALLING_TITLE "Tê sazkirin"
  ${LangFileString} MUI_TEXT_INSTALLING_SUBTITLE "Ji kerema xwe re heta sazkirina $(^NameDA) biqede raweste."
  ${LangFileString} MUI_TEXT_FINISH_TITLE "Sazkirin Qediya"
  ${LangFileString} MUI_TEXT_FINISH_SUBTITLE "Sazkirin bi serkeftinî qediya."
  ${LangFileString} MUI_TEXT_ABORT_TITLE "Sazkirin hate betalkirin"
  ${LangFileString} MUI_TEXT_ABORT_SUBTITLE "Sazkirin be tevahî qediya."
!endif

!ifdef MUI_UNINSTFILESPAGE
  ${LangFileString} MUI_UNTEXT_UNINSTALLING_TITLE "Tê rakirin"
  ${LangFileString} MUI_UNTEXT_UNINSTALLING_SUBTITLE "Heta bernameya $(^NameDA) ji pergala te were rakirin raweste."
  ${LangFileString} MUI_UNTEXT_FINISH_TITLE "Rakirina Bernameyê Biqedîne"
  ${LangFileString} MUI_UNTEXT_FINISH_SUBTITLE "Rakirina bernameyê bi serkeftin pêk hat."
  ${LangFileString} MUI_UNTEXT_ABORT_TITLE "Rakirina bernameyê hate betalkirin"
  ${LangFileString} MUI_UNTEXT_ABORT_SUBTITLE "Rakirina bernameyê neqediya."
!endif

!ifdef MUI_FINISHPAGE
  ${LangFileString} MUI_TEXT_FINISH_INFO_TITLE "Sêrbaziya sazkirina $(^NameDA) diqede."
  ${LangFileString} MUI_TEXT_FINISH_INFO_TEXT "$(^NameDA) li komputera te hate barkirin.$\r$\n$\r$\n'Biqedîne'yê bitikîne û sazkirinê bi dawî bîne."
  ${LangFileString} MUI_TEXT_FINISH_INFO_REBOOT "Ji bo bidawîkirina sazkirina $(^NameDA) divê tu komputerê ji nû ve vekî.Tu dixwazî komputerê ji nû ve vekî?"
!endif

!ifdef MUI_UNFINISHPAGE
  ${LangFileString} MUI_UNTEXT_FINISH_INFO_TITLE "Sêrbaziya Rakirina Bernameya $(^NameDA) Tê Temamkirin"
  ${LangFileString} MUI_UNTEXT_FINISH_INFO_TEXT "Bernameya $(^NameDA) ji pergale hate rakirin.$\r$\n$\r$\nJi bo girtina sêrbaz 'biqedîne'yê bitikîne."
  ${LangFileString} MUI_UNTEXT_FINISH_INFO_REBOOT "Ji bo rakirina bernameya $(^NameDA) biqede divê tu komputera xwe ji nû ve vekî. Tu dixwazî niha komputera te were girtin û ji nû ve dest pê bike?"
!endif

!ifdef MUI_FINISHPAGE | MUI_UNFINISHPAGE
  ${LangFileString} MUI_TEXT_FINISH_REBOOTNOW "Ji nû ve veke"
  ${LangFileString} MUI_TEXT_FINISH_REBOOTLATER "Ezê paþê ji nû ve vekim."
  ${LangFileString} MUI_TEXT_FINISH_RUN "Bernameya $(^NameDA) bixebitîne"
  ${LangFileString} MUI_TEXT_FINISH_SHOWREADME "Dosiya min bixwîne/readme &nîþan bide"
  ${LangFileString} MUI_BUTTONTEXT_FINISH "&Biqedîne"  
!endif

!ifdef MUI_STARTMENUPAGE
  ${LangFileString} MUI_TEXT_STARTMENU_TITLE "Hilbijartina Peldanka Pêþeka Destpêkê"
  ${LangFileString} MUI_TEXT_STARTMENU_SUBTITLE "Peldanka pêþeka destpêkê ya ku dê kineriya $(^NameDA) tê de were bikaranîn hilbijêre."
  ${LangFileString} MUI_INNERTEXT_STARTMENU_TOP "Peldanka pêþeka destpêkê ya ku dê kineriya bernameyê tê de were bicihkirin hilbijêre.  Tu dikarî bi navekî nû peldankeke nû ava bikî."
  ${LangFileString} MUI_INNERTEXT_STARTMENU_CHECKBOX "Bêyî çêkirina kineriyê bidomîne"
!endif

!ifdef MUI_UNCONFIRMPAGE
  ${LangFileString} MUI_UNTEXT_CONFIRM_TITLE "Bernameya $(^NameDA) Rake"
  ${LangFileString} MUI_UNTEXT_CONFIRM_SUBTITLE "Rakirina bernameya $(^NameDA) ji pergala te."
!endif

!ifdef MUI_ABORTWARNING
  ${LangFileString} MUI_TEXT_ABORTWARNING "Tu bawer î ku dixwazî ji sazkirina $(^Name) derkevî?"
!endif

!ifdef MUI_UNABORTWARNING
  ${LangFileString} MUI_UNTEXT_ABORTWARNING "Tu bawer î ku dixwazî dest ji rakirina bernameya $(^Name) berdî?"
!endif
