;Language: Welsh (1106)
;By Rhoslyn Prys, Meddal.com

!insertmacro LANGFILE "Welsh" "Welsh"

!ifdef MUI_WELCOMEPAGE
  ${LangFileString} MUI_TEXT_WELCOME_INFO_TITLE "Croeso i Ddewin Gosod $(^NameDA)"
  ${LangFileString} MUI_TEXT_WELCOME_INFO_TEXT "Bydd y dewin yn eich arwain drwy osodiad $(^NameDA).$\r$\n$\r$\nCaewch pob rhaglen cyn cychwyn y rhaglen osod. Bydd hyn yn ei gwneud yn bosibl i ddiweddaru'r ffeiliau system berthnasol heb fod angen ailgychwyn eich cyfrifiadur.$\r$\n$\r$\n$_CLICK"
!endif

!ifdef MUI_UNWELCOMEPAGE
  ${LangFileString} MUI_UNTEXT_WELCOME_INFO_TITLE "Croeso i Ddewin Dadosod $(^NameDA)"
  ${LangFileString} MUI_UNTEXT_WELCOME_INFO_TEXT "Bydd y dewin yn eich arwain drwy ddadosod $(^NameDA).$\r$\n$\r$\nCyn cychwyn dadosod, gwnewch yn siwr nad yw $(^NameDA) yn rhedeg.$\r$\n$\r$\n$_CLICK"
!endif

!ifdef MUI_LICENSEPAGE
  ${LangFileString} MUI_TEXT_LICENSE_TITLE "Cytundeb Trwyddedu"
  ${LangFileString} MUI_TEXT_LICENSE_SUBTITLE "Darllenwch amodau'r drwydded cyn gosod $(^NameDA)."
  ${LangFileString} MUI_INNERTEXT_LICENSE_BOTTOM "Os ydych yn derbyn amodau'r cytundeb, cliciwch Cytuno i barhau. Mae'n rhaid i chi dderbyn amodau'r cytundeb er mwyn gosod $(^NameDA)."
  ${LangFileString} MUI_INNERTEXT_LICENSE_BOTTOM_CHECKBOX "Os ydych yn derbyn amodau'r cytundeb, cliciwch y blwch ticio isod. Mae'n rhaid i chi dderbyn amodau'r cytundeb er mwyn gosod $(^NameDA). $_CLICK"
  ${LangFileString} MUI_INNERTEXT_LICENSE_BOTTOM_RADIOBUTTONS "Os ydych yn derbyn amodau'r cytundeb, cliciwch y dewis cyntaf isod. Mae'n rhaid i chi dderbyn amodau'r cytundeb er mwyn gosod $(^NameDA). $_CLICK"
!endif

!ifdef MUI_UNLICENSEPAGE
  ${LangFileString} MUI_UNTEXT_LICENSE_TITLE "Cytundeb Trwyddedu"
  ${LangFileString} MUI_UNTEXT_LICENSE_SUBTITLE "Darllenwch amodau'r drwydded cyn dadosod $(^NameDA)."
  ${LangFileString} MUI_UNINNERTEXT_LICENSE_BOTTOM "Os ydych yn derbyn amodau'r cytundeb, cliciwch Cytuno i barhau. Mae'n rhaid i chi dderbyn amodau'r cytundeb er mwyn dadosod  $(^NameDA)."
  ${LangFileString} MUI_UNINNERTEXT_LICENSE_BOTTOM_CHECKBOX "Os ydych yn derbyn amodau'r cytundeb, cliciwch y blwch ticio isod. Mae'n rhaid i chi dderbyn amodau'r cytundeb er mwyn dadosod $(^NameDA). $_CLICK"
  ${LangFileString} MUI_UNINNERTEXT_LICENSE_BOTTOM_RADIOBUTTONS "Os ydych yn derbyn amodau'r cytundeb, cliciwch y dewis cyntaf isod. Mae'n rhaid i chi dderbyn amodau'r cytundeb er mwyn dadosod $(^NameDA). $_CLICK"
!endif

!ifdef MUI_LICENSEPAGE | MUI_UNLICENSEPAGE
  ${LangFileString} MUI_INNERTEXT_LICENSE_TOP "Pwyswch Page Down i ddarllen gweddill y cytundeb."
!endif

!ifdef MUI_COMPONENTSPAGE
  ${LangFileString} MUI_TEXT_COMPONENTS_TITLE "Dewis Cydrannau"
  ${LangFileString} MUI_TEXT_COMPONENTS_SUBTITLE "Dewis pa nodweddion o $(^NameDA) rydych am eu gosod."
!endif

!ifdef MUI_UNCOMPONENTSPAGE
  ${LangFileString} MUI_UNTEXT_COMPONENTS_TITLE "Dewis Cydrannau"
  ${LangFileString} MUI_UNTEXT_COMPONENTS_SUBTITLE "Dewis pa nodweddion o $(^NameDA) i'w dadoso."
!endif

!ifdef MUI_COMPONENTSPAGE | MUI_UNCOMPONENTSPAGE
  ${LangFileString} MUI_INNERTEXT_COMPONENTS_DESCRIPTION_TITLE "Disgrifiad"
  !ifndef NSIS_CONFIG_COMPONENTPAGE_ALTERNATIVE
    ${LangFileString} MUI_INNERTEXT_COMPONENTS_DESCRIPTION_INFO "Gosod eich llygoden dros gydran i weld ei ddisgrifiad."
  !else
    ${LangFileString} MUI_INNERTEXT_COMPONENTS_DESCRIPTION_INFO "Gosod eich llygoden dros gydran i weld ei ddisgrifiad."
  !endif
!endif

