;Language: Hungarian (1038)
;Translation by Jozsef Tamas Herczeg ( - 1.61-ig),
;               Lajos Molnar (Orfanik) <orfanik@axelero.hu> ( 1.62 - tõl)

!insertmacro LANGFILE "Hungarian" "Magyar"

!ifdef MUI_WELCOMEPAGE
  ${LangFileString} MUI_TEXT_WELCOME_INFO_TITLE "Üdvözli a(z) $(^NameDA) Telepítõ Varázsló"
  ${LangFileString} MUI_TEXT_WELCOME_INFO_TEXT "A(z) $(^NameDA) telepítése következik a számítógépre.$\r$\n$\r$\nJavasoljuk, hogy indítás elõtt zárja be a futó alkalmazásokat. Így a telepítõ a rendszer újraindítása nélkül tudja frissíteni a szükséges rendszerfájlokat.$\r$\n$\r$\n$_CLICK"
!endif

!ifdef MUI_UNWELCOMEPAGE
  ${LangFileString} MUI_UNTEXT_WELCOME_INFO_TITLE "Üdvözli a(z) $(^NameDA) Eltávolító Varázsló"
  ${LangFileString} MUI_UNTEXT_WELCOME_INFO_TEXT "Ez a varázsló segíti a(z) $(^NameDA) eltávolításában.$\r$\n$\r$\nMielõtt elkezdi az eltávilítást gyõzõdjön meg arról, hogy a(z) $(^NameDA) nem fut.$\r$\n$\r$\n$_CLICK"
!endif

!ifdef MUI_LICENSEPAGE
  ${LangFileString} MUI_TEXT_LICENSE_TITLE "Licencszerzõdés"
  ${LangFileString} MUI_TEXT_LICENSE_SUBTITLE "A(z) $(^NameDA) telepítése elõtt tekintse át a szerzõdés feltételeit."
  ${LangFileString} MUI_INNERTEXT_LICENSE_BOTTOM "Ha elfogadja a szerzõdés valamennyi feltételét, az Elfogadom gombbal folytathatja. El kell fogadnia a(z) $(^NameDA) telepítéséhez."
  ${LangFileString} MUI_INNERTEXT_LICENSE_BOTTOM_CHECKBOX "Amennyiben elfogadja a feltételeket, jelölje be a jelölõnényzeten. A(z) $(^NameDA) telepítéséhez el kell fogadnia a feltételeket. $_CLICK"
  ${LangFileString} MUI_INNERTEXT_LICENSE_BOTTOM_RADIOBUTTONS "Amennyiben elfogadja a feltételeket, válassza az elsõ opciót. A(z) $(^NameDA) telepítéséhez el kell fogadnia a feltételeket. $_CLICK"
!endif

!ifdef MUI_UNLICENSEPAGE
  ${LangFileString} MUI_UNTEXT_LICENSE_TITLE "Licencszerzõdés"
  ${LangFileString} MUI_UNTEXT_LICENSE_SUBTITLE "A(z) $(^NameDA) eltávolítása elõtt tekintse át a szerzõdés feltételeit."
  ${LangFileString} MUI_UNINNERTEXT_LICENSE_BOTTOM "Ha elfogadja a szerzõdés valamennyi feltételét, az Elfogadom gombbal folytathatja. El kell fogadnia a(z) $(^NameDA) eltávolításához."
  ${LangFileString} MUI_UNINNERTEXT_LICENSE_BOTTOM_CHECKBOX "Amennyiben elfogadja a feltételeket, jelölje be a jelölõnényzeten. A(z) $(^NameDA) eltávolításához el kell fogadnia a feltételeket. $_CLICK"
  ${LangFileString} MUI_UNINNERTEXT_LICENSE_BOTTOM_RADIOBUTTONS "Amennyiben elfogadja a feltételeket, válassza az elsõ opciót. A(z) $(^NameDA) eltávolításához el kell fogadnia a feltételeket. $_CLICK"
!endif

!ifdef MUI_LICENSEPAGE | MUI_UNLICENSEPAGE
  ${LangFileString} MUI_INNERTEXT_LICENSE_TOP "A PageDown gombbal olvashatja el a szerzõdés folytatását."
!endif

!ifdef MUI_COMPONENTSPAGE
  ${LangFileString} MUI_TEXT_COMPONENTS_TITLE "Összetevõk kiválasztása"
  ${LangFileString} MUI_TEXT_COMPONENTS_SUBTITLE "Válassza ki, hogy a(z) $(^NameDA) mely funkcióit kívánja telepíteni."
!endif

!ifdef MUI_UNCOMPONENTSPAGE
  ${LangFileString} MUI_UNTEXT_COMPONENTS_TITLE "Összetevõk kiválasztása"
  ${LangFileString} MUI_UNTEXT_COMPONENTS_SUBTITLE "Válassza ki, hogy a(z) $(^NameDA) mely funkcióit kívánja eltávolítani."
!endif

!ifdef MUI_COMPONENTSPAGE | MUI_UNCOMPONENTSPAGE
  ${LangFileString} MUI_INNERTEXT_COMPONENTS_DESCRIPTION_TITLE "Leírás"
  !ifndef NSIS_CONFIG_COMPONENTPAGE_ALTERNATIVE
    ${LangFileString} MUI_INNERTEXT_COMPONENTS_DESCRIPTION_INFO "Vigye rá az egeret az összetevõre, hogy megtekinthesse a leírását."
  !else
    ${LangFileString} MUI_INNERTEXT_COMPONENTS_DESCRIPTION_INFO "Vigye rá az egeret az összetevõre, hogy megtekinthesse a leírását."
  !endif
!endif