!ifdef MUI_DIRECTORYPAGE
  ${LangFileString} MUI_TEXT_DIRECTORY_TITLE "Dewis Lleoliad Gosod"
  ${LangFileString} MUI_TEXT_DIRECTORY_SUBTITLE "Dewis y ffolder i osod $(^NameDA)."
!endif

!ifdef MUI_UNDIRECTORYPAGE
  ${LangFileString} MUI_UNTEXT_DIRECTORY_TITLE "Dewis Lleoliad Dadosod"
  ${LangFileString} MUI_UNTEXT_DIRECTORY_SUBTITLE "Dewis y ffolder i ddadosod $(^NameDA)."
!endif

!ifdef MUI_INSTFILESPAGE
  ${LangFileString} MUI_TEXT_INSTALLING_TITLE "Gosod"
  ${LangFileString} MUI_TEXT_INSTALLING_SUBTITLE "Arhoswch tra fo $(^NameDA) yn cael ei osod."
  ${LangFileString} MUI_TEXT_FINISH_TITLE "Cwblhawyd y Gosod"
  ${LangFileString} MUI_TEXT_FINISH_SUBTITLE "Mae'r Gosod wedi ei gwblhau'n llwyddiannus."
  ${LangFileString} MUI_TEXT_ABORT_TITLE "Ataliwyd y Gosod"
  ${LangFileString} MUI_TEXT_ABORT_SUBTITLE "Methwyd â chwblhau'r gosod yn llwyddiannus."
!endif

!ifdef MUI_UNINSTFILESPAGE
  ${LangFileString} MUI_UNTEXT_UNINSTALLING_TITLE "Dadosod"
  ${LangFileString} MUI_UNTEXT_UNINSTALLING_SUBTITLE "Arhoswch tra bo $(^NameDA) yn cael ei ddadosod."
  ${LangFileString} MUI_UNTEXT_FINISH_TITLE "Cwblhawyd y Dadosod"
  ${LangFileString} MUI_UNTEXT_FINISH_SUBTITLE "Mae'r Dadosod wedi ei gwblhau'n llwyddiannus."
  ${LangFileString} MUI_UNTEXT_ABORT_TITLE "Ataliwyd y Dadosod"
  ${LangFileString} MUI_UNTEXT_ABORT_SUBTITLE "Methwyd â chwblhau'r dadosod yn llwyddiannus."
!endif

!ifdef MUI_FINISHPAGE
  ${LangFileString} MUI_TEXT_FINISH_INFO_TITLE "Cwblhau Dewin Gosod $(^NameDA)"
  ${LangFileString} MUI_TEXT_FINISH_INFO_TEXT "Mae $(^NameDA) wedi cael ei osod ar eich cyfrifiadur.$\r$\n$\r$\nCliciwch Gorffen i gau'r dewin."
  ${LangFileString} MUI_TEXT_FINISH_INFO_REBOOT "Rhaid ailgychwyn eich cyfrifiadur i gwblhau gosod $(^NameDA). Hoffech chi ailgychwyn?"
!endif

!ifdef MUI_UNFINISHPAGE
  ${LangFileString} MUI_UNTEXT_FINISH_INFO_TITLE "Cwblhau Dewin Dadosod $(^NameDA)"
  ${LangFileString} MUI_UNTEXT_FINISH_INFO_TEXT "Mae $(^NameDA) wedi ei ddadosod oddi ar eich cyfrifiadur.$\r$\n$\r$\nCliciwch Gorffen i gau'r dewin."
  ${LangFileString} MUI_UNTEXT_FINISH_INFO_REBOOT "Rhaid ailgychwyn eich cyfrifiadur i gwblhau gosod $(^NameDA). Hoffech chi ailgychwyn?"
!endif

!ifdef MUI_FINISHPAGE | MUI_UNFINISHPAGE
  ${LangFileString} MUI_TEXT_FINISH_REBOOTNOW "Ailgychwyn"
  ${LangFileString} MUI_TEXT_FINISH_REBOOTLATER "Rwyf am ailgychwyn yn hwyrach"
  ${LangFileString} MUI_TEXT_FINISH_RUN "&Rhedeg $(^NameDA)"
  ${LangFileString} MUI_TEXT_FINISH_SHOWREADME "&Dangos Darllenfi"
  ${LangFileString} MUI_BUTTONTEXT_FINISH "&Gorffen"  
!endif

!ifdef MUI_STARTMENUPAGE
  ${LangFileString} MUI_TEXT_STARTMENU_TITLE "Dewis Ffolder Dewislen Cychwyn"
  ${LangFileString} MUI_TEXT_STARTMENU_SUBTITLE "Dewis ffolder Dewislen Cychwyn ar gyfer llwybrau byr $(^NameDA)."
  ${LangFileString} MUI_INNERTEXT_STARTMENU_TOP "Dewis ffolder Dewislen Cychwyn i greu llwybrau byr y rhaglen. Gallwch roi enw i greu ffolder newydd."
  ${LangFileString} MUI_INNERTEXT_STARTMENU_CHECKBOX "Peidio creu llwybrau byr"
!endif

!ifdef MUI_UNCONFIRMPAGE
  ${LangFileString} MUI_UNTEXT_CONFIRM_TITLE "Dadosod $(^NameDA)"
  ${LangFileString} MUI_UNTEXT_CONFIRM_SUBTITLE "Tynnu $(^NameDA) oddiar eich cyfrifiadur."
!endif

!ifdef MUI_ABORTWARNING
  ${LangFileString} MUI_TEXT_ABORTWARNING "Ydych chi'n si?r eich bod am adael Rhaglen Osod $(^Name)?"
!endif

!ifdef MUI_UNABORTWARNING
  ${LangFileString} MUI_UNTEXT_ABORTWARNING "Ydych chi'n siwr eich bod am adael Rhaglen Dadosod $(^Name)?"
!endif