!ifdef MUI_DIRECTORYPAGE
  ${LangFileString} MUI_TEXT_DIRECTORY_TITLE "Telepítési hely kiválasztása"
  ${LangFileString} MUI_TEXT_DIRECTORY_SUBTITLE "Válassza ki a(z) $(^NameDA) telepítésének mappáját."
!endif

!ifdef MUI_UNDIRECTORYPAGE
  ${LangFileString} MUI_UNTEXT_DIRECTORY_TITLE "Telepítési hely kiválasztása"
  ${LangFileString} MUI_UNTEXT_DIRECTORY_SUBTITLE "Válassza ki a(z) $(^NameDA) telepítésének mappáját."
!endif

!ifdef MUI_INSTFILESPAGE
  ${LangFileString} MUI_TEXT_INSTALLING_TITLE "Telepítési folyamat"
  ${LangFileString} MUI_TEXT_INSTALLING_SUBTITLE "Kis türelmet a(z) $(^NameDA) telepítéséig."
  ${LangFileString} MUI_TEXT_FINISH_TITLE "Telepítés befejezõdött"
  ${LangFileString} MUI_TEXT_FINISH_SUBTITLE "A telepítés sikeresen befejezõdött."
  ${LangFileString} MUI_TEXT_ABORT_TITLE "A telepítés megszakadt"
  ${LangFileString} MUI_TEXT_ABORT_SUBTITLE "A telepítés sikertelen volt."
!endif

!ifdef MUI_UNINSTFILESPAGE
  ${LangFileString} MUI_UNTEXT_UNINSTALLING_TITLE "Eltávolítási folyamat"
  ${LangFileString} MUI_UNTEXT_UNINSTALLING_SUBTITLE "Kis türelmet a(z) $(^NameDA) eltávolításáig."
  ${LangFileString} MUI_UNTEXT_FINISH_TITLE "Az eltávolítás befejezõdött"
  ${LangFileString} MUI_UNTEXT_FINISH_SUBTITLE "Az eltávolítás sikeresen befejezõdött."
  ${LangFileString} MUI_UNTEXT_ABORT_TITLE "Az eltávolítás megszakadt"
  ${LangFileString} MUI_UNTEXT_ABORT_SUBTITLE "Az eltávolítás sikertelen volt."
!endif

!ifdef MUI_FINISHPAGE
  ${LangFileString} MUI_TEXT_FINISH_INFO_TITLE "A(z) $(^NameDA) telepítése megtörtént."
  ${LangFileString} MUI_TEXT_FINISH_INFO_TEXT "A(z) $(^NameDA) telepítése megtörtént.$\r$\n$\r$\nA Befejezés gomb megnyomásával zárja be a varázslót."
  ${LangFileString} MUI_TEXT_FINISH_INFO_REBOOT "A(z) $(^NameDA) telepítésének befejezéséhez újra kell indítani a rendszert. Most akarja újraindítani?"
!endif

!ifdef MUI_UNFINISHPAGE
  ${LangFileString} MUI_UNTEXT_FINISH_INFO_TITLE "A(z) $(^NameDA) eltávolítás varázslójának befejezése."
  ${LangFileString} MUI_UNTEXT_FINISH_INFO_TEXT "A(z) $(^NameDA) eltávolítása sikeresen befejezõdött.$\r$\n$\r$\nA Finish-re kattintva bezárul ez a varázsló."
  ${LangFileString} MUI_UNTEXT_FINISH_INFO_REBOOT "A számítógépet újra kell indítani, hogy a(z) $(^NameDA) eltávolítása teljes legyen. Akarja most újraindítani a rendszert?"
!endif

!ifdef MUI_FINISHPAGE | MUI_UNFINISHPAGE
  ${LangFileString} MUI_TEXT_FINISH_REBOOTNOW "Most indítom újra"
  ${LangFileString} MUI_TEXT_FINISH_REBOOTLATER "Késõbb fogom újraindítani"
  ${LangFileString} MUI_TEXT_FINISH_RUN "$(^NameDA) futtatása"
  ${LangFileString} MUI_TEXT_FINISH_SHOWREADME "OlvassEl fájl megjelenítése"
  ${LangFileString} MUI_BUTTONTEXT_FINISH "&Befejezés"  
!endif

!ifdef MUI_STARTMENUPAGE
  ${LangFileString} MUI_TEXT_STARTMENU_TITLE "Start menü mappa kijelölése"
  ${LangFileString} MUI_TEXT_STARTMENU_SUBTITLE "Start menü mappa kijelölése a program parancsikonjaihoz."
  ${LangFileString} MUI_INNERTEXT_STARTMENU_TOP "Jelöljön ki egy mappát a Start menüben, melybe a program parancsikonjait fogja elhelyezni. Beírhatja új mappa nevét is."
  ${LangFileString} MUI_INNERTEXT_STARTMENU_CHECKBOX "Nincs parancsikon elhelyezés"
!endif

!ifdef MUI_UNCONFIRMPAGE
  ${LangFileString} MUI_UNTEXT_CONFIRM_TITLE "A(z) $(^NameDA) Eltávolítása."
  ${LangFileString} MUI_UNTEXT_CONFIRM_SUBTITLE "A(z) $(^NameDA) eltávolítása következik a számítógéprõl."
!endif

!ifdef MUI_ABORTWARNING
  ${LangFileString} MUI_TEXT_ABORTWARNING "Biztos, hogy ki akar lépni a(z) $(^Name) Telepítõbõl?"
!endif

!ifdef MUI_UNABORTWARNING
  ${LangFileString} MUI_UNTEXT_ABORTWARNING "Biztos, hogy ki akar lépni a(z) $(^Name) Eltávolítóból?"
!endif
